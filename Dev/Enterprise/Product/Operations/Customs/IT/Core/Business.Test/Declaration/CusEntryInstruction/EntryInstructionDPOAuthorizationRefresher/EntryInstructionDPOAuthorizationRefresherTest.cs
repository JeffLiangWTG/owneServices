using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryInstructionDPOAuthorizationRefresherTest : BaseEntryInstructionDPOAuthorizationTest
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when declaration is null", () => EntryInstructionDPOAuthorizationRefresher.New(null));
		AssertNoExceptionThrown("No exception when declaration is valid", () => EntryInstructionDPOAuthorizationRefresher.New(declaration.CustomsEntryInstructions));
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			declaration.JE_DefermentAccountNumber = "1111111";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", declarant.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "1111111", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertNoDPOAuthorizationUsageIsAdded("When Entry Header bound to instruction has not empty CH_EntryStatus", entryInstruction, "1111111");
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarantIsNullAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";
		declaration.DeclarantOrgAddress.OA_OH = ZGuid.Empty;
		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Declarant is empty", entryInstruction, "1111111");
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs2()
	{
		declaration.JE_PaymentMethod = "2";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			declaration.JE_DefermentAccountNumber = "2222222";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "2222222");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", consignee.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "2222222", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs2()
	{
		declaration.JE_PaymentMethod = "2";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertNoDPOAuthorizationUsageIsAdded("When Entry Header bound to instruction has not empty CH_EntryStatus", entryInstruction, "2222222");
	}

	public void TestDPOAuthorizationIsNotAddedWhenConsigneeIsNullAndPaymentPartyIs2()
	{
		declaration.JE_OH_Importer = ZGuid.Empty;
		declaration.JE_OA_Representative = representative.MainAddress.PK;
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		declaration.JE_PaymentMethod = "2";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Consignee is empty", entryInstruction, "2222222");
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			declaration.JE_DefermentAccountNumber = "3333333";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", forwarder.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "3333333", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertNoDPOAuthorizationUsageIsAdded("When Entry Header bound to instruction has not empty CH_EntryStatus", entryInstruction, "3333333");
	}

	public void TestDPOAuthorizationIsNotAddedWhenForwarderIsNullAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";
		declaration.JE_OH_Forwarder = ZGuid.Empty;
		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Forwarder is empty", entryInstruction, "333333");
	}

	public void TestDPOAuthorizationIsAddedWhenEntryStateIsEmptyAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("When Entry Header bound to instruction has empty CH_EntryStatus", () =>
		{
			declaration.JE_DefermentAccountNumber = "4444444";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "4444444"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "4444444");
			AssertEquals("The new DPO AuthorizationUsage must have representative as owner", representative.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "4444444", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenEntryStateIsNotEmptyAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");
		AssertNoDPOAuthorizationUsageIsAdded("When Entry Header bound to instruction has not empty CH_EntryStatus", entryInstruction, "4444444");
	}

	public void TestDPOAuthorizationIsNotAddedWhenRepresentativeIsNullAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";
		declaration.RepresentativeOrgAddress.OA_OH = ZGuid.Empty;
		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Representative is empty", entryInstruction, "4444444");
	}

	public void TestDPOAuthorizationUsageIsAddedOnMultipleInstructions()
	{
		declaration.JE_PaymentMethod = "1";
		declaration.JE_DefermentAccountNumber = ZString.Empty;

		var entryInstruction1 = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		var entryInstruction2 = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		var entryInstruction3 = AddTestInstructionAndAuthorizationUsages(entryStatus: "AAA");

		CombineAssertions("Only Instructions bound to entry Headers having empty status must be modified", () =>
		{
			declaration.JE_DefermentAccountNumber = "1111111";
			AssertEquals("A New DPO Authorization must be added on entryInstruction1", 1, entryInstruction1.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));
			var cusAuthorizationUsage = entryInstruction1.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", declarant.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "1111111", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);

			AssertEquals("A New DPO Authorization must be added on entryInstruction2", 1, entryInstruction2.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));
			cusAuthorizationUsage = entryInstruction2.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", declarant.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "1111111", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);

			AssertEquals("NoNew DPO Authorization must be added on entryInstruction3", 0, entryInstruction3.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarationIsEXPAndPaymentMethodIs1()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_PaymentMethod = "1";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Declaration is EXP and MethodOfPayment is 1", entryInstruction, "1111111");
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarationIsEXPAndPaymentMethodIs2()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_PaymentMethod = "2";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Declaration is EXP and MethodOfPayment is 2", entryInstruction, "2222222");
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarationIsEXPAndPaymentMethodIs3()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_PaymentMethod = "3";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Declaration is EXP and MethodOfPayment is 3", entryInstruction, "3333333");
	}

	public void TestDPOAuthorizationsAreDeletedWhenDefermentAccountNoIsEmptyAndDeclarationIsIMP()
	{
		declaration.JE_MessageType = "IMP";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("Authorizations must be deleted when Declaration is IMP", () =>
		{
			declaration.JE_DefermentAccountNumber = "WHATEVER";
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertEquals("existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("Existing DPO authorizations must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Code == "DPO"));
		});
	}

	public void TestDPOAuthorizationsAreNotDeletedWhenDefermentAccountNoIsEmptyAndDeclarationIsEXP()
	{
		declaration.JE_MessageType = "EXP";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("Authorizations must not be deleted when Declaration is EXP", () =>
		{
			declaration.JE_DefermentAccountNumber = "WHATEVER";
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertEquals("existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("Existing DPO authorizations must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Code == "DPO"));
		});
	}

	public void TestDPOAuthorizationIsNotAddedWhenDeclarantIsEmptyAndPaymentMethodIs1()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "1";
		declaration.JE_DefermentAccountNumber = "WHATEVER";
		declaration.JE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
		declaration.DeclarantOrgAddress.OA_OH = ZGuid.Empty;

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Declarant is empty and MethodOfPayment is 1", entryInstruction, "1111111");
	}

	public void TestDPOAuthorizationIsNotAddedWhenConsigneeIsEmptyAndPaymentMethodIs2()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "2";
		declaration.JE_DefermentAccountNumber = "WHATEVER";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Consignee is empty and MethodOfPayment is 2", entryInstruction, "2222222");
	}

	public void TestDPOAuthorizationIsNotAddedWhenForwarderIsEmptyAndPaymentMethodIs3()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "3";
		declaration.JE_DefermentAccountNumber = "WHATEVER";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		AssertNoDPOAuthorizationUsageIsAdded("When Forwarder is empty and MethodOfPayment is 3", entryInstruction, "3333333");
	}

	public void TestDPOAuthorizationIsNotAddedWhenRepresentativeIsEmptyAndPaymentMethodIs4()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "4";
		declaration.JE_DefermentAccountNumber = "WHATEVER";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);
		AssertNoDPOAuthorizationUsageIsAdded("When Declarant is empty and MethodOfPayment is 4", entryInstruction, "4444444");
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs1()
	{
		declaration.JE_PaymentMethod = "1";

		var entryInstruction = AddTestInstructionAndAuthorizationUsagesWithNoEntryHeader();

		CombineAssertions("When instruction has no Entry Header", () =>
		{
			declaration.JE_DefermentAccountNumber = "1111111";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "1111111"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "1111111");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", declarant.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "1111111", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs2()
	{
		declaration.JE_PaymentMethod = "2";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("When instruction has no Entry Header", () =>
		{
			declaration.JE_DefermentAccountNumber = "2222222";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "2222222"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "2222222");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", consignee.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "2222222", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs3()
	{
		declaration.JE_PaymentMethod = "3";

		var entryInstruction = AddTestInstructionAndAuthorizationUsages(entryStatus: ZString.Empty);

		CombineAssertions("When instruction has no Entry Header", () =>
		{
			declaration.JE_DefermentAccountNumber = "3333333";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "3333333"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "3333333");
			AssertEquals("The new DPO AuthorizationUsage must have declarant as owner", forwarder.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "3333333", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationIsAddedWhenEntryHeaderIsNullAndPaymentPartyIs4()
	{
		declaration.JE_PaymentMethod = "4";

		var entryInstruction = AddTestInstructionAndAuthorizationUsagesWithNoEntryHeader();

		CombineAssertions("When instruction has no Entry Header", () =>
		{
			declaration.JE_DefermentAccountNumber = "4444444";
			AssertEquals("When deferment account number is filled, existing DPO Authorization must be deleted", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("When deferment account number is filled, existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("A New DPO Authorization must be added", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "4444444"));

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Number == "4444444");
			AssertEquals("The new DPO AuthorizationUsage must have representative as owner", representative.PK, cusAuthorizationUsage.AGC_OH_Owner);
			AssertEquals("The new DPO AuthorizationUsage must have DefermentAccountNumber as value", "4444444", cusAuthorizationUsage.AGC_Number);
			AssertEquals("The new DPO AuthorizationUsage must have DPO as code", "DPO", cusAuthorizationUsage.AGC_Code);
		});
	}

	public void TestDPOAuthorizationForUCC6Export_ExistingInstructionAndLines()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();

		var headerOne = declaration.CustomsEntryHeaders.AddNew();
		var headerTwo = declaration.CustomsEntryHeaders.AddNew();
		var headerThree = declaration.CustomsEntryHeaders.AddNew();

		headerOne.CH_EntryStatus = "ACO";
		headerOne.MergedLines.AddNew();
		headerOne.CH_CEI_Instruction = entryInstruction1.PK;

		headerTwo.CH_EntryStatus = "ABC";
		headerTwo.AllEntryLines.AddNew();
		headerTwo.AllEntryLines.AddNew();
		headerThree.CH_CEI_Instruction = entryInstruction2.PK;

		headerThree.CH_EntryStatus = ZString.Empty;
		headerThree.CH_CEI_Instruction = entryInstruction3.PK;

		var paymentMethodsAndExpectedPartyIds = SetupDataForTestingUcc6ExportCusAuthorizationRecordsCreation();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			foreach (var (paymentMethod, expectedPartyId) in paymentMethodsAndExpectedPartyIds)
			{
				CombineAssertions($"DPO Authorization Usage Record handling when Payment Method: {paymentMethod}", () =>
				{
					declaration.JE_PaymentMethod = paymentMethod;
					declaration.JE_DefermentAccountNumber = "12345";
					AssertCusAuthRecords(entryInstruction1, 0, ZGuid.Empty, "DPO", "");
					AssertCusAuthRecords(entryInstruction2, 1, expectedPartyId, "DPO", "12345");
					AssertCusAuthRecords(entryInstruction3, 1, expectedPartyId, "DPO", "12345");

					declaration.JE_DefermentAccountNumber = "562211";
					AssertCusAuthRecords(entryInstruction1, 0, ZGuid.Empty, "DPO", "");
					AssertCusAuthRecords(entryInstruction2, 1, expectedPartyId, "DPO", "562211");
					AssertCusAuthRecords(entryInstruction3, 1, expectedPartyId, "DPO", "562211");

					declaration.JE_DefermentAccountNumber = ZString.Empty;
					AssertCusAuthRecords(entryInstruction1, 0, ZGuid.Empty, "DPO", "");
					AssertCusAuthRecords(entryInstruction2, 0, expectedPartyId, "DPO", "");
					AssertCusAuthRecords(entryInstruction3, 0, expectedPartyId, "DPO", "");

					declaration.JE_DefermentAccountNumber = "909211";
					AssertCusAuthRecords(entryInstruction1, 0, ZGuid.Empty, "DPO", "");
					AssertCusAuthRecords(entryInstruction2, 1, expectedPartyId, "DPO", "909211");
					AssertCusAuthRecords(entryInstruction3, 1, expectedPartyId, "DPO", "909211");
				});
			}
		}
	}

	public void TestDPOAuthorizationForUCC6Export_NewEntryInstruction()
	{
		var paymentMethodsAndExpectedPartyIds = SetupDataForTestingUcc6ExportCusAuthorizationRecordsCreation();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var header = declaration.CustomsEntryHeaders.AddNew();
		header.CH_Status = "";
		header.AllEntryLines.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			foreach (var (paymentMethod, expectedPartyId) in paymentMethodsAndExpectedPartyIds)
			{
				CombineAssertions($"DPO Authorization Usage Record Creation when Payment Method: {paymentMethod}", () =>
				{
					declaration.JE_PaymentMethod = paymentMethod;
					declaration.JE_DefermentAccountNumber = "12345";

					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					header.CH_CEI_Instruction = entryInstruction.PK;
					AssertCusAuthRecords(entryInstruction, 1, expectedPartyId, "DPO", "12345");

					declaration.JE_DefermentAccountNumber = "562211";
					AssertCusAuthRecords(entryInstruction, 1, expectedPartyId, "DPO", "562211");

					declaration.JE_DefermentAccountNumber = ZString.Empty;
					AssertCusAuthRecords(entryInstruction, 0, expectedPartyId, "DPO", "");

					declaration.JE_DefermentAccountNumber = "909211";
					AssertCusAuthRecords(entryInstruction, 1, expectedPartyId, "DPO", "909211");
				});
			}
		}
	}

	void AssertNoDPOAuthorizationUsageIsAdded(ZString message, CusEntryInstruction entryInstruction, ZString defermentAccountNumber)
	{
		CombineAssertions(message, () =>
		{
			declaration.JE_DefermentAccountNumber = defermentAccountNumber;
			AssertEquals("Despite deferment account number is filled, existing DPO Authorization is not deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "123456"));
			AssertEquals("existing not DPO Authorization must not be deleted", 1, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == "888888"));
			AssertEquals("No New DPO Authorization must be added", 0, entryInstruction.CusAuthorizationUsages.Count(x => x.AGC_Number == defermentAccountNumber));
		});
	}

	CusEntryInstruction AddTestInstructionAndAuthorizationUsagesWithNoEntryHeader()
	{
		var genericOrg = Factory.NewWithValidTestData<OrgHeader>();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var dpoAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		dpoAuthorizationUsage.AGC_OH_Owner = genericOrg.PK;
		dpoAuthorizationUsage.AGC_Number = "123456";
		dpoAuthorizationUsage.AGC_Code = "DPO";

		var genericAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		genericAuthorizationUsage.AGC_OH_Owner = genericOrg.PK;
		genericAuthorizationUsage.AGC_Number = "888888";
		genericAuthorizationUsage.AGC_Code = "ZIZ";

		return entryInstruction;
	}

	(string, ZGuid)[] SetupDataForTestingUcc6ExportCusAuthorizationRecordsCreation()
	{
		var (declarantID, importerID, forwarderID, supplierID, exporterID) = SetupDeclarationWithParties(declaration);
		var paymentMethodsAndExpectedPartyIds = new[]
		{
			(Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions, declarantID),
			(Ucc6ExportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions, importerID),
			(Ucc6ExportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions, forwarderID),
			(Ucc6ExportDefermentMethodList.Codes.SuppliersAccountFromCustomsDecisions, supplierID),
			(Ucc6ExportDefermentMethodList.Codes.ExportersAccountFromCustomsDecisions, exporterID),
		};

		return paymentMethodsAndExpectedPartyIds;
	}

	(ZGuid declaratnID, ZGuid importerID, ZGuid forwarderID, ZGuid supplierID, ZGuid exporterID) SetupDeclarationWithParties(JobDeclaration jobDeclaration)
	{
		var declarant = Factory.New<OrgHeader>();
		var importer = Factory.New<OrgHeader>();
		var forwarder = Factory.New<OrgHeader>();
		var supplier = Factory.New<OrgHeader>();
		var exporter = Factory.New<OrgHeader>();

		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_OH_Forwarder = forwarder.PK;
		declaration.JE_OH_Supplier = supplier.PK;
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.ExporterDocAddress.OrganisationPK = exporter.PK;
		declaration.JE_OA_Representative = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

		return (declarant.PK, importer.PK, forwarder.PK, supplier.PK, exporter.PK);
	}

	void AssertCusAuthRecords(CusEntryInstruction entryInstruction, int expectedRecordCount, ZGuid ownerId, string recordType, string referenceNumber)
	{
		var records = entryInstruction.CusAuthorizationUsages.Where(c => c.AGC_Code == recordType).ToArray();
		AssertEquals($"CusAuthorizationUsages Record Count for AuthType: {recordType} with Number: {referenceNumber}", expectedRecordCount, records.Length);
		if (expectedRecordCount > 0)
		{
			Assert($"CusAuthorizationUsages Records Contain OwnerId: {ownerId} and Ref. Number: {referenceNumber}", records.All(r => r.AGC_Number == referenceNumber && r.AGC_OH_Owner == ownerId));
		}
	}
}
