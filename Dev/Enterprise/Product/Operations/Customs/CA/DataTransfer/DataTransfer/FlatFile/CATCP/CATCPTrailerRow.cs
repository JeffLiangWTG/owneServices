using CargoWise.Types;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPTrailerRow : CATCPRow
	{
		public CATCPTrailerRow()
			: base(new[] { Schema.Length.RecordIdentifier, Schema.Length.NumberOfRecords })
		{
		}

		public static class Schema
		{
			public const int RecordIdentifier = 0;
			public const int NumberOfRecords = 1;
			public static class Length
			{
				public const int RecordIdentifier = 2;
				public const int NumberOfRecords = 9;
			}
		}

		protected override void SetFieldCore(int position, ZString value)
		{
			if (position == Schema.NumberOfRecords)
			{
				value = value.PadLeft(GetFieldLength(position), '0');
			}
			base.SetFieldCore(position, value);
		}
	}
}
