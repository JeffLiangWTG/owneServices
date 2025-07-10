using System;
using System.Collections.Generic;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class GLBaseOnlineAggregatorTest : TestCaseWithFactory
	{
		public class MockGLBaseOnlineAggregator : GLBaseOnlineAggregator
		{
			public MockGLBaseOnlineAggregator(GLJournal gL, GLJournal originalGL)
				: base(gL, originalGL)
			{
			}

			public bool HasNaturalKeyChanged_Exposed(GLJournalLine line, GLJournalLine originalLine)
			{
				return base.HasNaturalKeyChanged(line, originalLine);
			}

			public int AddToPeriod_Exposed(int period, int increment)
			{
				return base.AddToPeriod(period, increment);
			}

			[SuppressWeaklyTypedCollectionMessage]
			public static List<GLJournal> SplitJournal_Exposed(GLJournal gL, int size)
			{
				return SplitJournal(gL, size);
			}

			protected internal override void GenerateUpdateCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL, GLJournal originalGL)
			{
			}

			protected internal override void GenerateReverseCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL)
			{
			}

			protected internal override void GenerateNewCommandsWithBizO(AggregatorDbCommandFactory dbCommandFactory, GLJournal gL)
			{
			}
		}

		public void TestAddToPeriod()
		{
			TestCaseHelper.ClearTable(MasterFiles.Business.AccPeriodManagement.Schema.TableName);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1), new ZDateTime(2003, 2, 28, 23, 59, 59));
			testHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200304, new ZDateTime(2003, 4, 1), new ZDateTime(2003, 4, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200305, new ZDateTime(2003, 5, 1), new ZDateTime(2003, 5, 31, 23, 59, 59));

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			GLJournal journal = testFactory.New(typeof(GLJournal)) as GLJournal;

			MockGLBaseOnlineAggregator testBaseAggregator = new MockGLBaseOnlineAggregator(journal, journal);

			AssertEquals(200305, testBaseAggregator.AddToPeriod_Exposed(200303, 2));
		}

		[ExpectNoExceptions]
		public void TestReAggregationSafe()
		{
			GLJournal gL = Factory.New(typeof(FCBAdjustmentJournal)) as GLJournal;
			gL.AH_TransactionType = TransactionTypes.GLStandardJournal;
			gL.AH_PostDate = Env.Time.CurrentLocalDate;

			gL.GLJournalLines.AddNew();
			gL.GLJournalLines.AddNew();
			gL.GLJournalLines.AddNew();

			gL.GLJournalLines[0].AL_OSExTaxAmount = 10.0m;
			gL.GLJournalLines[1].AL_OSExTaxAmount = 20.0m;
			gL.GLJournalLines[2].AL_OSExTaxAmount = 30.0m;

			ZGuid pK1 = gL.GLJournalLines[0].PK;
			ZGuid pK2 = gL.GLJournalLines[1].PK;
			ZGuid pK3 = gL.GLJournalLines[2].PK;

			MockGLBaseOnlineAggregator testBaseAggregator = new MockGLBaseOnlineAggregator(gL, gL);
			var splittedJournal = MockGLBaseOnlineAggregator.SplitJournal_Exposed(gL, 2);
		}

		public void TestHasNaturalKeyChanged()
		{
			TestCaseHelper.ClearTable(MasterFiles.Business.AccPeriodManagement.Schema.TableName);

			DateTime originalDate = new DateTime(2003, 2, 3);
			DateTime newDate = new DateTime(2003, 3, 3);

			DateTime originalReverseDate = new DateTime(2003, 4, 3);
			DateTime newReverseDate = new DateTime(2003, 5, 3);

			AccGLHeader account1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader account2 = Factory.NewWithValidTestData<AccGLHeader>();

			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch branch1 = currentCompany.Branches[0];
			GlbBranch branch2 = currentCompany.Branches[1];

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1), new ZDateTime(2003, 2, 28));
			testHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 30));
			testHelper.SetupSinglePeriod(200304, new ZDateTime(2003, 4, 1), new ZDateTime(2003, 4, 30));
			testHelper.SetupSinglePeriod(200305, new ZDateTime(2003, 5, 1), new ZDateTime(2003, 5, 30));

			BusinessObjectFactory testFactory = new BusinessObjectFactory();

			GLJournal journal = testFactory.New<GLJournal>();
			GLJournalLine line = testFactory.New<GLJournalLine>();
			GLJournalLine originalLine = testFactory.New<GLJournalLine>();

			line.AL_PostDate = newDate;
			originalLine.AL_PostDate = newDate;

			line.AL_ReverseDate = newReverseDate;
			originalLine.AL_ReverseDate = newReverseDate;

			line.AL_AG = account1.PK;
			originalLine.AL_AG = account1.PK;

			line.AL_GB = branch1.PK;
			originalLine.AL_GB = branch1.PK;

			MockGLBaseOnlineAggregator testBaseAggregator = new MockGLBaseOnlineAggregator(journal, journal);

			Assert(!testBaseAggregator.HasNaturalKeyChanged_Exposed(line, originalLine));

			originalLine.AL_AG = account2.PK;
			Assert(testBaseAggregator.HasNaturalKeyChanged_Exposed(line, originalLine));

			originalLine.AL_AG = account1.PK;
			line.AL_PostDate = newDate;
			originalLine.AL_PostDate = originalDate;
			Assert(testBaseAggregator.HasNaturalKeyChanged_Exposed(line, originalLine));
		}

		public void TestGLJournalSplit()
		{
			GLJournal gL = Factory.New(typeof(GLJournal)) as GLJournal;

			gL.AH_TransactionType = TransactionTypes.GLStandardJournal;
			gL.AH_PostDate = Env.Time.CurrentLocalDate;

			gL.GLJournalLines.AddNew();
			gL.GLJournalLines.AddNew();
			gL.GLJournalLines.AddNew();
			gL.GLJournalLines.AddNew();
			gL.GLJournalLines.AddNew();

			gL.GLJournalLines[0].AL_OSExTaxAmount = 10.0m;
			gL.GLJournalLines[1].AL_OSExTaxAmount = 20.0m;
			gL.GLJournalLines[2].AL_OSExTaxAmount = 30.0m;
			gL.GLJournalLines[3].AL_OSExTaxAmount = 40.0m;
			gL.GLJournalLines[4].AL_OSExTaxAmount = 50.0m;

			ZGuid pK1 = gL.GLJournalLines[0].PK;
			ZGuid pK2 = gL.GLJournalLines[1].PK;
			ZGuid pK3 = gL.GLJournalLines[2].PK;
			ZGuid pK4 = gL.GLJournalLines[3].PK;
			ZGuid pK5 = gL.GLJournalLines[4].PK;

			MockGLBaseOnlineAggregator testBaseAggregator = new MockGLBaseOnlineAggregator(gL, gL);
			var splittedJournal = MockGLBaseOnlineAggregator.SplitJournal_Exposed(gL, 2);

			AssertEquals(3, splittedJournal.Count);
			AssertEquals(2, splittedJournal[0].GLJournalLines.Count);
			AssertEquals(2, splittedJournal[1].GLJournalLines.Count);
			AssertEquals(1, splittedJournal[2].GLJournalLines.Count);

			AssertEquals(pK1, splittedJournal[0].GLJournalLines[0].PK);
			AssertEquals(pK2, splittedJournal[0].GLJournalLines[1].PK);
			AssertEquals(pK3, splittedJournal[1].GLJournalLines[0].PK);
			AssertEquals(pK4, splittedJournal[1].GLJournalLines[1].PK);
			AssertEquals(pK5, splittedJournal[2].GLJournalLines[0].PK);

			AssertEquals(10.0m, splittedJournal[0].GLJournalLines[0].AL_OSExTaxAmount);
			AssertEquals(20.0m, splittedJournal[0].GLJournalLines[1].AL_OSExTaxAmount);
			AssertEquals(30.0m, splittedJournal[1].GLJournalLines[0].AL_OSExTaxAmount);
			AssertEquals(40.0m, splittedJournal[1].GLJournalLines[1].AL_OSExTaxAmount);
			AssertEquals(50.0m, splittedJournal[2].GLJournalLines[0].AL_OSExTaxAmount);
		}
	}
}