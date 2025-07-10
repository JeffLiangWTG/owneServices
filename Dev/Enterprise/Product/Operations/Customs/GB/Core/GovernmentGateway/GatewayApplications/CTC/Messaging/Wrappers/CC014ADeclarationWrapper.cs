using CargoWise.Types;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC014ADeclarationWrapper : EU.NCTS.Business.CC014ADeclarationWrapper, ICC014ADeclaration
	{
		public CC014ADeclarationWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ZString DateOfCancellationRequest => ZDateTime.Now.ToString("yyyyMMdd");

		public ZString CancellationReason => nctsHeader.ExplanationToCustomsForWhyCancelling;

		public ZString CancellationReasonLanguage => ZString.Empty;
	}
}
