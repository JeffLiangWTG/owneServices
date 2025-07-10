using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(FilteredInvoicingBaseConsolCollectionForImportingView))]
	public class FilteredInvoicingBaseConsolCollectionForImportingViewTest : BusinessObjectCollectionViewTestCase<FilteredInvoicingBaseConsolCollectionForImportingView>
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
				var consol1 = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
				TestObjectCreator.CreateShipment("S001", "", "", consol1);
				var cost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
				cost1.E6_OSCostAmount = 100M;

				var consol2 = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C002");
				TestObjectCreator.CreateShipment("S002", "", "", consol2);
				var cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
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

				AssertEquals("Precondition", 2, importer.Consols.Count);
				AssertEquals("Should show all consols", importer.Consols.Count, importer.ConsolsFilteredByViewingPermission.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				importer.LoadConsolsCollection();
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				AssertEquals("Precondition", 2, importer.Consols.Count);
				AssertEquals("Should show all consols", importer.Consols.Count, importer.ConsolsFilteredByViewingPermission.Count);

				var newFactory = new BusinessObjectFactory();
				var cost2InNewFactory = newFactory.Load<JobConsolCost>(cost2.PK);
				cost2InNewFactory.ApportionmentCharges[0].JR_GB = testBranch.PK;
				newFactory.Save();

				importer.LoadConsolsCollection();
				AssertEquals("Precondition", 2, importer.Consols.Count);
				AssertEquals("Should show only consol1 as consol2's cost branch is outside login permission", 1, importer.ConsolsFilteredByViewingPermission.Count);
			}
		}

		#endregion

		public override void TestDelete()
		{
			base.TestRemoveFromRelationship();
		}

		#region Implementation

		protected override FilteredInvoicingBaseConsolCollectionForImportingView GetCollectionToTest()
		{
			var invoice = Factory.New<APInvoice>();
			var costing = new APInvoiceConsolCosting(Factory, invoice);
			var importer = new InvoicingBaseBulkConsolCostImporter(costing);
			var consolCollection = new InvoicingBaseConsolCollectionForImporting(importer, Factory);
			return new FilteredInvoicingBaseConsolCollectionForImportingView(consolCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var consol = Factory.New<InvoicingBaseConsolForImporting>();
			var cost = Factory.New<InvoicingBaseConsolCostForImporting>();
			cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, JobConsolSchema.Constants.Prefix);
			cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			Assert(consol.ConsolCostsFilteredByViewingPermission.Any());

			return consol;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator_cached ?? (testObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator_cached;

		#endregion
	}
}
