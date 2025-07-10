using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var oHeader = Factory.New<CusEntryHeader>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(oHeader,
				"KRCusEntryHeader",
				schemaTypeName: nameof(AutoCusEntryHeader.Schema));
		}

		public void TestAddLog()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var eventTime = new ZDateTimeOffset(2020, 9, 1);
			entryHeader.AddLog(Events.StatusUpdated, "REF", eventTime);

			var log = entryHeader.Logs.MostRecentLog;
			AssertEquals(Events.StatusUpdated.Code, log.SL_SE_NKEvent);
			AssertEquals(eventTime.ToZDateTime(), log.SL_EventTime);
			AssertEquals("REF", log.SL_Reference);
		}

		public void TestCESLoggingSuspender()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			AssertEquals(0, entry.Logs.GetAllLogs().Count);
			using (entry.SuspendCESLog())
			{
				entry.CH_EntryStatus = "CLR";
			}
			AssertEquals("No Log has been added", 0, entry.Logs.GetAllLogs().Count);

			entry.CH_EntryStatus = "ANT";
			AssertEquals("One Log has been added", 1, entry.Logs.GetAllLogs().Count);
			var log = entry.Logs.GetAllLogs().Cast<StmALog>().Single();
			AssertEquals("CES", log.SL_SE_NKEvent);
			AssertEquals("ANT", log.SL_Reference);
		}

		public void TestAddMessageAcceptedLogs()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			var messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(0, messageAcceptedLogs.Count());
			Assert(!entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(0, messageAcceptedLogs.Count());
			Assert(!entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(0, messageAcceptedLogs.Count());
			Assert(!entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(1, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(1, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(1, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(1, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(2, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(2, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationRejected;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(2, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(2, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(!entry.HasBeenWithdrawn);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted;
			messageAcceptedLogs = entry.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.MessageAcceptedCode);
			AssertEquals(3, messageAcceptedLogs.Count());
			Assert(entry.HasBeenLodgedAtCustoms);
			Assert(entry.HasBeenWithdrawn);
		}

		public void TestPivotsToContainersIsLoaded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var pivot = entry.PivotsToContainers.GetOrCreatePivotFor(container);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
			entry.LoadChildEditableObjects();
			AssertEquals("db hits", 1, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));
			AssertEquals(1, entry.PivotsToContainers.Count);
			AssertEquals(pivot.PK, entry.PivotsToContainers[0].PK);
		}

		public void TestSnapshotIsDeleted()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._830;

			var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			Assert(snapshot.IsDeleted);

			snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			Assert(!snapshot.IsDeleted);
			AssertEquals("Status is updated", EntrySnapshotStatus.Lodged, snapshot.CES_Status);

			var snapshot2 = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			Assert(!snapshot.IsDeleted);
			Assert(!snapshot2.IsDeleted);
			AssertEquals("Status is updated", EntrySnapshotStatus.Lodged, snapshot.CES_Status);
			AssertEquals("Status is updated", EntrySnapshotStatus.Lodged, snapshot2.CES_Status);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
			Assert(!snapshot.IsDeleted);
			Assert(!snapshot2.IsDeleted);
			AssertEquals("Status is updated", EntrySnapshotStatus.Lodged, snapshot.CES_Status);
			AssertEquals("Status is updated", EntrySnapshotStatus.Lodged, snapshot2.CES_Status);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted;
			Assert(!snapshot.IsDeleted);
			Assert(!snapshot2.IsDeleted);
			AssertEquals("Status is not updated", EntrySnapshotStatus.Lodged, snapshot.CES_Status);
			AssertEquals("Status is not updated", EntrySnapshotStatus.Lodged, snapshot2.CES_Status);
			AssertEquals(2, entry.Snapshots.Count);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
			Assert(!snapshot.IsDeleted);
			Assert(!snapshot2.IsDeleted);
			AssertEquals("Status is updated", EntrySnapshotStatus.Deleted, snapshot.CES_Status);
			AssertEquals("Status is updated", EntrySnapshotStatus.Deleted, snapshot2.CES_Status);
			AssertEquals("All snapshots get marked as Deleted and no deleted snapshots are in the collection", 0, entry.Snapshots.Count);
		}

		public void TestEntryStatusList()
		{
			var entry = (ICusEntryNumEntryStatusListProvider)Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			AssertEquals(typeof(CustomsMessageStatusTypeList), entry.EntryStatusList.GetType());
		}

		[TestDate(2021, 10, 10)]
		public void TestGenerateEntryNumberWhenFirstSave()
		{
			var impEntry1 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import);
			Factory.Save();
			var impEntryNumber = impEntry1.EntryNumber;
			AssertEquals("entry is in db", true, impEntry1.IsInDatabase);
			AssertEquals("entry number is assigned", false, impEntryNumber.IsEmpty);
			AssertEquals("entry number length is 14", 14, impEntryNumber.Length);
			AssertEquals("entry number is assigned", "6N00221000001M", impEntryNumber);

			var impEntry2 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._11);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000002M", impEntry2.EntryNumber);

			var impEntryB1 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._12);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000003B", impEntryB1.EntryNumber);

			var impEntryB2 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._27);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000004B", impEntryB2.EntryNumber);

			var impEntryB3 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._31);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000005B", impEntryB3.EntryNumber);

			var impEntryS1 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._18);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000006S", impEntryS1.EntryNumber);

			var impEntryS2 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._24);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000007S", impEntryS2.EntryNumber);

			var impEntryS3 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._30);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000008S", impEntryS3.EntryNumber);

			var impEntryS4 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._33);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000009S", impEntryS4.EntryNumber);

			var impEntryF1 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._14);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000010F", impEntryF1.EntryNumber);

			var impEntryF2 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._35);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000011F", impEntryF2.EntryNumber);

			var impEntryF3 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._37);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000012F", impEntryF3.EntryNumber);

			var impEntryH1 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._39, ImportDealingTypeCodeList.Codes._69);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000013H", impEntryH1.EntryNumber);

			var impEntryH2 = CreateEntryForTest(KRJobMessageTypeList.Codes.Import, "", DeclarationProcedureTypeCodeList.Codes._39, ImportDealingTypeCodeList.Codes._11);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000014M", impEntryH2.EntryNumber);

			var expEntry1 = CreateEntryForTest(KRJobMessageTypeList.Codes.Export);
			Factory.Save();
			var expEntryNumber = expEntry1.EntryNumber;
			AssertEquals("entry is in db", true, expEntry1.IsInDatabase);
			AssertEquals("entry number is assigned", false, expEntryNumber.IsEmpty);
			AssertEquals("entry number length is 14", 14, expEntryNumber.Length);
			AssertEquals("entry number is assigned", "6N00221000001X", expEntryNumber);

			var expEntry2 = CreateEntryForTest(KRJobMessageTypeList.Codes.Export);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000002X", expEntry2.EntryNumber);

			JobDeclaration declarationM = Factory.New<JobDeclaration>();
			declarationM.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declarationM.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declarationM.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			CusEntryHeader entryM = declarationM.CustomsEntryHeaders.AddNew();
			entryM.CH_MessageType = KRJobMessageTypeList.Codes.Export;
			Factory.Save();

			var expEntryNumber2 = entryM.EntryNumber;
			AssertEquals("entry is in db", true, entryM.IsInDatabase);
			AssertEquals("entry number is assigned", false, expEntryNumber2.IsEmpty);
			AssertEquals("entry number length is 14", 14, expEntryNumber2.Length);
			AssertEquals("entry number is assigned", "6N00221000003R", expEntryNumber2);

			var lexEntry1 = CreateEntryForTest(KRJobMessageTypeList.Codes.LocalExport, LocalExportTransactionNatureCodeList.Codes._01);
			Factory.Save();
			var lexEntryNumber = lexEntry1.EntryNumber;
			AssertEquals("entry is in db", true, lexEntry1.IsInDatabase);
			AssertEquals("entry number is assigned", false, lexEntryNumber.IsEmpty);
			AssertEquals("entry number length is 13", 13, lexEntryNumber.Length);
			AssertEquals("entry number is assigned", "6N00221000001", lexEntryNumber);

			var lexEntry2 = CreateEntryForTest(KRJobMessageTypeList.Codes.LocalExport, LocalExportTransactionNatureCodeList.Codes._01);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000002", lexEntry2.EntryNumber);

			var lexEntry3 = CreateEntryForTest(KRJobMessageTypeList.Codes.LocalExport, LocalExportTransactionNatureCodeList.Codes._07);
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000003", lexEntry3.EntryNumber);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			var entry008 = declaration.CustomsEntryHeaders.AddNew();
			entry008.CH_MessageType = ElectronicDocumentTypeList.Codes._008;
			Factory.Save();

			var entryNumber008 = entry008.EntryNumber;
			AssertEquals("entry is in db", true, entry008.IsInDatabase);
			AssertEquals("entry number is assigned", false, entryNumber008.IsEmpty);
			AssertEquals("entry number length is 14", 14, entryNumber008.Length);
			AssertEquals("entry number is assigned", "6N00221000001M", entryNumber008);

			var entry0081 = declaration.CustomsEntryHeaders.AddNew();
			entry0081.CH_MessageType = ElectronicDocumentTypeList.Codes._008;
			Factory.Save();
			AssertEquals("entry number is assigned", "6N00221000002M", entry0081.EntryNumber);

			JobDeclaration declaration5SM = Factory.New<JobDeclaration>();
			declaration5SM.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			var entry5SM = declaration5SM.CustomsEntryHeaders.AddNew();
			entry5SM.CH_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			Factory.Save();

			var entryNumber5SM = entry5SM.EntryNumber;
			AssertEquals("entry is in db", true, entry5SM.IsInDatabase);
			AssertEquals("entry number is assigned", "6N002210001U", entryNumber5SM);

			var entry5SM1 = declaration.CustomsEntryHeaders.AddNew();
			entry5SM1.CH_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			Factory.Save();
			AssertEquals("entry number is assigned", "6N002210002U", entry5SM1.EntryNumber);
		}

		[TestDate(2021, 10, 10)]
		public void TestGenerateEntryNumberWhenRegistryEmpty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			Factory.Save();

			var emptyEntryNumber = entry.EntryNumber;
			AssertEquals("entry is in db", true, entry.IsInDatabase);
			AssertEquals("entry number is assigned", true, emptyEntryNumber.IsEmpty);
		}

		public void TestClearEntryNumberWhenSaveFails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			EntryThrowingExceptionAfterOnSaving entry = Factory.New<EntryThrowingExceptionAfterOnSaving>();
			declaration.CustomsEntryHeaders.Add(entry);
			entry.ShouldThrowException = true;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");

			AssertEquals("entry number is empty", true, entry.EntryNumber.IsEmpty);
			try
			{
				Factory.Save();
			}
			catch (Exception) { }

			AssertEquals("entry is not in db", false, entry.IsInDatabase);
			AssertEquals("entry number is removed as save failed", true, entry.EntryNumber.IsEmpty);

			entry.ShouldThrowException = false;
			Factory.Save();
			AssertEquals("entry is in db", true, entry.IsInDatabase);
			AssertEquals("entry number is assigned", false, entry.EntryNumber.IsEmpty);

			var entryNumber = entry.EntryNumber;
			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("entry number is not removed or changed as it was assigned before this transaction", entryNumber, entry.EntryNumber);
		}

		public void TestCargoManagementNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var masterBill = declaration.Bills.AddNew();
			var houseBill = declaration.Bills.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ImportCargoManagementNumber = "01KE0766SS200100003";
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;

			AssertEquals("01KE0766SS200100003", entry.RandomHeader.JZ_ImportCargoManagementNumber);
			AssertEquals("01KE0766SS2-0010-0003", entry.FormattedCargoManagementNo);

			invoice.JZ_CU_RelatedHouseBill = masterBill.PK;
			AssertEquals("01KE0766SS200100003", entry.RandomHeader.JZ_ImportCargoManagementNumber);
			AssertEquals("01KE0766SS2-0010-0003", entry.FormattedCargoManagementNo);
		}

		public void TestFormattedTotalEntryLineCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MergedLines.AddNew();
			AssertEquals("001", entry.FormattedTotalEntryLineCount);

			entry.MergedLines.AddNew();
			AssertEquals("002", entry.FormattedTotalEntryLineCount);
		}

		public void TestSnapshotWhenEntryNumStatusAcceptedOrRejected()
		{
			TestSnapshot(ElectronicDocumentTypeList.Codes._5SC);
			TestSnapshot(ElectronicDocumentTypeList.Codes._DHR);
			TestSnapshot(ElectronicDocumentTypeList.Codes._5BA);
		}

		[TestDate(2021, 10, 10)]
		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1234567890I";

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2021, 10, 10), ((ICurrencyConverterDataProvider)entry).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)entry).RateType);
			AssertNotNull("CurrencyConverter", ((ICurrencyConverterProvider)entry).CurrencyConverter);
			AssertEquals("ICurrencyConverterDataProvider.MaximumDaysToFallback", 0, ((ICurrencyConverterDataProvider)entry).MaximumDaysToFallback);
			AssertEquals("ICurrencyConverterDataProvider.Company", GlbCompany.CurrentCompany.PK, ((ICurrencyConverterDataProvider)entry).Company.PK);
			AssertEquals("ICurrencyConverterDataProvider.LocalCurrencyCodeOverride", GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, ((ICurrencyConverterDataProvider)entry).LocalCurrencyCodeOverride);
			AssertEquals("ICurrencyConverterDataProvider.IsReciprocalOverride", true, ((ICurrencyConverterDataProvider)entry).IsReciprocalOverride);

			var entryNum1 = entry.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 1, 1);

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2022, 1, 1), ((ICurrencyConverterDataProvider)entry).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)entry).RateType);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var entryExport = declaration.CustomsEntryHeaders.AddNew();
			entryExport.CH_MessageType = declaration.JE_MessageType;
			entryExport.EntryNumber = "1234567890E";

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2021, 10, 10), ((ICurrencyConverterDataProvider)entryExport).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.CustomsSecondary, ((ICurrencyConverterDataProvider)entryExport).RateType);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			var entryLocal = declaration.CustomsEntryHeaders.AddNew();
			entryLocal.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entryLocal.EntryNumber = "1234567890L";
			var entryNum3 = entryLocal.CusEntryNumber;
			entryNum3.CE_IssueDate = new ZDateTime(2022, 5, 1);

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2022, 5, 1), ((ICurrencyConverterDataProvider)entryLocal).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.CustomsSecondary, ((ICurrencyConverterDataProvider)entryLocal).RateType);
		}

		[TestDate(2021, 10, 10)]
		public override void TestCurrencyConverter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1234567890I";

			AssertEquals("CurrencyConverter", new ZDateTime(2021, 10, 10), entry.CurrencyConverter.DateForRate);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = declaration.JE_MessageType;
			entry1.EntryNumber = "1234567890I";
			var entryNum1 = entry1.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 1, 1);

			AssertEquals("CurrencyConverter", new ZDateTime(2022, 1, 1), entry1.CurrencyConverter.DateForRate);
		}

		public void TestCustomsValueUSD()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1234567890I";
			var entryNum = entry.CusEntryNumber;
			entryNum.CE_IssueDate = new ZDateTime(2023, 01, 10);

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 100m;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 50m;

			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, new ZDateTime(2023, 01, 01), new ZDateTime(2023, 01, 31), 0.8573m, uSD);

			AssertEquals(175m, entry.CustomsValueUSD);

			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, new ZDateTime(2023, 02, 01), new ZDateTime(2023, 02, 28), 0.0m, uSD);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = declaration.JE_MessageType;
			entry2.EntryNumber = "0987654321I";
			var entryNum2 = entry2.CusEntryNumber;
			entryNum2.CE_IssueDate = new ZDateTime(2023, 02, 10);

			var entryLine3 = entry2.MergedLines.AddNew();
			entryLine3.CL_CustomsValue = 100m;
			var entryLine4 = entry2.MergedLines.AddNew();
			entryLine4.CL_CustomsValue = 50m;

			Factory.Save();

			AssertEquals(0m, entry2.CustomsValueUSD);
		}

		void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();
		}

		public void TestHighestLineNumber()
		{
			AssertHighestLineNumberByType(KRJobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._929);
			AssertHighestLineNumberByType(KRJobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._830);
			AssertHighestLineNumberByType(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP, LocalExportTransactionNatureCodeList.Codes._01);
			AssertHighestLineNumberByType(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DQ, LocalExportTransactionNatureCodeList.Codes._07);
		}

		public void TestSaveHighestSequenceNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Export;

			var container = declaration.CusContainers.AddNew();
			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 1;

			var container2 = declaration.CusContainers.AddNew();
			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_SequenceNumber = 1;

			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_SequenceNumber = 2;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_SequenceNumber = 1;

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest Container Number is 0", (ZShort)0, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 0", (ZShort)0, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine2 is 0", (ZShort)0, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			var genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			var genAddOnColumn2 = GetGenAddOnColumn(Factory, entryLine2.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertNull(genAddOnColumn1);
			AssertNull(genAddOnColumn2);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
			AssertEquals("Highest Container Number is 0", (ZShort)0, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 0", (ZShort)0, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine2 is 0", (ZShort)0, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			genAddOnColumn2 = GetGenAddOnColumn(Factory, entryLine2.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertNull(genAddOnColumn1);
			AssertNull(genAddOnColumn2);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Container Number is 2", (ZShort)2, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 2", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine2 is 1", (ZShort)1, entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			genAddOnColumn2 = GetGenAddOnColumn(Factory, entryLine2.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);
			AssertEquals(entry.MergedLines[1].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn2.XA_Data);

			declaration.CusContainers.Delete(container2);
			entryLine.InvoiceLines.RemoveAndDelete(invoiceLine1);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest Container Number is 2", (ZShort)2, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 2", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
			AssertEquals("Highest Container Number is 2", (ZShort)2, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 2", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Container Number is 1", (ZShort)1, entry.CH_HighestContainerNumber);
			AssertEquals("CL_HighestInvoiceLineSequenceNo can't be decreased.", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);

			container2 = declaration.CusContainers.AddNew();
			containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;

			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_SequenceNumber = 2;
			entryLine.InvoiceLines.Reload(true);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest Container Number is 1", (ZShort)1, entry.CH_HighestContainerNumber);
			AssertEquals("CL_HighestInvoiceLineSequenceNo can't be decreased", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1 = GetGenAddOnColumn(Factory, entryLine.PK, CusEntryLine.GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo);
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
			AssertEquals("Highest Container Number is 1", (ZShort)1, entry.CH_HighestContainerNumber);
			AssertEquals("CL_HighestInvoiceLineSequenceNo can't be decreased", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1.Reload();
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Container Number is 2", (ZShort)2, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 2", (ZShort)2, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1.Reload();
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);

			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_SequenceNumber = 3;
			entryLine.InvoiceLines.Reload(true);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Container Number is 3", (ZShort)2, entry.CH_HighestContainerNumber);
			AssertEquals("Highest InvoiceLineSequenceNumber of entryLine1 is 3", (ZShort)3, entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo);
			genAddOnColumn1.Reload();
			AssertEquals(entry.MergedLines[0].KR_HighestInvoiceLineSequenceNo.ToString(), genAddOnColumn1.XA_Data);
		}

		GenAddOnColumn GetGenAddOnColumn(BusinessObjectFactory factory, ZGuid parentPK, ZString name)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentPK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusEntryLineSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, name);
			return factory.Load<GenAddOnColumn>(query).FirstOrDefault();
		}

		public void TestSaveHighestFTAEntryLineNumber()
		{
			SaveHighestFTAEntryLineNumber(ElectronicDocumentTypeList.Codes._5SC);
			SaveHighestFTAEntryLineNumber(ElectronicDocumentTypeList.Codes._DHR);
		}

		public void TestGetCusEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals(ZDateTime.Empty, entry.AcceptedDate);
			AssertEquals(ZDateTime.Empty, entry.DueDateofLoading);

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "6N00221000025X";
			entryNumber.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_IssueDate = new ZDateTime(2022, 1, 1);
			entryNumber.CE_ExpiryDate = new ZDateTime(2022, 1, 2);

			Factory.Save();

			AssertEquals(new ZDateTime(2022, 1, 1), entry.AcceptedDate);
			AssertEquals(new ZDateTime(2022, 1, 2), entry.DueDateofLoading);
		}

		public void TestGetIndividualStatements()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "AAA111";
			CreateNewStatementHeader(declaration.CompanyPK, entry1.EntryNumber, StatementHeaderTypeList.Codes.CustomsDisbursementBill, "0127020112001320507");
			entry1.Reload();

			var statement1 = entry1.CustomsDisbursementBills[0];
			statement1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
			Factory.Save();
			entry1.Reload();
			AssertEquals(1, entry1.IndividualStatements.Length);

			statement1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			Factory.Save();
			entry1.Reload();
			AssertEquals(1, entry1.IndividualStatements.Length);

			CreateNewStatementHeader(declaration.CompanyPK, entry1.EntryNumber, StatementHeaderTypeList.Codes.CustomsDisbursementBill, "0127020112001320508");
			var statement2 = entry1.CustomsDisbursementBills[1];
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
			Factory.Save();
			entry1.Reload();
			AssertEquals(2, entry1.IndividualStatements.Length);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "BBB111";
			CreateNewStatementHeader(declaration.CompanyPK, entry2.EntryNumber, StatementHeaderTypeList.Codes.Invoice, "0127020112001320509");
			entry2.Reload();

			var statement3 = entry2.CustomsDisbursementBills[0];
			statement3.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
			Factory.Save();
			entry2.Reload();
			AssertEquals(0, entry2.IndividualStatements.Length);

			statement3.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			Factory.Save();
			entry2.Reload();
			AssertEquals(0, entry2.IndividualStatements.Length);
		}

		void CreateNewStatementHeader(ZGuid companyPK, ZString entryNumber, ZString statementType, ZString statementNumber)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = companyPK;
			statement.B2_StatementType = statementType;
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			statement.B2_StatementNumber = statementNumber;
			statement.B2_StatementAmount = 1000000m;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_SequenceNumber = 1;

			var chargeDTY = statementLine.Charges.AddNew();
			chargeDTY.B4_ChargeType = ChargeTypeList.Codes.Duty;
			chargeDTY.B4_ChargeAmount = 1000000m;
			Factory.Save();
		}

		public void TestIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("FOB", entry.Incoterm);
		}

		public void TestTotalInvoiceAmountAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = KRJobMessageTypeList.Codes.Export;
			var entryLine1 = entry1.MergedLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.InvoiceLines.AddNew().JI_CL = entryLine1.PK;
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 150m;
			invoice2.InvoiceLines.AddNew().JI_CL = entryLine1.PK;
			invoice2.JZ_RX_NKInvoice_Currency = "USD";

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = KRJobMessageTypeList.Codes.Export;
			var entryLine2 = entry2.MergedLines.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceAmount = 10m;
			invoice3.InvoiceLines.AddNew().JI_CL = entryLine2.PK;
			invoice3.JZ_RX_NKInvoice_Currency = "KRW";
			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_InvoiceAmount = 180m;
			invoice4.InvoiceLines.AddNew().JI_CL = entryLine2.PK;
			invoice4.JZ_RX_NKInvoice_Currency = "KRW";

			AssertEquals(250.00m, entry1.TotalInvoiceAmount);
			AssertEquals("USD", entry1.InvoiceAmountCurrency);

			AssertEquals(190.00m, entry2.TotalInvoiceAmount);
			AssertEquals("KRW", entry2.InvoiceAmountCurrency);
		}

		public void TestTotalDeductedAdditionalAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Has Attribute InvoiceQuantity in CU1");
			helper.CreateTariffAttribute(Enterprise.Customs.KR.Messaging.Constants.ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208122222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Has Not Attribute");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CRF";
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1.ZZ1_TariffCode;
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_LinePrice = 6000000m;
			var a104Charge = invoiceLine1.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A104, 100000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			a104Charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			a104Charge.J7_IsDutiable = true;

			var a118Charge = invoiceLine1.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A118, 50000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			a118Charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			a118Charge.J7_IsDutiable = false;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff2.ZZ1_TariffCode;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_LinePrice = 4000000m;
			var a115Charge = invoiceLine2.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A115, 100000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			a115Charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			a115Charge.J7_IsDutiable = true;
			var a119Charge = invoiceLine2.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A119, 50000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			a119Charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			a119Charge.J7_IsDutiable = false;
			declaration.ResumeApportionment();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var result = new ImportEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount && !J7_IsIncludedInITOT => (50000 + 50000) + JI_LinePrice(6000000 + 4000000)", 10100000m, result.TotalInvoiceAmount);
			AssertEquals("!J7_Calc_IsIncludedInInvoiceAmount && J7_IsDutiable => 100000 + 100000", 200000.00m, result.AdditionalAmount);
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount && !J7_IsDutiable => 50000 + 50000", 100000.00m, result.DeductedAmount);
		}

		public void TestFreightAndInsurance()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			entry.CH_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 100.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 2000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 200.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 10.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 1.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 20.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 2.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			AssertEquals(1111m, entry.Freight);
			AssertEquals(2222m, entry.Insurance);

			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0m, entry.Freight);
			AssertEquals(0m, entry.Insurance);

			invoiceLine.Charges.AddNew(ImportChargeMethodCodeList.Codes.A114, 1000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.A114, 100.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(ImportChargeMethodCodeList.Codes.A116, 2000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.A116, 200.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(ImportChargeMethodCodeList.Codes.B311, 3000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B311, 300.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(ImportChargeMethodCodeList.Codes.B313, 4000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B313, 400.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(ImportChargeMethodCodeList.Codes.B501, 5000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B501, 500.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(ImportChargeMethodCodeList.Codes.B503, 6000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B503, 600.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			invoiceLine2.Charges.AddNew(ImportChargeMethodCodeList.Codes.A114, 10.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.A114, 1.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(ImportChargeMethodCodeList.Codes.A116, 20.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.A116, 2.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(ImportChargeMethodCodeList.Codes.B311, 30.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B311, 3.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(ImportChargeMethodCodeList.Codes.B313, 40.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B313, 4.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(ImportChargeMethodCodeList.Codes.B501, 50.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B501, 5.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(ImportChargeMethodCodeList.Codes.B503, 60.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(ImportChargeMethodCodeList.Codes.B503, 6.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			AssertEquals(1111m, entry.Freight);
			AssertEquals(2222m, entry.Insurance);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			AssertEquals(3333m, entry.Freight);
			AssertEquals(4444m, entry.Insurance);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			AssertEquals(5555m, entry.Freight);
			AssertEquals(6666m, entry.Insurance);
		}

		public void TestTotalGrossWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_Weight = 100m;
			invoice1.InvoiceLines.AddNew().JI_CL = entryLine.PK;
			invoice1.JZ_WeightUQ = "KG";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_Weight = 1511m;
			invoice2.InvoiceLines.AddNew().JI_CL = entryLine.PK;
			invoice2.JZ_WeightUQ = "G";
			AssertEquals("101.511 => 101.511", 101.511m, entry.TotalGrossWeightInKG);
		}

		public void TestTotalPackagesAndUQ()
		{
			var exportEntry = CreateData(KRJobMessageTypeList.Codes.Export, "BA", 125m);
			AssertEquals("If MessageType is not IMP, TotalPackages value is Sum(JZ_NoOfPacks:125 + 125)", 250m, exportEntry.TotalPackages);
			AssertEquals("BA", exportEntry.PackagesUQ);

			var importEntry = CreateData(KRJobMessageTypeList.Codes.Import, "BA", 125m);
			AssertEquals("If MessageType is IMP, TotalPackages value is CEI_PackQty:125", 125m, importEntry.TotalPackages);
			AssertEquals("BA", importEntry.PackagesUQ);

			CusEntryHeader CreateData(string messageType, string packType, decimal packQTY)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.JE_TotalNoOfPacksPackType = packType;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_PackQty = (ZInt)packQTY;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = instruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_NoOfPacks = packQTY;
				invoice1.InvoiceLines.AddNew().JI_CL = entryLine.PK;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_NoOfPacks = packQTY;
				invoice2.InvoiceLines.AddNew().JI_CL = entryLine.PK;

				return entry;
			}
		}

		public void TestResponsibleCustomsOfficer()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(ZString.Empty, entry.ResponsibleCustomsOfficer);

			entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "001", "FirstOfficerName", new ZDateTime("2022-01-01"));
			entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "002", "SecondOfficerName", new ZDateTime("2022-01-02"));
			entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.InspectionCustomsOfficer, "003", "ThirdOfficerName", new ZDateTime("2022-01-03"));
			AssertEquals("002-SecondOfficerName", entry.ResponsibleCustomsOfficer);
		}

		public void TestFormattedRefNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			AssertEquals("", entry.FormattedRefNumber);

			entry.CH_BGMReference = "1234";
			AssertEquals("1234", entry.FormattedRefNumber);

			entry.CH_BGMReference = "12345678901234";
			AssertEquals("Formats only if the length of the value is 14.", "123-45-67-890123-4", entry.FormattedRefNumber);

			entry.CH_BGMReference = "123456789012";
			AssertEquals("Formats only if the length of the value is 14.", "123456789012", entry.FormattedRefNumber);

			entry.CH_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertEquals("123-45-67-89012", entry.FormattedRefNumber);
		}

		public void TestCustomsReviewDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(ZDateTime.Empty, entry.MostRecentCustomsReviewDate);

			var log1 = entry.Logs.AddNew(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTime.Today.ToOffset(), reference: entry.CH_EntryStatus));
			Factory.Save();

			var log2 = entry.Logs.AddNew(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTime.Today.AddDays(1).ToOffset(), reference: entry.CH_EntryStatus));
			Factory.Save();

			AssertEquals("Values are taken from the last log.", ZDateTime.Today.AddDays(1), entry.MostRecentCustomsReviewDate);
		}

		public override void TestTotalTAndI()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

				var (overseasFreightChargeCode, overseasInsuranceChargeCode, notIncludedChargeCode) = GetChargeCodesForTotalTAndI();

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 1000.00m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

				var freight = invoiceHeader.Charges.AddNew();
				freight.J7_ChargeType = overseasFreightChargeCode;
				freight.J7_Amount = 120.00m;
				freight.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

				var insurance = invoiceHeader.Charges.AddNew();
				insurance.J7_ChargeType = overseasInsuranceChargeCode;
				insurance.J7_Amount = 30.00m;
				insurance.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

				var packing = invoiceHeader.Charges.AddNew();
				packing.J7_ChargeType = notIncludedChargeCode;
				packing.J7_Amount = 70.00m;
				packing.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

				var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				invoiceLine1.JI_LinePrice = 500.00m;
				invoiceLine1.JI_Weight = 1;
				invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_LinePrice = 500.00m;
				invoiceLine2.JI_Weight = 1;
				invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

				DoMerge(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
					AssertEquals("TotalTAndI.Amount", 150.00m, declaration.CustomsEntryHeaders[0].TotalTAndI.Amount);
				});
			}
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public override void TestFOBAndCIFFigures()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var setup = GetChargesCurrencyTestSetup();
				setup.SetupJobDecWithOFTAndCIFCharges(declaration, declaration.LocalCurrencyCode);
				DoMerge(declaration);

				CombineAssertions(() =>
				{
					AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
					var entryHeader = declaration.CustomsEntryHeaders[0];
					AssertEquals("Overseas Freight for the entry does not include ForeignInlandFreight", 500m, entryHeader.OverseasFreight.Amount);
					AssertEquals("FOB for the entry", setup.ExpectedFOB, entryHeader.FOB.Amount);
					AssertEquals("CIF for the entry", setup.ExpectedCIF, entryHeader.CIF.Amount);
				});
			}
		}

		[TestDate(2007, 12, 12, 12, 12, 12)]
		public override void TestFOBAndCIFInLocalCurrency()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var newCurrency = RefCurrency.New(Factory);
				newCurrency.RX_Code = "MDD";
				var from = new ZDateTime(2007, 6, 1);
				var to = new ZDateTime(2007, 12, 30);
				newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m).RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var setup = GetChargesCurrencyTestSetup();
				setup.SetupJobDecWithOFTAndCIFCharges(declaration, newCurrency.RX_Code);
				DoMerge(declaration);

				CombineAssertions(() =>
				{
					AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
					var entryHeader = declaration.CustomsEntryHeaders[0];
					AssertEquals("FOB in local currency", setup.ExpectedFOB * 2, entryHeader.FOBInLocalCurrency.Amount);
					AssertEquals("CIF in local currency", setup.ExpectedCIF * 2, entryHeader.CIFInLocalCurrency.Amount);
				});
			}
		}

		public void TestImportChargeMethodCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			entry.CH_MessageType = KRJobMessageTypeList.Codes.Import;
			var charge104 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A104, 10m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge104.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge104.J7_IsDutiable = true;
			var charge105 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A105, 11m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge105.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge105.J7_IsDutiable = true;
			var charge106 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A106, 12m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge106.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge106.J7_IsDutiable = true;
			var charge107 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A107, 13m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge107.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge107.J7_IsDutiable = true;
			var charge108 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A108, 14m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge108.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge108.J7_IsDutiable = true;
			var charge109 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A109, 15m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge109.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge109.J7_IsDutiable = true;
			var charge110 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A110, 16m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge110.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge110.J7_IsDutiable = true;
			var charge111 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A111, 17m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge111.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge111.J7_IsDutiable = true;
			var charge112 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A112, 18m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge112.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge112.J7_IsDutiable = true;
			invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A114, 19m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			var charge115 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A115, 20m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge115.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge115.J7_IsDutiable = true;
			invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A116, 21m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			var charge118 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A118, 22m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge118.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge118.J7_IsDutiable = false;
			var charge119 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A119, 23m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge119.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge119.J7_IsDutiable = false;
			var charge120 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A120, 24m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge120.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge120.J7_IsDutiable = false;
			var charge121 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A121, 25m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge121.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge121.J7_IsDutiable = false;

			var charge303 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B303, 26m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge303.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge303.J7_IsDutiable = false;
			var charge304 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B304, 27m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge304.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge304.J7_IsDutiable = false;
			var charge305 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B305, 28m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge305.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge305.J7_IsDutiable = false;
			var charge306 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B306, 29m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge306.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge306.J7_IsDutiable = false;
			var charge307 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B307, 30m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge307.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge307.J7_IsDutiable = false;
			var charge309 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B309, 31m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge309.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge309.J7_IsDutiable = true;
			var charge310 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B310, 32m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge310.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge310.J7_IsDutiable = true;
			var charge311 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B311, 33m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge311.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge311.J7_IsDutiable = true;
			var charge312 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B312, 34m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge312.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge312.J7_IsDutiable = true;
			var charge313 = invoiceLine.Charges.AddNew(ImportChargeMethodTwoAndThreeCodeList.Codes.B313, 35m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge313.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge313.J7_IsDutiable = true;

			var charge404 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B404, 36m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge404.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge404.J7_IsDutiable = false;
			var charge405 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B405, 37m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge405.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge405.J7_IsDutiable = false;
			var charge406 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B406, 38m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge406.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge406.J7_IsDutiable = false;
			var charge407 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B407, 39m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge407.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge407.J7_IsDutiable = false;
			var charge408 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B408, 40m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge408.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge408.J7_IsDutiable = false;
			var charge409 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B409, 41m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge409.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge409.J7_IsDutiable = false;
			var charge410 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B410, 42m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge410.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge410.J7_IsDutiable = false;
			var charge411 = invoiceLine.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B411, 43m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge411.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge411.J7_IsDutiable = false;

			invoiceLine.Charges.AddNew(ImportChargeMethodFiveAndSixCodeList.Codes.B501, 44m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			var charge502 = invoiceLine.Charges.AddNew(ImportChargeMethodFiveAndSixCodeList.Codes.B502, 45m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge502.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge502.J7_IsDutiable = true;
			invoiceLine.Charges.AddNew(ImportChargeMethodFiveAndSixCodeList.Codes.B503, 46m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			Factory.Save();

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			AssertCharges(entry, 19m, 21m, 146m, 94m);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
			AssertCharges(entry, 33m, 35m, 97m, 140m);
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
			AssertCharges(entry, 33m, 35m, 97m, 140m);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			AssertCharges(entry, 0m, 0m, 0m, 316m);
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
			AssertCharges(entry, 0m, 0m, 0m, 316m);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
			AssertCharges(entry, 44m, 46m, 45m, 0m);
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodSix;
			AssertCharges(entry, 44m, 46m, 45m, 0m);
		}

		void AssertCharges(CusEntryHeader entry, decimal freight, decimal insurance, decimal additionalAmount, decimal deductedAmount)
		{
			AssertEquals(freight, entry.Freight);
			AssertEquals(insurance, entry.Insurance);
			AssertEquals(additionalAmount, entry.AdditionalAmount);
			AssertEquals(deductedAmount, entry.DeductedAmount);
		}

		public void TestVatTaxesAndPenalties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			CreateChargesData(ChargeTypeList.Codes.Duty, 2000m);
			CreateChargesData(ChargeTypeList.Codes.SpecialConsumptionTax, 4000m);
			CreateChargesData(ChargeTypeList.Codes.TransportationTax, 6000m);
			CreateChargesData(ChargeTypeList.Codes.LiquorTax, 8000m);
			CreateChargesData(ChargeTypeList.Codes.EducationTax, 10000m);
			CreateChargesData(ChargeTypeList.Codes.AgricultureTax, 12000m);
			CreateChargesData(ChargeTypeList.Codes.VAT, 14000m);
			CreateChargesData(ChargeTypeList.Codes.PenaltyForLateDeclaration, 16000m);
			CreateChargesData(ChargeTypeList.Codes.PenaltyForMissedDeclaration, 18000m);

			CreateChargesData(ChargeTypeList.Codes.Duty, 200000m);
			CreateChargesData(ChargeTypeList.Codes.SpecialConsumptionTax, 400000m);
			CreateChargesData(ChargeTypeList.Codes.TransportationTax, 600000m);
			CreateChargesData(ChargeTypeList.Codes.LiquorTax, 800000m);
			CreateChargesData(ChargeTypeList.Codes.EducationTax, 1000000m);
			CreateChargesData(ChargeTypeList.Codes.AgricultureTax, 1200000m);
			CreateChargesData(ChargeTypeList.Codes.VAT, 1400000m);
			CreateChargesData(ChargeTypeList.Codes.PenaltyForLateDeclaration, 1600000m);
			CreateChargesData(ChargeTypeList.Codes.PenaltyForMissedDeclaration, 1800000m);

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_ValueForVAT = 1000m;
			entryLine1.CL_ValueExemptForVAT = 1500m;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_ValueForVAT = 1000m;
			entryLine2.CL_ValueExemptForVAT = 1500m;

			AssertEquals(202000m, entry.FormattedTotalDutyAmount);
			AssertEquals(404000m, entry.TotalSpecialConsumptionTax);
			AssertEquals(606000m, entry.TotalTransportationTax);
			AssertEquals(808000m, entry.TotalLiquorTax);
			AssertEquals(1010000m, entry.TotalEducationTax);
			AssertEquals(1212000m, entry.TotalAgricultureTax);
			AssertEquals(1414000m, entry.TotalVAT);
			AssertEquals(5656000m, entry.TotalAmountPayable);
			AssertEquals(2000m, entry.TotalValueForVAT);
			AssertEquals(3000m, entry.TotalVATExemptionValue);
			AssertEquals(1616000m, entry.PenaltyForLateDeclaration);
			AssertEquals(1818000m, entry.PenaltyForMissedDeclaration);

			void CreateChargesData(string chargeType, decimal chargeAmount)
			{
				var charges = entry.Charges.AddNew();
				charges.C1_ChargeAmount = chargeAmount;
				charges.C1_ChargeType = chargeType;
			}
		}

		public override BaseJobDeclaration GetNewBaseJobDeclaration()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			return base.GetNewBaseJobDeclaration();
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;
				invoiceLine.JI_CEI = instruction.PK;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 9800m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10300m;
		}

		[TestDate(2023, 12, 31)]
		public void TestLoadCusEntryNumberLEX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var entry = Factory.New<CusEntryHeaderForTest>();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.CH_JE = declaration.PK;
			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum1.CE_EntryLineReference = "1";
			entryNum1.CE_EntryNum = "6N00223000001";
			entryNum1.CE_IssueDate = new ZDateTime("2023-11-03 00:00:00");
			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum2.CE_EntryLineReference = "2";
			entryNum2.CE_EntryNum = "6N00223000002";
			entryNum2.CE_IssueDate = new ZDateTime("2023-11-04 00:00:00");
			var entryNum3 = entry.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum3.CE_EntryLineReference = "3";
			entryNum3.CE_EntryNum = "6N00223000003";
			entryNum3.CE_IssueDate = new ZDateTime("2023-11-05 00:00:00");
			Factory.Save();
			AssertEquals("CusEntryNumber has the lowest CE_EntryLineReference is returned", "1", entry.LoadCusEntryNumber().CE_EntryLineReference);

			var df3EntryNum = entry.EntryNumbers.AddNew();
			df3EntryNum.CE_EntryNum = "1234520000045M";
			df3EntryNum.CE_EntryType = "DF3";
			Factory.Save();

			var currentEntryNum = entry.LoadCusEntryNumber();
			AssertEquals(entryNum1.CE_EntryNum, currentEntryNum.CE_EntryNum);
			AssertEquals(entryNum1.CE_IssueDate, currentEntryNum.CE_IssueDate);
		}

		public void TestCreateCusEntryNumberLEX()
		{
			var entry = Factory.New<CusEntryHeaderForTest>();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.CH_VersionID = 3;
			AssertEquals("CE_EntryLineReference of the entry number created is CH_VersionID + 1", "4", entry.CreateCusEntryNumber().CE_EntryLineReference);
			entry.CH_VersionID++;
			AssertEquals("CE_EntryLineReference of the entry number created is CH_VersionID + 1", "5", entry.CreateCusEntryNumber().CE_EntryLineReference);
		}

		[TestDate(2023, 12, 31)]
		public void TestGetEntryNumberForLEXMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			var entry = Factory.New<CusEntryHeaderForTest>();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.CH_VersionID = 1;
			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum1.CE_EntryLineReference = "1";
			entryNum1.CE_EntryNum = "11111";
			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum2.CE_EntryLineReference = "2";
			entryNum2.CE_EntryNum = "22222";
			AssertEquals("Existing CusEntryNumber with CE_EntryLineReference = CH_VersionID + 1 is returned.", "22222", entry.GetEntryNumberForLEXMessage());

			entry.CH_VersionID++;
			AssertEquals("CusEntryNumber is generated", "6N00223000001", entry.GetEntryNumberForLEXMessage());
		}

		class CusEntryHeaderForTest : CusEntryHeader
		{
			public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			new public CusEntryNumber LoadCusEntryNumber() => base.LoadCusEntryNumber();
			new public CusEntryNumber CreateCusEntryNumber() => base.CreateCusEntryNumber();
			new public ZString GetEntryNumberForLEXMessage() => base.GetEntryNumberForLEXMessage();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			result.PivotsToContainers.DeleteAll();
			result.PivotsToContainers.GetOrCreatePivotFor(result.Declaration.CusContainers.AddNew());
			return result;
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = base.ImportJobDeclaration;
				KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(result.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
				return result;
			}
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override bool RatesAreReciprocal => true;

		void TestSnapshot(string electronicDocumentType)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum5SC = entry.EntryNumbers.AddNew();
			entryNum5SC.CE_EntryType = electronicDocumentType;

			var snapshot1 = entry.Snapshots.AddNew();
			snapshot1.CES_MessageType = electronicDocumentType;
			snapshot1.CES_Status = EntrySnapshotStatus.Current;
			snapshot1.CES_SystemCreateTimeUtc = ZDateTime.Today;
			entryNum5SC.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals("Entry has 1 snapshots", 1, entry.Snapshots.Count);
			var latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(electronicDocumentType, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);

			snapshot1 = entry.Snapshots.AddNew();
			snapshot1.CES_MessageType = electronicDocumentType;
			snapshot1.CES_Status = EntrySnapshotStatus.Current;
			snapshot1.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
			entryNum5SC.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;

			AssertEquals("Entry has 2 snapshots", 2, entry.Snapshots.Count);
			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(electronicDocumentType, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			snapshot1 = entry.Snapshots.AddNew();
			snapshot1.CES_MessageType = electronicDocumentType;
			snapshot1.CES_Status = EntrySnapshotStatus.Current;
			snapshot1.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(2);
			AssertEquals("Entry has 3 snapshots", 3, entry.Snapshots.Count);
			entryNum5SC.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			AssertEquals("Entry has 2 snapshots", 2, entry.Snapshots.Count);
			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(electronicDocumentType, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			snapshot1 = entry.Snapshots.AddNew();
			snapshot1.CES_MessageType = electronicDocumentType;
			snapshot1.CES_Status = EntrySnapshotStatus.Current;
			snapshot1.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(3);
			AssertEquals("Entry has 3 snapshots", 3, entry.Snapshots.Count);
			entryNum5SC.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentRejected;

			AssertEquals("Entry has 2 snapshots", 2, entry.Snapshots.Count);
			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(electronicDocumentType, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);
		}

		void SaveHighestFTAEntryLineNumber(ZString entryType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_PrimaryPreference = "FEU1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_PrimaryPreference = "FEU1";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = "12345";
			var entryNum = entry.CusEntryNumber;
			entryNum.CE_EntryType = entryType;

			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_FTASequenceNumber = 1;

			var entryLine2 = entry.MergedLines[1];
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_FTASequenceNumber = 2;

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest FTASequenceNumber", (ZShort)0, entry.CH_HighestFTASequenceNumber);

			CreateSnapshot(entry, entryType);
			AssertEquals("Highest FTASequenceNumber", (ZShort)0, entry.CH_HighestFTASequenceNumber);

			invoice.InvoiceLines.Remove(invoiceLine2);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest FTASequenceNumber", (ZShort)2, entry.CH_HighestFTASequenceNumber);

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest FTASequenceNumber", (ZShort)2, entry.CH_HighestFTASequenceNumber);

			CreateSnapshot(entry, entryType);
			AssertEquals("Highest FTASequenceNumber", (ZShort)2, entry.CH_HighestFTASequenceNumber);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest FTASequenceNumber", (ZShort)1, entry.CH_HighestFTASequenceNumber);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine3 = entry.MergedLines[1];
			entryLine3.CL_LineNumber = 2;
			entryLine3.CL_FTASequenceNumber = 2;

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest FTASequenceNumber", (ZShort)1, entry.CH_HighestFTASequenceNumber);

			CreateSnapshot(entry, entryType);
			AssertEquals("Highest FTASequenceNumber", (ZShort)1, entry.CH_HighestFTASequenceNumber);
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest FTASequenceNumber", (ZShort)2, entry.CH_HighestFTASequenceNumber);
		}

		void CreateSnapshot(CusEntryHeader entry, ZString messageType)
		{
			if (messageType == ElectronicDocumentTypeList.Codes._5SC)
			{
				var header5AC = new ImportFTACreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header5AC))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._DHR)
			{
				var headerDHR = new ImportDHRCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(headerDHR))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._830)
			{
				var header = new ExportEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				var header = new LocalExport5DPEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._5DQ)
			{
				var header = new LocalExport5DQEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._929)
			{
				var header = new ImportEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._DF3)
			{
				var header = new LocalExportAmendDF3Creator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
		}

		void AssertHighestLineNumberByType(string declarationType, string messageType, string messageSubType = null)
		{
			var entry = CreateEntryForTest(declarationType, messageSubType);
			AssertEquals("Highest Line Number is 0", (ZShort)0, entry.CH_HighestLineNumber);

			CreateSnapshot(entry, messageType);
			AssertEquals("Highest Line Number is 0", (ZShort)0, entry.CH_HighestLineNumber);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Line Number is 2", (ZShort)2, entry.CH_HighestLineNumber);

			entry.MergedLines.RemoveAt(1);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest Line Number is 2", (ZShort)2, entry.CH_HighestLineNumber);

			CreateSnapshot(entry, messageType);
			AssertEquals("Highest Line Number is 2", (ZShort)2, entry.CH_HighestLineNumber);

			entry.CH_Status = "";
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Line Number is 1", (ZShort)1, entry.CH_HighestLineNumber);

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			var invoiceLine3 = entry.Declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("Highest Line Number is 1", (ZShort)1, entry.CH_HighestLineNumber);

			CreateSnapshot(entry, messageType);
			AssertEquals("Highest Line Number is 1", (ZShort)1, entry.CH_HighestLineNumber);

			entry.CH_Status = "";
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals("Highest Line Number is 4", (ZShort)4, entry.CH_HighestLineNumber);
		}

		public void TestMessageTypeCheckedColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			entry.CH_MessageType = KRJobMessageTypeList.Codes.Export;

			AssertEquals(false, entry.IsImport);
			AssertEquals(true, entry.IsExport);
			AssertEquals(false, entry.IsLocalExport);
			AssertEquals(false, entry.IsMisc);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			entry.CH_MessageType = KRJobMessageTypeList.Codes.Import;

			AssertEquals(true, entry.IsImport);
			AssertEquals(false, entry.IsExport);
			AssertEquals(false, entry.IsLocalExport);
			AssertEquals(false, entry.IsMisc);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;

			AssertEquals(false, entry.IsImport);
			AssertEquals(false, entry.IsExport);
			AssertEquals(true, entry.IsLocalExport);
			AssertEquals(false, entry.IsMisc);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DQ;

			AssertEquals(false, entry.IsImport);
			AssertEquals(false, entry.IsExport);
			AssertEquals(true, entry.IsLocalExport);
			AssertEquals(false, entry.IsMisc);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._008;

			AssertEquals(true, entry.IsImport);
			AssertEquals(false, entry.IsExport);
			AssertEquals(false, entry.IsLocalExport);
			AssertEquals(true, entry.IsMisc);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._D87;

			AssertEquals(true, entry.IsImport);
			AssertEquals(false, entry.IsExport);
			AssertEquals(false, entry.IsLocalExport);
			AssertEquals(true, entry.IsMisc);
		}

		CusEntryHeader CreateEntryForTest(string declarationType, string messageSubType = null, string procedureType = null, string tradeType = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = declarationType;
			if (messageSubType != null)
			{
				declaration.JE_MessageSubType = messageSubType;
			}
			if (procedureType != null)
			{
				declaration.JE_ProcedureType = procedureType;
			}
			if (tradeType != null)
			{
				declaration.JE_TradeType = tradeType;
			}
			var invoice = declaration.Invoices.AddNew();

			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration.CustomsEntryHeaders[0];
		}

		public void TestCH_HighestTransportMeansNo5DQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			declaration.TransportMeans.AddNew().CY_Order = 1;
			declaration.TransportMeans.AddNew().CY_Order = 2;
			declaration.TransportMeans.AddNew().CY_Order = 3;
			AssertEquals(3u, declaration.TransportMeans.Cast<TransportMeans>().Max(x => x.CY_Order));
			AssertEquals(0u, entry.CH_HighestTransportMeansNo);

			CreateLocalExportSnapshot(entry, entry.CH_MessageType);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(3u, declaration.TransportMeans.Cast<TransportMeans>().Max(x => x.CY_Order));
			AssertEquals(3u, entry.CH_HighestTransportMeansNo);

			declaration.TransportMeans.AddNew().CY_Order = 4;
			CreateLocalExportSnapshot(entry, entry.CH_MessageType);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(4u, declaration.TransportMeans.Cast<TransportMeans>().Max(x => x.CY_Order));
			AssertEquals(4u, entry.CH_HighestTransportMeansNo);
		}
		void CreateLocalExportSnapshot(CusEntryHeader entry, string messageType)
		{
			LocalExportEntryHeader header = null;
			if (messageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				header = new LocalExport5DPEntryHeaderCreator().Create(entry);
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._5DQ)
			{
				header = new LocalExport5DQEntryHeaderCreator().Create(entry);
			}
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				Factory.Save();
			}
		}

		public void TestGetOriginalFTAType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "CN", "중국", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "ID", "인도네시아", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "IN", "인도", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "VN", "베트남", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			AssertWhenNoSnapshot(Core.Constants.CountryCodes.China, true, ElectronicDocumentTypeList.Codes._DHR);
			AssertWhenNoSnapshot(Core.Constants.CountryCodes.UnitedStates, false, ElectronicDocumentTypeList.Codes._5SC);
			AssertWhenNoSnapshot(Core.Constants.CountryCodes.India, true, ElectronicDocumentTypeList.Codes._DHR);

			var snapShot = entry.Snapshots.AddNew();
			snapShot.CES_MessageType = ElectronicDocumentTypeList.Codes._DHR;
			snapShot.CES_Status = EntrySnapshotStatus.Deleted;
			AssertHasInValidSnapShot(Core.Constants.CountryCodes.China, true, ElectronicDocumentTypeList.Codes._DHR);
			snapShot.CES_MessageType = ElectronicDocumentTypeList.Codes._5SC;
			snapShot.CES_Status = EntrySnapshotStatus.Deleted;
			AssertHasInValidSnapShot(Core.Constants.CountryCodes.UnitedStates, false, ElectronicDocumentTypeList.Codes._5SC);

			snapShot.CES_MessageType = ElectronicDocumentTypeList.Codes._DHR;
			snapShot.CES_Status = EntrySnapshotStatus.Lodged;
			AssertHasValidSnapShot();
			snapShot.CES_MessageType = ElectronicDocumentTypeList.Codes._5SC;
			AssertHasValidSnapShot();

			snapShot.CES_MessageType = ElectronicDocumentTypeList.Codes._DHR;
			snapShot.CES_Status = EntrySnapshotStatus.Current;
			AssertHasValidSnapShot();
			snapShot.CES_MessageType = ElectronicDocumentTypeList.Codes._5SC;
			AssertHasValidSnapShot();

			void AssertWhenNoSnapshot(string countryOfOrigin, bool isDHR, string originalFTAType)
			{
				invoiceLine2.JI_CountryOfOrigin = countryOfOrigin;
				AssertEquals(0, entry.Snapshots.Count);
				AssertEquals(originalFTAType, entry.GetOriginalFTAType());
			}

			void AssertHasInValidSnapShot(string countryOfOrigin, bool isDHR, string originalFTAType)
			{
				invoiceLine2.JI_CountryOfOrigin = countryOfOrigin;
				AssertEquals(1, entry.Snapshots.Count);
				AssertEquals(snapShot.CES_Status, EntrySnapshotStatus.Deleted);
				AssertEquals(originalFTAType, entry.GetOriginalFTAType());
			}

			void AssertHasValidSnapShot()
			{
				invoiceLine2.JI_CountryOfOrigin = ZString.Empty;
				AssertEquals(1, entry.Snapshots.Count);
				AssertNotEquals(snapShot.CES_Status, EntrySnapshotStatus.Deleted);
				AssertEquals(snapShot.CES_MessageType, entry.GetOriginalFTAType());
			}
		}

		public void TestExceptionForCE_EntryStatus()
		{
			var entry = Factory.New<CusEntryHeader>();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryStatus = ZString.Empty;
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			entryNum.CE_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
			AssertEquals("EntryStatus to be set 'ANT'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2024, 05, 08)]
		public void TestRevertingRefundNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum.CE_EntryLineReference = "1";

			entry.GetRefundNumber("1");
			entry.OnSaved(false);
			AssertEquals(ZString.Empty, entryNum.CE_EntryNum);

			entry.GetRefundNumber("1");
			entry.OnSaved(true);
			AssertEquals("6N0022400002U", entryNum.CE_EntryNum);
		}

		public void TestGetLatestSnapshot()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var expHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertGetLatestSnapshot(entry, expHeader, ElectronicDocumentTypeList.Codes._830);

			var impHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertGetLatestSnapshot(entry, impHeader, ElectronicDocumentTypeList.Codes._929);

			var lexHeader_5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			AssertGetLatestSnapshot(entry, lexHeader_5DP, ElectronicDocumentTypeList.Codes._5DP);

			var lexHeader_5DQ = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertGetLatestSnapshot(entry, lexHeader_5DQ, ElectronicDocumentTypeList.Codes._5DQ);
		}

		void AssertGetLatestSnapshot<T>(CusEntryHeader entry, T xmlObject, ZString messageType)
		{
			using (var stream = KRXmlObjectSerializer.Serialize(xmlObject))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, messageType);
				Factory.Save();
			}

			using (var stream = KRXmlObjectSerializer.Serialize(xmlObject))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				entry.Snapshots[1].CES_Status = EntrySnapshotStatus.Deleted;
				Factory.Save();
			}

			using (var stream = KRXmlObjectSerializer.Serialize(xmlObject))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5UA, stream);
				Factory.Save();
			}

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			CusEntrySnapshot snapshot1 = entry.Snapshots[0];
			CusEntrySnapshot snapshot2 = entry.Snapshots[1];

			entry.Snapshots.Load();

			var getLatestSnapshot = typeof(CusEntryHeaderExtensionMethods).GetMethod("GetLatestSnapshot", BindingFlags.NonPublic | BindingFlags.Static);
			var snapshot = (CusEntrySnapshot)getLatestSnapshot.Invoke(null, new object[] { entry, new string[] { messageType } });
			AssertEquals(EntrySnapshotStatus.Lodged, snapshot.CES_Status);
			AssertEquals(snapshot1, snapshot);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
			Factory.Save();

			snapshot = (CusEntrySnapshot)getLatestSnapshot.Invoke(null, new object[] { entry, new string[] { messageType } });
			AssertEquals(EntrySnapshotStatus.Deleted, snapshot.CES_Status);
			AssertEquals(snapshot2, snapshot);
		}

		public void TestKR_ImmediateDeliveryNo()
		{
			var entry = GetCusEntryHeader();
			var entryline = entry.MergedLines[0];

			entryline.ImmediateDeliveries.AddNew().CY_Order = 1;
			entryline.ImmediateDeliveries.AddNew().CY_Order = 2;
			entryline.ImmediateDeliveries.AddNew().CY_Order = 3;
			AssertEquals(3u, entryline.ImmediateDeliveries.Cast<ImmediateDelivery>().Max(x => x.CY_Order));
			AssertEquals(0u, entryline.KR_HighestImmediateDeliveryNo);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(3u, entryline.ImmediateDeliveries.Cast<ImmediateDelivery>().Max(x => x.CY_Order));
			AssertEquals(3u, entryline.KR_HighestImmediateDeliveryNo);

			entryline.ImmediateDeliveries.AddNew().CY_Order = 4;
			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(4u, entryline.ImmediateDeliveries.Cast<ImmediateDelivery>().Max(x => x.CY_Order));
			AssertEquals(4u, entryline.KR_HighestImmediateDeliveryNo);

			AssertNoExceptionThrownWhenAmendmentAccepted(entry);
		}

		public void TestKR_HighestNonGADetailNo()
		{
			var entry = GetCusEntryHeader();
			var entryLine = entry.MergedLines[0];

			entryLine.NonGADetailCollection.AddNew().CSI_LineNo = 1;
			entryLine.NonGADetailCollection.AddNew().CSI_LineNo = 2;
			entryLine.NonGADetailCollection.AddNew().CSI_LineNo = 3;
			AssertEquals(3u, entryLine.NonGADetailCollection.Cast<NonGADetail>().Max(x => x.CSI_LineNo));
			AssertEquals(0u, entryLine.KR_HighestNonGADetailNo);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(3u, entryLine.NonGADetailCollection.Cast<NonGADetail>().Max(x => x.CSI_LineNo));
			AssertEquals(3u, entryLine.KR_HighestNonGADetailNo);

			entryLine.NonGADetailCollection.AddNew().CSI_LineNo = 4;
			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(4u, entryLine.NonGADetailCollection.Cast<NonGADetail>().Max(x => x.CSI_LineNo));
			AssertEquals(4u, entryLine.KR_HighestNonGADetailNo);

			AssertNoExceptionThrownWhenAmendmentAccepted(entry);
		}

		public void TestKR_HighestPreviousExpDecLineNo()
		{
			var entry = GetCusEntryHeader();
			var entryLine = entry.MergedLines[0];

			entryLine.PreviousExpDecLineCollection.AddNew().CSI_LineNo = 1;
			entryLine.PreviousExpDecLineCollection.AddNew().CSI_LineNo = 2;
			entryLine.PreviousExpDecLineCollection.AddNew().CSI_LineNo = 3;
			AssertEquals(3u, entryLine.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>().Max(x => x.CSI_LineNo));
			AssertEquals(0u, entryLine.KR_HighestPreviousExpDecLineNo);

			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(3u, entryLine.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>().Max(x => x.CSI_LineNo));
			AssertEquals(3u, entryLine.KR_HighestPreviousExpDecLineNo);

			entryLine.PreviousExpDecLineCollection.AddNew().CSI_LineNo = 4;
			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			AssertEquals(4u, entryLine.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>().Max(x => x.CSI_LineNo));
			AssertEquals(4u, entryLine.KR_HighestPreviousExpDecLineNo);

			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929);

			AssertNoExceptionThrownWhenAmendmentAccepted(entry);
		}

		CusEntryHeader GetCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration.CustomsEntryHeaders[0];
		}

		void AssertNoExceptionThrownWhenAmendmentAccepted(CusEntryHeader entry)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entry.MergedLines.RemoveAndDeleteAll();
			CreateSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			entry.MergedLines.AddNew();
			AssertNoExceptionThrown(() => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted);
		}

		public void TestStatement929()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1";

			var statement1 = CreateCusStatementData(StatementHeaderTypeList.Codes.CustomsDisbursementBill, new ZDateTime(2024, 2, 1));
			var statement2 = CreateCusStatementData(StatementHeaderTypeList.Codes.Invoice, new ZDateTime(2024, 3, 1));
			Factory.Save();
			AssertEquals(statement1.PK, entry.Statement929.PK);

			var statement3 = CreateCusStatementData(StatementHeaderTypeList.Codes.Invoice, new ZDateTime(2024, 1, 1));
			Factory.Save();

			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			AssertEquals(statement3.PK, entry.Statement929.PK);

			CusStatementHeader CreateCusStatementData(string statementType, ZDateTime processDate)
			{
				var statement = Factory.New<CusStatementHeader>();
				statement.B2_StatementType = statementType;
				statement.B2_ProcessDate = processDate;

				var statementLine = statement.StatementLines.AddNew();
				statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
				statementLine.B3_EntryNum = entry.EntryNumber;

				return statement;
			}
		}

		public void TestExtensionMethod_GetDutyOrTaxAmount()
		{
			var entry = Factory.New<CusEntryHeader>();
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.Duty, 1m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.LiquorTax, 2m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.TransportationTax, 3m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 4m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.AgricultureTax, 5m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.EducationTax, 6m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.VAT, 7m);

			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.Duty, 10m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.LiquorTax, 20m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.TransportationTax, 30m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 40m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.AgricultureTax, 50m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.EducationTax, 60m);
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.VAT, 70m);

			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.Duty, 100m, "1");
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.LiquorTax, 200m, "1");
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.TransportationTax, 300m, "1");
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 400m, "1");
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.AgricultureTax, 500m, "1");
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.EducationTax, 600m, "1");
			CreateEntryHeaderChargeData(entry, ChargeTypeList.Codes.VAT, 700m, "1");

			AssertEquals(11m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.Duty));
			AssertEquals(22m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.LiquorTax));
			AssertEquals(33m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.TransportationTax));
			AssertEquals(44m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals(55m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.AgricultureTax));
			AssertEquals(66m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.EducationTax));
			AssertEquals(77m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.VAT));

			AssertEquals(100m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.Duty, "1"));
			AssertEquals(200m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.LiquorTax, "1"));
			AssertEquals(300m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.TransportationTax, "1"));
			AssertEquals(400m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.SpecialConsumptionTax, "1"));
			AssertEquals(500m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.AgricultureTax, "1"));
			AssertEquals(600m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.EducationTax, "1"));
			AssertEquals(700m, entry.GetAdditionalDutyOrTaxAmount(ChargeTypeList.Codes.VAT, "1"));

			AssertEquals(111m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.Duty));
			AssertEquals(222m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.LiquorTax));
			AssertEquals(333m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.TransportationTax));
			AssertEquals(444m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals(555m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.AgricultureTax));
			AssertEquals(666m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.EducationTax));
			AssertEquals(777m, entry.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.VAT));
		}

		void CreateEntryHeaderChargeData(CusEntryHeader entry, string chargeType, decimal chargeAmount, string rateOverrideReasonCode = "")
		{
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeType = chargeType;
			charge.C1_ChargeAmount = chargeAmount;
			charge.C1_RateOverrideReasonCode = rateOverrideReasonCode;
		}

		public void TestCustomsDisbursementBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement2.B2_ProcessDate = new ZDateTime(2024, 10, 08);
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "1234520000045M";
			statementLine2.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement3.B2_ProcessDate = new ZDateTime(2024, 10, 07);
			var statementLine3 = statement3.StatementLines.AddNew();
			statementLine3.B3_EntryNum = "1234520000045M";
			statementLine3.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			Factory.Save();

			AssertEquals(2, entry.CustomsDisbursementBills.Count);
			AssertEquals(statement3, entry.CustomsDisbursementBills[0]);
			AssertEquals(statement2, entry.CustomsDisbursementBills[1]);
		}

		public void TestStatementLinesAndPaidStatementLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement2.B2_ProcessDate = new ZDateTime(2024, 10, 08);
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "1234520000045M";
			statementLine2.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			var statementLine2_2 = statement2.StatementLines.AddNew();
			statementLine2_2.B3_EntryNum = "1234520000045M";
			statementLine2_2.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement3.B2_ProcessDate = new ZDateTime(2024, 10, 07);
			statement3.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine3 = statement3.StatementLines.AddNew();
			statementLine3.B3_EntryNum = "1234520000045M";
			statementLine3.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var statement4 = Factory.New<CusStatementHeader>();
			statement4.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement4.B2_ProcessDate = new ZDateTime(2024, 10, 07);
			statement4.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
			var statementLine4 = statement4.StatementLines.AddNew();
			statementLine4.B3_EntryNum = "1234520000045M";
			statementLine4.B3_EntryType = KRJobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			Factory.Save();

			AssertEquals(4, entry.StatementLines.Count());
			Assert(entry.StatementLines.Contains(statementLine2));
			Assert(entry.StatementLines.Contains(statementLine2_2));
			Assert(entry.StatementLines.Contains(statementLine3));
			Assert(entry.StatementLines.Contains(statementLine4));

			AssertEquals(3, entry.PaidStatementLines.Count());
			Assert(entry.StatementLines.Contains(statementLine2));
			Assert(entry.StatementLines.Contains(statementLine2_2));
			Assert(entry.StatementLines.Contains(statementLine3));
		}

		[TestDate(2024, 11, 1, 23, 00, 00)]
		public void TestPenaltyForLateDeclaration()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var firstLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.FirstLevel, 0.005m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			firstLevelFee.ZZF_Maximum = 5000000m;
			var secondLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.SecondLevel, 0.01m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			secondLevelFee.ZZF_Maximum = 5000000m;
			var thirdLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.ThirdLevel, 0.015m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			thirdLevelFee.ZZF_Maximum = 5000000m;
			var fourthLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.FourthLevel, 0.02m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			fourthLevelFee.ZZF_Maximum = 5000000m;
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-10);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-51);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrency.Code;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 200000000;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("If KR_LateDecPenaltyDateCode is empty, Penalty for Late Declaration is 0.", 0m, entry.PenaltyForLateDeclaration);

			declaration.JE_LateDecPenaltyDateCode = LateDecPenaltyDateCodeList.Codes.D;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("For 51 ~ 80 days after arrival date, Penalty for Late Declaration is (CustomsValue * 0.01 = 2,000,000)", 2000000m, entry.PenaltyForLateDeclaration);

			declaration.JE_LateDecPenaltyDateCode = LateDecPenaltyDateCodeList.Codes.W;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Within 30 days after arrival date, Penalty for Late Declaration is 0", 0m, entry.PenaltyForLateDeclaration);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-31);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("For 31 ~ 50 days after arrival date, Penalty for Late Declaration is (CustomsValue * 0.005 = 1,000,000)", 1000000m, entry.PenaltyForLateDeclaration);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-51);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("For 51 ~ 80 days after arrival date, Penalty for Late Declaration is (CustomsValue * 0.01 = 2,000,000)", 2000000m, entry.PenaltyForLateDeclaration);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-81);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("For 81 ~ 110 days after arrival date, Penalty for Late Declaration is (CustomsValue * 0.015 = 3,000,000)", 3000000m, entry.PenaltyForLateDeclaration);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-111);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("After 110 days from arrival date, Penalty for Late Declaration is (CustomsValue * 0.02 = 4,000,000)", 4000000m, entry.PenaltyForLateDeclaration);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200000000;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Penalty for Late Declaration is over the 5,000,000 won, Penalty for Late Declaration is 5,000,000", 5000000m, entry.PenaltyForLateDeclaration);

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum.CE_EntryNum = "1234524000001M";
			entryNum.CE_IssueDate = new ZDateTime(2024, 9, 1);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("If R99 for 929 is received, the comparing date is IssueDate. So, Penalty for Late Declaration is (CustomsValue * 0.005 = 2,000,000)", 2000000m, entry.PenaltyForLateDeclaration);

			invoiceLine.JI_LinePrice = 100000000m;
			invoiceLine2.JI_LinePrice = 111111111.111m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Penalty for Late Declaration is truncated. 211,111,111.111 * 0.005 = 1,055,555.555555 = 1,055,550", 1055550m, entry.PenaltyForLateDeclaration);
		}

		public void TestPenaltyForMissedDeclaration()
		{
			var helper = new TestDataSetupHelper(Factory);
			helper.SetupTaxOrFeeData();
			helper.SetupRefDBDataForDomesticTax();

			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrency.Code;

			var invoiceLine_1 = invoice.InvoiceLines.AddNew();
			invoiceLine_1.JI_Tariff = "9003191000";
			invoiceLine_1.JI_DomesticTaxCode = "412000-A";
			invoiceLine_1.JI_LinePrice = 100000000m;
			invoiceLine_1.JI_PrimaryPreference = "C";
			invoiceLine_1.JI_SecondaryPreference = "A095000102";
			invoiceLine_1.JI_DomesticTaxExemptionCode = "E109801";
			invoiceLine_1.JI_CustomsUnitQty = "U";
			invoiceLine_1.JI_CustomsQuantity = 2;
			invoiceLine_1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine_1.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			invoiceLine_1.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA;

			var invoiceLine_2 = invoice.InvoiceLines.AddNew();
			invoiceLine_2.JI_Tariff = "9003191000";
			invoiceLine_2.JI_DomesticTaxCode = "941220-A";
			invoiceLine_2.JI_LinePrice = 100000000m;
			invoiceLine_2.JI_PrimaryPreference = "C";
			invoiceLine_2.JI_CustomsUnitQty = "U";
			invoiceLine_2.JI_CustomsQuantity = 2;
			invoiceLine_2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine_2.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			invoiceLine_2.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA;

			var invoiceLine_3 = invoice.InvoiceLines.AddNew();
			invoiceLine_3.JI_Tariff = "9003191000";
			invoiceLine_3.JI_DomesticTaxCode = "779030-A";
			invoiceLine_3.JI_LinePrice = 100000000m;
			invoiceLine_3.JI_PrimaryPreference = "C";
			invoiceLine_3.JI_CustomsUnitQty = "U";
			invoiceLine_3.JI_CustomsQuantity = 2;
			invoiceLine_3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine_3.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			invoiceLine_3.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("If JE_MissedDecPenaltyRate = 0, Penalty For Missed Declaration is 0.", 0m, entry.PenaltyForMissedDeclaration);
			AssertNull("CusEntryHeaderCharge(Type:PMM) is not created when the amount is 0.", entry.Charges.FirstOrDefault(x => x.C1_ChargeType == ChargeTypeList.Codes.PenaltyForMissedDeclaration));

			declaration.JE_MissedDecPenaltyRate = 20;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(216000000m, entry.FormattedTotalDutyAmount);
			AssertEquals(21200000m, entry.TotalSpecialConsumptionTax);
			AssertEquals(54000000m, entry.TotalLiquorTax);
			AssertEquals(144000000m, entry.TotalTransportationTax);
			AssertEquals(11760000m, entry.TotalEducationTax);
			AssertEquals(6920000m, entry.TotalAgricultureTax);
			AssertEquals(23788000m, entry.TotalVAT);
			AssertEquals(477668000m, entry.TotalAmountPayable);
			AssertEquals("Penalty For Missed Declaration is Total Payable Amount * JE_MissedDecPenaltyRate = 477,668,000 * 0.2 = 95,533,600 ", 95533600m, entry.PenaltyForMissedDeclaration);

			invoiceLine_1.JI_LinePrice = 10000m;
			invoiceLine_2.JI_LinePrice = 10000m;
			invoiceLine_3.JI_LinePrice = 10000m;
			declaration.JE_MissedDecPenaltyRate = 1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(44500m, entry.TotalAmountPayable);
			AssertEquals("Penalty For Missed Declaration is truncated. 44,500 * 0.01 = 445 = 440", 440m, entry.PenaltyForMissedDeclaration);

			var existingCharge = entry.Charges.Single(x => x.C1_ChargeType == ChargeTypeList.Codes.PenaltyForMissedDeclaration && x.C1_RateOverrideReasonCode.IsEmpty);
			existingCharge.C1_RateOverrideReasonCode = "1";

			declaration.JE_MissedDecPenaltyRate = 0;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("If JE_MissedDecPenaltyRate = 0, Penalty For Missed Declaration is 0.", 0m, entry.PenaltyForMissedDeclaration);

			var currentCharge = entry.Charges.Single(x => x.C1_ChargeType == ChargeTypeList.Codes.PenaltyForMissedDeclaration && x.C1_RateOverrideReasonCode.IsEmpty);
			AssertEquals("Current version of PenaltyForMissedDeclaration", -440m, currentCharge.C1_ChargeAmount);
		}

		void SetupRefDBDataForDutyCalculation()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			var tariff1 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101211000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "농가 사육용");
			var tariff2 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101291000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "경주말");
			var tariff3 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101292000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "경주말");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateForTariff1 = referenceDataHelper.CreateRate(tariff1, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "0");
			var rateForTariff2 = referenceDataHelper.CreateRate(tariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "8");
			var rateForTariff3 = referenceDataHelper.CreateRate(tariff3, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "8");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateForTariff1, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff3, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
		}

		void SetupPenaltyForLateDeclaration()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var firstLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.FirstLevel, 0.005m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			firstLevelFee.ZZF_Maximum = 5000000m;
			var secondLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.SecondLevel, 0.01m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			secondLevelFee.ZZF_Maximum = 5000000m;
			var thirdLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.ThirdLevel, 0.015m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			thirdLevelFee.ZZF_Maximum = 5000000m;
			var fourthLevelFee = helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.PenaltyForLateDeclaration.FourthLevel, 0.02m, dataGrouping, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			fourthLevelFee.ZZF_Maximum = 5000000m;
			Factory.Save();
		}

		[TestDate(2024, 11, 1, 23, 00, 00)]
		public void TestCreateOnlyDiffAmount()
		{
			SetupRefDBDataForDutyCalculation();
			SetupPenaltyForLateDeclaration();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_LateDecPenaltyDateCode = LateDecPenaltyDateCodeList.Codes.D;
			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-10);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-51);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			invoice.JZ_InvoiceAmount = 5326522m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0101291000";
			invoiceLine1.JI_LinePrice = 22222m;
			invoiceLine1.JI_PrimaryPreference = "C";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0101291000";
			invoiceLine2.JI_LinePrice = 4326522m;
			invoiceLine2.JI_PrimaryPreference = "C";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("One entry line exists", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];

			var charges = entry.Charges.Cast<CusEntryHeaderCharges>().Where(x => x.C1_ChargeType == ChargeTypeList.Codes.PenaltyForLateDeclaration);
			var totalVersionedCharge = charges.Where(x => !x.C1_RateOverrideReasonCode.IsEmpty).Sum(x => x.C1_ChargeAmount);
			var nonVersionedCharge = charges.Where(x => x.C1_RateOverrideReasonCode.IsEmpty).Sum(x => x.C1_ChargeAmount);
			AssertEquals("Versioned Amount", 0m, totalVersionedCharge);
			AssertEquals("Non-Versioned Amount", 43480m, nonVersionedCharge);

			var fees = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == ChargeTypeList.Codes.Duty);
			var totalVersionedFee = fees.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			var nonVersionedFee = fees.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			AssertEquals("Versioned Amount", 0m, totalVersionedFee);
			AssertEquals("Non-Versioned Amount", 347899m, nonVersionedFee);

			AssertEquals(0u, entry.CH_VersionID);
			AssertEquals(false, charges.Any(x => x.C1_RateOverrideReasonCode == (entry.CH_VersionID + 1).ToString()));
			AssertEquals(false, fees.Any(x => x.CF_RateOverrideReasonCode == (entry.CH_VersionID + 1).ToString()));

			entry.SetChargesAndLineFeesVersion();
			AssertEquals(true, charges.Any(x => x.C1_RateOverrideReasonCode == (entry.CH_VersionID + 1).ToString()));
			AssertEquals(true, fees.Any(x => x.CF_RateOverrideReasonCode == (entry.CH_VersionID + 1).ToString()));

			declaration.JE_LateDecPenaltyDateCode = LateDecPenaltyDateCodeList.Codes.W;
			invoiceLine1.JI_LinePrice = 32222m;
			invoiceLine2.JI_LinePrice = 5326522m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			totalVersionedCharge = charges.Where(x => !x.C1_RateOverrideReasonCode.IsEmpty).Sum(x => x.C1_ChargeAmount);
			nonVersionedCharge = charges.Where(x => x.C1_RateOverrideReasonCode.IsEmpty).Sum(x => x.C1_ChargeAmount);
			AssertEquals("Versioned Amount", 43480m, totalVersionedCharge);
			AssertEquals("Non-Versioned Amount", -43480m, nonVersionedCharge);

			totalVersionedFee = fees.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			nonVersionedFee = fees.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			AssertEquals("One entry line exists", 1, entry.MergedLines.Count);
			AssertEquals("Versioned Amount", 347899m, totalVersionedFee);
			AssertEquals("Non-Versioned Amount", 80800m, nonVersionedFee);

			invoiceLine2.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			totalVersionedFee = fees.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			nonVersionedFee = fees.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			AssertEquals("One entry line exists", 2, entry.MergedLines.Count);
			AssertEquals("Versioned Amount", 347899m, totalVersionedFee);
			AssertEquals("(347899 * Amount proportion + 8088 * Amount proportion) - totalVersionedFee", -345322m, nonVersionedFee);

			var fees2 = entry.MergedLines[1].Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == ChargeTypeList.Codes.Duty);
			AssertEquals("Versioned Amount", 0m, fees2.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount));
			AssertEquals("(347899 * Amount proportion + 8088 * Amount proportion) - totalVersionedFee", 426121m, fees2.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount));
		}

		[TestDate(2024, 11, 1, 23, 00, 00)]
		public void TestGenerateHeaderChargesWithLineFees()
		{
			SetupRefDBDataForDutyCalculation();
			SetupPenaltyForLateDeclaration();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_LateDecPenaltyDateCode = LateDecPenaltyDateCodeList.Codes.D;
			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-10);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-51);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			invoice.JZ_InvoiceAmount = 1000000m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0101291000";
			invoiceLine1.JI_LinePrice = 50000m;
			invoiceLine1.JI_PrimaryPreference = "C";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0101291000";
			invoiceLine2.JI_LinePrice = 50000m;
			invoiceLine2.JI_PrimaryPreference = "C";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("One entry line exists", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];

			var charges = entry.Charges.Where(x => x.C1_ChargeType == ChargeTypeList.Codes.Duty);
			AssertEquals("One header charge exists", 1, charges.Count());

			var fees = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == ChargeTypeList.Codes.Duty);
			AssertEquals("One line fee exists", 1, fees.Count());

			var totalVersionedFee = fees.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			var nonVersionedFee = fees.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			AssertEquals("Versioned Amount", 0m, totalVersionedFee);
			AssertEquals("Non-Versioned Amount", 8000m, nonVersionedFee);

			entry.SetChargesAndLineFeesVersion();
			AssertEquals(true, charges.Any(x => x.C1_RateOverrideReasonCode == "1"));
			AssertEquals(true, fees.Any(x => x.CF_RateOverrideReasonCode == "1"));

			invoiceLine2.JI_Tariff = "0101292000";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Two entry line exists", 2, entry.MergedLines.Count);
			charges = entry.Charges.Cast<CusEntryHeaderCharges>().Where(x => x.C1_ChargeType == ChargeTypeList.Codes.Duty);
			AssertEquals("One header charge exists", 1, charges.Count());

			fees = entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == ChargeTypeList.Codes.Duty);
			AssertEquals("Two line fees exist", 2, fees.Count());

			totalVersionedFee = fees.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			nonVersionedFee = fees.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			AssertEquals("Versioned Amount", 8000m, totalVersionedFee);
			AssertEquals("Non-Versioned Amount", -4000m, nonVersionedFee);

			var entryLine2 = entry.MergedLines[1];
			fees = entryLine2.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == ChargeTypeList.Codes.Duty);
			totalVersionedFee = fees.Where(x => !x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			nonVersionedFee = fees.Where(x => x.CF_RateOverrideReasonCode.IsEmpty).Sum(x => x.CF_ChargeAmount);
			AssertEquals("Versioned Amount", 0m, totalVersionedFee);
			AssertEquals("Non-Versioned Amount", 4000m, nonVersionedFee);
		}

		public void TestGetOrCreateRefundSessionalDataOriginalSendable()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entry1 = CreateSessionalDatasForCusEntryHeader("0127030112200237051", ZDateTime.Empty);
			AssertEquals(1, entry1.EntryInstruction.RefundSessionalDataCollection.Count);
			AssertEquals("0127030112200237051", entry1.EntryInstruction.RefundSessionalDataCollection[0].CSI_ReferenceNumber2);
			AssertEquals(ZDateTime.Empty, entry1.EntryInstruction.RefundSessionalDataCollection[0].CSI_DateOfIssue);
			var refundSessionalData = entry1.GetOrCreateRefundSessionalDataOriginalSendable("XXXXXXXXXXXXXXXXXXX");
			AssertEquals(2, entry1.EntryInstruction.RefundSessionalDataCollection.Count);
			AssertEquals("XXXXXXXXXXXXXXXXXXX", refundSessionalData.CSI_ReferenceNumber2);
			AssertEquals(ZDateTime.Empty, refundSessionalData.CSI_DateOfIssue);

			var entry2 = CreateSessionalDatasForCusEntryHeader("0127030112200237052", new ZDateTime("2025-01-01"));
			AssertEquals(1, entry2.EntryInstruction.RefundSessionalDataCollection.Count);
			AssertEquals("0127030112200237052", entry2.EntryInstruction.RefundSessionalDataCollection[0].CSI_ReferenceNumber2);
			AssertEquals(new ZDateTime("2025-01-01"), entry2.EntryInstruction.RefundSessionalDataCollection[0].CSI_DateOfIssue);
			refundSessionalData = entry2.GetOrCreateRefundSessionalDataOriginalSendable("0127030112200237052");
			AssertEquals(2, entry2.EntryInstruction.RefundSessionalDataCollection.Count);
			AssertEquals("0127030112200237052", refundSessionalData.CSI_ReferenceNumber2);
			AssertEquals(ZDateTime.Empty, refundSessionalData.CSI_DateOfIssue);

			var entry3 = CreateSessionalDatasForCusEntryHeader("0127030112200237053", ZDateTime.Empty);
			AssertEquals(1, entry3.EntryInstruction.RefundSessionalDataCollection.Count);
			AssertEquals("0127030112200237053", entry3.EntryInstruction.RefundSessionalDataCollection[0].CSI_ReferenceNumber2);
			AssertEquals(ZDateTime.Empty, entry3.EntryInstruction.RefundSessionalDataCollection[0].CSI_DateOfIssue);
			entry3.GetOrCreateRefundSessionalDataOriginalSendable("0127030112200237053");
			AssertEquals(1, entry3.EntryInstruction.RefundSessionalDataCollection.Count);

			CusEntryHeader CreateSessionalDatasForCusEntryHeader(string billNumber, ZDateTime dateOfIssue)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = instruction.PK;

				var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
				refundSessionalData.CSI_ReferenceNumber2 = billNumber;
				refundSessionalData.CSI_DateOfIssue = dateOfIssue;

				return entry;
			}
		}

		sealed class EntryThrowingExceptionAfterOnSaving : CusEntryHeader
		{
			public EntryThrowingExceptionAfterOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldThrowException;
			public override void OnSaving()
			{
				base.OnSaving();
				if (ShouldThrowException)
				{
					throw new ApplicationException("intended");
				}
			}
		}
	}
}
