using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	public static class AvalaraConstants
	{
		public static class IntegrationStatus
		{
			public static class Codes
			{
				public const string Off = "OFF";
				public const string Sandbox = "SND";
				public const string Production = "PRD";
			}
			public static class Descriptions
			{
				public const string Off = "Off";
				public const string Sandbox = "Sandbox";
				public const string Production = "Production";
			}

			public static CodeDescriptionPairList List()
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.Off,        Descriptions.Off);
				result.AddPair(Codes.Sandbox,    Descriptions.Sandbox);
				result.AddPair(Codes.Production, Descriptions.Production);
				return result;
			}
		}

		public const string SandboxBaseUrl    = "https://sandbox-rest.avatax.com/api/v2/";
		public const string ProductionBaseUrl = "https://rest.avatax.com/api/v2/";
	}
}
