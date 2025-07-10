using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementTotalsResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingMovementTotalsResponseEDIMessage>
	{
		public CDSInventoryLinkingMovementTotalsResponseEDIMessagePrettier(CDSInventoryLinkingMovementTotalsResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
				+ ToH3IfNotEmpty(Invariant($"An EMR / ERS sent by the Inventory Linking Component"))
				+ ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("Message Code", response.MessageCode),
						("CRC", response.Crc),
						("Goods Location", response.GoodsLocation),
						("Master UCR", response.MasterUcr),
						("Declaration Count", response.DeclarationCount),
						("Goods Arrival Date Time", response.GoodsArrivalDateTime),
						("Shed Operator", response.ShedOPID),
						("Movement Reference Number", response.MovementReference),
						("Master ROE", response.MasterROE),
						("Master SOE", response.MasterSOE)
					})
				+ GetEntryDetailsHtml();

			return interpretation;
		}

		ZString GetEntryDetailsHtml()
		{
			ZString interpretation = "";
			var entryIndex = ZInt.Zero;

			if (response.Entry.Any())
			{
				foreach (var entry in response.Entry)
				{
					interpretation = interpretation
					+ ToH4IfNotEmpty(Invariant($"Entry: {++entryIndex}"))
					+ ToKeyValuePairSection(new (ZString key, ZString value)[]
						{
							("UCR", entry.UCR),
							("UCR Type", entry.UCRType),
							("Submit Role", entry.SubmitRole),
							("Entry Status ICS", entry.ICS),
							("Entry Status ROE", InventoryLinkingStatusHelper.GetROEStatusDescription(entry.ROE)),
							("Entry Status SOE", InventoryLinkingStatusHelper.GetSOEStatusDescription(entry.SOE))
						})
					+ ToTableSection(ToStrongIfNotEmpty("Goods Items"), GetEntryLinesHtml(entry));
				}
			}
			return interpretation;
		}

		HtmlTableCreator GetEntryLinesHtml(InventoryLinkingEntry entry)
		{
			HtmlTableCreator tableCreator = null;

			if (entry.GoodsItem.Any())
			{
				tableCreator = GetHtmlTableCreator();
				tableCreator.WriteRow(ToStrongIfNotEmpty("Commodity Code"), ToStrongIfNotEmpty("Total Packages"), ToStrongIfNotEmpty("Total Net Mass"));

				foreach (var goodsItem in entry.GoodsItem)
				{
					tableCreator.WriteRow(goodsItem.CommodityCode, goodsItem.TotalPackages, goodsItem.TotalNetMass);
				}
			}
			return tableCreator;
		}

		readonly InventoryLinkingMovementTotalsResponse response;
	}
}
