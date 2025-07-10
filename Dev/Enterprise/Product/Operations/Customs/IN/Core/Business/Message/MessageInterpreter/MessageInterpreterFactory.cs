using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IN.Business;

static class MessageInterpreterFactory
{
	public static IMessageInterpreter GetINMessageInterpreter(EDIMessage message)
	{
		switch (message?.EM_ReceiveTransmit)
		{
			case EDIMessage.Direction.Receive:
				return new ReceiveMessageInterpreter(message);
			case EDIMessage.Direction.Transmit:
				return new TransmitMessageInterpreter(message);
			default:
				return null;
		}
	}

	public static string GetResponseDescription(BusinessObjectFactory factory, string errorCode)
	{
		var errorDescription = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCodeDescription, ZDateTime.Today).GetDescriptionFromCode(errorCode);
		return errorDescription.IsNullOrEmpty() ? "Response code description not found" : errorDescription;
	}
}
