using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class UserTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var staff = GlbStaff.CurrentUser;
			staff.SignatureImage = new Bitmap(1, 1);

			AssertNotNull("prerequisite", staff);

			var user = new Enterprise.MasterFiles.Business.Macros.User(staff);

			AssertEquals("Name", user.Name, staff.GS_FullName);
			AssertEquals("Phone", user.Phone, staff.GS_WorkPhone);
			AssertEquals("Email", user.Email, staff.GS_EmailAddress);
			AssertEquals("Fax", user.Fax, staff.GS_FaxNum);
			AssertEquals("IsDeveloper", user.IsDeveloper, staff.GS_IsDeveloper);
			AssertNotNull("Signature", user.Signature);
		}
	}
}