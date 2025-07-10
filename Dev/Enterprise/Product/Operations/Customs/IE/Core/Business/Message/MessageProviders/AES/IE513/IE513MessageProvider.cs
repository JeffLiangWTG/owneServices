using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class IE513MessageProvider : IE513And515CommonMessageProvider, IIE513Header, IIE513ExportOperation, IIE513GoodsShipment
	{
		public IE513MessageProvider(CusEntryHeader entryHeader) : base(entryHeader, false)
		{
			Consignment = new IE513And515ConsignmentProvider(entryHeaderWrapper, isSubStyle_B_C_E_F);
		}

		#region IIE513Header Members
		public IIE513ExportOperation ExportOperation => this;
		public IIE513GoodsShipment GoodsShipment => this;
		#endregion

		#region IIE513ExportOperation Members
		public string LRN => entryHeader.CH_BGMReference;
		public string MRN => entryHeader.MovementReferenceNumber;
		#endregion

		#region IIE513GoodsShipment Members
		public IIE513And515Consignment Consignment { get; }
		public IReadOnlyCollection<IIE513And515CommonGoodsItem> GoodsItems => goodsItems ?? (goodsItems = entryHeader.MergedLines.Select(x => new IE513And515CommonGoodsItemProvider(x, entryHeaderWrapper, isSubStyle_B_C_E_F)).ToArray());
		IReadOnlyCollection<IIE513And515CommonGoodsItem> goodsItems;
		#endregion

		public bool IsInTransitionPeriod => declaration.IsTransitionPeriodAES30;
	}
}
