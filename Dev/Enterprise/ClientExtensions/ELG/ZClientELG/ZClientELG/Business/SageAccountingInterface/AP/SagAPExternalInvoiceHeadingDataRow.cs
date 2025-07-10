using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	internal class SagAPExternalInvoiceHeadingDataRow : SagFlatFileDataRow
	{
		public SagAPExternalInvoiceHeadingDataRow() : base(SagAPExternalInvoiceHeadingDataRow.Schema.FieldCapacity)
		{
			this.AccountNumber = "AccountNumber";
			this.DueDate = "DueDate";
			this.GoodsValueInAccountCurrency = "GoodsValueInAccountCurrency";
			this.SaleControlnValueInBaseCurrency = "PerControlValueInBaseCurrency";
			this.DocumentToBaseCurrencyRate = "DocumentToBaseCurrencyRate";
			this.DocumentToAccountCurrencyRate = "DocumentToAccountCurrencyRate";
			this.TransactionReference = "TransactionReference";
			this.SecondReference = "SecondReference";
			this.Source = "Source";
			this.SYSTraderTranType = "SYSTraderTranType";
			this.TransactionDate = "TransactionDate";
			this.TaxValue = "TaxValue";
			this.NominalAnalysisTransactionValue_1 = "NominalAnalysisTransactionValue/1";
			this.NominalAnalysisTransactionValue_2 = "NominalAnalysisTransactionValue/2";
			this.NominalAnalysisTransactionValue_3 = "NominalAnalysisTransactionValue/3";
			this.NominalAnalysisTransactionValue_4 = "NominalAnalysisTransactionValue/4";
			this.NominalAnalysisTransactionValue_5 = "NominalAnalysisTransactionValue/5";
			this.NominalAnalysisTransactionValue_6 = "NominalAnalysisTransactionValue/6";
			this.NominalAnalysisTransactionValue_7 = "NominalAnalysisTransactionValue/7";
			this.NominalAnalysisTransactionValue_8 = "NominalAnalysisTransactionValue/8";
			this.NominalAnalysisTransactionValue_9 = "NominalAnalysisTransactionValue/9";
			this.NominalAnalysisTransactionValue_10 = "NominalAnalysisTransactionValue/10";
			this.NominalAnalysisTransactionValue_11 = "NominalAnalysisTransactionValue/11";
			this.NominalAnalysisTransactionValue_12 = "NominalAnalysisTransactionValue/12";
			this.NominalAnalysisTransactionValue_13 = "NominalAnalysisTransactionValue/13";
			this.NominalAnalysisTransactionValue_14 = "NominalAnalysisTransactionValue/14";
			this.NominalAnalysisTransactionValue_15 = "NominalAnalysisTransactionValue/15";
			this.NominalAnalysisNominalAccountNumber_1 = "NominalAnalysisNominalAccountNumber/1";
			this.NominalAnalysisNominalAccountNumber_2 = "NominalAnalysisNominalAccountNumber/2";
			this.NominalAnalysisNominalAccountNumber_3 = "NominalAnalysisNominalAccountNumber/3";
			this.NominalAnalysisNominalAccountNumber_4 = "NominalAnalysisNominalAccountNumber/4";
			this.NominalAnalysisNominalAccountNumber_5 = "NominalAnalysisNominalAccountNumber/5";
			this.NominalAnalysisNominalAccountNumber_6 = "NominalAnalysisNominalAccountNumber/6";
			this.NominalAnalysisNominalAccountNumber_7 = "NominalAnalysisNominalAccountNumber/7";
			this.NominalAnalysisNominalAccountNumber_8 = "NominalAnalysisNominalAccountNumber/8";
			this.NominalAnalysisNominalAccountNumber_9 = "NominalAnalysisNominalAccountNumber/9";
			this.NominalAnalysisNominalAccountNumber_10 = "NominalAnalysisNominalAccountNumber/10";
			this.NominalAnalysisNominalAccountNumber_11 = "NominalAnalysisNominalAccountNumber/11";
			this.NominalAnalysisNominalAccountNumber_12 = "NominalAnalysisNominalAccountNumber/12";
			this.NominalAnalysisNominalAccountNumber_13 = "NominalAnalysisNominalAccountNumber/13";
			this.NominalAnalysisNominalAccountNumber_14 = "NominalAnalysisNominalAccountNumber/14";
			this.NominalAnalysisNominalAccountNumber_15 = "NominalAnalysisNominalAccountNumber/15";
			this.NominalAnalysisNominalCostCentre_1 = "NominalAnalysisNominalCostCentre/1";
			this.NominalAnalysisNominalCostCentre_2 = "NominalAnalysisNominalCostCentre/2";
			this.NominalAnalysisNominalCostCentre_3 = "NominalAnalysisNominalCostCentre/3";
			this.NominalAnalysisNominalCostCentre_4 = "NominalAnalysisNominalCostCentre/4";
			this.NominalAnalysisNominalCostCentre_5 = "NominalAnalysisNominalCostCentre/5";
			this.NominalAnalysisNominalCostCentre_6 = "NominalAnalysisNominalCostCentre/6";
			this.NominalAnalysisNominalCostCentre_7 = "NominalAnalysisNominalCostCentre/7";
			this.NominalAnalysisNominalCostCentre_8 = "NominalAnalysisNominalCostCentre/8";
			this.NominalAnalysisNominalCostCentre_9 = "NominalAnalysisNominalCostCentre/9";
			this.NominalAnalysisNominalCostCentre_10 = "NominalAnalysisNominalCostCentre/10";
			this.NominalAnalysisNominalCostCentre_11 = "NominalAnalysisNominalCostCentre/11";
			this.NominalAnalysisNominalCostCentre_12 = "NominalAnalysisNominalCostCentre/12";
			this.NominalAnalysisNominalCostCentre_13 = "NominalAnalysisNominalCostCentre/13";
			this.NominalAnalysisNominalCostCentre_14 = "NominalAnalysisNominalCostCentre/14";
			this.NominalAnalysisNominalCostCentre_15 = "NominalAnalysisNominalCostCentre/15";
			this.NominalAnalysisNominalDepartment_1 = "NominalAnalysisNominalDepartment/1";
			this.NominalAnalysisNominalDepartment_2 = "NominalAnalysisNominalDepartment/2";
			this.NominalAnalysisNominalDepartment_3 = "NominalAnalysisNominalDepartment/3";
			this.NominalAnalysisNominalDepartment_4 = "NominalAnalysisNominalDepartment/4";
			this.NominalAnalysisNominalDepartment_5 = "NominalAnalysisNominalDepartment/5";
			this.NominalAnalysisNominalDepartment_6 = "NominalAnalysisNominalDepartment/6";
			this.NominalAnalysisNominalDepartment_7 = "NominalAnalysisNominalDepartment/7";
			this.NominalAnalysisNominalDepartment_8 = "NominalAnalysisNominalDepartment/8";
			this.NominalAnalysisNominalDepartment_9 = "NominalAnalysisNominalDepartment/9";
			this.NominalAnalysisNominalDepartment_10 = "NominalAnalysisNominalDepartment/10";
			this.NominalAnalysisNominalDepartment_11 = "NominalAnalysisNominalDepartment/11";
			this.NominalAnalysisNominalDepartment_12 = "NominalAnalysisNominalDepartment/12";
			this.NominalAnalysisNominalDepartment_13 = "NominalAnalysisNominalDepartment/13";
			this.NominalAnalysisNominalDepartment_14 = "NominalAnalysisNominalDepartment/14";
			this.NominalAnalysisNominalDepartment_15 = "NominalAnalysisNominalDepartment/15";
			this.NominalAnalysisNominalAnalysisNarrative_1 = "NominalAnalysisNominalAnalysisNarrative/1";
			this.NominalAnalysisNominalAnalysisNarrative_2 = "NominalAnalysisNominalAnalysisNarrative/2";
			this.NominalAnalysisNominalAnalysisNarrative_3 = "NominalAnalysisNominalAnalysisNarrative/3";
			this.NominalAnalysisNominalAnalysisNarrative_4 = "NominalAnalysisNominalAnalysisNarrative/4";
			this.NominalAnalysisNominalAnalysisNarrative_5 = "NominalAnalysisNominalAnalysisNarrative/5";
			this.NominalAnalysisNominalAnalysisNarrative_6 = "NominalAnalysisNominalAnalysisNarrative/6";
			this.NominalAnalysisNominalAnalysisNarrative_7 = "NominalAnalysisNominalAnalysisNarrative/7";
			this.NominalAnalysisNominalAnalysisNarrative_8 = "NominalAnalysisNominalAnalysisNarrative/8";
			this.NominalAnalysisNominalAnalysisNarrative_9 = "NominalAnalysisNominalAnalysisNarrative/9";
			this.NominalAnalysisNominalAnalysisNarrative_10 = "NominalAnalysisNominalAnalysisNarrative/10";
			this.NominalAnalysisNominalAnalysisNarrative_11 = "NominalAnalysisNominalAnalysisNarrative/11";
			this.NominalAnalysisNominalAnalysisNarrative_12 = "NominalAnalysisNominalAnalysisNarrative/12";
			this.NominalAnalysisNominalAnalysisNarrative_13 = "NominalAnalysisNominalAnalysisNarrative/13";
			this.NominalAnalysisNominalAnalysisNarrative_14 = "NominalAnalysisNominalAnalysisNarrative/14";
			this.NominalAnalysisNominalAnalysisNarrative_15 = "NominalAnalysisNominalAnalysisNarrative/15";
			this.NominalAnalysisTransactionAnalysisCode_1 = "NominalAnalysisTransactionAnalysisCode/1";
			this.NominalAnalysisTransactionAnalysisCode_2 = "NominalAnalysisTransactionAnalysisCode/2";
			this.NominalAnalysisTransactionAnalysisCode_3 = "NominalAnalysisTransactionAnalysisCode/3";
			this.NominalAnalysisTransactionAnalysisCode_4 = "NominalAnalysisTransactionAnalysisCode/4";
			this.NominalAnalysisTransactionAnalysisCode_5 = "NominalAnalysisTransactionAnalysisCode/5";
			this.NominalAnalysisTransactionAnalysisCode_6 = "NominalAnalysisTransactionAnalysisCode/6";
			this.NominalAnalysisTransactionAnalysisCode_7 = "NominalAnalysisTransactionAnalysisCode/7";
			this.NominalAnalysisTransactionAnalysisCode_8 = "NominalAnalysisTransactionAnalysisCode/8";
			this.NominalAnalysisTransactionAnalysisCode_9 = "NominalAnalysisTransactionAnalysisCode/9";
			this.NominalAnalysisTransactionAnalysisCode_10 = "NominalAnalysisTransactionAnalysisCode/10";
			this.NominalAnalysisTransactionAnalysisCode_11 = "NominalAnalysisTransactionAnalysisCode/11";
			this.NominalAnalysisTransactionAnalysisCode_12 = "NominalAnalysisTransactionAnalysisCode/12";
			this.NominalAnalysisTransactionAnalysisCode_13 = "NominalAnalysisTransactionAnalysisCode/13";
			this.NominalAnalysisTransactionAnalysisCode_14 = "NominalAnalysisTransactionAnalysisCode/14";
			this.NominalAnalysisTransactionAnalysisCode_15 = "NominalAnalysisTransactionAnalysisCode/15";
			this.TaxAnalysisTaxRate_1 = "TaxAnalysisTaxRate/1";
			this.TaxAnalysisGoodsValueBeforeDiscount_1 = "TaxAnalysisGoodsValueBeforeDiscount/1";
			this.TaxAnalysisDiscountValue_1 = "TaxAnalysisDiscountValue/1";
			this.TaxAnalysisDiscountPercentage_1 = "TaxAnalysisDiscountPercentage/1";
			this.TaxAnalysisTaxOnGoodsValue_1 = "TaxAnalysisTaxOnGoodsValue/1";
			this.TaxAnalysisTaxRate_2 = "TaxAnalysisTaxRate/2";
			this.TaxAnalysisGoodsValueBeforeDiscount_2 = "TaxAnalysisGoodsValueBeforeDiscount/2";
			this.TaxAnalysisDiscountValue_2 = "TaxAnalysisDiscountValue/2";
			this.TaxAnalysisDiscountPercentage_2 = "TaxAnalysisDiscountPercentage/2";
			this.TaxAnalysisTaxOnGoodsValue_2 = "TaxAnalysisTaxOnGoodsValue/2";
			this.TaxAnalysisTaxRate_3 = "TaxAnalysisTaxRate/3";
			this.TaxAnalysisGoodsValueBeforeDiscount_3 = "TaxAnalysisGoodsValueBeforeDiscount/3";
			this.TaxAnalysisDiscountValue_3 = "TaxAnalysisDiscountValue/3";
			this.TaxAnalysisDiscountPercentage_3 = "TaxAnalysisDiscountPercentage/3";
			this.TaxAnalysisTaxOnGoodsValue_3 = "TaxAnalysisTaxOnGoodsValue/3";
			this.TaxAnalysisTaxRate_4 = "TaxAnalysisTaxRate/4";
			this.TaxAnalysisGoodsValueBeforeDiscount_4 = "TaxAnalysisGoodsValueBeforeDiscount/4";
			this.TaxAnalysisDiscountValue_4 = "TaxAnalysisDiscountValue/4";
			this.TaxAnalysisDiscountPercentage_4 = "TaxAnalysisDiscountPercentage/4";
			this.TaxAnalysisTaxOnGoodsValue_4 = "TaxAnalysisTaxOnGoodsValue/4";
		}

		public static class Schema
		{
			public static readonly FlatFileFieldProperty AccountNumber = new FlatFileFieldProperty(0, 13);
			public static readonly FlatFileFieldProperty DueDate = new FlatFileFieldProperty(1, 7);
			public static readonly FlatFileFieldProperty GoodsValueInAccountCurrency = new FlatFileFieldProperty(2, 27);
			public static readonly FlatFileFieldProperty SaleControlnValueInBaseCurrency = new FlatFileFieldProperty(3, 31);
			public static readonly FlatFileFieldProperty DocumentToBaseCurrencyRate = new FlatFileFieldProperty(4, 26);
			public static readonly FlatFileFieldProperty DocumentToAccountCurrencyRate = new FlatFileFieldProperty(5, 29);
			public static readonly FlatFileFieldProperty TransactionReference = new FlatFileFieldProperty(6, 20);
			public static readonly FlatFileFieldProperty SecondReference = new FlatFileFieldProperty(7, 15);
			public static readonly FlatFileFieldProperty Source = new FlatFileFieldProperty(8, 6);
			public static readonly FlatFileFieldProperty SYSTraderTranType = new FlatFileFieldProperty(9, 17);
			public static readonly FlatFileFieldProperty TransactionDate = new FlatFileFieldProperty(10, 15);
			public static readonly FlatFileFieldProperty TaxValue = new FlatFileFieldProperty(11, 8);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_1 = new FlatFileFieldProperty(12, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_2 = new FlatFileFieldProperty(13, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_3 = new FlatFileFieldProperty(14, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_4 = new FlatFileFieldProperty(15, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_5 = new FlatFileFieldProperty(16, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_6 = new FlatFileFieldProperty(17, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_7 = new FlatFileFieldProperty(18, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_8 = new FlatFileFieldProperty(19, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_9 = new FlatFileFieldProperty(20, 33);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_10 = new FlatFileFieldProperty(21, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_11 = new FlatFileFieldProperty(22, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_12 = new FlatFileFieldProperty(23, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_13 = new FlatFileFieldProperty(24, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_14 = new FlatFileFieldProperty(25, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_15 = new FlatFileFieldProperty(26, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_1 = new FlatFileFieldProperty(27, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_2 = new FlatFileFieldProperty(28, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_3 = new FlatFileFieldProperty(29, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_4 = new FlatFileFieldProperty(30, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_5 = new FlatFileFieldProperty(31, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_6 = new FlatFileFieldProperty(32, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_7 = new FlatFileFieldProperty(33, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_8 = new FlatFileFieldProperty(34, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_9 = new FlatFileFieldProperty(35, 37);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_10 = new FlatFileFieldProperty(36, 38);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_11 = new FlatFileFieldProperty(37, 38);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_12 = new FlatFileFieldProperty(38, 38);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_13 = new FlatFileFieldProperty(39, 38);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_14 = new FlatFileFieldProperty(40, 38);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_15 = new FlatFileFieldProperty(41, 38);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_1 = new FlatFileFieldProperty(42, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_2 = new FlatFileFieldProperty(43, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_3 = new FlatFileFieldProperty(44, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_4 = new FlatFileFieldProperty(45, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_5 = new FlatFileFieldProperty(46, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_6 = new FlatFileFieldProperty(47, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_7 = new FlatFileFieldProperty(48, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_8 = new FlatFileFieldProperty(49, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_9 = new FlatFileFieldProperty(50, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_10 = new FlatFileFieldProperty(51, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_11 = new FlatFileFieldProperty(52, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_12 = new FlatFileFieldProperty(53, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_13 = new FlatFileFieldProperty(54, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_14 = new FlatFileFieldProperty(55, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_15 = new FlatFileFieldProperty(56, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_1 = new FlatFileFieldProperty(57, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_2 = new FlatFileFieldProperty(58, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_3 = new FlatFileFieldProperty(59, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_4 = new FlatFileFieldProperty(60, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_5 = new FlatFileFieldProperty(61, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_6 = new FlatFileFieldProperty(62, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_7 = new FlatFileFieldProperty(63, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_8 = new FlatFileFieldProperty(64, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_9 = new FlatFileFieldProperty(65, 34);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_10 = new FlatFileFieldProperty(66, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_11 = new FlatFileFieldProperty(67, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_12 = new FlatFileFieldProperty(68, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_13 = new FlatFileFieldProperty(69, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_14 = new FlatFileFieldProperty(70, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_15 = new FlatFileFieldProperty(71, 35);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_1 = new FlatFileFieldProperty(72, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_2 = new FlatFileFieldProperty(73, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_3 = new FlatFileFieldProperty(74, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_4 = new FlatFileFieldProperty(75, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_5 = new FlatFileFieldProperty(76, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_6 = new FlatFileFieldProperty(77, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_7 = new FlatFileFieldProperty(78, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_8 = new FlatFileFieldProperty(79, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_9 = new FlatFileFieldProperty(80, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_10 = new FlatFileFieldProperty(81, 42);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_11 = new FlatFileFieldProperty(82, 42);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_12 = new FlatFileFieldProperty(83, 42);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_13 = new FlatFileFieldProperty(84, 42);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_14 = new FlatFileFieldProperty(85, 42);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_15 = new FlatFileFieldProperty(86, 42);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_1 = new FlatFileFieldProperty(87, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_2 = new FlatFileFieldProperty(88, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_3 = new FlatFileFieldProperty(89, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_4 = new FlatFileFieldProperty(90, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_5 = new FlatFileFieldProperty(91, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_6 = new FlatFileFieldProperty(92, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_7 = new FlatFileFieldProperty(93, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_8 = new FlatFileFieldProperty(94, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_9 = new FlatFileFieldProperty(95, 40);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_10 = new FlatFileFieldProperty(96, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_11 = new FlatFileFieldProperty(97, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_12 = new FlatFileFieldProperty(98, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_13 = new FlatFileFieldProperty(99, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_14 = new FlatFileFieldProperty(100, 41);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_15 = new FlatFileFieldProperty(101, 41);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_1 = new FlatFileFieldProperty(102, 20);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_1 = new FlatFileFieldProperty(103, 37);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_1 = new FlatFileFieldProperty(104, 26);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_1 = new FlatFileFieldProperty(105, 31);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_1 = new FlatFileFieldProperty(106, 28);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_2 = new FlatFileFieldProperty(107, 20);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_2 = new FlatFileFieldProperty(108, 37);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_2 = new FlatFileFieldProperty(109, 26);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_2 = new FlatFileFieldProperty(110, 31);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_2 = new FlatFileFieldProperty(111, 28);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_3 = new FlatFileFieldProperty(112, 20);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_3 = new FlatFileFieldProperty(113, 37);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_3 = new FlatFileFieldProperty(114, 26);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_3 = new FlatFileFieldProperty(115, 31);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_3 = new FlatFileFieldProperty(116, 28);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_4 = new FlatFileFieldProperty(117, 20);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_4 = new FlatFileFieldProperty(118, 37);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_4 = new FlatFileFieldProperty(119, 26);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_4 = new FlatFileFieldProperty(120, 31);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_4 = new FlatFileFieldProperty(121, 28);

			public const int FieldCapacity = 122;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.AccountNumber);
			FieldProperties.Add(Schema.DueDate);
			FieldProperties.Add(Schema.GoodsValueInAccountCurrency);
			FieldProperties.Add(Schema.SaleControlnValueInBaseCurrency);
			FieldProperties.Add(Schema.DocumentToBaseCurrencyRate);
			FieldProperties.Add(Schema.DocumentToAccountCurrencyRate);
			FieldProperties.Add(Schema.TransactionReference);
			FieldProperties.Add(Schema.SecondReference);
			FieldProperties.Add(Schema.Source);
			FieldProperties.Add(Schema.SYSTraderTranType);
			FieldProperties.Add(Schema.TransactionDate);
			FieldProperties.Add(Schema.TaxValue);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_1);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_2);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_3);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_4);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_5);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_6);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_7);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_8);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_9);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_10);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_11);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_12);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_13);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_14);
			FieldProperties.Add(Schema.NominalAnalysisTransactionValue_15);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_1);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_2);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_3);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_4);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_5);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_6);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_7);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_8);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_9);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_10);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_11);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_12);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_13);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_14);
			FieldProperties.Add(Schema.NominalAnalysisNominalAccountNumber_15);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_1);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_2);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_3);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_4);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_5);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_6);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_7);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_8);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_9);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_10);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_11);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_12);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_13);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_14);
			FieldProperties.Add(Schema.NominalAnalysisNominalCostCentre_15);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_1);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_2);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_3);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_4);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_5);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_6);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_7);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_8);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_9);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_10);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_11);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_12);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_13);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_14);
			FieldProperties.Add(Schema.NominalAnalysisNominalDepartment_15);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_1);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_2);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_3);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_4);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_5);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_6);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_7);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_8);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_9);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_10);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_11);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_12);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_13);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_14);
			FieldProperties.Add(Schema.NominalAnalysisNominalAnalysisNarrative_15);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_1);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_2);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_3);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_4);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_5);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_6);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_7);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_8);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_9);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_10);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_11);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_12);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_13);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_14);
			FieldProperties.Add(Schema.NominalAnalysisTransactionAnalysisCode_15);
			FieldProperties.Add(Schema.TaxAnalysisTaxRate_1);
			FieldProperties.Add(Schema.TaxAnalysisGoodsValueBeforeDiscount_1);
			FieldProperties.Add(Schema.TaxAnalysisDiscountValue_1);
			FieldProperties.Add(Schema.TaxAnalysisDiscountPercentage_1);
			FieldProperties.Add(Schema.TaxAnalysisTaxOnGoodsValue_1);
			FieldProperties.Add(Schema.TaxAnalysisTaxRate_2);
			FieldProperties.Add(Schema.TaxAnalysisGoodsValueBeforeDiscount_2);
			FieldProperties.Add(Schema.TaxAnalysisDiscountValue_2);
			FieldProperties.Add(Schema.TaxAnalysisDiscountPercentage_2);
			FieldProperties.Add(Schema.TaxAnalysisTaxOnGoodsValue_2);
			FieldProperties.Add(Schema.TaxAnalysisTaxRate_3);
			FieldProperties.Add(Schema.TaxAnalysisGoodsValueBeforeDiscount_3);
			FieldProperties.Add(Schema.TaxAnalysisDiscountValue_3);
			FieldProperties.Add(Schema.TaxAnalysisDiscountPercentage_3);
			FieldProperties.Add(Schema.TaxAnalysisTaxOnGoodsValue_3);
			FieldProperties.Add(Schema.TaxAnalysisTaxRate_4);
			FieldProperties.Add(Schema.TaxAnalysisGoodsValueBeforeDiscount_4);
			FieldProperties.Add(Schema.TaxAnalysisDiscountValue_4);
			FieldProperties.Add(Schema.TaxAnalysisDiscountPercentage_4);
			FieldProperties.Add(Schema.TaxAnalysisTaxOnGoodsValue_4);
		}

		public ZString AccountNumber
		{
			get { return GetField(Schema.AccountNumber); }
			private set { SetField(Schema.AccountNumber, value); }
		}

		public ZString DueDate
		{
			get { return GetField(Schema.DueDate); }
			private set { SetField(Schema.DueDate, value); }
		}

		public ZString GoodsValueInAccountCurrency
		{
			get { return GetField(Schema.GoodsValueInAccountCurrency); }
			private set { SetField(Schema.GoodsValueInAccountCurrency, value); }
		}

		public ZString SaleControlnValueInBaseCurrency
		{
			get { return GetField(Schema.SaleControlnValueInBaseCurrency); }
			private set { SetField(Schema.SaleControlnValueInBaseCurrency, value); }
		}

		public ZString DocumentToBaseCurrencyRate
		{
			get { return GetField(Schema.DocumentToBaseCurrencyRate); }
			private set { SetField(Schema.DocumentToBaseCurrencyRate, value); }
		}

		public ZString DocumentToAccountCurrencyRate
		{
			get { return GetField(Schema.DocumentToAccountCurrencyRate); }
			private set { SetField(Schema.DocumentToAccountCurrencyRate, value); }
		}

		public ZString TransactionReference
		{
			get { return GetField(Schema.TransactionReference); }
			private set { SetField(Schema.TransactionReference, value); }
		}

		public ZString SecondReference
		{
			get { return GetField(Schema.SecondReference); }
			private set { SetField(Schema.SecondReference, value); }
		}

		public ZString Source
		{
			get { return GetField(Schema.Source); }
			private set { SetField(Schema.Source, value); }
		}

		public ZString SYSTraderTranType
		{
			get { return GetField(Schema.SYSTraderTranType); }
			private set { SetField(Schema.SYSTraderTranType, value); }
		}

		public ZString TransactionDate
		{
			get { return GetField(Schema.TransactionDate); }
			private set { SetField(Schema.TransactionDate, value); }
		}

		public ZString TaxValue
		{
			get { return GetField(Schema.TaxValue); }
			private set { SetField(Schema.TaxValue, value); }
		}

		public ZString NominalAnalysisTransactionValue_1
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_1); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_1, value); }
		}

		public ZString NominalAnalysisTransactionValue_2
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_2); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_2, value); }
		}

		public ZString NominalAnalysisTransactionValue_3
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_3); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_3, value); }
		}

		public ZString NominalAnalysisTransactionValue_4
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_4); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_4, value); }
		}

		public ZString NominalAnalysisTransactionValue_5
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_5); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_5, value); }
		}

		public ZString NominalAnalysisTransactionValue_6
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_6); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_6, value); }
		}

		public ZString NominalAnalysisTransactionValue_7
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_7); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_7, value); }
		}

		public ZString NominalAnalysisTransactionValue_8
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_8); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_8, value); }
		}

		public ZString NominalAnalysisTransactionValue_9
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_9); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_9, value); }
		}

		public ZString NominalAnalysisTransactionValue_10
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_10); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_10, value); }
		}

		public ZString NominalAnalysisTransactionValue_11
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_11); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_11, value); }
		}

		public ZString NominalAnalysisTransactionValue_12
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_12); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_12, value); }
		}

		public ZString NominalAnalysisTransactionValue_13
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_13); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_13, value); }
		}

		public ZString NominalAnalysisTransactionValue_14
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_14); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_14, value); }
		}

		public ZString NominalAnalysisTransactionValue_15
		{
			get { return GetField(Schema.NominalAnalysisTransactionValue_15); }
			private set { SetField(Schema.NominalAnalysisTransactionValue_15, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_1
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_1); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_1, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_2
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_2); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_2, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_3
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_3); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_3, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_4
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_4); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_4, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_5
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_5); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_5, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_6
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_6); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_6, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_7
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_7); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_7, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_8
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_8); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_8, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_9
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_9); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_9, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_10
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_10); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_10, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_11
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_11); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_11, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_12
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_12); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_12, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_13
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_13); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_13, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_14
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_14); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_14, value); }
		}

		public ZString NominalAnalysisNominalAccountNumber_15
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_15); }
			private set { SetField(Schema.NominalAnalysisNominalAccountNumber_15, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_1
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_1); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_1, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_2
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_2); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_2, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_3
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_3); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_3, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_4
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_4); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_4, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_5
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_5); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_5, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_6
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_6); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_6, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_7
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_7); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_7, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_8
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_8); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_8, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_9
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_9); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_9, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_10
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_10); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_10, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_11
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_11); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_11, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_12
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_12); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_12, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_13
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_13); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_13, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_14
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_14); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_14, value); }
		}

		public ZString NominalAnalysisNominalCostCentre_15
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_15); }
			private set { SetField(Schema.NominalAnalysisNominalCostCentre_15, value); }
		}

		public ZString NominalAnalysisNominalDepartment_1
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_1); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_1, value); }
		}

		public ZString NominalAnalysisNominalDepartment_2
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_2); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_2, value); }
		}

		public ZString NominalAnalysisNominalDepartment_3
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_3); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_3, value); }
		}

		public ZString NominalAnalysisNominalDepartment_4
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_4); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_4, value); }
		}

		public ZString NominalAnalysisNominalDepartment_5
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_5); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_5, value); }
		}

		public ZString NominalAnalysisNominalDepartment_6
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_6); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_6, value); }
		}

		public ZString NominalAnalysisNominalDepartment_7
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_7); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_7, value); }
		}

		public ZString NominalAnalysisNominalDepartment_8
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_8); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_8, value); }
		}

		public ZString NominalAnalysisNominalDepartment_9
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_9); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_9, value); }
		}

		public ZString NominalAnalysisNominalDepartment_10
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_10); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_10, value); }
		}

		public ZString NominalAnalysisNominalDepartment_11
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_11); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_11, value); }
		}

		public ZString NominalAnalysisNominalDepartment_12
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_12); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_12, value); }
		}

		public ZString NominalAnalysisNominalDepartment_13
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_13); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_13, value); }
		}

		public ZString NominalAnalysisNominalDepartment_14
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_14); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_14, value); }
		}

		public ZString NominalAnalysisNominalDepartment_15
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_15); }
			private set { SetField(Schema.NominalAnalysisNominalDepartment_15, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_1
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_1); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_1, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_2
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_2); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_2, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_3
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_3); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_3, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_4
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_4); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_4, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_5
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_5); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_5, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_6
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_6); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_6, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_7
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_7); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_7, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_8
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_8); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_8, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_9
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_9); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_9, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_10
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_10); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_10, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_11
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_11); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_11, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_12
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_12); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_12, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_13
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_13); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_13, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_14
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_14); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_14, value); }
		}

		public ZString NominalAnalysisNominalAnalysisNarrative_15
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_15); }
			private set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_15, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_1
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_1); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_1, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_2
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_2); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_2, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_3
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_3); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_3, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_4
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_4); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_4, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_5
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_5); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_5, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_6
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_6); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_6, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_7
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_7); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_7, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_8
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_8); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_8, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_9
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_9); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_9, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_10
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_10); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_10, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_11
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_11); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_11, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_12
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_12); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_12, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_13
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_13); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_13, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_14
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_14); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_14, value); }
		}

		public ZString NominalAnalysisTransactionAnalysisCode_15
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_15); }
			private set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_15, value); }
		}

		public ZString TaxAnalysisTaxRate_1
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_1); }
			private set { SetField(Schema.TaxAnalysisTaxRate_1, value); }
		}

		public ZString TaxAnalysisGoodsValueBeforeDiscount_1
		{
			get { return GetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_1); }
			private set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_1, value); }
		}

		public ZString TaxAnalysisDiscountValue_1
		{
			get { return GetField(Schema.TaxAnalysisDiscountValue_1); }
			private set { SetField(Schema.TaxAnalysisDiscountValue_1, value); }
		}

		public ZString TaxAnalysisDiscountPercentage_1
		{
			get { return GetField(Schema.TaxAnalysisDiscountPercentage_1); }
			private set { SetField(Schema.TaxAnalysisDiscountPercentage_1, value); }
		}

		public ZString TaxAnalysisTaxOnGoodsValue_1
		{
			get { return GetField(Schema.TaxAnalysisTaxOnGoodsValue_1); }
			private set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_1, value); }
		}

		public ZString TaxAnalysisTaxRate_2
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_2); }
			private set { SetField(Schema.TaxAnalysisTaxRate_2, value); }
		}

		public ZString TaxAnalysisGoodsValueBeforeDiscount_2
		{
			get { return GetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_2); }
			private set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_2, value); }
		}

		public ZString TaxAnalysisDiscountValue_2
		{
			get { return GetField(Schema.TaxAnalysisDiscountValue_2); }
			private set { SetField(Schema.TaxAnalysisDiscountValue_2, value); }
		}

		public ZString TaxAnalysisDiscountPercentage_2
		{
			get { return GetField(Schema.TaxAnalysisDiscountPercentage_2); }
			private set { SetField(Schema.TaxAnalysisDiscountPercentage_2, value); }
		}

		public ZString TaxAnalysisTaxOnGoodsValue_2
		{
			get { return GetField(Schema.TaxAnalysisTaxOnGoodsValue_2); }
			private set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_2, value); }
		}

		public ZString TaxAnalysisTaxRate_3
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_3); }
			private set { SetField(Schema.TaxAnalysisTaxRate_3, value); }
		}

		public ZString TaxAnalysisGoodsValueBeforeDiscount_3
		{
			get { return GetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_3); }
			private set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_3, value); }
		}

		public ZString TaxAnalysisDiscountValue_3
		{
			get { return GetField(Schema.TaxAnalysisDiscountValue_3); }
			private set { SetField(Schema.TaxAnalysisDiscountValue_3, value); }
		}

		public ZString TaxAnalysisDiscountPercentage_3
		{
			get { return GetField(Schema.TaxAnalysisDiscountPercentage_3); }
			private set { SetField(Schema.TaxAnalysisDiscountPercentage_3, value); }
		}

		public ZString TaxAnalysisTaxOnGoodsValue_3
		{
			get { return GetField(Schema.TaxAnalysisTaxOnGoodsValue_3); }
			private set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_3, value); }
		}

		public ZString TaxAnalysisTaxRate_4
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_4); }
			private set { SetField(Schema.TaxAnalysisTaxRate_4, value); }
		}

		public ZString TaxAnalysisGoodsValueBeforeDiscount_4
		{
			get { return GetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_4); }
			private set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_4, value); }
		}

		public ZString TaxAnalysisDiscountValue_4
		{
			get { return GetField(Schema.TaxAnalysisDiscountValue_4); }
			private set { SetField(Schema.TaxAnalysisDiscountValue_4, value); }
		}

		public ZString TaxAnalysisDiscountPercentage_4
		{
			get { return GetField(Schema.TaxAnalysisDiscountPercentage_4); }
			private set { SetField(Schema.TaxAnalysisDiscountPercentage_4, value); }
		}

		public ZString TaxAnalysisTaxOnGoodsValue_4
		{
			get { return GetField(Schema.TaxAnalysisTaxOnGoodsValue_4); }
			private set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_4, value); }
		}
	}
}
