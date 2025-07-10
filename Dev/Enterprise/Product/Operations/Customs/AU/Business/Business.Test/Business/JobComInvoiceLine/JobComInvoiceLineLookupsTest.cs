using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineLookupsTest : TestCaseWithFactory
	{
		public void TestClassificationListForImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(ImportClassificationCollection), invoiceLine.Lookups.ClassificationList.GetType());
		}

		public void TestClassificationListForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(ExportClassificationCollection), invoiceLine.Lookups.ClassificationList.GetType());
		}

		public void TestClassificationListForDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			AssertEquals(typeof(ImportClassificationCollection), invoiceLine.Lookups.ClassificationList.GetType());
		}

		public void TestClassificationListForMissingDeclaration()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(typeof(BaseClassificationCollection<BaseCusClassification>), invoiceLine.Lookups.ClassificationList.GetType());
		}

		public void TestSuppliersList()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			var supplierList = invoiceLine.Lookups.Suppliers;
			AssertEquals(typeof(ConsignorCollection), supplierList.GetType());
			AssertEquals("Can remove the consignor filtering", true, ((ConsignorCollection)supplierList).AllowOtherOrgTypes);
			Assert("Has Customs Code filter to assist locating Suppliers with CID codes", supplierList.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Any(f => f.FilterName == OrgConstants.FilterControl.DropEditRelationships.Code.CustomsCodeType));
		}
	}
}
