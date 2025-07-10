using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	class DisableStaffCommissionAgreementsActionValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2002, 2, 2)]
		public void TestCheckDate()
		{
			var staff = Factory.New<GlbStaff>();
			var action = DisableStaffCommissionAgreementsAction.New(staff);

			action.Date = new ZDateTime(2002, 2, 2);
			AssertNoErrors(action.DateInfo);

			action.Date = ZDateTime.Empty;
			AssertMandatoryValidationError(action.DateInfo, true);
		}
	}
}
