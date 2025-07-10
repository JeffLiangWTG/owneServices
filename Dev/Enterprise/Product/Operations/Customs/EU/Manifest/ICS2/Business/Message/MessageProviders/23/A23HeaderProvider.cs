namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A23HeaderProvider : SendAndAmendHeader23Provider, IAmendedItemsProvider
	{
		public A23HeaderProvider(AsycudaManifestHeader header)
		: base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
