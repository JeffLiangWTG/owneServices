namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F44HeaderProvider : SendAndAmendHeader44Provider
	{
		public F44HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
