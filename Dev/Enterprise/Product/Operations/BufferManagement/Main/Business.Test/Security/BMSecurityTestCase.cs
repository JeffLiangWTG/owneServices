using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test.Security
{
	public class BMSecurityTestCase : TestCaseWithFactory
	{
		protected GlbSecurityCollection SecurityCollection { get; set; }
		protected GlbStaff Staff { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "First Controller Staff";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;
			Factory.Save();

			Staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			Staff.GS_IsController = false;

			SecurityCollection = new GlbSecurityCollection(Factory);
			SecurityCollection.Load();

			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();
		}
	}
}
