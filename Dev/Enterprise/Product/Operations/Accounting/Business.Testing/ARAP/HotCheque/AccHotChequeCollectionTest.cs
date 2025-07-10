using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.HotCheque.Testing
{
	[TestedType(typeof(AccHotChequeCollection))]
	public class AccHotChequeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			BusinessObject obj1 = HotChequeCollection.AddNew();
			AssertEquals("Index 0", obj1, HotChequeCollection[0]);

			BusinessObject obj2 = HotChequeCollection.AddNew();
			AssertEquals("Index 1", obj2, HotChequeCollection[1]);
		}

		public void TestCompanyFilter()
		{
			HotChequeCollection.Load();
			int originalCount = HotChequeCollection.Count;

			AccChequeBook chqBook1 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chqBook1.AK_GB = GlbBranch.CurrentBranch.PK;

			AccHotCheque hotChq1 = Factory.New(typeof(AccHotCheque)) as AccHotCheque;
			AccHotCheque hotChq2 = Factory.New(typeof(AccHotCheque)) as AccHotCheque;
			AccHotCheque hotChq3 = Factory.New(typeof(AccHotCheque)) as AccHotCheque;

			hotChq1.AQ_AK = chqBook1.PK;
			hotChq2.AQ_AK = ZGuid.NewZGuid();
			hotChq3.AQ_AK = chqBook1.PK;

			HotChequeCollection.Load();
			AssertEquals(2, HotChequeCollection.Count - originalCount);
		}

		public void TestNoChequeBooksForCurrentCompany()
		{
			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccHotCheque testHotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			testHotCheque.AQ_AK = testChequeBook.PK;
			testChequeBook.AK_GB = testBranch.PK;
			testBranch.GB_GC = testCompany.PK;

			Factory.Save();

			AccHotChequeCollection testCheques = new AccHotChequeCollection(Factory);
			testCheques.Load();
			AssertEquals("Collection should not contain any hot cheques", 0, testCheques.Count);

			testBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			testCheques.Load();
			AssertEquals("Collection should contain exactly one hot cheque", 1, testCheques.Count);
		}

		protected AccHotChequeCollection HotChequeCollection;

		protected override void SetUp()
		{
			base.SetUp();
			HotChequeCollection = new AccHotChequeCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccHotChequeCollection(Factory);
		}
	}
}
