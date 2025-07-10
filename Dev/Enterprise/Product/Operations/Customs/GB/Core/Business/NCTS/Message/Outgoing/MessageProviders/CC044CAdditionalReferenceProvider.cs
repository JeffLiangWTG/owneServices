using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class CC044CAdditionalReferenceProvider : CC044CDocumentProvider, IAdditionalReference
	{
		public CC044CAdditionalReferenceProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod) : base(document, isInPhase5TransitionPeriod)
		{
		}
	}
}
