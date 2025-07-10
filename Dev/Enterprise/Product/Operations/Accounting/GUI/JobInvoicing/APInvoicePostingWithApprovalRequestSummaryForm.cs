using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APInvoicePostingWithApprovalRequestSummaryForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public APInvoicePostingWithApprovalRequestSummaryForm()
		{
			InitializeComponent();
		}

		public APInvoicePostingWithApprovalRequestSummaryForm(APInvoiceChargesCollection bo, ZString currentPostingJobNumber)
			: base(bo)
		{
			InitializeComponent();
			grid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("86fd2435-b715-4a1a-8eb9-84206b8ec1f9", "&Preview"), HandlePrint));

			jobNumber = currentPostingJobNumber;
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			//don't save anything
		}

		protected override void SaveInternal()
		{
			//don't save anything
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel && DialogResult == DialogResult.OK)
			{
				e.Cancel = ValidateAndSave() == ContinueWithSave.No;
			}
		}

		public override string FormVerb
		{
			get { return jobNumber; }
		}

		readonly ZString jobNumber;

		void HandlePrint(object sender, EventArgs e)
		{
			if (!Env.Security.APInvoiceApproval_Print.IsAllowed)
			{
				Env.Security.APInvoiceApproval_Print.ShowError();
			}
			else
			{
				var errorMessages = new ZStringBuilder();
				if (CurrentSelectedInvoiceCharge != null)
				{
					if (CurrentSelectedInvoiceCharge.ApprovingRequest != null)
					{
						CurrentSelectedInvoiceCharge.ApprovingRequest.RegisterInvoiceAndRelatedBizoNotToBeSaved();
						var result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(CurrentSelectedInvoiceCharge.ApprovingRequest);
						if (!string.IsNullOrEmpty(result.ErrorMessage))
						{
							errorMessages.AppendLine(Res.GetString("eaf4f2a1-98b9-4019-8e75-a6df92c13409", "The selected request(s) cannot be printed."));
							errorMessages.Append(Res.GetString("2fcfc54d-c3f8-4cb1-9421-80bab1fd0d5e", "Request ({0}) - {1}", CurrentSelectedInvoiceCharge.ApprovingRequest.FormatedRequestId, result.ErrorMessage));
						}
						else
						{
							InvoicePrintHelper.PrintCostConfirmationDocument(result.InvoiceToPreview.Factory, false, result.InvoiceToPreview);
						}
					}
					else
					{
						errorMessages.Append(Res.GetString("1f4c0ee1-531f-488c-ba09-610df787056a", "No approval request found."));
					}
				}
				else
				{
					errorMessages.Append(Res.GetString("ac145e6d-e5f7-4558-acf5-14cc2725c124", "Please select a record in the grid."));
				}
				if (errorMessages.Length > 0)
				{
					Globals.Message.ShowInformation(errorMessages.ToString());
				}
			}
		}

		APInvoiceCharges CurrentSelectedInvoiceCharge
		{
			get
			{
				return grid != null && grid.ListManager != null ? grid.ListManager.GetCurrent() as APInvoiceCharges : null;
			}
		}
	}
}
