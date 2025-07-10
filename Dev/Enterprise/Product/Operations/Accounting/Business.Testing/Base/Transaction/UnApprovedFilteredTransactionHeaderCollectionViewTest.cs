using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(UnApprovedFilteredTransactionHeaderCollectionView))]
	public class UnApprovedFilteredTransactionHeaderCollectionViewTest : BusinessObjectCollectionViewTestCase<UnApprovedFilteredTransactionHeaderCollectionView>
	{
		#region TestIsThisPartOfTheCollection

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIsThisPartOfTheCollectionForUnApprovedTransactions()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			var nonCurrentCompany = TestObjectCreator.NonCurrentNonDemoCompany;
			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, nonCurrentCompany.PK));

			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var sendingCompanyJob = TestObjectCreator.CreateJobHeader();
			var receivingCompanyJob = TestObjectCreator.CreateJobHeader();

			sendingCompanyJob.JH_JobNum = "S001001";
			sendingCompanyJob.JH_GC = TestObjectCreator.NonCurrentNonDemoCompany.PK;
			sendingCompanyJob.JH_GS_NKRepOps = "SN";
			sendingCompanyJob.JH_GB = nonCurrentBranch.PK;
			sendingCompanyJob.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			receivingCompanyJob.JH_JobNum = "S001001";
			receivingCompanyJob.JH_GC = GlbCompany.CurrentCompany.PK;
			receivingCompanyJob.JH_GS_NKRepOps = "RV";
			receivingCompanyJob.JH_GB = GlbBranch.CurrentBranch.PK;
			receivingCompanyJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			invoice.AH_JH = sendingCompanyJob.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var securityFactory = new BusinessObjectFactory();
				var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				var security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				var loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				var loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				var invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = Env.Security.PayablesViewingFinancialOutsideLoginPermission.Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = true;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				var transactionHeaders = new ARTransactionHeaderCollection(Factory);
				var filteredtransactionHeaders = new UnApprovedFilteredTransactionHeaderCollectionView(transactionHeaders);
				transactionHeaders.Load();
				filteredtransactionHeaders.Load();

				AssertEquals("should show the invoice", 1, transactionHeaders.Count);
				AssertEquals("should show the invoice", 1, filteredtransactionHeaders.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				transactionHeaders.Load();
				filteredtransactionHeaders.Load();
				AssertEquals("should show the invoice", 1, transactionHeaders.Count);
				AssertEquals("should show the invoice", 1, filteredtransactionHeaders.Count);

				receivingCompanyJob.JH_GB = testBranch.PK;
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				Factory.Save();

				transactionHeaders.Load();
				filteredtransactionHeaders.Load();
				AssertEquals("should show the invoice", 1, transactionHeaders.Count);
				AssertEquals("should show not show any invoice", 0, filteredtransactionHeaders.Count);
			}
		}

		#endregion

		#region Implementation

		protected override UnApprovedFilteredTransactionHeaderCollectionView GetCollectionToTest()
		{
			var transactionHeaders = new ARTransactionHeaderCollection(Factory);
			return new UnApprovedFilteredTransactionHeaderCollectionView(transactionHeaders);
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
