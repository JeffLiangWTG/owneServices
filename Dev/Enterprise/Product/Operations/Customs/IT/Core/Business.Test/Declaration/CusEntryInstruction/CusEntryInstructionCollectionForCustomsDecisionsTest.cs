using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryInstructionCollectionForCustomsDecisionsTest : TestCaseWithFactory
{
	public void TestAddNew()
	{
		declaration.JE_PaymentMethod = "1";
		declaration.JE_DefermentAccountNumber = "1111111";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions("The insertion of a new instruction must trigger the insertion of a new authorization for it", () =>
		{
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AutorizatioUsage must have declarant as owner", declarant.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AutorizatioUsage must have DefermentAccountNumber as value", "1111111", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AutorizatioUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
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

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: declarant.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: consignee.PK, "2222222", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DPO", permitHolder: forwarder.PK, "3333333", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		declaration.JE_OH_Forwarder = forwarder.PK;
		declaration.JE_OH_Importer = consignee.PK;
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
	}

	JobDeclaration declaration;
	OrgHeader declarant;
	OrgHeader forwarder;
	OrgHeader consignee;
}
