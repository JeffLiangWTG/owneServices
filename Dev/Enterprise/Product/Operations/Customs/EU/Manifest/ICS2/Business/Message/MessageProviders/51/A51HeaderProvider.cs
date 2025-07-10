namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A51HeaderProvider : SendAndAmendHeader51Provider, IAmendedItemsProvider
	{
		public A51HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}
		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		public IAmendedItem[] AmendedItems { get; set; }
	}
}
