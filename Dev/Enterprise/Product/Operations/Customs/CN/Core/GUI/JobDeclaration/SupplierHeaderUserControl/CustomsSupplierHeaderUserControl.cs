using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CustomsSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
	{
		public CustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.JZ_MarksAndNumbersLongTextBox, "Invoices.JZ_MarksAndNumbers");
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutPi3ByCKMr9ZmpQDDAywqWw==";

			ApportionmentPendingLabel.AllowOverlap(JZ_Calc_OFTInInvoiceCurrencyControl);
			ApportionmentPendingLabel.AllowOverlap(JZ_Calc_ONSInInvoiceCurrencyControl);
		}

		protected override void InitializeGridLayoutCore()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("EE49F2B4-97B6-4728-921C-06643B0983FE", "Contracts");
			zTextBoxColumnStyleInfo1.ColumnName = JobComInvoiceHeader.Schema.ContractNumbersAsString;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(ColumnNamesInSortOrder);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, DefaultColumnsForGrid);

			InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleForDutiable);
			InvoiceChargesGrid.SetColumnWidth(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, 80);
			InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGST);
			InvoiceChargesGrid.RefreshTableStyles();

			ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleForDutiable);
			ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGST);
			ApportionedChargesGrid.RefreshTableStyles();

			BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleForDutiable);
			BaseGroupChargesGrid.SetColumnWidth(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, 80);
			BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGST);
			BaseGroupChargesGrid.RefreshTableStyles();
		}

		protected string ColumnTitleForDutiable => Res.GetString("80f01260-0e00-47fb-bd7e-806f1db28526", "Add to FOB?");

		protected string ColumnTitleForGST => Res.GetString("59c1bd57-264c-4e5f-b63a-3541de38970d", "Add to CIF?");

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = new[]
					{
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
						JobComInvoiceHeader.Schema.ContractNumbersAsString,
						JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
						JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer,
						JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
						JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
						BaseJobComInvoiceHeader.Schema.InvoiceLineTotal,
						BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						JobComInvoiceHeaderSchema.Constants.JZ_NoOfPacks,
						BaseJobComInvoiceHeader.Schema.NoOfPacksPackType,
						JobComInvoiceHeaderSchema.Constants.JZ_Weight,
						JobComInvoiceHeaderSchema.Constants.JZ_WeightUQ,
						JobComInvoiceHeaderSchema.Constants.JZ_NetWeight,
						JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ,
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate
					};
				}
				return defaultColumnsForGrid;
			}
		}
		string[] defaultColumnsForGrid;

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);
					columns.Add(BaseJobComInvoiceHeader.Schema.SupplierName);
					columns.Add(BaseJobComInvoiceHeader.Schema.JZ_Calc_ChargesExcludedFromITOT);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate);
					columns.Add(BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
					columns.Add(BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
					columns.Add(BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
					columns.Add(BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_Volume);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentNo);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentAmount);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill);
					columns.Add(BaseJobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDisplaySequence);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace);
					columnNamesInSortOrder = columns.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;
	}
}
