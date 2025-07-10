using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class CusFRNctsHeaderPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFN_IsPrelodgedMovement()
		{
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_IsPrelodgedMovement();
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovementInfo, "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked.");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup = helper.LoadOrCreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.France, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroupCUAM = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			nctsHeader.SecurityConsignor.E2_OA_Address = orgHeader.MainAddress.PK;
			nctsHeader.BH_FTZMove = true;
			nctsHeader.MovementHeader.BM_TypeOfSecurity = "BTH";

			nctsHeader.FRNctsHeader.Validation.ValidateCFN_IsPrelodgedMovement();
			AssertHasMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovementInfo, "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked.");

			nctsHeader.MovementHeader.BM_TypeOfSecurity = "ZZZ";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_IsPrelodgedMovement();
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovementInfo, "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked.");

			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			nctsHeader.BH_FTZMove = false;
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_IsPrelodgedMovement();
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovementInfo, "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked.");

			nctsHeader.BH_FTZMove = true;
			nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovement = true;
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_IsPrelodgedMovement();
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovementInfo, "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked.");

			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ukraine;
			nctsHeader.SecurityConsignor.E2_OA_Address = orgHeader.MainAddress.PK;
			nctsHeader.BH_FTZMove = true;
			nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovement = false;

			nctsHeader.MovementHeader.BM_TypeOfSecurity = "BTH";
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_IsPrelodgedMovement();
			AssertHasMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_IsPrelodgedMovementInfo, "The Pre-lodged Movement field must be ticked if Consignor is not EU and Safety and Security is ticked.");
		}

		public void TestCheckCFN_NatureOfSeals()
		{
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, NatureOfSealsMessageError);

			nctsHeader.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR;
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, NatureOfSealsMessageError);

			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = NatureOfSealsList.Codes.NS3;
			AssertHasMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, NatureOfSealsMessageError);

			nctsHeader.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T2;
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_NatureOfSeals();
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, NatureOfSealsMessageError);

			nctsHeader.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR;
			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = "2";
			nctsHeader.FRNctsHeader.Validation.ValidateCFN_NatureOfSeals();
			AssertHasMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, NatureOfSealsMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		}
		NctsHeader nctsHeader;

		const string NatureOfSealsMessageError = "Nature of Seals must be 1 when type of declaration is TIR";
	}
}
