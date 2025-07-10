using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.MessageSending.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	class SendPrelodgeAmendmentOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestSendPrelodgeAmendment()
		{
			var deltaG1Declaration = deltaGMessageSenderTest.CreateDeclaration(false);
			deltaG1Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			deltaG1Declaration.JE_CustomsProfile = "DGE001";
			deltaG1Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			deltaG1Declaration.Supplier.MainAddress.OA_PostCode = "24100";
			deltaG1Declaration.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			deltaG1Declaration.CustomsEntryHeaders[0].CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			deltaG1Declaration.CustomsEntryHeaders[0].CH_BGMReference = "2022000001";
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			deltaG1Declaration.DoMerge(shutUp);

			var deltaG2Declaration = deltaGMessageSenderTest.CreateDeclaration(false);
			deltaG2Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			deltaG2Declaration.JE_CustomsProfile = "DGE002";
			deltaG2Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			deltaG2Declaration.Supplier.MainAddress.OA_PostCode = "24100";
			deltaG2Declaration.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			deltaG2Declaration.CustomsEntryHeaders[0].CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			deltaG2Declaration.CustomsEntryHeaders[0].CH_BGMReference = "2022000002";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			deltaG2Declaration.DoMerge(shutUp);

			var deltaG2DeclarationWithWrongStatus = deltaGMessageSenderTest.CreateDeclaration(false);
			deltaG2DeclarationWithWrongStatus.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			deltaG2DeclarationWithWrongStatus.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			deltaG2DeclarationWithWrongStatus.Supplier.MainAddress.OA_PostCode = "24100";
			deltaG2DeclarationWithWrongStatus.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			deltaG2DeclarationWithWrongStatus.CustomsEntryHeaders[0].CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			deltaG2DeclarationWithWrongStatus.CustomsEntryHeaders[0].CH_BGMReference = "2022000003";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			deltaG2DeclarationWithWrongStatus.DoMerge(shutUp);

			var deltaIEDeclaration = deltaGMessageSenderTest.CreateDeclaration(false);
			deltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			deltaIEDeclaration.Supplier.MainAddress.OA_PostCode = "24100";
			deltaIEDeclaration.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			deltaIEDeclaration.CustomsEntryHeaders[0].CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;
			deltaIEDeclaration.CustomsEntryHeaders[0].CH_BGMReference = "2022000004";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			deltaIEDeclaration.DoMerge(shutUp);

			var deltaIEDeclarationWithWrongStatus = deltaGMessageSenderTest.CreateDeclaration(false);
			deltaIEDeclarationWithWrongStatus.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			deltaIEDeclarationWithWrongStatus.Supplier.MainAddress.OA_PostCode = "24100";
			deltaIEDeclarationWithWrongStatus.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			deltaIEDeclarationWithWrongStatus.CustomsEntryHeaders[0].CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Amended;
			deltaIEDeclarationWithWrongStatus.CustomsEntryHeaders[0].CH_BGMReference = "2022000005";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			deltaIEDeclarationWithWrongStatus.DoMerge(shutUp);

			BusinessObject[] selectedDeclarations = new BusinessObject[] { deltaG1Declaration, deltaG2Declaration, deltaG2DeclarationWithWrongStatus, deltaIEDeclaration, deltaIEDeclarationWithWrongStatus };

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new SendPrelodgeAmendmentOperationalActionRunner(log, selectedDeclarations);

			CombineAssertions(() =>
			{
				runner.SendPrelodgeAmendment(false);
				AssertContains("2 entries should have been found.", "INFO: 2 DeltaG entries with status ANTICIPE found", log.MessagesString());

				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "MAP");
				var foundMessages = Factory.Load<EDIMessage>(query);
				deltaG1Declaration.RunPreSaveValidation();
				Assert(deltaG1Declaration.HasMessageErrors);
				AssertEquals("Declaration has Message Errors and is not allowed to send messages with Message Errors, No message MAP should have been created.", 0, foundMessages.Length);
				AssertContains("Log should reflect fact that MAP message could not be sent.", "ERROR: Error creating MAP message for entry 2022000001", log.MessagesString());

				query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "MDA");
				foundMessages = Factory.Load<EDIMessage>(query);
				deltaG2Declaration.RunPreSaveValidation();
				Assert(deltaG2Declaration.HasMessageErrors);
				AssertEquals("Declaration has Message Errors and is not allowed to send messages with Message Errors, No message MAP should have been created.", 0, foundMessages.Length);
				AssertContains("Log should reflect fact that MAP message could not be sent.", "ERROR: Error creating MDA message for entry 2022000002", log.MessagesString());

				AssertContains("1 Delta IE entry should have been found.", "INFO: 1 DeltaIE entries with status REG found", log.MessagesString());

				query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "413");
				foundMessages = Factory.Load<EDIMessage>(query);
				deltaIEDeclaration.RunPreSaveValidation();
				Assert(deltaIEDeclaration.HasMessageErrors);
				AssertEquals("Declaration has Message Errors and is not allowed to send messages with Message Errors, No message 413 should have been created.", 0, foundMessages.Length);
				AssertContains("Log should reflect fact that 413 message could not be sent.", "ERROR: Error creating 413 message for entry 2022000004", log.MessagesString());

				AssertContains("No message should have been sent for non ANTICIPE status Delta G entry 2022000003.", "INFO: Entry 2022000003 is not available for amendment because its status is 010", log.MessagesString());
				AssertContains("No message should have been sent for non REG status Delta IE entry 2022000005.", "INFO: Entry 2022000005 is not available for amendment because its status is AMD.", log.MessagesString());
			});

			CombineAssertions(() =>
			{
				runner.SendPrelodgeAmendment(true);
				AssertContains("2 entries should have been found.", "INFO: 2 DeltaG entries with status ANTICIPE found", log.MessagesString());

				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "MAP");
				var foundMessages = Factory.Load<EDIMessage>(query);
				deltaG1Declaration.RunPreSaveValidation();
				Assert(deltaG1Declaration.HasMessageErrors);
				AssertEquals("Declaration has Message Errors but allows sending messages With Message Errors, One message MAP should have been created.", 1, foundMessages.Length);
				AssertContains("Log should reflect fact that MAP message was sent.", "INFO: Successfully created MAP message for entry 2022000001", log.MessagesString());

				query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "MDA");
				foundMessages = Factory.Load<EDIMessage>(query);
				deltaG2Declaration.RunPreSaveValidation();
				Assert(deltaG2Declaration.HasMessageErrors);
				AssertEquals("Declaration has Message Errors but allows sending messages With Message Errors, One message MDA should have been created.", 1, foundMessages.Length);
				AssertContains("Log should reflect fact that MDA message was sent.", "INFO: Successfully created MDA message for entry 2022000002", log.MessagesString());

				AssertContains("1 Delta IE entry should have been found.", "INFO: 1 DeltaIE entries with status REG found", log.MessagesString());

				query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "413");
				foundMessages = Factory.Load<EDIMessage>(query);
				deltaIEDeclaration.RunPreSaveValidation();
				Assert(deltaIEDeclaration.HasMessageErrors);
				AssertEquals("Declaration has Message Errors but allows sending messages With Message Errors, One message 413 should have been created.", 1, foundMessages.Length);
				AssertContains("Log should reflect fact that MDA message was sent.", "INFO: Successfully created 413 message for entry 2022000004", log.MessagesString());
				AssertContains("Motivation should be MODIF", "\"amendmentMotivation\": \"MODIF\"\r\n", foundMessages[0].EM_MessageText);

				AssertContains("No message should have been sent for non ANTICIPE status entry 2022000003.", "INFO: Entry 2022000003 is not available for amendment because its status is 010", log.MessagesString());
				AssertContains("No message should have been sent for non REG status Delta IE entry 2022000005.", "INFO: Entry 2022000005 is not available for amendment because its status is AMD.", log.MessagesString());
			});

			CombineAssertions(() =>
			{
				selectedDeclarations = new BusinessObject[] { deltaG2DeclarationWithWrongStatus, deltaIEDeclarationWithWrongStatus };
				log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
				runner = new SendPrelodgeAmendmentOperationalActionRunner(log, selectedDeclarations);
				runner.SendPrelodgeAmendment(true);
				AssertContains("No Delta G entry with status ANTICIPE was found.", "INFO: No Delta G entry with status ANTICIPE was found.", log.MessagesString());
				AssertContains("No Delta IE entry with status REG was found.", "INFO: No Delta IE entry with status REG was found.", log.MessagesString());
			});
		}

		public void TestSendPrelodgeAmendmentSkipsNonDeltaGDeclarations()
		{
			var declaration = deltaGMessageSenderTest.CreateDeclaration(false);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.Supplier.MainAddress.OA_PostCode = "24100";
			declaration.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			declaration.CustomsEntryHeaders[0].CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(shutUp);

			BusinessObject[] selectedDeclarations = new BusinessObject[] { declaration };

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new SendPrelodgeAmendmentOperationalActionRunner(log, selectedDeclarations);
			runner.SendPrelodgeAmendment(false);

			AssertContains("Message asserting that no entry was found.", $"No Delta G entry with status {EntryStatusDescriptionCodeList.Descriptions.ES050} was found.", log.MessagesString());
		}

		public void TestSendPrelodgeAmendmentSkipsEntriesWithWrongStatus()
		{
			var declaration = deltaGMessageSenderTest.CreateDeclaration(false);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.Supplier.MainAddress.OA_PostCode = "24100";
			declaration.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			declaration.CustomsEntryHeaders[0].CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(shutUp);

			BusinessObject[] selectedDeclarations = new BusinessObject[] { declaration };

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var runner = new SendPrelodgeAmendmentOperationalActionRunner(log, selectedDeclarations);
			runner.SendPrelodgeAmendment(false);

			AssertContains("Message asserting that no entries was found.", $"No Delta G entry with status {EntryStatusDescriptionCodeList.Descriptions.ES050} was found.", log.MessagesString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			deltaGMessageSenderTest = new DeltaGMessageSenderTest();
		}

		DeltaGMessageSenderTest deltaGMessageSenderTest;
	}
}
