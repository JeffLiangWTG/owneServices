using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingMovementResponse : InventoryLinkingResponse
	{
		public InventoryLinkingMovementResponse(ZString xml) : base(xml)
		{
		}

		protected override ZString RootNode => "inventoryLinkingMovementResponse";

		public ZString Crc => SelectSingleNode(CrcNodeXPath);

		public ZString GoodsArrivalDateTime => SelectSingleNode(GoodsArrivalDateTimeNodeXPath);

		public ZString GoodsLocation => SelectSingleNode(GoodsLocationNodeXPath);

		public ZString ShedOPID => SelectSingleNode(ShedOPIDNodeXPath);

		public ZString SubmitRole => SelectSingleNode(SubmitRoleNodeXPath);

		public ZString UCR => UCRHelper.UcrBlockToString(SelectSingleNode(UCRNodeXPath), UCRPartNo, UCRType);

		public ZString UCRPartNo => SelectSingleNode(UCRPartNoNodeXPath);

		public ZString UCRType => SelectSingleNode(UCRTypeNodeXPath);

		public List<InventoryLinkingGoodsItem> GoodsItem => GetInventoryLinkingGoodsItemList(GoodsItemNodeList);

		public List<InventoryLinkingGoodsItem> GetInventoryLinkingGoodsItemList(XmlNodeList goodsItemNodeList)
		{
			var goodsItemList = new List<InventoryLinkingGoodsItem>();

			foreach (XmlNode giNode in goodsItemNodeList)
			{
				var gi = new InventoryLinkingGoodsItem
				{
					CommodityCode = GetStringFromNode(giNode, CommodityCodeNodeXPath),
					TotalNetMass = GetDecimalFromNode(giNode, TotalNetMassNodeXPath),
					TotalPackages = GetStringFromNode(giNode, TotalPackagesNodeXPath)
				};
				goodsItemList.Add(gi);
			}
			return goodsItemList;
		}

		public XmlNodeList GoodsItemNodeList => SelectNodes(GoodsItemNodeXPath);

		public ZString ICS => SelectSingleNode(ICSNodeXPath);

		public ZString ROE => SelectSingleNode(ROENodeXPath);

		public ZString SOE => SelectSingleNode(SOENodeXPath);

		ZString CrcNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='crc']");
		ZString GoodsArrivalDateTimeNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='goodsArrivalDateTime']");
		ZString GoodsLocationNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='goodsLocation']");
		ZString ShedOPIDNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='shedOPID']");
		ZString SubmitRoleNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='submitRole']");
		ZString UCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='ucrBlock']/*[local-name()='ucr']");
		ZString UCRPartNoNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='ucrBlock']/*[local-name()='ucrPartNo']");
		ZString UCRTypeNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='ucrBlock']/*[local-name()='ucrType']");
		ZString GoodsItemNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='goodsItem']");
		ZString CommodityCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='commodityCode']");
		ZString TotalNetMassNodeXPath => System.FormattableString.Invariant($"*[local-name()='totalNetMass']");
		ZString TotalPackagesNodeXPath => System.FormattableString.Invariant($"*[local-name()='totalPackages']");
		ZString ICSNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='entryStatus']/*[local-name()='ics']");
		ZString ROENodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='entryStatus']/*[local-name()='roe']");
		ZString SOENodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='entryStatus']/*[local-name()='soe']");
	}
}
