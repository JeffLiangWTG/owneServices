using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPackageLookups : ZLookups
	{
		public AQISPackageLookups(AQISPackage parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AQISPackageTypeList
		{
			get
			{
				if (fZA_AQISPackageType_List == null)
				{
					fZA_AQISPackageType_List = new CMRPackageTypes();
				}

				return fZA_AQISPackageType_List;
			}
		}
		CodeDescriptionPairList fZA_AQISPackageType_List;
	}
}
