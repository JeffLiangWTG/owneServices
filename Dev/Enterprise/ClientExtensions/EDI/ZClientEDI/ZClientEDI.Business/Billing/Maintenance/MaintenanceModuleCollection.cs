using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenanceModuleCollection : NonPersistentBusinessObjectCollection<MaintenanceModule>
	{
		public MaintenanceModuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MaintenanceModule();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}


