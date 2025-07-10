using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public enum ConsolCostingSingleEditResult
	{
		Apportion, Cancel
	}

	public partial class APInvoiceConsolCostingSingleEditForm : ZChildForm
	{
		public APInvoiceConsolCostingSingleEditForm(JobConsolCost cost) : base(cost)
		{
			ApplyButton.Click += new EventHandler(ApplyButton_Click);
			if (!BusinessEntity.Factory.HasContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice))
			{
				if (BusinessEntity.Factory.HasContext(BusinessContext.OSTaxAmountModifiedFromCalculatedAmountForConsolCost))
				{
					ErrorReporter.ReportOnce("OSTaxAmountModifiedFromCalculatedAmountForConsolCost", "OSTaxAmountModifiedFromCalculatedAmountForConsolCost context is set before ModifyingConsolCostDetailsFromAPInvoice context is set.");
				}
				BusinessEntity.Factory.SetContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice);
			}
			reportingDeletedApportionmentChargesSuspender = cost.ParentAPInvoice.GetReportingDeletedApportionmentChargesSuspender();
		}

		void ApplyButton_Click(object sender, EventArgs e)
		{
			ContinueWithSave validateAndSaveResult = ValidateAndSave();
			if (validateAndSaveResult == ContinueWithSave.Yes)
			{
				Close();
			}
		}

		readonly IDisposable reportingDeletedApportionmentChargesSuspender;

		public override string FormVerb
		{
			get { return FormVerbs.Edit; }
		}

		public ConsolCostingSingleEditResult FormResult = ConsolCostingSingleEditResult.Cancel;

		protected override void Save(ITransactionParticipant[] factories)
		{
			FormResult = ConsolCostingSingleEditResult.Apportion;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				CostDetails.Controls.Remove(govtChargeCodeTextBox);
				ApportionedChargesGrid.RemoveFromAvailableColumns(AutoJobCharge.Schema.JR_CostGovtChargeCode);
			}

			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				fixedPlaceOfSupplyDropEdit.Visible = false;
				ApportionedChargesGrid.RemoveFromAvailableColumns(AutoJobCharge.Schema.JR_CostPlaceOfSupply);
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (FormResult == ConsolCostingSingleEditResult.Cancel)
			{
				DialogResult result = Globals.Message.Show(Res.GetString("00cd2337-ef72-4b50-b995-5a34d67c4006", "Do you want to apply changes?"), Res.GetString("5847d098-3869-4fc6-80b5-afeee9763503", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
				if (result == DialogResult.Yes)
				{
					ContinueWithSave validateAndSaveResult = ValidateAndSave();
					if (validateAndSaveResult == ContinueWithSave.No)
					{
						e.Cancel = true;
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			try
			{
				base.OnClosed(e);
			}
			finally
			{
				BusinessEntity.Factory.RemoveContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice);

				reportingDeletedApportionmentChargesSuspender.Dispose();
			}
		}
	}
}
