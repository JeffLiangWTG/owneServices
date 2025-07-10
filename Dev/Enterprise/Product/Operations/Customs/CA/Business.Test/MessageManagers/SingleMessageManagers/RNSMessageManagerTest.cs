using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(RNSMessageManager))]
	sealed class RNSMessageManagerTest : CAMessageManagerTestCase
	{
		public override void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", RNSMessageTypes.Descriptions.StatusQuery + " for " + dataWrapper.TopLevelBusinessObject.HumanReadableName,
				messageManager.MessageFriendlyName);
			messageManager = new RNSMessageManagerTesting((IRNSRequest)dataWrapper, false);
			AssertEquals("MessageFriendlyName", RNSMessageTypes.Descriptions.ArrivalCertification + " for " + dataWrapper.TopLevelBusinessObject.HumanReadableName,
				messageManager.MessageFriendlyName);
		}

		public override void TestCanSendThisMessage()
		{
			Env.Security.CARNSEnqMsgSend.IsAllowed = false;
			Env.Security.CARNSArrivalMsgSend.IsAllowed = false;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "CABBB";
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
			var manager = new RNSMessageManagerTesting(RNSMessagingBOTest.GetRNSMessagingBO(shipment));
			ZString messageText;

			Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(out messageText));
			AssertEquals("MessageText", "Job not yet saved, Please save before sending.", messageText);

			Factory.Save();
			Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(out messageText));
			Assert(messageText, messageText.Contains("You do not have the appropriate security rights to run this function."));
			Assert(messageText, messageText.Contains("RNS Status Query Message Send"));

			Env.Security.CARNSEnqMsgSend.IsAllowed = true;
			Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(out messageText));
			AssertEquals(@"The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", messageText);

			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(out messageText));
			AssertContains("MessageText", "Cargo Control Number must be specified.", messageText);

			var entryNumber = shipment.Numbers.AddNew();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			Factory.Save();
			Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(out messageText));

			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			using (rnsRequestBO.SuspendSettingHasChanges())
			{
				rnsRequestBO.CargoControlNumber = entryNumber.CE_EntryNum;
			}

			manager = new RNSMessageManagerTesting(rnsRequestBO, false);
			Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(out messageText));
			Assert(messageText, messageText.Contains("You do not have the appropriate security rights to run this function."));
			Assert(messageText, messageText.Contains("RNS Arrival Message Send"));

			Env.Security.CARNSArrivalMsgSend.IsAllowed = true;
			Assert("CanSendThisMessage", manager.CanSendThisMessage_Exposed(out messageText));

			rnsRequestBO.DateOfArrival = ZDateTime.Empty;
			Assert("CanSendThisMessage", !manager.CanSendThisMessage_Exposed(out messageText));
			AssertContains("MessageText", "valid Arrival Date must be provided for Warehouse Arrival Certification Message.", messageText);
		}

		public void TestGetNotificationsForSendingAnOriginal()
		{
			Env.Security.CARNSEnqMsgSend.IsAllowed = false;
			Env.Security.CARNSArrivalMsgSend.IsAllowed = false;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "CABBB";
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, string.Empty);
			var manager = new RNSMessageManagerTesting(RNSMessagingBOTest.GetRNSMessagingBO(shipment));

			AssertContains("Warning Text", "As Job not yet saved, Please save before sending.", manager.GetNotificationsForSendingAnOriginal().WarningNotificationsAsString());

			manager = new RNSMessageManagerTesting(RNSMessagingBOTest.GetRNSMessagingBO(shipment), messageErrors: new string[] { "Test Message Error 1", "Test Message Error 2" });
			AssertContains("Warning Text", "Test Message Error 1", manager.GetNotificationsForSendingAnOriginal().WarningNotificationsAsString());
			AssertContains("Warning Text", "Test Message Error 2", manager.GetNotificationsForSendingAnOriginal().WarningNotificationsAsString());
		}

		public override void TestPopulateMessages()
		{
			var messages = ((RNSMessageManagerTesting)messageManager).PopulateMessage_Exposed(MessageSubTypes.Undefined);
			AssertEquals("PopulateMessage", 1, messages.Length);
			AssertEquals("1 message", 1, declaration.CustomsEntryHeaders[0].Messages.Count);
		}

		[TestDate(2015, 08, 19)]
		public void TestAdditionalWarningsMessage()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			CACustomsDataRegistry.Instance.SecurityKeyAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "1234");

			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			using (rnsRequestBO.SuspendSettingHasChanges())
			{
				rnsRequestBO.TransactionNumber = "081954321";
				rnsRequestBO.CargoControlNumber = "081912345";
			}
			rnsRequestBO.DateOfArrival = ZDateTime.Now;
			var timeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;

			var unlocoZ = Factory.New<RefUNLOCO>();
			unlocoZ.RL_Code = "!ZZ";
			unlocoZ.RL_R3 = timeZoneSet.PK;
			var effectiveDate = unlocoZ.LocationDateTime;

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			cusEntryNum.CE_EntryNum = rnsRequestBO.TransactionNumber;
			cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var newdeclaration = Factory.New<JobDeclaration>();
			newdeclaration.JE_EntryStatus = "CAN";
			newdeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = newdeclaration.Invoices.AddNew();
			cusEntryNum.CE_ParentTable = newdeclaration.TableName;

			Factory.Save();
			var addInfo = Factory.New<CargoControlNumber>();
			addInfo.B7_Type = CusAddInfoTypeAttribute.Codes.CACCN;
			addInfo.B7_AddInfoData = "CCNInfoNumber=" + rnsRequestBO.CargoControlNumber;
			addInfo.B7_ParentID = ZGuid.NewZGuid();
			addInfo.B7_ParentTableCode = newdeclaration.TablePrefix;

			CombineAssertions("No Job Declaration is loaded.", () =>
			{
				var manager = new RNSMessageManagerTesting(rnsRequestBO, false, true);
				var notifcation = manager.Notification as TestMessageInstructionUserNotification;
				AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);

				notifcation.Reset();
				manager.SendMessage(MessageSubTypes.Request, false);

				var loadeddecfromTn = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(rnsRequestBO.Factory, rnsRequestBO.TransactionNumber, JobMessageTypeList.Codes.Import);
				var loadeddecfromCcn = ImportLinkedObjectManager.GetCusJobDeclarationByCargoControlNumber(rnsRequestBO.Factory, rnsRequestBO.CargoControlNumber, JobMessageTypeList.Codes.Import);

				AssertNull("Can not load job declaration with TransactionNumber.", loadeddecfromTn);
				AssertNull("Can not load job declaration with CargoControlNumber.", loadeddecfromCcn);
				AssertEquals("ContainsAdditionalWarnings", false, notifcation.ContainsAdditionalWarnings);
				AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);

				var testHelper = new DeclarationTestHelper(Factory, true);
				testHelper.GetEDIReleaseResponseMessage(rnsRequestBO.CargoControlNumber, "2015081912345", "1", true);
				AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);
			});

			cusEntryNum.CE_ParentID = newdeclaration.PK;
			Factory.Save();

			CombineAssertions("Job Declaration is loaded by TransactionNumber.", () =>
			{
				var loadeddecfromCcn = ImportLinkedObjectManager.GetCusJobDeclarationByCargoControlNumber(rnsRequestBO.Factory, rnsRequestBO.CargoControlNumber, JobMessageTypeList.Codes.Import);
				AssertNull("Can not load job declaration by CargoControlNumber.", loadeddecfromCcn);
				var builder = new ZStringBuilder();
				builder.Append("An ACROSS release declaration appears to have not been lodged.");
				AssertAdditionalWarnings(builder.ToStringWithDelimiterBetweenAppends("\r\n"), rnsRequestBO, "2015081912346", "2");
			});

			cusEntryNum.Delete();
			newdeclaration = Factory.New<JobDeclaration>();
			newdeclaration.JE_EntryStatus = "ERR";
			newdeclaration.CargoControlNumbers.AddNew(rnsRequestBO.CargoControlNumber);
			newdeclaration.JE_RL_NKPortOfArrival = "!ZZ";
			Factory.Save();

			CombineAssertions("Job Declaration is loaded by CargoControlNumber.", () =>
			{
				var loadeddecfromTn = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(rnsRequestBO.Factory, rnsRequestBO.TransactionNumber, JobMessageTypeList.Codes.Import);
				AssertNull("Can not load job declaration by TransactionNumber.", loadeddecfromTn);
				var builder = new ZStringBuilder();
				builder.Append("An ACROSS release declaration appears to have not been lodged.");
				AssertAdditionalWarnings(builder.ToStringWithDelimiterBetweenAppends("\r\n"), rnsRequestBO, "2015081912347", "3");
			});

			CombineAssertions("Related ETA First Port of Arrival date is farther than 20 hours in the future for an AIR shipment being arrived.", () =>
			{
				newdeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;

				newdeclaration.JE_DateOfFirstArrival = effectiveDate.AddHours(21);

				var builder = new ZStringBuilder();
				builder.Append("An ACROSS release declaration appears to have not been lodged.");
				builder.Append("Related ETA First Port of Arrival date cannot be farther than 20 hours in the future for an AIR shipment being arrived.");
				AssertAdditionalWarnings(builder.ToStringWithDelimiterBetweenAppends("\r\n"), rnsRequestBO, "2015081912348", "4");
			});

			CombineAssertions("Related ETA First Port of Arrival date is in the future for a Marine shipment being arrived.", () =>
			{
				newdeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				newdeclaration.JE_DateOfFirstArrival = effectiveDate.AddDays(1);

				var builder = new ZStringBuilder();
				builder.Append("An ACROSS release declaration appears to have not been lodged.");
				builder.Append("Related ETA First Port of Arrival date cannot be in the future for a Marine shipment being arrived.");
				AssertAdditionalWarnings(builder.ToStringWithDelimiterBetweenAppends("\r\n"), rnsRequestBO, "2015081912349", "5");
			});

			CombineAssertions("Related ETA First Port of Arrival date is in the future for a ROAD shipment being arrived.", () =>
			{
				newdeclaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				newdeclaration.JE_DateOfFirstArrival = effectiveDate.AddDays(1);

				var builder = new ZStringBuilder();
				builder.Append("An ACROSS release declaration appears to have not been lodged.");
				builder.Append("Related ETA First Port of Arrival date cannot be in the future for a ROAD shipment being arrived.");
				AssertAdditionalWarnings(builder.ToStringWithDelimiterBetweenAppends("\r\n"), rnsRequestBO, "2015081912350", "6");
			});

			CombineAssertions("Related ETA First Port of Arrival time zone is empty.", () =>
			{
				newdeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				var refUNLOCO = Factory.New<RefUNLOCO>();
				refUNLOCO.RL_R3 = ZGuid.Empty;
				refUNLOCO.RL_Code = "CATE0";
				newdeclaration.JE_RL_NKPortOfArrival = refUNLOCO.RL_Code;
				effectiveDate = unlocoZ.LocationDateTime;
				newdeclaration.JE_DateOfFirstArrival = effectiveDate.AddDays(1);

				var builder = new ZStringBuilder();
				builder.Append("An ACROSS release declaration appears to have not been lodged.");
				builder.Append("Please check the Time Zone of Arrival Port or your Home Port, it should not be empty.");
				AssertAdditionalWarnings(builder.ToStringWithDelimiterBetweenAppends("\r\n"), rnsRequestBO, "2015081912351", "7");
			});
		}

		void AssertAdditionalWarnings(string message, RNSRequestBO rnsRequestBO, string interchangeNum, string messageNumber)
		{
			var manager = new RNSMessageManagerTesting(rnsRequestBO, false, true);
			var notfcation = manager.Notification as TestMessageInstructionUserNotification;
			if (notfcation != null)
			{
				notfcation.Reset();
				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("ContainsAdditionalWarnings", true, notfcation.ContainsAdditionalWarnings);
				AssertEquals("AdditionalWarnings", message, notfcation.AdditionalWarningsMessage);

				var testHelper = new DeclarationTestHelper(Factory, true);
				testHelper.GetEDIReleaseResponseMessage(rnsRequestBO.CargoControlNumber, interchangeNum, messageNumber, true);
				AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);
			}
		}

		public void TestAdditionalWarningsMessage_MQWarningMessage()
		{
			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			var mQwarningText = CAMessageManager.MQWarningMessage;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP");
				var rusManager = new RNSMessageManagerTesting(rnsRequestBO, false, true);
				AssertContains("AdditionalWarnings", mQwarningText, rusManager.GetAdditionalWarningsMessage(MessageSubTypes.Request));

				CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW");
				rusManager = new RNSMessageManagerTesting(rnsRequestBO, false, true);
				AssertNotContains("AdditionalWarnings", mQwarningText, rusManager.GetAdditionalWarningsMessage(MessageSubTypes.Request));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var rusManager = new RNSMessageManagerTesting(rnsRequestBO, false, true);
				rusManager = new RNSMessageManagerTesting(rnsRequestBO, false, true);
				AssertNotContains("AdditionalWarnings", mQwarningText, rusManager.GetAdditionalWarningsMessage(MessageSubTypes.Request));
			}
		}

		public override void TestIsWaitingForResponse()
		{
			const string awaitingReplyMessage = @"An RNS Request has already been sent and is awaiting a CBSA response (Transaction Number: 123454321, CCN: 123412345).
Sending another one now may cause you, or the other party who sent the original request, to  not receive a response.
Please be aware that acknowledgement responses are sent to all relevant parties, so you should not need to resend this request.
If you do not receive a response within a reasonable period of time, then try resending the request at that time.


Are you sure that you want to resend to the CBSA?";

			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			CACustomsDataRegistry.Instance.SecurityKeyAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "1234");

			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			using (rnsRequestBO.SuspendSettingHasChanges())
			{
				rnsRequestBO.TransactionNumber = "123454321";
				rnsRequestBO.CargoControlNumber = "123412345";
			}

			CombineAssertions("Traditional Notification Confirmation", () =>
			{
				var manager = new RNSMessageManagerTesting(rnsRequestBO);
				AssertEquals("1. IsWaitingForResponse", false, manager.IsWaitingForResponse);

				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("2. IsWaitingForResponse", true, manager.IsWaitingForResponse);

				manager.Notification.NextAnswer = false;
				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("3. IsWaitingForResponse", awaitingReplyMessage, manager.Notification.LastMessage);

				manager.Notification.NextAnswer = true;
				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("4. IsWaitingForResponse", true, manager.IsWaitingForResponse);

				var testHelper = new DeclarationTestHelper(Factory, true);
				testHelper.GetEDIReleaseResponseMessage(rnsRequestBO.CargoControlNumber, "20210624123457", ZDateTime.UtcNow, "2", true);
				AssertEquals("5. IsWaitingForResponse", false, manager.IsWaitingForResponse);
			});

			CombineAssertions("New Notification Confirmation Form", () =>
			{
				var manager = new RNSMessageManagerTesting(rnsRequestBO, true, true);
				var notification = manager.Notification as TestMessageInstructionUserNotification;
				AssertEquals("1. IsWaitingForResponse", false, manager.IsWaitingForResponse);

				notification.Reset();
				manager.Notification.NextAnswer = true;
				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("2. notificationForm Shown", true, notification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("2. notifcation.IsWaitingForResponse", false, notification.IsWaitingForResponse);
				AssertEquals("2. ContainsValidationErrors", false, notification.ContainsValidationErrors);
				AssertEquals("2. ContainsAdditionalWarnings", false, notification.ContainsAdditionalWarnings);
				AssertEquals("2. IsWaitingForResponse", true, manager.IsWaitingForResponse);

				notification.Reset();
				manager.Notification.NextAnswer = false;
				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("3. notificationForm Shown", true, notification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("3. notifcation.IsWaitingForResponse", true, notification.IsWaitingForResponse);
				AssertEquals("3. ContainsValidationErrors", false, notification.ContainsValidationErrors);
				AssertEquals("3. ContainsAdditionalWarnings", true, notification.ContainsAdditionalWarnings);
				AssertEquals("3. Additional Warning", awaitingReplyMessage, notification.AdditionalWarningsMessage);

				notification.Reset();
				manager.Notification.NextAnswer = true;
				manager.SendMessage(MessageSubTypes.Request, false);
				AssertEquals("4. notificationForm Shown", true, notification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("4. notifcation.IsWaitingForResponse", true, notification.IsWaitingForResponse);
				AssertEquals("4. ContainsValidationErrors", false, notification.ContainsValidationErrors);
				AssertEquals("4. ContainsAdditionalWarnings", true, notification.ContainsAdditionalWarnings);
				AssertEquals("4. Additional Warning", awaitingReplyMessage, notification.AdditionalWarningsMessage);
				AssertEquals("4. IsWaitingForResponse", true, manager.IsWaitingForResponse);

				notification.Reset();
				var testHelper = new DeclarationTestHelper(Factory, true);
				testHelper.GetEDIReleaseResponseMessage(rnsRequestBO.CargoControlNumber, "20210624123557", ZDateTime.UtcNow, "3", true);
				AssertEquals("5. IsWaitingForResponse", false, manager.IsWaitingForResponse);
			});
		}

		public override void TestCanSendWithdrawal()
		{
			AssertEquals("CanSendWithdrawal", false, messageManager.CanSendWithdrawal);
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder",
				typeof(RNSRequestMessageBuilder),
				((RNSMessageManagerTesting)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public override void TestBusinessObject()
		{
			AssertEquals("BusinessObject", declaration, messageManager.BusinessObject);
		}

		public void TestSetShouldWaitUntilResponded()
		{
			var rnsMessageManager = (RNSMessageManagerTesting)GetMessageManager();
			Assert("ShouldWaitUntilResponded", rnsMessageManager.ShouldWaitUntilResponded_Exposed);

			rnsMessageManager.SetShouldWaitUntilResponded(false);
			Assert("ShouldWaitUntilResponded", !rnsMessageManager.ShouldWaitUntilResponded_Exposed);
		}

		public void TestIncludeCargoControlNumberInEDIMessage()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			Env.Security.CARNSEnqMsgSend.IsAllowed = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "CABBB";
			var cargoControlNumber = shipment.Numbers.AddNew();
			cargoControlNumber.CE_EntryNum = "IAN TEST CCN ";
			cargoControlNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var manager = new RNSMessageManagerTesting(RNSMessagingBOTest.GetRNSMessagingBO(shipment));
			Factory.Save();

			var messages = manager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals(1, messages.Length);
			AssertEquals("IANTESTCCN", messages[0].EM_ApplicationReference);
		}

		public void TestIncludeTransactionNumberInEDIMessage()
		{
			var rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			using (rnsRequestBO.SuspendSettingHasChanges())
			{
				rnsRequestBO.TransactionNumber = "54321";
			}

			var manager = new RNSMessageManagerTesting(rnsRequestBO);
			var messages = manager.PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals(1, messages.Length);
			AssertEquals("54321", messages[0].EM_MessageOwner);
		}

		public void TestShouldJobBeSavedBeforeSendingMessage()
		{
			var rnsMessageManager = (RNSMessageManagerTesting)GetMessageManager();
			Assert("ShouldJobBeSavedBeforeSendingMessage", rnsMessageManager.ShouldJobBeSavedBeforeSendingMessage_Exposed);

			rnsMessageManager.SetShouldJobBeSavedBeforeSendingMessage(false);
			Assert("ShouldJobBeSavedBeforeSendingMessage", !rnsMessageManager.ShouldJobBeSavedBeforeSendingMessage_Exposed);
		}

		public void TestSendWithMessageErrorsSecurityCheckpoint()
		{
			var rnsMessageManager = (RNSMessageManagerTesting)GetMessageManager();
			Env.Security.CARNSSendWithMessageErrors.IsAllowed = false;
			AssertEquals(false, rnsMessageManager.SendWithMessageErrorsSecurityCheckpoint_Exposed().IsAllowed);

			Env.Security.CARNSSendWithMessageErrors.IsAllowed = true;
			AssertEquals(true, rnsMessageManager.SendWithMessageErrorsSecurityCheckpoint_Exposed().IsAllowed);
		}

		#region Implementation

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			return new StatusQueryMessageWrapper(entryHeader);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new RNSMessageManagerTesting((IRNSRequest)dataWrapper);
		}

		JobDeclaration declaration;

		#region RNSMessageManagerTesting

		class RNSMessageManagerTesting : RNSMessageManager
		{
			public RNSMessageManagerTesting(IRNSRequest dataWrapper, bool isStatusQuery = true, bool useNewNotificationForm = false, IEnumerable<string> messageErrors = null)
				: base(dataWrapper, useNewNotificationForm ? new TestMessageInstructionUserNotification() : new TestUserNotification(), isStatusQuery, messageErrors)
			{
			}

			public bool CanSendThisMessage_Exposed(out ZString messageText)
			{
				return CanSendThisMessage(MessageSubTypes.Undefined, out messageText);
			}

			public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
			{
				return GetMessageBuilder(actionCode);
			}

			public new ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
			{
				return base.GetAdditionalWarningsMessage(actionCode);
			}

			public Enterprise.Messaging.Business.EDIMessage[] PopulateMessage_Exposed(MessageSubTypes actionCode)
			{
				return PopulateMessage(actionCode);
			}

			public bool ShouldWaitUntilResponded_Exposed
			{
				get
				{
					return base.ShouldWaitUntilResponded;
				}
			}

			public bool ShouldJobBeSavedBeforeSendingMessage_Exposed
			{
				get { return base.ShouldJobBeSavedBeforeSendingMessage; }
			}

			public TestUserNotification Notification
			{
				get { return ((TestUserNotification)notification); }
			}

			public SecurityCheckpoint SendWithMessageErrorsSecurityCheckpoint_Exposed()
			{
				return base.SendWithMessageErrorsSecurityCheckpoint;
			}
		}

		#endregion

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			Assert(true);
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}

		#endregion
	}
}
