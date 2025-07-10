using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxPortConfigObj : AutoFaxPortConfigObj
	{
		public FaxPortConfigObj(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FaxPortConfigObjLookups Lookups
		{
			get { return lookups ?? (lookups = new FaxPortConfigObjLookups(this)); }
		}
		FaxPortConfigObjLookups lookups;
	}
}
