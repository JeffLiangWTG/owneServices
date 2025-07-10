namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A24HeaderProvider : SendAndAmendHeader24Provider, IAmendedItemsProvider
	{
		public A24HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
