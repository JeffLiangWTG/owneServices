using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(AIRSWebPageNaviagtorForm))]
	sealed class AIRSWebPageNaviagtorFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var urlFra = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageFra;
			var urlEng = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng;
			AIRSHtmlHelperTest.SetupRefSysConfigType(Factory, urlFra, urlEng);
			Factory.Save();
			var bo = new AIRSWebpageNavigator(Factory, "803010");
			return new AIRSWebPageNaviagtorForm(bo);
		}
	}
}
