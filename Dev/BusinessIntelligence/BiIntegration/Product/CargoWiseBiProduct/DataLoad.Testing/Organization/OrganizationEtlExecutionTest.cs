using Enterprise.MasterFiles.Business;

namespace CargoWise.Bi.Product.DataLoad.Testing.Organization
{
	public abstract class OrganizationEtlExecutionTest : EdwEtlExecutionTest
	{
		protected GlbStaff CreateStaff(string code)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;

			factory.Save();

			return staff;
		}
	}
}
