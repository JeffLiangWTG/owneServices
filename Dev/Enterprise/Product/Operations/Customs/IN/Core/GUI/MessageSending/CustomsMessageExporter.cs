using System.IO;
using CargoWise.Common;
using Enterprise.Customs.IN.Business;
using Enterprise.Messaging.Business;
using EDIMessage = Enterprise.Customs.IN.Business.EDIMessage;

namespace Enterprise.Customs.IN.GUI;

static class CustomsMessageExporter
{
	public static string SaveToFile(EDIMessage message, string messageDownloadFolder)
	{
		Argument.NotNull(message, nameof(message));
		Argument.NotNullOrEmpty(messageDownloadFolder, nameof(messageDownloadFolder));

		var fileExtension = GetFileExtension(message);
		var fileName = message.EM_MessageNum;
		var filePath = Path.Combine(messageDownloadFolder, fileName + fileExtension);
		var count = 1;
		while (File.Exists(filePath))
		{
			filePath = Path.Combine(messageDownloadFolder, $"{fileName} ({count++}){fileExtension}");
		}

		using (var stream = File.OpenWrite(filePath))
		using (var writer = new StreamWriter(stream))
		{
			writer.AddStream(message.GetEM_MessageTextReader());
			writer.Flush();
		}
		return filePath;
	}

	static string GetFileExtension(EDIMessage message)
	{
		return (string)message.EM_MessageSubType switch
		{
			EDIMessageSubTypeList.Codes.GoodsRegistration => Constants.MessageDownloadFileExtension.GoodRegistration,
			_ => "." + message.EM_MessageType,
		};
	}
}
