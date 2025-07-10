using Enterprise.Customs.EU.Manifest.ICS2.Business;

namespace Enterprise.Customs.EU.Manifest
{
	public sealed class A22HeaderProvider : SendAndAmendHeader22Provider, IAmendedItemsProvider
	{
		public A22HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override string GetReferralRequestReferenceCore() => helper.GetReferralRequestReference(this);

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
