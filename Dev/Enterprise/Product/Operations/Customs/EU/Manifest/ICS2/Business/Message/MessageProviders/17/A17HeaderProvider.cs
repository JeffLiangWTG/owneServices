namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A17HeaderProvider : SendAndAmendHeader17Provider, IAmendedItemsProvider
	{
		public A17HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
