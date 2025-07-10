using IAidaXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.ITemporaryStorageResponseMessage;

namespace Enterprise.Customs.IT.Business;

public interface ITemporaryStorageResponseMessageWithWrapper : IResponseMessage
{
	IAidaXmlResponseMessage GetResponseMessageContents();
}
