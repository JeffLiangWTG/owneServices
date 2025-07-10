using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotLookups : Customs.Business.CusSCAPivotLookups
	{
		public CusSCAPivotLookups(Customs.Business.BaseCusSCAPivot parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList PackageTypes
		{
			get { return Factory.GetCachedValue<CMRPackageTypes>(); }
		}
	}
}
