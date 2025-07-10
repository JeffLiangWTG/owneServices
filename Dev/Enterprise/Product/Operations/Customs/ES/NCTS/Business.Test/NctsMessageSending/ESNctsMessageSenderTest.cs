using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.ES.NCTS.Business.ESNctsMessageSender;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ESNctsMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ESNctsMessageSender(null));
		}

		readonly ZString nonExistentMessageType = "AAA";

		public void TestGetMessageBuildersData()
		{
			CombineAssertions(() =>
			{
				var messageSendingObject = new NctsMessageSendingObject(nctsHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = nonExistentMessageType;
				var sender = new ESNctsMessageSender(messageSendingObjectParent);
				var messageBuildersData = sender.GetMessageBuildersData();
				AssertEquals("The returned messagebuildersdata list has 1 element", 1, messageBuildersData.Count);
				AssertEquals("MessageBuilders failure is true when message type is not correct", true, messageBuildersData.FirstOrDefault().FailureFlag);
				AssertEquals("LastKeyReported has exception in the builder", "ESNctsMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				messageSendingObject = new NctsMessageSendingObject(nctsHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(messageSendingObject);
				sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = DeclarationMessageTypeList.Codes.NctsDeparture;
				sender = new ESNctsMessageSender(messageSendingObjectParent);
				messageBuildersData = sender.GetMessageBuildersData();
				AssertEquals("MessageBuilders count is 1", 1, messageBuildersData.Count);
				AssertEquals("MessaegBuilder Type", "DEP", messageBuildersData.FirstOrDefault().MessageBuilder.MessageType);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendDepartureMessage_Phase4()
		{
			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.NctsDeparture);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender, messageStatus: NctsMessageStatusList.Codes.DepartureDeclarationSent);
			});
		}

		public void TestSendDepartureMessage_Phase4_NewGuaranteeTransactionWhenAmountGreaterthan0()
		{
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.NctsDeparture);
				SendMessageAndAssertCorrectResult(sender, messageStatus: NctsMessageStatusList.Codes.DepartureDeclarationSent);

				var transactionsGuarantee1 = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee1 after sending", 2, transactionsGuarantee1.Count);
				AssertGuaranteeTransaction("Guarantee1", transactionsGuarantee1[1], guarantee1.PW_BondAmount);

				var transactionsGuarantee2 = guarantee2.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee2 after sending", 2, transactionsGuarantee2.Count);
				AssertGuaranteeTransaction("Guarantee2", transactionsGuarantee2[1], guarantee2.PW_BondAmount);
			});
		}

		public void TestSendDepartureMessage_Phase4_NoNewGuaranteeTransactionWhenAmountLesserthan0()
		{
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee1.PW_BondAmount = -10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondAmount = 0m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.NctsDeparture);
				SendMessageAndAssertCorrectResult(sender, messageStatus: NctsMessageStatusList.Codes.DepartureDeclarationSent);

				AssertEquals("Transactions for guarantee1 after sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 after sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);
			});
		}

		public void TestSendArrivalMessage_Phase4()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.NctsArrivalNotification);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender, messageStatus: NctsMessageStatusList.Codes.ArrivalNotificationSent);
			});
		}

		public void TestSendOBSMessage()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);
			var messageBuilders = sender.GetMessageBuildersData();
			var result = ESNctsMessageSender.Send(messageBuilders);
			CombineAssertions(() =>
			{
				AssertEquals("NctsHeader message status has changed, EffectiveMessageStatus", NctsMessageStatusList.Codes.UnloadingRemarksSent, nctsHeader.EffectiveMessageStatus);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);
			});
		}

		public void TestSendDepartureMessage_Phase5()
		{
			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5Departure);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
			});
		}

		public void TestSendDepartureMessage_DPT_Phase5_NewGuaranteeTransactionWhenAmountGreaterThan0()
		{
			SetUpGuaranteeTransactions();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5Departure);
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				var transactionsGuarantee1 = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee1 after sending", 2, transactionsGuarantee1.Count);
				AssertGuaranteeTransaction("Guarantee1", transactionsGuarantee1[1], guarantee1.PW_BondAmount);

				var transactionsGuarantee2 = guarantee2.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee2 after sending", 2, transactionsGuarantee2.Count);
				AssertGuaranteeTransaction("Guarantee2", transactionsGuarantee2[1], guarantee2.PW_BondAmount);
			});
		}

		public void TestSendDepartureMessage_DPT_Phase5_NoNewGuaranteeTransactionWhenAmountLesserThan0()
		{
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = -10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 0m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5Departure);
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				var transactionsGuarantee1 = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee1 after sending", 1, transactionsGuarantee1.Count);

				var transactionsGuarantee2 = guarantee2.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee2 after sending", 1, transactionsGuarantee2.Count);
			});
		}

		public void TestSendDepartureMessage_DPN_Phase5_NewGuaranteeTransactionWhenAmountGreaterThan0()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification);
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Presentation);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				var transactionsGuarantee1 = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee1 after sending", 2, transactionsGuarantee1.Count);
				AssertGuaranteeTransaction("Guarantee1", transactionsGuarantee1[1], guarantee1.PW_BondAmount);

				var transactionsGuarantee2 = guarantee2.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee2 after sending", 2, transactionsGuarantee2.Count);
				AssertGuaranteeTransaction("Guarantee2", transactionsGuarantee2[1], guarantee2.PW_BondAmount);
			});
		}

		public void TestSendDepartureMessage_DPN_Phase5_NoNewGuaranteeTransactionWhenAmountLesserThan0()
		{
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = -10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 0m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification);
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Presentation);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				var transactionsGuarantee1 = guarantee1.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee1 after sending", 1, transactionsGuarantee1.Count);

				var transactionsGuarantee2 = guarantee2.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions for guarantee2 after sending", 1, transactionsGuarantee2.Count);
			});
		}

		public void TestSendDepartureMessage_DPC_Phase5_NoNewGuaranteeTransactionWhenAmountGreaterThan0()
		{
			SetUpGuaranteeTransactions();
			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";
			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, reasonForCancellation: "reason");
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Cancellation);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);
				AssertEquals("Transactions for guarantee1 after sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 after sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);
			});
		}

		public void TestSendDepartureMessage_DPD_Phase5_NoNewGuaranteeTransactionWhenAmountGreaterThan0()
		{
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				AssertEquals("Transactions for guarantee1 after sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 after sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);
			});
		}

		[TestDate(2024, 11, 19)]
		public void TestSendDepartureMessage_ValuationDate()
		{
			var testCases = new[]
			{
				(phase: CusInBondApplicationCodeList.Codes.NCTS4, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.NctsDeparture, expectedValuationDate: ZDateTime.Empty),
				(phase: CusInBondApplicationCodeList.Codes.NCTS5, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, expectedValuationDate: ZDateTime.Now),
				(phase: CusInBondApplicationCodeList.Codes.NCTS5, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.Ncts5Departure, expectedValuationDate: ZDateTime.Now),
				(phase: CusInBondApplicationCodeList.Codes.NCTS5, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, expectedValuationDate: ZDateTime.Now),
				(phase: CusInBondApplicationCodeList.Codes.NCTS5, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, expectedValuationDate: ZDateTime.Empty),
				(phase: CusInBondApplicationCodeList.Codes.NCTS5, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, expectedValuationDate: ZDateTime.Empty),
				(phase: CusInBondApplicationCodeList.Codes.NCTS5, MessageTypeAndSubTypeListHelper: DeclarationMessageTypeList.Codes.TransitNcts5Query, expectedValuationDate: ZDateTime.Empty)
			};

			foreach (var (phase, messageType, expectedValuationDate) in testCases)
			{
				nctsHeader.BH_ApplicationCode = phase;
				nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.Empty;

				var sender = GetMessageSenderForSpecificMessageType(messageType);
				var messageBuilders = sender.GetMessageBuildersData();
				ESNctsMessageSender.Send(messageBuilders);

				AssertEquals($"Message type: {messageType}", expectedValuationDate, nctsHeader.MovementHeader.BM_ValuationDate);
			}
		}

		public void TestSendDepartureMessage_DPM_Phase5_NoNewGuaranteeTransactionWhenAmountGreaterThan0()
		{
			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Amendment);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				AssertEquals("Transactions for guarantee1 after sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 after sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);
			});
		}

		public void TestSendDepartureMessage_DPA_Phase5_NoNewGuaranteeTransactionWhenAmountGreaterThan0()
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode).CE_EntryNum = "MRN-Test1";

			var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "file1.pdf", "CIV");
			var pivot1 = nctsHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			var eDoc2 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "file2.pdf", "CIV");
			var pivot2 = nctsHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;

			Factory.Save();
			nctsHeader.DocManagerInfo.Save();

			SetUpGuaranteeTransactions();

			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "GUA1";

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 20m;
			guarantee2.PW_BondNumber = "GUA2";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Transactions for guarantee1 before sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 before sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, requestDispatch: "N");
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Annexes);
				nctsHeader.Messages.Reload(true);
				var msg = nctsHeader.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for the ncts header", msg);

				AssertEquals("Transactions for guarantee1 after sending", 1, guarantee1.CusGuarantee.CusGuaranteeLineTransactions.Count);
				AssertEquals("Transactions for guarantee2 after sending", 1, guarantee2.CusGuarantee.CusGuaranteeLineTransactions.Count);
			});
		}

		public void TestSendAllButTheLastNctsAnnexMessage_RequestDispatchN()
		{
			var declaration = CreateNctsAnnexDeclaration(true);

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, declaration, requestDispatch: "N");
				var messageBuilders = sender.GetMessageBuildersData();
				var result = ESNctsMessageSender.Send(messageBuilders);
				AssertEquals("2 annex messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("CustomsStatus has not changed ", NctsTransitStatusList.Codes.GoodsUnderCustomsControl, declaration.MovementHeader.BM_CustomsStatus);
				AssertEquals("RequestDispatch was set to N", "N", declaration.RequestDispatch);
				AssertEquals("NctsHeader status is SNT", LogicalStatusList.Codes.Sent, declaration.MovementHeader.BM_MessageStatus);
				AssertEquals("NctsHeader phase status is DOT", ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, declaration.MovementHeader.BM_Phase);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				declaration.Messages.Reload(true);
				var msgs = declaration.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the nctsHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type DPA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "finAnexos>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "finAnexos>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = declaration.EDocPivotCollection.Cast<NctsCusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 9, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = declaration.EDocPivotCollection.Cast<NctsCusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to 4 of the sent annexes", 4, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendAllButTheLastNctsAnnexMessage_RequestDispatchY()
		{
			var declaration = CreateNctsAnnexDeclaration(true);

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, declaration, requestDispatch: "Y");
				var messageBuilders = sender.GetMessageBuildersData();
				var result = ESNctsMessageSender.Send(messageBuilders);
				AssertEquals("2 annex messages have been created and sent, MessagesSent is 2", 2, result.MessagesSent);
				AssertEquals("CustomsStatus has not changed ", NctsTransitStatusList.Codes.GoodsUnderCustomsControl, declaration.MovementHeader.BM_CustomsStatus);
				AssertEquals("RequestDispatch was set to Y", "Y", declaration.RequestDispatch);
				AssertEquals("NctsHeader status is SNT", LogicalStatusList.Codes.Sent, declaration.MovementHeader.BM_MessageStatus);
				AssertEquals("NctsHeader phase status is DOT", ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, declaration.MovementHeader.BM_Phase);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				declaration.Messages.Reload(true);
				var msgs = declaration.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == "TRX").ToList();
				AssertEquals("There are 2 TRX EDIMessages created for the entryHeader", 2, msgs.Count);
				AssertEquals("New EDIMessages are type DPA", true, msgs.All(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes));
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, first", "finAnexos>N", msgs[0].EM_MessageText);
				AssertContains("New EDIMessages have dispatch request flag to N because there is one annex left to send, second", "finAnexos>N", msgs[1].EM_MessageText);

				var pivotsForFirstMessage = declaration.EDocPivotCollection.Cast<NctsCusStorageDocPivot>().Where(x => x.Message == msgs[0]);
				AssertEquals("First new EDIMessage is associated to 9 of the sent annexes", 9, pivotsForFirstMessage.Count());

				var pivotsForSecondMessage = declaration.EDocPivotCollection.Cast<NctsCusStorageDocPivot>().Where(x => x.Message == msgs[1]);
				AssertEquals("Second new EDIMessage is associated to 4 of the sent annexes", 4, pivotsForSecondMessage.Count());
			});
		}

		public void TestSendLastNctsAnnexMessage_RequestDispatchN()
		{
			var declaration = CreateNctsAnnexDeclaration(false);

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, declaration, requestDispatch: "N");
				var messageBuilders = sender.GetMessageBuildersData();
				var result = ESNctsMessageSender.Send(messageBuilders);
				AssertEquals("No annex message has been created or sent since there is only one annex left and request dispatch is N, MessagesSent is 0", 0, result.MessagesSent);
				AssertEquals("NctsHeader doesn't have messages", false, declaration.Messages.Any());
				AssertNotEquals("NctsHeader status not SNT", LogicalStatusList.Codes.Sent, declaration.MovementHeader.BM_MessageStatus);
				AssertNotEquals("NctsHeader phase status is not DOT", ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, declaration.MovementHeader.BM_Phase);
				AssertEquals("RequestDispatch was not changed", ZString.Empty, declaration.RequestDispatch);
				AssertEquals("LastKeyReported has no exception", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendLastNctsAnnexMessage_RequestDispatchY()
		{
			var declaration = CreateNctsAnnexDeclaration(false);

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, declaration, requestDispatch: "Y");
				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, header: declaration);
				AssertEquals("CustomsStatus has not changed ", NctsTransitStatusList.Codes.GoodsUnderCustomsControl, declaration.MovementHeader.BM_CustomsStatus);
				AssertEquals("RequestDispatch was set to Y", "Y", declaration.RequestDispatch);
				declaration.Messages.Reload(true);
				var msg = declaration.Messages.LastOutgoingMessage;
				AssertNotNull("EDIMessage was created for entry header", msg);
				AssertEquals("New EDIMessage is type DPA", DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, msg.EM_MessageType);
				AssertContains("New EDIMessage has dispatch request flag to S since it is the last annex and the flag in the form is Y", "finAnexos>S", msg.EM_MessageText);

				var msgPivot = declaration.EDocPivotCollection.Cast<NctsCusStorageDocPivot>().FirstOrDefault().Message;
				AssertEquals("New EDIMessage is associated to the annex", msgPivot, msg);
			});
		}

		public void TestTrySendLastNctsAnnexMessageWithError()
		{
			var declaration = CreateNctsAnnexDeclaration(false, "Invoice.xxx");

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, declaration, requestDispatch: "Y");
				var messageBuilders = sender.GetMessageBuildersData();
				var result = ESNctsMessageSender.Send(messageBuilders);
				AssertEquals("The message was created but could not be sent, MessagesWithSendFailure is 1", 1, result.MessagesWithSendFailure);
				AssertEquals("NctsHeader doesn't have messages", false, declaration.Messages.Any());
				AssertNotEquals("NctsHeader status not SNT", LogicalStatusList.Codes.Sent, declaration.MovementHeader.BM_MessageStatus);
				AssertNotEquals("NctsHeader phase status is not DOT", ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, declaration.MovementHeader.BM_Phase);
				AssertEquals("RequestDispatch was not changed", ZString.Empty, declaration.RequestDispatch);
				AssertEquals("LastKeyReported has exception in the sender", "ESNctsMessageSender.Send", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendArrivalMessage_Phase5()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var nctsBill = nctsHeader.Bills.AddNew();
			nctsBill.ArrivalGoodsItems.AddNew();

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Arrival);
			});
		}

		public void TestSendTNNMessage()
		{
			var mrnCode = "ES239928883";

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = mrnCode;
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;

			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = mrnCode;
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData);
				AssertEquals("NctsHeader message status has changed for nctsHeaderDeparture, BM_MessageStatus", LogicalStatusList.Codes.Sent, tnnMovement.BM_MessageStatus);
			});
		}

		public void TestSendUnloadingRemarksMessage()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var nctsBill = nctsHeader.Bills.AddNew();
			nctsBill.ArrivalGoodsItems.AddNew();

			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks);
			});
		}

		public void TestSendTQUMessage_Phase5_CustomsStatusNotPRE()
		{
			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.TransitNcts5Query);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender);
				AssertEquals("UpdatePreDeclaration is left as false", false, nctsHeader.UpdatePreDeclaration);
			});
		}

		public void TestSendTQUMessage_Phase5_CustomsStatusPRE()
		{
			CombineAssertions(() =>
			{
				SendMessageWithNonExistentMessageTypeAndAssertErrorInSender();

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
				var sender = GetMessageSenderForSpecificMessageType(DeclarationMessageTypeList.Codes.TransitNcts5Query);
				SendMessageWithNullMessageBuilderAndAssertErrorInSend(sender);

				SendMessageAndAssertCorrectResult(sender);
				AssertEquals("UpdatePreDeclaration is set to true", true, nctsHeader.UpdatePreDeclaration);
			});
		}

		void SendMessageWithNonExistentMessageTypeAndAssertErrorInSender()
		{
			var emptyNctsHeader = Factory.New<NctsHeader>();
			var messageSendingObject = new NctsMessageSendingObject(emptyNctsHeader, staff);
			messageSendingObject.Factory.RefreshEnabled = false;
			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(messageSendingObject);
			var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = nonExistentMessageType;
			var sender = new ESNctsMessageSender(messageSendingObjectParent);
			var messageBuilders = sender.GetMessageBuildersData();
			var result = ESNctsMessageSender.Send(messageBuilders);
			AssertEquals("The message was created but could not be sent", 1, result.MessagesWithSendFailure);
			AssertEquals("LastKeyReported has exception in the builder", "ESNctsMessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		void SendMessageWithNullMessageBuilderAndAssertErrorInSend(ESNctsMessageSender sender, string phaseStatus = "", string messageStatus = LogicalStatusList.Codes.Sent, NctsHeader header = null)
		{
			var messageBuilders = new List<MessageBuilderData> { new MessageBuilderData { MessageBuilder = null } };
			var result = ESNctsMessageSender.Send(messageBuilders);
			AssertEquals("The message was created but could not be sent", 1, result.MessagesWithSendFailure);
			AssertEquals("NctsHeader doesn't have messages", false, nctsHeader.Messages.Any());
			AssertEquals("LastKeyReported has exception in the sender", "ESNctsMessageSender.Send", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		void SendMessageAndAssertCorrectResult(ESNctsMessageSender sender, string phaseStatus = "", string messageStatus = LogicalStatusList.Codes.Sent, NctsHeader header = null)
		{
			var messageBuilders = sender.GetMessageBuildersData();
			var result = ESNctsMessageSender.Send(messageBuilders);
			Factory.Save();
			AssertEquals("The message has been created and sent", 1, result.MessagesSent);
			AssertEquals("NctsHeader message status has changed, BM_MessageStatus", messageStatus, (header ?? nctsHeader).EffectiveMessageStatus);
			AssertEquals("NctsHeader phase status is correct", phaseStatus, (header ?? nctsHeader).CommonMovementHeader.BM_Phase);
			AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			(header ?? nctsHeader).Messages.Reload(true);
			var msg = (header ?? nctsHeader).Messages.LastOutgoingMessage;
			AssertNotNull("EDIMessage was created for the ncts header", msg);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.FillWithValidTestData();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;

			nctsHeader.MovementHeader.GoodsItems.AddNew();
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = certificate.CertificateName;

			Factory.Save();
		}
		NctsHeader nctsHeader;

		void SetUpGuaranteeTransactions()
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = "GUA1";
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader1.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			var transaction1 = guaranteeHeader1.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = "Entry Number";
			transaction1.CPL_TranValue = 100m;
			transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction1.CPL_AppId = "Entry Reference";
			transaction1.CPL_Comment = "Instruction Desc.";
			transaction1.CPL_Procedure = "AAA";

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = "GUA2";
			guaranteeHeader2.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader2.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			var transaction2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
			transaction2.CPL_Reference = "Entry Number";
			transaction2.CPL_TranValue = 100m;
			transaction2.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction2.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction2.CPL_AppId = "Entry Reference";
			transaction2.CPL_Comment = "Instruction Desc.";
			transaction2.CPL_Procedure = "AAA";

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
		}

		void AssertGuaranteeTransaction(ZString assertMessageText, BaseCusGuaranteeLineTransaction transaction, ZDecimal guaranteeAmount)
		{
			AssertEquals(assertMessageText + "'s Transaction Reference", nctsHeader.BH_JobReference, transaction.CPL_Reference);
			AssertEquals(assertMessageText + "'s Transaction Comment", "NCTS departure " + nctsHeader.LocalReferenceNumber, transaction.CPL_Comment);
			AssertEquals(assertMessageText + "'s Transaction Category", PermitTransactionCategoryList.Codes.CUM, transaction.CPL_TransactionCategory);
			AssertEquals(assertMessageText + "'s Transaction Type", PermitTransactionTypeList.Codes.TRA, transaction.CPL_TransactionType);
			AssertEquals(assertMessageText + "'s Transaction Value", -guaranteeAmount, transaction.CPL_TranValue);
			AssertEquals(assertMessageText + "'s Transaction ID", "1", transaction.CPL_AppId);
			AssertEquals(assertMessageText + "'s Transaction Status", PermitTransactionStatusList.Codes.Pending, transaction.CPL_TransactionStatus);
			AssertEquals(assertMessageText + "'s Transaction Reference Line No.", 0, transaction.CPL_ReferenceNumberLine);
		}

		ESNctsMessageSender GetMessageSenderForSpecificMessageType(ZString messageType, NctsHeader header = null, string requestDispatch = "", string reasonForCancellation = "")
		{
			var messageSendingObject = new NctsMessageSendingObject(header ?? nctsHeader, staff);
			messageSendingObject.Factory.RefreshEnabled = false;
			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(messageSendingObject);
			var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = messageType;
			sendingObject.RequestDispatch = requestDispatch;
			sendingObject.ReasonForCancellation = reasonForCancellation;
			return new ESNctsMessageSender(messageSendingObjectParent);
		}

		NctsHeader CreateNctsAnnexDeclaration(ZBool addMultipleAnnexDocs, string filename = "Invoice.pdf")
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = certificate.CertificateName;

			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode).CE_EntryNum = "MRN-Test1";

			Factory.Save();

			var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], filename, "CIV");
			var pivot1 = nctsHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			if (addMultipleAnnexDocs)
			{
				for (int i = 0; i <= 12; i++)
				{
					var eDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice" + i + ".txt", "MSC");
					var pivot = nctsHeader.EDocPivotCollection.AddNew();
					pivot.CSD_StorageDocReference = eDoc.UniqueKey;
				}
			}

			Factory.Save();
			nctsHeader.DocManagerInfo.Save();

			return nctsHeader;
		}

		CertificateProviderTestClass certificate;
		GlbStaff staff;
	}
}
