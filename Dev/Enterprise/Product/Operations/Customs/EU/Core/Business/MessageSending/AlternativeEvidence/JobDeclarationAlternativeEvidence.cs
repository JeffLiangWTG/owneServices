using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business;

public class JobDeclarationAlternativeEvidence(BusinessObjectFactory factory) : AutoJobDeclarationAlternativeEvidence(factory)
{
	public ZZRefCusCodeListCombined DocTypeRefCusCodeList
	{
		get
		{
			ZZRefCusCodeListCombined result = null;
			var docType = DocType;
			if (!docType.IsEmpty)
			{
				result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, docType, Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, ZDateTime.Today);
			}
			return result;
		}
	}

	protected override JobDeclarationAlternativeEvidenceValidation GetNewValidation() => new(this);

	public JobDeclarationAlternativeEvidenceLookups Lookups => lookups ??= GetNewLookups();
	JobDeclarationAlternativeEvidenceLookups lookups;

	protected virtual JobDeclarationAlternativeEvidenceLookups GetNewLookups() => new(this);
}
