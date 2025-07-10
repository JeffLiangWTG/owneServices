using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderCCNs))]
	sealed class JobComInvoiceHeaderCCNsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValue()
		{
			AssertEquals(JobComInvoiceHeaderCCNs.Constants.CCN, invoiceCCN.J2_ReferenceType);
		}

		public void TestLookups()
		{
			AssertType<JobComInvoiceHeaderCCNsLookups>(invoiceCCN.Lookups);
		}

		public void TestValidation()
		{
			AssertType<JobComInvoiceHeaderCCNsValidation>(invoiceCCN.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => invoiceCCN;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			invoiceCCN = header.CargoControlNumbersList.AddNew();
		}
		JobComInvoiceHeaderCCNs invoiceCCN;
	}
}
