using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportSupplierHeaderUserControl : EUNonLayoutImportSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutLFsI2M1sSfWMZ12NGcJeKw==";// Grid Id
		}

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(PlugIn.AdditionalInfosUserControl);
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(ImportSupplierHeaderSupportingDocumentsUserControl);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			InitializeInvoiceChargesGridLayout();
			InitializeApportionedChargesGridLayout();
			InitializeBaseGroupChargesGridLayout();
			InitializeJobComInvoiceHeadersBoundGrid();
		}

		protected override EU.GUI.CalculateFreightForm GetCalculateFreightForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			var bizObj = EU.Business.Declaration.CalculateFreightBizObj.New(charges, CurrentDataItem);
			if (bizObj != null)
			{
				return new CalculateFreightForm(bizObj);
			}
			return null;
		}

		protected override EU.Business.Declaration.CalculateInsuranceBizObj GetCalculateInsuranceBusinessObject(EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory, EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) =>
			new CalculateInsuranceBizObj(euIncoTermAndChargeFactory, invoiceHeader);

		void InitializeInvoiceChargesGridLayout()
		{
			using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceChargesGrid.ColumnStyles.Add(CreateIATAColumn(InvoiceCharge.Schema.IsJ7_ExchangeRateIATA));
				InvoiceChargesGrid.ColumnStyles.Add(CreateExchangeRateDateColumn(InvoiceCharge.Schema.J7_ExchangeRateDate));
				InvoiceChargesGrid.ReOrderColumns(
					[
						InvoiceCharge.Schema.J7_ChargeType,
						InvoiceCharge.Schema.J7_Amount,
						InvoiceCharge.Schema.J7_RX_NKCurrency,
						InvoiceCharge.Schema.J7_IsDutiable,
						InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
						InvoiceCharge.Schema.J7_IsGSTApplicable,
						InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
						InvoiceCharge.Schema.J7_DistributeBy,
						InvoiceCharge.Schema.IsJ7_ExchangeRateIATA,
						InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						InvoiceCharge.Schema.J7_ExchangeRate,
						InvoiceCharge.Schema.J7_ExchangeRateDate,
						InvoiceCharge.Schema.J7_IsIncludedInITOT
					]);
			}
		}

		void InitializeApportionedChargesGridLayout()
		{
			using (ApportionedChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ApportionedChargesGrid.ColumnStyles.Add(CreateIATAColumn(InvoiceApportionCharge.Schema.IsJ7_ExchangeRateIATA));
				ApportionedChargesGrid.ColumnStyles.Add(CreateExchangeRateDateColumn(InvoiceApportionCharge.Schema.J7_ExchangeRateDate));
				ApportionedChargesGrid.ReOrderColumns(
					[
						InvoiceApportionCharge.Schema.J7_ChargeType,
						InvoiceApportionCharge.Schema.J7_Amount,
						InvoiceApportionCharge.Schema.J7_RX_NKCurrency,
						InvoiceApportionCharge.Schema.J7_IsDutiable,
						InvoiceApportionCharge.Schema.J7_IsStatisticalValueApplicable,
						InvoiceApportionCharge.Schema.J7_IsGSTApplicable,
						InvoiceApportionCharge.Schema.J7_IsIncludedInITOT,
						InvoiceApportionCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
						InvoiceApportionCharge.Schema.IsJ7_ExchangeRateIATA,
						InvoiceApportionCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						InvoiceApportionCharge.Schema.J7_ExchangeRate,
						InvoiceApportionCharge.Schema.J7_ExchangeRateDate
					]);
			}
		}

		void InitializeBaseGroupChargesGridLayout()
		{
			using (BaseGroupChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				BaseGroupChargesGrid.ColumnStyles.Add(CreateIATAColumn(GroupInvoiceCharge.Schema.IsJ7_ExchangeRateIATA));
				BaseGroupChargesGrid.ColumnStyles.Add(CreateExchangeRateDateColumn(GroupInvoiceCharge.Schema.J7_ExchangeRateDate));
				BaseGroupChargesGrid.ReOrderColumns(
					[
						GroupInvoiceCharge.Schema.J7_ChargeType,
						GroupInvoiceCharge.Schema.J7_Amount,
						GroupInvoiceCharge.Schema.J7_RX_NKCurrency,
						GroupInvoiceCharge.Schema.J7_IsDutiable,
						GroupInvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
						GroupInvoiceCharge.Schema.J7_IsGSTApplicable,
						GroupInvoiceCharge.Schema.J7_Percentage,
						GroupInvoiceCharge.Schema.J7_DistributeBy,
						GroupInvoiceCharge.Schema.J7_FullOrPartialApportionment,
						BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
						GroupInvoiceCharge.Schema.IsJ7_ExchangeRateIATA,
						GroupInvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						GroupInvoiceCharge.Schema.J7_ExchangeRate,
						GroupInvoiceCharge.Schema.J7_ExchangeRateDate
					]);
			}
		}

		void InitializeJobComInvoiceHeadersBoundGrid()
		{
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(true, [JobComInvoiceHeader.Schema.BuyerOrgPK, JobComInvoiceHeader.Schema.JZ_OA_BuyerAddress]);
		}

		ZCheckBoxColumnStyleInfo CreateIATAColumn(string columnName)
		{
			return new ZCheckBoxColumnStyleInfo
			{
				ColumnName = columnName,
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47)
			};
		}

		ZDateEditColumnStyleInfo CreateExchangeRateDateColumn(string columnName)
		{
			return new ZDateEditColumnStyleInfo()
			{
				ColumnName = columnName,
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121),
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
			};
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				InitTabsVisibility();
			}
		}

		void InitTabsVisibility()
		{
			var jobDeclaration = JobDeclaration as JobDeclaration;
			var isInwardProcessingAVABR = jobDeclaration?.IsInwardProcessingAVABR ?? false;
			SupportingDocumentsTabPage.TabVisible = !isInwardProcessingAVABR;
			CustomFieldsTabPage.TabVisible = !isInwardProcessingAVABR;
		}

		protected override void UnHookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			base.UnHookDeclarationEventsCore(declaration);
			declaration.CustomsEntryInstructions.ListChanged -= CustomsEntryInstructions_ListChanged;
		}

		protected override void HookDeclarationEventsCore(BaseJobDeclaration declaration)
		{
			declaration.CustomsEntryInstructions.ListChanged += CustomsEntryInstructions_ListChanged;
			base.HookDeclarationEventsCore(declaration);
		}

		void CustomsEntryInstructions_ListChanged(object sender, EventArgs e)
		{
			InitTabsVisibility();
		}
	}
}
