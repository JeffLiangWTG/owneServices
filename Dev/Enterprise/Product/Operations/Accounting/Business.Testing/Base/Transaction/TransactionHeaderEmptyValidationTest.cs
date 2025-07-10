using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionHeaderEmptyValidationTest : TestCaseWithFactory
	{
		public void TestMultipleReversingErrors()
		{
			TransactionHeaderValidation testValidation = new TransactionHeaderValidation(Header);

			Header.MultipleReversingErrors = null;
			testValidation.ValidateAll();
			AssertNoRowErrors(Header);

			Header.MultipleReversingErrors = new string[] { "error 1", "error 2" };
			testValidation.ValidateAll();
			AssertHasRowError(Header, "error 1");
			AssertHasRowError(Header, "error 2");
		}

		public void TestBranchDepartmentCombinationValidation_TransactionHeaderEmptyValidation()
		{
			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);

			var receipt = testObjectCreator.CreateARReceipt(ReceiptTypes.DirectCredit, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, testObjectCreator.AUDBankAccount.PK);
			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("000101", testObjectCreator.AUD, 1.0M, testObjectCreator.AALSHI);
			testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1m, -100m, 0m, 0m, -100m, 0m, 0m);
			receipt.MatchingBaseObject.MatchedTransactions.Add(invoice);

			AssertEquals(receipt.IsInMatchingContext, true);
			var validation = new TransactionHeaderEmptyValidation(receipt);

			receipt.Branch.AllowedDepartments.DeleteAll();
			validation.ValidateAll();
			AssertNoErrors(receipt.AH_GEInfo);

			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(receipt.Branch, new GlbDepartment[] { bbbDepartment });

			validation.ValidateAll();
			AssertHasError(receipt.AH_GEInfo, string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, receipt.Department.GE_Code, receipt.Branch.GB_Code));
		}

		#region Implementation

		protected TransactionHeader Header;

		protected override void SetUp()
		{
			base.SetUp();

			Header = Factory.New(HeaderType) as TransactionHeader;
		}

		protected virtual Type HeaderType
		{
			get { return typeof(ARInvoice); }
		}

		#endregion
	}
}