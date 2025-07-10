using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class BaseEntryInstructionDPOAuthorizationTest : TestCaseWithFactory
{
	protected CusEntryInstruction AddTestInstructionAndAuthorizationUsages(ZString entryStatus)
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = entryStatus;

		var genericOrg = Factory.NewWithValidTestData<OrgHeader>();

		var entryInstruction = Factory.New<CusEntryInstruction>();
		var dpoAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		dpoAuthorizationUsage.AGC_OH_Owner = genericOrg.PK;
		dpoAuthorizationUsage.AGC_Number = "123456";
		dpoAuthorizationUsage.AGC_Code = "DPO";

		var genericAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		genericAuthorizationUsage.AGC_OH_Owner = genericOrg.PK;
		genericAuthorizationUsage.AGC_Number = "888888";
		genericAuthorizationUsage.AGC_Code = "ZIZ";

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);
		return entryInstruction;
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.ZG_VATDeferType = "A";

		declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DEC1";
		declarant.CustomsCodes.AddNew("DAN", "1000", declaration.CountryCode);

		forwarder = Factory.NewWithValidTestData<OrgHeader>();
		forwarder.OH_Code = "FOR1";

		consignee = Factory.NewWithValidTestData<OrgHeader>();
		consignee.OH_Code = "CON1";

		representative = Factory.NewWithValidTestData<OrgHeader>();
		representative.OH_Code = "REP1";
		representative.CustomsCodes.AddNew("DAN", "4444", declaration.CountryCode);

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: declarant.PK, "DDDDDDD", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: consignee.PK, "CCCCCCC", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: forwarder.PK, "FFFFFFF", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: representative.PK, "EEEEEEE", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_OH_Forwarder = forwarder.PK;
		declaration.JE_OH_Importer = consignee.PK;
		declaration.JE_OA_Representative = representative.MainAddress.PK;

		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
	}

	protected JobDeclaration declaration;
	protected OrgHeader declarant;
	protected OrgHeader forwarder;
	protected OrgHeader consignee;
	protected OrgHeader representative;
}
