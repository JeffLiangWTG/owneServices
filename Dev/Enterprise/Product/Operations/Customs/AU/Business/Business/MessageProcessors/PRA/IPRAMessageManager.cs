using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IPRAMessageManager
	{
		string LastMessageTypeSent { get; }

		EDIMessageCollection Messages { get; }
		BusinessObject Container { get; }
		BusinessObject Parent { get; }
		ContainerMessagingData ContainerMessagingData { get; }

		ZDateTime DepartureDate { get; }
		ZString JobType { get; }
		ZString JobReference { get; set; }
		ZString ContainerNumber { get; set; }
	}
}
