using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(GlobalDrawbackCusEntryLineCollection))]
	public class GlobalDrawbackCusEntryLineCollectionTest : BusinessObjectCollectionTestCase
	{
		[TestDate(2018, 03, 20)]
		public void TestExportDrawbackDate()
		{
			var invoice1 = declaration.Invoices[0];
			invoice1.JZ_InvoiceDate = ZDateTime.Empty;
			var filterCollection = new GlobalDrawbackCusEntryLineCollection(Factory, invoiceLine);
			Assert("Pre-condition: Filter does not contain drawback date default", !filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Drawback Date:Property2"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice1 = declaration.Invoices[0];
			invoice1.JZ_InvoiceDate = ZDateTime.Now.AddDays(-12);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDate = ZDateTime.Now.AddDays(-15);
			AssertEquals("ExportDrawbackDate", new ZDateTime(2018, 03, 05), filterCollection.ExportDrawbackDate);
			filterCollection = new GlobalDrawbackCusEntryLineCollection(Factory, invoiceLine);
			Assert(filterCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Drawback Date:Property2"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalDrawbackCusEntryLineCollection(Factory);
		}

		#region Implementation
		//OrgHeader Importer;
		BaseJobDeclaration declaration;
		BaseJobComInvoiceLine invoiceLine;
		//OrgSupplierPart Product;

		protected override void SetUp()
		{
			base.SetUp();
			//Importer = Factory.New<OrgHeader>();
			//Importer.OH_Code = "Importer";
			declaration = Factory.New<BaseJobDeclaration>();
			//Declaration.JE_OH_Importer = Importer.PK;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			//Product = Factory.New<OrgSupplierPart>();
			//Product.OP_PartNum = "TestPartNum";
			//OrgPartRelation Relation = Product.RelatedOrganisations.AddNew();
			//Relation.OU_Relationship = "OWN";
			//Relation.OU_OH = Importer.PK;
			//InvoiceLine.JI_PartNo = Product.OP_PartNum;
			invoiceLine.JI_Tariff = "12345678";
		}
		#endregion
	}
}
