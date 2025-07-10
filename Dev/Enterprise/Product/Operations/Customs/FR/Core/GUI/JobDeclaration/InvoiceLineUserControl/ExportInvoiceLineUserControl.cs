using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.Customs.FR.GUI.PlugIn;

namespace Enterprise.Customs.FR.GUI
{
	public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);

			AdditionalInfosTabPage.Text = CaptionProvider.AdditionalInfoTabPageCaption(false).Caption;
		}

		protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => CaptionProvider.AdditionalInfoTabPageCaption(false);

		protected override bool IsZG_TransactionNatureVisible => IsDeltaIE;

		bool IsDeltaIE => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsDeltaIE;

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(PreviousDocumentsUserControl);
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(SupportingDocumentsUserControl);
		}

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControl);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RemoveMethodOfPayment();
		}

		void RemoveMethodOfPayment()
		{
			var methodOfPaymentColumn = TaxGrid.GetColumnStyle("Data+G4_MethodOfPayment");
			TaxGrid.ColumnStyles.Remove(methodOfPaymentColumn);

			MethodOfPaymentDropEdit.Visible = false;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			const string isVisibleForBinding = "IsVisibleForBinding";

			PreviousEntryNumberTextBox.DataBindings.RemoveBinding(isVisibleForBinding);
			PreviousEntryLineNumberCalcEdit.DataBindings.RemoveBinding(isVisibleForBinding);
			BondedWhsQuantityCalcDropEdit.DataBindings.RemoveBinding(isVisibleForBinding);

			if (DataSource != null)
			{
				PreviousEntryNumberTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, "FilteredInvoiceLines.IsPreviousEntryNumberVisible"));
				PreviousEntryLineNumberCalcEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, "FilteredInvoiceLines.IsPreviousEntryNumberVisible"));
				BondedWhsQuantityCalcDropEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, "FilteredInvoiceLines.IsBondedWhsQuantityVisible"));
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			TransactionNatureDropEdit.Visible = IsZG_TransactionNatureVisible;
		}

		protected override EU.GUI.GuidedDecisionMakingForm GetGuidedDecisionMakingFormCore(EU.Business.GuidedDecisionMakingBasic gDMBasic, EU.Business.IGuidedDecisionMakingTarget target) => new GDM.GuidedDecisionMakingForm((GuidedDecisionMakingBasic)gDMBasic, (IGuidedDecisionMakingTarget)target);

		protected override ZString UniversalTariffType
		{
			get
			{
				var result = base.UniversalTariffType;
				if (JobDeclaration is JobDeclaration declaration && !declaration.JE_TariffType.IsEmpty)
				{
					result = declaration.JE_TariffType;
				}
				return result;
			}
		}
	}
}
