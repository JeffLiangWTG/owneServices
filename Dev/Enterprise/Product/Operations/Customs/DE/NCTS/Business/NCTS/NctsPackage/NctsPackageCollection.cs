using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPackageCollection : NctsPackageCollection<NctsPackage, NctsCommonCargoDesc>
	{
		public NctsPackageCollection(NctsCommonCargoDesc parent)
			: base(parent)
		{
		}

		protected override bool AllowNewCore => Count < 99;
	}
}
