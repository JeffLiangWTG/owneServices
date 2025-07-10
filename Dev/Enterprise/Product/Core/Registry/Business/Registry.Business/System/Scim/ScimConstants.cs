using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class ScimConstants
	{
		public static class AuthenticationCodes
		{
			public const string AzureEntra = "AZE";
			public const string ApiToken = "APT";
		}

		public static class SafelistTypes
		{
			public const string None = "NON";
			public const string AzureEntra = "AZE";
		}

		public static class AuthenticationDescriptions
		{
			public static string AzureEntra
			{
				get { return (NoResString)"Azure Entra"; }
			}

			public static ResourceString ApiToken
			{
				get { return ResString.GetMultilingualString("A2F857B5-D4CF-429E-8ED5-3E1A89E11DFE", "API Token"); }
			}
		}

		public static class SafelistTypeDescriptions
		{
			public static string None
			{
				get { return ResString.GetMultilingualString("745761DE-CAF7-4925-81E8-C4ABEFA0A604", "None"); }
			}
			public static string AzureEntra
			{
				get { return (NoResString)"Azure Entra"; }
			}
		}
	}
}
