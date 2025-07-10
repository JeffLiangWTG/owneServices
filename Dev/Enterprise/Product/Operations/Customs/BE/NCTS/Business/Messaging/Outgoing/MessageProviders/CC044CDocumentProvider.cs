using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CDocumentProvider : DocumentProvider
	{
		public CC044CDocumentProvider(CusSupportingInfo document) : base(document)
		{
		}

		public override int SequenceNumber => document.CSI_LineNo;

		public override string Type => StatusIsNew ? base.Type : null;

		public override string ReferenceNumber => StatusIsNew ? base.ReferenceNumber : null;

		protected bool StatusIsNew => document.CSI_Status == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
	}
}
