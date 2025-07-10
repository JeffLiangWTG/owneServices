using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.GUI
{
	public partial class LocalExportSupplierHeaderUserControl : LayoutDeclarationInvoiceHeaderUserControl
	{
		public LocalExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = Constants.ColumnLayoutContextLocalExport;

			SEDDetailsDynamicLayoutPanel.UpdateLayout(new LocalExportSEDDetailsLayout());

			JZ_CIFAmountBoundCurrencyControl.Visible = false;
			JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;

			AddHeaderColumns();
			AddChargeColumns();

			RemoveOrChangeExistingColumnsInGrid();

			ReOrderColumns();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var columnTitleForDutiable = ColumnTitleWhenExportForDutiable;
			InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, columnTitleForDutiable);
			BaseGroupChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, columnTitleForDutiable);
			ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, columnTitleForDutiable);
		}

		void RemoveOrChangeExistingColumnsInGrid()
		{
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_NoOfPacks)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OH_Supplier)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFAmount)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFCurrency)));

			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.NoOfPacksPackType)).GroupName = PackagesGroup;

			InvoiceChargesGrid.ColumnStyles.Remove(InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			ApportionedChargesGrid.ColumnStyles.Remove(ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			BaseGroupChargesGrid.ColumnStyles.Remove(BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));

			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.InvoiceLineTotal)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_IncoTerm)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceAmount)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ExporterBankSWIFTCode)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceDate)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceCurrLandedCostExRate)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_PrepaidCollect).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Percentage).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			BaseGroupChargesGrid.GetColumnStyle(BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_PrepaidCollect).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Amount).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("276B6E36-3927-439C-B2DE-8DC6ADEAD7BE", "Amount, Curr.");
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("EE1A080C-FF41-4AE9-B295-001E59EF7BCD", "Amount, Curr.");
			ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Amount).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("90537723-C91A-439B-86B0-8397771B57E6", "Amount, Curr.");
			ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("7F27B30D-76B8-4227-9509-3A1A196F3636", "Amount, Curr.");

			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)).CaptionResourceString = Res.GetData("D3340012-6279-4BFA-A758-C957C50B013A", "Supplier Name");
		}

		void AddChargeColumns()
		{
			InvoiceChargesGrid.AddExchangeRateColumn();
			ApportionedChargesGrid.AddExchangeRateColumn();
			BaseGroupChargesGrid.AddExchangeRateColumn();
		}

		void AddHeaderColumns()
		{
			InvoiceHeadersBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZOrganisationFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OH_Manufacturer),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_DRWApplicantType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_NoOfPacks),
					GroupName = PackagesGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.JZ_InboundDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.SupportingDocumentCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobComInvoiceHeader.SupportingDocumentReferenceNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				}
			});
		}

		void ReOrderColumns()
		{
			InvoiceHeadersBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
			InvoiceChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.HeaderChargeColumnsInOrder);
			ApportionedChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.HeaderChargeColumnsInOrder);
			BaseGroupChargesGrid.ReOrderColumnsAndChangeVisibility(ControlExtensionMethods.GroupChargeColumnsInOrder);
		}

		readonly string[] headerColumns =
		{
				nameof(JobComInvoiceHeader.JZ_InvoiceNumber),
				nameof(JobComInvoiceHeader.JZ_OH_Manufacturer),
				nameof(JobComInvoiceHeader.JZ_OH_Buyer),
				nameof(JobComInvoiceHeader.JZ_DRWApplicantType),
				nameof(JobComInvoiceHeader.JZ_IncoTerm),
				nameof(JobComInvoiceHeader.JZ_InvoiceAmount),
				nameof(JobComInvoiceHeader.JZ_RX_NKInvoice_Currency),
				nameof(JobComInvoiceHeader.JZ_InvoiceCurrExRate),
				nameof(JobComInvoiceHeader.JZ_Calc_BalanceString),
				nameof(JobComInvoiceHeader.InvoiceLineTotal),
				nameof(JobComInvoiceHeader.JZ_Weight),
				nameof(JobComInvoiceHeader.JZ_WeightUQ),
				nameof(JobComInvoiceHeader.JZ_NetWeight),
				nameof(JobComInvoiceHeader.JZ_NetWeightUQ),
				nameof(JobComInvoiceHeader.JZ_NoOfPacks),
				nameof(JobComInvoiceHeader.NoOfPacksPackType),
				nameof(JobComInvoiceHeader.SupportingDocumentCode),
				nameof(JobComInvoiceHeader.SupportingDocumentReferenceNumber),
				nameof(JobComInvoiceHeader.JZ_InboundDate),
		};

		static ResourceStringData PackagesGroup => Res.GetData("D85C0787-7E06-4A4A-B793-DE2C95BA7FBD", "Packages");
	}
}
