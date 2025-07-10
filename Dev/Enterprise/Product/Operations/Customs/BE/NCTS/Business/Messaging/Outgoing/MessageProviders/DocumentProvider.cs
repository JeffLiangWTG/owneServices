using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class DocumentProvider : IDocument
	{
		protected readonly CusSupportingInfo document;

		public DocumentProvider(CusSupportingInfo document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		public virtual int SequenceNumber => document.CSI_LineNo;

		public virtual string Type => document.CSI_Code;

		public virtual string ReferenceNumber => document.CSI_ReferenceNumber;
	}
}
