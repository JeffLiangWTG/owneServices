using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(TIRCanOnlyGoToEU))]
	class TIRCanOnlyGoToEUTest : ValidationRuleAbstractTest<TIRCanOnlyGoToEU>
	{
		public override void TestIsApplied()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, Rule.IsApplied);
				officeCode.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
				AssertEquals("TIR", true, Rule.IsApplied);
			});
		}

		public override void TestValidate()
		{
			CombineAssertions(() =>
			{
				officeCode.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
				var validationResult = Rule.Validate(country);
				AssertEquals("TIR-IsValid", false, validationResult.IsValid);
				AssertEquals("TIR-Message", "TIR can only go to countries in the EU.", validationResult.Message);
			});
		}

		protected override TIRCanOnlyGoToEU Rule => new TIRCanOnlyGoToEU(officeCode);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			officeCode = header.MovementHeader.CustomsOffices.AddNew();
			country = Factory.New<RefCountry>();
			country.Code = Core.Constants.CountryCodes.Switzerland;
		}
		RefCountry country;
		NctsEuOfficeCode officeCode;
	}
}
