using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChequeFilterBusinessObject))]
	sealed class ChequeFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Fields

		ChequeFilterBusinessObject filterBO;
		AccReceivedChequeCollection filterCollection;
		TestObjectCreator testObjectCreator;

		#endregion

		public void TestAmountFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleNumberRangeFilter)filterBO["Amount"];
			filter.IsActive = true;

			filter.Property1 = 100;
			filter.Property2 = 100;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property1 = 100;
			filter.Property2 = 200;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);

			filter.Property1 = 300;
			filter.Property2 = 300;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestBankNameFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleTextFilter)filterBO["Cheque Bank Account"];
			filter.IsActive = true;

			filter.Property = "BankA";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "BankB";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "BankC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestChequeDrawerFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleTextFilter)filterBO["Cheque Drawer"];
			filter.IsActive = true;

			filter.Property = "DrawerA";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "DrawerB";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "DrawerC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestCurrencyFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleNkFilter)filterBO["Currency"];
			filter.IsActive = true;

			filter.Property = testObjectCreator.AUD.Code;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = testObjectCreator.USD.Code;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = testObjectCreator.TRY.Code;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestChequeNumberFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleTextFilter)filterBO["Cheque #"];
			filter.IsActive = true;

			filter.Property = "0001";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "0002";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "9999";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestChequePortfolioNumberFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleTextFilter)filterBO["Cheque Portfolio #"];
			filter.IsActive = true;

			filter.Property = "R001";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "R002";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "R999";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestDueDateFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleDateFilter)filterBO["Due Date"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDate.Today.AddDays(10);
			filter.Property2 = ZDate.Today.AddDays(10);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property1 = ZDate.Today.AddDays(10);
			filter.Property2 = ZDate.Today.AddDays(20);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);

			filter.Property1 = ZDate.Today.AddDays(30);
			filter.Property2 = ZDate.Today.AddDays(40);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestPaymentLocationFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleTextFilter)filterBO["Place of Payment"];
			filter.IsActive = true;

			filter.Property = "LocationA";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "LocationB";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = "LocationZ";
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestReceivedFromFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleGuidFilter)filterBO["Received Debtor"];
			filter.IsActive = true;

			filter.Property = testObjectCreator.DebtorDE.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = testObjectCreator.DebtorTR.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);

			filter.Property = testObjectCreator.Debtor.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestGivenBankAccountFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleGuidFilter)filterBO["Given Bank Account"];
			filter.IsActive = true;

			filter.Property = testObjectCreator.AUDBankAccount.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(testObjectCreator.AUDBankAccount.PK, filterCollection[0].RCH_AB_GivenBankAccount);

			filter.Property = testObjectCreator.USDBankAccount.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(testObjectCreator.USDBankAccount.PK, filterCollection[0].RCH_AB_GivenBankAccount);

			filter.Property = testObjectCreator.CashAtBankAccount.PK;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestStatusFilter()
		{
			testObjectCreator.CreateChequesAndSave();

			var filter = (ModuleTextFilter)filterBO["Status"];
			filter.IsActive = true;
			filter.Property = "COH";

			filterCollection.Load(filterBO.Filter);

			AssertEquals(2, filterCollection.Count);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new ChequeFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			testObjectCreator = new TestObjectCreator(Factory);
			filterCollection = new AccReceivedChequeCollection(Factory);
			filterBO = (ChequeFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
