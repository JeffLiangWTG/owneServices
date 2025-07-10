using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillValidationHelperTest : TestCaseWithFactory
	{
		public void TestGetMessageError_WhenRuleE1301Active()
		{
			var attributeDescription = "DESC";

			AssertEquals("[E1301] In transition period, which is now, DESC must be empty", NctsBillValidationHelper.GetMessageErrorForRuleE1301(attributeDescription));
		}

		public void TestGetMessageErrorR0506MustBeDifferent()
		{
			var attributeDescription = "DESC";

			AssertEquals("[R0506] DESC must be different for at least one of the house consignment.", NctsBillValidationHelper.GetMessageErrorR0506MustBeDifferent(attributeDescription));
		}
	}
}
