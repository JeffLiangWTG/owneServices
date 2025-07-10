using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentProductCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public void TestGetCodeDescriptionPairList_ConstainsEnterprise()
		{
			var provider = new SupportIncidentProductCodeDescriptionPairProvider();
			var list = provider.GetCodeDescriptionPairList();
			Assert("Should contain code of ediEnterprise", list.ContainsCode(ProductTypes.Codes.Enterprise));
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SupportIncidentProductCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var collection = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			Assert("collection is CodeDescriptionPairList", collection is CodeDescriptionPairList);
		}
	}
}