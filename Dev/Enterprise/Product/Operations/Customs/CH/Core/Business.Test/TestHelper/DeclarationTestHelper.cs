using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

static class DeclarationTestHelper
{
	internal static void DoMergeForTesting(this JobDeclaration declaration)
	{
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.DoMerge();
	}
}
