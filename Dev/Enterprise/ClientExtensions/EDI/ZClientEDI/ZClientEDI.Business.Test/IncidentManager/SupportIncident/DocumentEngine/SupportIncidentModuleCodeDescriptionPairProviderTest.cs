using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentModuleCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SupportIncidentModuleCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertEquals(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().GetType(), typeof(CodeDescriptionPairList));
		}
	}
}