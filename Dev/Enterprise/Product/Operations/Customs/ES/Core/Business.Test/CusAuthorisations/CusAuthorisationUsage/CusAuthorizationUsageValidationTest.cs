using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CusAuthorizationUsageValidationTest : TestCaseWithFactory
	{
		public void TestCheckAGC_Code()
		{
			CombineAssertions(() =>
			{
				string expectedWarning = "EIR authorization must be used only for Sub Style Z.";
				cusAuthorizationUsage.Instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;
				cusAuthorizationUsage.AGC_Code = "ABC";
				AssertNoWarning("No Warning as AGC_Code is not EIR", cusAuthorizationUsage.AGC_CodeInfo, expectedWarning);

				cusAuthorizationUsage.Instruction.CEI_SubStyle = "A";
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				AssertHasWarning("Warning as AGC_Code is EIR and SubStyle is not Z.", cusAuthorizationUsage.AGC_CodeInfo, expectedWarning);

				cusAuthorizationUsage.Instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;
				cusAuthorizationUsage.Validation.ValidateAGC_Code();
				AssertNoWarning("No Warning as AGC_Code is EIR and SubStyle is Z.", cusAuthorizationUsage.AGC_CodeInfo, expectedWarning);
			});
		}

		public void TestCheckAGC_Number_ValidWhitoutPremises()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var authorizationHeaderLame = Factory.New<CusAuthorisationHeader>();
			authorizationHeaderLame.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport;
			authorizationHeaderLame.CPH_Number = "ES00LAME";
			authorizationHeaderLame.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeaderLame.CPH_OA_AppliesTo = orgAddress.PK;
			authorizationHeaderLame.CPH_StartDate = new ZDate(2021, 11, 03);

			var authorizationHeaderTST = Factory.New<CusAuthorisationHeader>();
			authorizationHeaderTST.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			authorizationHeaderTST.CPH_Number = "ES00TST";
			authorizationHeaderTST.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeaderTST.CPH_OA_AppliesTo = orgAddress.PK;
			authorizationHeaderTST.CPH_StartDate = new ZDate(2021, 11, 03);

			var propertyInfo = cusAuthorizationUsage.AGC_NumberInfo;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.EffectiveReferenceNumber = "ES00LAME";
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertNoWarnings("No warning when LAME Authorization and LAM type Premise", propertyInfo);

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertHasWarningContaining("warning when TST Authorization and LAM type Premise", propertyInfo, "Authorization number: ES00LAME doesn't exist for Code: TST, Owner: OH1");

				cusAuthorizationUsage.EffectiveReferenceNumber = "ES00TST";
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertNoWarnings("No warning when TST Authorization and ADT type Premise", propertyInfo);

				cusAuthorizationUsage.AGC_Code = ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport;
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertHasWarningContaining("warning when LAME Authorization and ADT type Premise", propertyInfo, "Authorization number: ES00TST doesn't exist for Code LAME, Owner: OH1");
			});
		}

		public void TestCheckAGC_Number_ValidWithPremisesForLAM()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport;
			authorizationHeader.CPH_Number = "ES00LAME";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeader.CPH_OA_AppliesTo = orgAddress.PK;
			authorizationHeader.CPH_StartDate = new ZDate(2021, 11, 03);

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Code = "PREMISES";
			premises.SRP_Description = "Description Premises";
			premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			premises.AuthorizationNumber = "ES00LAME";

			cusAuthorizationUsage.EffectiveReferenceNumber = "ES00LAME";
			var propertyInfo = cusAuthorizationUsage.AGC_NumberInfo;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertNoWarnings("No warning when LAME Authorization and LAM type Premise", propertyInfo);

				cusAuthorizationUsage.EffectiveReferenceNumber = "ES00TST";
				premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertHasWarningContaining("warning when TST Authorization and LAM type Premise", propertyInfo, "Authorization number: ES00TST doesn't exist for Code LAME, Owner: OH1");
			});
		}

		public void TestCheckAGC_Number_ValidWithPremisesForTST()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			authorizationHeader.CPH_Number = "ES00TST";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeader.CPH_OA_AppliesTo = orgAddress.PK;
			authorizationHeader.CPH_StartDate = new ZDate(2021, 11, 03);

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Code = "PREMISES";
			premises.SRP_Description = "Description Premises";
			premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			premises.AuthorizationNumber = "ES00TST";

			cusAuthorizationUsage.EffectiveReferenceNumber = "ES00TST";
			var propertyInfo = cusAuthorizationUsage.AGC_NumberInfo;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertNoWarnings("No warning when TST Authorization and ADT type Premise", propertyInfo);

				cusAuthorizationUsage.EffectiveReferenceNumber = "ES00LAME";
				premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertHasWarningContaining("warning when LAME Authorization and ADT type Premise", propertyInfo, "Authorization number: ES00LAME doesn't exist for Code: TST, Owner: OH1");
			});
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, grouping);
			helper.CreateCusCodeType("AUTH", "Authorisation");
			helper.CreateCusCodeList("EUN", "AUTH", "ACT", "ACT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusAuthorizationUsage = (CusAuthorizationUsage)entryInstruction.CusAuthorizationUsages.AddNew();
		}
		JobDeclaration declaration;
		Declaration.CusEntryInstruction entryInstruction;
		CusAuthorizationUsage cusAuthorizationUsage;
	}
}
