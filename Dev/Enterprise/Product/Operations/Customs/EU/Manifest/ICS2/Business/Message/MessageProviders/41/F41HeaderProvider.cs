namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F41HeaderProvider : SendAndAmendHeader41Provider
	{
		public F41HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
