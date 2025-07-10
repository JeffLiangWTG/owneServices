using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class UnloadedGoodsItemWrapper : IUnloadedGoodsItem
	{
		public static UnloadedGoodsItemWrapper New(NctsArrivalAndUnloadingCargoDesc goodsItem) => goodsItem != null ? new UnloadedGoodsItemWrapper(goodsItem) : null;

		UnloadedGoodsItemWrapper(NctsArrivalAndUnloadingCargoDesc goodsItem)
		{
			this.goodsItem = goodsItem;
		}
		readonly NctsArrivalAndUnloadingCargoDesc goodsItem;

		public ZInt ItemNumber => goodsItem.BY_LineNo;

		public ZString CommodityCode => goodsItem.BY_HarmonisedTariff;

		public ZString GoodsDescription => goodsItem.BY_Description;

		public ZDecimal GrossWeight => goodsItem.BY_GrossWeight;

		public ZDecimal NetWeight => goodsItem.BY_NetWeight;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = goodsItem.SupportingDocuments.OfType<NctsSupportingDocument>().Select(p => new Business.MessagesWrappers.Common.DocumentWrapper(p)).ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IControlResult> ControlResults => controlResults ?? (controlResults = goodsItem.ResultsOfControlCollection.OfType<CusAddInfo<ResultsOfControlAddInfo>>().Select(p => new ControlResultWrapper(p.Data)).ToArray());
		IReadOnlyCollection<IControlResult> controlResults;

		public IReadOnlyCollection<ZString> Containers => containers ?? (containers = goodsItem.Containers.Select(x => x.ContainerNumber).ToArray());
		IReadOnlyCollection<ZString> containers;

		public IReadOnlyCollection<IPackage> Packages => packages ?? (packages = goodsItem.Packages.OfType<NctsPackage>().Select(p => new PackageWrapper(p)).ToArray());
		IReadOnlyCollection<IPackage> packages;
	}
}
