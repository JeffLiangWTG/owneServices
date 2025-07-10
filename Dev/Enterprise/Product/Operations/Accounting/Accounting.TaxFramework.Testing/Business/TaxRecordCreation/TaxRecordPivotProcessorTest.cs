using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecordPivotProcessorTest : TestCaseWithFactory
	{
		public void TestIsUsagedAsDependency()
		{
			AssertType<TaxRecordPivotProcessor>(new TaxRecordCreator().TaxRecordPivotProcessor_ExposedForTestOnly);
			AssertType<TaxRecordPivotProcessor>(new TaxRecordCalculator(new TaxFrameworkConfigurationHelper()).TaxRecordPivotProcessor_ExposedForTestOnly);
		}

		public void TestCreatePivot()
		{
			ITaxRecordPivotProcessor pivotCreator = new TaxRecordPivotProcessor();
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;
			var line = Factory.New<APInvoiceLine>();
			line.AL_ExchangeRate = 4;
			line.AL_OSExTaxAmount = 100;

			var pivot = pivotCreator.Create(taxRecord, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			AssertPivot(false);

			taxRecord.ATT_AG_TaxExpenseAccount = TestObjectCreator.GLHeader1.PK;
			pivot = pivotCreator.Create(taxRecord, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			AssertPivot(true);

			void AssertPivot(bool isTaxExpense)
			{
				AssertEquals("ATP_ATT", taxRecord.PK, pivot.ATP_ATT);
				AssertEquals("ATP_AL_TransactionLine", line.PK, pivot.ATP_AL_TransactionLine);
				AssertEquals("ATP_IsTaxExpense", isTaxExpense, pivot.ATP_IsTaxExpense);
				AssertEquals("BaseOSAmount", -100m, pivot.BaseOSAmount);
				AssertEquals("LocalTaxBaseAmount", -25m, pivot.LocalTaxBaseAmount);
			}
		}

		public void TestDeleteTaxRecordsWithPivots()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var pivot21 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var pivot22 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot21.ATP_ATT = taxRecord2.PK;
			pivot22.ATP_ATT = taxRecord2.PK;
			var taxRecord3 = Factory.New<AccTaxTransaction>();
			var pivot31 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var pivot32 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var pivot33 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot31.ATP_ATT = taxRecord3.PK;
			pivot32.ATP_ATT = taxRecord3.PK;
			pivot33.ATP_ATT = taxRecord3.PK;

			ITaxRecordPivotProcessor pivotProcessor = new TaxRecordPivotProcessor();
			pivotProcessor.DeleteTaxRecordsWithPivots(taxRecord1, taxRecord3);
			Assert(taxRecord1.IsDeleted);
			Assert(pivot1.IsDeleted);
			Assert(!taxRecord2.IsDeleted);
			Assert(!pivot21.IsDeleted);
			Assert(!pivot22.IsDeleted);
			Assert(taxRecord3.IsDeleted);
			Assert(pivot31.IsDeleted);
			Assert(pivot32.IsDeleted);
			Assert(pivot33.IsDeleted);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
