namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A50HeaderProvider : SendAndAmendHeader50Provider, IAmendedItemsProvider
	{
		public A50HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}
		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		public IAmendedItem[] AmendedItems { get; set; }
	}
}
