using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiLicenceSettingFlattenedCollection : NonPersistentBusinessObjectCollection<EdiLicenceSettingFlattened>
	{
		public EdiLicenceSettingFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EdiLicenceSettingFlattened();
		}
	}
}


