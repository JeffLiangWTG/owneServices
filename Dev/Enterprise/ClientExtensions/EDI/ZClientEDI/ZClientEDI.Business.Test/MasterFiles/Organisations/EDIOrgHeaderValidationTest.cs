using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EDIOrgHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOH_FullName()
		{
			var org = Factory.New<EDIOrgHeader>();
			AssertNoWarnings(org.OH_FullNameInfo);

			org.OH_FullName = ZString.Replicate('A', 50);
			AssertNoNotifications(org.OH_FullNameInfo);

			org.OH_FullName = ZString.Replicate('A', 51);
			AssertHasWarning(org.OH_FullNameInfo, "Length exceeds 50 characters. An ediEnterprise license cannot be generated.");
		}
	}
}