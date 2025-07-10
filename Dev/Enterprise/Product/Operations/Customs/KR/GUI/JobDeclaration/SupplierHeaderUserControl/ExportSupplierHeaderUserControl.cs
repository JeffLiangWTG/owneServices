using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExportSupplierHeaderUserControl : LayoutDeclarationInvoiceHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(DeclarationType.Export);

			SEDDetailsDynamicLayoutPanel.UpdateLayout(new ExportSEDDetailsLayout());

			JZ_CIFAmountBoundCurrencyControl.Visible = false;
			JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;

			RemoveOrChangeExistingColumnsInGrid();

			AddHeaderColumns();
			AddChargeColumns();
			ReOrderColumns();

			ApportionmentPendingLabel.AllowOverlap(CustomsValueKRW);
			ApportionmentPendingLabel.AllowOverlap(CustomsValueUSD);
		}

		void RemoveOrChangeExistingColumnsInGrid()
		{
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OH_Supplier));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice));
			InvoiceHeadersBoundGrid.ColumnStyles.Remove(InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_NoOfPacks));

			InvoiceHeadersBoundGrid.GetColumnStyle(nameof(JobComInvoiceHeader.NoOfPacksPackType)).GroupName = PackagesGroup;

			InvoiceChargesGrid.ColumnStyles.Remove(InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			ApportionedChargesGrid.ColumnStyles.Remove(ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			BaseGroupChargesGrid.ColumnStyles.Remove(BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));

			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.InvoiceLineTotal).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTerm).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_PrepaidCollect).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Percentage).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			BaseGroupChargesGrid.GetColumnStyle("J7_Calc_IsIncludedInITOT").Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_PrepaidCollect).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			BaseGroupChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Remarks).CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("{B51189AD-36A9-4BB1-8B4E-D04B9E406734", "Declarant Desc.");
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Amount).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("203D9385-7D98-4454-ADB0-FB7184D78BF3", "Amount, Curr.");
			InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("203D9385-7D98-4454-ADB0-FB7184D78BF3", "Amount, Curr.");
			ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_Amount).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("4DB35C67-19D7-4123-A650-4CC0A0ED7D68", "Amount, Curr.");
			ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency).GroupName = Enterprise.Customs.KR.GUI.Res.GetData("4DB35C67-19D7-4123-A650-4CC0A0ED7D68", "Amount, Curr.");
		}

		void AddHeaderColumns()
		{
			InvoiceHeadersBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZOrganisationFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.ManufacturerOrgPK),
					GroupName = ManufacturerGroup,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZAddressDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Normal,
					ColumnName = nameof(JobComInvoiceHeader.JZ_OA_ManufacturerAddress),
					GroupName = ManufacturerGroup,
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
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_PaymentTerms),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.CertificateOfOriginIssueStatus),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_RN_NKDefaultOrigin),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_COOLabelLocation),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceHeader.JZ_ImportCargoManagementNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170),
					IsVisible = false
				}
			});
		}

		void AddChargeColumns()
		{
			InvoiceChargesGrid.AddExchangeRateColumn();
			ApportionedChargesGrid.AddExchangeRateColumn();
			BaseGroupChargesGrid.AddExchangeRateColumn();
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
				nameof(JobComInvoiceHeader.ManufacturerOrgPK),
				nameof(JobComInvoiceHeader.JZ_OA_ManufacturerAddress),
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
				nameof(JobComInvoiceHeader.JZ_PaymentTerms),
				nameof(JobComInvoiceHeader.JZ_LetterOfCreditNumber),
				nameof(JobComInvoiceHeader.JZ_RN_NKDefaultOrigin),
				nameof(JobComInvoiceHeader.CertificateOfOriginIssueStatus),
				nameof(JobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin),
				nameof(JobComInvoiceHeader.JZ_COOLabelLocation),
				nameof(JobComInvoiceHeader.JZ_Remarks),
		};

		static ResourceStringData ManufacturerGroup => Res.GetData("935C179A-292D-488F-930B-6C6ECF83620B", "Manufacturer");
		static ResourceStringData PackagesGroup => Res.GetData("D85C0787-7E06-4A4A-B793-DE2C95BA7FBD", "Packages");
	}
}
