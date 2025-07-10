using System;
using System.Text;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.GenericMessageDelivery)]
	public class GenericMessageDeliveryMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;

			Message.MessageStream.SeekBegin();
			GetElements(out var headerContent, out var interchangeType, out var bodyContent, out var interchangeNumber);

			if (InterchangeExists(interchange.EI_From, interchange.EI_To, interchangeNumber))
			{
				var duplicateMessage = Res.GetString("bb28ec86-2c0b-4764-9c0e-4e7199997884",
					"The value of {0} + {1} + {2} must be unique on {3}. The duplicate value(s) are: ({4}, {5}, {6}).",
					"Interchange Number",
					"From",
					"To",
					"EDIInterchange",
					interchangeNumber,
					interchange.EI_From,
					interchange.EI_To);
				interchange.Notes.AddNew(true, Res.GetString("e886dec6-81e6-4148-978d-7d234944814d", "eHub: Duplication Detected"), duplicateMessage);
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
				interchangeNumber = "";
			}

			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_HeaderText = headerContent;
			interchange.EI_BodyText = bodyContent;

			interchange.NumberStrategy = new GenericMessageNumberStrategy(interchange.Factory, interchangeNumber);

			return interchange;
		}

		void GetElements(out string headerContent, out string interchangeType, out string bodyContent, out string interchangeNumber)
		{
			using (var reader = XmlReader.Create(Message.MessageStream))
			{
				reader.MoveToContent();
				var doc = new XmlDocument();
				var xmlContent = $@"<root>{reader.ReadInnerXml()}</root>";
				doc.LoadXml(xmlContent);
				headerContent = GetNodeString(doc, ConvertToXPath("root/Header"));
				bodyContent = GetNodeString(doc, ConvertToXPath("root/Body"));
				interchangeType = GetNodeString(doc, ConvertToXPath("root/Header/InterchangeType"));
				interchangeNumber = GetNodeString(doc, ConvertToXPath("root/Header/InterchangeNumber"));
			}
		}

		string GetNodeString(XmlDocument doc, string xPath)
		{
			var node = doc.SelectSingleNode(xPath);
			var result = node?.InnerXml ?? "";
			return result;
		}

		string ConvertToXPath(string xmlPath)
		{
			const StringSplitOptions options = new StringSplitOptions();
			var delimiters = new[] { "/" };
			var arrS = xmlPath.Split(delimiters, options);
			var sb = new StringBuilder("/");

			foreach (var str in arrS)
			{
				sb.Append($"/*[local-name()='{str}']");
			}

			return sb.ToString();
		}

		bool InterchangeExists(string senderID, string recipientID, string interchangeNumber)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, interchangeNumber);
			query.AddToFilter(EDIInterchangeSchema.EI_From, senderID);
			query.AddToFilter(EDIInterchangeSchema.EI_To, recipientID);
			return FactoryProvider.Current.ExistsInDatabase("EDIInterchange", query);
		}
	}
}
