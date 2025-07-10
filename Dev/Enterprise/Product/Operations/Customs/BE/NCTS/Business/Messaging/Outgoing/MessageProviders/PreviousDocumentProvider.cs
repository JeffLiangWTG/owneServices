using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class PreviousDocumentProvider : DocumentProvider, IPreviousDocument
	{
		public PreviousDocumentProvider(CusSupportingInfo document) : base(document)
		{
		}

		public string ComplementOfInformation => document.CSI_ReferenceNumber2;
	}
}
