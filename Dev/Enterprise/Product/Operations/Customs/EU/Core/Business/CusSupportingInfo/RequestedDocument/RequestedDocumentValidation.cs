using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class RequestedDocumentValidation : CusSupportingInfoValidation
	{
		public RequestedDocumentValidation(RequestedDocument parent) : base(parent)
		{
		}

		protected new RequestedDocument Parent => (RequestedDocument)base.Parent;

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_StatusInfo);
		}
	}
}
