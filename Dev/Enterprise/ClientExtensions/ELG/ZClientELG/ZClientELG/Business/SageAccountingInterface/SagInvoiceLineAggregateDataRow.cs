using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	/// <summary>
	/// Nominal Analysis Band aka Aggregate Invoice Lines.
	/// </summary>
	public class SagInvoiceLineAggregateDataRow : SagFlatFileDataRow
	{
		public SagInvoiceLineAggregateDataRow() : base(SagInvoiceLineAggregateDataRow.Schema.FieldCapacity)
		{
		}

		public static class Schema
		{
			public static readonly FlatFileFieldProperty TransactionValue = new FlatFileFieldProperty(0, 256);
			public static readonly FlatFileFieldProperty AccountNumber = new FlatFileFieldProperty(1, 256);
			public static readonly FlatFileFieldProperty CostCentre = new FlatFileFieldProperty(2, 256);
			public static readonly FlatFileFieldProperty Department = new FlatFileFieldProperty(3, 256);
			public static readonly FlatFileFieldProperty Narrative = new FlatFileFieldProperty(4, 256);
			public static readonly FlatFileFieldProperty AnalysisCode = new FlatFileFieldProperty(5, 256);

			internal const int FieldCapacity = 6;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.TransactionValue);
			FieldProperties.Add(Schema.AccountNumber);
			FieldProperties.Add(Schema.CostCentre);
			FieldProperties.Add(Schema.Department);
			FieldProperties.Add(Schema.Narrative);
			FieldProperties.Add(Schema.AnalysisCode);
		}

		/// <summary>
		/// aka NominalAnalysisTransactionValue
		/// </summary>
		public ZDecimal TransactionValue
		{
			get { return GetFieldAsZDecimal(Schema.TransactionValue, 2); }
			set { SetField(Schema.TransactionValue, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAccountNumber
		/// </summary>
		public ZString AccountNumber
		{
			get { return GetField(Schema.AccountNumber); }
			set { SetField(Schema.AccountNumber, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalCostCentre
		/// </summary>
		public ZString CostCentre
		{
			get { return GetField(Schema.CostCentre); }
			set { SetField(Schema.CostCentre, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalDepartment
		/// </summary>
		public ZString Department
		{
			get { return GetField(Schema.Department); }
			set { SetField(Schema.Department, value); }
		}

		/// <summary>
		/// aka NominalAnalysisNominalAnalysisNarrative
		/// </summary>
		public ZString Narrative
		{
			get { return GetField(Schema.Narrative); }
			set { SetField(Schema.Narrative, value); }
		}

		/// <summary>
		/// aka NominalAnalysisTransactionAnalysisCode
		/// </summary>
		public ZString AnalysisCode
		{
			get { return GetField(Schema.AnalysisCode); }
			set { SetField(Schema.AnalysisCode, value); }
		}

		public static SagInvoiceLineAggregateDataRow operator +(SagInvoiceLineAggregateDataRow value, SagInvoiceLineAggregateDataRow addition)
		{
			value.TransactionValue += addition.TransactionValue;
			return value;
		}
	}
}
