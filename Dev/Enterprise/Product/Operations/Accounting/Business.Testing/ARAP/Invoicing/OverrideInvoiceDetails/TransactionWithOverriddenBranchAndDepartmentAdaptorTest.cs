using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(TransactionWithOverriddenBranchAndDepartmentAdaptor))]
	public class TransactionWithOverriddenBranchAndDepartmentAdaptorTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoice = Factory.New<ARInvoice>();
			return new TransactionWithOverriddenBranchAndDepartmentAdaptor(invoice);
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		public void TestApplyOrCancelChanges()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("TRN0001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);
			var adaptor = new TransactionWithOverriddenBranchAndDepartmentAdaptor(invoice);

			AssertEquals("AH_GB", invoice.AH_GB, adaptor.WrappedObjects[0].AH_GB);
			AssertEquals("AH_GE", invoice.AH_GE, adaptor.WrappedObjects[0].AH_GE);

			var branchPK = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "NER").PK;
			adaptor.WrappedObjects[0].AH_GB = branchPK;
			adaptor.WrappedObjects[0].AH_GE = TestObjectCreator.FESDepartment.PK;

			adaptor.ApplyOrCancelChanges(false);
			AssertEquals("Context is not set", false, invoice.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment));

			AssertEquals("invoice.AH_GB: Cancelled", invoice.AH_GB, adaptor.WrappedObjects[0].AH_GB);
			AssertEquals("invoice.AH_GE: Cancelled", invoice.AH_GE, adaptor.WrappedObjects[0].AH_GE);

			AssertNotEquals("wrappedBizo.AH_GB: Cancelled", branchPK, adaptor.WrappedObjects[0].AH_GB);
			AssertNotEquals("wrappedBizo.AH_GE: Cancelled", TestObjectCreator.FESDepartment.PK, adaptor.WrappedObjects[0].AH_GE);

			adaptor.WrappedObjects[0].AH_GB = branchPK;
			adaptor.WrappedObjects[0].AH_GE = TestObjectCreator.FESDepartment.PK;

			adaptor.ApplyOrCancelChanges(true);
			AssertEquals("Context gets set", true, invoice.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment));

			AssertEquals("wrappedBizo.AH_GB: Applied", branchPK, adaptor.WrappedObjects[0].AH_GB);
			AssertEquals("wrappedBizo.AH_GE: Applied", TestObjectCreator.FESDepartment.PK, adaptor.WrappedObjects[0].AH_GE);

			AssertEquals("AH_GB: Applied", adaptor.WrappedObjects[0].AH_GB, invoice.AH_GB);
			AssertEquals("AH_GE: Applied", adaptor.WrappedObjects[0].AH_GE, invoice.AH_GE);
		}

		public void TestBranchAndDepartmentOverrideCreatesLog()
		{
			var branch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "NER");
			branch.Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("TRN0001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);
			var adaptor = new TransactionWithOverriddenBranchAndDepartmentAdaptor(invoice);
			var oldBranchCode = invoice.Branch.GB_Code;
			var oldDeptCode = invoice.Department.GE_Code;

			adaptor.WrappedObjects[0].AH_GB = branch.PK;
			adaptor.WrappedObjects[0].AH_GE = TestObjectCreator.FESDepartment.PK;

			adaptor.ApplyOrCancelChanges(true);

			AssertEquals("wrappedBizo.AH_GB: Applied", branch.PK, adaptor.WrappedObjects[0].AH_GB);
			AssertEquals("wrappedBizo.AH_GE: Applied", TestObjectCreator.FESDepartment.PK, adaptor.WrappedObjects[0].AH_GE);

			AssertEquals("AH_GB: Applied", adaptor.WrappedObjects[0].AH_GB, invoice.AH_GB);
			AssertEquals("AH_GE: Applied", adaptor.WrappedObjects[0].AH_GE, invoice.AH_GE);

			Factory.Save();

			AssertEquals("Invoice Log", 3, invoice.Logs.DatabaseCount);
			var logs = invoice.Logs.GetAllLogs();
			AssertCollectionContains("A Log is created for overridden Branch", string.Format("Transaction Branch Edited: From '{0}' to 'NER'", oldBranchCode), logs.Select(l => l.SL_Reference));
			AssertCollectionContains("A Log is created for overridden Department", string.Format("Transaction Department Edited: From '{0}' to 'FES'", oldDeptCode), logs.Select(l => l.SL_Reference));
		}

		public void TestOverrdingBranchAndDepartmentSavingProcessRemovesInvalidDepartmentRowErrorMessageFirst()
		{
			var branch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "NER");
			branch.Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("TRN0001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);
			var adaptor = new TransactionWithOverriddenBranchAndDepartmentAdaptor(invoice);
			var oldBranchCode = invoice.Branch.GB_Code;
			var oldDeptCode = invoice.Department.GE_Code;

			invoice.AddRowError(IntercompanyTransactionImportHelper.GetErrorMessageWhenTransactionLinesHaveInvalidDepartment());

			adaptor.WrappedObjects[0].AH_GB = branch.PK;
			adaptor.WrappedObjects[0].AH_GE = TestObjectCreator.FESDepartment.PK;

			adaptor.ApplyOrCancelChanges(true);

			AssertEquals("wrappedBizo.AH_GB: Applied", branch.PK, adaptor.WrappedObjects[0].AH_GB);
			AssertEquals("wrappedBizo.AH_GE: Applied", TestObjectCreator.FESDepartment.PK, adaptor.WrappedObjects[0].AH_GE);
			AssertHasRowError(invoice, IntercompanyTransactionImportHelper.GetErrorMessageWhenTransactionLinesHaveInvalidDepartment());

			AssertEquals("AH_GB: Applied", adaptor.WrappedObjects[0].AH_GB, invoice.AH_GB);
			AssertEquals("AH_GE: Applied", adaptor.WrappedObjects[0].AH_GE, invoice.AH_GE);

			Factory.Save();

			AssertNoRowError(invoice, IntercompanyTransactionImportHelper.GetErrorMessageWhenTransactionLinesHaveInvalidDepartment());
			AssertEquals("Invoice Log", 3, invoice.Logs.DatabaseCount);
			var logs = invoice.Logs.GetAllLogs();
			AssertCollectionContains("A Log is created for overridden Branch", string.Format("Transaction Branch Edited: From '{0}' to 'NER'", oldBranchCode), logs.Select(l => l.SL_Reference));
			AssertCollectionContains("A Log is created for overridden Department", string.Format("Transaction Department Edited: From '{0}' to 'FES'", oldDeptCode), logs.Select(l => l.SL_Reference));
		}
	}
}
