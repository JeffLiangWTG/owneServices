using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.eNett;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	public class TestAllocateToOrganisationForm : TestCaseWithFactory
	{
		public void TestAllocateButton_Click_OrgCusCodeRowDeleted()
		{
			var comPayLine = new ComPayRegisteredOrganisation();

			using (var form = new AllocateToOrganisationForm(comPayLine))
			{
				comPayLine.CusCode.Delete();
				Assert(comPayLine.CusCode.IsDeleted);
				AssertNoExceptionThrown("Cannot access the property of a deleted OrgCusCode row.", () => form.AllocateButton_Click_ForTestOnly(null, null));
			}
		}

		public void TestAllocateButton_Click()
		{
			using (AllocateToOrganisationForm form = new AllocateToOrganisationForm(new ComPayRegisteredOrganisation()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.AllocateButton_Click_ForTestOnly(null, null);
				AssertEquals("Last message", "Please enter the organization", UnitTestUserNotification.Instance.LastMessage.Text);

				form.ComPayLine_ForTestOnly.CusCode.OK_OH = Environment.Env.CurrentCompany.OrganisationPK;
				form.ComPayLine_ForTestOnly.ECN = 223355;
				form.AllocateButton_Click_ForTestOnly(null, null);
				AssertEquals("Last message", "Organization was successfully allocated", UnitTestUserNotification.Instance.LastMessage.Text);
				OrgCusCode cusCode = Factory.LoadTop1<OrgCusCode>(new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "223355"));
				AssertNotNull(cusCode);
			}

			using (AllocateToOrganisationForm form = new AllocateToOrganisationForm(new ComPayRegisteredOrganisation()))
			{
				form.ComPayLine_ForTestOnly.CusCode.OK_OH = Environment.Env.CurrentCompany.OrganisationPK;
				form.ComPayLine_ForTestOnly.ECN = 223366;
				form.AllocateButton_Click_ForTestOnly(null, null);
				AssertEquals("Last message", "Allocation for organization was updated", UnitTestUserNotification.Instance.LastMessage.Text);
				OrgCusCode cusCode = Factory.LoadTop1<OrgCusCode>(new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, "223366"));
				AssertNotNull(cusCode);
			}
		}
	}
}
