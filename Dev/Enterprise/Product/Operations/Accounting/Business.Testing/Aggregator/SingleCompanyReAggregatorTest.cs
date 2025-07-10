using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class SingleCompanyReAggregatorTest : ReAggregatorTest
	{
		public override void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_GJL()
		{
			CoreTestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany(TransactionTypes.GLStandardJournal);
		}

		public override void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_AJL()
		{
			CoreTestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany(TransactionTypes.GLAutoJournal);
		}

		public override void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_RJL()
		{
			CoreTestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany(TransactionTypes.GLReversingJournal);
		}

		public override void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_NJL()
		{
			CoreTestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany(TransactionTypes.GLNoteJournal);
		}

		void CoreTestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany(ZString journalType)
		{
			SetupTestData(journalType);

			((IReAggregator)new ReAggregator()).ReAggregate();

			var originalAggregateLinePksForABCCompany = GetAggregateLinePksForCompany(journalType, BranchABC);
			var originalAggregateLinePksForXYZCompany = GetAggregateLinePksForCompany(journalType, BranchXYZ);

			var postToGLMarker = "X";
			var factoryToSetPostToGLFlagForTest = Factory.CreateNewFactory();
			var journalsForAllCompanies = factoryToSetPostToGLFlagForTest.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, journalType));
			foreach (var journal in journalsForAllCompanies)
			{
				journal.AH_PostToGL = postToGLMarker;
				journal.Lines[0].AL_PostToGL = postToGLMarker;
				journal.Lines[0].AL_ReverseToGL = postToGLMarker;
				journal.Lines[1].AL_PostToGL = postToGLMarker;
				journal.Lines[1].AL_ReverseToGL = postToGLMarker;
			}
			factoryToSetPostToGLFlagForTest.Save();

			ReAggregator.ClearAggregate();

			var newFactory = Factory.CreateNewFactory();
			var aggregateLinesForABCCompany = newFactory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_GC, CompanyABC.PK));
			AssertEquals("The current company's aggregate table should be cleared", 0, aggregateLinesForABCCompany.Length);
			var aggregateLinesForXYZCompany = newFactory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_GC, CompanyXYZ.PK));
			AssertNotEquals("The other company's aggregate table should not be cleared", 0, aggregateLinesForXYZCompany);

			ReAggregator.UpdateFlags();
			var factoryToReloadAfterUpdatingPostedFlags = new BusinessObjectFactory();
			journalsForAllCompanies = factoryToSetPostToGLFlagForTest.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, journalType));
			foreach (var journal in journalsForAllCompanies)
			{
				AssertEquals("AH_PostToGL", journal.AH_GC == CompanyABC.PK ? Core.Constants.BooleanFalseString : postToGLMarker, journal.AH_PostToGL);
				AssertEquals("Lines[0].AL_PostToGL", journal.AH_GC == CompanyABC.PK ? Core.Constants.BooleanFalseString : postToGLMarker, journal.Lines[0].AL_PostToGL);
				AssertEquals("Lines[1].AL_PostToGL", journal.AH_GC == CompanyABC.PK ? Core.Constants.BooleanFalseString : postToGLMarker, journal.Lines[1].AL_PostToGL);
			}

			ReAggregator.ReAggregateGL();

			var newAggregateLinePksForABCCompany = GetAggregateLinePksForCompany(journalType, BranchABC);
			var newAggregateLinePksForXYZCompany = GetAggregateLinePksForCompany(journalType, BranchXYZ);

			AssertContainsExactElementsInAnyOrder("Should be the same Aggregate row for journals in XYZ company", originalAggregateLinePksForXYZCompany, newAggregateLinePksForXYZCompany);
			originalAggregateLinePksForABCCompany.ForEach(p => AssertCollectionNotContains("Should be a new Aggregate row for for journals in ABC company", p, newAggregateLinePksForABCCompany));
		}

		List<ZGuid> GetAggregateLinePksForCompany(ZString journalType, GlbBranch branch)
		{
			switch (journalType)
			{
				case TransactionTypes.GLStandardJournal:
					if (branch.PK == BranchABC.PK)
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 210m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -210m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 120m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -120m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 130m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -130m).PK
						};
					}
					else
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 710m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -710m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 370m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -370m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 380m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -380m).PK
						};
					}

				case TransactionTypes.GLNoteJournal:
					if (branch.PK == BranchABC.PK)
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchABC, glHeaderNTE1, 250m).PK,
							FindAggregateRecord(BranchABC, glHeaderNTE2, -200m).PK,
							FindAggregateRecord(BranchABC, glHeaderNTE1, 150m).PK
						};
					}
					else
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchXYZ, glHeaderNTE1, 350m).PK,
							FindAggregateRecord(BranchXYZ, glHeaderNTE2, -800m).PK,
							FindAggregateRecord(BranchXYZ, glHeaderNTE2, -150m).PK
						};
					}

				case TransactionTypes.GLReversingJournal:
					if (branch.PK == BranchABC.PK)
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 210m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, -250m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -210m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, 250m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, -100m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, 100m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, -110m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, 110m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 120m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -120m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 130m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -130m).PK
						};
					}
					else
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 710m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, -750m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -710m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, 750m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, -350m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, 350m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, -360m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, 360m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 370m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -370m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 380m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -380m).PK
						};
					}

				case TransactionTypes.GLAutoJournal:
					if (branch.PK == BranchABC.PK)
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 460m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -460m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 210m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -210m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 110m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -110m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 250m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -250m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 130m).PK,
							FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -130m).PK
						};
					}
					else
					{
						return new List<ZGuid>()
						{
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 1460m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -1460m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 710m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -710m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 360m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -360m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 750m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -750m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 380m).PK,
							FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -380m).PK
						};
					}

				default:
					return new List<ZGuid>();
			}
		}

		protected override ZGuid[] GetExpectedAccCashBasisVATQueue(AccCashBasisVAT[] accCashBasisVATs) //only 1 company
		{
			return accCashBasisVATs.Where(cash => cash.YC_GC == CompanyPK).Select(cash => cash.PK).ToArray();
		}

		protected override ZGuid[] GetExpectedAccTaxGLMovementQueuePKs(AccTaxGLMovement[] accTaxGLMovements)
		{
			return accTaxGLMovements.Where(x => x.TaxTransaction.ATT_GC == CompanyPK).Select(x => x.PK).ToArray();
		}

		protected override ReAggregator GetNewReAggregatorForTest()
		{
			return new SingleCompanyReAggregator(CompanyABC.PK);
		}

		protected override Guid CompanyPK
		{
			get { return CompanyABC.PK.ToGuid(); }
		}
	}
}
