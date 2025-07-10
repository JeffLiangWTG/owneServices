using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class AdditionalReferenceProvider : DocumentProvider, IAdditionalReference
	{
		public AdditionalReferenceProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod) : base(document, isInPhase5TransitionPeriod)
		{
		}

		public override int SequenceNumber => document.CSI_LineNo;
	}
}
