using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ManifestInformationForTest : IManifestInfo
	{
		public ZInt Quantity { get; set; }
		public ZString UQ { get; set; }
		public ZString GoodsDescription { get; set; }
		public ZString MarksAndNumbers { get; set; }
		public ZString CustomsStatus { get; set; }
		public ZGuid PK { get; set; }
		public string TablePrefix { get; set; }
	}
}
