using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CTransportDocumentProvider : CC044CDocumentProvider, ITransportDocument
	{
		public CC044CTransportDocumentProvider(CusSupportingInfo document) : base(document)
		{
		}
	}
}
