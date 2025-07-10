using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(FilteredTransactionHeaderCollectionView))]
	public class FilteredTransactionHeaderCollectionViewTest : BusinessObjectCollectionViewTestCase<FilteredTransactionHeaderCollectionView>
	{
		#region TestIsThisPartOfTheCollection

		public void TestIsThisPartOfTheCollection()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			InvoicingBase testInvoiceInOtherCompany;
			using (TestObjectCreator.SwitchEnvToBranch(testBranch))
			{
				testInvoiceInOtherCompany = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "InvTBR", TestObjectCreator.AUD, 1, 30, 0, 10, 0);
				testInvoiceInOtherCompany.AH_OH = TestObjectCreator.AALSHI.PK;
			}
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var testInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
				testInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
				var testInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv002", TestObjectCreator.AUD, 1, 20, 0, 10, 0);
				testInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();
				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				SecurityCore security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				GlbSecurity loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = Env.Security.ReceivablesViewingFinancialOutsideLoginPermission.Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.ReceivablesViewingFinancialOutsideLoginPermission.IsAllowed = true;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				var transactionHeaders = new ARTransactionHeaderCollection(Factory);
				var filteredtransactionHeaders = new FilteredTransactionHeaderCollectionView(transactionHeaders);
				transactionHeaders.Load();
				filteredtransactionHeaders.Load();

				AssertEquals("should show all invoices", 3, transactionHeaders.Count);
				AssertEquals("should show all invoices", 3, filteredtransactionHeaders.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.ReceivablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				transactionHeaders.Load();
				filteredtransactionHeaders.Load();
				AssertEquals("should show all invoices", 3, transactionHeaders.Count);
				AssertEquals("should show all invoices", 3, filteredtransactionHeaders.Count);

				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));

				transactionHeaders.Load();
				filteredtransactionHeaders.Load();
				AssertEquals("should show all invoices", 3, transactionHeaders.Count);
				AssertContainsExactElementsInAnyOrder("should show only testInvoice1 and 2", new[] { testInvoice1.PK, testInvoice2.PK }, filteredtransactionHeaders.Select(x => x.PK));
			}
		}

		#endregion

		#region TestTypeOfElement

		public void TestTypeOfElement()
		{
			var testInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			var transactionHeadercollection = new ARTransactionHeaderCollection(Factory);
			var filteredtransactionHeadercollection = new FilteredTransactionHeaderCollectionView(transactionHeadercollection);

			AssertEquals("Precondition", 0, transactionHeadercollection.Count);
			AssertEquals("Precondition", 0, filteredtransactionHeadercollection.Count);
			AssertEquals("Type should be TransactionHeader", typeof(TransactionHeader), filteredtransactionHeadercollection.GetTypeOfElementsFromPK(ZGuid.Empty));

			transactionHeadercollection.Load();
			filteredtransactionHeadercollection.Load();
			AssertEquals("Precondition", 1, transactionHeadercollection.Count);
			AssertEquals("Precondition", 1, filteredtransactionHeadercollection.Count);
			AssertEquals("Type should be type of collection to filter", typeof(ARInvoice), filteredtransactionHeadercollection.GetTypeOfElementsFromPK(testInvoice.PK));
		}

		#endregion

		#region Implementation

		protected override FilteredTransactionHeaderCollectionView GetCollectionToTest()
		{
			var transactionHeaders = new ARTransactionHeaderCollection(Factory);
			return new FilteredTransactionHeaderCollectionView(transactionHeaders);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ARInvoice));
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
