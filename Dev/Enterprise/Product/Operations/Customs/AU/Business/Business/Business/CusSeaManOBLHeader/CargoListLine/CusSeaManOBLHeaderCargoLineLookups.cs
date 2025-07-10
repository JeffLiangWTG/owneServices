namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderCargoLineLookups : BaseCusSeaManOBLHeaderLookups
	{
		public CusSeaManOBLHeaderCargoLineLookups(CusSeaManOBLHeaderCargoLine parent)
			: base(parent)
		{
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewCargoCodes() => Factory.GetCachedValue("AUCusSeaManOBLHeaderCargoLineLookups.CargoCodes", () => new CMRCargoCodes());

		public CMRCargoTypes CargoTypes => Factory.GetCachedValue("AUCusSeaManOBLHeaderCargoLineLookups.CargoTypes", () => new CMRCargoTypes());

		public CMRPackageTypes PackageTypes => Factory.GetCachedValue("AUCusSeaManOBLHeaderCargoLineLookups.PackageTypes", () => new CMRPackageTypes());
	}
}
