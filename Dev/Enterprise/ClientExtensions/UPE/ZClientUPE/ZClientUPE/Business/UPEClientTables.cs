
namespace Enterprise.Client.UPE.Business
{
	public static class UPEClientTables
	{
		public abstract class ClientBISIShipmentHeader
		{
			public const string TableName = "ClientBISIShipmentHeader";
			public const string PK = "T8_PK";
			public const string T8_CS = "T8_CS";
			public const string T8_UploadBatchNumber = "T8_UploadBatchNumber";
			public const string T8_ThirdPartyIndicator = "T8_ThirdPartyIndicator";
		}

		public abstract class ClientBISIShipmentCharge
		{
			public const string TableName = "ClientBISIShipmentCharge";
			public const string PK = "T9_PK";
			public const string T9_T8 = "T9_T8";
			public const string T9_ChargeType = "T9_ChargeType";
			public const string T9_GrossAmount = "T9_GrossAmount";
		}
	}
}
