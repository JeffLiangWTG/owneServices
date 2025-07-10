using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxPortConfigObjCollection : NonPersistentBusinessObjectCollection<FaxPortConfigObj>
	{
		public FaxPortConfigObjCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FaxPortConfigObj(Factory);
		}

		#endregion
	}
}
