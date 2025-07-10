namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F23HeaderProvider : SendAndAmendHeader23Provider
	{
		public F23HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
