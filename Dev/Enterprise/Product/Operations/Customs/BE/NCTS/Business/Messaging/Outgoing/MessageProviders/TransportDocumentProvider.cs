using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class TransportDocumentProvider : DocumentProvider, ITransportDocument
	{
		public TransportDocumentProvider(CusSupportingInfo document) : base(document)
		{
		}

		public override int SequenceNumber => document.CSI_LineNo;
	}
}
