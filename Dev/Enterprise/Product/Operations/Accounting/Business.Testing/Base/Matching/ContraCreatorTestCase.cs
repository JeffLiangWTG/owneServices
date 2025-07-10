using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ContraCreator))]
	public class ContraCreatorTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContraCreator(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestContraCreator = (ContraCreator)GetNewBusinessObject();
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.OH_Code = "TSORG1";
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_Code = "TSORG2";
		}

		protected ContraCreator TestContraCreator;
		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;

		#endregion

		public void TestCreateContra()
		{
			OrganizationSubBalance testSubBal = new OrganizationSubBalance(TestOrg1.PK, 35.67M, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			Contra testContra = TestContraCreator.CreateContra(TestOrg2.PK, testSubBal);
			AssertEquals("APRow should have org equal to TestOrg1", TestOrg1.PK, testContra.APRow.AH_OH);
			AssertEquals("APRow should have InvoiceAmount equal to -35.67", -35.67M, testContra.APRow.AH_InvoiceAmount);
			AssertEquals("APRow should have OutstandingAmount equal to -35.67", -35.67M, testContra.APRow.AH_OutstandingAmount);
			AssertEquals("APRow should have description mentioning the orgs involved",
				"RECEIVABLE AND PAYABLE CONTRA (SYSTEM GENERATED)", testContra.APRow.AH_Desc);
			Assert("APRow should be flagged as system generated", testContra.APRow.AH_TransactionCreatedByMatching);

			AssertEquals("ARRow should have org equal to TestOrg2", TestOrg2.PK, testContra.ARRow.AH_OH);
			AssertEquals("ARRow should have InvoiceAmount equal to 35.67", 35.67M, testContra.ARRow.AH_InvoiceAmount);
			AssertEquals("ARRow should have OutstandingAmount equal to 35.67", 35.67M, testContra.ARRow.AH_OutstandingAmount);
			AssertEquals("ARRow should have description mentioning the orgs involved",
				"RECEIVABLE AND PAYABLE CONTRA (SYSTEM GENERATED)", testContra.ARRow.AH_Desc);
			Assert("ARRow should be flagged as system generated", testContra.ARRow.AH_TransactionCreatedByMatching);
		}
	}
}
