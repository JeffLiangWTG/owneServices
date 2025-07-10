namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class F51HeaderProvider : SendAndAmendHeader51Provider
	{
		public F51HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
