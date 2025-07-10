using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public static class PackageHelper
	{
		public static ZString[] GetSingleCountPackageTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DE|GetSingleCountPackageTypes", () => new ZString[] { "VG", "VL", "VO", "VQ", "VR", "VS", "VY" });
		}

		public static ZString[] GetMultipleCountPackageTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DE|GetMultipleCountPackageTypes", () => new ZString[] { "NE", "NF", "NG" });
		}

		public static ZBool IsSupportEmptyPackType(BusinessObjectFactory factory, ZString type)
		{
			var singleCountPackageTypes = GetSingleCountPackageTypes(factory);
			var multipleCountPackageTypes = GetMultipleCountPackageTypes(factory);
			return !singleCountPackageTypes.Contains(type) && !multipleCountPackageTypes.Contains(type);
		}
	}
}
