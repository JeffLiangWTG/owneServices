using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Registry;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public abstract class CINImportMessageBuilder
	{
		#region SuppressResourceStringsCheckRegion

		public enum MessageBuilderType
		{
			MovementIn,
			MovementOut,
			Correction,
			Deconsolidation
		}

		protected const string UTC_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";
		protected ZString nOT_CIN_LOCATION = "NOTCIN";

		protected CINImportMessageBuilder(ICINHeader header, string messageId)
		{
			this.header = header;
			this.messageId = messageId;
		}

		public static CINImportMessageBuilder Create(ICINHeader header, ICINHeader previousValues, string messageId, MessageBuilderType type)
		{
			switch (type)
			{
				case MessageBuilderType.MovementIn:
					return new CINWarehouseMovementInMessageBuilder(header, messageId);
				case MessageBuilderType.MovementOut:
					return new CINWarehouseMovementOutMessageBuilder(header, messageId);
				case MessageBuilderType.Correction:
					return new CINWarehouseMovementCorMessageBuilder(header, previousValues, messageId);
				case MessageBuilderType.Deconsolidation:
					return new CINWarehouseMovementDeconsMessageBuilder(header, messageId);
				default:
					throw new NotImplementedException(FormattableString.Invariant($"Message Type: {type} has not setup"));
			}
		}

		public ZString GetMessage()
		{
			var doc = CreateDocument();

			return new ZString(doc?.ToString() ?? "");
		}

		XDocument CreateDocument()
		{
			var doc = new XDocument();

			doc.Add(PopulateMessage());

			return doc;
		}

		XElement PopulateMessage()
		{
			var element = new XElement("Message");
			element.Add(
				PopulateMessageEnvelope(),
				PopulateCinMessage()
				);

			return element;
		}

		XElement PopulateMessageEnvelope()
		{
			var element = new XElement("EnvelopeMessage");
			var messageEnvelope = header.MessageEnvelope;
			element.Add(
				new XElement("schemaID", messageEnvelope.SchemaID),
				new XElement("schemaVersion", messageEnvelope.SchemaVersion),
				new XElement("transactionID", messageEnvelope.TransactionId)
				);
			return element;
		}

		XElement PopulateCinMessage()
		{
			var element = new XElement("CinMessage");
			element.Add(
				new XAttribute("type", MessageType),
				PopulateHeader(),
				PopulateMessageBody()
			);
			return element;
		}

		XElement PopulateHeader()
		{
			var element = new XElement("Header");

			element.Add(
				new XAttribute("from", FRCustomsDataRegistry.Instance.CINSenderID.Value),
				new XAttribute("to", "CIN"),
				new XAttribute("messageTime", header.MovementTime.ToString(UTC_FORMAT, CultureInfo.InvariantCulture)),
				new XAttribute("messageId", messageId)
			);

			return element;
		}

		protected abstract string MessageType { get; }
		protected abstract string MessageBodyTag { get; }

		XElement PopulateMessageBody()
		{
			var element = new XElement(MessageBodyTag);

			element.Add(
				new XElement("movementTime", header.MovementTime.ToString(UTC_FORMAT, CultureInfo.InvariantCulture)),
				PopulateCodeLabelPair("declaredIn", header.CurrentLocation, header.CurrentLocation),
				GetMessageBodyElements
				);

			return element;
		}

		#region Elements
		protected abstract IEnumerable<XElement> GetMessageBodyElements { get; }

		protected virtual ZString FromLocation => header.CurrentLocation;
		protected virtual ZString FromLocationLabel => header.CurrentLocation;
		protected XElement PopulateFromElement()
		{
			return PopulateCodeLabelPair("from", FromLocation, FromLocationLabel);
		}

		protected virtual ZString ToLocation => header.NewLocation;
		protected virtual ZString ToLocationLabel => header.NewLocation;
		protected XElement PopulateToElement()
		{
			return PopulateCodeLabelPair("to", ToLocation, ToLocationLabel);
		}

		protected XElement PopulateCustomsStatus()
		{
			return new XElement("customsStatus", header.CustomsStatus);
		}

		protected XElement PopulateDescription(ICINLine line)
		{
			return new XElement("description", line.DescriptionOfGoods);
		}

		protected IEnumerable<XElement> PopulateCustomsDocuments()
		{
			return header?.CustomsDocuments?.Select(x => PopulateTypeRefPair("customsDocument", x.Type, x.Reference));
		}

		protected XElement PopulateDetailedGoods(ICINLine line)
		{
			var element = PopulateGoods("goods", line);

			element.Add(
				PopulateDescription(line),
				PopulateQuantityWeightPair("totalRefAmount", line.TotalNoPieces, line.TotalMass)
				);

			return element;
		}

		protected XElement PopulateToGoods(IEnumerable<ICINLine> lines)
		{
			return PopulateGoodsGroup("toGoods", lines);
		}

		protected XElement PopulateFromGoods(ICINLine line)
		{
			return PopulateGoods("fromGoods", line);
		}

		protected XElement PopulateFromGoods(IEnumerable<ICINLine> lines)
		{
			return PopulateGoodsGroup("fromGoods", lines);
		}

		protected IEnumerable<XElement> PopulateGoodsLines(IEnumerable<ICINLine> lines)
		{
			return lines.Select(x => PopulateGoods("goods", x));
		}
		#endregion

		#region Helpers/Builders
		XElement PopulateGoods(string elementTag, ICINLine line)
		{
			var element = new XElement(elementTag);

			element.Add(
				PopulateTypeCodePair("ref", line.Type, line.ReferenceNumber),
				PopulateQuantityWeightPair("amount", line.NoPieces, line.Mass)
			);

			return element;
		}

		XElement PopulateGoodsGroup(string elementTag, IEnumerable<ICINLine> lines)
		{
			var element = new XElement(elementTag);

			element.Add(PopulateGoodsLines(lines));

			return element;
		}

		XElement PopulateCodeLabelPair(string elementTag, string code, string label)
		{
			return PopulateElementValuePair(elementTag, "code", code, "label", label);
		}

		XElement PopulateTypeCodePair(string elementTag, string type, string code)
		{
			return PopulateElementValuePair(elementTag, "type", type, "code", code);
		}

		XElement PopulateTypeRefPair(string elementTag, string type, string reference)
		{
			return PopulateElementValuePair(elementTag, "type", type, "ref", reference);
		}

		XElement PopulateQuantityWeightPair(string elementTag, int quantity, decimal weight)
		{
			return PopulateElementValuePair(elementTag, "quantity", quantity, "weight", weight.ToString("0.000", CultureInfo.InvariantCulture));
		}

		XElement PopulateElementValuePair(string elementTag, string codeTag, object code, string valueTag, object value)
		{
			var element = new XElement(elementTag);

			element.Add(
				new XAttribute(codeTag, code),
				new XAttribute(valueTag, value)
				);

			return element;
		}
		#endregion

		protected ICINLine AirWayBill
		{
			get
			{
				return header.Bills.FirstOrDefault(x => x.IsAirwayBill);
			}
		}

		protected IEnumerable<ICINLine> OtherBills
		{
			get
			{
				return header.Bills.Where(x => !x.IsAirwayBill);
			}
		}

		protected readonly ICINHeader header;
		readonly string messageId;

		#endregion
	}
}
