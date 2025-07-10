using Enterprise.Customs.EU.Manifest.ICS2.Business;

namespace Enterprise.Customs.EU.Manifest
{
	public sealed class F15HeaderProvider : SendAndAmendHeader15Provider
	{
		public F15HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
