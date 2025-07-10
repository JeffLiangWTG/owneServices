namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPHeaderRow : CATCPRow
	{
		public CATCPHeaderRow()
			: base(new[]
			{
				Schema.Length.RecordIdentifier,
				Schema.Length.BusinessNumber
			})
		{
		}

		public static class Schema
		{
			public const int RecordIdentifier = 0;
			public const int BusinessNumber = 1;

			public static class Length
			{
				public const int RecordIdentifier = 2;
				public const int BusinessNumber = 9;
			}
		}
	}
}
