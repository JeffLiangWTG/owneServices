namespace Enterprise.Customs.IT.Business;

interface IResponseMessageSubProcessor
{
	void ProcessMessage(IXmlCustomsLinkedObjectAdapter adapter, CargoWise.Customs.IT.MessageDefinitions.IResponseMessage responseMessageWrapper);
}
