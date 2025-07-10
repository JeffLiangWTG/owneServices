namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLineRow : CATCPRow
	{
		public CATCPLineRow()
			: base(new[]
			{
				Schema.Length.RecordIdentifier,
				Schema.Length.BusinessNumber,
				Schema.Length.TCPTypeCode,
				Schema.Length.TCPIdentifier,
				Schema.Length.AddressLine1,
				Schema.Length.AddressLine2,
				Schema.Length.City,
				Schema.Length.ProvinceStateCode,
				Schema.Length.CountryCode,
				Schema.Length.PostalZipCode,
				Schema.Length.BusinessName
			})
		{
		}

		public static class Schema
		{
			public const int RecordIdentifier = 0;
			public const int BusinessNumber = 1;
			public const int TCPTypeCode = 2;
			public const int TCPIdentifier = 3;
			public const int AddressLine1 = 4;
			public const int AddressLine2 = 5;
			public const int City = 6;
			public const int ProvinceStateCode = 7;
			public const int CountryCode = 8;
			public const int PostalZipCode = 9;
			public const int BusinessName = 10;
			public const int Filler = 11;

			public static class Length
			{
				public const int RecordIdentifier = 2;
				public const int BusinessNumber = 15;
				public const int TCPTypeCode = 2;
				public const int TCPIdentifier = 15;
				public const int AddressLine1 = 30;
				public const int AddressLine2 = 30;
				public const int City = 30;
				public const int ProvinceStateCode = 2;
				public const int CountryCode = 2;
				public const int PostalZipCode = 10;
				public const int BusinessName = 175;
			}
		}
	}
}
