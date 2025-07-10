using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class NctsPackagePhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRuleC0060()
		{
			const string expectedWarning = "[C0060] You have not entered a Number of Packages - valid for goods in shared packaging.";
			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsPackageValidationDeciderTestContext<INctsPackageDeparturePhase5ValidationDecider>(Factory);
				ruleTestContext.EnableRule(decider => decider.IsRuleC0060Active);

				package1.Validation.ValidateB5_UnitCount();
				package2.Validation.ValidateB5_UnitCount();
				package3.Validation.ValidateB5_UnitCount();
				AssertHasWarningContaining("Warning because package2 is not bulk type and unit count is 0", package2.B5_UnitCountInfo, expectedWarning);
				AssertHasWarningContaining("Warning because package3 is not bulk type and unit count is 0", package3.B5_UnitCountInfo, expectedWarning);
				package3.B5_UnitCount = 1;
				AssertNoWarningContaining("No warning when unit count > 0", package3.B5_UnitCountInfo, expectedWarning);
				package2.B5_UnitType = "AC";
				package2.Validation.ValidateB5_UnitCount();
				AssertHasWarningContaining("Warning because package2 is not bulk type and unit count is 0", package2.B5_UnitCountInfo, expectedWarning);
				package2.B5_UnitType = unpackedType;
				package2.Validation.ValidateB5_UnitCount();
				AssertHasWarningContaining("Warning for unpackaged type when unit count is 0", package2.B5_UnitCountInfo, expectedWarning);
			});
		}

		public void TestCheckRuleC0060_BulkAndUnpackaged()
		{
			const string expectedWarning = "[C0060] You have not entered a Number of Packages - valid for goods in shared packaging.";
			CombineAssertions(() =>
			{
				package1.B5_UnitType = unpackedType;
				package2.B5_UnitType = unpackedType;
				package1.Validation.ValidateB5_UnitCount();
				package2.Validation.ValidateB5_UnitCount();
				package3.Validation.ValidateB5_UnitCount();
				AssertHasWarningContaining("Warning for unpackaged type when unit count is 0", package2.B5_UnitCountInfo, expectedWarning);
				AssertHasWarningContaining("Warning because package3 is not bulk type and unit count is 0", package3.B5_UnitCountInfo, expectedWarning);

				package3.B5_UnitType = bulkCode;
				package3.Validation.ValidateB5_UnitCount();
				AssertNoWarningContaining("No warning for bulk type", package3.B5_UnitCountInfo, expectedWarning);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			bulkCode = Factory.SetupBulkCusCode();
			unpackedType = Factory.SetupUnpackCusCode();
			var bill = header.Bills.FirstOrDefault() ?? header.Bills.AddNew();
			var item1 = bill.GoodsItems.AddNew();
			var item2 = bill.GoodsItems.AddNew();
			package1 = item1.Packages.AddNew();
			package2 = item2.Packages.AddNew();
			package3 = item2.Packages.AddNew();

			package1.B5_UnitType = "AB";
			package1.B5_MarksAndNumbers = "Mark1";
			package1.B5_UnitCount = 1;

			package2.B5_UnitType = "AB";
			package2.B5_MarksAndNumbers = "Mark1";
			package2.B5_UnitCount = 0;

			package3.B5_UnitType = "AB";
			package3.B5_MarksAndNumbers = "Mark2";
			package3.B5_UnitCount = 0;
		}
		string bulkCode, unpackedType;
		NctsHeader header;
		NctsPackage package1, package2, package3;
	}
}
