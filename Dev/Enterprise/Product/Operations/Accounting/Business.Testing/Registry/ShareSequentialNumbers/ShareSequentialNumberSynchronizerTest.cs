using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	class ShareSequentialNumberSynchronizerTest : TestCaseWithFactory
	{
		public void TestSynchronizeNumberGenerators()
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				var apINVFountain = Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain();
				var apCRDFountain = Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain();
				var apADJFountain = Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain();

				var source = new List<INumberFountainProxy>() { apINVFountain, apCRDFountain };

				apINVFountain.SetNext(Db.Connection, 1100);
				apCRDFountain.SetNext(Db.Connection, 1200);
				apADJFountain.SetNext(Db.Connection, 1305);

				AssertEquals("Pre-condition", 1100, apINVFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1200, apCRDFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1305, apADJFountain.PeekPreliminary(Db.Connection));

				ShareSequentialNumberSynchronizer.SynchronizeNumberGenerators(source, Factory);

				AssertEquals("Fountain in source list should be synchonized", 1200, apINVFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1200, apCRDFountain.PeekPreliminary(Db.Connection));
				AssertEquals(1305, apADJFountain.PeekPreliminary(Db.Connection));
			}
		}

		public void TestCreateShareSequentialReferenceNumbers()
		{
			AssertEquals(true, ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true).Value);
			AssertEquals(false, ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(false).Value);
		}
	}
}
