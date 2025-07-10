using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(DestinationOfficeCannotBeInSanMarinoForT1Movements))]
	class DestinationOfficeCannotBeInSanMarinoForT1MovementsTest : ValidationRuleAbstractTest<DestinationOfficeCannotBeInSanMarinoForT1Movements>
	{
		public override void TestIsApplied()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, Rule.IsApplied);
				var movementHeader = header.MovementHeader;
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				AssertEquals("T1 only", false, Rule.IsApplied);
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
				AssertEquals("DES only", false, Rule.IsApplied);
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
				AssertEquals("DES and T1", true, Rule.IsApplied);
			});
		}

		public override void TestValidate()
		{
			CombineAssertions(() =>
			{
				var movementHeader = header.MovementHeader;
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
				var validationResult = Rule.Validate(country);
				AssertEquals("T1-IsValid", false, validationResult.IsValid);
				AssertEquals("T1-Message", "Destination Office cannot be in San Marino for T1 Movements.", validationResult.Message);
			});
		}

		protected override DestinationOfficeCannotBeInSanMarinoForT1Movements Rule => new DestinationOfficeCannotBeInSanMarinoForT1Movements(officeCode);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			officeCode = header.MovementHeader.CustomsOffices.AddNew();
			country = Factory.New<RefCountry>();
			country.Code = Core.Constants.CountryCodes.SanMarino;
		}
		NctsHeader header;
		RefCountry country;
		NctsEuOfficeCode officeCode;
	}
}
