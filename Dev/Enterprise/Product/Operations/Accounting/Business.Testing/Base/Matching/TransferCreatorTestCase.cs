using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class TransferCreatorTestCase : NonPersistentBusinessObjectTestCase
	{
		protected TransferCreator TestTransferCreator;
		protected OrganizationSubBalance TestOrgSubBalance1;
		protected OrganizationSubBalance TestOrgSubBalance2;
		protected OrgHeader TestOrg;
		protected OrgHeader TestOrg2;

		protected Transfer CreatedTransfer;

		protected override void SetUp()
		{
			base.SetUp();
			TestTransferCreator = (TransferCreator)GetNewBusinessObject();
			TestOrgSubBalance1 = new OrganizationSubBalance();
			TestOrgSubBalance2 = new OrganizationSubBalance();
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_Code = "TSTORG";
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_Code = "TSTORG2";
		}

		public virtual void TestCreateTransfer()
		{
			OrganizationSubBalance testSubBalance1 = new OrganizationSubBalance(TestOrg.PK, 40M, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			CreatedTransfer = TestTransferCreator.CreateTransfer(TestOrg2.PK, testSubBalance1);
			AssertEquals("TransferFrom organization should be TestOrg", TestOrg.PK, CreatedTransfer.TransferFrom.AH_OH);
			AssertEquals("TransferFrom amount should be -40", -40M, CreatedTransfer.TransferFrom.AH_InvoiceAmount);
			AssertEquals("TransferFrom outstanding amount should be -40", -40M, CreatedTransfer.TransferFrom.AH_OutstandingAmount);
			Assert("TransferFrom should be flagged as system generated", CreatedTransfer.TransferFrom.AH_TransactionCreatedByMatching);
			AssertEquals("TransferFrom AH_NumberOfSupportingDocuments should be 1", 1, CreatedTransfer.TransferFrom.AH_NumberOfSupportingDocuments.ToZInt());

			AssertEquals("TransferTo organization should be PrimaryOrg", TestOrg2.PK, CreatedTransfer.TransferTo.AH_OH);
			AssertEquals("TransferTo amount should be 40", 40M, CreatedTransfer.TransferTo.AH_InvoiceAmount);
			AssertEquals("TransferTo outstanding amount should be 40", 40M, CreatedTransfer.TransferTo.AH_OutstandingAmount);
			Assert("TransferTo should be flagged as system generated", CreatedTransfer.TransferTo.AH_TransactionCreatedByMatching);
			AssertEquals("TransferTo AH_NumberOfSupportingDocuments should be 1", 1, CreatedTransfer.TransferTo.AH_NumberOfSupportingDocuments.ToZInt());
		}
	}
}
