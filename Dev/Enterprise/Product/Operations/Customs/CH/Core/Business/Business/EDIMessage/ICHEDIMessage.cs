using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public interface ICHEDIMessage
{
	public BusinessObject EM_LinkedObject { get; }

	public IMessageDetail MessageDetail { get; }
}
