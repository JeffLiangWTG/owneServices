using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(MergedTransactionCollection))]
	public class MergedTransactionCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			AssertNotNull(TestCollection.AddNew(typeof(BankReconTransaction)));
			AssertEquals(1, TestCollection.Count);
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew(typeof(BankReconTransaction));
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew(typeof(Statement));
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		public void TestDebitTotal()
		{
			TestCollection.RemoveAndDeleteAll();

			BankReconTransaction trans1 = TestCollection.AddNew(typeof(BankReconTransaction)) as BankReconTransaction;
			BankReconTransaction trans2 = TestCollection.AddNew(typeof(BankReconTransaction)) as BankReconTransaction;
			BankReconTransaction trans3 = TestCollection.AddNew(typeof(BankReconTransaction)) as BankReconTransaction;

			Statement statement1 = TestCollection.AddNew(typeof(Statement)) as Statement;
			Statement statement2 = TestCollection.AddNew(typeof(Statement)) as Statement;

			trans1.AH_OSTotal = -120.0m;
			trans1.AH_TransactionType = TransactionTypes.Payment;
			trans1.OnLoaded();

			trans2.AH_OSTotal = 120.0m;
			trans2.AH_TransactionType = TransactionTypes.Receipt;
			trans2.OnLoaded();

			trans3.AH_OSTotal = -50.0m;
			trans3.AH_TransactionType = TransactionTypes.Payment;
			trans3.OnLoaded();

			statement1.AS_Amount = 60.0m;
			statement1.AS_DebitCredit = "DR";

			statement2.AS_Amount = 70.0m;
			statement2.AS_DebitCredit = "CR";

			AssertEquals(240.0m, TestCollection.CalculateDebitTotal());
		}

		public void TestCreditTotal()
		{
			TestCollection.RemoveAndDeleteAll();

			BankReconTransaction trans1 = TestCollection.AddNew(typeof(BankReconTransaction)) as BankReconTransaction;
			BankReconTransaction trans2 = TestCollection.AddNew(typeof(BankReconTransaction)) as BankReconTransaction;
			BankReconTransaction trans3 = TestCollection.AddNew(typeof(BankReconTransaction)) as BankReconTransaction;

			Statement statement1 = TestCollection.AddNew(typeof(Statement)) as Statement;
			Statement statement2 = TestCollection.AddNew(typeof(Statement)) as Statement;

			trans1.AH_OSTotal = 120.0m;
			trans1.AH_TransactionType = TransactionTypes.Payment;
			trans1.OnLoaded();

			trans2.AH_OSTotal = 10.0m;
			trans2.AH_TransactionType = TransactionTypes.Payment;
			trans2.OnLoaded();

			trans3.AH_OSTotal = -50.0m;
			trans3.AH_TransactionType = TransactionTypes.Payment;
			trans3.OnLoaded();

			statement1.AS_Amount = 60.0m;
			statement1.AS_DebitCredit = "CR";

			statement2.AS_Amount = 70.0m;
			statement2.AS_DebitCredit = "DR";

			AssertEquals(200.0m, TestCollection.CalculateCreditTotal());
		}

		public void TestTypeOfElementsFromPK()
		{
			BusinessObject trans1 = TestCollection.AddNew(typeof(BankReconTransaction));
			BusinessObject statement1 = TestCollection.AddNew(typeof(Statement));
			AssertEquals(typeof(BankReconTransaction), TestCollection.GetTypeOfElementsFromPK(trans1.PK));
			AssertEquals(typeof(Statement), TestCollection.GetTypeOfElementsFromPK(statement1.PK));
			AssertEquals(typeof(AccTransactionHeader), TestCollection.GetTypeOfElementsFromPK(ZGuid.Invalid));
		}

		protected MergedTransactionCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new MergedTransactionCollection(Factory, new BankReconciliation(Factory));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new MergedTransactionCollection(Factory, new BankReconciliation(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(BankReconTransaction));
		}
	}
}
