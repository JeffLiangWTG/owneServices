namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageLinkPackageValidation : AutoTemporaryStorageLinkPackageValidation
	{
		public TemporaryStorageLinkPackageValidation(AutoTemporaryStorageLinkPackage parent) : base(parent)
		{
			packedItem = ((TemporaryStorageLinkPackage)parent).PackedItem;
		}
		readonly TemporaryStoragePackedItem packedItem;

		protected override void CheckIsLinked()
		{
			var message = Res.GetString("01A73ECF-650C-4B83-9353-0C54D120A02A", "At least one selected package is required.");

			if (!packedItem.TemporaryStorageLinkPackages.HasAtLeastOneLinkedRecord)
			{
				Parent.AddRowMessageError(message);
			}
			else
			{
				Parent.RemoveRowMessageError(message);
			}
		}
	}
}
