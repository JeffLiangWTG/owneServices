namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F40HeaderProvider : SendAndAmendHeader40Provider
	{
		public F40HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
