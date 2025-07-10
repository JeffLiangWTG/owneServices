namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A16HeaderProvider : SendAndAmendHeader16Provider, IAmendedItemsProvider
	{
		public IAmendedItem[] AmendedItems { get; set; }

		public A16HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore()
		{
			return helper.GetReferralRequestReference(this);
		}

		protected override bool UsesPlaceHolder => true;
	}
}
