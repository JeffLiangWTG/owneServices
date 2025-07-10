namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A13HeaderProvider : SendAndAmendHeader13Provider, IAmendedItemsProvider
	{
		public A13HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		public IAmendedItem[] AmendedItems { get; set; }

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);
	}
}
