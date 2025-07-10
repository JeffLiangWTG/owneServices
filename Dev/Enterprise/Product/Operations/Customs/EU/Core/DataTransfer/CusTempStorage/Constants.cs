using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class Constants
	{
		public static class AddressType
		{
			public const string Declarant = "Declarant";
			public const string SendersLocalClient = "SendersLocalClient";
		}

		public static class AdditionalReference
		{
			public static class EntryType
			{
				public static class Codes
				{
					public const string Registration = "TSR";
				}

				public static class Descriptions
				{
					public static MultilingualString Registration => ResString.GetMultilingualString("CDADBF10-0A30-4C21-B747-DCBD3B17B681", "Temporary Storage Registration");
					public static MultilingualString PreviousReferenceNumber => ResString.GetMultilingualString("85AE59F6-7813-43FF-923A-84D7369928F0", "Previous Reference Number");
				}
			}
		}
	}
}
