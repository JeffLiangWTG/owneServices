using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(AIRSSelectionForm))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class AIRSSelectionFormTest : ZFormBasherTest
	{
		public void TestOKButton_Click()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			CombineAssertions("With LPCO", () =>
			{
				var urlFra = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageFra;
				var urlEng = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng;
				AIRSHtmlHelperTest.SetupRefSysConfigType(Factory, urlFra, urlEng);

				var navigator = new AIRSWebpageNavigator(Factory, ZString.Empty);
				AIRSSelectionLoader.InitializeAIRSNavigator(webBrowser.Document, navigator);

				using (var frm = new AIRSSelectionForm(navigator))
				{
					frm.Show();
					Application.DoEvents();

					frm.OKButton.PerformClick();
					AssertContains("Please select at least one LPCO/Registration set.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();

					var radioGroupBox = frm.Controls.Find("RadioGroupBox", true).FirstOrDefault();
					var radio = radioGroupBox.Controls.Cast<ZRadioButton>().FirstOrDefault();
					radio.Checked = true;
					frm.OKButton.PerformClick();
					AssertNotContains("Please select at least one LPCO/Registration set.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			});

			CombineAssertions("With No LPCO", () =>
			{
				using (var webBrowser = new ZWebBrowser())
				{
					webBrowser.Navigate("about:blank");
					webBrowser.Document.Write(File.ReadAllText(AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageDummy));

					var navigator = new AIRSWebpageNavigator(Factory, ZString.Empty);
					AIRSSelectionLoader.InitializeAIRSNavigator(webBrowser.Document, navigator);

					using (var frm = new AIRSSelectionForm(navigator))
					{
						frm.Show();
						Application.DoEvents();

						frm.OKButton.PerformClick();
						AssertNotContains("Please select at least one LPCO/Registration set.", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
			});
		}

		public void TestWebBrowserAndRadioButtonsGenerated()
		{
			var urlFra = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageFra;
			var urlEng = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng;
			AIRSHtmlHelperTest.SetupRefSysConfigType(Factory, urlFra, urlEng);

			var navigator = new AIRSWebpageNavigator(Factory, ZString.Empty);
			AIRSSelectionLoader.InitializeAIRSNavigator(webBrowser.Document, navigator);

			using (var frm = new AIRSSelectionForm(navigator))
			{
				frm.Show();
				Application.DoEvents();

				var mainGroupBox = frm.Controls.Find("MainGroupBox", true).FirstOrDefault();
				AssertNotNull(mainGroupBox);
				var webBrowsers = mainGroupBox.Controls.ToList<ZWebBrowser>();
				AssertEquals(2, webBrowsers.Count);
				webBrowsers.ForEach(x => x.Focus());
				var radioGroupBox = frm.Controls.Find("RadioGroupBox", true).FirstOrDefault();
				AssertNotNull(radioGroupBox);
				AssertEquals(2, radioGroupBox.Controls.ToList<Control>().Count(x => x is ZRadioButton));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var urlFra = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageFra;
			var urlEng = AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng;
			AIRSHtmlHelperTest.SetupRefSysConfigType(Factory, urlFra, urlEng);

			var navigator = new AIRSWebpageNavigator(Factory, ZString.Empty);
			return new AIRSSelectionForm(navigator);
		}

		ZWebBrowser webBrowser;

		protected override void SetUp()
		{
			base.SetUp();
			webBrowser = new ZWebBrowser();
			webBrowser.Navigate("about:blank");
			webBrowser.Document.Write(File.ReadAllText(AIRSHtmlHelperTest.TestPath + AIRSHtmlHelperTest.AIRSTestWebPageEng));
		}

		protected override void TearDown()
		{
			base.TearDown();
			webBrowser.Dispose();
		}
	}
}
