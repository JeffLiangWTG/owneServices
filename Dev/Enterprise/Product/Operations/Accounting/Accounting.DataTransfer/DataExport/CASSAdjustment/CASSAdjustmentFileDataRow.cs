using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public abstract class CASSAdjustmentFileDataRow : FlatFileDataRow
	{
		protected CASSAdjustmentFileDataRow(int fieldCount)
			: base(fieldCount)
		{
		}

		public ZString RecordType
		{
			get { return DataRow[Schema.RecordType]; }
			set { DataRow[Schema.RecordType] = value; }
		}

		protected const int NextSchemaFieldNumber = 1;

		public abstract class Schema
		{
			public const int RecordType = 0;
		}
	}
}
