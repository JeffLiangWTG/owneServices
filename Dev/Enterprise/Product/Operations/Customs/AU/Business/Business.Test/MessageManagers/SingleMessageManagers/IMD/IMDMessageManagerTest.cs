using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDMessageManagerTest : TestCaseWithFactory
	{
		public void TestRequiresAmendment()
		{
			var decQuestion375 = entryHeader.Questions.AddNew();
			decQuestion375.ON_CPDecNum = 375;
			decQuestion375.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			Factory.Save();

			var manager = new IMDMessageManagerForAmendmentTest(entryHeader, multiManager);
			Assert("pre-conhdition, amendment not required", !manager.RequiresAmendment());
			decQuestion375.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			Assert("amendment now required", manager.RequiresAmendment());
		}

		public void TestWarningForAutomaticRefundReasonCode()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var sender = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = sender;
			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			invoice1.JobComInvoiceLines.AddNew();
			invoice2.JobComInvoiceLines.AddNew();
			testDec.DoMerge();
			Factory.Save();
			var entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_HighestLineNumber = 2;
			entryHeader.EntryNumber = "AAA";
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.DoMerge();
			multiManager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Amendment);
			var manager = new IMDMessageManager(entryHeader, multiManager);
			const string RefundReason126AForADeletedLine = @"The system has set a refund reason code, '126A' (REMISSION OF DUTY IF AN IMPORT ENTRY IS TAKEN TO BE WITHDRAWN) 
