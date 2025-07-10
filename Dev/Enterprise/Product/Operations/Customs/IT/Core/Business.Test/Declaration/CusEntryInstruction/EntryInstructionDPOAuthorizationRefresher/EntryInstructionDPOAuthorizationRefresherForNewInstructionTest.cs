using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryInstructionDPOAuthorizationRefresherForNewInstructionTest : BaseEntryInstructionDPOAuthorizationTest
{
	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";
		declaration.JE_DefermentAccountNumber = "1111111";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", declarant.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "1111111", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));

		CombineAssertions("When Entry Header bound to instruction has filled CH_EntryStatus", () =>
		{
			AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarantIsEmptyAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";
		declaration.DeclarantOrgAddress.OA_OH = ZGuid.Empty;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));
	}

	public void TestDPOAuthorizationIsNotAddedWhenConsigneeIsEmptyAndPaymentPartyIs2()
	{
		declaration.JE_OH_Importer = ZGuid.Empty;
		declaration.JE_PaymentMethod = "2";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs2()
	{
		declaration.JE_PaymentMethod = "2";
		declaration.JE_DefermentAccountNumber = "2222222";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "2222222");
			AssertEquals("The new DPO AuthorizationUsage must have consignee as owner", consignee.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "2222222", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs2()
	{
		declaration.JE_PaymentMethod = "2";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));

		CombineAssertions("When Entry Header bound to instruction has filled CH_EntryStatus", () =>
		{
			AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";
		declaration.JE_DefermentAccountNumber = "3333333";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
			AssertEquals("The new DPO AuthorizationUsage must have forwarder as owner", forwarder.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "3333333", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));

		CombineAssertions("When Entry Header bound to instruction has filled CH_EntryStatus", () =>
		{
			AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenForwarderIsEmptyAndPaymentPartyIs3()
	{
		declaration.JE_OH_Forwarder = ZGuid.Empty;
		declaration.JE_PaymentMethod = "3";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";
		declaration.JE_DefermentAccountNumber = "4444444";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "4444444"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "4444444");
			AssertEquals("The new DPO AuthorizationUsage must have representative as owner", representative.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "4444444", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";
		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");

		CombineAssertions("When Entry Header bound to instruction has filled CH_EntryStatus", () =>
		{
			AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "4444444"));
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenRepresentativeIsEmptyAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";
		declaration.RepresentativeOrgAddress.OA_OH = ZGuid.Empty;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "4444444"));
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarationIsEXPAndPaymentMethodIs1()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_PaymentMethod = "1";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarationIsEXPAndPaymentMethodIs2()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_PaymentMethod = "2";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarationIsEXPAndPaymentMethodIs3()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_PaymentMethod = "3";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = ZString.Empty;

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_JE = declaration.PK;

		AssertEquals("No DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";
		declaration.JE_DefermentAccountNumber = "1111111";

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header is null", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", declarant.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "1111111", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs2()
	{
		declaration.JE_PaymentMethod = "2";
		declaration.JE_DefermentAccountNumber = "2222222";

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header is null", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "2222222");
			AssertEquals("The new DPO AuthorizationUsage must have consignee as owner", consignee.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "2222222", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";
		declaration.JE_DefermentAccountNumber = "3333333";

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
			AssertEquals("The new DPO AuthorizationUsage must have forwarder as owner", forwarder.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "3333333", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";
		declaration.JE_DefermentAccountNumber = "4444444";

		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		CombineAssertions("When Entry Header is null", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "4444444"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "4444444");
			AssertEquals("The new DPO AuthorizationUsage must have representative as owner", representative.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "4444444", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIfEntryInstructionAddedToDeclaration()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var authorization = entryInstruction.CusAuthorizationUsages.AddNew();
		authorization.AGC_Code = "DPO";
		authorization.AGC_Number = "121212";
		authorization.AGC_OH_Owner = declarant.PK;

		AssertEquals("Pre: CusAuthorizationUsages Count", 1, entryInstruction.CusAuthorizationUsages.Count);

		declaration.JE_PaymentMethod = "3";
		declaration.JE_DefermentAccountNumber = "3333333";

		entryInstruction.CEI_JE = declaration.PK;
		declaration.CustomsEntryInstructions.Add(entryInstruction);

		AssertEquals("After adding instruction to Declaration, CusAuthorizationUsages Count", 1, entryInstruction.CusAuthorizationUsages.Count);

		CombineAssertions("DPO added as per Payment method and Deferment account number", () =>
		{
			AssertEquals("A DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
			AssertEquals("The new DPO AuthorizationUsage must have forwarder as owner", forwarder.PK, authorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "3333333", authorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", authorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIfDeclarationReOpened()
	{
		declaration.JE_PaymentMethod = "3";
		declaration.JE_DefermentAccountNumber = "3333333";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("Pre: CusAuthorizationUsages Count", 1, entryInstruction.CusAuthorizationUsages.Count);
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
		var declarationPK = declaration.PK;
		Factory.Save();

		var declarationFromDB = NewFactory().Load<JobDeclaration>(declarationPK);
		var entryInstructionFromDB = declarationFromDB.CustomsEntryInstructions[0];

		AssertEquals("After reopening Declaration, CusAuthorizationUsages Count", 1, entryInstructionFromDB.CusAuthorizationUsages.Count);

		var authorizationUsageFromDB = entryInstructionFromDB.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
		AssertEquals("Both refering to same entry", authorizationUsage.PK, authorizationUsageFromDB.PK);
	}
}
