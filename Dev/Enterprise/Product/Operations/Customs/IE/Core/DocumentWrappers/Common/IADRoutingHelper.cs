using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.Business;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;
using EDIMessage = Enterprise.Customs.IE.Business.EDIMessage;
using UCC5IM460 = CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.Im460;
using UCC6IM460 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM460.Im460;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	public static class IADRoutingHelper
	{
		public static ZString GetRouting(CusEntryHeader entryHeader)
		{
			var result = ZString.Empty;

			var ediMessages = entryHeader.Messages.Cast<EDIMessage>();

			if (HasIM429Message(ediMessages))
			{
				result = RoutingMapping.Green.ToUpper();
			}
			else
			{
				var lastIM460Message = ediMessages
					.Where(message =>
						message.EM_Status.EqualsIgnoringCase(EDIMessage.Status.ProcessedOK)
						&& message.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Receive)
						&& message.EM_MessageType.EqualsIgnoringCase(Enterprise.Customs.IE.Messaging.AISInterchangeTypeList.Codes.IM460)
					)
					.OrderByDescending(message => message.EM_SystemCreateTimeUtc)
					.FirstOrDefault();

				if (lastIM460Message != null)
				{
					var controlTypeXPath = $"//*[local-name()='{nameof(UCC5IM460.OverallControlType)}']/*[local-name()='{nameof(UCC5IM460.OverallControlType.ControlTypeCoded)}']";
					var controlTypeXPathUcc6 = $"//*[local-name()='{nameof(UCC6IM460.OverallControlType)}']/*[local-name()='{nameof(UCC6IM460.OverallControlType.ControlTypeCoded)}']";

					var document = new XmlDocument();
					document.Load(lastIM460Message.GetEM_MessageTextReader());
					if ((document.SelectSingleNode(controlTypeXPath) ?? document.SelectSingleNode(controlTypeXPathUcc6)) is XmlNode controlTypeCodedNode)
					{
						result = RoutingMapping.Get(controlTypeCodedNode.InnerText);
					}
				}
			}
			return result;

			static bool HasIM429Message(IEnumerable<EDIMessage> messages) => messages
				.Any(message =>
					message.EM_Status.EqualsIgnoringCase(EDIMessage.Status.ProcessedOK)
					&& message.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Receive)
					&& message.EM_MessageType.EqualsIgnoringCase(Enterprise.Customs.IE.Messaging.AISInterchangeTypeList.Codes.IM429)
				);
		}

		static class RoutingMapping
		{
			public const string Green = nameof(Green);
			public const string Orange = nameof(Orange);
			public const string Red = nameof(Red);

			const string MessageValueGreen = "G";
			const string MessageValueOrange = "O";
			const string MessageValueRed = "R";

			public static string Get(string messageValue) => (messageValue.Trim().ToUpper() switch
			{
				MessageValueGreen => Green,
				MessageValueOrange => Orange,
				MessageValueRed => Red,
				_ => string.Empty,
			}).ToUpper();
		}
	}
}
