namespace Enterprise.Customs.IT.Business;

public interface IOutgoingCustomsMessageCreationStrategy
{
	ITEDIMessage GenerateMessage();
}
