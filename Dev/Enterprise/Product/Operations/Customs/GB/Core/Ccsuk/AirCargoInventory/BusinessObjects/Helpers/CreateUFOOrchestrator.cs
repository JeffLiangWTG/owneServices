using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Validators;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CreateUFOOrchestrator
	{
		public CreateUFOOrchestrator(CusMAWB mawb)
		{
			Mawb = mawb;
		}

		public bool SendFRIMessageForUFO(ISendsMessagesToCustoms initiator)
		{
			var manager = new CcsukInventoryMessageManager((BusinessObject)Mawb, new CcsukTransmissionMessageFunction.CUSCAR.FRI.UFO(), initiator);
			var ufoValidator = new CARFRIUFOMessageValidator(Mawb);
			var notifications = ufoValidator.Validate();
			if (notifications.Count > 0)
			{
				initiator.NotifyUserOfAnInvalidOperation(notifications.ErrorNotificationsAsString());
				return false;
			}
			else
			{
				return manager.SendToCommunity();
			}
		}

		public readonly CusMAWB Mawb;
	}
}
