using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseResponseObjectCollection : NonPersistentBusinessObjectCollection<ImportLicenseResponseObject>
	{
		public ImportLicenseResponseObjectCollection(BusinessObjectFactory factory)
		: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ImportLicenseResponseObject(Factory);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
