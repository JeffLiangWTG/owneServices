using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class AddressExtensions
	{
		public static bool HasAddress(this JobDocAddress docAddress)
		{
			return docAddress.E2_AddressOverride ? !docAddress.IsOverridenButEmpty : docAddress.Address != null;
		}

		public static ZString GetChineseCompanyName(this OrgAddress orgAddress)
		{
			var result = orgAddress?.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.ChineseSimplified)?.CompanyName ?? ZString.Empty;

			if (result.IsEmpty)
			{
				result = orgAddress?.EffectiveCompanyName ?? ZString.Empty;
			}

			return result;
		}

		public static ZString GetEnglishCompanyName(this OrgAddress orgAddress)
		{
			var result = orgAddress?.TranslatedAddresses.Cast<OrgTranslatedAddress>().FirstOrDefault(x => x.IsEnglish)?.CompanyName ?? ZString.Empty;

			if (result.IsEmpty)
			{
				result = orgAddress?.EffectiveCompanyName ?? ZString.Empty;
			}
			return result;
		}
	}
}
