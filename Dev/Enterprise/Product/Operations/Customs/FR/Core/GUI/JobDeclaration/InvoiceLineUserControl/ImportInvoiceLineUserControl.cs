using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.Customs.FR.GUI.PlugIn;

namespace Enterprise.Customs.FR.GUI
{
	public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			ChangeAdditionalInfosTabPageCaption();
			tariffBypassCodeControl.CaptionResourceString = CaptionProvider.TariffBypassCodeCaption;
			tariffBypassReasonControl.CaptionResourceString = CaptionProvider.TariffBypassReasonCaption;
		}

		protected override bool IsZG_CountryOfDispatchVisible => IsUCC6;

		protected override bool IsZG_TransactionNatureVisible => IsUCC6;

		protected override Type GetSupportingDocumentsUserControlType() => typeof(SupportingDocumentsUserControl);

		protected override Type GetPreviousDocumentsUserControlType() => typeof(InvoiceLinePreviousDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => IsUCC6
			? typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid)
			: typeof(AdditionalInfosUserControl);

		protected override Type GetValuationIndicatorsUserControlType() => typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl);

		protected override EU.GUI.PlugIn.TaxUserControl GetTaxUserControl() => new TaxUserControl();

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
			CusNumberCodeFindBox.Visible = ((JobDeclaration)JobDeclaration)?.IsUCC6 ?? false;
			TransactionNatureDropEdit.Visible = IsZG_TransactionNatureVisible;

			ChangeAdditionalInfosTabPageCaption();
		}

		protected override void SetTabPagesVisibilityCore()
		{
			var jobDeclaration = CurrentDataItem as JobDeclaration;
			var configuration = jobDeclaration?.Configuration.InvoiceLineConfiguration;
			if (configuration != null)
			{
				OrganizationsTabPage.TabVisible = configuration.OrganizationsSupport(jobDeclaration);
			}
		}

		protected override bool IsJI_TaxOrFeeDetailVisible => true;

		void ChangeAdditionalInfosTabPageCaption()
		{
			var captionResourceString = CaptionProvider.AdditionalInfoTabPageCaption(IsUCC6);
			AdditionalInfosTabPage.CaptionResourceString = captionResourceString;
			AdditionalInfosTabPage.Text = captionResourceString.Caption;
		}

		bool IsUCC6 => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6;

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
