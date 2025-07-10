using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingQueryResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingQueryResponseEDIMessage>
	{
		public CDSInventoryLinkingQueryResponseEDIMessagePrettier(CDSInventoryLinkingQueryResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			return MessagePrettierCss.CSS
				+ ToH3IfNotEmpty(Invariant($"A response to an inventory linking query"))
				+ ToH4IfNotEmpty(Invariant($"The response includes the queried MUCR: 1/ children (the whole subtree with all MUCRs and DUCRs with declarations); 2/ all movements of the queried MUCR; 3/ all parents with their movements"))
				+ GetQueriedUCRHtml()
				+ GetChildMUCRHtml()
				+ GetChildDUCRHtml()
				+ GetParentMUCRHtml();
		}

		ZString GetQueriedUCRHtml()
		{
			var ucr = ZString.Empty;
			var movements = ZString.Empty;
			var goodsItems = ZString.Empty;

			if (response.QueriedMUCR != null)
			{
				ucr = ToKeyValuePairSection(GetMUCRKeyValuePairs(response.QueriedMUCR));
				movements = GetUCRMovementHtml(response.QueriedMUCR?.Movement);
			}
			else
			{
				ucr = ToKeyValuePairSection(GetDUCRKeyValuePairs(response.QueriedDUCR));
				movements = GetUCRMovementHtml(response.QueriedDUCR?.Movement);
				goodsItems = GetDUCRGoodsItemHtml(response.QueriedDUCR?.GoodsItem);
			}

			return ucr + movements + goodsItems;
		}

		ZString GetParentMUCRHtml()
		{
			var interpretation = ZString.Empty;
			if (response.ParentMUCR != null)
			{
				interpretation = ToH4IfNotEmpty(Invariant($"Parent MUCR:")) + ToKeyValuePairSection(GetMUCRKeyValuePairs(response.ParentMUCR)) + GetUCRMovementHtml(response.ParentMUCR?.Movement);
			}
			return interpretation;
		}

		ZString GetChildMUCRHtml()
		{
			var interpretation = ZString.Empty;

			if (response.ChildMUCR != null && response.ChildMUCR.Count > 0)
			{
				var tableCreator = GetHtmlTableCreator();
				tableCreator.WriteRow(ToStrongIfNotEmpty("MUCR"), ToStrongIfNotEmpty("Shut"), ToStrongIfNotEmpty("Parent MUCR"), ToStrongIfNotEmpty("Entry Status ICS"), ToStrongIfNotEmpty("Entry Status ROE"), ToStrongIfNotEmpty("Entry Status SOE"));

				foreach (var mucr in response.ChildMUCR)
				{
					tableCreator.WriteRow(mucr.UCR, mucr.Shut.ToString(), mucr.ParentMUCR, mucr.ICS, InventoryLinkingStatusHelper.GetROEStatusDescription(mucr.ROE), InventoryLinkingStatusHelper.GetSOEMasterStatusDescription(mucr.SOE));
					var movements = GetUCRMovementHtml(mucr.Movement);
					if (!movements.IsEmpty)
					{
						tableCreator.WriteRowWithFormatting(new CellWithFormatting(movements, "colspan", "6"));
					}
				}

				interpretation = ToUlIfNotEmpty(ToTableSection(ToStrongIfNotEmpty("Child MUCRs"), tableCreator));
			}

			return interpretation;
		}

		ZString GetChildDUCRHtml()
		{
			var interpretation = ZString.Empty;

			if (response.ChildDUCR != null && response.ChildDUCR.Count > 0)
			{
				var tableCreator = GetHtmlTableCreator();
				tableCreator.WriteRow(ToStrongIfNotEmpty("DUCR"), ToStrongIfNotEmpty("Declaration ID"), ToStrongIfNotEmpty("Parent MUCR"), ToStrongIfNotEmpty("Entry Status ICS"), ToStrongIfNotEmpty("Entry Status ROE"), ToStrongIfNotEmpty("Entry Status SOE"));

				foreach (var ducr in response.ChildDUCR)
				{
					tableCreator.WriteRow(ducr.UCR, ducr.DeclarationID, ducr.ParentMUCR, ducr.ICS, InventoryLinkingStatusHelper.GetROEStatusDescription(ducr.ROE), InventoryLinkingStatusHelper.GetSOEStatusDescription(ducr.SOE));
					var movements = GetUCRMovementHtml(ducr.Movement);
					if (!movements.IsEmpty)
					{
						tableCreator.WriteRowWithFormatting(new CellWithFormatting(movements, "colspan", "6"));
					}
					var goodsItems = GetDUCRGoodsItemHtml(ducr.GoodsItem);
					if (!goodsItems.IsEmpty)
					{
						tableCreator.WriteRowWithFormatting(new CellWithFormatting(goodsItems, "colspan", "6"));
					}
				}

				interpretation = ToUlIfNotEmpty(ToTableSection(ToStrongIfNotEmpty("Child DUCRs"), tableCreator));
			}
			return interpretation;
		}

		IEnumerable<(ZString key, ZString value)> GetMUCRKeyValuePairs(InventoryLinkingMUCR mucr)
		{
			var dict = new Dictionary<ZString, ZString>();

			if (mucr != null)
			{
				dict.Add("Queried UCR", mucr.UCR);
				dict.Add("Queried UCR Type", nameof(ucrType.M));
				dict.Add("Shut", mucr.Shut.ToString());
				dict.Add("Parent UCR", mucr.ParentMUCR);
				dict.Add("Entry Status ICS", mucr.ICS);
				dict.Add("Entry Status ROE", InventoryLinkingStatusHelper.GetROEStatusDescription(mucr.ROE));
				dict.Add("Entry Status SOE", InventoryLinkingStatusHelper.GetSOEMasterStatusDescription(mucr.SOE));
			}

			return dict.SelectMany(d => new (ZString key, ZString value)[] { (d.Key, d.Value) });
		}

		IEnumerable<(ZString key, ZString value)> GetDUCRKeyValuePairs(InventoryLinkingDUCR ducr)
		{
			var dict = new Dictionary<ZString, ZString>();

			if (ducr != null)
			{
				dict.Add("Queried UCR", ducr.UCR);
				dict.Add("Queried UCR Type", nameof(ucrType.D));
				dict.Add("Declaration ID", ducr.DeclarationID);
				dict.Add("Parent UCR", ducr.ParentMUCR);
				dict.Add("Entry Status ICS", ducr.ICS);
				dict.Add("Entry Status ROE", InventoryLinkingStatusHelper.GetROEStatusDescription(ducr.ROE));
				dict.Add("Entry Status SOE", InventoryLinkingStatusHelper.GetSOEStatusDescription(ducr.SOE));
			}

			return dict.SelectMany(d => new (ZString key, ZString value)[] { (d.Key, d.Value) });
		}

		ZString GetUCRMovementHtml(List<InventoryLinkingMovement> movements)
		{
			var interpretation = ZString.Empty;

			if (movements != null && movements.Count > 0)
			{
				var movementIndex = ZInt.Zero;
				foreach (var movement in movements)
				{
					interpretation += ToH4IfNotEmpty(Invariant($"Movement: {++movementIndex}")) + ToKeyValuePairSection(GetMovementKeyValuePairs(movement));
				}
			}

			return interpretation;
		}

		(ZString key, ZString value)[] GetMovementKeyValuePairs(InventoryLinkingMovement movement)
		{
			return new (ZString key, ZString value)[]
			{
				("Message Code", movement.MessageCode),
				("Goods Location", movement.GoodsLocation),
				("Goods Arrival Date", movement.GoodsArrivalDateTime),
				("Goods Departure Date", movement.GoodsDepartureDateTime),
				("Movement Reference", movement.MovementReference),
				("Transport ID", movement.TransportDetails.TransportID),
				("Transport Mode", movement.TransportDetails.TransportMode),
				("Transport Nationality", movement.TransportDetails.TransportNationality)
			};
		}

		ZString GetDUCRGoodsItemHtml(List<InventoryLinkingGoodsItemObject> goodsItems)
		{
			var interpretation = ZString.Empty;

			if (goodsItems != null && goodsItems.Count > 0)
			{
				var tableCreator = GetHtmlTableCreator();
				tableCreator.WriteRow(ToStrongIfNotEmpty("Total Packages"));
				foreach (var goodsItem in goodsItems)
				{
					tableCreator.WriteRow(goodsItem.TotalPackages);
				}
				interpretation = ToTableSection(ToStrongIfNotEmpty("Goods Items"), tableCreator);
			}

			return interpretation;
		}

		readonly InventoryLinkingQueryResponse response;
	}
}
