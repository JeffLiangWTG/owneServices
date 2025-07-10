using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntrySnapshot))]
	sealed class CusEntrySnapshotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			return entry.Snapshots.AddNew();
		}

		public void TestGetDataProviderObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._5DP);
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Current);
			AssertEquals(typeof(LocalExportEntryHeader), snapshot.DataProviderObject.GetType());

			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Current);
			AssertEquals(typeof(LocalExportEntryHeader), snapshot.DataProviderObject.GetType());

			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._830);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Current);
			AssertEquals(typeof(ExportEntryHeader), snapshot.DataProviderObject.GetType());

			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Current);
			AssertEquals(typeof(ImportEntryHeader), snapshot.DataProviderObject.GetType());

			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._DHR);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._DHR, EntrySnapshotStatus.Current);
			AssertEquals(typeof(ImportDHRHeader), snapshot.DataProviderObject.GetType());

			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._5BA);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5BA, EntrySnapshotStatus.Current);
			AssertEquals(typeof(Import5BAHeader), snapshot.DataProviderObject.GetType());

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			CreateEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._5SC);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5SC, EntrySnapshotStatus.Current);
			AssertEquals(typeof(ImportFTAHeader), snapshot.DataProviderObject.GetType());
		}

		void CreateEntrySnapshot(CusEntryHeader entry, ZString messageType)
		{
			if (messageType == ElectronicDocumentTypeList.Codes._5DP)
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
			else if (messageType == ElectronicDocumentTypeList.Codes._830)
			{
				var header = new ExportEntryHeaderCreator().Create(entry);
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
			else if (messageType == ElectronicDocumentTypeList.Codes._DHR)
			{
				var header = new ImportDHRCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._5SC)
			{
				var header = new ImportFTACreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
			else if (messageType == ElectronicDocumentTypeList.Codes._5BA)
			{
				var header = new Import5BAHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
					Factory.Save();
				}
			}
		}
	}
}


