using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public abstract class CASSHOTFileDataRow : FlatFileDataRow
	{
		protected CASSHOTFileDataRow(int fieldCount)
			: base(fieldCount)
		{
		}

		public ZString RecordType
		{
			get { return GetField(Schema.RecordType); }
		}

		protected const int NextSchemaFieldNumber = 1;

		public abstract class Schema
		{
			public const int RecordType = 0;
		}
	}
}
