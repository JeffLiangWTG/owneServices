using System.Collections.Generic;

namespace CargoWise.Bi.Product.DataLoad.Testing.Organization
{
	public class StaffTest : OrganizationEtlExecutionTest
	{
		protected override IEnumerable<string> MainDbTableList => new[] { "GlbStaff" };

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStaffLoginName()
		{
			RunInitialLoad();

			var staff = CreateStaff(code: "TST");

			RunIncrementalLoad();

			AssertTableHasRow("Organization.MDL__Staff", "[Staff Code] = 'TST'");
		}
	}
}
