using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	internal class SagAPExternalInvoiceDataRow : SagFlatFileDataRow
	{
		public SagAPExternalInvoiceDataRow() : base(SagAPExternalInvoiceDataRow.Schema.FieldCapacity)
		{
		}

		public static class Schema
		{
			public static readonly FlatFileFieldProperty AccountNumber = new FlatFileFieldProperty(0, 256);
			public static readonly FlatFileFieldProperty DueDate = new FlatFileFieldProperty(1, 10);
			public static readonly FlatFileFieldProperty GoodsValueInAccountCurrency = new FlatFileFieldProperty(2, 256);
			public static readonly FlatFileFieldProperty PerControlValueInBaseCurrency = new FlatFileFieldProperty(3, 256);
			public static readonly FlatFileFieldProperty DocumentToBaseCurrencyRate = new FlatFileFieldProperty(4, 256);
			public static readonly FlatFileFieldProperty DocumentToAccountCurrencyRate = new FlatFileFieldProperty(5, 256);
			public static readonly FlatFileFieldProperty TransactionReference = new FlatFileFieldProperty(6, 256);
			public static readonly FlatFileFieldProperty SecondReference = new FlatFileFieldProperty(7, 256);
			public static readonly FlatFileFieldProperty Source = new FlatFileFieldProperty(8, 1);
			public static readonly FlatFileFieldProperty SYSTraderTranType = new FlatFileFieldProperty(9, 1);
			public static readonly FlatFileFieldProperty TransactionDate = new FlatFileFieldProperty(10, 10);
			public static readonly FlatFileFieldProperty TaxValue = new FlatFileFieldProperty(11, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_1 = new FlatFileFieldProperty(12, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_2 = new FlatFileFieldProperty(13, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_3 = new FlatFileFieldProperty(14, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_4 = new FlatFileFieldProperty(15, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_5 = new FlatFileFieldProperty(16, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_6 = new FlatFileFieldProperty(17, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_7 = new FlatFileFieldProperty(18, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_8 = new FlatFileFieldProperty(19, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_9 = new FlatFileFieldProperty(20, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_10 = new FlatFileFieldProperty(21, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_11 = new FlatFileFieldProperty(22, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_12 = new FlatFileFieldProperty(23, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_13 = new FlatFileFieldProperty(24, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_14 = new FlatFileFieldProperty(25, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionValue_15 = new FlatFileFieldProperty(26, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_1 = new FlatFileFieldProperty(27, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_2 = new FlatFileFieldProperty(28, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_3 = new FlatFileFieldProperty(29, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_4 = new FlatFileFieldProperty(30, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_5 = new FlatFileFieldProperty(31, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_6 = new FlatFileFieldProperty(32, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_7 = new FlatFileFieldProperty(33, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_8 = new FlatFileFieldProperty(34, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_9 = new FlatFileFieldProperty(35, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_10 = new FlatFileFieldProperty(36, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_11 = new FlatFileFieldProperty(37, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_12 = new FlatFileFieldProperty(38, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_13 = new FlatFileFieldProperty(39, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_14 = new FlatFileFieldProperty(40, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAccountNumber_15 = new FlatFileFieldProperty(41, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_1 = new FlatFileFieldProperty(42, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_2 = new FlatFileFieldProperty(43, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_3 = new FlatFileFieldProperty(44, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_4 = new FlatFileFieldProperty(45, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_5 = new FlatFileFieldProperty(46, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_6 = new FlatFileFieldProperty(47, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_7 = new FlatFileFieldProperty(48, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_8 = new FlatFileFieldProperty(49, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_9 = new FlatFileFieldProperty(50, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_10 = new FlatFileFieldProperty(51, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_11 = new FlatFileFieldProperty(52, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_12 = new FlatFileFieldProperty(53, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_13 = new FlatFileFieldProperty(54, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_14 = new FlatFileFieldProperty(55, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalCostCentre_15 = new FlatFileFieldProperty(56, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_1 = new FlatFileFieldProperty(57, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_2 = new FlatFileFieldProperty(58, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_3 = new FlatFileFieldProperty(59, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_4 = new FlatFileFieldProperty(60, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_5 = new FlatFileFieldProperty(61, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_6 = new FlatFileFieldProperty(62, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_7 = new FlatFileFieldProperty(63, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_8 = new FlatFileFieldProperty(64, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_9 = new FlatFileFieldProperty(65, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_10 = new FlatFileFieldProperty(66, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_11 = new FlatFileFieldProperty(67, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_12 = new FlatFileFieldProperty(68, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_13 = new FlatFileFieldProperty(69, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_14 = new FlatFileFieldProperty(70, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalDepartment_15 = new FlatFileFieldProperty(71, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_1 = new FlatFileFieldProperty(72, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_2 = new FlatFileFieldProperty(73, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_3 = new FlatFileFieldProperty(74, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_4 = new FlatFileFieldProperty(75, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_5 = new FlatFileFieldProperty(76, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_6 = new FlatFileFieldProperty(77, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_7 = new FlatFileFieldProperty(78, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_8 = new FlatFileFieldProperty(79, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_9 = new FlatFileFieldProperty(80, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_10 = new FlatFileFieldProperty(81, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_11 = new FlatFileFieldProperty(82, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_12 = new FlatFileFieldProperty(83, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_13 = new FlatFileFieldProperty(84, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_14 = new FlatFileFieldProperty(85, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisNominalAnalysisNarrative_15 = new FlatFileFieldProperty(86, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_1 = new FlatFileFieldProperty(87, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_2 = new FlatFileFieldProperty(88, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_3 = new FlatFileFieldProperty(89, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_4 = new FlatFileFieldProperty(90, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_5 = new FlatFileFieldProperty(91, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_6 = new FlatFileFieldProperty(92, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_7 = new FlatFileFieldProperty(93, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_8 = new FlatFileFieldProperty(94, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_9 = new FlatFileFieldProperty(95, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_10 = new FlatFileFieldProperty(96, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_11 = new FlatFileFieldProperty(97, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_12 = new FlatFileFieldProperty(98, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_13 = new FlatFileFieldProperty(99, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_14 = new FlatFileFieldProperty(100, 256);
			public static readonly FlatFileFieldProperty NominalAnalysisTransactionAnalysisCode_15 = new FlatFileFieldProperty(101, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_1 = new FlatFileFieldProperty(102, 1);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_1 = new FlatFileFieldProperty(103, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_1 = new FlatFileFieldProperty(104, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_1 = new FlatFileFieldProperty(105, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_1 = new FlatFileFieldProperty(106, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_2 = new FlatFileFieldProperty(107, 1);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_2 = new FlatFileFieldProperty(108, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_2 = new FlatFileFieldProperty(109, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_2 = new FlatFileFieldProperty(110, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_2 = new FlatFileFieldProperty(111, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_3 = new FlatFileFieldProperty(112, 1);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_3 = new FlatFileFieldProperty(113, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_3 = new FlatFileFieldProperty(114, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_3 = new FlatFileFieldProperty(115, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_3 = new FlatFileFieldProperty(116, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxRate_4 = new FlatFileFieldProperty(117, 1);
			public static readonly FlatFileFieldProperty TaxAnalysisGoodsValueBeforeDiscount_4 = new FlatFileFieldProperty(118, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountValue_4 = new FlatFileFieldProperty(119, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisDiscountPercentage_4 = new FlatFileFieldProperty(120, 256);
			public static readonly FlatFileFieldProperty TaxAnalysisTaxOnGoodsValue_4 = new FlatFileFieldProperty(121, 256);

			internal const int FieldCapacity = 122;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.AccountNumber);
			FieldProperties.Add(Schema.DueDate);
			FieldProperties.Add(Schema.GoodsValueInAccountCurrency);
			FieldProperties.Add(Schema.PerControlValueInBaseCurrency);
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

		/// <summary>
		/// aka AccountNumber
		/// </summary>
		public ZString AccountNumber
		{
			get { return GetField(Schema.AccountNumber); }
			set { SetField(Schema.AccountNumber, value); }
		}

		/// <summary>
		/// aka DueDate
		/// </summary>
		public ZDateTime DueDate
		{
			get { return GetFieldAsZDateTime(Schema.DueDate, ELGConstants.DataDateFormat); }
			set { SetField(Schema.DueDate, value); }
		}

		/// <summary>
		/// aka GoodsValueInAccountCurrency
		/// </summary>
		public ZDecimal GoodsValueInAccountCurrency
		{
			get { return GetFieldAsZDecimal(Schema.GoodsValueInAccountCurrency, 2); }
			set { SetField(Schema.GoodsValueInAccountCurrency, value); }
		}

		/// <summary>
		/// aka PerControlValueInBaseCurrency
		/// </summary>
		public ZDecimal PerControlValueInBaseCurrency
		{
			get { return GetFieldAsZDecimal(Schema.PerControlValueInBaseCurrency, 2); }
			set { SetField(Schema.PerControlValueInBaseCurrency, value); }
		}

		/// <summary>
		/// aka DocumentToBaseCurrencyRate
		/// </summary>
		public ZDecimal DocumentToBaseCurrencyRate
		{
			get { return GetFieldAsZDecimal(Schema.DocumentToBaseCurrencyRate, 6); }
			set { SetField(Schema.DocumentToBaseCurrencyRate, value, 6); }
		}

		/// <summary>
		/// aka DocumentToAccountCurrencyRate
		/// </summary>
		public ZDecimal DocumentToAccountCurrencyRate
		{
			get { return GetFieldAsZDecimal(Schema.DocumentToAccountCurrencyRate, 6); }
			set { SetField(Schema.DocumentToAccountCurrencyRate, value, 6); }
		}

		/// <summary>
		/// aka TransactionReference
		/// </summary>
		public ZString TransactionReference
		{
			get { return GetField(Schema.TransactionReference); }
			set { SetField(Schema.TransactionReference, value); }
		}

		/// <summary>
		/// aka SecondReference
		/// </summary>
		public ZString SecondReference
		{
			get { return GetField(Schema.SecondReference); }
			set { SetField(Schema.SecondReference, value); }
		}

		/// <summary>
		/// aka Source
		/// </summary>
		public ZInt Source
		{
			get { return GetFieldAsZInt(Schema.Source); }
			set { SetField(Schema.Source, value); }
		}

		/// <summary>
		/// aka SYSTraderTranType
		/// </summary>
		public ZInt SYSTraderTranType
		{
			get { return GetFieldAsZInt(Schema.SYSTraderTranType); }
			set { SetField(Schema.SYSTraderTranType, value); }
		}

		/// <summary>
		/// aka TransactionDate
		/// </summary>
		public ZDateTime TransactionDate
		{
			get { return GetFieldAsZDateTime(Schema.TransactionDate, ELGConstants.DataDateFormat); }
			set { SetField(Schema.TransactionDate, value); }
		}

		/// <summary>
		/// aka TaxValue
		/// </summary>
		public ZDecimal TaxValue
		{
			get { return GetFieldAsZDecimal(Schema.TaxValue, 2); }
			set { SetField(Schema.TaxValue, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/1
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_1
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_1, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_1, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/2
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_2
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_2, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_2, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/3
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_3
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_3, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_3, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/4
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_4
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_4, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_4, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/5
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_5
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_5, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_5, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/6
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_6
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_6, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_6, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/7
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_7
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_7, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_7, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/8
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_8
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_8, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_8, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/9
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_9
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_9, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_9, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/10
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_10
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_10, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_10, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/11
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_11
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_11, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_11, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/12
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_12
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_12, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_12, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/13
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_13
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_13, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_13, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/14
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_14
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_14, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_14, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue/15
		/// </summary>
		public ZDecimal NominalAnalysisTransactionValue_15
		{
			get { return GetFieldAsZDecimal(Schema.NominalAnalysisTransactionValue_15, 2); }
			set { SetField(Schema.NominalAnalysisTransactionValue_15, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/1
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_1
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_1); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_1, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/2
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_2
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_2); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_2, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/3
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_3
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_3); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_3, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/4
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_4
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_4); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_4, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/5
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_5
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_5); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_5, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/6
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_6
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_6); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_6, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/7
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_7
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_7); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_7, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/8
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_8
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_8); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_8, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/9
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_9
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_9); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_9, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/10
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_10
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_10); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_10, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/11
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_11
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_11); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_11, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/12
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_12
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_12); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_12, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/13
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_13
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_13); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_13, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/14
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_14
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_14); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_14, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber/15
		/// </summary>
		public ZString NominalAnalysisNominalAccountNumber_15
		{
			get { return GetField(Schema.NominalAnalysisNominalAccountNumber_15); }
			set { SetField(Schema.NominalAnalysisNominalAccountNumber_15, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/1
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_1
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_1); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_1, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/2
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_2
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_2); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_2, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/3
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_3
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_3); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_3, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/4
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_4
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_4); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_4, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/5
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_5
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_5); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_5, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/6
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_6
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_6); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_6, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/7
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_7
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_7); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_7, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/8
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_8
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_8); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_8, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/9
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_9
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_9); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_9, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/10
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_10
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_10); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_10, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/11
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_11
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_11); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_11, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/12
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_12
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_12); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_12, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/13
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_13
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_13); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_13, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/14
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_14
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_14); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_14, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre/15
		/// </summary>
		public ZString NominalAnalysisNominalCostCentre_15
		{
			get { return GetField(Schema.NominalAnalysisNominalCostCentre_15); }
			set { SetField(Schema.NominalAnalysisNominalCostCentre_15, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/1
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_1
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_1); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_1, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/2
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_2
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_2); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_2, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/3
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_3
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_3); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_3, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/4
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_4
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_4); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_4, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/5
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_5
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_5); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_5, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/6
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_6
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_6); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_6, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/7
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_7
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_7); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_7, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/8
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_8
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_8); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_8, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/9
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_9
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_9); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_9, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/10
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_10
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_10); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_10, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/11
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_11
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_11); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_11, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/12
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_12
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_12); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_12, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/13
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_13
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_13); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_13, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/14
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_14
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_14); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_14, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment/15
		/// </summary>
		public ZString NominalAnalysisNominalDepartment_15
		{
			get { return GetField(Schema.NominalAnalysisNominalDepartment_15); }
			set { SetField(Schema.NominalAnalysisNominalDepartment_15, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/1
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_1
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_1); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_1, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/2
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_2
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_2); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_2, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/3
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_3
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_3); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_3, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/4
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_4
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_4); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_4, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/5
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_5
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_5); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_5, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/6
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_6
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_6); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_6, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/7
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_7
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_7); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_7, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/8
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_8
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_8); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_8, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/9
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_9
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_9); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_9, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/10
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_10
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_10); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_10, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/11
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_11
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_11); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_11, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/12
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_12
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_12); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_12, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/13
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_13
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_13); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_13, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/14
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_14
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_14); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_14, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative/15
		/// </summary>
		public ZString NominalAnalysisNominalAnalysisNarrative_15
		{
			get { return GetField(Schema.NominalAnalysisNominalAnalysisNarrative_15); }
			set { SetField(Schema.NominalAnalysisNominalAnalysisNarrative_15, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/1
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_1
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_1); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_1, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/2
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_2
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_2); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_2, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/3
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_3
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_3); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_3, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/4
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_4
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_4); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_4, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/5
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_5
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_5); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_5, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/6
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_6
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_6); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_6, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/7
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_7
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_7); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_7, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/8
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_8
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_8); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_8, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/9
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_9
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_9); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_9, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/10
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_10
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_10); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_10, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/11
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_11
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_11); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_11, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/12
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_12
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_12); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_12, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/13
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_13
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_13); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_13, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/14
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_14
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_14); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_14, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode/15
		/// </summary>
		public ZString NominalAnalysisTransactionAnalysisCode_15
		{
			get { return GetField(Schema.NominalAnalysisTransactionAnalysisCode_15); }
			set { SetField(Schema.NominalAnalysisTransactionAnalysisCode_15, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxRate/1
		/// </summary>
		public ZString TaxAnalysisTaxRate_1
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_1); }
			set { SetField(Schema.TaxAnalysisTaxRate_1, value); }
		}

		/// <summary>
		/// aka TaxAnalysisGoodsValueBeforeDiscount/1
		/// </summary>
		public ZDecimal TaxAnalysisGoodsValueBeforeDiscount_1
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisGoodsValueBeforeDiscount_1, 2); }
			set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_1, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountValue/1
		/// </summary>
		public ZDecimal TaxAnalysisDiscountValue_1
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountValue_1, 2); }
			set { SetField(Schema.TaxAnalysisDiscountValue_1, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountPercentage/1
		/// </summary>
		public ZDecimal TaxAnalysisDiscountPercentage_1
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountPercentage_1, 2); }
			set { SetField(Schema.TaxAnalysisDiscountPercentage_1, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxOnGoodsValue/1
		/// </summary>
		public ZDecimal TaxAnalysisTaxOnGoodsValue_1
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisTaxOnGoodsValue_1, 2); }
			set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_1, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxRate/2
		/// </summary>
		public ZString TaxAnalysisTaxRate_2
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_2); }
			set { SetField(Schema.TaxAnalysisTaxRate_2, value); }
		}

		/// <summary>
		/// aka TaxAnalysisGoodsValueBeforeDiscount/2
		/// </summary>
		public ZDecimal TaxAnalysisGoodsValueBeforeDiscount_2
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisGoodsValueBeforeDiscount_2, 2); }
			set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_2, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountValue/2
		/// </summary>
		public ZDecimal TaxAnalysisDiscountValue_2
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountValue_2, 2); }
			set { SetField(Schema.TaxAnalysisDiscountValue_2, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountPercentage/2
		/// </summary>
		public ZDecimal TaxAnalysisDiscountPercentage_2
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountPercentage_2, 2); }
			set { SetField(Schema.TaxAnalysisDiscountPercentage_2, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxOnGoodsValue/2
		/// </summary>
		public ZDecimal TaxAnalysisTaxOnGoodsValue_2
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisTaxOnGoodsValue_2, 2); }
			set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_2, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxRate/3
		/// </summary>
		public ZString TaxAnalysisTaxRate_3
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_3); }
			set { SetField(Schema.TaxAnalysisTaxRate_3, value); }
		}

		/// <summary>
		/// aka TaxAnalysisGoodsValueBeforeDiscount/3
		/// </summary>
		public ZDecimal TaxAnalysisGoodsValueBeforeDiscount_3
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisGoodsValueBeforeDiscount_3, 2); }
			set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_3, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountValue/3
		/// </summary>
		public ZDecimal TaxAnalysisDiscountValue_3
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountValue_3, 2); }
			set { SetField(Schema.TaxAnalysisDiscountValue_3, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountPercentage/3
		/// </summary>
		public ZDecimal TaxAnalysisDiscountPercentage_3
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountPercentage_3, 2); }
			set { SetField(Schema.TaxAnalysisDiscountPercentage_3, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxOnGoodsValue/3
		/// </summary>
		public ZDecimal TaxAnalysisTaxOnGoodsValue_3
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisTaxOnGoodsValue_3, 2); }
			set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_3, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxRate/4
		/// </summary>
		public ZString TaxAnalysisTaxRate_4
		{
			get { return GetField(Schema.TaxAnalysisTaxRate_4); }
			set { SetField(Schema.TaxAnalysisTaxRate_4, value); }
		}

		/// <summary>
		/// aka TaxAnalysisGoodsValueBeforeDiscount/4
		/// </summary>
		public ZDecimal TaxAnalysisGoodsValueBeforeDiscount_4
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisGoodsValueBeforeDiscount_4, 2); }
			set { SetField(Schema.TaxAnalysisGoodsValueBeforeDiscount_4, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountValue/4
		/// </summary>
		public ZDecimal TaxAnalysisDiscountValue_4
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountValue_4, 2); }
			set { SetField(Schema.TaxAnalysisDiscountValue_4, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountPercentage/4
		/// </summary>
		public ZDecimal TaxAnalysisDiscountPercentage_4
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisDiscountPercentage_4, 2); }
			set { SetField(Schema.TaxAnalysisDiscountPercentage_4, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxOnGoodsValue/4
		/// </summary>
		public ZDecimal TaxAnalysisTaxOnGoodsValue_4
		{
			get { return GetFieldAsZDecimal(Schema.TaxAnalysisTaxOnGoodsValue_4, 2); }
			set { SetField(Schema.TaxAnalysisTaxOnGoodsValue_4, value); }
		}
	}
}
