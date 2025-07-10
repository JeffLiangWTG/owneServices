using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PreviousDocumentProvider : DocumentProvider, IPreviousDocument
	{
		public PreviousDocumentProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod) : base(document, isInPhase5TransitionPeriod)
		{
		}

		public string ComplementOfInformation => document.CSI_ReferenceNumber2;
	}
}
