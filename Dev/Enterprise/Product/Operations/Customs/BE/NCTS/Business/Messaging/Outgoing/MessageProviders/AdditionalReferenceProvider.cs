using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AdditionalReferenceProvider : DocumentProvider, IDocument
	{
		public AdditionalReferenceProvider(CusSupportingInfo document) : base(document)
		{
		}

		public override int SequenceNumber => document.CSI_LineNo;
	}
}
