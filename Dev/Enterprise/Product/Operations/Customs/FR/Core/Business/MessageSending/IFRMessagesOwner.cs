using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.EdiMessages;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public interface IFRMessagesOwner : IBusiness
	{
		FREDIMessageCollection Messages { get; }
	}
}
