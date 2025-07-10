using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class ImportInvoiceLineUserControl : EUImportInvoiceLineUserControl
	{
		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			AddColumnToGrid();
			SetEntryInstructionsVisibility();

			MoveSplitterUp();
			InvoiceLineFormHelper.SwitchOutCPCFindBoxToBeFormattedProcedureCodeFindBox(this, true);

			PreferenceCodeDropEdit.CaptionResourceString = null;
			StatisticalValueCalcEdit.CaptionResourceString = null;
			MethodOfPaymentDropEdit.CaptionResourceString = null;
			CountryOfSupplyCodeFindBox.CaptionResourceString = null;

			GDMLink.AllowOverlap(JI_AdditionalSupplementsTextBox);
			SupplementaryCode2DropEdit.AllowOverlap(zLabel5);
			SupplementaryCode1DropEdit.AllowOverlap(zLabel4);
			SupplementaryCode1DropEdit.AllowOverlap(zLabel2);
			zLabel2.AllowOverlap(zLabel4);
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);

			if (JobDeclaration.Invoices.Any())
			{
				BindingSource.SetBindingMember(this.SupervisingOfficeAddressControl, "FilteredInvoiceLines.SupervisingOfficeDocAddress");
			}
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
				PreviousEntryNumberTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, BindingSource.DataSource, "FilteredInvoiceLines.IsPreviousEntryNumberVisible", false, DataSourceUpdateMode.Never));
				PreviousEntryLineNumberCalcEdit.DataBindings.Add(new KBinding(isVisibleForBinding, BindingSource.DataSource, "FilteredInvoiceLines.IsPreviousEntryNumberVisible", false, DataSourceUpdateMode.Never));
				BondedWhsQuantityCalcDropEdit.DataBindings.Add(new KBinding(isVisibleForBinding, BindingSource.DataSource, "FilteredInvoiceLines.IsBondedWhsQuantityVisible", false, DataSourceUpdateMode.Never));
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			InvoiceLineFormHelper.SwitchOutCPCColumnToBeFormattedProcedureColumn(this);

			this.SetTariffColumnStyleInfo(UseUniversalTariff);

			var isJobComInvoiceLineMethodOfPaymentAvailableOnGui = JobDeclaration != null;
			MethodOfPaymentDropEdit.Visible = isJobComInvoiceLineMethodOfPaymentAvailableOnGui;
			CustomsInvoiceLinesBoundGrid.SetColumnVisible(isJobComInvoiceLineMethodOfPaymentAvailableOnGui, EU.Business.Declaration.AutoJobComInvoiceLine.Schema.ZG_MethodOfPayment);
			CustomsInvoiceLinesBoundGrid.SetAvailability(isJobComInvoiceLineMethodOfPaymentAvailableOnGui, EU.Business.Declaration.AutoJobComInvoiceLine.Schema.ZG_MethodOfPayment);
			CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);
		}

		protected override bool IsStatValueAndManualOverrideVisible_NbThisIsNotTheFieldIntheCalculationsAreaButTheOneLabeled46 => true;

		protected override bool IsZG_ValueAdjustmentCodeVisible => false;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			this.SetTariffFindBox(UseUniversalTariff);
			SetControlsProperty();
		}

		void AddColumnToGrid()
		{
			zDropEditColumnStyleInfoMethoOfPayment = new ZDropEditColumnStyleInfo
			{
				BindToList = "AddInfoLookups.MethodOfPaymentList",
				ColumnName = EU.Business.Declaration.AutoJobComInvoiceLine.Schema.ZG_MethodOfPayment,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("45079BAB-F00E-49D9-B2C7-373D31066FA8", "Method of Payment")
			};

			zDropEditColumnStyleInfoDispatchCountry = new ZDropEditColumnStyleInfo
			{
				BindToList = "Lookups.CountryOfExportList",
				ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			};

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfoMethoOfPayment);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfoDispatchCountry);
		}

		void MoveSplitterUp()
		{
			Splitter.Location = ControlDpiScalingHelper.NewScaledPoint(0, 142, true); //higher position in y-axis
			BottomPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 160, true); //higher position in y-axis
			BottomPanel.Size = ControlDpiScalingHelper.NewScaledSize(968, 420, true); //higher size
		}

		void SetControlsProperty()
		{
			var jobDeclaration = JobDeclaration;

			if (jobDeclaration != null)
			{
				CustomsInvoiceLinesBoundGrid.RefreshColumnCaptions(typeof(JobComInvoiceLine), jobDeclaration.MultipleKeysToUse);

				goodsOriginDropEdit.ShowDescriptionBox = false;
				SupervisingOfficeAddressControl.Visible = false;
				ValuationAdjustmentCodeDropEdit.Visible = false;
				ValuationAdjustmentPercentageCalcEdit.Visible = false;
				PackagesPivotTabPage.Text = jobDeclaration.InvoiceLinePackagesPivotTabCaption;
				TaxTabPage.Text = jobDeclaration.InvoiceLineTaxTabCaption;
				zLabel2.Text = jobDeclaration.InvoiceLineAdditionalCodesCaption;
				zLabel4.Text = jobDeclaration.InvoiceLineBlankCaption;
				zLabel5.Text = jobDeclaration.InvoiceLineBlankCaption;
				SupportingDocumentsTabPage.Text = jobDeclaration.SupportingDocumentsCaption;
				PreviousDocumentsTabPage.Text = jobDeclaration.PreviousDocumentsCaption;
				AdditionalInfosTabPage.Text = jobDeclaration.AdditionalInfoCaption;
				VatDetailGuidDropEdit.Visible = false;
				VatTypeDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_ZZF_NKTaxType), jobDeclaration.MultipleKeysToUse)?.Caption;

				JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 16, true);
				JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 21, true);
				JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 40, true);
				JI_LinePriceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
				goodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 40, true);
				goodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 17, true);
				JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 65, true);
				JI_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
				ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 146, true);
				ValuationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);

				tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 16, true);
				zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 19, true);
				SupplementaryCode1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 16, true);
				SupplementaryCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 16, true);
				zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 19, true);
				zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 19, true);
				SupplementaryCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 16, true);
				SupplementaryCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 16, true);
				JI_AdditionalSupplementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 16, true);
				AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(806, 14, true);
				GDMLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(861, 19, true);
				CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 198, true);
				CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
				zTextBoxAddtionalProcedureCodeAsString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 198, true);
				zButtonMoreAdditionalProcedureCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(852, 196, true);

				CountryOfSupplyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(899, 40, true);
				VatTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(899, 65, true);
				MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(899, 89, true);

				QuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 120, true);
				QuotaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 120, true);
				SecondQuotaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 120, true);
				SecondQuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 120, true);
				CheckQuotaBalanceLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 122, true);
				SpoffFromDeclarantButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 120, true);
				BondedWhsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 224, true);

				Controls.Find("SpoffFromDeclarantButton", true).Single().Visible = false;
			}
		}

		protected override EU.GUI.PlugIn.TaxUserControl GetTaxUserControl()
		{
			return new TaxUserControl();
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(GBSupportingDocumentsUserControl);
		}

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(GBPreviousDocumentsUserControl);
		}

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(GBAdditionalInfosUserControl);
		}

		protected override ResourceStringData GetValueIndicatorsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => Res.GetData("d0e674a3-58f2-47fe-9468-35ac8ea92b2b", "[UCC 4/13] Value Indicators");

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				declaration.ZG_ManualCalcInfo.ValueChanged -= ZG_ManualCalcInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				declaration.JE_NorthernIrelandModeInfo.ValueChanged += JE_NorthernIrelandModeInfo_ValueChanged;
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var declaration = CurrentDataItem as JobDeclaration;
			if (declaration != null)
			{
				var supplementaryCodeProvider = SupplementaryCodeProvider.GetByJobDeclaration(declaration);
				SetAdditionalSupplementaryCodeVisibility(supplementaryCodeProvider.NumberOfCodes > 0);
				declaration.ZG_ManualCalcInfo.ValueChanged += ZG_ManualCalcInfo_ValueChanged;
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				declaration.JE_NorthernIrelandModeInfo.ValueChanged += JE_NorthernIrelandModeInfo_ValueChanged;
				ControlVisiblityOfLineChargesTabPage();
				SetGoodsCategoryDropEditVisibility();
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisiblityOfLineChargesTabPage();
		}

		void ZG_ManualCalcInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisiblityOfLineChargesTabPage();
		}

		void JE_NorthernIrelandModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetGoodsCategoryDropEditVisibility();
		}

		void ControlVisiblityOfLineChargesTabPage()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				ControlVisiblityOfLineChargesTabPage(declaration.UseStandardValuation);
			}
		}

		void SetGoodsCategoryDropEditVisibility()
		{
			var isVisible = CurrentDataItem is JobDeclaration declaration && declaration.IsNorthernIrelandDomestic;
			GoodsCategoryDropEdit.Visible = isVisible;
			InvoiceDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(InvoiceDetailsGroupBox.Size.Width, isVisible ? 140 : 114);
			if (isVisible && string.IsNullOrEmpty(GoodsCategoryDropEdit.DescriptionBox.Text))
			{
				GoodsCategoryDropEdit.DescriptionBox.Text = new GoodsCategoryList().GetDescriptionFromCode(GoodsCategoryDropEdit.Text);
			}
		}

		void SpoffFromDeclarant_Click(object sender, EventArgs e)
		{
			EUExportInvoiceLineUserControl.SetSpoffOnLine(CustomsInvoiceLinesBoundGrid);
		}

		protected override bool UseUniversalTariff => true;

		protected override string[] GetDefaultColumnsForGrid()
		{
			var result = new List<string>(base.GetDefaultColumnsForGrid());
			result.Add(JobComInvoiceLine.Schema.JI_Calc_InstructionDisplaySequence);
			return result.ToArray();
		}
	}
}
