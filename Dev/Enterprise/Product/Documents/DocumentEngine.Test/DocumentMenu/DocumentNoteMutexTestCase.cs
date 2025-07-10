using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Data.Mutex;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[UseSnapshotProtection]
	sealed class DocumentNoteMutexTestCase : TestCase
	{
		public void TestLoadNoteWithExclusiveMutexTakesMutexAndReleasesOnDispose()
		{
			var note = helper.GetNewDocumentNote();
			try
			{
				AssertMutexesAcquired(note.mutexes, shouldBeAcquired: true);

				note.Dispose();
				AssertEquals("Mutexes should have been disposed when DocumentNote disposed", 0, note.mutexes.Count);
			}
			finally
			{
				note.mutexes.Dispose();
				note.Delete();
				factory.Save();
			}
		}

		public void TestLoadNoteWithExclusiveMutexOnSecondDbConnectionWillNotAcquireMutex()
		{
			DocumentNote note1 = null, note2 = null;
			BusinessObjectFactory factoryOnAnotherConnection = null;
			try
			{
				note1 = helper.GetNewDocumentNote();
				AssertMutexesAcquired(note1.mutexes, shouldBeAcquired: true);

				using (var newConnection = Db.NewAdminConnection())
				{
					factoryOnAnotherConnection = new BusinessObjectFactory(newConnection);
					var consol = factoryOnAnotherConnection.Load<DummyConsolBusinessObject>(note1.ST_ParentID);
					note2 = helper.GetNewDocumentNote(factoryOnAnotherConnection, consol);

					AssertNull("Since we can't get a lock, we shouldn't get a DocumentNote", note2);
				}
			}
			finally
			{
				note1.Dispose();
				note1.Delete();
				factory.Save();

				if (note2 != null)
				{
					note2.Dispose();
					note2.Delete();
					factoryOnAnotherConnection.Save();
				}
			}
		}

		void AssertMutexesAcquired(IList<IDisposable> mutexes, bool shouldBeAcquired)
		{
			Assert(string.Format("Should {0}have created mutexes when creating DocumentNote", shouldBeAcquired ? "" : "not "),
				mutexes.Count > 0);

			var badMutex = mutexes.Cast<ZGlobalMutex>().FirstOrDefault(mutex => mutex.HasLock != shouldBeAcquired);

			if (badMutex != null)
			{
				Fail(string.Format("Mutex [{0}] should {1}have been acquired", badMutex.MutexID, shouldBeAcquired ? "" : "not "));
			}
		}

		DocumentNoteTestHelper helper;
		BusinessObjectFactory factory;

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
			helper = new DocumentNoteTestHelper(factory);
		}
	}
}
