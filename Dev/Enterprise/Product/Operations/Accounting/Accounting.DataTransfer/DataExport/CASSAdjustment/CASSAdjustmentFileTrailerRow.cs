using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileTrailerRow : CASSAdjustmentFileDataRow
	{
		public CASSAdjustmentFileTrailerRow()
			: this(NextSchemaFieldNumber)
		{
		}

		protected CASSAdjustmentFileTrailerRow(int fieldCount)
			: base(fieldCount)
		{
			RecordType = CASSAdjustmentFileFormat.RecortID.Trailer;
		}

		public ZInt NumberOfRecords
		{
			get { return ZInt.ParseSafe(DataRow[Schema.NumberOfRecords], ZInt.Zero); }
			set { DataRow[Schema.NumberOfRecords] = value.ToString(); }
		}

		protected new const int NextSchemaFieldNumber = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 1;

		public new abstract class Schema : CASSAdjustmentFileDataRow.Schema
		{
			public const int NumberOfRecords = CASSAdjustmentFileDataRow.NextSchemaFieldNumber;
		}
	}
}
