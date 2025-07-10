using System;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.eHubMessaging.Business.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.CanadianCustoms)]
	public class CanadianCustomsReplyHandler : MessageHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xml field name")]
		protected override EDIInterchange CreateInterchange()
		{
			Message.MessageStream.SeekBegin();

			using (var reader = XmlReader.Create(Message.MessageStream))
			{
				var reference = reader.GetElementAsString("Reference", "http://cargowise.com/ehub/products/canadiancustoms");
				using (var interchangeStream = reader.GetElementAsStream("Content", "http://cargowise.com/ehub/products/canadiancustoms"))
				{
					var interchange = base.CreateInterchange();
					if (!string.IsNullOrEmpty(Message.EmailSubject) && Message.EmailSubject == "CAD")
					{
						interchange.EI_From = Message.SenderID;
						interchange.EI_To = Message.RecipientID;
					}
					else
					{
						var referenceParts = ParseReference(reference);
						interchange.EI_To = referenceParts.Item1;
						interchange.EI_From = referenceParts.Item2;
					}

					interchangeStream.Position = 0;
					var decodedInterchangeStream = interchangeStream.DecodeStream();
					interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.CanadianCustoms;
					interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
					interchange.SetEI_BodyTextSource(new TextReaderSource(decodedInterchangeStream));

					return interchange;
				}
			}
		}

		static Tuple<string, string> ParseReference(string reference)
		{
			var parts = reference.Split(new [] { " - " }, StringSplitOptions.None);
			if (parts.Length != 2)
			{
				throw new InvalidOperationException(string.Format("Invalid Canadian customs reference format '{0}'. Should be 'sender - recipient'", reference));
			}

			return new Tuple<string, string>(parts[0], parts[1]);
		}

		public static string GetCanadianCustomsEHubId(string recipientId)
		{
			const string canadianCustomsEHubId = "CACustoms";
			const string canadianCustomsEHubIdTest = "CACustomsTest";

			if (string.IsNullOrWhiteSpace(recipientId))
			{
				return canadianCustomsEHubIdTest;
			}

			var recipientIdCharArray = recipientId.ToCharArray();
			return recipientIdCharArray[recipientIdCharArray.Length - 1] != 'P' ? canadianCustomsEHubIdTest : canadianCustomsEHubId;
		}
	}
}
