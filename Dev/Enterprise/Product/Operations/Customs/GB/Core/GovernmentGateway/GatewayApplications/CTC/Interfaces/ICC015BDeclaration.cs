using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public interface ICC015BDeclaration : EU.NCTS.Messaging.ICC015BDeclaration
	{
		ITrader Carrier { get; }

		ZBool IsSimplifiedNctsProcedure { get; }

		ZBool IsConsignorDefinedAtGoodsItemLevel { get; }
		ZBool IsConsigneeDefinedAtGoodsItemLevel { get; }
		ZBool HasSecurityAtGoodsItemLevel { get; }
	}
}
