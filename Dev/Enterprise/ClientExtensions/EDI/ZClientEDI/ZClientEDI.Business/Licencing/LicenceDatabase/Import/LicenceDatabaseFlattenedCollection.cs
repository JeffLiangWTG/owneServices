using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseFlattenedCollection : NonPersistentBusinessObjectCollection<LicenceDatabaseFlattened>
	{
		public LicenceDatabaseFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LicenceDatabaseFlattened();
		}
	}
}