for entry lines to be deleted.";
			AssertEquals("They should be warned of the automatic refund reason", true, manager.GetNotificationsForSendingAReplacement().ContainsWarning(RefundReason126AForADeletedLine));
		}

		public void TestGetMessageTextExceptForThisSegment()
		{
			const string message = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/MEL3:3+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+DEHAM::6'LOC+79+AUMEL::6'";
			const string messageWithoutLOC9 = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/MEL3:3+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+79+AUMEL::6'";

			AssertMultilineASCIIEquals("", messageWithoutLOC9, manager.GetMessageTextExceptForThisSegment(message, "LOC+9+"));
		}

		public void TestDoNotSendDeveloperExceptionWhenTheMessageIsSameExceptForDeliveryAddress()
		{
			const string message1 = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/MEL3:3+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+DEHAM::6'LOC+79+AUMEL::6'DTM+178:20060430:102'DTM+260:20060330:102'DTM+252:20060427:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:2568.00000'FTX+DEL+++DEGUSSA AUSTRALIA PTY LIMITED'RFF+ABQ:6927'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:CIF'TDT+20+6013+S+++++9192430::11'NAD+AT+80005415752::95'NAD+VT+AA67ET::95'NAD+DP++YENNORA++26 LOFTUS RD++:::NSW+2161+AU'MOA+63:20217.58:EUR'MOA+141:21768.00:EUR'MOA+39:21768.00:EUR'MOA+68:2637.67:AUD'MOA+313:1496.00:EUR'MOA+71:54.42:EUR'UNS+D'DMS+1'LIN+1+I'PAC+++FCL:67:95'PAC+240+1'PCI+1'RFF+AAQ:ECMU1321050'PCI+1'RFF+MB:DE1260182'CST+1+I::95+N10::95'FTX+AAA+++AEROSIL R 202'LOC+27+DE::5'MEA+AAA++KG:2400.00000'NAD+SU+CCF4497966F::95'MOA+38:21768.00:EUR'MOA+68:2637.67:AUD'RFF+ABD:38249090'RFF+AED:58'RFF+AFD:505'RFF+AES:9603603::TC'RFF+AFV:TV'UNS+S'UNT+51+<<MSGNO PLACEHOLDER>>'";
			const string message2 = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/MEL3:3+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+DEHAM::6'LOC+79+AUMEL::6'DTM+178:20060430:102'DTM+260:20060330:102'DTM+252:20060427:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:2568.00000'FTX+DEL+++DEGUSSA AUSTRALIA PTY LIMITED'RFF+ABQ:6927'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:CIF'TDT+20+6013+S+++++9192430::11'NAD+AT+80005415752::95'NAD+VT+AA67ET::95'NAD+DP++MOOREBANK++10 CHURCH ROAD::ATT?: JIM OTTAWAY++:::NSW+2170+AU'MOA+63:20217.58:EUR'MOA+141:21768.00:EUR'MOA+39:21768.00:EUR'MOA+68:2637.67:AUD'MOA+313:1496.00:EUR'MOA+71:54.42:EUR'UNS+D'DMS+1'LIN+1+I'PAC+++FCL:67:95'PAC+240+1'PCI+1'RFF+AAQ:ECMU1321050'PCI+1'RFF+MB:DE1260182'CST+1+I::95+N10::95'FTX+AAA+++AEROSIL R 202'LOC+27+DE::5'MEA+AAA++KG:2400.00000'NAD+SU+CCF4497966F::95'MOA+38:21768.00:EUR'MOA+68:2637.67:AUD'RFF+ABD:38249090'RFF+AED:58'RFF+AFD:505'RFF+AES:9603603::TC'RFF+AFV:TV'UNS+S'UNT+51+<<MSGNO PLACEHOLDER>>'";

			const string message1WithoutDeliveryAddress = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/MEL3:3+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+DEHAM::6'LOC+79+AUMEL::6'DTM+178:20060430:102'DTM+260:20060330:102'DTM+252:20060427:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:2568.00000'FTX+DEL+++DEGUSSA AUSTRALIA PTY LIMITED'RFF+ABQ:6927'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:CIF'TDT+20+6013+S+++++9192430::11'NAD+AT+80005415752::95'NAD+VT+AA67ET::95'MOA+63:20217.58:EUR'MOA+141:21768.00:EUR'MOA+39:21768.00:EUR'MOA+68:2637.67:AUD'MOA+313:1496.00:EUR'MOA+71:54.42:EUR'UNS+D'DMS+1'LIN+1+I'PAC+++FCL:67:95'PAC+240+1'PCI+1'RFF+AAQ:ECMU1321050'PCI+1'RFF+MB:DE1260182'CST+1+I::95+N10::95'FTX+AAA+++AEROSIL R 202'LOC+27+DE::5'MEA+AAA++KG:2400.00000'NAD+SU+CCF4497966F::95'MOA+38:21768.00:EUR'MOA+68:2637.67:AUD'RFF+ABD:38249090'RFF+AED:58'RFF+AFD:505'RFF+AES:9603603::TC'RFF+AFV:TV'UNS+S'UNT+51+<<MSGNO PLACEHOLDER>>'";
			const string message2WithoutDeliveryAddress = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/MEL3:3+9'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+DEHAM::6'LOC+79+AUMEL::6'DTM+178:20060430:102'DTM+260:20060330:102'DTM+252:20060427:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:2568.00000'FTX+DEL+++DEGUSSA AUSTRALIA PTY LIMITED'RFF+ABQ:6927'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:CIF'TDT+20+6013+S+++++9192430::11'NAD+AT+80005415752::95'NAD+VT+AA67ET::95'MOA+63:20217.58:EUR'MOA+141:21768.00:EUR'MOA+39:21768.00:EUR'MOA+68:2637.67:AUD'MOA+313:1496.00:EUR'MOA+71:54.42:EUR'UNS+D'DMS+1'LIN+1+I'PAC+++FCL:67:95'PAC+240+1'PCI+1'RFF+AAQ:ECMU1321050'PCI+1'RFF+MB:DE1260182'CST+1+I::95+N10::95'FTX+AAA+++AEROSIL R 202'LOC+27+DE::5'MEA+AAA++KG:2400.00000'NAD+SU+CCF4497966F::95'MOA+38:21768.00:EUR'MOA+68:2637.67:AUD'RFF+ABD:38249090'RFF+AED:58'RFF+AFD:505'RFF+AES:9603603::TC'RFF+AFV:TV'UNS+S'UNT+51+<<MSGNO PLACEHOLDER>>'";

			var testManager = new IMDMessageManagerForTest(entryHeader, multiManager);
			AssertEquals("PreCondition:GetMessageTextExceptForThisSegment", message1WithoutDeliveryAddress, testManager.GetMessageTextExceptForThisSegment(message1, "NAD+DP+"));
			AssertEquals("PreCondition:GetMessageTextExceptForThisSegment", message2WithoutDeliveryAddress, testManager.GetMessageTextExceptForThisSegment(message2, "NAD+DP+"));

			Assert(message1 != message2);
			Assert(message1WithoutDeliveryAddress == message2WithoutDeliveryAddress);

			AssertEquals("ShouldSendDeveloperException", false, testManager.ShouldSendDeveloperExceptionForFalsePositiveCore(message1, message2));
		}

		public void TestDoNotSendDeveloperExceptionWhenTheMessageIsSameExceptForDeliveryAddressAndUNTSegment()
		{
			const string message1 = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/PRD3:3+9'CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUMEL::6'LOC+9+USHNL::6'LOC+79+AUMEL::6'DTM+178:20060503:102'DTM+260:20060428:102'DTM+252:20060503:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:5.40000'FTX+DEL+++SARAH TRAINER-RUSSELL'RFF+ABQ:1ZX236896649732390'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:FOB'TDT+20++A++5X::3'NAD+IM+CCK6739969R::95'NAD+VT+AA47NT::95'MOA+63:1649.34:USD'MOA+141:1834.16:USD'MOA+39:1649.34:USD'MOA+68:245.00:AUD'MOA+313:245.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:40692990704'PCI+1'RFF+HWB:1ZX236896649732390'CST+1+I::95+N10::95'FTX+AAA+++KENDALL DRESS'LOC+27+ID::5'MEA+AAA++NO:22.00000'NAD+SU+CCK6739646L::95'MOA+38:1649.34:USD'MOA+68:245.00:AUD'RFF+ABD:62044300'RFF+AED:17'RFF+AFV:TV'UNS+S'UNT+46+<<MSGNO PLACEHOLDER>>'";
			const string message2 = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/PRD3:3+9'CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUMEL::6'LOC+9+USHNL::6'LOC+79+AUMEL::6'DTM+178:20060503:102'DTM+260:20060428:102'DTM+252:20060503:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:5.40000'FTX+DEL+++SARAH TRAINER-RUSSELL'RFF+ABQ:1ZX236896649732390'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:FOB'TDT+20++A++5X::3'NAD+IM+CCK6739969R::95'NAD+VT+AA47NT::95'NAD+DP++KINGSWOOD++41 HALSBURY AVENUE++:::SA+5062+AU'MOA+63:1649.34:USD'MOA+141:1834.16:USD'MOA+39:1649.34:USD'MOA+68:245.00:AUD'MOA+313:245.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:40692990704'PCI+1'RFF+HWB:1ZX236896649732390'CST+1+I::95+N10::95'FTX+AAA+++KENDALL DRESS'LOC+27+ID::5'MEA+AAA++NO:22.00000'NAD+SU+CCK6739646L::95'MOA+38:1649.34:USD'MOA+68:245.00:AUD'RFF+ABD:62044300'RFF+AED:17'RFF+AFV:TV'UNS+S'UNT+47+<<MSGNO PLACEHOLDER>>'";

			const string message1WithoutDeliveryAddressAndUNT = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/PRD3:3+9'CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUMEL::6'LOC+9+USHNL::6'LOC+79+AUMEL::6'DTM+178:20060503:102'DTM+260:20060428:102'DTM+252:20060503:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:5.40000'FTX+DEL+++SARAH TRAINER-RUSSELL'RFF+ABQ:1ZX236896649732390'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:FOB'TDT+20++A++5X::3'NAD+IM+CCK6739969R::95'NAD+VT+AA47NT::95'MOA+63:1649.34:USD'MOA+141:1834.16:USD'MOA+39:1649.34:USD'MOA+68:245.00:AUD'MOA+313:245.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:40692990704'PCI+1'RFF+HWB:1ZX236896649732390'CST+1+I::95+N10::95'FTX+AAA+++KENDALL DRESS'LOC+27+ID::5'MEA+AAA++NO:22.00000'NAD+SU+CCK6739646L::95'MOA+38:1649.34:USD'MOA+68:245.00:AUD'RFF+ABD:62044300'RFF+AED:17'RFF+AFV:TV'UNS+S'";
			const string message2WithoutDeliveryAddressAndUNT = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+929:::IMD+<<SENDERS REFERENCE PLACE HOLDER>>/PRD3:3+9'CST++N10::95'LOC+8+AUMEL::6'LOC+12+AUMEL::6'LOC+9+USHNL::6'LOC+79+AUMEL::6'DTM+178:20060503:102'DTM+260:20060428:102'DTM+252:20060503:102'GIS+Y:153:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:5.40000'FTX+DEL+++SARAH TRAINER-RUSSELL'RFF+ABQ:1ZX236896649732390'RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'RFF+APH:FOB'TDT+20++A++5X::3'NAD+IM+CCK6739969R::95'NAD+VT+AA47NT::95'MOA+63:1649.34:USD'MOA+141:1834.16:USD'MOA+39:1649.34:USD'MOA+68:245.00:AUD'MOA+313:245.00:AUD'UNS+D'DMS+1'LIN+1+I'PAC+1+1'PCI+1'RFF+MWB:40692990704'PCI+1'RFF+HWB:1ZX236896649732390'CST+1+I::95+N10::95'FTX+AAA+++KENDALL DRESS'LOC+27+ID::5'MEA+AAA++NO:22.00000'NAD+SU+CCK6739646L::95'MOA+38:1649.34:USD'MOA+68:245.00:AUD'RFF+ABD:62044300'RFF+AED:17'RFF+AFV:TV'UNS+S'";

			var testManager = new IMDMessageManagerForTest(entryHeader, multiManager);
			AssertEquals("PreCondition:GetMessageTextExceptForThisSegment", message1WithoutDeliveryAddressAndUNT, testManager.GetMessageTextExceptForSegments(message1));
			AssertEquals("PreCondition:GetMessageTextExceptForThisSegment", message2WithoutDeliveryAddressAndUNT, testManager.GetMessageTextExceptForSegments(message2));

			Assert(message1 != message2);
			Assert(message1WithoutDeliveryAddressAndUNT == message2WithoutDeliveryAddressAndUNT);

			AssertEquals("ShouldSendDeveloperException", false, testManager.ShouldSendDeveloperExceptionForFalsePositiveCore(message1, message2));
		}

		public void TestMaximumNumberOfEntryLines()
		{
			var currentMax = AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.Value;
			AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 2);

			try
			{
				var testDec = Factory.New<JobDeclaration>();
				var entryHeader = testDec.CustomsEntryHeaders.AddNew();
				entryHeader.MergedLines.AddNew();
				entryHeader.MergedLines.AddNew();
				entryHeader.MergedLines.AddNew();

				multiManager = new IMDMultiMessageManager(testDec, CMRMessageTypes.LodgeWithoutPay);
				manager = new IMDMessageManagerForTest(entryHeader, multiManager);

				var errorMessage = new ZStringBuilder();
				errorMessage.Append("The total number of entry lines has exceeded the maximum number Customs accepts and this entry will fail. Currently the maximum number Customs accepts is ");
				errorMessage.Append(AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.Value.ToString());
				errorMessage.Append(". Please try and merge more invoice lines. Or you can create separate entries.");

				var result = manager.GetNotificationsForSendingAnOriginal();
				AssertEquals("Maximum number of entry lines error should be there", true, result.ContainsError(errorMessage.ToString()));
			}
			finally
			{
				AUCustomsDataRegistry.Instance.MaxNumberOfEntryLinesAcceptedAtCustoms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, currentMax);
			}
		}

		public void TestValidateApportionmentBalance()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");

				AssertEquals("Apportionment is not balanced", false, declaration.Invoices.AreChargesBalancedForInvoices(out _));

				multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
				manager = new IMDMessageManagerForTest(entryHeader, multiManager);
				var result = manager.GetNotificationsForSendingAnOriginal();

				var errorMessage = result.ErrorNotificationsAsString();
				Assert("Result should contain unbalanced apportionment", errorMessage.Contains("Current apportionment is not balanced"));

				invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
				AssertEquals("Apportionment is balanced", true, declaration.Invoices.AreChargesBalancedForInvoices(out _));
				result = manager.GetNotificationsForSendingAnOriginal();
				errorMessage = result.ErrorNotificationsAsString();
				Assert("Result should not contain unbalanced apportionment", !errorMessage.Contains("Current apportionment is not balanced,"));
			}
		}

		public void TestGenerateMessagesForAmendmentDetection()
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "SAC";
			AssertEquals("IsSAC", true, declaration.IsSAC);
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
			var testManager = new IMDMessageManagerForTest(entryHeader, multiManager);
			testManager.GenerateMessagesForAmendmentDetection(entryHeader);
			AssertEquals("Message type for test manager", CMRMessageTypes.OriginalForAmendmentDetection, testManager.ImportMessageBuilderForTesting.MessageType);
			AssertEquals("MessageSubType", Customs.Common.MessageBuilders.MessageSubTypes.Create, testManager.ImportMessageBuilderForTesting.MessageSubType);
			AssertEquals("Message builder type", typeof(SACMessageBuilder), testManager.ImportMessageBuilderForTesting.GetType());

			declaration.JE_MessageSubType = "FRM";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
			testManager = new IMDMessageManagerForTest(entryHeader, multiManager);
			testManager.GenerateMessagesForAmendmentDetection(entryHeader);
			AssertEquals("Message type for test manager", CMRMessageTypes.OriginalForAmendmentDetection, testManager.ImportMessageBuilderForTesting.MessageType);
			AssertEquals("MessageSubType", Customs.Common.MessageBuilders.MessageSubTypes.Create, testManager.ImportMessageBuilderForTesting.MessageSubType);
			AssertEquals("Message builder type", typeof(IMDMessageBuilder), testManager.ImportMessageBuilderForTesting.GetType());
		}

		public void TestWarnWhichAccountForAQISPayment()
		{
			entryHeader.EntryNumber = "AAA111BBB";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = 150m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);

			var importer = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = importer.PK;

			importer.MiscServ.OM_IMEftQuarantineFromImport = true;
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is not warned", false, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));

			importer.MiscServ.OM_IMEftQuarantineFromImport = false;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is warned", true, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));

			importer.MiscServ.OM_IMEftQuarantineFromImport = false;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is not warned as this is not Quarantine payment", false, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = 150m;
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 0m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is not warned as this includes Customs payment", true, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = 0m;
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 150m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is not warned as this includes Customs payment", false, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));
		}

		public void TestAQISPaymentWarningNotShownWhenPaymentPartyIsBroker()
		{
			entryHeader.EntryNumber = "AAA111BBB";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = 150m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);

			var importer = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = importer.PK;

			importer.MiscServ.OM_IMEftQuarantineFromImport = true;
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is not warned", false, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));

			importer.MiscServ.OM_IMEftQuarantineFromImport = false;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is warned", true, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Debiting from Broker's account is not warned when Payment Party is 'Broker'", false, result.ContainsWarning("This importer is not configured as Direct EFT payment for Quarantine amount as 'EFT Quarantine From Importer A/C' is not ticked on the Consignee tab (Electronic Fund Transfer) of Organisation form. This amount will be debited from Broker's account."));
		}

		public void TestWarningForDefaultPaymentOptionForCustomsCharge()
		{
			const string BrokerPaymentWhenImporterIsOnDirectDebit = "Importer is on direct debit according to its EFT configurations, but for this entry, the system recognises that BROKER should be the payee as either the total payable amount exceeds their maximum amount, or the importer's bank details are not complete. If you still want the importer to pay for this entry, please set the payment party on Misc. Options to importer.";
			var importer = OrgHeader.New(Factory);
			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			importer.MiscServ.OM_IMEFTBankAccount = "123456";
			importer.MiscServ.OM_IMEFTBankBSB = "123";
			importer.MiscServ.OM_IMMaxEFTAmount = 500m;

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			declaration.JE_OH_Importer = importer.PK;

			entryHeader.CH_TotalPaid = 1000m;

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);

			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Default setting for payment party should have been warned", true, result.ContainsWarning(BrokerPaymentWhenImporterIsOnDirectDebit));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Default setting for payment party should have been warned", false, result.ContainsWarning(BrokerPaymentWhenImporterIsOnDirectDebit));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			entryHeader.EntryNumber = "AAA111BBB";
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 0m;
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = 10m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Default setting not warned as this is Quarantine payment only", false, result.ContainsWarning(BrokerPaymentWhenImporterIsOnDirectDebit));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 1000m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();

			var paymentRetriver = new PaymentDetailRetriever(declaration);
			AssertEquals("Payment Party is broker", PaymentParty.Broker, paymentRetriver.PartyToPayEntry);
			AssertEquals("Default setting warned as this is Customs Charge payment only which is over the limit", true, result.ContainsWarning(BrokerPaymentWhenImporterIsOnDirectDebit));
		}

		public void TestGetNotificationsForSendingAReplacementForRefundReasonCode()
		{
			const string RefundAdviceWarning = "There are entry lines that would require a Refund Reason as the current Duty and Tax is less than the Duty and Tax advised in Last Lodgement Response Message.";
			const string ClearRefundAdviceWarning = "There are no entry lines that would require a Refund Reason, but you have a Refund Reason entered on the header or one of the entry lines.";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.RefundReasonCode = "FA";
			AssertEquals("IsLessDutyAndTax", false, entryLine.IsLessDutyAndTax);
			AssertEquals("IsRefundLikely", false, entryHeader.MergedLines.IsRefundLikely);

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is not likely and Refund code should not be entered", true, result.ContainsWarning(ClearRefundAdviceWarning));

			entryLine.RefundReasonCode = "";
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is not likely and Refund code should not be entered", false, result.ContainsWarning(ClearRefundAdviceWarning));

			entryHeader.RefundReasonCode = "FA";
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is not likely and Refund code should not be entered", true, result.ContainsWarning(ClearRefundAdviceWarning));

			entryHeader.RefundReasonCode = "";
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is not likely and Refund code should not be entered", false, result.ContainsWarning(ClearRefundAdviceWarning));

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 100m);
			AssertEquals("Entry line's total Duty and Tax", 100m, entryLine.TotalDutyTaxAdvisedInLastClearanceMessage);
			AssertEquals("IsLessDutyAndTax", true, entryLine.IsLessDutyAndTax);
			AssertEquals("IsRefundLikely", true, entryHeader.MergedLines.IsRefundLikely);

			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is likely and Refund code is empty", true, result.ContainsWarning(RefundAdviceWarning));

			entryHeader.RefundReasonCode = "FA";
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is likely and Header Refund code is not empty, which is valid", false, result.ContainsWarning(RefundAdviceWarning));

			entryHeader.RefundReasonCode = "";
			entryLine.RefundReasonCode = "FA";
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("Refund is likely and Refund code is not empty, which is valid", false, result.ContainsWarning(RefundAdviceWarning));
		}

		public void TestCanSendOriginalForPayment()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SWL";

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			AssertEquals("Can send original", true, manager.CanSendOriginal);
		}

		public void TestCanSendAmendmentWtihCH_Status()
		{
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			entryHeader.CH_Status = ZString.Empty;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.FailPreLodge.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.FailWithdrawal.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);
		}

		public void TestCanSendAmendmentWtihCH_EntryStatus()
		{
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Rejected.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);
		}

		public void TestCanSendAmendment()
		{
			entryHeader.CH_Status = ZString.Empty;
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("CanSendAmendment", false, manager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("CanSendAmendment", true, manager.CanSendAmendment);
		}

		public void TestPaymentMessageDoesNotRequireBrokerLicence()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";

			GlbStaff.CurrentUser.Certificates.DeleteAll();

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Broker licence not required for Payment message", false, result.ContainsError(NoBrokerLicenceForUserError));

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Broker licence required for Lodge with pay", true, result.ContainsError(NoBrokerLicenceForUserError));
		}

		public void TestCannotSendReplacementWhenWaitingForResponse()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.PreLodge);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("PreCondition:IsWaiting for response", true, manager.IsWaitingForResponse);
			AssertEquals("Still waiting for responses", true, result.ContainsError(SingleMessageManager.PendingMessagesErrorMessage));

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("PreCondition:IsWaiting for response", false, manager.IsWaitingForResponse);
			AssertEquals("Still waiting for responses", false, result.ContainsError(SingleMessageManager.PendingMessagesErrorMessage));
		}

		public void TestCannotSendReplacementForSAC()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SAC";

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("PreCondition:IsSac", true, declaration.IsSAC);
			AssertEquals("Cannot Send Amendment", true, result.ContainsError(CannotSendAmendmentForSAC));

			declaration.JE_MessageSubType = "FRM";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("PreCondition:IsSac", false, declaration.IsSAC);
			AssertEquals("Cannot Send Amendment", false, result.ContainsError(CannotSendAmendmentForSAC));
		}

		public void TestCannotSendReplacementForSACWithLines()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SWL";

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("PreCondition:IsSac", true, declaration.IsSAC);
			AssertEquals("Cannot Send Amendment", true, result.ContainsError(CannotSendAmendmentForSAC));

			declaration.JE_MessageSubType = "FRM";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			result = manager.GetNotificationsForSendingAReplacement();
			AssertEquals("PreCondition:IsSac", false, declaration.IsSAC);
			AssertEquals("Cannot Send Amendment", false, result.ContainsError(CannotSendAmendmentForSAC));
		}

		public void TestStopPaymentIfCashPayment()
		{
			const string CannotSendPaymentIfCashPayment = "You have indicated that this entry will be paid via Cash at a Customs Counter and therefore you cannot approve Payment or send a Payment Message.";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Cash;

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Cant lodge Payment with Cash payment", true, result.ContainsError(CannotSendPaymentIfCashPayment));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Can send Payment", false, result.ContainsError(CannotSendPaymentIfCashPayment));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Cash;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Cant send Payment with Cash payment", true, result.ContainsError(CannotSendPaymentIfCashPayment));

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Can lodge Payment", false, result.ContainsError(CannotSendPaymentIfCashPayment));
		}

		public void TestStopPreLodgeForSACEntry()
		{
			const string CannotSendPreLodgementForSAC = "You cannot send a Pre-Lodgement for SAC entries";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SAC";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.PreLodge);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Cant send PreLodge With SAC", true, result.ContainsError(CannotSendPreLodgementForSAC));

			declaration.JE_MessageSubType = "FRM";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.PreLodge);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Can send PreLodge With FRML", false, result.ContainsError(CannotSendPreLodgementForSAC));
		}

		public void TestStopLodgeWithPaymentIfHeldUntilAuthorityAndCustomsImportEFTIsTrue()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.MiscServ.OM_IMEftHoldUntilPayAuthorised = true;
			declaration.Importer.MiscServ.OM_IMEftCustomsFromImport = true;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);

			AssertAuthorityAndEFTCustomsWithPaymentDEFMethod(declaration, multiManager);
		}

		public void TestStopPaymentIfHeldUntilAuthorityAndCustomsImportEFTIsTrue()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.MiscServ.OM_IMEftHoldUntilPayAuthorised = true;
			declaration.Importer.MiscServ.OM_IMEftCustomsFromImport = true;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			entryHeader.EntryNumber = "AAA111BBB";
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 100m;

			AssertAuthorityAndEFTCustomsWithPaymentDEFMethod(declaration, multiManager);
		}

		public void TestDoNotStopPaymentIfPaymentMethodIsImporter()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			declaration.Importer.MiscServ.OM_IMEftHoldUntilPayAuthorised = true;
			declaration.Importer.MiscServ.OM_IMEftCustomsFromImport = true;
			MessageSendingNotificationCollection result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No error expected because PaymentMethod is Importer", false, result.ContainsError(CannotSendPaymentIfNoAuthorityOrCustomsImportEFT));
		}

		public void TestDoNotStopPaymentIfOnlyPayingAQIS()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.MiscServ.OM_IMEftHoldUntilPayAuthorised = true;
			declaration.Importer.MiscServ.OM_IMEftCustomsFromImport = true;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			entryHeader.EntryNumber = "AAA111BBB";
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 0m;
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = 10m;

			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No error expected because Quarantine is being paid", false, result.ContainsError(CannotSendPaymentIfNoAuthorityOrCustomsImportEFT));
		}

		public void TestSecurityRightChecksForLodgement()
		{
			const string NoSecurityRightForLodgement = "You do not have the security right to send a Lodgement Message. Please contact your system administrator.";
			Env.Security.CustomsDeclarationLodgement.IsAllowed = false;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodgement", true, result.ContainsError(NoSecurityRightForLodgement));

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodgement", true, result.ContainsError(NoSecurityRightForLodgement));

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodgement", true, result.ContainsError(NoSecurityRightForLodgement));

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Withdrawal);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodgement", true, result.ContainsError(NoSecurityRightForLodgement));

			Env.Security.CustomsDeclarationLodgement.IsAllowed = true;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodgement", false, result.ContainsError(NoSecurityRightForLodgement));
		}

		public void TestSecurityRightChecksForPayment()
		{
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			Env.Security.CustomsDeclarationPayment.IsAllowed = true;
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No EFT Ino", true, result.ContainsError("You cannot send this payment message because no EFT Payment Information has been entered."));

			RunSecurityRightChecksForPayment(true, true, 0m, 0m, false, false);
			RunSecurityRightChecksForPayment(true, true, 0m, 1m, false, false);
			RunSecurityRightChecksForPayment(true, true, 1m, 0m, false, false);
			RunSecurityRightChecksForPayment(true, true, 1m, 1m, false, false);

			RunSecurityRightChecksForPayment(true, false, 0m, 0m, false, false);
			RunSecurityRightChecksForPayment(true, false, 0m, 1m, false, true);
			RunSecurityRightChecksForPayment(true, false, 1m, 0m, false, false);
			RunSecurityRightChecksForPayment(true, false, 1m, 1m, false, true);

			RunSecurityRightChecksForPayment(false, true, 0m, 0m, false, false);
			RunSecurityRightChecksForPayment(false, true, 0m, 1m, false, false);
			RunSecurityRightChecksForPayment(false, true, 1m, 0m, true, false);
			RunSecurityRightChecksForPayment(false, true, 1m, 1m, true, false);

			RunSecurityRightChecksForPayment(false, false, 0m, 0m, false, false);
			RunSecurityRightChecksForPayment(false, false, 0m, 1m, false, true);
			RunSecurityRightChecksForPayment(false, false, 1m, 0m, true, false);
			RunSecurityRightChecksForPayment(false, false, 1m, 1m, true, true);
		}

		public void TestSecurityRightChecksForLodgeAndPayment()
		{
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			Env.Security.CustomsDeclarationPaymentCustoms.IsAllowed = false;
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodge and pay", true, result.ContainsError(NoSecurityRightForCustomsPayment));

			Env.Security.CustomsDeclarationPaymentCustoms.IsAllowed = true;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for Lodge and pay", false, result.ContainsError(NoSecurityRightForCustomsPayment));
		}

		public void TestSecurityRightChecksForPreLodge()
		{
			const string NoSecurityRightForPreLodge = "You do not have the security right to send a Pre-Lodgement Message. Please contact your system administrator.";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.PreLodge);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			Env.Security.CustomsDeclarationPreLodge.IsAllowed = false;
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for PreLodge", true, result.ContainsError(NoSecurityRightForPreLodge));

			Env.Security.CustomsDeclarationPreLodge.IsAllowed = true;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Security Right for PreLodge", false, result.ContainsError(NoSecurityRightForPreLodge));
		}

		public void TestGetCommonNotificationForSAC()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "SAC";
			AssertEquals("IsSAC", true, declaration.IsSAC);

			GlbStaff.CurrentUser.Certificates.DeleteAll();

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("GetCommonNotification", false, result.ContainsError(NoBrokerLicenceForUserError));

			declaration.JE_MessageSubType = "FRM";
			AssertEquals("IsSAC", false, declaration.IsSAC);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("GetCommonNotification", true, result.ContainsError(NoBrokerLicenceForUserError));
		}

		public void TestGetBuilderWithIMDMessageTypeSelection()
		{
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			entryHeader.EntryNumber = "AAA111333";
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 100m;
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var builders = manager.GetBuilder(entryHeader);
			var paystdBuilder = (PAYSTDMessageBuilder)builders[0];
			AssertEquals("Message builder", typeof(PAYSTDMessageBuilder), paystdBuilder.GetType());

			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.PreLodge);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			builders = manager.GetBuilder(entryHeader);
			var imdBuilder = (IMDMessageBuilder)builders[0];
			AssertEquals("Message builder", typeof(IMDMessageBuilder), imdBuilder.GetType());

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			builders = manager.GetBuilder(entryHeader);
			var sacBuilder = (SACMessageBuilder)builders[0];
			AssertEquals("Message builder", typeof(SACMessageBuilder), sacBuilder.GetType());
		}

		public void TestBrokerLicenceForPreLodge()
		{
			GlbStaff.CurrentUser.Certificates.DeleteAll();
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.PreLodge);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No Pre-lodge licence", true, result.ContainsError(PrelodgeValidationForLicenceCode));

			Env.Registry.AUCustoms.PreLodgementLicenceCode = "12345";
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Pre-lodge licence exists", false, result.ContainsError(PrelodgeValidationForLicenceCode));

			Env.Registry.AUCustoms.PreLodgementLicenceCode = ZString.Empty;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No Pre-lodge licence", true, result.ContainsError(PrelodgeValidationForLicenceCode));

			var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
			certificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			certificate.XZ_RefNumber = "AAA";
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Broker licence exists", false, result.ContainsError(PrelodgeValidationForLicenceCode));
		}

		public void TestNoBrokerLicenceForUserError()
		{
			GlbStaff.CurrentUser.Certificates.DeleteAll();
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", true, result.ContainsError(NoBrokerLicenceForUserError));

			var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
			certificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			certificate.XZ_RefNumber = "AAA";
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", false, result.ContainsError(NoBrokerLicenceForUserError));
		}

		public void TestBrokerLicenceExpired()
		{
			const string BrokerLicenceExpired = "A message cannot be sent.  Your Broker Licence has expired.  Please renew your Broker Licence and update the expiry date in your Staff record.";
			GlbStaff.CurrentUser.Certificates.DeleteAll();
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("BrokerLicenceExpired", false, result.ContainsError(BrokerLicenceExpired));

			var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
			certificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			certificate.XZ_RefNumber = "AAA";
			certificate.XZ_ExpiryOrDueDate = ZDateTime.Today;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("BrokerLicenceExpired", false, result.ContainsError(BrokerLicenceExpired));
			certificate.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("BrokerLicenceExpired", true, result.ContainsError(BrokerLicenceExpired));
		}

		public void TestGettingNoBrokerLicenceForWhenCompanyIsImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			declaration.JE_OH_Importer = importer.PK;

			GlbStaff.CurrentUser.Certificates.DeleteAll();
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", false, result.ContainsError(NoBrokerLicenceForUserError));
		}

		public void TestGettingNoBrokerLicenceForWhenCompanyIsSoleTrader()
		{
			AUCustomsDataRegistry.Instance.SoleTrader.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			GlbStaff.CurrentUser.Certificates.DeleteAll();
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", false, result.ContainsError(NoBrokerLicenceForUserError));
		}

		public void TestGettingNoPreLodgeLicenceForWhenCompanyIsImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			declaration.JE_OH_Importer = importer.PK;

			GlbStaff.CurrentUser.Certificates.DeleteAll();
			Env.Registry.AUCustoms.PreLodgementLicenceCode = ZString.Empty;
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", false, result.ContainsError(PrelodgeValidationForLicenceCode));
		}

		public void TestLocalCustomsBranchIdentifier()
		{
			const string NoBranchIdError = "You cannot send a message because the Local Customs Branch Id has not been entered. Please enter this through the registry.";
			AssertEquals("Prerequisite: declaration.IsSAC = False", false, declaration.IsSAC);

			AUCustomsDataRegistry.Instance.LocalContactPhoneNumber.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, string.Empty);
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "";
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", true, result.ContainsError(NoBranchIdError));

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "TEST";
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoBrokerLicenceForUserError", false, result.ContainsError(NoBranchIdError));
		}

		public void TestLocalContactPhoneNumber() => CombineAssertions(() =>
		{
			const string message = "You cannot send a message because either the Local Customs Branch Id or the Local Contact Phone Number is needed. Please enter this through the registry.";

			declaration.JE_MessageType = Declaration.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("Prerequisite: declaration.IsSAC = True", true, declaration.IsSAC);

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "";
			AUCustomsDataRegistry.Instance.LocalContactPhoneNumber.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, string.Empty);
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Both LocalCustomsBranchIdentifier and LocalContactPhoneNumber are empty", true, result.ContainsError(message));

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "TEST";
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("LocalCustomsBranchIdentifier is not empty", false, result.ContainsError(message));

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "";
			AUCustomsDataRegistry.Instance.LocalContactPhoneNumber.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, "92818273");
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("LocalContactPhoneNumber is not empty", false, result.ContainsError(message));
		});

		public void TestNoInvoiceHeaderAndLine()
		{
			const string NoInvoiceHeaderAndLinesError = "Please ensure there is at least one invoice header and each invoice header has at least one invoice line.";
			var result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoInvoiceHeaderAndLinesError", true, result.ContainsError(NoInvoiceHeaderAndLinesError));

			declaration.JE_SettlementPeriodType = "SW";
			declaration.NilReturnInd = true;
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoInvoiceHeaderAndLinesError", false, result.ContainsError(NoInvoiceHeaderAndLinesError));

			declaration.JE_SettlementPeriodType = ZString.Empty;
			declaration.NilReturnInd = false;
			declaration.Invoices.AddNew();
			declaration.FilteredInvoiceLines.AddNew();
			result = manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("NoInvoiceHeaderAndLinesError", false, result.ContainsError(NoInvoiceHeaderAndLinesError));
		}

		public void TestIsWaitingForResponse()
		{
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingSAC.Code;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);
		}

		public void TestCanSendWithdrawal()
		{
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("CanSendWithdrawal", true, manager.CanSendWithdrawal);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("CanSendWithdrawal", true, manager.CanSendWithdrawal);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			AssertEquals("CanSendWithdrawal", true, manager.CanSendWithdrawal);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPayment.Code;
			AssertEquals("CanSendWithdrawal", true, manager.CanSendWithdrawal);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("CanSendWithdrawal", true, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals("CanSendWithdrawal", false, manager.CanSendWithdrawal);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("CanSendWithdrawal", false, manager.CanSendWithdrawal);

			entryHeader.CH_Status = CustomsEntryStatus.FailWithdrawal.Code;
			AssertEquals("CanSendWithdrawal", true, manager.CanSendWithdrawal);
		}

		public void TestCanSendOriginal()
		{
			entryHeader.CH_Status = "";
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);

			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);

			entryHeader.CH_Status = CustomsEntryStatus.FailPreLodge.Code;
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			AssertEquals("CanSendOriginal", false, manager.CanSendOriginal);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("CanSendOriginal", false, manager.IsWaitingForResponse);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("CanSendOriginal", false, manager.IsWaitingForResponse);
		}

		public void TestBusinessObject()
		{
			AssertEquals("BusinessObject", entryHeader, manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("Message Friendly Name", "IMD Message for " + entryHeader.CH_BGMReference, manager.MessageFriendlyName);
		}

		public void TestGetMessages()
		{
			AssertEquals("GetMessages", entryHeader.Messages, manager.GetMessages(entryHeader));
		}

		public void TestGetBuilder()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("PreCondition:IsSACWithoutLines", true, declaration.IsSACWithoutLines);
			var builders = manager.GetBuilder(entryHeader);
			var sacBuilder = (SACMessageBuilder)builders[0];
			AssertEquals("Message Builer for SAC", typeof(SACMessageBuilder), sacBuilder.GetType());

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("PreCondition:IsSACWithoutLines", true, declaration.IsSACWithLines);
			builders = manager.GetBuilder(entryHeader);
			sacBuilder = (SACMessageBuilder)builders[0];
			AssertEquals("Message Builer for SWL", typeof(SACMessageBuilder), sacBuilder.GetType());

			declaration.JE_MessageType = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			var multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.LodgeWithoutPay);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			builders = manager.GetBuilder(entryHeader);
			var imdBuilder = (IMDMessageBuilder)builders[0];
			AssertEquals("Message Builder for normal", typeof(IMDMessageBuilder), imdBuilder.GetType());

			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			builders = manager.GetBuilder(entryHeader);
			imdBuilder = (IMDMessageBuilder)builders[0];
			AssertEquals("Message Builder for normal", typeof(IMDMessageBuilder), imdBuilder.GetType());
		}

		public void TestGetStatus()
		{
			entryHeader.CH_Status = "YYY";
			AssertEquals("Status", entryHeader.CH_Status, manager.GetStatus());
		}

		public void TestGetStatusException()
		{
			entryHeader.CH_Status = "YYY";
			AssertEquals("Status", entryHeader.CH_Status, manager.GetStatus());

			entryHeader.Delete();
			AssertEquals("Status", ZString.Empty, manager.GetStatus());
		}

		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, manager.StatusCalculators.Length);
			AssertEquals("Status Calculator", entryHeader.Calculator, manager.StatusCalculators[0]);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		IMDMessageManager manager;
		IMDMultiMessageManager multiManager;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Amendment);
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "XXX";
			manager = new IMDMessageManager(entryHeader, multiManager);
		}

		void AssertAuthorityAndEFTCustomsWithPaymentDEFMethod(JobDeclaration declaration, IMDMultiMessageManager multiManager)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			entryHeader.CH_TotalPaid = 1000m;

			var testManager = new IMDMessageManagerForTest(entryHeader, multiManager);
			var result = testManager.GetNotificationsForSendingAnOriginal();
			AssertEquals("Should have shown error because it was held and EFt Customs from Import was true", true, result.ContainsError(CannotSendPaymentIfNoAuthorityOrCustomsImportEFT));

			declaration.Importer.MiscServ.OM_IMEftCustomsFromImport = false;
			result = testManager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No error expected because EFt Customs from Import was false", false, result.ContainsError(CannotSendPaymentIfNoAuthorityOrCustomsImportEFT));

			declaration.Importer.MiscServ.OM_IMEftHoldUntilPayAuthorised = false;
			declaration.Importer.MiscServ.OM_IMEftCustomsFromImport = true;
			result = testManager.GetNotificationsForSendingAnOriginal();
			AssertEquals("No error expected because HoldUntilPayAuthorised was false", false, result.ContainsError(CannotSendPaymentIfNoAuthorityOrCustomsImportEFT));
		}

		void RunSecurityRightChecksForPayment(bool customsAllowed, bool aQISAllowed, decimal customsAmount, decimal aQISAmount, bool expectedCustomsResult, bool expectedAQISResult)
		{
			entryHeader.EntryNumber = "AAA111BBB";
			multiManager = new IMDMultiMessageManager(declaration, CMRMessageTypes.Payment);
			manager = new IMDMessageManagerForTest(entryHeader, multiManager);
			multiManager.EFTPaymentInformations = new EFTPaymentInformationCollection(declaration);
			multiManager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = customsAmount;
			multiManager.EFTPaymentInformations[0].AQISServicePaymentAmountPayableNow = aQISAmount;
			Env.Security.CustomsDeclarationPaymentCustoms.IsAllowed = customsAllowed;
			Env.Security.CustomsDeclarationPaymentAQIS.IsAllowed = aQISAllowed;
			var result = manager.GetNotificationsForSendingAnOriginal();
			var messageText = "- Customs payment allowed: " + customsAllowed.ToString() + " Customs Amt: " + customsAmount.ToString() +
				" Quarantine payment allowed: " + aQISAllowed.ToString() + " Quarantine Amt: " + aQISAmount.ToString();
			AssertEquals("Customs" + messageText, expectedCustomsResult, result.ContainsError(NoSecurityRightForCustomsPayment));
			AssertEquals("Quarantine" + messageText, expectedAQISResult, result.ContainsError("You do not have the security right to send a Quarantine Payment Message. Please contact your system administrator."));
		}

		const string NoBrokerLicenceForUserError = "A message cannot be sent.  No Broker Licence Number has been entered in your Staff record.  Please update your Staff record with your Licence Number.";

		const string NoSecurityRightForCustomsPayment = "You do not have the security right to send a Customs Payment Message. Please contact your system administrator.";

		const string CannotSendAmendmentForSAC = "You cannot send an Amendment for SAC entries";

		const string CannotSendPaymentIfNoAuthorityOrCustomsImportEFT = "The client requires an EFT Authority to be sent, please do so.\r\nOnce the client has authorised this payment, please click a menu 'Add a log 'EFT Payment Authority' Given by Importer' before you pay or Lodge & Pay the entry.";

		const string PrelodgeValidationForLicenceCode = @"Customs Agent Nominee Licence number  missing on Pre-lodge: -
