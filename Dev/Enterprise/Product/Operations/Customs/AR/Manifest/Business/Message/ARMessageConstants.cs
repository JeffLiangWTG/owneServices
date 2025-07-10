using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public static class ARMessageConstants
	{
		public const string MessageServiceTaskCategory = "ARC";

		public const string ARCustomsSeaMode = "ARCustomsSeaMode";

		public const string EmptyContainerCondition = "V";

		public const string NotEmptyContainerCondition = "P";

		public const string BooleanTrueString = "S";

		public const string CountryMapType = "CNTRY";

		public const string ARCustomsAirMode = "ARCustomsAirMode";

		internal const string HBL = "HBL";

		internal static MultilingualString MessageCreateFailure => ResString.GetMultilingualString("D9BD1085-4EF5-420F-8CC8-97E91ACA41C1", "Failed to create message");

		internal static MultilingualString MessageSendFailure => ResString.GetMultilingualString("87E4AC15-5CEA-408D-9520-46CFB0A809DD", "Failed to send message");

		internal static MultilingualString MessageSendSuccessful => ResString.GetMultilingualString("CAAA71A0-6020-4879-8862-4E47F4172BEC", "Message sent successfully");
	}
}
