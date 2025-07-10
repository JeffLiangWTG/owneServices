using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public partial class CustomsRegimeList
	{
		public static CodeDescriptionPairList GetCustomRegimeListForExport(BusinessObjectFactory factory)
		{
			var cachekey = "CustomsRegimeList_EXP";
			return factory.GetCachedValue(cachekey, () =>
			{
				var result = new CustomsRegimeList();
				result.RemoveCode(Codes.IMD);
				result.RemoveCode(Codes.ITR);
				result.RemoveCode(Codes.ITE);
				return result;
			});
		}

		public static CodeDescriptionPairList GetCustomRegimeListForImport(BusinessObjectFactory factory)
		{
			var cachekey = "CustomsRegimeList_IMP";
			return factory.GetCachedValue(cachekey, () =>
			{
				var result = new CustomsRegimeList();
				result.RemoveCode(Codes.EXD);
				result.RemoveCode(Codes.ETR);
				result.RemoveCode(Codes.ETE);
				return result;
			});
		}
	}
}
