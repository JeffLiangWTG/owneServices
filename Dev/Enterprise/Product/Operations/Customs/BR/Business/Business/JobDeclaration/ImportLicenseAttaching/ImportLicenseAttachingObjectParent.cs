using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseAttachingObjectParent : NonPersistentBusinessObject
	{
		public ImportLicenseAttachingObjectParent(JobDeclaration declaration) : base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		public readonly JobDeclaration Declaration;

		public ImportLicenseAttachingObjectCollection ImportLicenses
		{
			get
			{
				if (fImportLicenses == null)
				{
					fImportLicenses = new ImportLicenseAttachingObjectCollection(Declaration);
					fImportLicenses.Load();
					RegisterEditableChildObject(fImportLicenses);
				}
				return fImportLicenses;
			}
		}

		ImportLicenseAttachingObjectCollection fImportLicenses;
	}
}
