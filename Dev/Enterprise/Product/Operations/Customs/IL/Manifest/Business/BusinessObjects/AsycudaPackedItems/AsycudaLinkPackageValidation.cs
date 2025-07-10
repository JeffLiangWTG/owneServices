namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaLinkPackageValidation : AutoAsycudaLinkPackageValidation
	{
		public AsycudaLinkPackageValidation(AutoAsycudaLinkPackage parent) : base(parent)
		{
			packedItem = ((AsycudaLinkPackage)parent).PackedItem;
		}
		readonly AsycudaPackedItem packedItem;

		protected override void CheckIsLinked()
		{
			if (!packedItem.AsycudaLinkPackages.HasAtLeastOneLinkedRecord)
			{
				Parent.AddRowMessageError(Constants.AsycudaLinkPackageValidation.SelectedPackageIsRequired);
			}
			else
			{
				Parent.RemoveRowMessageError(Constants.AsycudaLinkPackageValidation.SelectedPackageIsRequired);
			}

			if (packedItem.AsycudaLinkPackages.HasMoreThanOneLinkedRecord)
			{
				Parent.AddRowMessageError(Constants.AsycudaLinkPackageValidation.MoreThanOneSelectedPackageIsProhibited);
			}
			else
			{
				Parent.RemoveRowMessageError(Constants.AsycudaLinkPackageValidation.MoreThanOneSelectedPackageIsProhibited);
			}
		}
	}
}
