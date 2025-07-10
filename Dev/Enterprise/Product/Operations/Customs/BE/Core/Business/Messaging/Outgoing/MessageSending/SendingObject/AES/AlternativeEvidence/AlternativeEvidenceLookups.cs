using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business;

public class AlternativeEvidenceLookups(AlternativeEvidence parent) : JobDeclarationAlternativeEvidenceLookups(parent)
{
	public override ICollection TransportDocumentTypeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, ZDateTime.Today);
}
