using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileHeaderRow : CASSAdjustmentFileDataRow
	{
		public CASSAdjustmentFileHeaderRow()
			: this(NextSchemaFieldNumber)
		{
		}

		protected CASSAdjustmentFileHeaderRow(int fieldCount)
			: base(fieldCount)
		{
			RecordType = CASSAdjustmentFileFormat.RecortID.Header;
		}

		public ZString Agent
		{
			get { return DataRow[Schema.Agent]; }
			set { DataRow[Schema.Agent] = value; }
		}

		public ZInt InvoicePeriod
		{
			get { return ZInt.ParseSafe(DataRow[Schema.InvoicePeriod], ZInt.Zero); }
			set { DataRow[Schema.InvoicePeriod] = value.ToString(); }
		}

		protected new const int NextSchemaFieldNumber = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 4;

		public new abstract class Schema : CASSAdjustmentFileDataRow.Schema
		{
			public const int Agent = CASSAdjustmentFileDataRow.NextSchemaFieldNumber;
			public const int InvoicePeriod = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 1;
			public const int Filler = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 2;
		}
	}
}
