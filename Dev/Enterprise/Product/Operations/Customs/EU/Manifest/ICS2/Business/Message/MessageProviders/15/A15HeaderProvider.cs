namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A15HeaderProvider : SendAndAmendHeader15Provider, IAmendedItemsProvider
	{
		public A15HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		public IAmendedItem[] AmendedItems { get; set; }
	}
}
