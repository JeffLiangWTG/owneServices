using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE515MessageProvider : IE513And515CommonMessageProvider, IIE515Header, IIE515ExportOperation, IIE515GoodsShipment
	{
		public IE515MessageProvider(CusEntryHeader entryHeader) : base(entryHeader, true)
		{
			Consignment = new IE513And515ConsignmentProvider(entryHeaderWrapper, isSubStyle_B_C_E_F);
		}

		#region IIE515Header Members
		public IIE515ExportOperation ExportOperation => this;
		public IIE515GoodsShipment GoodsShipment => this;
		#endregion

		public string LRN => AESOutboundEDIMessage.LRNPlaceHolder;

		#region IIE515GoodsShipment Members
		public IIE513And515Consignment Consignment { get; }

		public IReadOnlyCollection<IIE513And515CommonGoodsItem> GoodsItems => goodsItems ?? (goodsItems = entryHeader.MergedLines.Select(x => new IE513And515CommonGoodsItemProvider(x, entryHeaderWrapper, isSubStyle_B_C_E_F, !allowPreviousDocumentsOnHeaderDuringTransitionPeriod)).ToArray());

		IReadOnlyCollection<IIE513And515CommonGoodsItem> goodsItems;
		#endregion

		public bool IsInTransitionPeriod => declaration.IsTransitionPeriodAES30;
	}
}
