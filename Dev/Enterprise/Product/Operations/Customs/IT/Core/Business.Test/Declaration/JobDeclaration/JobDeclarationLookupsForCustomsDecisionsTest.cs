using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationLookupsForCustomsDecisionsTest : TestCaseWithFactory
{
	public void TestPaymentPartyList()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("When declaration is IMP, Customs Decisions codes are available", "A, B, C, D, 1, 2, 3, 4", declaration.Lookups.PaymentPartyList.CodesAsString);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("When declaration is not IMP, Customs Decisions codes are not available", "A, B, C, D", declaration.Lookups.PaymentPartyList.CodesAsString);
	}

	public void TestDefermentApprovalNumberListWhenDefermentMethodIsDeclarantAccountFromCustomsDecisions()
	{
		var availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("1000"));

		declaration.JE_PaymentMethod = "1";
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available authorization having type DPO and declarant as holder ", 1, availableDefermentApprovalNumbers.Count);
		Assert("1111111 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("1111111"));

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("When Declarant is empty, no authorization having type DPO must be found", 0, availableDefermentApprovalNumbers.Count);
	}

	public void TestDefermentApprovalNumberListWhenDefermentMethodIsConsigneeAccountFromCustomsDecisions()
	{
		var availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("1000"));

		declaration.JE_PaymentMethod = "2";
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available authorization having type DPO and consignee as holder ", 1, availableDefermentApprovalNumbers.Count);
		Assert("2222222 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("2222222"));

		declaration.JE_OH_Importer = ZGuid.Empty;
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("When Importer is empty, no authorization having type DPO must be found", 0, availableDefermentApprovalNumbers.Count);
	}

	public void TestDefermentApprovalNumberListWhenDefermentMethodIsForwarderAccountFromCustomsDecisions()
	{
		var availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("1000"));

		declaration.JE_PaymentMethod = "3";
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available authorization having type DPO and forwarder as holder ", 1, availableDefermentApprovalNumbers.Count);
		Assert("3333333 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("3333333"));

		declaration.JE_OH_Forwarder = ZGuid.Empty;
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("When Forwarder is empty, no available authorization having type DPO must be found", 0, availableDefermentApprovalNumbers.Count);
	}

	public void TestDefermentApprovalNumberListWhenDefermentMethodIsRepresentativeAccountFromCustomsDecisions()
	{
		var availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("1000"));

		declaration.JE_PaymentMethod = "4";
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("Only one available authorization having type DPO and representative as holder ", 1, availableDefermentApprovalNumbers.Count);
		Assert("4444444 is available deferment approval number", availableDefermentApprovalNumbers.ContainsCode("4444444"));

		declaration.JE_OA_Representative = ZGuid.Empty;
		availableDefermentApprovalNumbers = (declaration.Lookups).DefermentApprovalNumberList;
		AssertEquals("When representative is empty, no authorization having type DPO must be found", 0, availableDefermentApprovalNumbers.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var factory = Factory;
		declaration = factory.New<JobDeclaration>();

		var declarant = factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DEC1";
		declarant.CustomsCodes.AddNew("DAN", "1000", declaration.CountryCode);

		var forwarder = factory.NewWithValidTestData<OrgHeader>();
		forwarder.OH_Code = "FOR1";

		var consignee = factory.NewWithValidTestData<OrgHeader>();
		consignee.OH_Code = "CON1";

		var representative = factory.NewWithValidTestData<OrgHeader>();
		representative.OH_Code = "REP1";
		representative.CustomsCodes.AddNew("DAN", "4444", declaration.CountryCode);

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: declarant.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: consignee.PK, "2222222", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: forwarder.PK, "3333333", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(factory, type: "DPO", permitHolder: representative.PK, "4444444", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		factory.Save();

		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_OH_Forwarder = forwarder.PK;
		declaration.JE_OH_Importer = consignee.PK;
		declaration.JE_OA_Representative = representative.MainAddress.PK;

		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "A";
		declaration.ZG_VATDeferType = "A";
	}

	JobDeclaration declaration;
}
