using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP
{
	public static class Constants
	{
		public static class EDIMessageTypes
		{
			public const string UCN = "UCN";
		}

		public static class EDIMessageSubTypes
		{
			public const string SendIslMessage = "SIM";

			public const string GetIslReports = "GIR";

			public const string AckIslReports = "AIR";
		}

		public static class EDIInterchange
		{
			public const string GBCustoms = "GBCustoms";
		}

		public static class ResponseCode
		{
			public const string Success = "0000";

			public const string UnitIdTooLong = "0001";

			public const string InvalidUnitId = "0294";

			public const string AlreadyNominated = "1147";
		}

		public static class ServiceTasksCode
		{
			public const string MCPClaimUCNServiceTaskCode = Messaging.Business.EDIInterchange.ApplicationCodes.GbMcpEdifactOutboundOnly;
			public const string MiscTextAndIslServiceTaskCode = ApplicationCodeList.Codes.GbMiscTextAndIslService;
		}
	}
}
