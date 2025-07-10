using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class ReAggregatorTest : TestCaseWithFactory
	{
		[SuspendCriticalValidation]
		public void TestUnMarkHeadersAsPosted()
		{
			var inv = Factory.New<ARInvoice>();
			inv.AH_GC = CompanyPK;
			var line = (ARInvoiceLine)inv.Lines.AddNew();
			line.AL_GC = CompanyPK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100m;
			inv.AH_PostToGL = Core.Constants.BooleanTrueString;

			Factory.Save();

			AssertEquals("Precondition: AH_PostToGL", Core.Constants.BooleanTrueString, inv.AH_PostToGL);

			ReleaseFactory();

			var reAggregator = GetNewReAggregatorForTest();
			((IReAggregator)reAggregator).ReAggregate();

			inv = Factory.Load<ARInvoice>(inv.PK);
			AssertEquals("AH_PostToGL", Core.Constants.BooleanFalseString, inv.AH_PostToGL);
		}

		[SuspendCriticalValidation]
		public void TestUnMarkLinesAsPosted()
		{
			var wip = Factory.New<WIP>();
			wip.AL_GC = CompanyPK;
			wip.AL_AC = TestObjectCreator.CC1.PK;
			wip.AL_OSExTaxAmount = 100m;
			wip.AL_PostToGL = Core.Constants.BooleanTrueString;
			wip.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			wip.AL_ReverseDate = ZDateTime.Now;

			var oldWIPInDBBeforeTransformation = Factory.New<WIP>();
			oldWIPInDBBeforeTransformation.AL_GC = CompanyPK;
			oldWIPInDBBeforeTransformation.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			oldWIPInDBBeforeTransformation.AL_OSExTaxAmount = 0m;
			oldWIPInDBBeforeTransformation.AL_PostToGL = Core.Constants.BooleanTrueString;
			oldWIPInDBBeforeTransformation.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			oldWIPInDBBeforeTransformation.AL_ReverseDate = ZDateTime.Now;

			var inv = Factory.New<ARInvoice>();
			inv.AH_GC = CompanyPK;
			var invoiceLine = (TransactionLine)inv.Lines.AddNew();
			invoiceLine.AL_GC = CompanyPK;
			invoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			invoiceLine.AL_PostToGL = Core.Constants.BooleanTrueString;
			invoiceLine.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			invoiceLine.AL_ReverseDate = ZDateTime.Now;

			var commentInvoiceLine = (TransactionLine)inv.Lines.AddNew();
			commentInvoiceLine.AL_GC = CompanyPK;
			commentInvoiceLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			commentInvoiceLine.AL_PostToGL = Core.Constants.BooleanTrueString;
			commentInvoiceLine.AL_ReverseToGL = Core.Constants.BooleanTrueString;
			commentInvoiceLine.AL_ReverseDate = ZDateTime.Now;

			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionLines_WIPACRMustHaveGLAccount ON AccTransactionLines");

			Factory.Save();

			TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_AccTransactionLines_WIPACRMustHaveGLAccount ON AccTransactionLines;");

			AssertNotEquals("Precondition: wip.AL_AG", ZGuid.Empty, wip.AL_AG);
			AssertEquals("Precondition: wip.AL_PostToGL", Core.Constants.BooleanTrueString, wip.AL_PostToGL);
			AssertEquals("Precondition: wip.AL_ReverseToGL", Core.Constants.BooleanTrueString, wip.AL_ReverseToGL);

			AssertEquals("Precondition: oldWIPInDBBeforeTransformation.AL_AG", ZGuid.Empty, oldWIPInDBBeforeTransformation.AL_AG);
			AssertEquals("Precondition: oldWIPInDBBeforeTransformation.AL_PostToGL", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_PostToGL);
			AssertEquals("Precondition: oldWIPInDBBeforeTransformation.AL_ReverseToGL", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_ReverseToGL);

			AssertNotEquals("Precondition: invoiceLine.AL_AG", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("Precondition: invoiceLine.AL_PostToGL", Core.Constants.BooleanTrueString, invoiceLine.AL_PostToGL);
			AssertEquals("Precondition: invoiceLine.AL_ReverseToGL", Core.Constants.BooleanTrueString, invoiceLine.AL_ReverseToGL);

			AssertEquals("Precondition: commentInvoiceLine.AL_AG", ZGuid.Empty, commentInvoiceLine.AL_AG);
			AssertEquals("Precondition: commentInvoiceLine.AL_PostToGL", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_PostToGL);
			AssertEquals("Precondition: commentInvoiceLine.AL_ReverseToGL", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_ReverseToGL);

			ReleaseFactory();

			var reAggregator = GetNewReAggregatorForTest();
			((IReAggregator)reAggregator).ReAggregate();

			wip = Factory.Load<WIP>(wip.PK);
			AssertEquals("wip.AL_PostToGL", Core.Constants.BooleanFalseString, wip.AL_PostToGL);
			AssertEquals("wip.AL_ReverseToGL", Core.Constants.BooleanFalseString, wip.AL_ReverseToGL);

			oldWIPInDBBeforeTransformation = Factory.Load<WIP>(oldWIPInDBBeforeTransformation.PK);
			AssertEquals("oldWIPInDBBeforeTransformation.AL_PostToGL remains untouched", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_PostToGL);
			AssertEquals("oldWIPInDBBeforeTransformation.AL_ReverseToGL remains untouched", Core.Constants.BooleanTrueString, oldWIPInDBBeforeTransformation.AL_ReverseToGL);

			invoiceLine = Factory.Load<TransactionLine>(invoiceLine.PK);
			AssertEquals("invoiceLine.AL_PostToGL", Core.Constants.BooleanFalseString, invoiceLine.AL_PostToGL);
			AssertEquals("invoiceLine.AL_ReverseToGL", Core.Constants.BooleanFalseString, invoiceLine.AL_ReverseToGL);

			commentInvoiceLine = Factory.Load<TransactionLine>(commentInvoiceLine.PK);
			AssertEquals("commentInvoiceLine.AL_PostToGL remains untouched", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_PostToGL);
			AssertEquals("commentInvoiceLine.AL_ReverseToGL remains untouched", Core.Constants.BooleanTrueString, commentInvoiceLine.AL_ReverseToGL);
		}

		public virtual void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_GJL()
		{
			SetupTestData(TransactionTypes.GLStandardJournal);
			var aggregateLineForABCGlHeader1_200704 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 210m);
			var aggregateLineForABCGlHeader2_200704 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -210m);
			var aggregateLineForABCGlHeader1_200705 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 120m);
			var aggregateLineForABCGlHeader2_200705 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -120m);
			var aggregateLineForABCGlHeader1_200703 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 130m);
			var aggregateLineForABCGlHeader2_200703 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -130m);

			AssertEquals(200704, aggregateLineForABCGlHeader1_200704.AA_Period);
			AssertEquals(200704, aggregateLineForABCGlHeader2_200704.AA_Period);
			AssertEquals(200703, aggregateLineForABCGlHeader1_200703.AA_Period);
			AssertEquals(200703, aggregateLineForABCGlHeader2_200703.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader1_200705.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader2_200705.AA_Period);

			var aggregateLineForXYZGlHeader1_200610 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 710m);
			var aggregateLineForXYZGlHeader2_200610 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -710m);
			var aggregateLineForXYZGlHeader1_200611 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 370m);
			var aggregateLineForXYZGlHeader2_200611 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -370m);
			var aggregateLineForXYZGlHeader1_200609 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 380m);
			var aggregateLineForXYZGlHeader2_200609 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -380m);

			AssertEquals(200610, aggregateLineForXYZGlHeader1_200610.AA_Period);
			AssertEquals(200610, aggregateLineForXYZGlHeader2_200610.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader1_200611.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader2_200611.AA_Period);
			AssertEquals(200609, aggregateLineForXYZGlHeader1_200609.AA_Period);
			AssertEquals(200609, aggregateLineForXYZGlHeader2_200609.AA_Period);
		}

		public virtual void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_RJL()
		{
			SetupTestData(TransactionTypes.GLReversingJournal);

			var aggregateLineForABCGlHeader1_200704_start = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 210m);
			var aggregateLineForABCGlHeader1_200704_end = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, -250m);
			var aggregateLineForABCGlHeader2_200704_start = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -210m);
			var aggregateLineForABCGlHeader2_200704_end = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, 250m);
			var aggregateLineForABCGlHeader1_200705 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, -100m);
			var aggregateLineForABCGlHeader2_200705 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, 100m);
			var aggregateLineForABCGlHeader1_200706 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, -110m);
			var aggregateLineForABCGlHeader2_200706 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, 110m);
			var aggregateLineForABCGlHeader1_200703 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 120m);
			var aggregateLineForABCGlHeader2_200703 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -120m);
			var aggregateLineForABCGlHeader1_200702 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 130m);
			var aggregateLineForABCGlHeader2_200702 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -130m);

			AssertEquals(200704, aggregateLineForABCGlHeader1_200704_start.AA_Period);
			AssertEquals(200704, aggregateLineForABCGlHeader1_200704_end.AA_Period);
			AssertEquals(200704, aggregateLineForABCGlHeader2_200704_start.AA_Period);
			AssertEquals(200704, aggregateLineForABCGlHeader2_200704_end.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader1_200705.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader2_200705.AA_Period);
			AssertEquals(200706, aggregateLineForABCGlHeader1_200706.AA_Period);
			AssertEquals(200706, aggregateLineForABCGlHeader2_200706.AA_Period);
			AssertEquals(200703, aggregateLineForABCGlHeader1_200703.AA_Period);
			AssertEquals(200703, aggregateLineForABCGlHeader2_200703.AA_Period);
			AssertEquals(200702, aggregateLineForABCGlHeader1_200702.AA_Period);
			AssertEquals(200702, aggregateLineForABCGlHeader2_200702.AA_Period);

			var aggregateLineForXYZGlHeader1_200610_start = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 710m);
			var aggregateLineForXYZGlHeader1_200610_end = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, -750m);
			var aggregateLineForXYZGlHeader2_200610_start = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -710m);
			var aggregateLineForXYZGlHeader2_200610_end = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, 750m);
			var aggregateLineForXYZGlHeader1_200611 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, -350m);
			var aggregateLineForXYZGlHeader2_200611 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, 350m);
			var aggregateLineForXYZGlHeader1_200612 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, -360m);
			var aggregateLineForXYZGlHeader2_200612 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, 360m);
			var aggregateLineForXYZGlHeader1_200609 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 370m);
			var aggregateLineForXYZGlHeader2_200609 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -370m);
			var aggregateLineForXYZGlHeader1_200608 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 380m);
			var aggregateLineForXYZGlHeader2_200608 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -380m);

			AssertEquals(200610, aggregateLineForXYZGlHeader1_200610_start.AA_Period);
			AssertEquals(200610, aggregateLineForXYZGlHeader1_200610_end.AA_Period);
			AssertEquals(200610, aggregateLineForXYZGlHeader2_200610_start.AA_Period);
			AssertEquals(200610, aggregateLineForXYZGlHeader2_200610_end.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader1_200611.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader2_200611.AA_Period);
			AssertEquals(200612, aggregateLineForXYZGlHeader1_200612.AA_Period);
			AssertEquals(200612, aggregateLineForXYZGlHeader2_200612.AA_Period);
			AssertEquals(200609, aggregateLineForXYZGlHeader1_200609.AA_Period);
			AssertEquals(200609, aggregateLineForXYZGlHeader2_200609.AA_Period);
			AssertEquals(200608, aggregateLineForXYZGlHeader1_200608.AA_Period);
			AssertEquals(200608, aggregateLineForXYZGlHeader2_200608.AA_Period);
		}

		public virtual void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_AJL()
		{
			SetupTestData(TransactionTypes.GLAutoJournal);

			var aggregateLineForABCGlHeader1_200704 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 460m);
			var aggregateLineForABCGlHeader2_200704 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -460m);
			var aggregateLineForABCGlHeader1_200705 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 210m);
			var aggregateLineForABCGlHeader2_200705 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -210m);
			var aggregateLineForABCGlHeader1_200706 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 110m);
			var aggregateLineForABCGlHeader2_200706 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -110m);
			var aggregateLineForABCGlHeader1_200703 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 250m);
			var aggregateLineForABCGlHeader2_200703 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -250m);
			var aggregateLineForABCGlHeader1_200702 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader1, 130m);
			var aggregateLineForABCGlHeader2_200702 = FindAggregateRecord(BranchABC, TestObjectCreator.GLHeader2, -130m);

			AssertEquals(200704, aggregateLineForABCGlHeader1_200704.AA_Period);
			AssertEquals(200704, aggregateLineForABCGlHeader2_200704.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader1_200705.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader2_200705.AA_Period);
			AssertEquals(200706, aggregateLineForABCGlHeader1_200706.AA_Period);
			AssertEquals(200706, aggregateLineForABCGlHeader2_200706.AA_Period);
			AssertEquals(200703, aggregateLineForABCGlHeader1_200703.AA_Period);
			AssertEquals(200703, aggregateLineForABCGlHeader2_200703.AA_Period);
			AssertEquals(200702, aggregateLineForABCGlHeader1_200702.AA_Period);
			AssertEquals(200702, aggregateLineForABCGlHeader2_200702.AA_Period);

			var aggregateLineForXYZGlHeader1_200610 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 1460m);
			var aggregateLineForXYZGlHeader2_200610 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -1460m);
			var aggregateLineForXYZGlHeader1_200611 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 710m);
			var aggregateLineForXYZGlHeader2_200611 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -710m);
			var aggregateLineForXYZGlHeader1_200612 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 360m);
			var aggregateLineForXYZGlHeader2_200612 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -360m);
			var aggregateLineForXYZGlHeader1_200609 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 750m);
			var aggregateLineForXYZGlHeader2_200609 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -750m);
			var aggregateLineForXYZGlHeader1_200608 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader1, 380m);
			var aggregateLineForXYZGlHeader2_200608 = FindAggregateRecord(BranchXYZ, TestObjectCreator.GLHeader2, -380m);

			AssertEquals(200610, aggregateLineForXYZGlHeader1_200610.AA_Period);
			AssertEquals(200610, aggregateLineForXYZGlHeader2_200610.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader1_200611.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader2_200611.AA_Period);
			AssertEquals(200612, aggregateLineForXYZGlHeader1_200612.AA_Period);
			AssertEquals(200612, aggregateLineForXYZGlHeader2_200612.AA_Period);
			AssertEquals(200609, aggregateLineForXYZGlHeader1_200609.AA_Period);
			AssertEquals(200609, aggregateLineForXYZGlHeader2_200609.AA_Period);
			AssertEquals(200608, aggregateLineForXYZGlHeader1_200608.AA_Period);
			AssertEquals(200608, aggregateLineForXYZGlHeader2_200608.AA_Period);
		}

		public virtual void TestReAggregationPostsJournalsIntoCorrectPeriodForEachCompany_NJL()
		{
			SetupTestData(TransactionTypes.GLNoteJournal);

			var aggregateLineForABCGlHeader1_200704 = FindAggregateRecord(BranchABC, glHeaderNTE1, 250m);
			var aggregateLineForABCGlHeader2_200704 = FindAggregateRecord(BranchABC, glHeaderNTE2, -200m);
			var aggregateLineForABCGlHeader1_200705 = FindAggregateRecord(BranchABC, glHeaderNTE1, 150m);

			var aggregateLineForXYZGlHeader1_200610 = FindAggregateRecord(BranchXYZ, glHeaderNTE1, 350m);
			var aggregateLineForXYZGlHeader2_200610 = FindAggregateRecord(BranchXYZ, glHeaderNTE2, -800m);
			var aggregateLineForXYZGlHeader2_200611 = FindAggregateRecord(BranchXYZ, glHeaderNTE2, -150m);

			AssertEquals("Transaction should be aggregated correctly by period", 200704, aggregateLineForABCGlHeader1_200704.AA_Period);
			AssertEquals(200704, aggregateLineForABCGlHeader2_200704.AA_Period);
			AssertEquals(200705, aggregateLineForABCGlHeader1_200705.AA_Period);

			AssertEquals(200610, aggregateLineForXYZGlHeader1_200610.AA_Period);
			AssertEquals(200610, aggregateLineForXYZGlHeader2_200610.AA_Period);
			AssertEquals(200611, aggregateLineForXYZGlHeader2_200611.AA_Period);
		}

		[SuspendCriticalValidation]
		public void TestReaggregationReQueueCashVAT()
		{
			//precondition
			var invoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			invoice1.AH_GC = CompanyPK;
			var line1 = invoice1.Lines[0];
			line1.AL_GC = CompanyPK;
			line1.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var cashVATRecord1 = TestObjectCreator.CreateCashBasisVAT(invoice1.Lines[0], -100, -10);
			cashVATRecord1.YC_GC = CompanyPK;
			Factory.Save();

			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 2, 200, 20, 200, 20);
			invoice2.AH_FullyPaidDate = ZDateTime.Today;
			invoice2.AH_GC = CompanyXYZ.PK;
			var line2 = invoice2.Lines[0];
			line2.AL_GC = CompanyXYZ.PK;
			line2.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var cashVATRecord2 = TestObjectCreator.CreateCashBasisVAT(invoice2.Lines[0], -200, -20);
			cashVATRecord2.YC_GC = CompanyXYZ.PK;
			Factory.Save();

			//reaggregate 
			TestConnection.ExecuteNonQuery("TRUNCATE TABLE AccCashBasisVATQueue");
			var reAggregator = GetNewReAggregatorForTest();
			((IReAggregator)reAggregator).ReAggregate();

			var expectedAccCashBasisVATQueue = GetExpectedAccCashBasisVATQueue(new AccCashBasisVAT[] { cashVATRecord1, cashVATRecord2 });

			using (var reader = TestConnection.Command("SELECT YCC_YC FROM dbo.AccCashBasisVATQueue;").ExecuteReader())
			{
				var cashBasisVATQueue = new List<ZGuid>();

				while (reader.Read())
				{
					cashBasisVATQueue.Add(reader.GetGuid(0));
				}

				AssertEquals(expectedAccCashBasisVATQueue.Length, cashBasisVATQueue.Count);
				AssertContainsExactElementsInAnyOrder(expectedAccCashBasisVATQueue, cashBasisVATQueue);
			}
		}

		protected virtual ZGuid[] GetExpectedAccCashBasisVATQueue(AccCashBasisVAT[] accCashBasisVATs) //all companies
		{
			return accCashBasisVATs.Select(cash => cash.PK).ToArray();
		}

		[SuspendCriticalValidation]
		public void TestReAggregationReQueuesTaxGLMovementRecords()
		{
			var taxTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			var taxTransaction1 = taxTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = CompanyABC, Branch = BranchABC });
			var taxTransaction2 = taxTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = CompanyXYZ, Branch = BranchXYZ });

			var glMovement1 = taxTestObjectCreator.CreateAccTaxGLMovement(taxTransaction1.PK, TestObjectCreator.GLHeader1.PK, TestObjectCreator.GLHeader2.PK, 10);
			var glMovement2 = taxTestObjectCreator.CreateAccTaxGLMovement(taxTransaction2.PK, TestObjectCreator.GLHeader1.PK, TestObjectCreator.GLHeader2.PK, 10);

			Factory.Save();

			var actualDBCount = Factory.LoadScalarValue<ZInt>("SELECT COUNT(*) as CNT FROM dbo.AccTaxGLMovementQueue");
			AssertEquals("DB count", 2, actualDBCount);

			TestConnection.ExecuteNonQuery("TRUNCATE TABLE AccTaxGLMovementQueue");
			actualDBCount = Factory.LoadScalarValue<ZInt>("SELECT COUNT(*) as CNT FROM dbo.AccTaxGLMovementQueue");

			AssertEquals("DB count after truncate", 0, actualDBCount);

			var expectedAccTaxGLMovementQueuePKs = GetExpectedAccTaxGLMovementQueuePKs(new[] { glMovement1, glMovement2 });
			var reAggregator = GetNewReAggregatorForTest();
			((IReAggregator)reAggregator).ReAggregate();

			using (var reader = TestConnection.Command("SELECT ATQ_ATM FROM dbo.AccTaxGLMovementQueue;").ExecuteReader())
			{
				var glMovementQueue = new List<ZGuid>();

				while (reader.Read())
				{
					glMovementQueue.Add(reader.GetGuid(0));
				}

				AssertEquals(expectedAccTaxGLMovementQueuePKs.Length, glMovementQueue.Count);
				AssertContainsExactElementsInAnyOrder(expectedAccTaxGLMovementQueuePKs, glMovementQueue);
			}
		}

		protected virtual ZGuid[] GetExpectedAccTaxGLMovementQueuePKs(AccTaxGLMovement[] accTaxGLMovements)
		{
			return accTaxGLMovements.Select(x => x.PK).ToArray();
		}

		protected virtual Guid CompanyPK
		{
			get { return Env.CurrentCompany.PK; }
		}

		protected ZString SetupTestData(ZString journalType)
		{
			var oct312006 = new ZDateTime(2006, 10, 31);
			var periodCalculatorForABCCompany = new AccountingPeriodCalculator(Factory, CompanyABC);
			AssertEquals("Precondition: GetPeriodFromDate for Company ABC", 200704, periodCalculatorForABCCompany.GetPeriodFromDate(oct312006));
			var periodCalculatorForXYZCompany = new AccountingPeriodCalculator(Factory, CompanyXYZ);
			AssertEquals("Precondition: GetPeriodFromDate for Company XYZ", 200610, periodCalculatorForXYZCompany.GetPeriodFromDate(oct312006));

			if (journalType == TransactionTypes.GLNoteJournal)
			{
				glHeaderNTE1 = TestObjectCreator.CreateAccGLHeader(TestObjectCreator.GetRandomString(10), string.Empty, string.Empty, Core.Constants.AccountType.Note, DebitCreditDataEntry.DR);
				glHeaderNTE2 = TestObjectCreator.CreateAccGLHeader(TestObjectCreator.GetRandomString(10), string.Empty, string.Empty, Core.Constants.AccountType.Note, DebitCreditDataEntry.CR);
				Factory.Save();

				CreateNoteJournal(BranchABC, oct312006, glHeaderNTE1.PK, 100m, DebitCredit.DR);
				CreateNoteJournal(BranchABC, oct312006, glHeaderNTE1.PK, 150m, DebitCredit.DR);
				CreateNoteJournal(BranchABC, oct312006.AddMonths(1), glHeaderNTE1.PK, 150m, DebitCredit.DR);
				CreateNoteJournal(BranchXYZ, oct312006, glHeaderNTE1.PK, 350m, DebitCredit.DR);

				CreateNoteJournal(BranchABC, oct312006, glHeaderNTE2.PK, 200m, DebitCredit.CR);
				CreateNoteJournal(BranchXYZ, oct312006, glHeaderNTE2.PK, 450m, DebitCredit.CR);
				CreateNoteJournal(BranchXYZ, oct312006.AddMonths(1), glHeaderNTE2.PK, 150m, DebitCredit.CR);
				CreateNoteJournal(BranchXYZ, oct312006, glHeaderNTE2.PK, 350m, DebitCredit.CR);
			}
			else
			{
				CreateGLJournalsForCompany(journalType, BranchABC, oct312006, 100m);
				CreateGLJournalsForCompany(journalType, BranchXYZ, oct312006, 350m);
			}
			Factory.Save();

			ReAggregator = GetNewReAggregatorForTest();
			ReAggregator.ClearAggregate();
			ReAggregator.UpdateFlags();
			ReAggregator.ReAggregateGL();
			return journalType;
		}

		void CreateGLJournalsForCompany(ZString journalType, GlbBranch branch, ZDateTime postDate, ZDecimal amount)
		{
			if (journalType == TransactionTypes.GLStandardJournal)
			{
				CreateStandardJournal(branch, postDate, amount);
				CreateStandardJournal(branch, postDate, amount + 10m);
				CreateStandardJournal(branch, postDate.AddMonths(1), amount + 20m);
				CreateStandardJournal(branch, postDate.AddMonths(-1), amount + 30m);
			}
			else if (journalType == TransactionTypes.GLAutoJournal || journalType == TransactionTypes.GLReversingJournal)
			{
				var isReverseJournal = journalType == TransactionTypes.GLReversingJournal;
				CreateAutoOrReverseJournal(branch, postDate, postDate.AddMonths(1), amount, isReverseJournal);
				CreateAutoOrReverseJournal(branch, postDate, postDate.AddMonths(2), amount + 10m, isReverseJournal);
				CreateAutoOrReverseJournal(branch, postDate.AddMonths(-1), postDate, amount + 20m, isReverseJournal);
				CreateAutoOrReverseJournal(branch, postDate.AddMonths(-2), postDate, amount + 30m, isReverseJournal);
			}
		}

		void CreateStandardJournal(GlbBranch branch, ZDateTime postDate, ZDecimal amount)
		{
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, postDate, postDate);
				TestObjectCreator.CreateGLJournalLine(journal, amount, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				TestObjectCreator.CreateGLJournalLine(journal, amount, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			}
		}

		void CreateNoteJournal(GlbBranch branch, ZDateTime postDate, ZGuid glHeaderPK, ZDecimal amount, DebitCredit debitCredit)
		{
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, postDate, postDate);
				TestObjectCreator.CreateGLJournalLine(journal, amount, debitCredit, glHeaderPK);
			}
		}

		void CreateAutoOrReverseJournal(GlbBranch branch, ZDateTime postDate, ZDateTime dueDate, ZDecimal amount, bool isReverseJournal)
		{
			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var journaltype = isReverseJournal ? TransactionTypes.GLReversingJournal : TransactionTypes.GLAutoJournal;
				var journal = TestObjectCreator.CreateGLJournal(journaltype, postDate, postDate, dueDate);
				TestObjectCreator.CreateGLJournalLine(journal, amount, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				TestObjectCreator.CreateGLJournalLine(journal, amount, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			}
		}

		protected virtual ReAggregator GetNewReAggregatorForTest()
		{
			return new ReAggregator();
		}

		protected ReAggregator ReAggregator;

		#region Implementation

		protected AccGLAggregate FindAggregateRecord(GlbBranch branch, AccGLHeader gLHeader, ZDecimal amount)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery findAggregateQuery = new ZQuery(AccGLAggregateSchema.AA_AG, gLHeader.PK);
			findAggregateQuery.AddToFilter(AccGLAggregateSchema.AA_Amount, amount);
			findAggregateQuery.AddToFilter(AccGLAggregateSchema.AA_GB, branch.PK);
			return newFactory.LoadTop1<AccGLAggregate>(findAggregateQuery);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ZGuid gLHeader1PK = TestObjectCreator.GLHeader1.PK;
			ZGuid gLHeader2PK = TestObjectCreator.GLHeader2.PK;

			CompanyABC = TestObjectCreator.CreateNewCompany("ABC");
			BranchABC = TestObjectCreator.CreateNewBranch(CompanyABC, "AB1");

			CompanyXYZ = TestObjectCreator.CreateNewCompany("XYZ");
			BranchXYZ = TestObjectCreator.CreateNewBranch(CompanyXYZ, "XY1");

			PeriodHelper.SetupSinglePeriod(200601, new ZDateTime(2005, 07, 01, 00, 00, 00), new ZDateTime(2005, 07, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200602, new ZDateTime(2005, 08, 01, 00, 00, 00), new ZDateTime(2005, 08, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200603, new ZDateTime(2005, 09, 01, 00, 00, 00), new ZDateTime(2005, 09, 30, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200604, new ZDateTime(2005, 10, 01, 00, 00, 00), new ZDateTime(2005, 10, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200605, new ZDateTime(2005, 11, 01, 00, 00, 00), new ZDateTime(2005, 11, 30, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200606, new ZDateTime(2005, 12, 01, 00, 00, 00), new ZDateTime(2005, 12, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200607, new ZDateTime(2006, 01, 01, 00, 00, 00), new ZDateTime(2006, 01, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200608, new ZDateTime(2006, 02, 01, 00, 00, 00), new ZDateTime(2006, 02, 28, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200609, new ZDateTime(2006, 03, 01, 00, 00, 00), new ZDateTime(2006, 03, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200610, new ZDateTime(2006, 04, 01, 00, 00, 00), new ZDateTime(2006, 04, 30, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200611, new ZDateTime(2006, 05, 01, 00, 00, 00), new ZDateTime(2006, 05, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200612, new ZDateTime(2006, 06, 01, 00, 00, 00), new ZDateTime(2006, 06, 30, 23, 59, 00), CompanyABC.PK);

			PeriodHelper.SetupSinglePeriod(200701, new ZDateTime(2006, 07, 01, 00, 00, 00), new ZDateTime(2006, 07, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200702, new ZDateTime(2006, 08, 01, 00, 00, 00), new ZDateTime(2006, 08, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200703, new ZDateTime(2006, 09, 01, 00, 00, 00), new ZDateTime(2006, 09, 30, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200704, new ZDateTime(2006, 10, 01, 00, 00, 00), new ZDateTime(2006, 10, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200705, new ZDateTime(2006, 11, 01, 00, 00, 00), new ZDateTime(2006, 11, 30, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200706, new ZDateTime(2006, 12, 01, 00, 00, 00), new ZDateTime(2006, 12, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200707, new ZDateTime(2007, 01, 01, 00, 00, 00), new ZDateTime(2007, 01, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200708, new ZDateTime(2007, 02, 01, 00, 00, 00), new ZDateTime(2007, 02, 28, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200709, new ZDateTime(2007, 03, 01, 00, 00, 00), new ZDateTime(2007, 03, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200710, new ZDateTime(2007, 04, 01, 00, 00, 00), new ZDateTime(2007, 04, 30, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200711, new ZDateTime(2007, 05, 01, 00, 00, 00), new ZDateTime(2007, 05, 31, 23, 59, 00), CompanyABC.PK);
			PeriodHelper.SetupSinglePeriod(200712, new ZDateTime(2007, 06, 01, 00, 00, 00), new ZDateTime(2007, 06, 30, 23, 59, 00), CompanyABC.PK);

			PeriodHelper.SetupSinglePeriod(200503, new ZDateTime(2005, 03, 01, 00, 00, 00), new ZDateTime(2005, 03, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200504, new ZDateTime(2005, 04, 01, 00, 00, 00), new ZDateTime(2005, 04, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200505, new ZDateTime(2005, 05, 01, 00, 00, 00), new ZDateTime(2005, 05, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200506, new ZDateTime(2005, 06, 01, 00, 00, 00), new ZDateTime(2005, 06, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200507, new ZDateTime(2005, 07, 01, 00, 00, 00), new ZDateTime(2005, 07, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200508, new ZDateTime(2005, 08, 01, 00, 00, 00), new ZDateTime(2005, 08, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200509, new ZDateTime(2005, 09, 01, 00, 00, 00), new ZDateTime(2005, 09, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200510, new ZDateTime(2005, 10, 01, 00, 00, 00), new ZDateTime(2005, 10, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200511, new ZDateTime(2005, 11, 01, 00, 00, 00), new ZDateTime(2005, 11, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200512, new ZDateTime(2005, 12, 01, 00, 00, 00), new ZDateTime(2005, 12, 31, 23, 59, 00), CompanyXYZ.PK);

			PeriodHelper.SetupSinglePeriod(200601, new ZDateTime(2006, 01, 01, 00, 00, 00), new ZDateTime(2006, 01, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200602, new ZDateTime(2006, 02, 01, 00, 00, 00), new ZDateTime(2006, 02, 28, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 03, 01, 00, 00, 00), new ZDateTime(2006, 03, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200604, new ZDateTime(2006, 04, 01, 00, 00, 00), new ZDateTime(2006, 04, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200605, new ZDateTime(2006, 05, 01, 00, 00, 00), new ZDateTime(2006, 05, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200606, new ZDateTime(2006, 06, 01, 00, 00, 00), new ZDateTime(2006, 06, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200607, new ZDateTime(2006, 07, 01, 00, 00, 00), new ZDateTime(2006, 07, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200608, new ZDateTime(2006, 08, 01, 00, 00, 00), new ZDateTime(2006, 08, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200609, new ZDateTime(2006, 09, 01, 00, 00, 00), new ZDateTime(2006, 09, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200610, new ZDateTime(2006, 10, 01, 00, 00, 00), new ZDateTime(2006, 10, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200611, new ZDateTime(2006, 11, 01, 00, 00, 00), new ZDateTime(2006, 11, 30, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200612, new ZDateTime(2006, 12, 01, 00, 00, 00), new ZDateTime(2006, 12, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200501, new ZDateTime(2005, 01, 01, 00, 00, 00), new ZDateTime(2005, 01, 31, 23, 59, 00), CompanyXYZ.PK);
			PeriodHelper.SetupSinglePeriod(200502, new ZDateTime(2005, 02, 01, 00, 00, 00), new ZDateTime(2005, 02, 28, 23, 59, 00), CompanyXYZ.PK);

			Factory.Save();
		}

		AccountingPeriodTestHelper PeriodHelper
		{
			get
			{
				if (fPeriodHelper == null)
				{
					fPeriodHelper = new AccountingPeriodTestHelper(Factory);
				}

				return fPeriodHelper;
			}
		}
		AccountingPeriodTestHelper fPeriodHelper;

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		protected GlbCompany CompanyABC;
		protected GlbBranch BranchABC;

		protected GlbCompany CompanyXYZ;
		protected GlbBranch BranchXYZ;

		protected AccGLHeader glHeaderNTE1, glHeaderNTE2;

		#endregion
	}
}
