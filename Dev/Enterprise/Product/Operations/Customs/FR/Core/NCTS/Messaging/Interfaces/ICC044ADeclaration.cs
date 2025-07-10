using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface ICC044ADeclaration : EU.NCTS.Messaging.ICC044ADeclaration
	{
		ZString AgreementNumber { get; }
		IReadOnlyCollection<IUnloadedGoodsItem> UnloadedGoodsItems { get; }
		IReadOnlyCollection<Tuple<string, string>> ListOfDifferenceInHeader { get; }
	}
}
