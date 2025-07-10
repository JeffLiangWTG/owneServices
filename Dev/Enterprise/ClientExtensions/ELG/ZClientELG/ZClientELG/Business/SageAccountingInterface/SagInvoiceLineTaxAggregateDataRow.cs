using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	/// <summary>
	/// Tax Analysis Band aka Aggregate Invoice Tax Lines.
	/// </summary>
	public class SagInvoiceLineTaxAggregateDataRow : SagFlatFileDataRow
	{
		public SagInvoiceLineTaxAggregateDataRow() : base(SagInvoiceLineTaxAggregateDataRow.Schema.FieldCapacity)
		{
		}

		public static class Schema
		{
			public static readonly FlatFileFieldProperty TaxRateIndicator = new FlatFileFieldProperty(0, 1);
			public static readonly FlatFileFieldProperty GoodsValue = new FlatFileFieldProperty(1, 256);
			public static readonly FlatFileFieldProperty DiscountValue = new FlatFileFieldProperty(2, 256);
			public static readonly FlatFileFieldProperty DiscountPercentage = new FlatFileFieldProperty(3, 256);
			public static readonly FlatFileFieldProperty TaxValue = new FlatFileFieldProperty(4, 256);

			internal const int FieldCapacity = 5;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.TaxRateIndicator);
			FieldProperties.Add(Schema.GoodsValue);
			FieldProperties.Add(Schema.DiscountValue);
			FieldProperties.Add(Schema.DiscountPercentage);
			FieldProperties.Add(Schema.TaxValue);
		}

		/// <summary>
		/// aka TaxAnalysisTaxRate
		/// </summary>
		public ZString TaxRateIndicator
		{
			get { return GetField(Schema.TaxRateIndicator); }
			set { SetField(Schema.TaxRateIndicator, value); }
		}

		/// <summary>
		/// aka TaxAnalysisGoodsValueBeforeDiscount
		/// </summary>
		public ZDecimal GoodsValue
		{
			get { return GetFieldAsZDecimal(Schema.GoodsValue, 2); }
			set { SetField(Schema.GoodsValue, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountValue
		/// </summary>
		public ZDecimal DiscountValue
		{
			get { return GetFieldAsZDecimal(Schema.DiscountValue, 2); }
			set { SetField(Schema.DiscountValue, value); }
		}

		/// <summary>
		/// aka TaxAnalysisDiscountPercentage
		/// </summary>
		public ZDecimal DiscountPercentage
		{
			get { return GetFieldAsZDecimal(Schema.DiscountPercentage, 2); }
			set { SetField(Schema.DiscountPercentage, value); }
		}

		/// <summary>
		/// aka TaxAnalysisTaxOnGoodsValue
		/// </summary>
		public ZDecimal TaxValue
		{
			get { return GetFieldAsZDecimal(Schema.TaxValue, 2); }
			set { SetField(Schema.TaxValue, value); }
		}

		public static SagInvoiceLineTaxAggregateDataRow operator +(SagInvoiceLineTaxAggregateDataRow value, SagInvoiceLineTaxAggregateDataRow addition)
		{
			value.GoodsValue += addition.GoodsValue;
			value.TaxValue += addition.TaxValue;
			return value;
		}
	}
}
