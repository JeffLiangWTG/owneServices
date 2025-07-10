using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentDispositionCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SupportIncidentDispositionCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertEquals(typeof(CodeDescriptionPairList), CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().GetType());
		}
	}
}