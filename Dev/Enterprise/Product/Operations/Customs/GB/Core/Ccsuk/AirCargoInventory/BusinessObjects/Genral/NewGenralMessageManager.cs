
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral
{
	public class NewGenralMessageManager
	{
		public NewGenralMessageManager(NonPersistentGenralEdiMessageForNew nonPersistentBO)
		{
			this.NonPersistentBO = nonPersistentBO;
		}

		public NewGenralMessageManager(NonPersistentGenralEdiMessageForNew nonPersistentBO, ICcsukCusAwb parent)
		{
			this.NonPersistentBO = nonPersistentBO;
			this.parent = parent;
		}

		public void ExecuteMakingRealEdiMessageFromNonPersistentHelper()
		{
			var message = GenralEdiMessage.MakeNewOutboundFromPayload(NonPersistentBO.Payload, NonPersistentBO.Pima, NonPersistentBO.Factory, NonPersistentBO.SendingProfile, NonPersistentBO.PreformattedLinesOf70);
			if (parent != null)
			{
				parent.Messages.Add(message);
			}
			NonPersistentBO.Factory.Save();
		}

		public NonPersistentGenralEdiMessageForNew NonPersistentBO { get; private set; }
		readonly ICcsukCusAwb parent;
	}
}
