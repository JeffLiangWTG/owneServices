using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public static class RefCusCodeListLoader
{
	public static CodeDescriptionPairList GetDirectTransportationCountryList(BusinessObjectFactory factory, ZDateTime date)
	{
		return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.DirectTransportationCountry, date);
	}

	public static bool IsDirectTransportationCountry(BusinessObjectFactory factory, ZString countryCode, ZDateTime date)
	{
		return GetDirectTransportationCountryList(factory, date).ContainsCode(countryCode);
	}

	public static CodeDescriptionPairList GetBulkPackTypeList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue($"CH.GetBulkPackTypeList.{ZDate.Today}",
			() => RefCusCodeListTypes.GetCachedListMatchAnyAttributes(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			ZDate.Today,
			new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Bulk, ZString.Empty) }
			));
	}

	public static CodeDescriptionPairList GetBreakBulkPackTypeList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue($"CH.GetBreakBulkPackTypeList.{ZDate.Today}",
			() => RefCusCodeListTypes.GetCachedListMatchAnyAttributes(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			ZDate.Today,
			new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.BreakBulk, ZString.Empty) }
			));
	}

	public static ZBool IsBulkPackType(BusinessObjectFactory factory, ZString packCode) => GetBulkPackTypeList(factory).ContainsCode(packCode);

	public static ZBool IsBreakBulkPackType(BusinessObjectFactory factory, ZString packCode) => GetBreakBulkPackTypeList(factory).ContainsCode(packCode);

	public static CodeDescriptionPairList GetExportSupportingDocumentsWithAttribute(BusinessObjectFactory factory, ZDateTime date, string attributeName)
	{
		return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, date, false, attributeName, new ZString[] { UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes });
	}

	public static ZBool IsExportSupportingDocumentRequiringReference(BusinessObjectFactory factory, ZString supportingDocumentCode, ZDateTime date)
	{
		return GetExportSupportingDocumentsWithAttribute(factory, date, UniversalReferenceConstants.RefCusCodeList.Attributes.Reference).ContainsCode(supportingDocumentCode);
	}

	public static ZBool IsExportSupportingDocumentRequiringIssueDate(BusinessObjectFactory factory, ZString supportingDocumentCode, ZDateTime date)
	{
		return GetExportSupportingDocumentsWithAttribute(factory, date, UniversalReferenceConstants.RefCusCodeList.Attributes.IssuingDate).ContainsCode(supportingDocumentCode);
	}

	public static CodeDescriptionPairList GetGSPCertificateCodes(BusinessObjectFactory factory, ZDateTime date)
	{
		return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, Core.Constants.CountryCodes.Switzerland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, date, false,
			UniversalReferenceConstants.RefCusCodeList.Attributes.GSPCertificate,
			new ZString[] { UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes });
	}

	public static bool IsGSPCertificate(BusinessObjectFactory factory, ZString code, ZDateTime date)
	{
		return IsAnyGSPCertificate(factory, new[] { code }, date);
	}

	public static bool IsAnyGSPCertificate(BusinessObjectFactory factory, IEnumerable<ZString> codes, ZDateTime date)
	{
		var result = false;
		if (codes != null && codes.Any())
		{
			var certificateCodes = GetGSPCertificateCodes(factory, date);
			result = codes.Any(code => certificateCodes.ContainsCode(code));
		}
		return result;
	}

	public static CodeDescriptionPairList GetOriginDocumentCodes(BusinessObjectFactory factory, ZDateTime date)
	{
		return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory: factory, country: Core.Constants.CountryCodes.Switzerland, codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, date: date, attributeName: UniversalReferenceConstants.RefCusCodeList.Attributes.OriginDocument, attributeValues: new ZString[] { UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes }, matchIfAttributeNotExists: false);
	}

	public static bool IsValidOriginDocument(BusinessObjectFactory factory, ZString code, ZDateTime date)
	{
		return IsAnyOriginDocument(factory, new[] { code }, date);
	}

	public static bool IsAnyOriginDocument(BusinessObjectFactory factory, IEnumerable<ZString> codes, ZDateTime date)
	{
		var result = false;
		if (codes != null && codes.Any())
		{
			var originDocumentCodes = GetOriginDocumentCodes(factory, date);
			result = codes.Any(code => originDocumentCodes.ContainsCode(code));
		}
		return result;
	}
}
