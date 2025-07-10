using CargoWise.Types;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public interface ICC014ADeclaration : EU.NCTS.Messaging.ICC014ADeclaration
	{
		ZString DateOfCancellationRequest { get; }
		ZString CancellationReason { get; }
		ZString CancellationReasonLanguage { get; }
	}
}
