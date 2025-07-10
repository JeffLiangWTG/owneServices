using System.Collections;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class TransportDocumentLookups : Customs.Business.CusSupportingInfoLookups
{
	public TransportDocumentLookups(TransportDocument parent)
		: base(parent)
	{
	}
	protected new TransportDocument Parent => (TransportDocument)base.Parent;

	public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.TransportDocumentType, Parent.JobDeclaration.DateOfValuation);
}
