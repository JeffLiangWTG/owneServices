using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(InterchangeNumberDetailsForm))]
	sealed class InterchangeNumberDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var increaseInterchangeNumber = new InterchangeNumberDetails(GlbCompany.CurrentCompany);
			return new InterchangeNumberDetailsForm(increaseInterchangeNumber);
		}

		public void TestButtonOK_Click()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "Test001";
			CompanyCredentialsPlugInTest.InsertTestRecord("ITest001AAA336C", 1);
			var interchangeNumber = new InterchangeNumberDetails(company);
			using (var form = new InterchangeNumberDetailsForm(interchangeNumber))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				interchangeNumber.NewInterchangeNumber = 2L;
				using (interchangeNumber.SuspendValidationTesting())
				{
					interchangeNumber.NewInterchangeNumberInfo.ClearAllNotifications();
				}
				form.ButtonOK.PerformClick();
				AssertEquals("Interchange Number failed to save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				interchangeNumber.NewInterchangeNumber = 123456L;
				form.ButtonOK.PerformClick();
				AssertEquals("Interchange Number saved successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

				var snValue = Db.Connection.ExecuteScalar<long>("select top 1 [SN_Value] from dbo.StmNums where [SN_Name] = 'ITest001AAA336C'");
				AssertEquals(123456L, snValue);
			}
		}

		public void TestButtonIncrement_Click()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "Test001";

			var interchangeNumber = new InterchangeNumberDetails(company);
			using (var form = new InterchangeNumberDetailsForm(interchangeNumber))
			{
				form.Show();

				form.ButtonIncrement.PerformClick();
				AssertEquals(InterchangeNumberDetails.NewInterchangeNumberIncrement, interchangeNumber.NewInterchangeNumber);
			}
		}
	}
}
