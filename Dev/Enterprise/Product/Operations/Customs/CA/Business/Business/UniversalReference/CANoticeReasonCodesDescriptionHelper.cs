using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	public static class CANoticeReasonCodesDescriptionHelper
	{
		public static ZString GetD4NoticesDescriptionFromCode(BusinessObjectFactory factory, string code)
		{
			return factory.GetCachedValue(code, delegate
			{
				var result = ZString.Empty;
				if (!string.IsNullOrEmpty(code))
				{
					var noticeCode = ZZRefCusCodeListCombined.Loader.LoadByCode(factory, Core.Constants.CountryCodes.Canada, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode }, code, ZDateTime.Today).FirstOrDefault();
					result = noticeCode != null ? noticeCode.ZZD_Description : ZString.Empty;
				}
				return result;
			});
		}
	}
}
