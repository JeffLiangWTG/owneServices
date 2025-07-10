using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentProductAreaCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SupportIncidentProductAreaCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertEquals(typeof(ReadOnlyCodeDescriptionPairList), CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().GetType());
		}
	}
}