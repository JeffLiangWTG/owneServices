namespace Enterprise.Customs.IT.Business;

interface IResponseMessageHandler
{
	void Handle(CargoWise.Customs.IT.MessageDefinitions.IResponseMessage responseMessage);
}
