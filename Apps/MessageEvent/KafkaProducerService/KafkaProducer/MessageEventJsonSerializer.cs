using System;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.KafkaProducer
{
	public class MessageEventJsonSerializer : ISerializer<XmlDocument>
	{
		private readonly ILogger<MessageEventJsonSerializer> _logger;

		public MessageEventJsonSerializer(ILogger<MessageEventJsonSerializer> logger)
		{
			_logger = logger;
		}

		public byte[] Serialize(XmlDocument? data, SerializationContext context)
		{
			if (data == null)
			{
				return new byte[] { };
			}

			var jsonObject = new JsonObject();
			jsonObject["@timestamp"] = data.SelectSingleNode("MessageEvent/@timestamp")?.InnerText ?? DateTime.UtcNow.ToString("O");
			jsonObject["MessageEvent"] = ParseXmlToJsonObject(data["MessageEvent"]!);

			JsonObject ParseXmlToJsonObject(XmlNode node)
			{
				var json = new JsonObject();
				foreach (XmlNode child in node.ChildNodes)
				{
					if (!child.HasChildNodes || child.FirstChild is XmlText)
					{
						json[child.Name] = child.InnerText ?? "";
					}
					else
					{
						json[child.Name] = ParseXmlToJsonObject(child);
					}
				}
				return json;
			}

			var json = jsonObject.ToJsonString();
			_logger.eHubLog(LogLevel.Trace, "", "", "", "Serialized value: {json}", json);
			return Encoding.UTF8.GetBytes(json);
		}
	}
}
