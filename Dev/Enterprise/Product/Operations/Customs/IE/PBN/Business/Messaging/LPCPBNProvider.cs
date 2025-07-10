using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging;

public class LPCPBNProvider
{
	public LPCPBNProvider(LPCDefinition jsonObject)
	{
		this.jsonObject = jsonObject;
		if (jsonObject?.PairedTransport != null)
		{
			PairedTransport = new PBNPairedTransportProvider(jsonObject.PairedTransport);
		}
	}

	public ZString PbnID => jsonObject.PbnID ?? ZString.Empty;

	public ZString Channel => jsonObject.Channel ?? ZString.Empty;

	public ZString Action => jsonObject.Action ?? ZString.Empty;

	public PBNPairedTransportProvider PairedTransport;

	readonly LPCDefinition jsonObject;
}
