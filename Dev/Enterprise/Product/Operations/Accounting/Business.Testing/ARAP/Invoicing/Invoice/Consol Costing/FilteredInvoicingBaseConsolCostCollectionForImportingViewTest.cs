using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(FilteredInvoicingBaseConsolCostCollectionForImportingView))]
	public class FilteredInvoicingBaseConsolCostCollectionForImportingViewTest : BusinessObjectCollectionViewTestCase<FilteredInvoicingBaseConsolCostCollectionForImportingView>
	{
		#region TestIsThisPartOfTheCollection

		public void TestIsThisPartOfTheCollection()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
				TestObjectCreator.CreateShipment("S001", "", "", consol);
				var cost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
				cost1.E6_OSCostAmount = 100M;
				var cost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
				cost2.E6_OSCostAmount = 300M;

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
				invoicingSecurity.GU_SecurityRight = Env.Security.PayablesViewingFinancialOutsideLoginPermission.Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				var invoice = Factory.New<APInvoice>();
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				var costing = new APInvoiceConsolCosting(Factory, invoice);
				var importer = new InvoicingBaseBulkConsolCostImporter(costing);
				importer.LoadConsolsCollection();

				AssertEquals("Precondition", 1, importer.Consols.Count);
				AssertEquals("Should show all consol costs", importer.Consols[0].ConsolCosts.Count, importer.Consols[0].ConsolCostsFilteredByViewingPermission.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				importer.LoadConsolsCollection();
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				AssertEquals("Precondition", 1, importer.Consols.Count);
				AssertEquals("Should show all consol costs", importer.Consols[0].ConsolCosts.Count, importer.Consols[0].ConsolCostsFilteredByViewingPermission.Count);

				var newFactory = new BusinessObjectFactory();
				var cost2InNewFactory = newFactory.Load<JobConsolCost>(cost2.PK);
				cost2InNewFactory.ApportionmentCharges[0].JR_GB = testBranch.PK;
				newFactory.Save();

				importer.LoadConsolsCollection();
				AssertEquals("Precondition", 1, importer.Consols.Count);
				Assert("Should show only cost1 as cost2's branch is outside login permission", importer.Consols[0].ConsolCosts.Count > importer.Consols[0].ConsolCostsFilteredByViewingPermission.Count);
			}
		}

		#endregion

		#region Implementation

		protected override FilteredInvoicingBaseConsolCostCollectionForImportingView GetCollectionToTest()
		{
			var consolCostCollection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			return new FilteredInvoicingBaseConsolCostCollectionForImportingView(consolCostCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<InvoicingBaseConsolCostForImporting>();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator_cached ?? (testObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator_cached;

		#endregion
	}
}
