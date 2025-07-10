using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class Q05MessageSender
{
	public Q05MessageSender(AsycudaManifestHeader header)
	{
		ManifestHeader = Argument.NotNull(header, nameof(header));
	}

	AsycudaManifestHeader ManifestHeader { get; }

	public string SendMessage()
	{
		var result = Res.GetString("5FDC1697-929B-4EB6-AFE4-F634F9E9A1A2", "Failed to send message.");

		var provider = new Q05HeaderProvider(ManifestHeader);
		var messageBuilder = new Q05MessageBuilder(provider);

		var message = ManifestHeader.Factory.New<Q05OutboundEDIMessage>();
		message.EM_LinkedObject = ManifestHeader;

		message.EM_MessageType = MessageTypes.Codes.Q05;
		message.EM_MessageText = messageBuilder.GetXMLMessage();

		ManifestHeader.Messages.Add(message);

		try
		{
			ManifestHeader.Factory.Save();
			result = Res.GetString("6B2C183A-CC9C-4C5C-BA48-2F9B191D8AC1", "The message has been sent.");
		}
		catch (ZSaveException ex)
		{
			message.Delete();
			ZExceptionReporting.HandleSaveException(ex);
		}

		return result;
	}
}
