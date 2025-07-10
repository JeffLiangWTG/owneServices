using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class CustomSupportIncidentCollection : NonPersistentBusinessObjectCollection<CustomSupportIncident>
	{
		public CustomSupportIncidentCollection()
			   : base()
		{
		}

		public CustomSupportIncidentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CustomSupportIncident(null);

		protected override bool AllowNewCore => false;
	}
}
