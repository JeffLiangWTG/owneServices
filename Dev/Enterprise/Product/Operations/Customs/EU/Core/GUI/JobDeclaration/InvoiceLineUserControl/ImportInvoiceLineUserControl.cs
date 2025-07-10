using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUImportInvoiceLineUserControl : EUInvoiceLineUserControl
	{
		public EUImportInvoiceLineUserControl()
		{
			InitializeComponent();
			InitializeTaxUserControl();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			VatTypeDropEdit.Visible = IsJI_ZZF_NKTaxTypeVisible;
			VatDetailGuidDropEdit.Visible = IsJI_TaxOrFeeDetailVisible;
			ValuationAdjustmentCodeDropEdit.Visible = IsZG_ValueAdjustmentCodeVisible;
			ValuationAdjustmentPercentageCalcEdit.Visible = IsJI_ValuationMarkupVisible;
			SetQuotaControlsVisibility();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(GDMLink, new SuppressFormsLocalizedTestAttribute());
#endif
			LineDetailsTabPage.BindingOrFirstShown += LineDetailsTabPage_BindingOrFirstShown;
		}

		void LineDetailsTabPage_BindingOrFirstShown(object sender, EventArgs e)
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				var methodOfPaymentVisible = declaration.Configuration.InvoiceLineConfiguration.MethodOfPaymentVisibleOnImportControl(declaration);
				MethodOfPaymentDropEdit.Visible = methodOfPaymentVisible;
				if (methodOfPaymentVisible)
				{
					LineDetailsTabPage.AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(939, 330, true);
				}
				else
				{
					LineDetailsTabPage.AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(440, 330, true);
				}
			}
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override bool IsCifCalculationVisible => true;
		protected override bool IsCustomsValueCalulationVisible => true;
		protected override bool IsValueForVatGstCalculationVisible => true;
		protected virtual bool IsJI_ZZF_NKTaxTypeVisible => true;
		protected virtual bool IsJI_TaxOrFeeDetailVisible => false;
		protected virtual bool IsZG_ValueAdjustmentCodeVisible => false;
		protected virtual bool IsJI_ValuationMarkupVisible => true;
		protected virtual bool IsZG_CountryOfDestinationVisible => JobDeclaration is JobDeclaration declaration && declaration.Configuration.InvoiceLineConfiguration.CountryOfDestinationVisibleOnImportControl(declaration);
		protected virtual bool IsZG_CountryOfDispatchVisible => false;

		void InitializeTaxUserControl()
		{
			TaxUserControl = GetTaxUserControl();
			TaxUserControl.AllowDrop = true;
			BindingSource.SetBindingMember(TaxUserControl, ".");
			TaxUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			TaxUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			TaxUserControl.Name = "TaxUserControl";
			TaxUserControl.Size = ControlDpiScalingHelper.NewScaledSize(720, 283, true);
			TaxUserControl.TabIndex = 0;
			TaxTabPage.Controls.Add(TaxUserControl);
		}

		protected override void AddColumnsToGrid()
		{
			base.AddColumnsToGrid();

			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(
				new ZGridColumnInfo[]
				{
					new ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A79A9CFE-B430-4D02-9682-53329DA842F4", "Stat. Value", "Statistical Value"),
						ColumnName = "ZG_StatisticalValue",
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106)
					},
					new ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A9FB5F7E-BC36-4CED-8173-17C5E9AB7413", "Value Adj. Amount", "Value Adjustment Amount"),
						ColumnName = "JI_ValuationMarkup",
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117),
						IsVisible = IsJI_ValuationMarkupVisible,
						IsUnavailable = !IsJI_ValuationMarkupVisible
					},
					new ZDropEditColumnStyleInfo
					{
						ColumnName = "JI_ValuationCode",
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155)
					},
					new ZDropEditColumnStyleInfo
					{
						CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F452FB06-3B3D-4E03-80CA-774E43414D75", "Value Adj. Code", "Value Adjustment Code"),
						ColumnName = "ZG_ValueAdjustmentCode",
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
						IsVisible = IsZG_ValueAdjustmentCodeVisible,
						IsUnavailable = !IsZG_ValueAdjustmentCodeVisible
					},
					new ZDropEditColumnStyleInfo
					{
						ColumnName = "JI_PrimaryPreference",
					},
					new ZDropEditColumnStyleInfo
					{
						ColumnName = "JI_ZZF_NKTaxType",
						Caption = Res.GetString("590FECA1-E924-4B2F-888D-048AEFADC44F", "{0}", JobDeclaration?.CustomsVATTypeCaption ?? "VAT"),
						IsVisible = IsJI_ZZF_NKTaxTypeVisible,
						IsUnavailable = !IsJI_ZZF_NKTaxTypeVisible
					},
					new ZCalcEditColumnStyleInfo
					{
						ColumnName = "JI_BondedWhsQuantity",
						IsVisible = false
					},
					new ZDropEditColumnStyleInfo
					{
						ColumnName = "JI_BondedWhsUnitQty",
						IsVisible = false
					},
					new ZCodeFindBoxColumnStyleInfo
					{
						ColumnName = "ZG_CountryOfDestination",
						IsVisible = IsZG_CountryOfDestinationVisible,
						IsUnavailable = !IsZG_CountryOfDestinationVisible
					},
					new ZCodeFindBoxColumnStyleInfo
					{
						ColumnName = JobComInvoiceLine.Schema.ZG_CountryOfDispatch,
						IsVisible = IsZG_CountryOfDispatchVisible,
						IsUnavailable = !IsZG_CountryOfDispatchVisible
					},
					new ZDropEditColumnStyleInfo
					{
						ColumnName = "ZG_TransNature",
						Caption = Res.GetString("f0e12b15-1a6d-4f00-b8b3-23aa2f0a8ac4", "[24] Tran. Nature"),
						IsVisible = IsZG_TransactionNatureVisible,
						IsUnavailable = !IsZG_TransactionNatureVisible
					},
					new ZTextBoxColumnStyleInfo(JobComInvoiceLine.Schema.JI_PreviousEntryNumber,  ControlDpiScalingHelper.ScaleToCurrentDpiX(100))
					{
						GroupName = Res.GetData("EF4285D3-CCAE-4A70-B2C8-A88D3E53982C", "Prev. Entry No."),
						IsVisible = false
					},
					new ZCalcEditColumnStyleInfo(JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,  ControlDpiScalingHelper.ScaleToCurrentDpiX(120), 0)
					{
						GroupName = Res.GetData("EF4285D3-CCAE-4A70-B2C8-A88D3E53982C", "Prev. Entry No."),
						IsVisible = false
					}
				}
			);
		}

		protected override string[] GetDefaultColumnsForGrid()
		{
			var result = new List<string>(base.GetDefaultColumnsForGrid());
			var index = result.IndexOf(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
			result.Insert(index + 1, JobComInvoiceLine.Schema.JI_PrimaryPreference);

			if (IsJI_ZZF_NKTaxTypeVisible)
			{
				index = result.IndexOf(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty);
				result.Insert(index + 1, JobComInvoiceLine.Schema.JI_ZZF_NKTaxType);
			}

			index = result.IndexOf(JobComInvoiceLine.Schema.MergedLineNumber);
			result.Insert(++index, JobComInvoiceLine.Schema.JI_BondedWhsQuantity);
			result.Insert(++index, JobComInvoiceLine.Schema.JI_BondedWhsUnitQty);
			if (IsZG_TransactionNatureVisible)
			{
				result.Insert(++index, JobComInvoiceLine.Schema.ZG_TransNature);
			}
			if (IsZG_CountryOfDestinationVisible)
			{
				result.Insert(++index, JobComInvoiceLine.Schema.ZG_CountryOfDestination);
			}
			if (IsZG_CountryOfDispatchVisible)
			{
				result.Insert(++index, JobComInvoiceLine.Schema.ZG_CountryOfDispatch);
			}

			return result.ToArray();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			VatTypeDropEdit.Visible = IsJI_ZZF_NKTaxTypeVisible;
			ValuationAdjustmentCodeDropEdit.Visible = IsZG_ValueAdjustmentCodeVisible;
			ValuationAdjustmentPercentageCalcEdit.Visible = IsJI_ValuationMarkupVisible;
			SetQuotaControlsVisibility();
			CountryOfDestinationCodeFindBox.Visible = IsZG_CountryOfDestinationVisible;
			CountryOfDispatchCodeFindBox.Visible = IsZG_CountryOfDispatchVisible;
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			CustomsInvoiceLinesBoundGrid.SetAvailability(IsZG_CountryOfDestinationVisible, JobComInvoiceLine.Schema.ZG_CountryOfDestination);
			CustomsInvoiceLinesBoundGrid.SetAvailability(IsZG_CountryOfDispatchVisible, JobComInvoiceLine.Schema.ZG_CountryOfDispatch);
		}

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Import;

		protected virtual bool ShouldTaxDocsDeprecated() => false;

		public static string TaxDocsIsDeprecatedMessage => Res.GetString("E64C5022-4737-445F-B9B8-2642ACDD59C9", "This function is deprecated now that {0} uses Universal Tariff for measures data.", Core.Constants.ProductName);

		public void SetTabPagesVisibility() => SetTabPagesVisibilityCore();
		protected virtual void SetTabPagesVisibilityCore()
		{
		}

		protected override void SetAdditionalSupplementaryCodeVisibility(bool hasAdditionalCodes)
		{
			base.SetAdditionalSupplementaryCodeVisibility(hasAdditionalCodes);
			if (!hasAdditionalCodes)
			{
				GDMLink.Location = ControlDpiScalingHelper.NewScaledPoint(421, 19, true);
			}
		}

		protected virtual TaxUserControl GetTaxUserControl() => new TaxUserControl();

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			VatTypeDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("43709BBC-2BB4-4B42-9D32-0191243B99C6", "{0}", JobDeclaration.CustomsVATTypeCaption);
		}

		protected override Type GetOrganizationsUserControlType() => typeof(ImportInvoiceLineOrganizationsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => IsUCC6
			? typeof(AdditionalInfosUserControlWithGrid)
			: typeof(AdditionalInfosUserControl);

		bool IsUCC6 => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6;

		void SetQuotaControlsVisibility()
		{
			var isSecondQuotaVisible = JobDeclaration is JobDeclaration declaration && declaration.Configuration.InvoiceLineConfiguration.SecondQuotaVisible(declaration);
			var useUniversalTariff = UseUniversalTariff;
			QuotaDropEdit.Visible = useUniversalTariff;
			QuotaTextBox.Visible = !useUniversalTariff;
			SecondQuotaDropEdit.Visible = isSecondQuotaVisible && useUniversalTariff;
			SecondQuotaTextBox.Visible = isSecondQuotaVisible && !useUniversalTariff;
		}

		void CheckQuotaBalanceLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var url = EU.Registry.EUCustomsDataRegistry.Instance.QuotaBalanceURL.Value;
			var lang = GlbStaff.CurrentUser.Language;
			var quotaCode = QuotaTextBox.Visible ? (ZString)QuotaTextBox.Text : (ZString)QuotaDropEdit.Text;
			if (quotaCode.Length > 6)
			{
				quotaCode = string.Empty;
			}
			url = url + (NoResString)"?" + (NoResString)"Lang=" + lang.ToLower() + (quotaCode.IsEmpty ? string.Empty : (NoResString)"&Code=" + quotaCode + (NoResString)"&Expand=true");
			WebUrlLauncher.Launch(url);
		}
	}
}
