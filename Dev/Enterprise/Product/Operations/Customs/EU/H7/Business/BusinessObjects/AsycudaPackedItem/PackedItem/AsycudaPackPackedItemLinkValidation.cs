namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackPackedItemLinkValidation : AutoAsycudaPackPackedItemLinkValidation
	{
		public AsycudaPackPackedItemLinkValidation(AutoAsycudaPackPackedItemLink parent) : base(parent)
		{
			packedItem = ((AsycudaPackPackedItemLink)parent).PackedItem;
		}

		protected override void CheckIsLinked()
		{
			var message = Res.GetString("77499104-d3c0-4155-80d1-6f67bba13f63", "At least one selected package is required.");

			if (!packedItem.AsycudaPackPackedItemLinks.HasAtLeastOneLinkedRecord)
			{
				Parent.AddRowMessageError(message);
			}
			else
			{
				Parent.RemoveRowMessageError(message);
			}
		}

		readonly AsycudaPackedItem packedItem;
	}
}
