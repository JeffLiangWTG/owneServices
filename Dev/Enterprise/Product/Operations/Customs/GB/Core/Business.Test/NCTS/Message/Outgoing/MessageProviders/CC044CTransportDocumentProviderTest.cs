using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC044CTransportDocumentProvider))]
	sealed class CC044CTransportDocumentProviderTest : DocumentProviderAbstractTest<CC044CTransportDocumentProvider>
	{
		protected override string SubType => "REF";
	}
}
