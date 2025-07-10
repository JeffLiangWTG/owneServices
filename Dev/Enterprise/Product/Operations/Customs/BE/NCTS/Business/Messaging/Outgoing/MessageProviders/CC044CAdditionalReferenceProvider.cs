using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CAdditionalReferenceProvider : CC044CDocumentProvider, IDocument
	{
		public CC044CAdditionalReferenceProvider(CusSupportingInfo document) : base(document)
		{
		}
	}
}
