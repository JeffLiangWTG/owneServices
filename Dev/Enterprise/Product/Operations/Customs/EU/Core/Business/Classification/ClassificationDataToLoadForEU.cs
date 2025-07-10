using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	internal class ClassificationDataToLoadForEU : ClassificationDataToLoad
	{
		public ClassificationDataToLoadForEU() : base() { }
		public ZString CPC { get; set; }
		public ZString ECSUPPLEMENT1 { get; set; }
		public ZString ECSUPPLEMENT2 { get; set; }
	}
}
