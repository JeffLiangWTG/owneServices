using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			AddColumnsToGrid();
			ChangeColumnsInGrid();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			ReOrderDetailsTabPage();
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetDynamicExportOtherDetailsPanelLayout();

			Splitter.MinSize = BottomPanel.MinimumSize.Height;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ReOrderColumns();
		}

		void SetDynamicExportOtherDetailsPanelLayout()
		{
			DynamicSteelExportPanel.UpdateLayout(new ExportSteelExportLayout());
		}
		void ReOrderDetailsTabPage()
		{
			LineDetailTabControl.Controls.Clear();
			LineDetailTabControl.Controls.Add(LineDetailsTabPage);
			LineDetailTabControl.Controls.Add(OtherDetailsTabPage);
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
					ColumnName = nameof(JobComInvoiceLine.CertificateOfOriginIssueStatus),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.JI_COOLabelLocation),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_Model,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_BrandName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_SequenceNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_LotNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.PRA_ReferenceNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsVisible = false
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.PRA_DateOfIssue),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					IsVisible = false
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.PRA_DateOfExpiry),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					IsVisible = false
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.JI_SkipManifestReport),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsVisible = false
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.CustomsUnitPrice),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PrimaryPreference,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					GroupName = Res.GetData("F2ADDB43-B867-4B11-A5EC-A62881D07BAA", "Second Customs Qty, UQ"),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					GroupName = Res.GetData("F2ADDB43-B867-4B11-A5EC-A62881D07BAA", "Second Customs Qty, UQ"),
					IsVisible = false
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_NoOfPacks),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Res.GetData("LocalExportInvoiceLineUserControl|CDE83046-0053-4EFE-A7F3-C0B6C5BFA7D4", "Packages"),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.JI_PackType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
					GroupName = Res.GetData("LocalExportInvoiceLineUserControl|CDE83046-0053-4EFE-A7F3-C0B6C5BFA7D4", "Packages"),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_PreviousEntryNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_Ingredient),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.CusEntryLine) + "+" + nameof(Business.CusEntryLine.CL_LineNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZGuidDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CEI,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				}
			});

			InvoiceLineCharges.ApportionedChargesGrid.AddIncludedInInvoiceColumn(ColumnTitleWhenExportForIncludedInInvoiceAmount);
			InvoiceLineCharges.ChargesGrid.AddExchangeRateColumn();
			InvoiceLineCharges.ApportionedChargesGrid.AddExchangeRateColumn();
		}

		void ChangeColumnsInGrid()
		{
			InvoiceLineCharges.ChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, ColumnTitleWhenExportForDutiable);
			InvoiceLineCharges.ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, ColumnTitleWhenExportForDutiable);
			InvoiceLineCharges.ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, ColumnTitleWhenExportForIncludedInInvoiceAmount);

			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Weight).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Volume).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CusContainerInvoiceLineGrid.GetColumnStyle(NonPersistentCusContainer.Schema.IsForInvoiceLine).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			CusContainerInvoiceLineGrid.GetColumnStyle(NonPersistentCusContainer.Schema.OwnerCountry).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Percentage).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		}

		void ReOrderColumns()
		{
			InvoiceLineCharges.ChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.LineChargeColumnsInOrder);
			InvoiceLineCharges.ApportionedChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.LineChargeColumnsInOrder);
			CustomsInvoiceLinesBoundGrid.ReOrderColumnsAndChangeVisibility(invLines_LineDetails);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
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
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_NoOfPacks,
			JobComInvoiceLine.Schema.JI_PackType,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			nameof(JobComInvoiceLine.CertificateOfOriginIssueStatus),
			nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin),
			nameof(JobComInvoiceLine.JI_COOLabelLocation),
			JobComInvoiceLine.Schema.JI_PrimaryPreference,
			JobComInvoiceLine.Schema.JI_Model,
			JobComInvoiceLine.Schema.JI_BrandName,
			nameof(JobComInvoiceLine.JI_Ingredient),
			nameof(JobComInvoiceLine.CusEntryLine) + "+" + nameof(Business.CusEntryLine.CL_LineNumber),
			nameof(JobComInvoiceLine.JI_SequenceNumber),
			nameof(JobComInvoiceLine.JI_PreviousEntryNumber),
			JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
			JobComInvoiceLine.Schema.JI_CEI,
		};
	}
}
