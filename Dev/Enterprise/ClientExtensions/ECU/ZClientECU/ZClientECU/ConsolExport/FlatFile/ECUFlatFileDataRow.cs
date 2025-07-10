
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ECU.ConsolExport
{
	public class ECUFlatFileDataRow : FlatFileDataRow
	{
		public ECUFlatFileDataRow(ZString headerValue, ZString fieldValue) : base(2)
		{
			SetField(Schema.Header, headerValue);
			SetField(Schema.Field, fieldValue);
		}

		public static class Schema
		{
			public const int Header = 0;
			public const int Field = 1;
		}
	}
}
