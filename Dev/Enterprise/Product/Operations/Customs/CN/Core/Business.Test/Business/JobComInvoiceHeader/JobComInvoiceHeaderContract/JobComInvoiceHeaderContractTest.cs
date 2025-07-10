using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderContract))]
	class JobComInvoiceHeaderContractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMaxLength()
		{
			AssertEquals(32, HeaderRefCTR.J2_ReferenceNumberInfo.MaxLength);
		}

		public void TestDefaultValues()
		{
			AssertEquals("CTR", HeaderRefCTR.J2_ReferenceType);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(JobComInvoiceHeaderContractValidation), HeaderRefCTR.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals(typeof(JobComInvoiceHeaderRefsLookups), HeaderRefCTR.Lookups.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return HeaderRefCTR;
		}

		JobComInvoiceHeaderContract HeaderRefCTR
		{
			get
			{
				if (fHeaderRefCTR == null)
				{
					var header = Factory.New<JobComInvoiceHeader>();
					fHeaderRefCTR = header.ContractNumbers.AddNew();
				}

				return fHeaderRefCTR;
			}
		}

		JobComInvoiceHeaderContract fHeaderRefCTR;
	}
}
