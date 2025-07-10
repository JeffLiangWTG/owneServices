using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public static class Extensions
	{
		public static CodeDescriptionPairList GetSealTypeList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.IL.Manifest.Business.SealTypeList", () =>
			{
				var sealTypeList = new ILSealTypeList();
				sealTypeList.RemoveCode(ILSealTypeList.Codes.AirFreshVentSeal);
				return sealTypeList;
			});
		}
	}
}
