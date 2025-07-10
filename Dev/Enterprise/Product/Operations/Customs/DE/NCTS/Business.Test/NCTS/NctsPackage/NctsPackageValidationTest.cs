using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsNctsPackagePhase5ValidationType() => AssertEquals(true, package.Validation is NctsPackagePhase5Validation);

		public void TestCheckB5_UnitCount_MaxValue()
		{
			var validation = new NctsPackageValidationForTest(package);
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Functionality is enabled", (ZLong)99999, validation.B5_UnitCountMaxValueExposed);

					var validation2 = new NctsPackageValidationForTest(Factory.New<NctsPackage>());
					AssertEquals("Package isn't for DepartureGoodsItem", (ZLong)99999999, validation2.B5_UnitCountMaxValueExposed);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Functionality is disabled", (ZLong)99999999, validation.B5_UnitCountMaxValueExposed);
				}
			});
		}

		public void TestCheckB5_MarksAndNumbers_CheckMandatoryMarksAndNumbers_DepartureCargoDesc()
		{
			package.B5_UnitType = "$%";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(package.B5_MarksAndNumbersInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = goodsItem.Packages.AddNew();
		}
		NctsPackage package;
	}

	sealed class NctsPackageValidationForTest : NctsPackageValidation
	{
		public NctsPackageValidationForTest(NctsPackage parent)
			: base(parent)
		{
		}

		public ZLong B5_UnitCountMaxValueExposed => B5_UnitCountMaxValue;
	}
}
