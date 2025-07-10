namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A14HeaderProvider : SendAndAmendHeader14Provider, IAmendedItemsProvider
	{
		public A14HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		public IAmendedItem[] AmendedItems { get; set; }
	}
}
