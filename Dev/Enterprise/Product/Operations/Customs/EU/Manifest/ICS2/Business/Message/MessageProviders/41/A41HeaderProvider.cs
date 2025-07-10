namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class A41HeaderProvider : SendAndAmendHeader41Provider, IAmendedItemsProvider
	{
		public A41HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		IAmendedItem[] IAmendedItemsProvider.AmendedItems { get; set; }
	}
}
