using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AlternateChartofAccountsForm))]
	public class AlternateChartofAccountsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var chart = Factory.New<AccAlternateChart>();
			return new AlternateChartofAccountsFormForTest(chart);
		}

		public void TestMinimumSize()
		{
			using (var form = new AlternateChartofAccountsForm(Factory.New<AccAlternateChart>()))
			{
				form.Show();
				var minimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 435, true);
				AssertEquals(minimumSize, form.MinimumSize);
				form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(minimumSize.Width - 1, minimumSize.Height - 1, true);
				AssertGreaterThanOrEqualTo(form.Size.Width, form.MinimumSize.Width);
				AssertGreaterThanOrEqualTo(form.Size.Height, form.MinimumSize.Height);
			}
		}

		public void TestControls()
		{
			using (var form = new AlternateChartofAccountsForm(Factory.New<AccAlternateChart>()))
			{
				form.Show();
				var groupBox = form.Controls.Find("AlternateChartGroupBox", true);

				AssertEquals(1, groupBox.Length);
				AssertEquals(1, groupBox[0].Controls.Find("zChartCode", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("zChartName", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("zIsGlobal", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("zFixedLength", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("zBalanceSheetStyle", false).Length);

				var mainTabPage = form.Controls.Find("MainTabPage", true);
				AssertEquals(1, mainTabPage.Length);

				var accountingFormatPanel = mainTabPage[0].Controls.Find("AccountingFormatPanel", true);
				AssertEquals(1, accountingFormatPanel.Length);
				AssertEquals(DockStyle.Fill, accountingFormatPanel[0].Dock);

				var accountingFormatTabControl = accountingFormatPanel[0].Controls.Find("AccountingFormatTabControl", true);
				AssertEquals(1, accountingFormatTabControl.Length);
			}
		}

		public void TestChartCodeKeyPress()
		{
			using (var testForm = new AlternateChartofAccountsForm(Factory.New<AccAlternateChart>()))
			{
				testForm.Show();
				var control = testForm.Controls.Find("zChartCode", true)[0];
				var codeTextBox = (ZTextBox)control;

				AssertEquals("Prereq - Initial Text", "", codeTextBox.Text);
				KeySender.PostKeyDown(codeTextBox, Keys.OemOpenBrackets);
				Application.DoEvents();
				AssertEquals("No text after invalid open square brace character keypress", "", codeTextBox.Text);
				KeySender.PostKeyDown(codeTextBox, Keys.D0);
				Application.DoEvents();
				AssertEquals("Text after 0 character keypress", "0", codeTextBox.Text);
				KeySender.PostKeyDown(codeTextBox, Keys.Back);
				Application.DoEvents();
				AssertEquals("Text after Back character keypress", "", codeTextBox.Text);
			}
		}

		public void TestBalanceSheetStyleKeyPress()
		{
			using (var testForm = new AlternateChartofAccountsForm(Factory.New<AccAlternateChart>()))
			{
				testForm.Show();
				var control = testForm.Controls.Find("zBalanceSheetStyle", true)[0];
				var balanceSheetStyleTextBox = (ZDropEdit)control;

				AssertEquals("Prereq - Initial Text", "EAL", balanceSheetStyleTextBox.Text);
				KeySender.PostKeyDown(balanceSheetStyleTextBox.CodeBox, Keys.D0);
				Application.DoEvents();
				AssertEquals("No text 0 character keypress", "", balanceSheetStyleTextBox.Text);
				KeySender.PostKeyDown(balanceSheetStyleTextBox.CodeBox, Keys.A);
				Application.DoEvents();
				AssertEquals("Text after comma character keypress", "A", balanceSheetStyleTextBox.Text);
				KeySender.PostKeyDown(balanceSheetStyleTextBox.CodeBox, Keys.Back);
				Application.DoEvents();
				AssertEquals("Text after comma character keypress", "", balanceSheetStyleTextBox.Text);
			}
		}

		public void TestControls_CurrencyTranslationTabPageVisible()
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var reportingBookFeatureControlData = new ReportingBookFeatureControlData() { EnableCurrencyTranslation = false };
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingReportingBookFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (var form = new AlternateChartofAccountsFormForTest(Factory.New<AccAlternateChart>()))
			{
				form.Show();
				form.OnShowForTest();
				var currencyTranslationTabPage = form.Controls.Find("CurrencyTranslationTabPage", true).FirstOrDefault();
				AssertNull(currencyTranslationTabPage);
			}

			reportingBookFeatureControlData.EnableCurrencyTranslation = true;
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (var form = new AlternateChartofAccountsFormForTest(Factory.New<AccAlternateChart>()))
			{
				form.Show();
				form.OnShowForTest();
				var currencyTranslationTabPage = form.Controls.Find("CurrencyTranslationTabPage", true).First() as ZTabPage;
				AssertNotNull(currencyTranslationTabPage);
				AssertEquals(true, currencyTranslationTabPage.TabVisible);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AlternateChartofAccountsFormForTest)GetFormToBashCore())
			{
				AssertNotNull("Alternate Chart form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
				Assert("ShowAuditPlugin of Alternate Chart Form is true", form.ShowAuditPlugin);
			}
		}
	}

	class AlternateChartofAccountsFormForTest : AlternateChartofAccountsForm
	{
		public AlternateChartofAccountsFormForTest(AccAlternateChart businessEntity) : base(businessEntity)
		{
		}

		public void OnShowForTest()
		{
			OnShown(new System.EventArgs());
		}

		public bool ShowAuditPlugin => ShowAuditTab;
	}
}
