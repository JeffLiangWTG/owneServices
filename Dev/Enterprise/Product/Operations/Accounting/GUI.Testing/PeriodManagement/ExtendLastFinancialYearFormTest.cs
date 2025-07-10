using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GUI.PeriodManagement;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.PeriodManagement
{
	[TestedType(typeof(ExtendLastFinancialYearForm))]
	public class ExtendLastFinancialYearFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			new TestObjectCreator(Factory).CreateTestPeriodsForEntireYear(2023);
			var extendLastFinancialYearForm = new ExtendLastFinancialYearForm(ExtendLastFinancialYearSettings, new PeriodManager(Factory));
			return extendLastFinancialYearForm;
		}

		public void TestFormCaption()
		{
			using (var testForm = (ExtendLastFinancialYearForm)GetFormToBash())
			{
				testForm.Show();
				AssertEquals("Modify Financial Year Range", testForm.Text);
			}
		}

		public void TestOKButtonClick()
		{
			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				var button = testForm.GetControl<ZButton>("OKButton");

				ExtendLastFinancialYearSettings.StartDateInfo.AddError("Please enter Start Date");
				button.PerformClick();
				AssertContains(@"Please fix errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);

				ExtendLastFinancialYearSettings.StartDateInfo.ClearAllNotifications();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ExtendLastFinancialYearSettings.FinancialYear = 2023;
				ExtendLastFinancialYearSettings.StartDate = new CargoWise.Types.ZDateTime(2023, 07, 01);
				ExtendLastFinancialYearSettings.EndDate = new CargoWise.Types.ZDateTime(2024, 07, 31);
				button.PerformClick();
				AssertEquals("Are you sure you want to set new Financial Year Range based on specified dates for company [Eagle Datamation International]?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		ExtendLastFinancialYearSettings ExtendLastFinancialYearSettings => extendLastFinancialYearSettings ?? (extendLastFinancialYearSettings = new ExtendLastFinancialYearSettings());
		ExtendLastFinancialYearSettings extendLastFinancialYearSettings;
	}
}
