using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObjectCollection : NonPersistentBusinessObjectCollection<ImportLicenseLoadingObject>
	{
		public ImportLicenseLoadingObjectCollection(ImportLicenseLoadingObjectParent parent)
			: base(parent.Factory)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		public ImportLicenseLoadingObjectParent Parent { get; private set; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ImportLicenseLoadingObject(Parent);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