In order to pre-lodge entries you must supply a valid Nominee Licence Number. We have provided two work arounds to this restriction.
a) If you are a licenced Customs Agent you may place your Nominee Licence Number on your staff file and this will be placed in the message (This is the same place that we use for formal declarations).
b) You may put a valid Customs Agent Nominee Licence Number under Customs -> Australia -> Pre-Lodgement Licence Code in the registry. This will be used for all pre-lodgement entries done by non Customs Agents.
Please note that pre-lodge is not a formal entry, is not retained by the ACS and is not a formal declaration.";

		sealed class IMDMessageManagerForTest : IMDMessageManager
		{
			public IMDMessageManagerForTest(CusEntryHeader entryHeader, IMDMultiMessageManager multiManager) : base(entryHeader, multiManager)
			{
			}

			internal new bool ShouldSendDeveloperExceptionForFalsePositiveCore(ZString factoryMessages, ZString databaseMessages) => base.ShouldSendDeveloperExceptionForFalsePositiveCore(factoryMessages, databaseMessages);
			internal new EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo) => base.GenerateMessagesForAmendmentDetection(bizo);
		}

		internal class IMDMessageManagerForAmendmentTest : IMDMessageManager
		{
			public IMDMessageManagerForAmendmentTest(CusEntryHeader entryHeader, IMDMultiMessageManager multiManager)
				: base(entryHeader, multiManager)
			{
			}

			public override bool HasActiveMessages => true;
			protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo) => new EDIMessage[] { bizo.Factory.New<EDIMessage>() };
			protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo) => new EDIMessage[] { bizo.Factory.New<EDIMessage>() };
		}
	}
}
