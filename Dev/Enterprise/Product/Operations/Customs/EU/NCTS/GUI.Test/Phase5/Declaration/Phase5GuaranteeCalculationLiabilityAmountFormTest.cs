using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GuaranteeCalculationLiabilityAmountForm))]
	sealed class Phase5GuaranteeCalculationLiabilityAmountFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestDataSourceType() => AssertEquals(typeof(CalculateLiabilityBizObj), Form.DataSourceType);

		public void TestFormText()
		{
			Form.Show();
			AssertEquals("Calculate Liability Amount", Form.Text);
		}

		[RequiresSTA]
		public void TestControls() => CombineAssertions(() =>
		{
			Form.Show();
			AssertNotNull("LiabilityAmountTotalValueCalculationMethodUserControl", Form.FindSingleOrDefault<LiabilityAmountTotalValueCalculationMethodUserControl>("LiabilityAmountTotalValueCalculationMethodUserControl"));
			AssertNotNull("TotalValueCalcDropEdit", Form.FindSingleOrDefault<ZCalcDropEdit>("TotalValueCalcDropEdit"));
			AssertNotNull("LiabilityPercentageIntEdit", Form.FindSingleOrDefault<ZIntEdit>("LiabilityPercentageIntEdit"));
			AssertNotNull("LiabilityAmountCalcDropEdit", Form.FindSingleOrDefault<ZCalcDropEdit>("LiabilityAmountCalcDropEdit"));
			AssertNotNull("OkButton", Form.FindSingleOrDefault<ZButton>("OkButton"));
			AssertNotNull("CancelButton", Form.FindSingleOrDefault<ZButton>("CancelButton2"));
			AssertNotNull("DefaultLiabilityAmountButton", Form.FindSingleOrDefault<ZButton>("DefaultLiabilityAmountButton"));
		});

		public void TestOkButton() => CombineAssertions(() =>
		{
			var guarantee = CalculateLiabilityBizObj.Guarantee;
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 36m;
			CalculateLiabilityBizObj.TotalValue = 100;

			var formIsClosed = false;
			Form.FormClosed += (sender, e) => { formIsClosed = true; };
			Form.Show();
			Form.DialogResult = DialogResult.None;
			var okButton = Form.FindSingle<ZButton>("OkButton");

			AssertEquals("Precondition_CalculateBizObj: LiabilityAmount", 25m, CalculateLiabilityBizObj.LiabilityAmount);
			AssertEquals("Precondition_Guarantee: LiabilityAmount", 36m, guarantee.PW_BondAmount);

			okButton.PerformClick();
			AssertEquals("LiabilityAmount of Guarantee updated", 25m, guarantee.PW_BondAmount);
			AssertEquals("PW_Override true", expected: true, guarantee.PW_Override);
			AssertEquals("Form has been closed", expected: true, formIsClosed);
			AssertEquals("Form DialogResult", DialogResult.OK, Form.DialogResult);
		});

		public void TestOkButton_ValidationErrors() => CombineAssertions(() =>
		{
			var guarantee = CalculateLiabilityBizObj.Guarantee;
			guarantee.PW_Override = true;
			CalculateLiabilityBizObj.TotalValue = 12345678901234567890m;
			var formIsClosed = false;
			Form.FormClosed += (sender, e) => { formIsClosed = true; };
			Form.Show();
			Form.DialogResult = DialogResult.None;
			var okButton = Form.FindSingle<ZButton>("OkButton");

			AssertEquals("Precondition_Guarantee: LiabilityAmount", 40m, guarantee.PW_BondAmount);
			AssertEquals("Precondition_ValidationError", expected: true, CalculateLiabilityBizObj.HasErrors);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			okButton.PerformClick();
			AssertEquals("Pop-up window shows", "The form has errors. Please fix them before continuing or cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form hasn't been closed", expected: false, formIsClosed);
			AssertEquals("Form DialogResult", DialogResult.None, Form.DialogResult);
			AssertEquals("LiabilityAmount of Guarantee not updated", 40m, guarantee.PW_BondAmount);
			AssertEquals("PW_Override still true", expected: true, guarantee.PW_Override);
		});

		public void TestCancelButton() => CombineAssertions(() =>
		{
			var guarantee = CalculateLiabilityBizObj.Guarantee;
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 36m;
			CalculateLiabilityBizObj.TotalValue = 100;

			var formIsClosed = false;
			Form.FormClosed += (sender, e) => { formIsClosed = true; };
			Form.Show();
			Form.DialogResult = DialogResult.None;
			var cancelButton = Form.FindSingle<ZButton>("CancelButton2");

			AssertEquals("Precondition_CalculateBizObj: LiabilityAmount", 25m, CalculateLiabilityBizObj.LiabilityAmount);
			AssertEquals("Precondition_Guarantee: LiabilityAmount", 36m, guarantee.PW_BondAmount);

			cancelButton.PerformClick();
			AssertEquals("LiabilityAmount of Guarantee remains", 36m, guarantee.PW_BondAmount);
			AssertEquals("PW_Override false", expected: false, guarantee.PW_Override);
			AssertEquals("Form has been closed", expected: true, formIsClosed);
			AssertEquals("Form DialogResult", DialogResult.Cancel, Form.DialogResult);
		});

		[RequiresSTA]
		public void TestDefaultLiabilityAmountButtonIsHidden() => AssertDefaultLiabilityAmountButtonVisibility(false);

		public void TestDefaultLiabilityAmountButtonIsShown() => AssertDefaultLiabilityAmountButtonVisibility(true);

		void AssertDefaultLiabilityAmountButtonVisibility(bool buttonVisibility)
		{
			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeDefaultLiabilityAmountButtonConfiguration(Factory, buttonVisibility, 10000m))
			using (var form = new Phase5GuaranteeCalculationLiabilityAmountForm(CalculateLiabilityBizObj))
			{
				Form.Show();
				var defaultLiabilityAmountButton = Form.FindSingle<ZButton>("DefaultLiabilityAmountButton");

				AssertEquals("DefaultLiabilityAmountButton.Visible", buttonVisibility, defaultLiabilityAmountButton.Visible);
			}
		}

		public void TestDefaultLiabilityAmountButtonClick() => CombineAssertions(() =>
		{
			using (NctsConfigurationTestHelper.TemporarilySetGuaranteeDefaultLiabilityAmountButtonConfiguration(Factory, true, 10000m))
			using (var form = new Phase5GuaranteeCalculationLiabilityAmountForm(CalculateLiabilityBizObj))
			{
				var guarantee = CalculateLiabilityBizObj.Guarantee;
				guarantee.PW_Override = true;
				var formIsClosed = false;
				form.FormClosed += (sender, e) => { formIsClosed = true; };
				form.Show();
				form.DialogResult = DialogResult.None;

				var configuration = guarantee.NctsHeader.Configuration.GuaranteeConfiguration;
				var defaultLiabilityAmountButton = form.FindSingle<ZButton>("DefaultLiabilityAmountButton");
				AssertEquals("DefaultLiabilityAmountButton.Caption", Utilities.FormatNumberNationalWithGroupSeparators(configuration.DefaultLiabilityAmount, 0), defaultLiabilityAmountButton.Text);

				defaultLiabilityAmountButton.PerformClick();
				AssertEquals("PW_BondAmount", configuration.DefaultLiabilityAmount, guarantee.PW_BondAmount);
				AssertEquals("PW_Override", true, guarantee.PW_Override);
				AssertEquals("Form has been closed", true, formIsClosed);
				AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
			}
		});

		protected override Form GetFormToBashCore() => Form;

		CalculateLiabilityBizObj CalculateLiabilityBizObj => calculateLiabilityBizObj ??= new CalculateLiabilityBizObj(Factory, CalculateLiabilityBizObjTestHelper.CreateGuaranteeForCalculateLiabilityBizObj(Factory).guarantee);
		CalculateLiabilityBizObj calculateLiabilityBizObj;

		Phase5GuaranteeCalculationLiabilityAmountForm Form => form ??= new Phase5GuaranteeCalculationLiabilityAmountForm(CalculateLiabilityBizObj);
		Phase5GuaranteeCalculationLiabilityAmountForm form;

		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
		}
	}
}
