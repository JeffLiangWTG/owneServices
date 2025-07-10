using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStoragePreviousDocumentConfiguration : EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentConfiguration
{
	protected override ICollection GetCodeListCore(EU.Business.CusTempStorage.TemporaryStoragePreviousDocument previousDocument)
	{
		var header = previousDocument.TemporaryStorageHeader;
		ZZRefCusCodeListCombinedCollection list;

		if (new ZString[] { G5MessageTypeCodeList.Codes.G5v1Expedition, G5MessageTypeCodeList.Codes.G5v1Reception }.Contains(previousDocument.TemporaryStorageHeader.AMA_MessageType))
		{
			var customsOfficeCountry = header.AMA_CustomsOffice.SubstringSafe(0, 2);

			if (customsOfficeCountry == Core.Constants.CountryCodes.Spain)
			{
				list = GetCachedPreviousDocumentPTNSCollection(GetIsNationalAttributeFilter(Core.Constants.BooleanTrueString));
			}
			else if(!customsOfficeCountry.IsEmpty)
			{
				list = GetCachedPreviousDocumentPTNSCollection(GetIsNationalAttributeFilter(Core.Constants.BooleanFalseString));
			}
			else
			{
				list = GetCachedPreviousDocumentPTNSCollection(null);
				list.Load();
				list = list.Count == 0 ? GetCachedPreviousDocumentPTNSCollection(null, RefDataGroupingCodes.EuropeanUnionEUN) : list;
			}
		}
		else
		{
			list = GetCachedPreviousDocumentPTNSCollection(null, includeParentDataGroupings: true);
		}

		return list;
		RefCusCodeListAttributeFilter[] GetIsNationalAttributeFilter(ZString value) => new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsNational, JoinCondition.And, value) };

		ZZRefCusCodeListCombinedCollection GetCachedPreviousDocumentPTNSCollection(IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, string dataGroupingCode = Core.Constants.CountryCodes.Spain, bool includeParentDataGroupings = false)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(previousDocument.Factory, dataGroupingCode, [Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS], ZDateTime.Today, attributeFilters, includeParentDataGroupings);
		}
	}
}
