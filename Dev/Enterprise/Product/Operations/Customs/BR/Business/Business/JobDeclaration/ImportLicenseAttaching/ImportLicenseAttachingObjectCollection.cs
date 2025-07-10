using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseAttachingObjectCollection : NonPersistentBusinessObjectCollection<ImportLicenseAttachingObject>
	{
		public ImportLicenseAttachingObjectCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public override void Load()
		{
			foreach (var instruction in declaration.GetPossibleEntryInstructionForAttachment())
			{
				Add(new ImportLicenseAttachingObject(instruction));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Users cannot create a new element in the grid and ImportLicenseAttachingObject requires CustomsEntryInstructions");
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
