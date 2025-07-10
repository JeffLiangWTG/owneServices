using System;
using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.GUI
{
	public partial class LocalExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public LocalExportInvoiceLineUserControl()
		{
			InitializeComponent();
			AddColumnsToGrid();
			ChangeColumnsInGrid();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = Constants.ColumnLayoutContextLocalExport;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Splitter.MinSize = BottomPanel.MinimumSize.Height;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ReOrderColumns();
		}

		void AddColumnsToGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.SupportingDocumentCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.SupportingDocumentReferenceNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.JI_InboundDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.JI_OriginalStateDocType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.JI_SerialNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
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
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					GroupName = Res.GetData("LocalExportInvoiceLineUserControl|CDE83046-0053-4EFE-A7F3-C0B6C5BFA7D4", "Packages"),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
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
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceLine.CustomsUnitPrice),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				}
			});

			InvoiceLineCharges.ChargesGrid.AddExchangeRateColumn();
			InvoiceLineCharges.ApportionedChargesGrid.AddExchangeRateColumn();
			InvoiceLineCharges.ApportionedChargesGrid.AddIncludedInInvoiceColumn(ColumnTitleWhenExportForIncludedInInvoiceAmount);
		}

		void ChangeColumnsInGrid()
		{
			InvoiceLineCharges.ChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, ColumnTitleWhenExportForDutiable);
			InvoiceLineCharges.ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, ColumnTitleWhenExportForDutiable);

			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Weight).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Volume).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Percentage).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		}
		void ReOrderColumns()
		{
			InvoiceLineCharges.ChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.LineChargeColumnsInOrder);
			InvoiceLineCharges.ApportionedChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.LineChargeColumnsInOrder);
			CustomsInvoiceLinesBoundGrid.ReOrderColumnsAndChangeVisibility(invLines_LineDetails);
		}

		readonly string[] invLines_LineDetails =
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
			JobComInvoiceLine.Schema.JI_Description,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_NoOfPacks,
			JobComInvoiceLine.Schema.JI_PackType,
			nameof(JobComInvoiceLine.SupportingDocumentCode),
			nameof(JobComInvoiceLine.SupportingDocumentReferenceNumber),
			nameof(JobComInvoiceLine.JI_InboundDate),
			nameof(JobComInvoiceLine.JI_OriginalStateDocType),
			nameof(JobComInvoiceLine.JI_PreviousEntryNumber),
			nameof(JobComInvoiceLine.JI_Ingredient),
			nameof(JobComInvoiceLine.JI_SerialNumber),
			nameof(JobComInvoiceLine.CusEntryLine) + "+" + nameof(Business.CusEntryLine.CL_LineNumber),
		};

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_CIFConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_InsuranceConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_FreightConvertToLocalCurrencyControl.Visible = false;
		}
	}
}
