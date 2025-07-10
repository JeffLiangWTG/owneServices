using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CIQInvoiceLineDetailsUserControl : ZUserControl
	{
		public CIQInvoiceLineDetailsUserControl()
		{
			InitializeComponent();
			InitializeComponentLostByDesignMode();
		}

		void InitializeComponentLostByDesignMode()
		{
			DangerousGoodsGuidFindBox.ShouldResize = true;
			JI_CIQCodeFindBox.ShouldResize = true;
			BindingSource.SetBindingMember(CIQIngredientTextBox, nameof(JobComInvoiceLine.CIQIngredient));
			SyncCIQDetailsButton.ToolTipCaption = ResString.GetMultilingualString("8931851D-A57A-4710-BEA9-27FAE95AB282", "Synchronize CIQ Ingredient, Specification, Brand, Model, Manufacture Dates with Specification & Model (Additional Information)");

			CargoAttributesTextBox.SetBindingMember(nameof(JobComInvoiceLine.CargoAttributesAsString), x => (x as JobComInvoiceLine)?.CargoAttributes);
		}

		public JobComInvoiceLine InvoiceLine => CurrentDataItem as JobComInvoiceLine;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			JI_NonDangerousChemicalFlagCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingConst);

			if (dataSource != null)
			{
				JI_NonDangerousChemicalFlagCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoiceLines.NonDangerousChemicalFlagVisible", false, DataSourceUpdateMode.Never));
			}
		}
		const string IsVisibleForBindingConst = "IsVisibleForBinding";

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (InvoiceLine != null)
			{
				InvoiceLine.JI_CIQTariffInfo.ValueChanged -= JI_CIQTariffInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (InvoiceLine != null)
			{
				InvoiceLine.JI_CIQTariffInfo.ValueChanged += JI_CIQTariffInfo_ValueChanged;
			}
		}

		void JI_CIQTariffInfo_ValueChanged(object sender, EventArgs e)
		{
			var instruction = InvoiceLine.EntryInstruction;
			if (!InvoiceLine.JI_CIQTariff.IsEmpty && (instruction?.IsCIQRequiresEditableAndFalse ?? false) && (Globals.Message.Show(Res.GetString("9364722C-CD92-4CE7-92A4-667133545F19", "You are entering CIQ data, do you want to mark this Entry Instruction as ‘Requires CIQ’?’"), Res.GetString("8EDA7495-A0B2-4AC3-8B33-4F067525ABF6", "Continue?"), MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK))
			{
				instruction.CEI_CIQRequires = true;
			}
		}

		void SyncCIQDetailsButton_Click(object sender, EventArgs e)
		{
			InvoiceLine.SyncCIQDetails();
		}
	}
}
