using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class EcoRegimeAuthorizationWrapperTest : TestCaseWithFactory
	{
		public void TestEcoRegimeAuthorizationNumber()
		{
			AssertEquals("EcoRegimeAuthorizationNumber", "ZZZZZ", wrapper.EcoRegimeAuthorizationNumber);
		}

		public void TestEcoRegimeCountryCode()
		{
			AssertEquals("EcoRegimeCountryCode", "GB", wrapper.EcoRegimeCountryCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			authHeader.CPH_PermitDescription = "CN Code 123456";

			var autRule = authHeader.CusAuthorisationRules.AddNew();
			autRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
			autRule.CPR_ValueFrom = "ZZZZZ";

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			wrapper = new EcoRegimeAuthorizationWrapper(entryHeader);
		}

		Declaration.CusEntryHeader entryHeader;
		IEcoRegimeAuthorization wrapper;
	}
}
