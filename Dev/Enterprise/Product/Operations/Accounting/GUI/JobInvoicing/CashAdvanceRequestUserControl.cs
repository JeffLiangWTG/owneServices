using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class CashAdvanceRequestUserControl : ZUserControl
	{
		public CashAdvanceRequestUserControl()
		{
			InitializeComponent();
			headerGrid.ReadOnly = lineGrid.ReadOnly = true;
			MarkAsPaidButton.Visible = ShowPaidUnpaidButtons;
			MarkAsUnpaidButton.Visible = ShowPaidUnpaidButtons;
			headerGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("{2A1DD810-CB41-4286-837B-81F5DAB6782E}", "&Print"), PrintRequests));
		}

		bool IsAlreadyBound;
		AccCashAdvanceRequestHeaderCollection CashAdvanceRequestHeaders;

		public void Bind(AccCashAdvanceRequestHeaderCollection cashAdvanceRequestHeaders)
		{
			if (!IsAlreadyBound)
			{
				CashAdvanceRequestHeaders = cashAdvanceRequestHeaders;
				base.SetDataBinding(CashAdvanceRequestHeaders, "");
				IsAlreadyBound = true;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Do Nothing. Handling manually.
		}

		internal void RefreshCashAdvances(AccCashAdvanceRequestHeaderCollection cashAdvanceRequestHeaders)
		{
			CashAdvanceRequestHeaders?.AddRange(cashAdvanceRequestHeaders);
		}

		#region Security

		internal SecurityCheckpoint PluginSecurity
		{
			get { return pluginSecurity; }
			set
			{
				pluginSecurity = value;
				ResetSecurityHelper();
			}
		}
		SecurityCheckpoint pluginSecurity;

		JobInvoicingSecurityHelper SecurityHelper
		{
			get { return securityHelper ?? (securityHelper = new JobInvoicingSecurityHelper(PluginSecurity, false)); }
		}
		JobInvoicingSecurityHelper securityHelper;

		void ResetSecurityHelper()
		{
			securityHelper = null;
		}

		#endregion

		void PrintRequests(object sender, EventArgs e)
		{
			var requestCollection = new List<AccCashAdvanceRequestHeader>();
			var errorMessages = new ZStringBuilder(Res.GetString("5389f71f-e0f9-459e-90d1-5e489bfa5d0f", "The following Advance Payment request(s) cannot be printed"));
			errorMessages.AppendLine();
			bool hasError = false;

			if (headerGrid.SelectedElements.Length > 0)
			{
				var allSelectedRequests = headerGrid.GetSelectedElements<AccCashAdvanceRequestHeader>();
				if (allSelectedRequests.Any())
				{
					foreach (AccCashAdvanceRequestHeader header in allSelectedRequests)
					{
						if (!header.Organization.OH_IsActive)
						{
							hasError = true;
							errorMessages.AppendLine(Res.GetString("37c7e56b-f16c-4c28-873e-6fd5936f2bb6", "Advance Payment request {0} cannot be printed because it is for an inactive organization", header.CAH_RequestReferenceNumber));
						}
						else if (header.CAH_Status == CashAdvanceStatusCodes.RequestHeader.Cancelled)
						{
							hasError = true;
							errorMessages.AppendLine(Res.GetString("8969b850-81a1-4744-9e6f-6c829dd08962", "Advance Payment request {0} cannot be printed because it is canceled", header.CAH_RequestReferenceNumber));
						}
						else
						{
							requestCollection.Add(header);
						}
					}
					if (hasError)
					{
						Globals.Message.ShowInformation(errorMessages.ToString(), Res.GetString("59d12fd2-9e6d-4d9d-84cc-b29bc8e87d5b", "Print Advance Payment"));
					}
					if (requestCollection.Count > 0)
					{
						Print(allSelectedRequests);
					}
				}
			}
			else
			{
				string message = Res.GetString("f043457a-974f-459d-9df1-2d357b4277d5", "Please select a request or requests before printing.");
				string caption = Res.GetString("c1a74050-605a-47e8-b979-55d7dab94635", "Select a request");
				Globals.Message.ShowInformation(message, caption);
			}
		}

		void Print(IEnumerable<AccCashAdvanceRequestHeader> headers)
		{
			try
			{
				GetPrintTask(headers.ToList()).Run();
			}
			catch (UnableToFindInvoiceDocumentCommandException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		CashAdvancePrintTask GetPrintTask(List<AccCashAdvanceRequestHeader> headers)
		{
			return new CashAdvancePrintTask(headers);
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			CashAdvanceRequestHelper.MarkAsCancelRequest(SecurityHelper.GetInvSecurity(SecurityCore.CancelARCashAdvanceRequest), headerGrid.SelectedElements);
		}

		bool ShowPaidUnpaidButtons => DesignModeFinder.IsDesigning ||
										(ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed &&
											ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled) ||
										(ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed &&
											ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsPayablesCashAdvanceFunctionalityEnabled);

		void MarkAsPaidButton_Click(object sender, EventArgs e)
		{
			CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(SecurityHelper.GetInvSecurity(SecurityCore.MarkARCashAdvancePaid), headerGrid.SelectedElements, (ca) => ca.MarkAsPaid(), Res.GetString("9e92a312-a381-4ef1-8cdd-09352de166e0", "Paid"));
		}

		void MarkAsUnpaidButton_Click(object sender, EventArgs e)
		{
			CashAdvanceRequestHelper.MarkAsPaidOrUnpaid(SecurityHelper.GetInvSecurity(SecurityCore.MarkARCashAdvanceUnPaid), headerGrid.SelectedElements, (ca) => ca.UndoPaidStatus(), Res.GetString("fccdc94f-0507-4981-b92d-44550125e59e", "Unpaid"));
		}
	}
}
