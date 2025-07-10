using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing;

#if !WINZOR
[TestedType(typeof(AIRSWebpageNavigator))]
[DatCapabilityRequirement("SOURCE_CODE")]
sealed class AIRSWebpageNavigatorTest : NonPersistentBusinessObjectTestCase
{
	public void TestInitializeProperties()
	{
		var navigator = new AIRSWebpageNavigator(Factory, "803010");
		AssertEquals(ZString.Empty, navigator.AG_EndUseCode);
		AssertEquals(ZString.Empty, navigator.AG_ExtensionCode);
		AssertEquals(ZString.Empty, navigator.AG_Miscellaneous);
		AssertEquals("803010", navigator.TariffCodePassedIn);
		AssertEquals(0, navigator.LPCOList.Count);
		using (var browser = new ZWebBrowser())
		{
			browser.Navigate("about:blank");
			browser.Document.Write(File.ReadAllText(navigator.WebPageConfiguration.Url));
			AIRSSelectionLoader.InitializeAIRSNavigator(browser.Document, navigator);
			AssertEquals("53", navigator.AG_EndUseCode);
			AssertEquals("400701", navigator.AG_ExtensionCode);
			AssertEquals("123", navigator.AG_Miscellaneous);
			AssertEquals(2, navigator.LPCOList.Count);
			var lpco = navigator.LPCOList[0];

			Assert(lpco.MaterializedLPCOs.ContainsCode("600"));
			Assert(lpco.MaterializedLPCOs.ContainsCode("601"));
			Assert(lpco.DeMaterializedLPCOs.ContainsCode("65"));
			Assert(lpco.DeMaterializedLPCOs.ContainsCode("893"));
			Assert(lpco.AIRSRegistrations.ContainsCode("68"));
			Assert(navigator.LPCOList.Last().MaterializedLPCOs.ContainsCode("600"));
		}
	}

	protected override BusinessObject GetNewBusinessObject() => new AIRSWebpageNavigator(Factory, ZString.Empty);

	protected override void SetUp()
	{
		base.SetUp();
		var urlFra = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageFra;
		var urlEng = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng;
		AIRSHtmlHelperTest.SetupRefSysConfigType(Factory, urlFra, urlEng);
	}
}
#endif
