namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F43HeaderProvider : SendAndAmendHeader43Provider
	{
		public F43HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
