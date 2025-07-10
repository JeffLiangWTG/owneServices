using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.ZA
{
	public partial class ZAMessageStatusList : CodeDescriptionPairList
	{
		public static bool IsError(string code)
		{
			return code == Codes.Error;
		}

		public static bool IsAwaiting(string code)
		{
			return code == Codes.AwaitingResponse;
		}

		public static bool IsAcknowledged(string code)
		{
			return code == Codes.Acknowledged;
		}
	}
}
