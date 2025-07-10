using System.Collections.Generic;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingQueryResponse : InventoryLinkingResponse
	{
		public InventoryLinkingQueryResponse(ZString xml) : base(xml) { }

		protected override ZString RootNode => "inventoryLinkingQueryResponse";

		public InventoryLinkingMUCR QueriedMUCR => GetMUCR((XmlNode)SelectSingleXmlNode(QueriedMUCRNodeXPath));
		public InventoryLinkingDUCR QueriedDUCR => GetDUCR((XmlNode)SelectSingleXmlNode(QueriedDUCRNodeXPath));

		public List<InventoryLinkingMUCR> ChildMUCR => GetChildMUCR(SelectNodes(ChildMUCRNodeXPath));

		public List<InventoryLinkingDUCR> ChildDUCR => GetChildDUCR(SelectNodes(ChildDUCRNodeXPath));

		public InventoryLinkingMUCR ParentMUCR => GetMUCR((XmlNode)SelectSingleXmlNode(ParentMUCRNodeXPath));

		List<InventoryLinkingMUCR> GetChildMUCR(XmlNodeList mucrNodeList)
		{
			var mucrList = new List<InventoryLinkingMUCR>();
			if (mucrNodeList != null)
			{
				foreach (XmlNode mucrNode in mucrNodeList)
				{
					mucrList.Add(GetMUCR(mucrNode));
				}
			}
			return mucrList;
		}

		InventoryLinkingMUCR GetMUCR(XmlNode mucrNode)
		{
			var mucr = default(InventoryLinkingMUCR);
			if (mucrNode != null)
			{
				mucr = new InventoryLinkingMUCR
				{
					UCR = GetStringFromNode(mucrNode, UCRChildNodeXPath),
					Shut = GetBoolFromNode(mucrNode, ShutChildNodeXPath),
					ParentMUCR = GetStringFromNode(mucrNode, ParentMUCRChildNodeXPath),
					ICS = GetStringFromNode(mucrNode, EntryStatusICSNodeXPath),
					ROE = GetStringFromNode(mucrNode, EntryStatusROENodeXPath),
					SOE = GetStringFromNode(mucrNode, EntryStatusSOENodeXPath),
					Movement = GetMovementList(SelectNodes(mucrNode, MovementNodeXPath))
				};
			}
			return mucr;
		}

		List<InventoryLinkingDUCR> GetChildDUCR(XmlNodeList ducrNodeList)
		{
			var ducrList = new List<InventoryLinkingDUCR>();
			if (ducrNodeList != null)
			{
				foreach (XmlNode ducrNode in ducrNodeList)
				{
					ducrList.Add(GetDUCR(ducrNode));
				}
			}
			return ducrList;
		}

		InventoryLinkingDUCR GetDUCR(XmlNode ducrNode)
		{
			var ducr = default(InventoryLinkingDUCR);
			if (ducrNode != null)
			{
				ducr = new InventoryLinkingDUCR
				{
					UCR = GetStringFromNode(ducrNode, UCRChildNodeXPath),
					DeclarationID = GetStringFromNode(ducrNode, DeclarationIDChildNodeXPath),
					ParentMUCR = GetStringFromNode(ducrNode, ParentMUCRChildNodeXPath),
					ICS = GetStringFromNode(ducrNode, EntryStatusICSNodeXPath),
					ROE = GetStringFromNode(ducrNode, EntryStatusROENodeXPath),
					SOE = GetStringFromNode(ducrNode, EntryStatusSOENodeXPath),
					Movement = GetMovementList(SelectNodes(ducrNode, MovementNodeXPath)),
					GoodsItem = GetGoodsItemList(SelectNodes(ducrNode, GoodsItemNodeXPath)),
				};
			}
			return ducr;
		}

		List<InventoryLinkingMovement> GetMovementList(XmlNodeList movementNodeList)
		{
			var movementList = new List<InventoryLinkingMovement>();
			if (movementNodeList != null)
			{
				foreach (XmlNode movementNode in movementNodeList)
				{
					var movement = new InventoryLinkingMovement
					{
						MessageCode = GetStringFromNode(movementNode, MovementMessageCodeNodeXPath),
						GoodsLocation = GetStringFromNode(movementNode, GoodsLocationNodeXPath),
						GoodsArrivalDateTime = GetStringFromNode(movementNode, GoodsArrivalDateTimeNodeXPath),
						GoodsDepartureDateTime = GetStringFromNode(movementNode, GoodsDepartureDateTimeNodeXPath),
						MovementReference = GetStringFromNode(movementNode, MovementReferenceNodeXPath),
						TransportDetails = GetTransportDetails(movementNode.SelectSingleNode(TransportDetailsNodeXPath)),
					};
					movementList.Add(movement);
				}
			}
			return movementList;
		}

		List<InventoryLinkingGoodsItemObject> GetGoodsItemList(XmlNodeList goodsItemObjectNodeList)
		{
			var goiList = new List<InventoryLinkingGoodsItemObject>();
			if (goodsItemObjectNodeList != null)
			{
				foreach (XmlNode gioNode in goodsItemObjectNodeList)
				{
					var gio = new InventoryLinkingGoodsItemObject
					{
						TotalPackages = GetStringFromNode(gioNode, TotalPackagesNodeXPath)
					};
					goiList.Add(gio);
				}
			}
			return goiList;
		}

		InventoryLinkingTransportDetails GetTransportDetails(XmlNode transportDetailsNpde)
		{
			if (transportDetailsNpde != null)
			{
				return new InventoryLinkingTransportDetails
				{
					TransportID = GetStringFromNode(transportDetailsNpde, TransportIDNodeXPath),
					TransportMode = GetStringFromNode(transportDetailsNpde, TransportModeNodeXPath),
					TransportNationality = GetStringFromNode(transportDetailsNpde, TransportNationalityNodeXPath)
				};
			}
			return new InventoryLinkingTransportDetails();
		}

		protected ZString QueriedMUCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='queriedMUCR']");
		protected ZString QueriedDUCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='queriedDUCR']");
		protected ZString ChildMUCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='childMUCR']");
		protected ZString ChildDUCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='childDUCR']");
		protected ZString ParentMUCRNodeXPath => System.FormattableString.Invariant($"*[local-name()='{RootNode}']/*[local-name()='parentMUCR']");
		protected ZString UCRChildNodeXPath => System.FormattableString.Invariant($"*[local-name()='UCR']");
		protected ZString ShutChildNodeXPath => System.FormattableString.Invariant($"*[local-name()='shut']");
		protected ZString ParentMUCRChildNodeXPath => System.FormattableString.Invariant($"*[local-name()='parentMUCR']");
		protected ZString DeclarationIDChildNodeXPath => System.FormattableString.Invariant($"*[local-name()='declarationID']");

		protected ZString MovementNodeXPath => System.FormattableString.Invariant($"*[local-name()='movement']");
		protected ZString MovementMessageCodeNodeXPath => System.FormattableString.Invariant($"*[local-name()='messageCode']");
		protected ZString GoodsLocationNodeXPath => System.FormattableString.Invariant($"*[local-name()='goodsLocation']");
		protected ZString GoodsArrivalDateTimeNodeXPath => System.FormattableString.Invariant($"*[local-name()='goodsArrivalDateTime']");
		protected ZString GoodsDepartureDateTimeNodeXPath => System.FormattableString.Invariant($"*[local-name()='goodsDepartureDateTime']");
		protected ZString MovementReferenceNodeXPath => System.FormattableString.Invariant($"*[local-name()='movementReference']");

		protected ZString TransportDetailsNodeXPath => System.FormattableString.Invariant($"*[local-name()='transportDetails']");
		protected ZString TransportIDNodeXPath => System.FormattableString.Invariant($"*[local-name()='transportID']");
		protected ZString TransportModeNodeXPath => System.FormattableString.Invariant($"*[local-name()='transportMode']");
		protected ZString TransportNationalityNodeXPath => System.FormattableString.Invariant($"*[local-name()='transportNationality']");

		protected ZString GoodsItemNodeXPath => System.FormattableString.Invariant($"*[local-name()='goodsItem']");
		protected ZString TotalPackagesNodeXPath => System.FormattableString.Invariant($"*[local-name()='totalPackages']");

		protected ZString EntryStatusICSNodeXPath => System.FormattableString.Invariant($"*[local-name()='entryStatus']/*[local-name()='ics']");
		protected ZString EntryStatusSOENodeXPath => System.FormattableString.Invariant($"*[local-name()='entryStatus']/*[local-name()='soe']");
		protected ZString EntryStatusROENodeXPath => System.FormattableString.Invariant($"*[local-name()='entryStatus']/*[local-name()='roe']");
	}
}
