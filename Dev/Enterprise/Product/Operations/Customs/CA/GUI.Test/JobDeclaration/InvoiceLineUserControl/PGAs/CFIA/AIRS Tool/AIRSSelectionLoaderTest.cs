using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
#if !WINZOR
	public class GetElementTextTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetElementTextById()
		{
			using (var browser = new ZWebBrowser())
			{
				browser.Navigate("about:blank");
				browser.Document.Write(File.ReadAllText(AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng));
				AssertEquals(ZString.Empty, AIRSSelectionLoader.GetElementTextById(browser.Document, ZString.Empty));
				AssertEquals(ZString.Empty, AIRSSelectionLoader.GetElementTextById(browser.Document, "XXXX"));
				AssertEquals("400701", AIRSSelectionLoader.GetElementTextById(browser.Document, "ctl00_ContentMain_lblOGDExtensionIDText"));
			}
		}
	}
#endif
}
