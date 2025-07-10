using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class PNTSCommunicationProvider : ICommunication
{
	public PNTSCommunicationProvider(string communicationValue, string communicationType)
	{
		this.communicationValue = communicationValue;
		this.communicationType = communicationType;
	}
	readonly string communicationValue;
	readonly string communicationType;

	public string Identifier => communicationValue;

	public string Type => communicationType;
}
