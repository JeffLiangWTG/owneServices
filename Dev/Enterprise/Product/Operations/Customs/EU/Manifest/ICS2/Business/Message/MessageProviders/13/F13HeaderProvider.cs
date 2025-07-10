namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F13HeaderProvider : SendAndAmendHeader13Provider
	{
		public F13HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
