using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingMovementTotalsResponse : InventoryLinkingResponse
	{
		public InventoryLinkingMovementTotalsResponse(ZString xml) : base(xml)
		{
		}

		protected override ZString RootNode => "inventoryLinkingMovementTotalsResponse";

		public ZString Crc => SelectSingleNode(CrcNodeXPath);

		public ZString GoodsLocation => SelectSingleNode(GoodsLocationNodeXPath);

		public ZString MasterUcr => SelectSingleNode(MasterUCRNodeXPath);

		public ZString DeclarationCount => SelectSingleNode(DeclarationCountNodeXPath);

		public ZString GoodsArrivalDateTime => SelectSingleNode(GoodsArrivalDateTimeNodeXPath);

		public ZString ShedOPID => SelectSingleNode(ShedOPIDNodeXPath);

		public ZString MasterROE => SelectSingleNode(MasterROENodeXPath);

		public ZString MasterSOE => SelectSingleNode(MasterSOENodeXPath);

		public List<InventoryLinkingEntry> Entry => GetInventoryLinkingMovementTotalsEntryResponse(SelectNodes(EntryNodeXPath));

		public List<InventoryLinkingEntry> GetInventoryLinkingMovementTotalsEntryResponse(XmlNodeList entryNodeList)
		{
			var entryList = new List<InventoryLinkingEntry>();

			foreach (XmlNode entryNode in entryNodeList)
			{
				var ucrPartNo = GetStringFromNode(entryNode, UCRPartNoNodeXPath);
				var ucrType = GetStringFromNode(entryNode, UCRTypeNodeXPath);
				var entry = new InventoryLinkingEntry
				{
					UCR = UCRHelper.UcrBlockToString(GetStringFromNode(entryNode, UCRNodeXPath), ucrPartNo, ucrType),
					UCRPartNo = ucrPartNo,
					UCRType = ucrType,
					GoodsItem = GetInventoryLinkingGoodsItemList(entryNode.SelectNodes(EntryNodeGoodsItemNodeXPath)),
					SubmitRole = GetStringFromNode(entryNode, SubmitRoleNodeXPath),
					ICS = GetStringFromNode(entryNode, EntryStatusICSNodeXPath),
					ROE = GetStringFromNode(entryNode, EntryStatusROENodeXPath),
					SOE = GetStringFromNode(entryNode, EntryStatusSOENodeXPath)
				};
				entryList.Add(entry);
			}
			return entryList;
		}

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

		ZString CrcNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='crc']");
		ZString GoodsLocationNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='goodsLocation']");
		ZString MasterUCRNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='masterUCR']");
		ZString DeclarationCountNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='declarationCount']");
		ZString GoodsArrivalDateTimeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='goodsArrivalDateTime']");
		ZString ShedOPIDNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='shedOPID']");
		ZString MasterROENodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='masterROE']");
		ZString MasterSOENodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='masterSOE']");
		ZString EntryNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='entry']");
		ZString UCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='ucrBlock']/*[local-name()='ucr']");
		ZString UCRPartNoNodeXPath => System.FormattableString.Invariant($"*[local-name()='ucrBlock']/*[local-name()='ucrPartNo']");
		ZString UCRTypeNodeXPath => System.FormattableString.Invariant($"*[local-name()='ucrBlock']/*[local-name()='ucrType']");
		ZString EntryNodeGoodsItemNodeXPath => System.FormattableString.Invariant($"*[local-name()='goodsItem']");
		ZString SubmitRoleNodeXPath => System.FormattableString.Invariant($"*[local-name()='submitRole']");
		ZString EntryStatusICSNodeXPath => System.FormattableString.Invariant($"*[local-name()='entryStatus']/*[local-name()='ics']");
		ZString EntryStatusSOENodeXPath => System.FormattableString.Invariant($"*[local-name()='entryStatus']/*[local-name()='soe']");
		ZString EntryStatusROENodeXPath => System.FormattableString.Invariant($"*[local-name()='entryStatus']/*[local-name()='roe']");
		ZString CommodityCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='commodityCode']");
		ZString TotalNetMassNodeXPath => System.FormattableString.Invariant($"*[local-name()='totalNetMass']");
		ZString TotalPackagesNodeXPath => System.FormattableString.Invariant($"*[local-name()='totalPackages']");
	}
}
