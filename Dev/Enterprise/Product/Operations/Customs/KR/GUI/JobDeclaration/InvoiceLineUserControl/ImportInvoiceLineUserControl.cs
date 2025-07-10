using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			AddColumnsToGrid();
			ChangeColumnsInGrid();
			ReOrderColumns();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			ReOrderDetailsTabPage();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			JI_Calc_CIFConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_FOBConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("B589E175-AE72-45B1-8A10-386E32C386F2", "Customs Value");
		}

		void ReOrderDetailsTabPage()
		{
			LineDetailTabControl.Controls.Clear();
			LineDetailTabControl.Controls.Add(LineDetailsTabPage);
			LineDetailTabControl.Controls.Add(ApprovalAndReImportTabPage);
			LineDetailTabControl.Controls.Add(CertificateOfOriginAndFTADetailsTabPage);
			LineDetailTabControl.Controls.Add(OtherDetailsTabPage);
			LineDetailTabControl.Controls.Add(ExemptionOrSpecificUseRateTabPage);
			LineDetailTabControl.Controls.Add(ContainersTabPage);
			LineDetailTabControl.Controls.Add(LineChargesTabPage);
			ReorderCustomFieldsTab();
		}

		void AddColumnsToGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PrimaryPreference,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_SecondaryPreference,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|E983580C-E77A-4DEB-9E21-E9B98B346605", "Customs Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsUnitQty,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|E983580C-E77A-4DEB-9E21-E9B98B346605", "Customs Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|B65489FD-F430-46BF-A138-AD4772136CA9", "Customs Qty 2"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|B65489FD-F430-46BF-A138-AD4772136CA9", "Customs Qty 2"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					IsVisible = false
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|ED90D7F6-34BA-40B1-ADA7-CC022E8DDABF", "Customs Qty 3"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|ED90D7F6-34BA-40B1-ADA7-CC022E8DDABF", "Customs Qty 3"),
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					IsVisible = false
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsFourthQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|CC75DA19-56F7-430A-AC5A-83CC3E947751", "Customs Qty 4"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|CC75DA19-56F7-430A-AC5A-83CC3E947751", "Customs Qty 4"),
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsFourthUnitQty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					IsVisible = false
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_InstallmentCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_IsSpecificUseCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_AdditionalDutyType,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|E8BD3637-7CD2-42DF-B88E-C50854FCC91D", "Additional Duty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_AdditionalDutyRate,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|E8BD3637-7CD2-42DF-B88E-C50854FCC91D", "Additional Duty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_DomesticTaxCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_DomesticTaxExemptionCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.EducationTaxExemptIndicator),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsReadOnly = true
				},
				new ZGuidDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CEI,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_DrawbackQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|3E96B69A-8516-47C6-9EED-E963860CDD11", "Drawback Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_DrawbackUQ,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|3E96B69A-8516-47C6-9EED-E963860CDD11", "Drawback Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.CustomsUnitPrice),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.DutyRateCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.DutyRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.DutyReductionRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_SpecificUseCodeDutyRatePermitNo),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.DomesticTaxType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.DomesticTaxRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_COOLabelLocation,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_COOLabelType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_COOExemptionReason,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_Model,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_BrandCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_BrandName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_Ingredient,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.DomesticTaxBaseQtyOrPrice),
					GroupName = Res.GetData("ImportInvoiceLineUserControl|1634983F-FCB5-43DB-9366-EF79961E9823", "Domestic Tax Base"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.DomesticTaxBaseUnit),
					GroupName = Res.GetData("ImportInvoiceLineUserControl|1634983F-FCB5-43DB-9366-EF79961E9823", "Domestic Tax Base"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_VATReductionCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.AgricultureTaxClassification),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_LotNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.CusEntryLine) + "+" + Customs.Business.CusEntryLine.Schema.CL_LineNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_SequenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_ProductTypeCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_ParentLine,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA1,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA3,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_MightRequireInspection,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CourierCargoSelectivityIndicator,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.CertificateOfOriginProductType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.IssuedInThirdCountry),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true,
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_RN_NKSecondCommercialInvoiceCountry,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.CountryOfOriginExporterNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true,
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.COOSplitOrder),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_COOSupportingDocType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.COOIssuerType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.COOTotalNetWeight),
					GroupName = Res.GetData("ImportInvoiceLineUserControl|FF15A735-2CFC-4231-989F-F27E51546210", "CO Total Net Weight"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.COOTotalNetWeightUQ),
					GroupName = Res.GetData("ImportInvoiceLineUserControl|FF15A735-2CFC-4231-989F-F27E51546210", "CO Total Net Weight"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.COOLineNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsFifthQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|0E34A1EF-4ACE-4E6C-921F-9D7B36E5B747", "CO Used Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.COOUsedQuantityUQ),
					GroupName = Res.GetData("ImportInvoiceLineUserControl|0E34A1EF-4ACE-4E6C-921F-9D7B36E5B747", "CO Used Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_InstallationCost),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsVisible = false,
				},
				new ZCheckBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_CoveredByCOOExporter),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170),
					IsVisible = false,
				},
			});

			InvoiceLineCharges.ChargesGrid.AddExchangeRateColumn();
			InvoiceLineCharges.ApportionedChargesGrid.AddExchangeRateColumn();
		}

		void ChangeColumnsInGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity));
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty));
		}

		void ReOrderColumns()
		{
			InvoiceLineCharges.ChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.LineChargeColumnsInOrder);
			InvoiceLineCharges.ApportionedChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.LineChargeColumnsInOrder);
			CustomsInvoiceLinesBoundGrid.ReOrderColumnsAndChangeVisibility(invLines_LineDetails);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DetailsPanel.UpdateLayout(new DetailsLayout());
			PostClearanceDetailsPanel.UpdateLayout(new PostClearanceDetailsLayout());
			DutyReductionDetailsPanel.UpdateLayout(new DutyReductionDetailsLayout());
			ReExportReductionDetailsPanel.UpdateLayout(new ReExportReductionDetailsLayout());
			Splitter.MinSize = BottomPanel.MinimumSize.Height;

			SetExemptionOrSpecificUseRateTabPageVisibility();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var listManager = CustomsInvoiceLinesBoundGrid?.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged -= TabPageVisibility_ValueChanged;
				listManager.CurrentChanged += TabPageVisibility_ValueChanged;
			}
		}

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);
			if (invoiceLine != null)
			{
				var line = (JobComInvoiceLine)invoiceLine;
				line.JI_IsSpecificUseCodeInfo.ValueChanged += TabPageVisibility_ValueChanged;
				line.JI_SecondaryPreferenceInfo.ValueChanged += TabPageVisibility_ValueChanged;
				line.JI_InstallmentCodeInfo.ValueChanged += TabPageVisibility_ValueChanged;
			}
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);
			if (invoiceLine != null)
			{
				var line = (JobComInvoiceLine)invoiceLine;
				line.JI_IsSpecificUseCodeInfo.ValueChanged -= TabPageVisibility_ValueChanged;
				line.JI_SecondaryPreferenceInfo.ValueChanged -= TabPageVisibility_ValueChanged;
				line.JI_InstallmentCodeInfo.ValueChanged -= TabPageVisibility_ValueChanged;
			}
		}

		void TabPageVisibility_ValueChanged(object sender, EventArgs e)
		{
			SetExemptionOrSpecificUseRateTabPageVisibility();
		}

		protected void SetExemptionOrSpecificUseRateTabPageVisibility()
		{
			var invoiceLine = CurrentInvoiceLine as JobComInvoiceLine;
			var code = invoiceLine?.DutyReductionClassificationCode ?? ZString.Empty;
			if (code == ImportDutyReductionClassificationList.Codes.Invalid || code == ZString.Empty)
			{
				ExemptionOrSpecificUseRateTabPage.TabVisible = false;
			}
			else
			{
				ExemptionOrSpecificUseRateTabPage.TabVisible = true;
			}
		}

		static string[] invLines_LineDetails => new string[]
		{
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_FormattedTariff,
			JobComInvoiceLine.Schema.JI_InvoiceQuantity,
			JobComInvoiceLine.Schema.JI_InvoiceUQ,
			JobComInvoiceLine.Schema.UnitPrice,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_Description,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_DrawbackQuantity,
			JobComInvoiceLine.Schema.JI_DrawbackUQ,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin),
			JobComInvoiceLine.Schema.JI_COOLabelLocation,
			JobComInvoiceLine.Schema.JI_COOLabelType,
			JobComInvoiceLine.Schema.JI_COOExemptionReason,
			JobComInvoiceLine.Schema.JI_Model,
			JobComInvoiceLine.Schema.JI_BrandCode,
			JobComInvoiceLine.Schema.JI_BrandName,
			JobComInvoiceLine.Schema.JI_Ingredient,
			nameof(JobComInvoiceLine.DutyRateCode),
			JobComInvoiceLine.Schema.JI_PrimaryPreference,
			nameof(JobComInvoiceLine.DutyRate),
			JobComInvoiceLine.Schema.JI_AdditionalDutyType,
			JobComInvoiceLine.Schema.JI_AdditionalDutyRate,
			JobComInvoiceLine.Schema.JI_SecondaryPreference,
			nameof(JobComInvoiceLine.DutyReductionRate),
			JobComInvoiceLine.Schema.JI_InstallmentCode,
			JobComInvoiceLine.Schema.JI_SpecificUseCodeDutyRatePermitNo,
			JobComInvoiceLine.Schema.JI_IsSpecificUseCode,
			JobComInvoiceLine.Schema.JI_DomesticTaxCode,
			nameof(JobComInvoiceLine.DomesticTaxType),
			nameof(JobComInvoiceLine.DomesticTaxRate),
			JobComInvoiceLine.Schema.JI_DomesticTaxExemptionCode,
			nameof(JobComInvoiceLine.DomesticTaxBaseQtyOrPrice),
			nameof(JobComInvoiceLine.DomesticTaxBaseUnit),
			JobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
			JobComInvoiceLine.Schema.JI_VATReductionCode,
			nameof(JobComInvoiceLine.EducationTaxExemptIndicator),
			nameof(JobComInvoiceLine.AgricultureTaxClassification),
			JobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.JI_LotNumber,
			nameof(JobComInvoiceLine.CusEntryLine) + "+" + Customs.Business.CusEntryLine.Schema.CL_LineNumber,
			JobComInvoiceLine.Schema.JI_SequenceNumber,
			JobComInvoiceLine.Schema.JI_ProductTypeCode,
			JobComInvoiceLine.Schema.JI_ParentLine,
			JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA1,
			JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA2,
			JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA3,
			JobComInvoiceLine.Schema.JI_MightRequireInspection,
			JobComInvoiceLine.Schema.JI_CourierCargoSelectivityIndicator,
		};
	}
}
