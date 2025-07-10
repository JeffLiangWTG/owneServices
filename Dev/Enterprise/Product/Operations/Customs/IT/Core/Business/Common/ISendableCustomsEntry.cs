using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ISendableCustomsEntry
{
	void PreProcessBeforeSending();
	void MarkAsSent(IMessageType sentMessage);
	void ConsumeGuarantee(BusinessObjectFactory factory, ITEDIMessage message);
	ZString CustomsProfile { get; }
}
