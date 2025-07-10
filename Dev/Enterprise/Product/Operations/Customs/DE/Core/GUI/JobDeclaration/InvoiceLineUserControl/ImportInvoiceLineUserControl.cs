using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			InvoiceLineDetailsUserControl.AllowOverlap(DynamicAdditionalInfoPanel);
			BottomPanel.AllowOutsideOfParent();
		}

		public new JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentInvoiceLine;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var netPriceGroupName = Res.GetData("8521A66E-C49E-4756-BF1E-8D51571D2FB4", "Net Price");

				CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZGridColumnInfo[]
				{
					new ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						CaptionResourceString = Res.GetData("C7BC9398-6DEF-4735-BE02-7B36C7DC7E19", "Net Price"),
						ColumnName = "JI_NetPrice",
						GroupName = netPriceGroupName,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
					},
					new ZCodeFindBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("C0DB7F80-ACBA-4AD9-BEB6-DC2220DB5EA7", "Currency Code"),
						ColumnName = "JI_RX_NKNetPriceCurr",
						GroupName = netPriceGroupName,
						BindToList = "Lookups+CurrencyList",
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
					}
				});
			}

			var customsQunatityGroupName = Res.GetData("300f031b-5301-423c-93a3-8c62eedbe9bf", "[38] Net Mass Measure");
			CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CustomsQuantity, 150);
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).GroupName = customsQunatityGroupName;
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty).GroupName = customsQunatityGroupName;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_StatisticalValue));
		}

		protected override void SetAdditionalSupplementaryCodeVisibility(bool hasAdditionalCodes)
		{
			base.SetAdditionalSupplementaryCodeVisibility(hasAdditionalCodes);
			ValuationMethodDropEdit.Visible = false;
			ValuationAdjustmentCodeDropEdit.Visible = false;
			ValuationAdjustmentPercentageCalcEdit.Visible = false;
		}

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (jobComInvoiceLine != null)
			{
				jobComInvoiceLine.JI_CEIInfo.ValueChanged += EntryInstruction_CEI_StyleInfo_ValueChanged;
				if (jobComInvoiceLine.EntryInstruction != null)
				{
					jobComInvoiceLine.EntryInstruction.CEI_StyleInfo.ValueChanged += EntryInstruction_CEI_StyleInfo_ValueChanged;
				}
				EntryInstruction_CEI_StyleInfo_ValueChanged(null, null);
			}
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (jobComInvoiceLine != null)
			{
				jobComInvoiceLine.JI_CEIInfo.ValueChanged -= EntryInstruction_CEI_StyleInfo_ValueChanged;
				if (jobComInvoiceLine.EntryInstruction != null)
				{
					jobComInvoiceLine.EntryInstruction.CEI_StyleInfo.ValueChanged -= EntryInstruction_CEI_StyleInfo_ValueChanged;
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetAdditionalInfoPanelLayout();

			if (!DesignModeFinder.IsDesigning)
			{
				InitTabsVisibility();
			}
		}

		protected virtual IPanelLayoutProvider GetAdditionalInfoPanelLayout() => new ImportAdditionalInfoLayout();

		protected override Type GetAdditionalInfosUserControlType() => typeof(PlugIn.AdditionalInfosUserControl);

		protected override Type GetPreviousDocumentsUserControlType() => null;

		protected override TaxUserControl GetTaxUserControl() => new PlugIn.SpecialCaseTaxUserControl();

		protected override Type GetSupportingDocumentsUserControlType() => typeof(ImportInvoiceLineSupportingDocumentsUserControl);

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override ZBool HasDifferentPanelLayout => ZBool.True;

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl() => new InvoiceLineChargesUserControl();

		protected override bool IsJI_ZZF_NKTaxTypeVisible => false;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			VatTypeDropEdit.Visible = false;
		}

		void EntryInstruction_CEI_StyleInfo_ValueChanged(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			invoiceLine?.ClearInwardMovementQuantityIfRequired();
			var instruction = invoiceLine?.EntryInstruction;
			InwardProcessingTabPage.TabVisible = instruction?.EnabledInwardProcessing ?? false;

			InitTabsVisibility();
		}

		protected override void SetGridLineColumnCharacterCasing()
		{
			var descColumn = CustomsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_Description);
			if (descColumn != null)
			{
				descColumn.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			}
		}

		void SetAdditionalInfoPanelLayout()
		{
			DynamicAdditionalInfoPanel.UpdateLayout(GetAdditionalInfoPanelLayout());
		}

		void InitTabsVisibility()
		{
			var instruction = CurrentInvoiceLine?.EntryInstruction;
			var isInwardProcessingAVABR = instruction == null || instruction.CEI_Style != DE.Business.ImportDeclarationTypeList.Codes.AVABR;
			LineChargesTabPage.TabVisible = isInwardProcessingAVABR;
			SupportingDocumentsTabPage.TabVisible = isInwardProcessingAVABR;
			AdditionalInfosTabPage.TabVisible = isInwardProcessingAVABR;
			PackagesPivotTabPage.TabVisible = isInwardProcessingAVABR;
			TaxTabPage.TabVisible = isInwardProcessingAVABR;
			CustomFieldsTabPage.TabVisible = isInwardProcessingAVABR;
		}
	}
}
