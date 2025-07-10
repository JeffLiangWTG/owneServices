using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(MustBeMemberOfEUOrCommonTransit))]
	class MustBeMemberOfEUOrCommonTransitTests : ValidationRuleAbstractTest<MustBeMemberOfEUOrCommonTransit>
	{
		public override void TestIsApplied() => AssertEquals(true, Rule.IsApplied);

		public override void TestValidate()
		{
			CombineAssertions(() =>
			{
				country.Code = Core.Constants.CountryCodes.Moldova;
				var result = Rule.Validate(country);
				AssertEquals("IsValid", false, result.IsValid);
				AssertContains("Message", "is not listed as being in the European Union or a member of the Common Transit convention", result.Message);

				country.Code = Core.Constants.CountryCodes.Spain;
				result = Rule.Validate(country);
				AssertEquals("EU IsValid", true, result.IsValid);
				AssertNotContains("No Message", "is not listed as being in the European Union or a member of the Common Transit convention", result.Message);

				country.Code = Core.Constants.CountryCodes.Turkey;
				result = Rule.Validate(country);
				AssertEquals("CT IsValid", true, result.IsValid);
				AssertNotContains("No Message", "is not listed as being in the European Union or a member of the Common Transit convention", result.Message);
			});
		}

		protected override MustBeMemberOfEUOrCommonTransit Rule => new MustBeMemberOfEUOrCommonTransit();

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.CustomsOffices.AddNew();
			country = Factory.New<RefCountry>();
			country.Code = Core.Constants.CountryCodes.UnitedKingdom;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Spain, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			tradeGroup = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Turkey, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
		}
		RefCountry country;
	}
}
