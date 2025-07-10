using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ArrivalDocumentProvider : ITransportDocument, IAdditionalReference
	{
		public ArrivalDocumentProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod)
		{
			this.document = Argument.NotNull(document, nameof(document));
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}

		public virtual int SequenceNumber => document.CSI_LineNo;

		public virtual string Type => StatusIsNew ? document.CSI_Code : ZString.Empty;

		public virtual string ReferenceNumber => StatusIsNew ? document.CSI_ReferenceNumber : ZString.Empty;

		protected bool StatusIsNew => document.CSI_Status == NctsUnloadedStateList.Codes.NEW;

		public int ReferenceNumberMaxLength => isInPhase5TransitionPeriod ? MessageSchemaInTransitionPeriod.ReferenceNumberMaxLength : MessageSchema.ReferenceNumberMaxLength;

		readonly CusSupportingInfo document;
		readonly bool isInPhase5TransitionPeriod;
	}
}
