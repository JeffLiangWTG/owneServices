using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business;

public class JobDeclarationAlternativeEvidenceLookups(AutoJobDeclarationAlternativeEvidence parent) : ZLookups(parent)
{
	public virtual CodeDescriptionPairList AlternativeEvidenceTypeList => RefCusCodeListTypes.GetCachedList(Factory, countryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, ZDateTime.Today);

	public virtual ICollection TransportDocumentTypeList => RefCusCodeListTypes.GetCachedList(Factory, countryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, ZDateTime.Today);

	string countryCode => Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
}
