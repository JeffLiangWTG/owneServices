using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingMovementResponseEDIMessage>
	{
		public CDSInventoryLinkingMovementResponseEDIMessagePrettier(CDSInventoryLinkingMovementResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
				+ ToH3IfNotEmpty(Invariant($"A response to a movement request message of type (EAL, EAA, EDL)"))
				+ ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("Message Code", response.MessageCode),
						("CRC", response.Crc),
						("Goods Arrival Date Time", response.GoodsArrivalDateTime),
						("Goods Location", response.GoodsLocation),
						("Shed Operator", response.ShedOPID),
						("Movement Reference Number", response.MovementReference),
						("Submit Role", response.SubmitRole),
						("UCR", response.UCR),
						("UCR Type", response.UCRType),
						("Entry Status ICS", response.ICS),
						("Entry Status ROE", InventoryLinkingStatusHelper.GetROEStatusDescription(response.ROE)),
						("Entry Status SOE", InventoryLinkingStatusHelper.GetROEStatusDescription(response.SOE))
					})
				+ ToTableSection(ToStrongIfNotEmpty("Goods Items"), GoodsItemTableCreator);

			return interpretation;
		}

		HtmlTableCreator GoodsItemTableCreator
		{
			get
			{
				HtmlTableCreator tableCreator = null;

				if (response.GoodsItem.Any())
				{
					tableCreator = GetHtmlTableCreator();
					tableCreator.WriteRow(ToStrongIfNotEmpty("Commodity Code"), ToStrongIfNotEmpty("Total Packages"), ToStrongIfNotEmpty("Total Net Mass"));

					foreach (var goodsItem in response.GoodsItem)
					{
						tableCreator.WriteRow(goodsItem.CommodityCode, goodsItem.TotalPackages, goodsItem.TotalNetMass);
					}
				}
				return tableCreator;
			}
		}

		readonly InventoryLinkingMovementResponse response;
	}
}
