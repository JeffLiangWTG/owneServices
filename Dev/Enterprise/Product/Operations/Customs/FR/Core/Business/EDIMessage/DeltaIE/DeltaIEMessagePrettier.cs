using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class DeltaIEMessagePrettier : FREDIMessagePrettier
	{
		public DeltaIEMessagePrettier(DeltaIEMessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		public new DeltaIEMessageDataObject MessageDataObject => (DeltaIEMessageDataObject)base.MessageDataObject;

		public override ZString GetMessageInterpretation()
		{
			var result = ZString.Empty;

			var messageText = MessageDataObject.MessageText;
			if (string.IsNullOrEmpty(messageText))
			{
				result = ZString.Empty;
			}
			else
			{
				ZString htmlFormattedOutput;
				try
				{
					var jsonElement = JsonSerializer.Deserialize<JsonElement>(messageText);
					var formattedJson = JsonSerializer.Serialize(jsonElement, SerializeOptions);
					htmlFormattedOutput = formattedJson.Replace("\r\n", "<br>").Replace("  ", "&emsp;");
				}
				catch (Exception ex)
				{
					htmlFormattedOutput = $@"{jsonError}: {ex.Message}<br><br>{messageText}";
				}

				var css = GetEmbeddedResourceFile("json-viewer.css");
				result = $@"<!doctype html>
<html>
	<head>
		<style>
			{css}
		</style>
	</head>
	<body>
		<div>
			<span class=""json-viewer"">{htmlFormattedOutput}</span>
		</div>
	</body>
</html>";
			}

			return result;
		}

		static ZString GetEmbeddedResourceFile(ZString fileName)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.FR.Business.EDIMessage." + fileName))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd().TrimEnd(System.Environment.NewLine.ToCharArray());
			}
		}

		static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions { WriteIndented = true };

		readonly ZString jsonError = ResString.GetMultilingualString("7E8F36A1-B876-47BE-B2B6-720E7C5E0211", "Message content is badly formatted.");
	}

	public abstract class DeltaIEMessagePrettier<TMessage> : UCCMessagePrettier<TMessage>
		where TMessage : class
	{
		protected DeltaIEMessagePrettier(DeltaIEMessageDataObject<TMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public new DeltaIEMessageDataObject<TMessage> MessageDataObject => (DeltaIEMessageDataObject<TMessage>)base.MessageDataObject;
	}
}
