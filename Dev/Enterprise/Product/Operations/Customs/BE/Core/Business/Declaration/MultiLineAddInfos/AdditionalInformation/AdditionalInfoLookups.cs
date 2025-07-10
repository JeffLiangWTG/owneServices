using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
{
	public AdditionalInfoLookups(AdditionalInfo parent) : base(parent)
	{
	}

	protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

	public CodeDescriptionPairList InvoiceHeaderKindList => Factory.GetCachedValue("BE.AdditionalInfoLookups.InvoiceHeaderKindList", () =>
	{
		var result = new BEAdditionalDocTypeList();
		result.RemoveCode(BEAdditionalDocTypeList.Codes.Authorization);
		return result;
	});

	internal ZZRefCusCodeListCombined FullTypeRefCusCode
	{
		get
		{
			var code = Parent.CSI_Code;
			var codeType = GetRefCusCodeListTypes(Parent.ImportExportParent, Parent.CSI_SubType)[0];
			var levelAttributeValue = RefCusCodeListLevel;
			var date = ZDateTime.Today;
			return Factory.GetCachedValue($"BE.AdditionalInfoLookups.FullTypeRefCusCode_{levelAttributeValue}_{codeType}_{code}_{date}", // Cache Key
				() => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, code, Core.Constants.CountryCodes.Belgium, codeType, date,
					attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, JoinCondition.And, levelAttributeValue) }));
		}
	}

	string RefCusCodeListLevel => Parent.ParentAsInvoiceHeader != null ? EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header : EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item;

	protected override ZString[] GetRefCusCodeListTypes(EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport importExportParent, ZString csiSubType)
	{
		switch (csiSubType)
		{
			case BEAdditionalDocTypeList.Codes.AdditionalReference:
			case BEAdditionalDocTypeList.Codes.AdditionalInformation:
			case BEAdditionalDocTypeList.Codes.TransportDocuments:
				return base.GetRefCusCodeListTypes(importExportParent, csiSubType);
			default:
				return importExportParent.IsImport
					? new ZString[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44I }
					: new ZString[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44E, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44E };
		}
	}
}
