namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A26HeaderProvider : SendAndAmendHeader26Provider, IAmendedItemsProvider
	{
		public A26HeaderProvider(AsycudaManifestHeader header)
		: base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
