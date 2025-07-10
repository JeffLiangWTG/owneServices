using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestAddRecordArchivedLog()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			AssertNull(testEntry.Logs.Find(log => log.SL_SE_NKEvent == Events.RecordArchivedCode).FirstOrDefault());
			AssertEquals(ZDateTime.Empty, testEntry.ArchiveDate);
			AssertEquals(ZString.Empty, testEntry.ArchiveUser);
			testEntry.AddRecordArchivedLog();
			var logACVs = testEntry.Logs.Find(log => log.SL_SE_NKEvent == Events.RecordArchivedCode);
			AssertEquals(1, logACVs.Count());
			var logACV = logACVs.First();
			AssertEquals(logACV.SL_EventTime, testEntry.ArchiveDate);
			AssertEquals(logACV.User.FullName, testEntry.ArchiveUser);
			AssertEquals(Env.CurrentUser.FullName, testEntry.ArchiveUser);
			testEntry.AddRecordArchivedLog();
			logACVs = testEntry.Logs.Find(log => log.SL_SE_NKEvent == Events.RecordArchivedCode);
			AssertEquals(1, logACVs.Count());
			logACV = logACVs.First();
			AssertEquals(logACV.SL_EventTime, testEntry.ArchiveDate);
			AssertEquals(logACV.User.FullName, testEntry.ArchiveUser);
			Factory.Save();
			testEntry.AddRecordArchivedLog();
			logACVs = testEntry.Logs.Find(log => log.SL_SE_NKEvent == Events.RecordArchivedCode);
			AssertEquals(2, logACVs.Count());
			logACV = logACVs.First(log => !log.IsCancelled);
			AssertEquals(logACV.SL_EventTime, testEntry.ArchiveDate);
			AssertEquals(logACV.User.FullName, testEntry.ArchiveUser);
		}

		public void TestHasBeenLodgedAtCustoms()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			Assert(!testEntry.HasBeenLodgedAtCustoms);
			testEntry.DeclarationUnifiedNumber = "1";
			Assert(testEntry.HasBeenLodgedAtCustoms);
			testEntry.DeclarationUnifiedNumber = "";
			testEntry.EntryNumber = "1";
			Assert(testEntry.HasBeenLodgedAtCustoms);
		}

		public void TestIsWaitingForResponse()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			Assert(!testEntry.IsWaitingForResponse);
			testEntry.CH_Status = JobMessageStatusList.Codes.AcknowledgedCompletedDeclaration;
			AssertEquals("IsWaitingForResponse should be false for ACM", false, testEntry.IsWaitingForResponse);
			testEntry.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;
			AssertEquals("IsWaitingForResponse should be false for AWM", true, testEntry.IsWaitingForResponse);
			testEntry.CH_Status = JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration;
			AssertEquals("IsWaitingForResponse should be false for AWO", true, testEntry.IsWaitingForResponse);
			testEntry.CH_Status = JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration;
			AssertEquals("IsWaitingForResponse should be false for AWP", true, testEntry.IsWaitingForResponse);
		}

		public void TestIsFormalEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert(!entryHeader.IsFormalEntry);
			entryHeader.CH_MessageType = "CUS";
			Assert(entryHeader.IsFormalEntry);
			entryHeader.CH_MessageType = "PRE";
			Assert(!entryHeader.IsFormalEntry);
			entryHeader.CH_MessageType = "REC";
			Assert(entryHeader.IsFormalEntry);
		}

		public void TestEntryHeaderContainerCollection()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals(typeof(EntryHeaderContainerCollection), testEntry.EntryHeaderContainers.GetType());
		}

		public void TestContainersEnabled()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			testDeclaration.JE_TransportMode = "AIR";
			AssertEquals(true, testEntry.Declaration.ContainersRequired);
			testDeclaration.JE_TransportMode = "SEA";
			AssertEquals(true, testEntry.Declaration.ContainersRequired);
			testDeclaration.JE_TransportMode = "FIX";
			AssertEquals(false, testEntry.Declaration.ContainersRequired);
		}

		public void TestPackageCounts()
		{
			var testInstruction = Factory.New<CusEntryInstruction>();
			testInstruction.CEI_Packages = 50;
			var testEntry = Factory.New<CusEntryHeader>();
			testEntry.CH_CEI_Instruction = testInstruction.PK;
			AssertEquals(50, testEntry.PackagesCount);
		}

		public void TestHumanReadableNameCore()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			testEntry.CH_BGMReference = "123";
			AssertEquals("Entry 123", testEntry.HumanReadableName);
		}

		public void TestIsCustomsEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			Assert("IsCustomsEntry", entryHeader.IsCustomsEntry);
			entryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
			Assert("IsCustomsEntry", !entryHeader.IsCustomsEntry);
		}

		public void TestIsRecordListing()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			Assert("IsRecordListing", !entryHeader.IsRecordListing);
			entryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
			Assert("IsRecordListing", entryHeader.IsRecordListing);
		}

		public void TestIsEnteringAndIsExiting()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				Assert("IMP+CUS+CUS - IsEntering", entryHeader.IsEntering);
				Assert("IMP+CUS+CUS - IsExiting", !entryHeader.IsExiting);
				declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				entryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
				Assert("IMP+REC+REC - IsEntering", entryHeader.IsEntering);
				Assert("IMP+REC+REC - IsExiting", !entryHeader.IsExiting);
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				Assert("IMP+BTH+CUS - IsEntering", entryHeader.IsEntering);
				Assert("IMP+BTH+CUS - IsExiting", !entryHeader.IsExiting);
				entryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
				Assert("IMP+BTH+REC - IsEntering", !entryHeader.IsEntering);
				Assert("IMP+BTH+REC - IsExiting", entryHeader.IsExiting);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				Assert("EXP+CUS+CUS - IsEntering", !entryHeader.IsEntering);
				Assert("EXP+CUS+CUS - IsExiting", entryHeader.IsExiting);
				declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				entryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
				Assert("EXP+REC+REC - IsEntering", !entryHeader.IsEntering);
				Assert("EXP+REC+REC - IsExiting", entryHeader.IsExiting);
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				Assert("EXP+BTH+CUS - IsEntering", !entryHeader.IsEntering);
				Assert("EXP+BTH+CUS - IsExiting", entryHeader.IsExiting);
				entryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
				Assert("EXP+BTH+REC - IsEntering", entryHeader.IsEntering);
				Assert("EXP+BTH+REC - IsExiting", !entryHeader.IsExiting);
			}

			);
		}

		public void TestManuallySetEntryNumber()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			var testDeclaration = Factory.New<JobDeclaration>();
			testEntry.CH_JE = testDeclaration.PK;
			AssertEquals(ZString.Empty, testEntry.PreEntryNumber);
			AssertEquals(ZString.Empty, testEntry.DeclarationUnifiedNumber);
			AssertEquals(ZString.Empty, testEntry.MovementReferenceNumber);
			AssertEquals(ZString.Empty, testEntry.CIQNumber);
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "000000000000000001", new ZDateTime(2018, 12, 3));
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "000000000000000002", new ZDateTime(2018, 12, 3));
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "000000000000000004", new ZDateTime(2018, 12, 3));
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2019, 5, 21));
			AssertEquals("000000000000000001", testEntry.PreEntryNumber);
			AssertEquals(new ZDateTime(2018, 12, 3), CusEntryNumber.Load(testEntry, CusEntryNumberTypes.China.PreEntryNumber, testEntry.CountryCode).CE_IssueDate);
			AssertEquals("000000000000000002", testEntry.DeclarationUnifiedNumber);
			AssertEquals(new ZDateTime(2018, 12, 3), CusEntryNumber.Load(testEntry, CusEntryNumberTypes.China.DeclarationUnifiedNumber, testEntry.CountryCode).CE_IssueDate);
			AssertEquals("000000000000000003", testEntry.MovementReferenceNumber);
			AssertEquals(new ZDateTime(2019, 5, 21), CusEntryNumber.Load(testEntry, CusEntryNumberTypes.Standard.MovementReferenceNumber, testEntry.CountryCode).CE_IssueDate);
			AssertEquals("000000000000000004", testEntry.CIQNumber);
			AssertEquals(new ZDateTime(2018, 12, 3), CusEntryNumber.Load(testEntry, CusEntryNumberTypes.China.CIQNumber, testEntry.CountryCode).CE_IssueDate);
			var testEntry2 = Factory.New<CusEntryHeader>();
			var testDeclaration2 = Factory.New<JobDeclaration>();
			testEntry2.CH_JE = testDeclaration2.PK;
			var entryNubmerPRE = Factory.New<CusEntryNumber>();
			entryNubmerPRE.CE_EntryNum = "100000000000000001";
			entryNubmerPRE.CE_EntryType = "PRE";
			entryNubmerPRE.CE_ParentTable = testEntry2.TableName;
			entryNubmerPRE.CE_RN_NKCountryCode = "CN";
			entryNubmerPRE.CE_ParentID = testEntry2.PK;
			entryNubmerPRE.CE_EntryIsSystemGenerated = false;
			var entryNubmerUNI = Factory.New<CusEntryNumber>();
			entryNubmerUNI.CE_EntryNum = "100000000000000002";
			entryNubmerUNI.CE_EntryType = "UNI";
			entryNubmerUNI.CE_ParentTable = testEntry2.TableName;
			entryNubmerUNI.CE_RN_NKCountryCode = "CN";
			entryNubmerUNI.CE_ParentID = testEntry2.PK;
			entryNubmerUNI.CE_EntryIsSystemGenerated = false;
			var entryNubmerMRN = Factory.New<CusEntryNumber>();
			entryNubmerMRN.CE_EntryNum = "100000000000000003";
			entryNubmerMRN.CE_EntryType = "MRN";
			entryNubmerMRN.CE_ParentTable = testEntry2.TableName;
			entryNubmerMRN.CE_RN_NKCountryCode = "CN";
			entryNubmerMRN.CE_ParentID = testEntry2.PK;
			entryNubmerMRN.CE_EntryIsSystemGenerated = false;
			var entryNubmerCIQ = Factory.New<CusEntryNumber>();
			entryNubmerCIQ.CE_EntryNum = "100000000000000004";
			entryNubmerCIQ.CE_EntryType = "CIQ";
			entryNubmerCIQ.CE_ParentTable = testEntry2.TableName;
			entryNubmerCIQ.CE_RN_NKCountryCode = "CN";
			entryNubmerCIQ.CE_ParentID = testEntry2.PK;
			entryNubmerCIQ.CE_EntryIsSystemGenerated = false;
			Factory.Save();
			AssertEquals("100000000000000001", testEntry2.PreEntryNumber);
			AssertEquals("100000000000000002", testEntry2.DeclarationUnifiedNumber);
			AssertEquals("100000000000000003", testEntry2.MovementReferenceNumber);
			AssertEquals("100000000000000004", testEntry2.CIQNumber);
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "100000000000000021");
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "100000000000000022");
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "100000000000000024");
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "100000000000000023", new ZDateTime(2019, 5, 21));
			AssertEquals("100000000000000021", entryNubmerPRE.CE_EntryNum);
			AssertEquals(ZDateTime.Empty, entryNubmerPRE.CE_IssueDate);
			AssertEquals("100000000000000022", entryNubmerUNI.CE_EntryNum);
			AssertEquals(ZDateTime.Empty, entryNubmerUNI.CE_IssueDate);
			AssertEquals("100000000000000023", entryNubmerMRN.CE_EntryNum);
			AssertEquals(new ZDateTime(2019, 5, 21), entryNubmerMRN.CE_IssueDate);
			AssertEquals("100000000000000024", entryNubmerCIQ.CE_EntryNum);
			AssertEquals(ZDateTime.Empty, entryNubmerCIQ.CE_IssueDate);
			entryNubmerPRE.CE_EntryIsSystemGenerated = true;
			entryNubmerUNI.CE_EntryIsSystemGenerated = true;
			entryNubmerMRN.CE_EntryIsSystemGenerated = true;
			entryNubmerCIQ.CE_EntryIsSystemGenerated = true;
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "100000000000000031");
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "100000000000000032");
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "100000000000000034");
			testEntry2.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "100000000000000033");
			AssertEquals("Should not manully update system generated entry number", "100000000000000021", entryNubmerPRE.CE_EntryNum);
			AssertEquals("Should not manully update system generated entry number", "100000000000000022", entryNubmerUNI.CE_EntryNum);
			AssertEquals("Should not manully update system generated entry number", "100000000000000023", entryNubmerMRN.CE_EntryNum);
			AssertEquals("Should not manully update system generated entry number", "100000000000000024", entryNubmerCIQ.CE_EntryNum);
		}

		public void TestRelatedMRNDefaulting()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageSubType = "BTH";
			var childInstruction = testItems.JobDeclaration.CustomsEntryInstructions.AddNew();
			testItems.EntryInstruction.CEI_DateForDuty = new ZDateTime(2018, 1, 1);
			var childEntry = testItems.JobDeclaration.ActiveEntryHeaders.AddNew();
			childEntry.CH_CEI_Instruction = childInstruction.PK;
			var entryHeader = testItems.EntryHeader;
			entryHeader.SetMovementReferenceNumber("100000000000000023", new ZDateTime(2018, 12, 3));
			AssertEquals("Child Instruction Related MRN should have been set.", "100000000000000023", childInstruction.CEI_RelatedMRN);
			AssertEquals("Instruction DateForDuty should have been set", new ZDateTime(2018, 12, 3), testItems.EntryInstruction.CEI_DateForDuty);
			childEntry.SetMovementReferenceNumber("100000000000000033", new ZDateTime(2018, 12, 3));
			AssertEquals("Instruction DateForDuty should have been set", new ZDateTime(2018, 12, 3), childInstruction.CEI_DateForDuty);
		}

		public void TestSetCIQNumberAndStatus()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("CIQNumber", ZString.Empty, entry.CIQNumber);
			AssertEquals("CIQIssueDate", ZDateTime.Empty, entry.CIQIssueDate);
			AssertEquals("CIQStatus", ZString.Empty, entry.CIQStatus);
			entry.SetCIQStatus("New");
			AssertEquals("CIQStatus", ZString.Empty, entry.CIQStatus);
			entry.SetCIQNumber("CIQ1234", new ZDateTime(2019, 5, 27));
			entry.SetCIQStatus("New");
			AssertEquals("CIQNumber", "CIQ1234", entry.CIQNumber);
			AssertEquals("CIQIssueDate", new ZDateTime(2019, 5, 27), entry.CIQIssueDate);
			AssertEquals("CIQStatus", "New", entry.CIQStatus);
		}

		public void TestPreEntryNumberAndIssueDateAndSetter()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			var testDeclaration = Factory.New<JobDeclaration>();
			testEntry.CH_JE = testDeclaration.PK;
			var entryNubmerPRE = Factory.New<CusEntryNumber>();
			entryNubmerPRE.CE_EntryNum = "100000000000000001";
			entryNubmerPRE.CE_IssueDate = new ZDateTime(2019, 5, 27);
			entryNubmerPRE.CE_EntryType = "PRE";
			entryNubmerPRE.CE_ParentTable = testEntry.TableName;
			entryNubmerPRE.CE_RN_NKCountryCode = "CN";
			entryNubmerPRE.CE_ParentID = testEntry.PK;
			entryNubmerPRE.CE_EntryIsSystemGenerated = false;
			Factory.Save();
			AssertEquals("PreEntryNumber should have loaded the correct value", "100000000000000001", testEntry.PreEntryNumber);
			AssertEquals("PreEntryNumber should have loaded the correct value", new ZDateTime(2019, 5, 27), testEntry.PreEntryNumberIssueDate);
			testEntry.SetPreEntryNumber("100000000000000002", new ZDateTime(2019, 5, 28));
			Factory.Save();
			AssertEquals("PreEntryNumber should have been set", "100000000000000002", testEntry.PreEntryNumber);
			AssertEquals("PreEntryNumber should have been set", new ZDateTime(2019, 5, 28), testEntry.PreEntryNumberIssueDate);
			var loadedPreEntryNum = new BusinessObjectFactory().Load<CusEntryNumber>(entryNubmerPRE.PK);
			AssertEquals("PreEntryNumber should have been set", "100000000000000002", loadedPreEntryNum.CE_EntryNum);
			AssertEquals("PreEntryNumber should have been set", new ZDateTime(2019, 5, 28), loadedPreEntryNum.CE_IssueDate);
		}

		public void TestLogs()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			var testDeclaration = Factory.New<JobDeclaration>();
			testEntry.CH_JE = testDeclaration.PK;
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "PRE1");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "UNI1");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "CIQ1");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRN1", new ZDateTime(2018, 12, 3));
			Factory.Save();
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "UNI: From <> to <UNI1>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "MRN: From <> to <MRN1>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "PRE: From <> to <PRE1>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "CIQ: From <> to <CIQ1>").FirstOrDefault() != null);
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "PRE2");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "UNI2");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "CIQ2");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRN2", new ZDateTime(2018, 12, 3));
			Factory.Save();
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "UNI: From <UNI1> to <UNI2>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "MRN: From <MRN1> to <MRN2>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "PRE: From <PRE1> to <PRE2>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "CIQ: From <CIQ1> to <CIQ2>").FirstOrDefault() != null);
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "", new ZDateTime(2018, 12, 3));
			Factory.Save();
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "UNI: From <UNI2> to <>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "MRN: From <MRN2> to <>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "PRE: From <PRE2> to <>").FirstOrDefault() != null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "CIQ: From <CIQ2> to <>").FirstOrDefault() != null);
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "");
			testEntry.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "", new ZDateTime(2018, 12, 3));
			Factory.Save();
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "UNI: From <> to <>").FirstOrDefault() == null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "MRN: From <> to <>").FirstOrDefault() == null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "PRE: From <> to <>").FirstOrDefault() == null);
			Assert(testEntry.Logs.Find(log => log.ReferenceFreeText == "CIQ: From <> to <>").FirstOrDefault() == null);
		}

		[TestDate(2018, 1, 5)]
		public void TestCNCustomsLocalReferenceNumber()
		{
			Env.NumberFountains.GetCNCustomsLocalReferenceNumber("CUS").Reset();
			Env.NumberFountains.GetCNCustomsLocalReferenceNumber("CIQ").Reset();
			Env.NumberFountains.GetCNCustomsLocalReferenceNumber("PRE").Reset();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntryCUS1 = Factory.New<CusEntryHeader>();
			testEntryCUS1.CH_JE = testDeclaration.PK;
			testEntryCUS1.CH_MessageType = "CUS";
			var testEntryCUS2 = Factory.New<CusEntryHeader>();
			testEntryCUS2.CH_JE = testDeclaration.PK;
			testEntryCUS2.CH_MessageType = "CUS";
			var testEntryPRE1 = Factory.New<CusEntryHeader>();
			testEntryPRE1.CH_JE = testDeclaration.PK;
			testEntryPRE1.CH_MessageType = "PRE";
			var testEntryPRE2 = Factory.New<CusEntryHeader>();
			testEntryPRE2.CH_JE = testDeclaration.PK;
			testEntryPRE2.CH_MessageType = "PRE";
			Factory.Save();
			AssertEquals("CUS201801050000001", testEntryCUS1.CH_BGMReference);
			AssertEquals("CUS201801050000002", testEntryCUS2.CH_BGMReference);
			AssertEquals("PRE201801050000001", testEntryPRE1.CH_BGMReference);
			AssertEquals("PRE201801050000002", testEntryPRE2.CH_BGMReference);
		}

		public void TestCustomsMessageRemarks()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = Factory.New<CusEntryHeader>();
			testDeclaration.CustomsEntryHeaders.Add(testEntry);
			var loadingQuery = new ZQuery(StmNoteSchema.ST_ParentID, testEntry.PK);
			loadingQuery.AddToFilter(StmNoteSchema.ST_Table, CusEntryHeader.Schema.TableName);
			var testLoadOfNote = Factory.Load<StmNote>(loadingQuery);
			AssertEquals(0, testLoadOfNote.Length);
			testEntry.CH_CustomsMessageRemarks = "test123";
			testLoadOfNote = Factory.Load<StmNote>(loadingQuery);
			AssertEquals(1, testLoadOfNote.Length);
			AssertEquals("test123", testLoadOfNote[0].ST_NoteText);
			AssertEquals("Customs Message Remarks", testLoadOfNote[0].ST_Description);
			testEntry.CH_CustomsMessageRemarks = "test123223";
			testLoadOfNote = Factory.Load<StmNote>(loadingQuery);
			AssertEquals(1, testLoadOfNote.Length);
			AssertEquals("test123223", testLoadOfNote[0].ST_NoteText);
			AssertEquals("Customs Message Remarks", testLoadOfNote[0].ST_Description);
			testEntry.CH_CustomsMessageRemarks = "";
			testLoadOfNote = Factory.Load<StmNote>(loadingQuery);
			AssertEquals(0, testLoadOfNote.Length);
			testEntry.CH_CustomsMessageRemarks = "test123223";
			testLoadOfNote = Factory.Load<StmNote>(loadingQuery);
			AssertEquals(1, testLoadOfNote.Length);
			AssertEquals(false, testLoadOfNote[0].IsDeleted);
			Factory.Save();
			testEntry.CH_CustomsMessageRemarks = "";
			AssertEquals(true, testLoadOfNote[0].IsDeleted);
		}

		public void TestIsChildEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction1.PK;
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;
			Assert("IsChildEntry", !entryHeader1.IsChildEntry);
			Assert("IsChildEntry", entryHeader2.IsChildEntry);
		}

		public void TestEntryHeaderSupportingDocuments()
		{
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNRequiredDocuments, "01", "1");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNRequiredDocuments, "0y", "y");
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNRequiredDocuments, "1Y", "Y");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", RefCusCodeListTypesCodes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", RefCusCodeListTypesCodes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			Factory.Save();
			code1.Attributes.AddNew("DisplayCode", "1");
			code1.Attributes.AddNew("Import", "");
			code2.Attributes.AddNew("DisplayCode", "y");
			code2.Attributes.AddNew("Import", "");
			code3.Attributes.AddNew("DisplayCode", "Y");
			code3.Attributes.AddNew("Import", "");
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItems.JobDeclaration;
			declaration.JE_MessageType = "IMP";
			testItems.InvoiceLine.CusSupportingDocuments.AddNew("01", "");
			testItems.InvoiceLine.CusSupportingDocuments.AddNew("0y", "");
			testItems.InvoiceLine.CusSupportingDocuments.AddNew("1Y", "");
			AssertEquals("CusSupportingDocuments should have item", 3, testItems.EntryHeader.CusSupportingDocuments.Count());
			AssertEquals("SupportingDocuments should have 3 items (COO should be incuded).", 3, testItems.EntryHeader.SupportingDocuments.Count);
		}

		public void TestInvoiceNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			var invoiceLine = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV002";
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction1.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("INV001,INV002", declaration.CustomsEntryHeaders[0].InvoiceNumbers);
		}

		public void TestTotalAmountProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.Duty);
			var expRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.ExportDuty);
			var excRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.Excise);
			var addRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.AntiDumping);
			var cvdRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.Countervailing);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, "DT1", dtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "DT2", dtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EX1", expRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EX2", expRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EC1", excRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EC2", excRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "ADD", addRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CVD", cvdRateType.PK);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_ZZF_NKTaxType = "VRD";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_ZZF_NKTaxType = "VAT";
			entryLine1.Fees.AddOrUpdate("DT1", 12m);
			entryLine2.Fees.AddOrUpdate("DT2", 23m);
			entryLine1.Fees.AddOrUpdate("EX1", 34m);
			entryLine2.Fees.AddOrUpdate("EX2", 45m);
			entryLine1.Fees.AddOrUpdate("EC1", 56m);
			entryLine2.Fees.AddOrUpdate("EC2", 67m);
			entryLine1.Fees.AddOrUpdate("VRD", 78m);
			entryLine2.Fees.AddOrUpdate("VAT", 91m);
			entryLine1.Fees.AddOrUpdate("ADD", 21.1m);
			entryLine2.Fees.AddOrUpdate("ADD", 32.2m);
			entryLine1.Fees.AddOrUpdate("CVD", 43.3m);
			entryLine2.Fees.AddOrUpdate("CVD", 54.4m);
			AssertEquals("DutyAmount", 35m, entryHeader.TotalDutyAmount);
			AssertEquals("GSTVATAmount", 169m, entryHeader.TotalGSTVATAmount);
			AssertEquals("ExciseAmount", 123m, entryHeader.TotalExciseAmount);
			AssertEquals("AntiDumpingAmount", 53.3m, entryHeader.TotalAntiDumpingAmount);
			AssertEquals("CountervailingAmount", 97.7m, entryHeader.TotalCountervailingAmount);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("DutyAmount", 79m, entryHeader.TotalDutyAmount);
		}

		public void TestLastAuditedInfos()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.Logs.AddNew(Events.RecordAudited, "test audit", new ZDateTimeOffset(2020, 1, 8));
			AssertEquals("LastAuditedDate", new ZDateTime(2020, 1, 8), entryHeader.LastAuditedDate);
			AssertEquals("LastAuditedUser", GlbStaff.CurrentUser.GS_FullName, entryHeader.LastAuditedUser);
		}

		public void TestHtmlFormatEntryData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var data1 = entryHeader.HtmlFormatEntryData;
			declaration.JE_MessageType = "EXP";
			var data2 = entryHeader.HtmlFormatEntryData;
			AssertEquals(data1, data2);
			entryHeader.MarkAsNeedingReloadHtmlFormatEntryData();
			var data3 = entryHeader.HtmlFormatEntryData;
			AssertNotEquals(data2, data3);
		}

		public void TestDocumentFieldExcludeFromMap_HtmlFormatEntryData()
		{
			var propertyInfo = typeof(CusEntryHeader).GetProperty("HtmlFormatEntryData");

			Assert(Attribute.IsDefined(propertyInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestDaysOfDelayedDeclaration()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			entryHeader.CH_JE = declaration.PK;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 3);
			AssertEquals(0, entryHeader.DaysOfDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 16));
			AssertEquals(0, entryHeader.DaysOfDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 17));
			AssertEquals(0, entryHeader.DaysOfDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 18));
			AssertEquals(1, entryHeader.DaysOfDelayedDeclaration);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 4);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 19));
			AssertEquals(1, entryHeader.DaysOfDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 20));
			AssertEquals(2, entryHeader.DaysOfDelayedDeclaration);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 5);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 21));
			AssertEquals(2, entryHeader.DaysOfDelayedDeclaration);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 10);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 2, 3));
			AssertEquals(10, entryHeader.DaysOfDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 4, 10));
			AssertEquals(77, entryHeader.DaysOfDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 4, 14));
			AssertEquals(81, entryHeader.DaysOfDelayedDeclaration);
		}

		[TestDate(2020, 4, 24)]
		public void TestDelayDeclarationTaxOrFee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertNull(entryHeader.DelayDeclarationTaxOrFee);
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateTaxOrFee("DDF", 0.0006m, "AU", 56m, 0m, "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Constants.UniversalReferenceConstants.RefCusTaxOrFee.DelayedFeeRate);
			helper.CreateTaxOrFee("VAT", 0.0007m, "CN", 57m, 0m, "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Constants.UniversalReferenceConstants.RefCusTaxOrFee.DelayedFeeRate);
			var taxOrFee = helper.CreateTaxOrFee("DDF", 0.0005m, "CN", 50m, 0m, "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Constants.UniversalReferenceConstants.RefCusTaxOrFee.DelayedFeeRate);
			newFactory.Save();
			AssertEquals(0.0005m, entryHeader.DelayDeclarationTaxOrFee.ZZF_Value);
			AssertEquals(50m, entryHeader.DelayDeclarationTaxOrFee.ZZF_Minimum);
			taxOrFee.ZZF_EndDate = new ZDateTime(2019, 6, 6);
			helper.CreateTaxOrFee("DDF", 0.0009m, "CN", 60m, 0m, "OTH", new ZDateTime(1990, 1, 1), new ZDateTime(2079, 6, 6), Constants.UniversalReferenceConstants.RefCusTaxOrFee.DelayedFeeRate);
			newFactory.Save();
			AssertEquals(0.0009m, entryHeader.DelayDeclarationTaxOrFee.ZZF_Value);
			AssertEquals(60m, entryHeader.DelayDeclarationTaxOrFee.ZZF_Minimum);
		}

		public void TestFeeForDelayedDeclaration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DDF", 0.0005m, "CN", 50m, 0m, "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Constants.UniversalReferenceConstants.RefCusTaxOrFee.DelayedFeeRate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine1.CL_CustomsValue = 65000m;
			entryLine2.CL_CustomsValue = 135000m;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 3);
			AssertEquals(0m, entryHeader.FeeForDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 16));
			AssertEquals(0m, entryHeader.FeeForDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 17));
			AssertEquals(0m, entryHeader.FeeForDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 18));
			AssertEquals(0m, entryHeader.FeeForDelayedDeclaration);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 4);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 19));
			AssertEquals(100m, entryHeader.FeeForDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 20));
			AssertEquals(200m, entryHeader.FeeForDelayedDeclaration);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 5);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 1, 21));
			AssertEquals(200m, entryHeader.FeeForDelayedDeclaration);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 10);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 2, 3));
			AssertEquals(100m, entryHeader.FeeForDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 4, 10));
			AssertEquals(6800m, entryHeader.FeeForDelayedDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", new ZDateTime(2020, 4, 14));
			AssertEquals(0m, entryHeader.FeeForDelayedDeclaration);
		}

		public void TestRemainingDaysForDeclaration()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", ZDateTime.Today);
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals(0, entryHeader.RemainingDaysForDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", ZDateTime.Empty);
			AssertEquals(0, entryHeader.RemainingDaysForDeclaration);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals(15, entryHeader.RemainingDaysForDeclaration);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-15);
			AssertEquals(-1, entryHeader.RemainingDaysForDeclaration);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-14);
			AssertEquals(1, entryHeader.RemainingDaysForDeclaration);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(10);
			AssertEquals(25, entryHeader.RemainingDaysForDeclaration);
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", ZDateTime.Today);
			AssertEquals(0, entryHeader.RemainingDaysForDeclaration);
		}

		public void TestGetDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				entryHeader.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;
				AssertEquals("CNCustomsDataRegistry false, XC_ClearanceMode ITD, GetDeclarationType should return 0", DeclarationTypeList.Codes.IntegratedDeclaration, entryHeader.GetDeclarationType());
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertEquals("CNCustomsDataRegistry false, XC_ClearanceMode TSD, GetDeclarationType should return 0", DeclarationTypeList.Codes.IntegratedDeclaration, entryHeader.GetDeclarationType());
			}

			declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("CNCustomsDataRegistry true, XC_ClearanceMode ITD, GetDeclarationType should return 0", DeclarationTypeList.Codes.IntegratedDeclaration, entryHeader.GetDeclarationType());
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertEquals("CNCustomsDataRegistry true, XC_ClearanceMode TSD, CH_Status AWM, GetDeclarationType should return 2", DeclarationTypeList.Codes.CompleteDeclaration, entryHeader.GetDeclarationType());
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepManual;
				AssertEquals("CNCustomsDataRegistry true, XC_ClearanceMode TSM, CH_Status AWM, GetDeclarationType should return 3", DeclarationTypeList.Codes.ManualDeclaration, entryHeader.GetDeclarationType());
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepAuto;
				AssertEquals("CNCustomsDataRegistry true, XC_ClearanceMode TSA, CH_Status AWM, GetDeclarationType should return 4", DeclarationTypeList.Codes.AutoDeclaration, entryHeader.GetDeclarationType());
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				entryHeader.CH_Status = JobMessageStatusList.Codes.AcknowledgedIntegratedDeclaration;
				AssertEquals("CNCustomsDataRegistry true, XC_ClearanceMode TSD, CH_Status ACO, GetDeclarationType should return 1", DeclarationTypeList.Codes.PreliminaryDeclaration, entryHeader.GetDeclarationType());
			}
		}
		public override void TestTotalDutyAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.ExportDuty);
			helper.LoadOrCreateNewCusRateCode(Factory, "EX1", expRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EX2", expRateType.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.GetOrAddFeeByFeeType("EX1").CF_ChargeAmount = 200m;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.GetOrAddFeeByFeeType("EX2").CF_ChargeAmount = 300m;
			entryLine2.Fees.GetOrAddFeeByFeeType(declaration.GSTOrVATCode).CF_ChargeAmount = 100m;

			AssertEquals("Total Duty Amount", 500m, entry.TotalDutyAmount);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryHeader to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestEntryChargeTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			AssertType<Registry.Business.Customs.ZZEntryChargeTypeList>(header.EntryChargeTypeList);
		}

		public void TestEntryNumberLookupFallback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("EntryNumber", ZString.Empty, entryHeader.EntryNumber);
			entryHeader.EntryNumber = "ENTA";
			AssertEquals("EntryNumber", "ENTA", entryHeader.EntryNumber);
			var number = entryHeader.CusEntryNumber;
			AssertEquals("CE_EntryNum", "ENTA", number.CE_EntryNum);
			AssertEquals("CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, number.CE_EntryType);
			number.Delete();
			var number1 = Factory.New<CusEntryNumber>();
			number1.CE_EntryType = JobMessageTypeList.Codes.Import;
			number1.CE_EntryNum = "ENT1";
			number1.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			number1.CE_ParentID = entryHeader.PK;
			number1.CE_ParentTable = entryHeader.TableName;
			var number2 = Factory.New<CusEntryNumber>();
			number2.CE_EntryType = JobMessageTypeList.Codes.Import;
			number2.CE_EntryNum = "ENT2";
			number2.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			number2.CE_ParentID = entryHeader.PK;
			number2.CE_ParentTable = entryHeader.TableName;
			var number3 = Factory.New<CusEntryNumber>();
			number3.CE_EntryType = JobMessageTypeList.Codes.Export;
			number3.CE_EntryNum = "ENT3";
			number3.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			number3.CE_ParentID = entryHeader.PK;
			number3.CE_ParentTable = entryHeader.TableName;
			var number4 = Factory.New<CusEntryNumber>();
			number4.CE_EntryType = JobMessageTypeList.Codes.Export;
			number4.CE_EntryNum = "ENT4";
			number4.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			number4.CE_ParentID = entryHeader.PK;
			number4.CE_ParentTable = entryHeader.TableName;
			AssertEquals("EntryNumber", "ENT2", entryHeader.EntryNumber);
			declaration.CustomsEntryHeaders.Add(entryHeader);
			AssertEquals("EntryNumber", "ENT4", entryHeader.EntryNumber);
			var number5 = Factory.New<CusEntryNumber>();
			number5.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number5.CE_EntryNum = "ENT5";
			number5.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			number5.CE_ParentID = entryHeader.PK;
			number5.CE_ParentTable = entryHeader.TableName;
			var number6 = Factory.New<CusEntryNumber>();
			number6.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number6.CE_EntryNum = "ENT6";
			number6.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			number6.CE_ParentID = entryHeader.PK;
			number6.CE_ParentTable = entryHeader.TableName;
			AssertEquals("EntryNumber", "ENT6", entryHeader.EntryNumber);
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("EntryNumber", "ENT6", entryHeader.EntryNumber);
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override bool RatesAreReciprocal => true;

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = testInstruction.PK;
			}
		}
	}
}
