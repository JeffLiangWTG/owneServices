using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceApportionedCharge))]
	public class InvoiceApportionedChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertEquals("Lookups type", typeof(JobComInvHeaderChargeLookups), AppCharge.Lookups.GetType());
		}

		#region Implementation

		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<JobDeclaration>();
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;

		JobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = TestDec.Invoices.AddNew();
				}
				return fInvoice;
			}
		}
		JobComInvoiceHeader fInvoice;

		InvoiceApportionedCharge AppCharge
		{
			get
			{
				if (fAppCharge == null)
				{
					fAppCharge = Invoice.GroupCharges.AddNew();
				}
				return fAppCharge;
			}
		}

		InvoiceApportionedCharge fAppCharge;

		protected override BusinessObject GetNewBusinessObject()
		{
			return AppCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		#endregion
	}
}
