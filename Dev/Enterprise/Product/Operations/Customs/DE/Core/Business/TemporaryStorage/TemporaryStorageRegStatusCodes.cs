using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public static class TemporaryStorageRegStatusCodes
	{
		public static class Code
		{
			public const string NCM = "NCM";
		}

		public static class Description
		{
			public static MultilingualString NCM { get { return ResString.GetMultilingualString("A9BD1F58-B96D-42D9-8B95-91A2453C7E39", "Not Complete"); } }
		}
	}
}
