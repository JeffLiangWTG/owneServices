using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class RoleHelper
	{
		public static class Code
		{
			public const string Production = "PRD";
			public const string ProductionFailover = "PFO";
			public const string Staging = "STG";
		}

		public static CodeDescriptionPair Production =>
			new CodeDescriptionPair(
				Code.Production,
				ResString.GetMultilingualString("AEF0DB98-B61C-40B2-A3FB-7CAAB27D3317", "Production"));

		public static CodeDescriptionPair ProductionFailover =>
			new CodeDescriptionPair(
				Code.ProductionFailover,
				ResString.GetMultilingualString("AEF0EB98-B61C-40B2-A3FB-7CAAB27D3317", "Production Fail-over"));

		public static CodeDescriptionPair Staging =>
			new CodeDescriptionPair(
				Code.Staging,
				ResString.GetMultilingualString("AEF0EB78-B61C-40B2-A3FB-7CAAB27D3317", "Staging"));
	}
}
