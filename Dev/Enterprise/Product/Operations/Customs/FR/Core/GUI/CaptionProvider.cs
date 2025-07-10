using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.FR.GUI
{
	public static class CaptionProvider
	{
		internal static ResourceStringData AdditionalInfoTabPageCaption(bool isUCC6)
		{
			return isUCC6 ? Res.GetData("BA1D4C56-43BA-4762-BA75-7F116BB51E38", "[44] Additional Documents")
				: Res.GetData("90A97655-EE04-4473-AE43-2E8C0BA2AEE5", "[44] Special Mentions");
		}

		internal static ResourceStringData NationalAdditionalCodeTabPageCaption => Res.GetData("1A206306-5966-428B-A29A-5F46AE1EB52F", "[44] National Additional Code");

		internal static ResourceStringData TariffBypassCodeCaption => Res.GetData("15B167B0-4C76-4F30-A5B6-FF6CF9CF8311", "Tariff Bypass Code");

		internal static ResourceStringData TariffBypassReasonCaption => Res.GetData("95888E03-704D-4000-A669-ED1AE50EE30C", "Reason", "Tariff Bypass Reason");
	}
}
