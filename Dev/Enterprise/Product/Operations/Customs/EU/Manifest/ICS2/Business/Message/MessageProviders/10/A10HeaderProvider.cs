using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend10;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A10HeaderProvider : SendAndAmendHeader10Provider, ISendAndAmendHeaderA10, IAmendedItemsProvider
	{
		public A10HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		public string ReferralRequestReference => helper.GetReferralRequestReference(this);

		public IAmendedItem[] AmendedItems { get; set; }
	}
}
