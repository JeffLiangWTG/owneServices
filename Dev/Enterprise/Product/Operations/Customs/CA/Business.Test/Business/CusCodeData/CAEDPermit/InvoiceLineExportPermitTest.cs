using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineExportPermit))]
	sealed class InvoiceLineExportPermitTest : Customs.Business.Testing.CusCodeDataTest<InvoiceLineExportPermit>
	{
		public void TestValidation()
		{
			InvoiceLineExportPermit permit = Factory.New<InvoiceLineExportPermit>();
			AssertEquals("Validation", typeof(InvoiceLineExportPermitValidation), permit.Validation.GetType());
		}

		public void TestLookups()
		{
			InvoiceLineExportPermit permit = Factory.New<InvoiceLineExportPermit>();
			AssertEquals("Validation", typeof(CusCodeDataLookups), permit.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			InvoiceLineExportPermit permit = Factory.New<InvoiceLineExportPermit>();
			AssertEquals(CusCodeDataTypeList.Codes.Permit, permit.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.Permit, permit.CY_Code);
		}

		public void TestParent()
		{
			InvoiceLineExportPermit permit = Factory.New<InvoiceLineExportPermit>();
			permit.CY_ParentID = InvoiceLine.PK;
			permit.CY_ParentTableCode = InvoiceLine.TablePrefix;
			AssertEquals(InvoiceLine, permit.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().Permits.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.Permits.AddNew();
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
