namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F26HeaderProvider : SendAndAmendHeader26Provider
	{
		public F26HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
