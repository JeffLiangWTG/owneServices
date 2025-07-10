using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(FilteredInvoicingLineBaseCollectionView))]
	public class FilteredInvoicingLineBaseCollectionViewTest : BusinessObjectCollectionViewTestCase<FilteredInvoicingLineBaseCollectionView>
	{
		public void TestNonAbstractTypeOfElements()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 0M, 10M, 0M, 0M, TestObjectCreator.AALSHI);
			AssertEquals(typeof(AccTransactionLines), ((IHaveAbstractElementType)invoice.FilteredLines).NonAbstractTypeOfElements);
		}

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
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 0M, 10M, 0M, 0M, TestObjectCreator.AALSHI);
				var line1 = invoice.Lines[0];
				var line2 = TestObjectCreator.CreateAPInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Desc1", 100M);
				var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Desc1", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, null);
				charge2.JR_AL_APLine = line2.PK;
				var line3 = TestObjectCreator.CreateAPInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC2, TestObjectCreator.AUD, 1m, "Desc12", 100M);
				var charge3 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC2, "Desc2", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, null);
				charge3.JR_AL_APLine = line3.PK;

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
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				var invoiceLineCollection = invoice.Lines;
				var filteredInvoiceLineCollection = invoice.FilteredLines;

				AssertEquals("should show all invoice lines", 3, invoiceLineCollection.Count);
				AssertEquals("should show all invoice lines", 3, filteredInvoiceLineCollection.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				AssertEquals("should show all invoice lines", 3, invoiceLineCollection.Count);
				AssertEquals("should show all invoice lines", 3, filteredInvoiceLineCollection.Count);

				line3.AL_GB = testBranch.PK;
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				Factory.Save();

				AssertEquals("should show all invoice lines", 3, invoiceLineCollection.Count);
				AssertEquals("should show only invoice line1 and line2", 2, filteredInvoiceLineCollection.Count);
				AssertCollectionNotContains("Should not contain line3", line3, filteredInvoiceLineCollection);
			}
		}

		#endregion

		public void TestSuspendAdditionallyForImport()
		{
			var invoiceLineCollection = new InvoicingLineBaseCollection((InvoicingBase)Factory.New(typeof(APInvoice)));
			var filteredInvoicingLineBaseCollectionView = new FilteredInvoicingLineBaseCollectionView(invoiceLineCollection);
			using (filteredInvoicingLineBaseCollectionView.SuspendAdditionallyForImport())
			{
				Assert(invoiceLineCollection.InvoicingBase.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(true).IsSuspended);
			}
		}

		#region Implementation

		protected override FilteredInvoicingLineBaseCollectionView GetCollectionToTest()
		{
			var invoiceLineCollection = new InvoicingLineBaseCollection((InvoicingBase)Factory.New(typeof(APInvoice)));
			return new FilteredInvoicingLineBaseCollectionView(invoiceLineCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoiceLine));
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
