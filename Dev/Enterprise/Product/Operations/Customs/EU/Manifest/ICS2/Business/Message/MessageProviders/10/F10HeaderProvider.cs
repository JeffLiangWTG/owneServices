namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F10HeaderProvider : SendAndAmendHeader10Provider
	{
		public F10HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
