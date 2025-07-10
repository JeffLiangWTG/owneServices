using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public interface IUnloadedGoodsItem
	{
		ZInt ItemNumber { get; }

		ZString CommodityCode { get; }

		ZString GoodsDescription { get; }

		ZDecimal GrossWeight { get; }

		ZDecimal NetWeight { get; }

		IReadOnlyCollection<ISupportingDocument> SupportingDocuments { get; }

		IReadOnlyCollection<IControlResult> ControlResults { get; }

		IReadOnlyCollection<ZString> Containers { get; }

		IReadOnlyCollection<IPackage> Packages { get; }
	}
}
