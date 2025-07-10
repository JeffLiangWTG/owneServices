namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F14HeaderProvider : SendAndAmendHeader14Provider
	{
		public F14HeaderProvider(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
