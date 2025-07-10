using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC044CTransportDocumentProvider))]
	sealed class CC044CTransportDocumentProviderTest : DocumentProviderAbstractTest<CC044CTransportDocumentProvider>
	{
		protected override string SubType => "REF";
	}
}
