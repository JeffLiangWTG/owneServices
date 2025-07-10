using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
{
	public PreviousDocumentLookups(PreviousDocument parent) : base(parent)
	{
	}

	public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Belgium, new ZString[] { Parent.Declaration?.IsImport == true ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection }, ZDateTime.Today, null, false);

	public CustomsOfficeCodeCollection CustomsOffices => new CustomsOfficeCodeCollection(Factory, Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

	public CodeDescriptionPairList CustomsUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

	public CodeDescriptionPairList PackageCodeList => Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, ZDateTime.Today);
}
