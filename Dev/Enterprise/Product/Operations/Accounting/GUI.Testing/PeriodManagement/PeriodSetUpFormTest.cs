using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(PeriodSetUpForm))]
	public class PeriodSetUpFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PeriodSetUpForm(new NewYearPeriodSettings());
		}

		[TestDate(1999, 7, 2)]
		public void TestNewPeriodsWarningMessage()
		{
			NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
			testNewYear.StartDate = new ZDateTime(1998, 1, 1);

			using (PeriodSetUpForm testForm = new PeriodSetUpForm(testNewYear))
			{
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.OKButton_ForTestOnly.PerformClick();

				AssertContains(@$"You are currently in year 1999. You are attempting to create periods for an accounting year that ends before today's date. If you continue new periods will be created for the 1998 accounting year with the given dates.
{BrandingFactory.Instance.ProductName} will create accounting periods for the previous accounting year automatically, because this is the first accounting year you are creating.

Please click Yes to continue or No to cancel and setup a different initial accounting year.",
					UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.NewYearSettings_ForTestOnly.StartDate = new ZDateTime(1998, 2, 1);
				testForm.OKButton_ForTestOnly.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormCaption()
		{
			using (PeriodSetUpForm testForm = (PeriodSetUpForm)GetFormToBash())
			{
				testForm.Show();
				AssertEquals("Set Periods for Accounting Year", testForm.Text);
			}
		}

		public void TestAccountingYearBaseOnVisible()
		{
			AssertAccountingYearBaseOnVisible(false, true);
			AssertAccountingYearBaseOnVisible(true, false);
		}

		void AssertAccountingYearBaseOnVisible(bool isPeriodsSetBefore, bool isVisible)
		{
			NewYearPeriodSettings newYearPeriodSettings = new NewYearPeriodSettings();
			newYearPeriodSettings.IsPeriodsSetBefore = isPeriodsSetBefore;
			using (PeriodSetUpForm testForm = new PeriodSetUpForm(newYearPeriodSettings))
			{
				testForm.Show();
				Control accountingYearBasedOn = testForm.Controls.Find("accountingYearBasedDropEdit", true)[0];
				AssertEquals("Should Accounting Year Based On show", accountingYearBasedOn.Visible, isVisible);
			}
		}
	}
}
