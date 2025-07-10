using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	[CodeAlive("Will be used in the next workflow")]
	public sealed class F25HeaderProvider : SendAndAmendHeader25Provider
	{
		public F25HeaderProvider(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override bool UsesPlaceHolder => true;
	}
}
