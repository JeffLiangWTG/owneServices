using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class TestAccQueryClaimUserControl : TestCaseWithFactory
	{
		public void TestDetailsScrollBarEnabled()
		{
			using (AccQueryClaimUserControl testControl = new AccQueryClaimUserControl())
			{
				AssertEquals("Should be Vertical", ScrollBars.Vertical, testControl.DetailsTextBox_ForTestOnly.ScrollBars);
			}
		}

		public void TestShowAddLogCommentPopupFormCreatesNewClaim()
		{
			using (TestFormFortAccQueryClaimUserControl form = new TestFormFortAccQueryClaimUserControl(new OrgAPQueryClaimDependentCollection(Factory.NewWithValidTestData<OrgHeader>(), Factory)))
			{
				AssertNull("Pre-con dition: Should be null", form.UserControl.Claim_ForTestOnly);
				form.UserControl.ShowAddLogCommentPopupForm_ForTestOnly();
				AssertNotNull("Should be created", form.UserControl.Claim_ForTestOnly);
			}
		}

		public void TestHideInterCompanyDetails()
		{
			var claims = new OrgAPQueryClaimDependentCollection(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var claim = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim.AY_MasterBillNumber = "1234";
			claims.Add(claim);

			using (TestFormFortAccQueryClaimUserControl form = new TestFormFortAccQueryClaimUserControl(claims))
			{
				form.Show();
				AssertEquals(form.UserControl.IntercompanyClaimDetailsGroupBox_ForTestOnly.Visible, false);
				AssertEquals(form.UserControl.MawbTextBox_ForTestOnly.Visible, true);
			}

			claim.AY_MasterBillNumber = "";
			using (TestFormFortAccQueryClaimUserControl form = new TestFormFortAccQueryClaimUserControl(claims))
			{
				form.Show();
				AssertEquals(form.UserControl.IntercompanyClaimDetailsGroupBox_ForTestOnly.Visible, true);
				AssertEquals(form.UserControl.MawbTextBox_ForTestOnly.Visible, false);
			}
		}

		#region Implementation

		public class TestFormFortAccQueryClaimUserControl : ZForm
		{
			public TestFormFortAccQueryClaimUserControl(OrgQueryClaimDependentCollection claims)
				: base(claims)
			{
				UserControl = new AccQueryClaimUserControl();
				Controls.Add(UserControl);
				UserControl.SetDataBinding(DataSource, "");
			}

			public AccQueryClaimUserControl UserControl;
		}

		#endregion
	}
}
