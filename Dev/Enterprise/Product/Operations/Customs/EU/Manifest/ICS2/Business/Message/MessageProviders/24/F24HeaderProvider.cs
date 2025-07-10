namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F24HeaderProvider : SendAndAmendHeader24Provider
	{
		public F24HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
