using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(ARTransactionHeaderCollection))]
	public class ARTransactionHeaderCollectionTest : TransactionHeaderCollectionTest
	{
		public override void TestFilterCorrect()
		{
			GlbBranch anotherCompanyBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;

			GlbDepartment testDept1 = Factory.New(typeof(GlbDepartment)) as GlbDepartment;
			testDept1.GE_Code = "%%%";
			testDept1.GE_Desc = "$$$";

			GlbDepartment testDept2 = Factory.New(typeof(GlbDepartment)) as GlbDepartment;
			testDept2.GE_Code = "!!!";
			testDept2.GE_Desc = "%%%";

			GlbCompany anotherCompany = Factory.New(typeof(GlbCompany)) as GlbCompany;
			anotherCompany.GC_StartDate = ZDateTime.Now;
			anotherCompany.GC_RX_NKLocalCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			anotherCompany.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery()).Code;
			anotherCompany.GC_Code = "!!!";

			anotherCompanyBranch.GB_GC = anotherCompany.PK;

			GlbBranch currentBranch = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)) as GlbBranch;
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			TestAPInvoice = Factory.New(typeof(APInvoice)) as APInvoice;
			TestAPInvoice.AH_TransactionNum = "TRAN100";
			TestAPInvoice.AH_GB = currentBranch.PK;
			TestAPInvoice.AH_PostDate = ZDateTime.Now;
			TestAPInvoice.AH_GE = testDept1.PK;
			TestAPInvoice.AH_InvoiceDate = ZDateTime.Now;

			TestARInvoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			TestARInvoice.AH_GB = currentBranch.PK;
			TestARInvoice.AH_PostDate = ZDateTime.Now;
			TestARInvoice.AH_GE = testDept2.PK;
			TestARInvoice.AH_InvoiceDate = ZDateTime.Now;

			ARReceipt testARReceipt = Factory.New(typeof(ARReceipt)) as ARReceipt;
			testARReceipt.AH_GB = anotherCompanyBranch.PK;
			testARReceipt.AH_PostDate = ZDateTime.Now;
			testARReceipt.AH_GE = testDept2.PK;
			testARReceipt.AH_InvoiceDate = ZDateTime.Now;

			InvoiceBatchHeader batchHeader = Factory.New<InvoiceBatchHeader>();
			batchHeader.AH_GB = currentBranch.PK;
			batchHeader.AH_PostDate = ZDateTime.Now;
			batchHeader.AH_GE = testDept2.PK;
			batchHeader.AH_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			ARTransactionHeaderCollection testCollection = new ARTransactionHeaderCollection(Factory);
			testCollection.Load();

			Assert("TestCollection should contain AR Invoice because it is AR AND it is created by the current company", testCollection.FindByPK(TestARInvoice.PK) != null);
			Assert("TestCollection should not contain ARReceipt because it is created by a different company", testCollection.FindByPK(testARReceipt.PK) == null);
			Assert("TestCollection should not contain APInvoice because it is created by a different company", testCollection.FindByPK(TestAPInvoice.PK) == null);
			Assert("TestCollection should not contain Batch Header because it shouldn't show in module screens", !testCollection.Contains(batchHeader.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ARTransactionHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ARInvoice));
		}
	}
}
