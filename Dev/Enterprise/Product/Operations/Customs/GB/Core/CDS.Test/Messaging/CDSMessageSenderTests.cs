using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.CDS.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GbConstants;
using static Enterprise.Customs.GB.CDS.Constants;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	class CDSMessageSenderTests : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22)]
		public void TestSendCancellation_OnDeclarationLevel()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			CreateValidExternalPassword(declaration);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry1.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_NotRequired;
			entry1.CH_CustomsMessageRemarks = "Reason Desc";
			entry1.CH_EntryStatus = EntryStatusList.Codes.Clear;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);

			var entry1Messages = entry1.Messages;
			AssertEquals(1, entry1Messages.Count);
			AssertType<CDSCancelDeclarationEDIMessage>(entry1Messages[0]);
		}

		[TestDate(2015, 8, 22)]
		public void TestSendWithPermitsWithEndDate()
		{
			TestSendWithPermits(true);
		}

		[TestDate(2015, 8, 22)]
		public void TestSendWithPermitsWithNullEndDate()
		{
			TestSendWithPermits(false);
		}

		[TestDate(2015, 8, 22)]
		public void TestSendWithPermitsWithError()
		{
			var declarationHelper = new DeclarationTestHelper(Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			declarationHelper.CreateRefDataForPermits(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			helper.CreateRefCusProcedure(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "65", "31", "71", "000", "Goods re exported from a customs warehouse.", "EXP", calculateDuty: false, landedCostOnly: false, intoWarehouse: false, outOfWarehouse: true, group: "B1, B2, B4, C1");
			Factory.Save();

			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			CreateValidExternalPassword(declaration);

			var invoice = declaration.Invoices[0];
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invLine = declaration.InvoiceLines[0];
			invLine.JI_LinePrice = 100;
			invLine.JI_Procedure = "3171000";
			var suppDoc = invLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = "9001";
			suppDoc.CSI_ReferenceNumber = "12345";
			suppDoc.CSI_Quantity = 1100;
			suppDoc.CSI_UnitOfQuantity = "KGM";
			suppDoc.CSI_Description = "Doc1";

			declarationHelper.SetupPermits(declaration.Declarant.Header, PermitQtyValIndicatorList.Codes.BTH, true);
			var profile = declaration.JE_CustomsProfile;
			var gateway = declaration.ZG_Gateway;
			declaration.JE_OH_Supplier = declaration.Declarant.Header.PK;
			declaration.JE_MessageType = "EXP";
			declaration.JE_CustomsProfile = profile;
			declaration.ZG_Gateway = gateway;
			cei.CEI_Style = "H1";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_CEI_Instruction = cei.PK;

			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParentForTestShouldSend(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);

			var entryMessages = entry.Messages;

			AssertEquals(1, entryMessages.Count);
			AssertType<CDSNewDeclarationEDIMessage>(entryMessages[0]);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(0, transactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
			AssertContains("The declared customs quantity, 1100.00, for permit 12345 is greater than the permit quantity balance of 1000.00", shutUp.ContinueWithActionMessage);
		}

		void TestSendWithPermits(bool useEndDate)
		{
			var declarationHelper = new DeclarationTestHelper(Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			declarationHelper.CreateRefDataForPermits(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			helper.CreateRefCusProcedure(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "65", "31", "71", "000", "Goods re exported from a customs warehouse.", "EXP", calculateDuty: false, landedCostOnly: false, intoWarehouse: false, outOfWarehouse: true, group: "B1, B2, B4, C1");
			Factory.Save();

			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			CreateValidExternalPassword(declaration);

			var invoice = declaration.Invoices[0];
			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invLine = declaration.InvoiceLines[0];
			invLine.JI_LinePrice = 100;
			invLine.JI_Procedure = "3171000";
			var suppDoc = invLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = "9001";
			suppDoc.CSI_ReferenceNumber = "12345";
			suppDoc.CSI_Quantity = 50.0;
			suppDoc.CSI_UnitOfQuantity = "KGM";
			suppDoc.CSI_Description = "Doc1";

			var suppDoc2 = invLine.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "9001";
			suppDoc2.CSI_ReferenceNumber = "12345";
			suppDoc2.CSI_Quantity = 25;
			suppDoc2.CSI_UnitOfQuantity = "KGM";
			suppDoc2.CSI_Description = "Doc2";

			var suppDoc3 = invLine.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "9001";
			suppDoc3.CSI_ReferenceNumber = "12345";
			suppDoc3.CSI_Value = 40;
			suppDoc3.CSI_Description = "Doc3";

			var suppDoc4 = invLine.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "9001";
			suppDoc4.CSI_ReferenceNumber = "12345";
			suppDoc4.CSI_Value = 30;
			suppDoc4.CSI_Description = "Doc4";

			declarationHelper.SetupPermits(declaration.Declarant.Header, PermitQtyValIndicatorList.Codes.BTH, useEndDate);
			var profile = declaration.JE_CustomsProfile;
			var gateway = declaration.ZG_Gateway;
			declaration.JE_OH_Supplier = declaration.Declarant.Header.PK;
			declaration.JE_MessageType = "EXP";
			declaration.JE_CustomsProfile = profile;
			declaration.ZG_Gateway = gateway;
			cei.CEI_Style = "H1";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_CEI_Instruction = cei.PK;

			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParentForTestShouldSend(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);

			var entryMessages = entry.Messages;
			AssertEquals(1, entryMessages.Count);
			AssertType<CDSNewDeclarationEDIMessage>(entryMessages[0]);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, entry.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
			AssertEquals((ZDecimal)(-75), transactions[0].CPL_TranQty);
			AssertEquals((ZDecimal)(-70), transactions[0].CPL_TranValue);

			suppDoc.CSI_Quantity = 20;
			suppDoc2.CSI_Quantity = 30;

			suppDoc3.CSI_Value = 60;
			suppDoc4.CSI_Value = 70;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry = declaration.CustomsEntryHeaders[0];
			entry.CH_CEI_Instruction = cei.PK;
			entry.CH_CustomsMessageRemarks = "Updating Permits";
			entry.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_NotRequired;
			entry.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;

			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			decWrapper = new JobDeclarationMessageSendingObjectParentForTestShouldSend(declaration);
			sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);

			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions.Length);
			AssertEquals((ZDecimal)(25.00), transactions[1].CPL_TranQty);
			AssertEquals((ZDecimal)(-60.00), transactions[1].CPL_TranValue);
		}

		[TestDate(2015, 8, 22)]
		public void TestSend()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "H1";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "H1";
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_Style = "H1";

			CreateValidExternalPassword(declaration);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = entryInstruction1.PK;
			entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry1.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_NotRequired;
			entry1.CH_CustomsMessageRemarks = "Reason Desc";
			entry1.CH_EntryStatus = EntryStatusList.Codes.Clear;
			MessageSendingTestHelper.CreateOriginalNewMessage(entry1);
			var entry2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = entryInstruction2.PK;
			var entry3 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry3.CH_CEI_Instruction = entryInstruction3.PK;
			MessageSendingTestHelper.CreateOriginalNewMessage(entry3);
			entry3.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry3.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_NotRequired;
			entry3.CH_CustomsMessageRemarks = "Reason Desc";
			entry3.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;
			declaration.PreviousDocuments.AddNew().CSI_Code = "123";

			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParentForTestShouldSend(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);

			var entry1Messages = entry1.Messages;
			AssertEquals(2, entry1Messages.Count);
			AssertType<CDSCancelDeclarationEDIMessage>(entry1Messages[1]);

			var entry2Messages = entry2.Messages;
			AssertEquals(1, entry2Messages.Count);
			AssertType<CDSNewDeclarationEDIMessage>(entry2Messages[0]);

			var entry3Messages = entry3.Messages;
			AssertEquals(3, entry3.Messages.Count);
			AssertType<CDSAmendDeclarationEDIMessage>(entry3Messages[2]);
		}

		[TestDate(2015, 8, 22)]
		public void TestSendingAmendmentsCreatesNewMessageForComparison()
		{
			TestSendingAmendmentsCreatesNewMessageForComparison(GatewayList.Codes.CDS, "CDS");
			TestSendingAmendmentsCreatesNewMessageForComparison(GatewayList.Codes.CCSUKviaNTMsgGW, "CVC");
		}

		void TestSendingAmendmentsCreatesNewMessageForComparison(ZString gateway, ZString expectedMessageApplicationCode)
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = gateway;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			if (gateway == GatewayList.Codes.CCSUKviaNTMsgGW)
			{
				CreateAndStoreBadge("AAA");
				MakeCredential();
				declaration.JE_CustomsProfile = "AAA";
			}

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			CreateValidExternalPassword(declaration);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = cei.PK;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);
			AssertEquals(1, entry1.Messages.Count);
			AssertEquals("NEW", entry1.Messages[0].EM_MessageType);

			declaration.PreviousDocuments.AddNew().CSI_Code = "123";
			declaration.JE_TotalNoOfPacks = 10;

			entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry1.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entry1.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;
			Factory.Save();

			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);
			AssertEquals(3, entry1.Messages.Count);
			AssertEquals("NAM", entry1.Messages[1].EM_MessageType);
			AssertEquals("AMD", entry1.Messages[2].EM_MessageType);

			AssertEquals(expectedMessageApplicationCode, entry1.Messages[1].EM_ApplicationCode);
			AssertEquals(expectedMessageApplicationCode, entry1.Messages[2].EM_ApplicationCode);
		}

		public void TestCheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(this.Factory);
			var sendSilently = new SendsMessagesToCustomsShutterUpperer(false);
			CreateAndStoreBadge("AAA");
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sender = new CDSMessageSender(decWrapper);
			declaration.JE_CustomsProfile = "AAA";
			declaration.ZG_Gateway = "";
			Assert(!sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(declaration, sendSilently));
			AssertContains(CDSJobDeclarationValidation.CSPZG_GatewayValidationError, sendSilently.InvalidOperationText);
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			MakeCredential();
			Assert(sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(declaration, sendSilently));
		}

		protected void MakeCredential(int failureCount = 0)
		{
			var credential = new CredentialsSetting();
			credential.BadgeCode = "AAA";
			credential.Printer = "x";
			credential.Company = "y";
			var allCreds = new CredentialsSettingCollection();
			allCreds.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
		}

		protected void CreateAndStoreBadge(string badgeCode)
		{
			var collection = new BadgeCodeSettingCollection();
			var badge = collection.AddNew();
			badge.BadgeCode = badgeCode;
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			collection.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection);
		}

		[TestDate(2015, 8, 22)]
		public void TestCheckBusinessObjectValidationAndGlbExternalPasswordValidation()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_TransportMode = "XXX";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);

			sender.Send(shutUp);
			Assert(!sender.CanSend);
			AssertContains("No company-level CDS credentials exist", shutUp.InvalidOperationText);

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "My Status Message";
			password.Status = PasswordStatusList.Codes.Invalid;
			password.IsTokenForCDS = true;
			decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			sender = new CDSMessageSenderForTest(decWrapper);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.Send(shutUp);
			Assert(!sender.CanSend);
			AssertContains("Credentials are invalid", shutUp.InvalidOperationText);
			AssertContains("My Status Message", shutUp.InvalidOperationText);

			password.Status = PasswordStatusList.Codes.Deactivated;
			decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			sender = new CDSMessageSenderForTest(decWrapper);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.Send(shutUp);
			Assert(!sender.CanSend);
			AssertContains("Credentials are inactive", shutUp.InvalidOperationText);
			AssertContains("My Status Message", shutUp.InvalidOperationText);

			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 7, 1);
			decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			sender = new CDSMessageSenderForTest(decWrapper);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.Send(shutUp);
			Assert(!sender.CanSend);
			AssertContains("Credentials are invalid", shutUp.InvalidOperationText);
			AssertContains(StatusDescriptions.ExpiredAccessToken, shutUp.InvalidOperationText);

			password.GP_ExpiryDate = new ZDate(2015, 9, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 20);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var checker = new CdsGlbExternalPasswordChecker(declaration, shutUp);
			Assert(checker.PasswordExistsAndOkToSendToCds);
			AssertContains("Your credentials were issued on", shutUp.Warning);

			password.IsTokenForCDS = false;
			password.GP_ExpiryDate = default;
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			checker = new CdsGlbExternalPasswordChecker(declaration, shutUp);
			_ = checker.PasswordExistsAndOkToSendToCds;
			AssertContains("No valid token that is applicable to CDS was found", shutUp.InvalidOperationText);

			password.IsTokenForCDS = true;
			password.GP_ExpiryDate = new ZDate(2015, 12, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 22);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			checker = new CdsGlbExternalPasswordChecker(declaration, shutUp);
			Assert(checker.PasswordExistsAndOkToSendToCds);
			AssertNull(shutUp.Warning);
		}

		[TestDate(2015, 8, 22)]
		public void TestSendingAmendmentsShowsWarningIfLineCountsDoNotMatch()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			CreateValidExternalPassword(declaration);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = cei.PK;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);
			AssertEquals(1, entry1.Messages.Count);
			AssertEquals("NEW", entry1.Messages[0].EM_MessageType);

			var entryLine1 = entry1.MergedLines.AddNew();
			var line = declaration.Invoices[0].InvoiceLines.AddNew();
			line.JI_CL = entryLine1.PK;
			entry1.CH_HighestLineNumber = 1;

			declaration.PreviousDocuments.AddNew().CSI_Code = "123";

			entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry1.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entry1.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;
			Factory.Save();

			var entryLine2 = entry1.MergedLines.AddNew();
			declaration.InvoiceLines[0].JI_CL = entryLine2.PK;
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);

			Assert(shutUp.PastMessages.Contains(string.Format(CultureInfo.CurrentCulture, "The following entries have a different line count to when they were accepted by HMRC.\r\n\r\n{0}\r\n\r\nDo you wish to continue?"
																			, entry1.CH_BGMReference)));
		}

		[TestDate(2015, 8, 22)]
		public void TestSendingAmendmentsDoesNotShowAmendmentReasonWarning()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_TransportMode = "XXX";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			CreateValidExternalPassword(declaration);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = cei.PK;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);

			declaration.PreviousDocuments.AddNew().CSI_Code = "123";
			entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entry1.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entry1.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;

			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);

			Assert(!shutUp.Warning.Contains("E2553	COMMIT IS NOT POSSIBLE WHILST REASON FOR ACTION MISSING"));
		}

		public void TestSemaphoreCreated()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "", "40", "00", "000", "Procedure", "EXP", calculateDuty: false, landedCostOnly: false, intoWarehouse: false, outOfWarehouse: false, group: "H1");

			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = "XX";
			declaration.ZG_Gateway = GatewayList.Codes.CDS;
			var sendsMessagesToCustoms = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = sendsMessagesToCustoms;
			declaration.DoMerge();

			CreateValidExternalPassword(declaration);

			var decWrapper = new JobDeclarationMessageSendingObjectParentForTestShouldSend(declaration);
			decWrapper.LockDeclarationUntilResponseReceived = true;
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, sendsMessagesToCustoms);

			AssertContains("Pre-requisite: should fail to save message", "CW1 does not support building message type", sendsMessagesToCustoms.LastErrorsAsString);
			sendsMessagesToCustoms.LastErrors.Clear();

			AssertEquals("Semaphore should not be created if save failed", expected: false, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(declaration));

			declaration.JE_DeclarationType = "H1";
			declaration.DoMerge();
			sender.Send(declaration, sendsMessagesToCustoms);
			AssertNullOrEmpty("Pre-requisite: should not fail to save message", sendsMessagesToCustoms.LastErrorsAsString);

			AssertEquals("Semaphore should be created when save is successful", expected: true, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(declaration));
			sender.MessageResponseSemaphoreHelper.Dispose();
		}

		[TestDate(2015, 8, 22)]
		public void TestCDSInventoryLinkingConsolidationRequest_Associate()
		{
			var declaration = GetJobDeclaration_for_TestingCDSInventoryLinkingConsolidationRequest();
			var decWrapper = new JobDeclarationMessageSendingObjectParent_ToTest_CDSInventoryLinkingConsolidationRequests(declaration, GbCusDecMessageFunctionsList.Codes.Associate);
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);
			var ediMessage = GetNewestEdiMessage();

			var assertMessage = "For: InventoryLinkingConsolidationRequest = Associate";
			CheckEdiMessage_ToTest_CDSInventoryLinkingConsolidationRequests(ediMessage, assertMessage);

			var expectedXml = EmbeddedResource.GetExpectedMessageXml(@"Messaging.Expected_CDS_InventoryLinkingConsolidationRequest_Associate.xml");
			AssertXmlEquals(
				assertMessage,
				expectedXml,
				ediMessage.EM_MessageText);
		}

		[TestDate(2015, 8, 22)]
		public void TestCDSInventoryLinkingConsolidationRequest_Disassociate()
		{
			var declaration = GetJobDeclaration_for_TestingCDSInventoryLinkingConsolidationRequest();
			var decWrapper = new JobDeclarationMessageSendingObjectParent_ToTest_CDSInventoryLinkingConsolidationRequests(declaration, GbCusDecMessageFunctionsList.Codes.Disassociate);
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);
			var ediMessage = GetNewestEdiMessage();

			var assertMessage = "For: InventoryLinkingConsolidationRequest = Disassociate";
			CheckEdiMessage_ToTest_CDSInventoryLinkingConsolidationRequests(ediMessage, assertMessage);

			var expectedXml = EmbeddedResource.GetExpectedMessageXml(@"Messaging.Expected_CDS_InventoryLinkingConsolidationRequest_Disassociate.xml");
			AssertXmlEquals(
				assertMessage,
				expectedXml,
				ediMessage.EM_MessageText);
		}

		[TestDate(2015, 8, 22)]
		public void TestCDSInventoryLinkingConsolidationRequest_Close()
		{
			var declaration = GetJobDeclaration_for_TestingCDSInventoryLinkingConsolidationRequest();
			var decWrapper = new JobDeclarationMessageSendingObjectParent_ToTest_CDSInventoryLinkingConsolidationRequests(declaration, GbCusDecMessageFunctionsList.Codes.Close);
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var sender = new CDSMessageSenderForTest(decWrapper);
			sender.Send(declaration, shutUp);
			var ediMessage = GetNewestEdiMessage();

			var assertMessage = "For: InventoryLinkingConsolidationRequest = Close";
			CheckEdiMessage_ToTest_CDSInventoryLinkingConsolidationRequests(ediMessage, assertMessage);

			var expectedXml = EmbeddedResource.GetExpectedMessageXml(@"Messaging.Expected_CDS_InventoryLinkingConsolidationRequest_Close.xml");
			AssertXmlEquals(
				assertMessage,
				expectedXml,
				ediMessage.EM_MessageText);
		}

		public void TestCanEntryParticipateEnhancedValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var decWrapper = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			var sender = new CDSMessageSenderForTest(decWrapper);
			var sendingObject = decWrapper.ObjectsToSend.First();
			var allMessageTypeList = new CDSEDIMessageTypeList().GetAllCodes().Concat(new GbCusDecMessageFunctionsList().GetAllCodes());
			foreach (var messageType in allMessageTypeList)
			{
				sendingObject.MessageType = messageType;
				AssertEquals(sendingObject.CanParticipateEnhancedValidation, sender.CanEntryParticipateEnhancedValidationExposed(entry));
			}
		}

		void CheckEdiMessage_ToTest_CDSInventoryLinkingConsolidationRequests(EDIMessage ediMessage, string assertMessage)
		{
			AssertEquals(assertMessage, "CDS", ediMessage.EM_ApplicationCode);
			AssertEquals(assertMessage, "LCQ", ediMessage.EM_MessageType);
			AssertEquals(assertMessage, "QUE", ediMessage.EM_Status);
			AssertEquals(assertMessage, "TRX", ediMessage.EM_ReceiveTransmit);
			AssertEquals(assertMessage, "CusEntryHeader", ediMessage.EM_LinkTable);
		}

		EDIMessage GetNewestEdiMessage()
		{
			var query = new ZQuery
			{
				OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC"
			};
			return Factory.LoadTop1<EDIMessage>(query);
		}

		public static void AssertTextContainsDespiteWhiteNoise(string msg, ZString expected, ZString actual)
		{
			var expectedNoBlanks = Regex.Replace(expected, @"\s", string.Empty);
			var actualNoBlanks = Regex.Replace(actual, @"\s", string.Empty);
			AssertContains(msg, expectedNoBlanks, actualNoBlanks);
		}

		static void AssertXmlEquals(string message, string expectedXml, string actualXml)
		{
			var expected = NormalizeNamespaces(XElement.Parse(expectedXml));
			var actual = NormalizeNamespaces(XElement.Parse(actualXml));

			if (!XNode.DeepEquals(expected, actual))
			{
				throw new Exception($"{message}{System.Environment.NewLine}XML content does not match.\n\nExpected:\n" + expected + "\n\nActual:\n" + actual);
			}
		}

		static XElement NormalizeNamespaces(XElement element, XNamespace defaultNamespace = null)
		{
			var currentNs = element.Name.Namespace;
			var effectiveNs = currentNs != XNamespace.None ? currentNs : defaultNamespace ?? XNamespace.None;

			var normalized = new XElement(
				effectiveNs + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration),
				element.Nodes().Select(n =>
				{
					if (n is XElement child)
					{
						return NormalizeNamespaces(child, effectiveNs);
					}
					else if (n is XText text)
					{
						return new XText(text.Value.Trim());
					}
					else
					{
						return n;
					}
				})
			);

			return normalized;
		}

		JobDeclaration GetJobDeclaration_for_TestingCDSInventoryLinkingConsolidationRequest()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.JE_MessageType = "EXP";
			declaration.ZG_Gateway = "CDS";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = ExportDeclarationTypeList.Codes.DeclarationForExport;
			declaration.JE_MasterUCR = "Master_UCR_Value_01";
			declaration.JE_UCR = "UcrValue01";

			CreateValidExternalPassword(declaration);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.ActiveEntryHeaders.AddNew();

			return declaration;
		}

		void CreateValidExternalPassword(JobDeclaration declaration)
		{
			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(180);
			password.GP_IssueDate = ZDateTime.Today.AddDays(-180);
			password.IsTokenForCDS = true;
		}
	}

	public class CDSMessageSenderForTest : CDSMessageSender
	{
		public CDSMessageSenderForTest(JobDeclarationMessageSendingObjectParentForTest decWrapper) : base(decWrapper)
		{
		}

		public bool CanSend { get; private set; }

		protected override bool ValidateAndShowUserAnyWarningsOrErrors(IBusiness bizO, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how)
		{
			CanSend = base.ValidateAndShowUserAnyWarningsOrErrors(bizO, sendMessagesToCustoms, how);
			return CanSend;
		}

		public void Send(BaseJobDeclaration baseDeclaration, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			Send(sendMessagesToCustoms);
		}

		public bool CanEntryParticipateEnhancedValidationExposed(CusEntryHeader entry) => CanEntryParticipateEnhancedValidation(entry);
	}

	public class JobDeclarationMessageSendingObjectParentForTest : JobDeclarationMessageSendingObjectParent
	{
		public JobDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			if (sendingObjectsCollection == null)
			{
				sendingObjectsCollection = new JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>(Factory);
				foreach (CusEntryHeader header in ParentDeclaration.ActiveEntryHeaders)
				{
					var sendingObject = GetNewSendingObject(header);
					sendingObjectsCollection.Add(sendingObject);
				}
				RegisterEditableChildObject(sendingObjectsCollection);
			}

			return sendingObjectsCollection;
		}

		protected virtual JobDeclarationMessageSendingObject GetNewSendingObject(CusEntryHeader header) => new JobDeclarationMessageSendingObject(header);

		JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject> sendingObjectsCollection;
	}

	public class JobDeclarationMessageSendingObjectParentForTestShouldSend : JobDeclarationMessageSendingObjectParentForTest
	{
		public JobDeclarationMessageSendingObjectParentForTestShouldSend(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override JobDeclarationMessageSendingObject GetNewSendingObject(CusEntryHeader header)
		{
			var sendingObject = base.GetNewSendingObject(header);
			sendingObject.ShouldSend = true;
			return sendingObject;
		}
	}

	class JobDeclarationMessageSendingObjectParent_ToTest_CDSInventoryLinkingConsolidationRequests : JobDeclarationMessageSendingObjectParentForTest
	{
		readonly string msgType;

		public JobDeclarationMessageSendingObjectParent_ToTest_CDSInventoryLinkingConsolidationRequests(JobDeclaration declaration, string messageType)
			: base(declaration)
		{
			msgType = messageType;
		}

		protected override NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var allSendingObjects = base.GetSendingObjectsCollectionCore();
			var tempList = new List<BusinessObject>();
			allSendingObjects.CopyToList(tempList);
			tempList.ConvertAll(x => (JobDeclarationMessageSendingObject)x).ForEach(x => x.MessageType = msgType);
			return allSendingObjects;
		}
	}
}

