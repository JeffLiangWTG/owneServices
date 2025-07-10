using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObjectCollection : NonPersistentBusinessObjectCollection<NFEImportObject>
	{
		public NFEImportObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NFEImportObject(Factory);
		}

		protected override bool AllowNewCore => false;
	}
}
