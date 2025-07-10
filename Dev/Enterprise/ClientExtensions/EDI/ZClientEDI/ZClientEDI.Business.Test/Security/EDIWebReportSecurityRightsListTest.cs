using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.Test
{
	internal class EDIWebReportSecurityRightsListTest : WebReportSecurityRightsListTest
	{
		public override void TestConstructor()
		{
			var menuItem1 = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem1.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			menuItem1.SU_BusinessContext = "RepHRReports";
			menuItem1.SU_MenuName = "Report 1";

			var menuItem2 = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem2.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			menuItem2.SU_BusinessContext = "RepCustomReports";
			menuItem2.SU_MenuName = "Report 2";

			var list = ReportsWebSecurityRights.New(Factory);
			WebSecurityRight rightForMenu1, rightForMenu2;
			Assert(nameof(rightForMenu1), list.TryGetValue(menuItem1.PK.ToString(), out rightForMenu1));
			Assert(nameof(rightForMenu2), !list.TryGetValue(menuItem2.PK.ToString(), out rightForMenu2));

			AssertEquals("RepHRReports: Report 1", rightForMenu1.Description);
			AssertNull(rightForMenu2);
		}
	}
}