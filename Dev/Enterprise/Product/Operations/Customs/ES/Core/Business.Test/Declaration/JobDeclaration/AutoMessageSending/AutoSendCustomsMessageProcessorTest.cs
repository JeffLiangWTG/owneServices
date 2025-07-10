using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Customs.ES.Business.ESConstants;
using CusTempStorageRegLineTransactionInternalReferenceTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionInternalReferenceTypeList;
using CusTempStorageRegLineTransactionStatusList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(AutoSendCustomsMessageProcessor))]
sealed class AutoSendCustomsMessageProcessorTest : Customs.Business.Testing.AutoSendCustomsMessageProcessorTest
{
	[TestDate(2019, 1, 1, 12, 0, 0)]
	[TestUtcOffset(1, 0, 0)]
	public void TestEndToEndTest_CheckNudgeIsDone()
	{
		var declaration = Factory.New<Customs.Business.BaseJobDeclaration>();
		PrepareDeclaration(declaration);
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "INV11071801";
		invoice.JZ_InvoiceAmount = 10000m;
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "2106900300";
		invoiceLine.JI_LinePrice = 10000m;
		PrepareInvoiceLine(invoiceLine);
		Factory.Save();
		AssertNull("Entry should be null as job is not merged.", GetEntryHeader(declaration));

		var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

		using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
		using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
		{
			var processor = CreateProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
			AssertEquals(1, informationNotifications.Length);
			AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<Customs.Business.BaseJobDeclaration>(declaration.PK);
			var entry = GetEntryHeader(loadedDeclaration);
			AssertNotNull("Entry should NOT be empty as job should have been merged", entry);
			AssertEntryAndMessageResultForEndToEndTest(entry);

			mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("ESS", TimeSpan.FromMinutes(10)), Times.Exactly(1));

			processor = CreateProcessor(loadedDeclaration);
			processor.Process(notifications);
			AssertContains(ExpectedLastNotification, notifications.AsString);

			newFactory = new BusinessObjectFactory();
			loadedDeclaration = newFactory.Load<Customs.Business.BaseJobDeclaration>(declaration.PK);
			entry = GetEntryHeader(loadedDeclaration);
			AssertEntryAndMessageResultForEndToEndTest(entry);
		}
	}

	#region Inventory Management PDC

	public void TestSendCustomsMessage_PDC_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods();
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(setCorrectPremisesInHeader: false);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005. Please, set the correct location before submitting this declaration to Customs.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1. Remaining Gross Weight in the Temporary Storage: 16 , ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_PDC_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	#endregion

	#region Inventory Management PDS

	public void TestSendCustomsMessage_PDS_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(entryInstructionSubStyle: EntrySubStyleList.Codes.C);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(entryInstructionSubStyle: EntrySubStyleList.Codes.C, locationInEntry: ZString.Empty);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(entryInstructionSubStyle: EntrySubStyleList.Codes.C, locationInPremises: "9999000005");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(entryInstructionSubStyle: EntrySubStyleList.Codes.C, shouldAddDoc: false);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(entryInstructionSubStyle: EntrySubStyleList.Codes.C, regHeaderReference: "reference");
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, setCorrectPremisesInHeader: false);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005. Please, set the correct location before submitting this declaration to Customs.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, prevDocLineNo: 2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, packageVin: "AAAA", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, packageQtyNotBulk: 2, transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, transactionGrossWeight: 5m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1. Remaining Gross Weight in the Temporary Storage: 16 , ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_PDS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.C, entryInstructionSubStyle2: EntrySubStyleList.Codes.C, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
			});
		}
	}

	#endregion

	public void TestSendToCustoms_T2I_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_NothingIsDone()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(entryInstructionSubStyle1: EntrySubStyleList.Codes.T2L, entryInstructionSubStyle2: EntrySubStyleList.Codes.T2L, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;
			entryHeader1.ZG_POUSVersion = POUSVersionCodes.POUS2;
			entryHeader2.ZG_POUSVersion = POUSVersionCodes.POUS2;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.T2lReceptionPous, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.T2lReceptionPous, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	#region Inventory Management EXS

	public void TestSendCustomsMessage_EXS_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithoutSUMdoc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005. Please, set the correct location before submitting this declaration to Customs.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1. Remaining Gross Weight in the Temporary Storage: 16 , ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}
	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithXSUMdocWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "XSUM", entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithXSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "XSUM", entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithXSUMdocWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "XSUM", entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "N337", entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "N337", entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_EXS_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration, docCode: "N337", entryInstructionSubStyle1: ExsEntrySubStyleList.Codes.EXS, entryInstructionSubStyle2: ExsEntrySubStyleList.Codes.EXS, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExitSummaryDeclaration);
			});
		}
	}

	#endregion

	#region Inventory Management H2

	public void TestSendCustomsMessage_H2_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWithout337doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(false, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005. Please, set the correct location before submitting this declaration to Customs.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(prevDocLineNo: 2, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightError_And_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1. Remaining Gross Weight in the Temporary Storage: 16 , ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, docCode: "337", regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the Temporary Storage 7 for TSD Number 99994000128, Item 1. This can cause mismatches in the stock at ES Customs records. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2, docReference: FormattedDocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	public void TestSendCustomsMessage_H2_TemporaryStorageEnabledWithPremisesWith337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration, docCode: "337", entryInstructionStyle1: IMPDeclarationTypeList.Codes.H2, entryInstructionStyle2: IMPDeclarationTypeList.Codes.H2, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.DvdH2, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);
			});
		}
	}

	#endregion

	#region Inventory Management EDP

	public void TestSendCustomsMessage_EDP_TemporaryStorageNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, message.EM_MessageType);

				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWithout1217doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false, messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				entryHeader.Messages.Reload(true);
				var message = entryHeader.Messages.LastOutgoingMessage;
				AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, message.EM_MessageType);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docInJobDeclaration()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = declaration.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(0, informationNotifications.Length);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertNotEquals("Entry2 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader2", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00002: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.|TYP=Sending Error", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader2.Messages.Reload(true);
				AssertEquals("Entry2 has no messages", 0, entryHeader2.Messages.Count);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docInEntryInstruction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.EntryInstruction.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docInInvoiceHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceHeaders.FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: For managed LAME storages, LAME Reception Certificate (document 1217) must always be declared at Invoice Line level to be able to manage the inventory properly. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWithMultiple1217docsWithoutQuantity()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";
			supDoc.CSI_UnitOfQuantity = "KGM";

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWithMultiple1217docsWithoutUnitOfQuantity()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;
			var supDoc = entryHeader1.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().SupportingDocuments.AddNew();
			supDoc.CSI_Code = "1217";
			supDoc.CSI_ReferenceNumber = "Reference";
			supDoc.CSI_Quantity = 20m;
			supDoc.CSI_UnitOfQuantity = "AAA";

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001 / Invoice 1 / 1: For invoice lines where there is more than one LAME Certificate, it is needed to specify the corresponding Gross Weight for each certificate in column Quantity, setting UOM to KGM, so the inventory can be managed properly. Please, set that data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader, regLineTransaction) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference", docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			var declaration = entryHeader.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(0, informationNotifications.Length);

				AssertEquals("Entry Message Status has not changed to awaiting for entry header", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("Entry Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is no record in the Temporary Storage Register for LAME Reception Certificate 24ES00999980001282. You might have mistaken the number. |TYP=Sending Error", entryHeader.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader.Messages.Reload(true);
				AssertEquals("Entry has no messages", 0, entryHeader.Messages.Count);

				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(false, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Goods in LAME Reception Certificate 24ES00999980001282 are not stored in location 9999000002. The correct location should be 9999000005. Please, set the correct location before submitting this declaration to Customs.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(regLineItemNo: 2, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is no item line 1 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageVin: "AAAA", transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: VIN VIN1 is not present in the LAME under Reception Certificate Number 24ES00999980001282. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 24ES00999980001282: 9 BX. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_NotBulk_WithPackagesInSupportingDocuments()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(packageQtyNotBulk: 2, transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, packageQtyNotBulkInSupportingDocuments: 3);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 24ES00999980001282: 3 BX. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There is not enough quantity of goods in the LAME for Reception Certificate Number 24ES00999980001282: 0 VG. Please, correct data and send again.|TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithGrossWeightError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 5m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: There might not be enough Gross Weight 34 for Reception Certificate Number 24ES00999980001282. Remaining Gross Weight in the Temporary Storage: 33 |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises_WithGrossWeightForVINsError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(transactionGrossWeight: 80m, transactionGrossWeightOBLForVINs: 7m, docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertNotEquals("Entry1 Message Status has not changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertEquals("New event in logs for entryHeader1", "Declaration has not been sent|RES=Entry was not sent automatically because of a Temporary Storage Inventory Management error: ES00001: Gross weight 22 used for the declaration is different to the gross weight entered in the LAME 7 for Reception Certificate 24ES00999980001282. |TYP=Sending Error", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors).SL_Reference);

				entryHeader1.Messages.Reload(true);
				AssertEquals("Entry1 has no messages", 0, entryHeader1.Messages.Count);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has no new transactions", 3, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
			});
		}
	}

	public void TestSendCustomsMessage_EDP_TemporaryStorageEnabledWithPremisesWith1217docWithRegHeaderWithPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (entryHeader1, entryHeader2, regLineTransaction1, regLineTransaction2, regLine1, regLine2, regLine3, _) = SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(docCode: "1217", messageType: Common.Shared.SharedJobMessageTypeList.Codes.Export, regLineTransactionInternalReferenceType: CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, regHeaderReference: DocReference, transactionGrossWeightOBLForVINs: 22m);
			var declaration = entryHeader1.Declaration;

			Factory.Save();

			CombineAssertions(() =>
			{
				var processor = CreateProcessor(declaration);
				var notifications = new NotificationBuffer();
				processor.Process(notifications);

				var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
				AssertEquals(1, informationNotifications.Length);
				AssertContains(ExpectedMessageWhenSent, informationNotifications[0].Message);

				AssertEquals("Entry1 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader1.CH_Status);
				AssertEquals("Entry1 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader1.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader1", entryHeader1.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader1.Messages.Reload(true);
				var messageHeader1 = entryHeader1.Messages.LastOutgoingMessage;
				AssertEquals("Entry1's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader1.EM_MessageType);

				AssertEquals("Entry2 Message Status has changed to awaiting for entry header", MessageStatusList.Codes.AwaitingResponse, entryHeader2.CH_Status);
				AssertEquals("Entry2 Status has not changed to awaiting for entry header, it's empty", ZString.Empty, entryHeader2.CH_EntryStatus);
				AssertNull("No event in logs for entryHeader2", entryHeader2.Logs.MostRecentLogByEventTime(Events.DeclarationHasErrors));

				entryHeader2.Messages.Reload(true);
				var messageHeader2 = entryHeader2.Messages.LastOutgoingMessage;
				AssertEquals("Entry2's new message's type is correct", DeclarationMessageTypeList.Codes.ExportUcc6, messageHeader2.EM_MessageType);

				AssertEquals("regLineTransaction1 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2 was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction2.SRT_TransactionStatus);

				AssertEquals("regLine1 has a new transaction", 4, regLine1.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
				AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

				AssertNewTransactionToReserveGoods(regLine1, -1, -22m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
				AssertNewTransactionToReserveGoods(regLine2, 0, -34m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
				AssertNewTransactionToReserveGoods(regLine3, -9, -6m, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration);
			});
		}
	}

	#endregion

	protected override ZString ExpectedMessageDescription => "Spain Customs Declaration";

	protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
	{
		CombineAssertions(() =>
		{
			AssertEquals("One entry should be generated.", 1, entry.Declaration.CustomsEntryHeaders.Count);

			var message = entry.Messages[0];
			AssertEquals("EM_ApplicationCode should be ESC.", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
			AssertEquals("EM_MessageSubType should be ORG.", "ORG", message.EM_MessageSubType);
			AssertEquals("EM_MessageType should be EDP.", "EDP", message.EM_MessageType);
			AssertEquals("EM_ReceiveTransmit should be TRX.", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status should be QUE.", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("Message is not held for later sending.", ZDateTime.Empty, message.EM_HeldUntilDate);
			AssertEquals("EM_ApplicationReference should have JE_CustomsProfile", "AAA", message.EM_ApplicationReference);
		});
	}

	protected override IProcessor CreateProcessor(Customs.Business.BaseJobDeclaration declaration)
	{
		return new AutoSendCustomsMessageProcessor(declaration);
	}

	protected override Customs.Business.CusEntryHeader GetEntryHeader(Customs.Business.BaseJobDeclaration declaration)
	{
		return declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().FirstOrDefault();
	}

	protected override void PrepareDeclaration(Customs.Business.BaseJobDeclaration declaration)
	{
		var esDeclaration = (JobDeclaration)declaration;
		esDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		esDeclaration.CustomsEntryInstructions.AddNew().CEI_SubStyle = EntrySubStyleList.Codes.A;
		esDeclaration.JE_CustomsProfile = "AAA";
	}

	protected override void PrepareInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
	{
		base.PrepareInvoiceLine(invoiceLine);
		invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions[0].PK;
	}

	protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
	{
		entry.EntryNumber = "123";
		entry.CH_EntryStatus = ZString.Empty;
	}
	protected override bool MessageCanBeAutoSentForLodgedEntries => false;

	const string EntryReference1 = "ES00001";
	const string EntryReference2 = "ES00002";
	const string LocationInEntry = "9999000002";
	const string DocCode = "SUM";
	const string DocReference = "24ES00999980001282";
	const string FormattedDocReference = "99994000128";
	const string ExpectedMessageWhenSent = "message has been sent to customs for Job";

	(CusEntryHeader entryHeader, CusTempStorageRegLineTransaction regLineTransaction) SetUpDataForForReserveTemporaryStorageGoods
			(bool shouldAddDoc = true, string docCode = DocCode, string docReference = DocReference, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, string regHeaderReference = FormattedDocReference,
			string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Import, string regLineTransactionInternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, string entryInstructionSubStyle = EntrySubStyleList.Codes.A, string entryInstructionStyle = "")
	{
		var isExportLAME = regLineTransactionInternalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration;

		var declaration = Factory.GetNewJobDeclaration(Staff, Supplier, Importer, Declarant, messageType: messageType, entryInstructionSubStyle1: entryInstructionSubStyle, entryInstructionStyle1: entryInstructionStyle);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		declaration.Factory.Save();
		entryHeader.CH_BGMReference = EntryReference1;

		if (shouldAddDoc)
		{
			var invoiceLine = entryHeader.AllEntryLines[0].RandomLine;

			if (isExportLAME)
			{
				var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
				supportingDoc.CSI_Code = docCode;
				supportingDoc.CSI_ReferenceNumber = docReference;
			}
			else
			{
				var previousDoc = invoiceLine.PreviousDocuments.AddNew();
				previousDoc.CSI_Code = docCode;
				previousDoc.CSI_ReferenceNumber = docReference;
			}
		}

		declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = locationInEntry;

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = isExportLAME ? "LAM" : "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransaction.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;

		return (entryHeader, regLineTransaction);
	}

	(CusEntryHeader entryHeader1, CusEntryHeader entryHeader2, CusTempStorageRegLineTransaction regLineTransaction1, CusTempStorageRegLineTransaction regLineTransaction2, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3, EU.TemporaryStorage.Business.CusTempStorageRegLineItem regLineItem)
		SetUpDataForForReserveTemporaryStorageGoodsWith2Headers(bool setCorrectPremisesInHeader = true, int prevDocLineNo = 1, int regLineItemNo = 1, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m, bool isForAmendment = false, string bulkPackageTypeForRegLine = "VG",
		string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Import, string regLineTransactionInternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration, string docCode = DocCode, string docReference = DocReference,
		string entryInstructionSubStyle1 = EntrySubStyleList.Codes.A, string entryInstructionSubStyle2 = EntrySubStyleList.Codes.B, string entryInstructionStyle1 = "", string entryInstructionStyle2 = "", string regHeaderReference = FormattedDocReference, decimal transactionGrossWeightOBLForVINs = 5m, int packageQtyNotBulkInSupportingDocuments = 0)
	{
		Factory.SetBulkTypeHelper();
		var isExportLAME = regLineTransactionInternalReferenceType == CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration;

		var declaration = Factory.GetNewJobDeclaration(Staff, Supplier, Importer, Declarant, messageType: messageType, createSecondEntry: true, createInvLinesInDifferentInvHeaders: true, entryInstructionSubStyle1: entryInstructionSubStyle1, entryInstructionSubStyle2: entryInstructionSubStyle2, entryInstructionStyle1: entryInstructionStyle1, entryInstructionStyle2: entryInstructionStyle2);
		var entryHeader1 = declaration.CustomsEntryHeaders.First(e => e.EntryInstruction.CEI_SubStyle == entryInstructionSubStyle1);
		var entryHeader2 = declaration.CustomsEntryHeaders.First(e => e != entryHeader1 && e.EntryInstruction.CEI_SubStyle == entryInstructionSubStyle2);
		declaration.Factory.Save();
		entryHeader1.CH_BGMReference = EntryReference1;
		entryHeader2.CH_BGMReference = EntryReference2;

		var invoiceLine = entryHeader1.AllEntryLines[0].RandomLine;
		var vehicle = invoiceLine.Vehicles.AddNew();
		invoiceLine.JI_Weight = 22m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		vehicle.CVH_VehicleIdentificationNumber = "VIN1";

		if (isExportLAME)
		{
			var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = docCode;
			supportingDoc.CSI_ReferenceNumber = docReference;
			if (packageQtyNotBulkInSupportingDocuments != 0)
			{
				supportingDoc.CSI_PackQty = packageQtyNotBulkInSupportingDocuments;
				supportingDoc.CSI_PackType = "BX";
			}
		}
		else
		{
			var previousDoc = invoiceLine.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = docCode;
			previousDoc.CSI_ReferenceNumber = docReference;
			previousDoc.CSI_LineNo = prevDocLineNo;
		}

		var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
		var packageInfo1 = packingGroups.Packages.AddNew();
		packageInfo1.CW_PackQty = 9;
		packageInfo1.CW_PackType = "BX";
		packageInfo1.CW_MarksAndNos = "marks";
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var packageInfo2 = packingGroups.Packages.AddNew();
		packageInfo2.CW_PackQty = 0;
		packageInfo2.CW_PackType = "VG";
		packageInfo2.CW_MarksAndNos = "bulk gas marks";
		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 0;
		invoiceLine.PackagesPivot.Add(pack2);

		if (isExportLAME)
		{
			var invoiceHeader = invoiceLine.InvoiceHeader;
			var entryInstruction = invoiceLine.EntryInstruction;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "2203001010";
			invoiceLine2.JI_Weight = 8m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.PackagesPivot.Remove(pack1);
			invoiceLine2.PackagesPivot.Add(pack1);

			var supportingDoc2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = docCode;
			supportingDoc2.CSI_ReferenceNumber = docReference;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "2203001010";
			invoiceLine3.JI_Weight = 4m;
			invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.PackagesPivot.Remove(pack2);
			invoiceLine3.PackagesPivot.Add(pack2);

			var supportingDoc3 = invoiceLine3.SupportingDocuments.AddNew();
			supportingDoc3.CSI_Code = docCode;
			supportingDoc3.CSI_ReferenceNumber = docReference;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);
			Factory.Save();
		}

		declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = LocationInEntry;
		declaration.CustomsEntryInstructions[1].GoodsLocation.Address.AuthorisationNumber = LocationInEntry;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Type = isExportLAME ? "LAM" : "ADT";
		premises1.SRP_CustomsLocation = LocationInEntry;
		premises1.SRP_Code = "X";
		premises1.SRP_Description = "DESC";
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;
		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Type = isExportLAME ? "LAM" : "ADT";
		premises2.SRP_CustomsLocation = "9999000005";
		premises2.SRP_Code = "A";
		premises2.SRP_Description = "DESC2";
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = setCorrectPremisesInHeader ? premises1.PK : premises2.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = isForAmendment ? "CT" : "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND1.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransactionPND1.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		var regLineTransactionPND2 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND2.SRT_InternalReferenceNumber = EntryReference2;
		regLineTransactionPND2.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransaction1.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		regLineTransaction1.SRT_PackageQty = 10;
		regLineTransaction1.SRT_GrossWeight = transactionGrossWeightOBLForVINs;

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference1;
		regLineTransaction2.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		regLineTransaction2.SRT_PackageQty = 10;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 3;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = regLineTransactionInternalReferenceType;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;
		regLineTransaction3.SRT_GrossWeight = 6m;

		var regLineItem = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = regLineItemNo;

		var regLineItemPivot1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		return (entryHeader1, entryHeader2, regLineTransactionPND1, regLineTransactionPND2, regLine1, regLine2, regLine3, regLineItem);
	}

	void AssertNewTransactionToReserveGoods(CusTempStorageRegLine regLine, ZInt expectedPackQty, ZDecimal expectedGrossWeight, string expectedInternalRefType, string expectedComment = "")
	{
		var lineNum = regLine.SRL_LineNumber;
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_GrossWeight == expectedGrossWeight);
		AssertNotNull("Line " + lineNum + " has new transaction", transaction);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceNumber correct", EntryReference1, transaction.SRT_InternalReferenceNumber);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceType correct", expectedInternalRefType, transaction.SRT_InternalReferenceType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_PackageQty correct", expectedPackQty, transaction.SRT_PackageQty);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_GrossWeight correct", expectedGrossWeight, transaction.SRT_GrossWeight);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_Comments correct", expectedComment, transaction.SRT_Comments);
	}

	public GlbStaff Staff
	{
		get
		{
			if (staff == null)
			{
				staff = Factory.GetStaffAccount();
			}

			return staff;
		}
	}
	GlbStaff staff;

	public OrgHeader Declarant
	{
		get
		{
			if (declarant == null)
			{
				declarant = Factory.GetNewDeclarant();
			}
			return declarant;
		}
	}
	OrgHeader declarant;

	public OrgHeader Importer
	{
		get
		{
			if (importer == null)
			{
				importer = Factory.GetNewImporter();
			}
			return importer;
		}
	}
	OrgHeader importer;

	public OrgHeader Supplier
	{
		get
		{
			if (supplier == null)
			{
				supplier = Factory.GetNewSupplier();
			}
			return supplier;
		}
	}
	OrgHeader supplier;
}
