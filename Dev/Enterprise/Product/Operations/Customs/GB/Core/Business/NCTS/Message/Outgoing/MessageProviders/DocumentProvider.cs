using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class DocumentProvider : IDocument
	{
		protected readonly CusSupportingInfo document;
		readonly bool isInPhase5TransitionPeriod;

		public DocumentProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod)
		{
			this.document = Argument.NotNull(document, nameof(document));
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}

		public virtual int SequenceNumber => document.CSI_LineNo;

		public virtual string Type => document.CSI_Code;

		public virtual string ReferenceNumber => document.CSI_ReferenceNumber;

		public int ReferenceNumberMaxLength => isInPhase5TransitionPeriod ? MessageSchemaInTransitionPeriod.ReferenceNumberMaxLength : MessageSchema.ReferenceNumberMaxLength;
	}
}
