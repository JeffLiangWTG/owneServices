using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(FilteredAPInvoicingBaseCollectionViewForModule))]
	public class FilteredInvoicingBaseCollectionViewForModuleTest : BusinessObjectCollectionViewTestCase<FilteredAPInvoicingBaseCollectionViewForModule>
	{
		#region TestIsThisPartOfTheCollection

		public void TestIsThisPartOfTheCollection()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			var testBranchOfCurrentCompany = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			var testBranchOfOtherCompany = TestObjectCreator.CreateBranchWithCompany("NZAKL");
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();
			var currentBranchSecurity = new UserLoginController().GetSecurityForUser(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var otherBranchSecurity = new UserLoginController().GetSecurityForUser(testUser.GS_LoginName, testBranchOfCurrentCompany.PK.ToGuid(), Env.CurrentDepartment.PK);
			var otherCompanySecurity = new UserLoginController().GetSecurityForUser(testUser.GS_LoginName, testBranchOfOtherCompany.PK.ToGuid(), Env.CurrentDepartment.PK);

			var currentBranchLoginSecurity = CreateSecurityRight(currentBranchSecurity.Login, GlbBranch.CurrentBranch);
			var otherBranchLoginSecurity = CreateSecurityRight(otherBranchSecurity.Login, testBranchOfCurrentCompany);
			var otherCompanyLoginSecurity = CreateSecurityRight(otherCompanySecurity.Login, testBranchOfOtherCompany);

			Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = true;
			currentBranchLoginSecurity.GU_SecurityItemIsAllowed = true;
			otherBranchLoginSecurity.GU_SecurityItemIsAllowed = true;
			otherCompanyLoginSecurity.GU_SecurityItemIsAllowed = true;

			securityFactory.Save();

			InvoicingBase testInvoiceInOtherBranch;
			using (TestObjectCreator.SwitchEnvToBranch(testBranchOfCurrentCompany))
			{
				testInvoiceInOtherBranch = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APInvTBR", TestObjectCreator.AUD, 1, 30, 0, 30, 0);
				Factory.Save();
			}

			InvoicingBase testInvoiceInOtherCompany;
			using (TestObjectCreator.SwitchEnvToBranch(testBranchOfOtherCompany))
			{
				testInvoiceInOtherCompany = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APInvAKL", TestObjectCreator.AUD, 1, 40, 0, 40, 0);
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var testInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APInv001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
				testInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
				var testInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "APInv002", TestObjectCreator.AUD, 1, 20, 0, 10, 0);
				testInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				var invoices = new InvoicingBaseCollectionForModule(Factory);
				var filteredInvoices = new FilteredAPInvoicingBaseCollectionViewForModule(invoices);
				filteredInvoices.Load();

				AssertEquals("should show all invoices in current company", 3, invoices.Count);
				AssertEquals("should show all invoices in current company", 3, filteredInvoices.Count);

				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;

				filteredInvoices.Load();
				AssertEquals("should show all invoices in current company", 3, invoices.Count);
				AssertEquals("should show all invoices in current company because user can login to other branch", 3, filteredInvoices.Count);

				otherBranchLoginSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				filteredInvoices.Load();
				AssertEquals("should show all invoices in current company", 3, invoices.Count);
				AssertEquals("should show all invoices in current company because factory level caching", 3, filteredInvoices.Count);

				Factory.ClearCachedValue<bool>("Login BRN:" + testBranchOfCurrentCompany.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code);

				filteredInvoices.Load();
				AssertEquals("should show all invoices in current company", 3, invoices.Count);
				AssertContainsExactElementsInAnyOrder("should show only testInvoice1 and 2 because user cannot login to other branch", new[] { testInvoice1.PK, testInvoice2.PK }, filteredInvoices.Select(x => x.PK));

				var invoicesBasedOnFilter = Factory.Load<InvoicingBase>(filteredInvoices.CompleteFilter);
				Assert("should only show invoices in current company based on collection filter", invoicesBasedOnFilter.Any(x => x.AH_GC == Env.CurrentCompanyPK));
			}

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, testBranchOfOtherCompany.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoices = new InvoicingBaseCollectionForModule(Factory);
				var filteredInvoices = new FilteredAPInvoicingBaseCollectionViewForModule(invoices);

				filteredInvoices.Load();
				AssertEquals("should show all invoices in current company", 1, filteredInvoices.Count);

				var invoicesBasedOnFilter = Factory.Load<InvoicingBase>(filteredInvoices.CompleteFilter);
				Assert("should only show invoices in current company based on collection filter", invoicesBasedOnFilter.Any(x => x.AH_GC == Env.CurrentCompanyPK));
			}

			#region Local Helpers

			GlbSecurity CreateSecurityRight(SecurityCheckpoint checkpoint, GlbBranch branch)
			{
				var security = securityFactory.New<GlbSecurity>();
				security.GU_GB = branch.PK;
				security.GU_GE = Env.CurrentDepartment.PK;
				security.GU_GS = testUser.PK.ToGuid();
				security.GU_SecurityRight = checkpoint.Code;
				return security;
			}

			#endregion
		}

		#endregion

		#region Implementation

		protected override FilteredAPInvoicingBaseCollectionViewForModule GetCollectionToTest()
		{
			var invoices = new InvoicingBaseCollectionForModule(Factory);
			return new FilteredAPInvoicingBaseCollectionViewForModule(invoices);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoice));
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
