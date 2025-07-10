using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC007ADeclarationWrapper : EU.NCTS.Business.CC007ADeclarationWrapper, ICC007ADeclaration
	{
		public CC007ADeclarationWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}
	}
}
