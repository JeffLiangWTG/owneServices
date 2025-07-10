using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackageLookups : Customs.Business.CusDecHouseContainerPackLookups
	{
		public PackageLookups(Package parent)
			: base(parent)
		{
		}

		public UNDGSubstanceCollection DangerousGoods
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}
	}
}
