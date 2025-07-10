using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class SendValideeMessageOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestSendValideeMessage()
		{
			var dec1 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec1.JE_PaymentMethod = "M";
			dec1.JE_CustomsProfile = "DGE002";
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.Supplier.MainAddress.OA_PostCode = "24100";
			dec1.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec1.CustomsEntryHeaders[0].CH_EntryStatus = EntryActionCodeList.Codes.ANT;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec1.DoMerge(shutUp);

			var dec2 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec2.JE_PaymentMethod = "A";
			dec2.JE_CustomsProfile = "DGE002";
			dec2.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec1.CustomsEntryHeaders[0].CH_EntryStatus = EntryActionCodeList.Codes.D2M;
			dec2.DoMerge(shutUp);

			var dec3 = deltaGMessageSenderTest.CreateDeclaration(true);
			dec3.JE_PaymentMethod = "C";
			dec3.JE_CustomsProfile = "DGI002";
			dec3.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec3.Supplier.MainAddress.OA_PostCode = "24100";
			dec3.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec3.CustomsEntryHeaders[0].CH_EntryStatus = EntryActionCodeList.Codes.ANT;
			dec3.DoMerge(shutUp);

			var dec4 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec4.JE_PaymentMethod = "M";
			dec4.JE_CustomsProfile = "DGE002";
			dec4.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec4.Supplier.MainAddress.OA_PostCode = "24100";
			dec4.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today.AddDays(-100);
			dec4.CustomsEntryHeaders[0].CH_EntryStatus = EntryActionCodeList.Codes.ANT;
			dec4.DoMerge(shutUp);
			Factory.Save();

			BusinessObject[] selectedDeclarations = new BusinessObject[] { dec1, dec2, dec3, dec4 };

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var sendValideeMessageOperationalActionRunner = new SendValideeMessageOperationalActionRunner(log, selectedDeclarations);
			sendValideeMessageOperationalActionRunner.SendValidee(false);

			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "VAL");
			var foundMessages = Factory.Load<EDIMessage>(query);

			AssertEquals("No messages should have been created", 0, foundMessages.Length);
			AssertContains("Message asserting that no entries could have been treated", "No entries with status " + EntryStatusDescriptionCodeList.Descriptions.ES050 + " or " + EntryStatusDescriptionCodeList.Descriptions.ES055 + " were found.", log.MessagesString());
		}

		public void TestSendValideeMessageVAAEdition()
		{
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RepresentativeID = "TESTREPID";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			var dec1 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec1.JE_PaymentMethod = "M";
			dec1.JE_CustomsProfile = "DGE002";
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.Supplier.MainAddress.OA_PostCode = "24100";
			dec1.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec1.CustomsEntryHeaders[0].CH_EntryStatus = "022";
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec1.DoMerge(shutUp);

			var dec2 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec2.JE_PaymentMethod = "M";
			dec2.JE_CustomsProfile = "DGE002";
			dec2.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec2.Supplier.MainAddress.OA_PostCode = "24100";
			dec2.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec2.CustomsEntryHeaders[0].CH_EntryStatus = "050";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec2.DoMerge(shutUp);

			var dec3 = deltaGMessageSenderTest.CreateDeclaration(true);
			orgCusAccount.CZ_OH = dec3.Importer.PK;
			dec3.JE_PaymentMethod = "C";
			dec3.JE_CustomsProfile = "DGI002";
			dec3.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec3.JE_OH_Importer = dec3.Importer.PK;
			dec3.JE_CustomsProfile = "TESTACC";
			dec3.Supplier.MainAddress.OA_PostCode = "24100";
			dec3.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec3.CustomsEntryHeaders[0].CH_EntryStatus = "050";
			dec3.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec3.DoMerge(shutUp);

			Factory.Save();

			BusinessObject[] selectedDeclarations = new BusinessObject[] { dec1, dec2, dec3 };

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var sendValideeMessageOperationalActionRunner = new SendValideeMessageOperationalActionRunner(log, selectedDeclarations);

			CombineAssertions(() =>
			{
				sendValideeMessageOperationalActionRunner.SendValidee(false);
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "VAA");
				var foundMessages = Factory.Load<EDIMessage>(query);

				dec1.RunPreSaveValidation();
				dec2.RunPreSaveValidation();
				dec3.RunPreSaveValidation();
				Assert(dec1.HasMessageErrors);
				Assert(dec2.HasMessageErrors);
				Assert(dec3.HasMessageErrors);
				AssertEquals("Declaration has Message Errors and is not allowed to send messages with Message Errors, No VAA message should have been created.", 0, foundMessages.Length);
				AssertContains("Message asserting that 022 entry can't be validated", "Entry " + dec1.CustomsEntryHeaders[0].CH_BGMReference + " is not available for validation, the entry status is 022.", log.MessagesString());
			});

			CombineAssertions(() =>
			{
				sendValideeMessageOperationalActionRunner.SendValidee(true);
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "VAA");
				var foundMessages = Factory.Load<EDIMessage>(query);

				dec1.RunPreSaveValidation();
				dec2.RunPreSaveValidation();
				dec3.RunPreSaveValidation();
				Assert(dec1.HasMessageErrors);
				Assert(dec2.HasMessageErrors);
				Assert(dec3.HasMessageErrors);
				AssertEquals("Declaration has Message Errors but allows sending messages With Message Errors, 2 VAA messages should have been created", 2, foundMessages.Length);
				AssertContains("Message asserting that 022 entry can't be validated", "Entry " + dec1.CustomsEntryHeaders[0].CH_BGMReference + " is not available for validation, the entry status is 022.", log.MessagesString());
			});
		}

		public void TestSendValideeMessageEAVEdition()
		{
			var dec1 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec1.JE_PaymentMethod = "M";
			dec1.JE_CustomsProfile = "DGE001";
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			dec1.Supplier.MainAddress.OA_PostCode = "24100";
			dec1.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec1.CustomsEntryHeaders[0].CH_EntryStatus = "022";
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec1.DoMerge(shutUp);

			var dec2 = deltaGMessageSenderTest.CreateDeclaration(false);
			dec2.JE_PaymentMethod = "M";
			dec2.JE_CustomsProfile = "DGE001";
			dec2.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			dec2.Supplier.MainAddress.OA_PostCode = "24100";
			dec2.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec2.CustomsEntryHeaders[0].CH_EntryStatus = "055";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec2.DoMerge(shutUp);

			var dec3 = deltaGMessageSenderTest.CreateDeclaration(true);
			dec3.JE_PaymentMethod = "C";
			dec3.JE_CustomsProfile = "DGI001";
			dec3.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			dec3.Supplier.MainAddress.OA_PostCode = "24100";
			dec3.CustomsEntryHeaders[0].CH_EntrySubmittedDate = ZDateTime.Today;
			dec3.CustomsEntryHeaders[0].CH_EntryStatus = "055";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			dec3.DoMerge(shutUp);

			Factory.Save();

			BusinessObject[] selectedDeclarations = new BusinessObject[] { dec1, dec2, dec3 };

			var log = new Services.OperationalActions.Support.Testing.DummyOperationalActionSectionLog();
			var sendValideeMessageOperationalActionRunner = new SendValideeMessageOperationalActionRunner(log, selectedDeclarations);

			CombineAssertions(() =>
			{
				sendValideeMessageOperationalActionRunner.SendValidee(false);
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "EAV");
				var foundMessages = Factory.Load<EDIMessage>(query);

				dec1.RunPreSaveValidation();
				dec2.RunPreSaveValidation();
				dec3.RunPreSaveValidation();
				Assert(dec1.HasMessageErrors);
				Assert(dec2.HasMessageErrors);
				Assert(dec3.HasMessageErrors);
				AssertEquals("Declaration has Message Errors and is not allowed to send messages with Message Errors, No EAV message should have been created.", 0, foundMessages.Length);
				AssertContains("Message asserting that 022 entry can't be validated", "Entry " + dec1.CustomsEntryHeaders[0].CH_BGMReference + " is not available for validation, the entry status is 022.", log.MessagesString());
			});

			CombineAssertions(() =>
			{
				sendValideeMessageOperationalActionRunner.SendValidee(true);
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, "EAV");
				var foundMessages = Factory.Load<EDIMessage>(query);

				dec1.RunPreSaveValidation();
				dec2.RunPreSaveValidation();
				dec3.RunPreSaveValidation();
				Assert(dec1.HasMessageErrors);
				Assert(dec2.HasMessageErrors);
				Assert(dec3.HasMessageErrors);
				AssertEquals("Declaration has Message Errors but allows sending messages With Message Errors, 2 EAV messages should have been created", 2, foundMessages.Length);
				AssertContains("Message asserting that 022 entry can't be validated", "Entry " + dec1.CustomsEntryHeaders[0].CH_BGMReference + " is not available for validation, the entry status is 022.", log.MessagesString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			deltaGMessageSenderTest = new MessageSending.Testing.DeltaGMessageSenderTest();
		}

		MessageSending.Testing.DeltaGMessageSenderTest deltaGMessageSenderTest;
	}
}
