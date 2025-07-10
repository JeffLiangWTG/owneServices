using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.DocumentWrappers.DocARInvoiceLine;

namespace Enterprise.DocumentWrappers.Accounting.Testing
{
	sealed class DocARInvoiceLineGenericTransactionSupporterTest : TestCaseWithFactory
	{
		ARInvoiceLine line;
		DocARInvoiceLine invoiceLineWrapper;
		DocARInvoiceLineGenericTransactionSupporter invoiceLineSupporter;

		protected override void SetUp()
		{
			base.SetUp();

			line = Factory.New<ARInvoiceLine>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "BR1";
			line.AL_GB = branch.PK;
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "DP1";
			line.AL_GE = department.PK;
			invoiceLineWrapper = DocARInvoiceLine.New(line, Factory);
			invoiceLineSupporter = new DocARInvoiceLineGenericTransactionSupporter(invoiceLineWrapper);
		}

		public void TestGetBranch()
		{
			AssertEquals("BR1", invoiceLineSupporter.GetBranch().Code);
		}

		public void TestGetDepartment()
		{
			AssertEquals("DP1", invoiceLineSupporter.GetDepartment().Code);
		}
	}
}
