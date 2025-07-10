using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business;

public static class CusAuthorizationUsageLookupsHelper
{
	public static CodeDescriptionPair GetAuthorizationUsageCodePairByCustomsCode(BusinessObjectFactory factory, ICodeDescription customsPair)
	{
		Argument.NotNull(customsPair, nameof(customsPair));
		return GetAuthorizationUsageCodePairByCustomsCode(factory, customsPair.Code, customsPair.Description);
	}

	internal static CodeDescriptionPair GetAuthorizationUsageCodePairByCustomsCode(BusinessObjectFactory factory, ZString customsCode, ZString description)
	{
		Argument.NotNullOrEmpty(customsCode, nameof(customsCode));
		Argument.NotNullOrEmpty(description, nameof(description));

		var cw1CodePairList = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(factory, DataGroupingCode, MapType, ZDateTime.Today);
		var cw1CodeList = cw1CodePairList.Where(x => x.Value == customsCode).Select(q => q.Key).OrderBy(x => x).ToArray();

		var (referenceCw1Code, descriptionCore) = ParseDescription();
		ZString cw1Code;
		if (cw1CodeList.Length == 0)
		{
			cw1Code = referenceCw1Code;
		}
		else
		{
			cw1Code = !referenceCw1Code.IsEmpty && cw1CodeList.Contains(referenceCw1Code) ? referenceCw1Code : cw1CodeList.FirstOrDefault();
		}

		return CreateAuthorizationUsageCodePair(cw1Code.IsEmpty ? customsCode : cw1Code, customsCode, descriptionCore);

		(ZString ReferenceCW1Code, ZString DescriptionCore) ParseDescription()
		{
			(ZString, ZString) result;
			if (description.Contains('-'))
			{
				var parts = description.Split('-');
				result = (GetValidReferenceCW1Code(parts[0].Trim()), parts[1].Trim());
			}
			else
			{
				result = (ZString.Empty, description);
			}
			return result;
		}

		ZString GetValidReferenceCW1Code(ZString referenceCW1Code) => referenceCW1Code.Length <= 4 ? referenceCW1Code : ZString.Empty;
	}

	public static CodeDescriptionPair GetAuthorizationUsageCodePairByCW1Code(BusinessObjectFactory factory, ZString cw1Code, ZString description)
	{
		Argument.NotNullOrEmpty(cw1Code, nameof(cw1Code));
		Argument.NotNullOrEmpty(description, nameof(description));

		var customsCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, DataGroupingCode, MapType, cw1Code, ZDateTime.Today);
		return CreateAuthorizationUsageCodePair(cw1Code, customsCode, description);
	}

	internal static CodeDescriptionPair CreateAuthorizationUsageCodePair(ZString cw1Code, ZString customsCode, ZString description)
	{
		var newDescription = (ZString)$"{customsCode} - {description}";
		return new CodeDescriptionPair(cw1Code.ToString(), newDescription.Trim(' ', '-').ToString());
	}

	const string DataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
	const string MapType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
}
