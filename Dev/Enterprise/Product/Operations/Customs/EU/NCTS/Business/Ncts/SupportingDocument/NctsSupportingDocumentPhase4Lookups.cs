using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsSupportingDocumentPhase4Lookups : NctsSupportingDocumentLookups
	{
		public NctsSupportingDocumentPhase4Lookups(NctsSupportingDocument parent) : base(parent)
		{
		}

		public override ZZRefCusCodeListCombinedCollection TypeCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
				, DataGroupingCode
				, RefCusCodeListType.Code.SupportingDocumentOfNCTS
				, ZDateTime.Today);
	}
}
