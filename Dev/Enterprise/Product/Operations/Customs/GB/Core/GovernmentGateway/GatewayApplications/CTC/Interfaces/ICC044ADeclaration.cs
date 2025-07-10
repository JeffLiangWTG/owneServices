using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public interface ICC044ADeclaration : EU.NCTS.Messaging.ICC044ADeclaration
	{
		IReadOnlyCollection<ISealID> Seals { get; }
		IReadOnlyCollection<IUnloadedGoodsItem> UnloadedGoodsItems { get; }
		int NumberOfSeals { get; }
	}
}
