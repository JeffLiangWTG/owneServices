using IAidaXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

public interface IResponseMessageWithWrapper : IResponseMessage
{
	IAidaXmlResponseMessage GetResponseMessageContents();
}
