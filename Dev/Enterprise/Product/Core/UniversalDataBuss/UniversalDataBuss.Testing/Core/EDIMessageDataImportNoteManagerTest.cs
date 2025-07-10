using System;
using System.IO;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Testing.Core
{
	public class EDIMessageDataImportNoteManagerTest : TestCaseWithFactory
	{
		public void TestAddNewDataImportLogNote()
		{
			using (Factory.AddDisposableService())
			{
				var originalDatabaseCount = Factory.GetDatabaseCount(typeof(StmNote));
				var testEDIMessage = Factory.NewWithValidTestData<EDIMessage>();
				var noteLength = DataImportNoteCreater.NoteSwitchToFileLimitInBytes + 3;

				Action<Stream> largeSizeNoteAction = (stream) =>
				{
					for (int i = 0; i < noteLength; i++)
					{
						stream.WriteByte(65);
					}
					stream.Flush();
					stream.Position = 0;
				};
				DataImportNoteCreater.AddNew(testEDIMessage, largeSizeNoteAction);
				testEDIMessage.Factory.Save();
				var stmNotesInFactory = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, SQLComparisonOperator.NotEqual, "")); //since it's a contrived unit test anyway
				AssertEquals("incorrect stmnote count in DB", originalDatabaseCount + 1, Factory.GetDatabaseCount(typeof(StmNote)));
				AssertEquals("incorrect stmnote count in Factory", originalDatabaseCount + 1, stmNotesInFactory.Length);
				AssertEquals("incorrect stmnote created for the edimessage", 1, stmNotesInFactory.Count(x => x.ST_Table == "EDIMessage" && x.ST_ParentID == testEDIMessage.PK && x.ST_Description == PredefinedNoteTypes.Instance.DataImportLogNote.Description && x.ST_NoteText.Length == noteLength && x.ST_NoteText.StartsWith("AAAA") && x.ST_GC_RelatedCompany.IsEmpty));

				Action<Stream> smallSizeNoteAction = (stream) =>
				{
					for (int i = 0; i < DataImportNoteCreater.NoteSwitchToFileLimitInBytes / 10; i++)
					{
						stream.WriteByte(65);
					}
					stream.Flush();
					stream.Position = 0;
				};
				DataImportNoteCreater.AddNew(testEDIMessage, smallSizeNoteAction);
				var reloadFactory = new BusinessObjectFactory();
				AssertEquals("incorrect stmnote count in DB", originalDatabaseCount + 1, Factory.GetDatabaseCount(typeof(StmNote)));
				AssertEquals("incorrect stmnote count in Factory", originalDatabaseCount + 2, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, SQLComparisonOperator.NotEqual, "")).Length); //same
			}
		}
	}

	public class EDIMessageDataImportNoteManagerTestNonTransactioned : TestCase
	{
		[UseSnapshotProtection]
		public void TestAddNewDataImportLogNoteCanBeSavedIfTransactionRolledBack()
		{
			var testFactory = new BusinessObjectFactory() { NameForDebugging = "message factory" };
			using (testFactory.AddDisposableService())
			{
				var message = testFactory.NewWithValidTestData<EDIMessage>();
				message.EM_Status = EDIMessageStatusList.Codes.Queued;
				testFactory.Save();

				using (message.Factory.DelayedTransaction())
				{
					Action<Stream> largeSizeNoteAction = (stream) =>
					{
						for (int i = 0; i < DataImportNoteCreater.NoteSwitchToFileLimitInBytes + 3; i++)
						{
							stream.WriteByte(65);
						}
						stream.Flush();
						stream.Position = 0;
					};

					message.EM_Status = EDIMessageStatusList.Codes.Error;
					DataImportNoteCreater.AddNew(message, largeSizeNoteAction);
				}
				message.Factory.Save();
				message.Reload();

				CombineAssertions(() =>
				{
					AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
					AssertEquals("Note changes should be saved", 1, message.Notes.GetAllNotes().Count(n => ((StmNote)n).ST_NoteText.StartsWith("AAAA")));
				});
			}
		}
	}
}
