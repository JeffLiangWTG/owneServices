using Enterprise.Customs.EU.Manifest.ICS2.Business;

namespace Enterprise.Customs.EU.Manifest
{
	public sealed class F22HeaderProvider : SendAndAmendHeader22Provider
	{
		public F22HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
