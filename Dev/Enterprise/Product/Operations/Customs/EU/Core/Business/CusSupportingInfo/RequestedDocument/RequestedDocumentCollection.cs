using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class RequestedDocumentCollection : Customs.Business.CusSupportingInfoCollection<RequestedDocument>
	{
		public RequestedDocumentCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument)
		{
		}
	}
}
