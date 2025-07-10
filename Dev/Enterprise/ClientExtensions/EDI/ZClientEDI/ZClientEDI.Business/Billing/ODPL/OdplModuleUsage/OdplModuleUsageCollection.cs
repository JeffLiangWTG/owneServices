using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	public class OdplModuleUsageCollection : NonPersistentBusinessObjectCollection<OdplModuleUsage>
	{
		public OdplModuleUsageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OdplModuleUsage(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

