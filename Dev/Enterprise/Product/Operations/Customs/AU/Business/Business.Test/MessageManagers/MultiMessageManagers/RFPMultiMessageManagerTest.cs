using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RFPMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestOnMessagesSent()
		{
			var dec = Factory.New<JobDeclaration>();
			var sender = dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = dec.Invoices.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;

			var inv1 = invoice.InvoiceLines.AddNew();
			var inv2 = invoice.InvoiceLines.AddNew();
			ResetMessageAndAssert(dec, invoice, EXDOCMessageTypeCodes.Codes.ORD, sender, 2);

			var inv3 = invoice.InvoiceLines.AddNew();
			ResetMessageAndAssert(dec, invoice, EXDOCMessageTypeCodes.Codes.LDG, sender, 3);

			var inv4 = invoice.InvoiceLines.AddNew();
			ResetMessageAndAssert(dec, invoice, EXDOCMessageTypeCodes.Codes.RPL, sender, 4);

			inv4.Delete();
			ResetMessageAndAssert(dec, invoice, EXDOCMessageTypeCodes.Codes.ORD, sender, 4);

			var inv5 = invoice.InvoiceLines.AddNew();
			var inv6 = invoice.InvoiceLines.AddNew();
			var inv7 = invoice.InvoiceLines.AddNew();
			var otherMessageTypes = new EXDOCMessageTypeCodes().GetAllCodes().Where(x => !(new string[] { EXDOCMessageTypeCodes.Codes.ORD, EXDOCMessageTypeCodes.Codes.LDG, EXDOCMessageTypeCodes.Codes.RPL }.Contains(x)));
			CombineAssertions(() =>
			{
				foreach (var msgType in otherMessageTypes)
				{
					ResetMessageAndAssert(dec, invoice, msgType, sender, 4);
				}
			});
		}

		[GuiTest]
		public void TestDeniedPartyForNEXDOCS()
		{
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				declaration.JE_ScreeningStatus = "UNK";
				AssertHasMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);

				nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Order);
				Factory.Save();
				AssertExceptionThrown(typeof(ApplicationException), "Error : Unable to submit message due to Denied Party Screening cancellation." + System.Environment.NewLine, () => nexdocMultiMessageManager.SendMessages(declaration.MessageInitiator));
			}
		}

		public void TestAllMessageManagers()
		{
			var allMessageManagers = exdocMultiMessageManager.GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 1, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(RFPMessageManager), allMessageManagers[0].GetType());
		}

		public void TestAllMessageManagersWithNull()
		{
			exdocMultiMessageManager = new RFPMultiMessageManagerForTest(null, EXDOCMessageTypeCodes.Codes.LDG);
			AssertEquals("AllMessageManagers.Length", 0, exdocMultiMessageManager.GetAllMessageManagers().Length);
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			AssertEquals("SendWheneverPossibleOnceMessagingActive", true, exdocMultiMessageManager.SendWheneverPossibleOnceMessagingActive);
		}

		public void TestCanSendOrderMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			Assert(!quarantineHeader.IsNEXDOCSActive);

			Assert("Can Send Order Message", exdocMultiMessageManager.CanSendOrderMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot Send Order Message", !exdocMultiMessageManager.CanSendOrderMessage);
		}

		public void TestCanSendOriginalMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			Assert(!quarantineHeader.IsNEXDOCSActive);

			Assert("Can Send Original Message", exdocMultiMessageManager.CanSendOriginalMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot Send Original Message", !exdocMultiMessageManager.CanSendOriginalMessage);
		}

		public void TestCanSendAmendmentMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			Assert(!quarantineHeader.IsNEXDOCSActive);

			Assert("Cannot Send Amendment Message", !exdocMultiMessageManager.CanSendAmendmentMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can Send Amendment Message", exdocMultiMessageManager.CanSendAmendmentMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot Send Amendment Message", !exdocMultiMessageManager.CanSendAmendmentMessage);
		}

		public void TestCanSendWithdrawlMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			Assert(!quarantineHeader.IsNEXDOCSActive);

			Assert("Cannot Send Withdrawal Message", !exdocMultiMessageManager.CanSendWithdrawlMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can Send Withdrawal Message", exdocMultiMessageManager.CanSendWithdrawlMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot Send Withdrawal Message", !exdocMultiMessageManager.CanSendWithdrawlMessage);
		}

		public void TestHasValidNEXDOCTokens()
		{
			Assert("Requires Group and User", !nexdocMultiMessageManager.HasValidNEXDOCTokens);
			Assert("Requires Group and User", !nexdocMultiMessageManager.CanSendNEXDOCMessage);

			AssertContains(GroupTokenRequiredMessage, nexdocMultiMessageManager.Notifications);
			AssertContains(UserTokenRequiredMessage, nexdocMultiMessageManager.Notifications);

			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Lodge);
			var staffWrapper = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser);
			using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "pwd" }))
			{
				AssertEquals(string.Empty, staffWrapper.NUTPassword.GP_PasswordStatus);

				Assert("No token when user password is empty", !nexdocMultiMessageManager.HasValidNEXDOCTokens);
				Assert("No token when user password is empty", !nexdocMultiMessageManager.CanSendNEXDOCMessage);

				AssertNotContains(GroupTokenRequiredMessage, nexdocMultiMessageManager.Notifications);
				AssertContains(UserTokenRequiredMessage, nexdocMultiMessageManager.Notifications);

				nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Lodge);
				staffWrapper.NUTPassword.CurrentDecryptedPassword = "Test";

				AssertEquals(Core.Constants.PasswordOK, staffWrapper.NUTPassword.GP_PasswordStatus);

				Assert("Both Tokens exist", nexdocMultiMessageManager.HasValidNEXDOCTokens);
				Assert("Both Tokens exist", nexdocMultiMessageManager.CanSendNEXDOCMessage);

				AssertNotContains(GroupTokenRequiredMessage, nexdocMultiMessageManager.Notifications);
				AssertNotContains(UserTokenRequiredMessage, nexdocMultiMessageManager.Notifications);
			}

			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Lodge);
			using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "" }))
			{
				AssertEquals(Core.Constants.PasswordOK, staffWrapper.NUTPassword.GP_PasswordStatus);

				Assert("No token when group password is empty", !nexdocMultiMessageManager.HasValidNEXDOCTokens);
				Assert("No token when group password is empty", !nexdocMultiMessageManager.CanSendNEXDOCMessage);

				AssertContains(GroupTokenRequiredMessage, nexdocMultiMessageManager.Notifications);
				AssertNotContains(UserTokenRequiredMessage, nexdocMultiMessageManager.Notifications);
			}
		}

		public void TestCanSendOrderMessage_NEXDOC()
		{
			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Order);
			AssertNEXDOCGroupTokenRequired("Order", (messageManager) => messageManager.CanSendOrderMessage);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot Send Order Message", !nexdocMultiMessageManager.CanSendOrderMessage);
		}

		public void TestCanSendOriginalMessage_NEXDOC()
		{
			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Lodge);
			AssertNEXDOCGroupTokenRequired("Original", (messageManager) => messageManager.CanSendOriginalMessage);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot Send Original Message", !nexdocMultiMessageManager.CanSendOriginalMessage);
		}

		public void TestCanSendAmendmentMessage_NEXDOC()
		{
			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Amend);
			Assert("Cannot Send Amendment Message", !nexdocMultiMessageManager.CanSendAmendmentMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;

			AssertNEXDOCGroupTokenRequired("Amendment", (messageManager) => messageManager.CanSendAmendmentMessage);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot Send Amendment Message", !nexdocMultiMessageManager.CanSendAmendmentMessage);
		}

		public void TestCanSendWithdrawlMessage_NEXDOC()
		{
			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Withdrawal);
			Assert("Cannot Send Withdrawal Message", !nexdocMultiMessageManager.CanSendWithdrawlMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;

			AssertNEXDOCGroupTokenRequired("Withdrawal", (messageManager) => messageManager.CanSendWithdrawlMessage);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot Send Withdrawal Message", !nexdocMultiMessageManager.CanSendWithdrawlMessage);
		}

		public void TestCanSendNormalMessage_NEXDOC()
		{
			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.REXForward);
			AssertNEXDOCGroupTokenRequired(NEXDOCMessageType.Codes.REXForward, (messageManager) => messageManager.CanSendOrderMessage);
		}

		public void TestCanSendNEXDOCCancellation()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.NEXDOCCancellationReason.Description, "Cancellation Reason");

			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.REXForward);
			AssertNEXDOCGroupTokenRequired(NEXDOCMessageType.Codes.REXForward, messageManager => messageManager.CanSendOrderMessage);
		}

		public void TestCanSendTransferMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "12";
			quarantineHeader.QH_TransfereeExporterNumber = "999";
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can Send Transfer Message", exdocMultiMessageManager.CanSendTransferMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot Send Transfer Message", !exdocMultiMessageManager.CanSendTransferMessage);
		}

		public void TestCanSendForwardMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.QH_ForwardeeEDIUserIdentifier = "1795T";
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can Send Forward Message", exdocMultiMessageManager.CanSendForwardMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot Send Forward Message", !exdocMultiMessageManager.CanSendForwardMessage);
		}

		public void TestCanSendTransferAcceptanceMessages()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			var exportNum = supplier.CustomsCodes.AddNew();
			exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
			exportNum.OK_RN_NKCodeCountry = "AU";
			exportNum.OK_OH = supplier.PK;
			exportNum.OK_CustomsRegNo = "12345";
			declaration.JE_OH_Supplier = supplier.PK;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can Send Transfer Message", exdocMultiMessageManager.CanSendTransferInMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Cannot Send Transfer Message", !exdocMultiMessageManager.CanSendTransferInMessage);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can Send Transfer Message", !exdocMultiMessageManager.CanSendTransferInMessage);
		}

		public void TestCanSendCopyMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can Send Transfer Message", exdocMultiMessageManager.CanSendCopyMessage);
			quarantineHeader.RequestForPermitStatus = ZString.Empty;
			Assert("Cannot Send Transfer Message", !exdocMultiMessageManager.CanSendCopyMessage);
		}

		public void TestInvoiceHeaderNotification()
		{
			exdocMultiMessageManager = new RFPMultiMessageManagerForTest(Factory.New<JobDeclaration>(), EXDOCMessageTypeCodes.Codes.ORD);
			Assert("Shouldn't be able to send message", !exdocMultiMessageManager.CanSendOrderMessage);
			AssertEquals("The error should say need invoice header", "Message cannot be sent until an invoice header is entered", exdocMultiMessageManager.Notifications);
		}

		public void TestCanSendCertificateRequestMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Can Send Certificate Request Message", exdocMultiMessageManager.CanSendCertificateRequestMessage);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot Send Certificate Request Message", !exdocMultiMessageManager.CanSendCertificateRequestMessage);
		}

		public void TestShouldSendMessagesInTestMode()
		{
			Env.Registry.AQISMessagingTestMode = false;
			Assert(!exdocMultiMessageManager.ShouldSendMessagesInTestMode);
			Env.Registry.AQISMessagingTestMode = true;
			Assert(exdocMultiMessageManager.ShouldSendMessagesInTestMode);
		}

		public void TestSendRexCancellation()
		{
			var note = declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.NEXDOCCancellationReason.Description;
			note.ST_NoteText = "Reason";

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var manager = new RFPMultiMessageManager(declaration, NEXDOCMessageType.Codes.Cancellation);
			var sender = declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			Assert("Should have sent message", manager.SendMessages(sender));
			Factory.Save();

			var messageQuery = new ZQuery();
			messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, quarantineHeader.PK);
			var messages = Factory.Load<EDIMessage>(messageQuery);
			AssertEquals("Should have created 1 message.", 1, messages.Length);
			var messageText = Regex.Replace(messages[0].EM_MessageText, @"\r\n?\s+", "");
			AssertContains("EventReference", "<EventReference>|MST=CANREX</EventReference>", messageText);
			AssertContains("Note Cancellation Reason", "<Note><Description>NEXDOC Cancellation Reason</Description>", messageText);
		}

		public void TestSendRequestReissueCertificate()
		{
			var reissueHeader = new CertificateReissueHeader(Factory, new CodeDescriptionPairList());
			var request1 = reissueHeader.CertificateReissueRequests.AddNew();
			request1.CertificateNumber = "AU1234567";
			request1.ReissueReason = "Is Replacement 1";
			var request2 = reissueHeader.CertificateReissueRequests.AddNew();
			request2.CertificateNumber = "AU9876543";
			request2.ReissueReason = "Is Replacement 2";

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var sender = declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var manager = new RFPMultiMessageManager(declaration, NEXDOCMessageType.Codes.ReissueCertificate);
			Assert("Should have sent the messages", manager.SendMessages(sender, reissueHeader));
			Factory.Save();

			quarantineHeader.Messages.Reload(true);
			AssertEquals("Should have created 2 messages.", 2, quarantineHeader.Messages.Count);

			var messages = quarantineHeader.Messages.Cast<EDIMessage>().ToArray();
			var message1 = messages[0];
			var message1XmlInline = Regex.Replace(message1.EM_MessageText, @"[\r\n]+\s+", string.Empty);
			AssertContains("<AddInfo><Key>ReissueCertificateName</Key><Value>AU1234567</Value></AddInfo>", message1XmlInline);
			AssertContains("<AddInfo><Key>ReissueCertificateReason</Key><Value>Is Replacement 1</Value></AddInfo>", message1XmlInline);
			AssertEquals("First message should be SNT", "SNT", message1.EM_Status);
			AssertEquals("First interchange should be HQU", "HQU", message1.Interchange.EI_Status);
			var message2 = messages[1];
			var message2XmlInline = Regex.Replace(message2.EM_MessageText, @"[\r\n]+\s+", string.Empty);
			AssertContains("<AddInfo><Key>ReissueCertificateName</Key><Value>AU9876543</Value></AddInfo>", message2XmlInline);
			AssertContains("<AddInfo><Key>ReissueCertificateReason</Key><Value>Is Replacement 2</Value></AddInfo>", message2XmlInline);
			AssertEquals("First message should be PND", "PND", message2.EM_Status);
			AssertEquals("First interchange should be PND", "PND", message2.Interchange.EI_Status);

			AssertEquals("Temporary fields are cleared", "", quarantineHeader.ReissueCertificateNameForMessaging);
			AssertEquals("Temporary fields are cleared", "", quarantineHeader.ReissueCertificateReasonForMessaging);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			exdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, EXDOCMessageTypeCodes.Codes.LDG);
			nexdocMultiMessageManager = new RFPMultiMessageManagerForTest(declaration, NEXDOCMessageType.Codes.Lodge);
		}

		JobDeclaration declaration;
		QuarantineExDocHeader quarantineHeader;
		RFPMultiMessageManagerForTest exdocMultiMessageManager;
		RFPMultiMessageManagerForTest nexdocMultiMessageManager;

		void ResetMessageAndAssert(JobDeclaration dec, JobComInvoiceHeader invoice, ZString messageType, Customs.Business.ISendsMessagesToCustoms sender, int valueToExpect)
		{
			dec.JE_MessageStatus = ZString.Empty;
			var rfpmmm = new RFPMultiMessageManager(dec, messageType);
			rfpmmm.SendMessages(sender);
			AssertEquals(new EXDOCMessageTypeCodes().GetDescriptionFromCode(messageType), valueToExpect, invoice.QuarantineExDocHeader.QH_QuarantineMessageMaxLine);
		}

		void AssertNEXDOCGroupTokenRequired(string messageType, Func<RFPMultiMessageManagerForTest, bool> canSendDelegate)
		{
			var messageTypeText = messageType + " NEXDOC Message";

			Assert("Cannot Send " + messageTypeText, !canSendDelegate(nexdocMultiMessageManager));
			AssertContains(GroupTokenRequiredMessage, nexdocMultiMessageManager.Notifications);

			using (AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "pwd" }))
			{
				Assert("Cannot Send " + messageTypeText, !canSendDelegate(nexdocMultiMessageManager));
				AssertContains(UserTokenRequiredMessage, nexdocMultiMessageManager.Notifications);

				var staffWrapper = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser);
				staffWrapper.NUTPassword.CurrentDecryptedPassword = "Test";
				Assert("Can Send " + messageTypeText, canSendDelegate(nexdocMultiMessageManager));
			}
		}

		const string GroupTokenRequiredMessage = "Please register your company for NEXDOC with DAWR, then enter your company's Group Token in the registry under Customs > Australia > NEXDOCS > NEXDOCS Group Token.";

		const string UserTokenRequiredMessage = "Your NEXDOCS User Token has not been entered or is invalid.  Please enter a valid User Token into your staff record.";

		sealed class RFPMultiMessageManagerForTest : RFPMultiMessageManager
		{
			public RFPMultiMessageManagerForTest(BusinessObject masterBusinessObject, string messageType)
				: base(masterBusinessObject, messageType)
			{
			}

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;
		}
	}
}
