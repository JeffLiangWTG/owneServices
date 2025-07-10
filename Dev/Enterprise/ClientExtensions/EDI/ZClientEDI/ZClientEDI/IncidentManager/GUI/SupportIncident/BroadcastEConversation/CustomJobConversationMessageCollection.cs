using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class CustomJobConversationMessageCollection : NonPersistentBusinessObjectCollection<CustomJobConversationMessage>
	{
		public CustomJobConversationMessageCollection()
			   : base()
		{
		}

		public CustomJobConversationMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CustomJobConversationMessage(null);

		protected override bool AllowNewCore => false;
	}
}
