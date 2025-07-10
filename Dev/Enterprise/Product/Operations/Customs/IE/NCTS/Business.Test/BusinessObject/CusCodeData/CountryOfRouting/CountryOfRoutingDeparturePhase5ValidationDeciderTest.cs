using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class CountryOfRoutingDeparturePhase5ValidationDeciderTest : TestCase
	{
		public void TestIsIsRuleB1836ActiveActive()
		{
			AssertEquals(true, validationDecider.IsRuleB1836Active);
		}

		[UseSnapshotProtection]
		public void TestIsRuleC0030ActiveDepartureMovementHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112,
				Core.Constants.CountryCodes.UnitedKingdom,
				"United Kingdom", startDate, endDate);
			factory.Save();

			var countryOfRouting = nctsHeader.CountriesOfRouting.AddNew();
			countryOfRouting.CY_Data = Core.Constants.CountryCodes.UnitedKingdom;

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
			AssertEquals(true, validationDecider.IsRuleC0030Active);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals(false, validationDecider.IsRuleC0030Active);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
			AssertEquals(false, validationDecider.IsRuleC0030Active);
		}

		[UseSnapshotProtection]
		public void TestIsRuleC0030ActiveArrivalMovementHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112,
				Core.Constants.CountryCodes.UnitedKingdom,
				"United Kingdom", startDate, endDate);
			factory.Save();

			var countryOfRouting = nctsHeader.CountriesOfRouting.AddNew();
			countryOfRouting.CY_Data = Core.Constants.CountryCodes.UnitedKingdom;

			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
			AssertEquals(true, validationDecider.IsRuleC0030Active);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals(true, validationDecider.IsRuleC0030Active);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
			AssertEquals(true, validationDecider.IsRuleC0030Active);
		}

		public void TestIsRuleC0586Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0586Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
			nctsHeader = factory.New<EU.NCTS.Business.NctsHeader>();
			validationDecider = new CountryOfRoutingDeparturePhase5ValidationDecider(nctsHeader);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}

		BusinessObjectFactory factory;
		CountryOfRoutingDeparturePhase5ValidationDecider validationDecider;
		EU.NCTS.Business.NctsHeader nctsHeader;
	}
}
