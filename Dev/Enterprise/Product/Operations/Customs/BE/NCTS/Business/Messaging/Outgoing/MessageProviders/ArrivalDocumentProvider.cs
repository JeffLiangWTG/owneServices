using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ArrivalDocumentProvider : ITransportDocument, IDocument
	{
		public ArrivalDocumentProvider(CusSupportingInfo document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}
		protected readonly CusSupportingInfo document;

		public virtual int SequenceNumber => document.CSI_LineNo;

		public virtual string Type => StatusIsNew ? document.CSI_Code : ZString.Empty;

		public virtual string ReferenceNumber => StatusIsNew ? document.CSI_ReferenceNumber : ZString.Empty;

		protected bool StatusIsNew => document.CSI_Status == NctsUnloadedStateList.Codes.NEW;
	}
}
