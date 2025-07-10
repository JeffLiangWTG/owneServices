using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetNewValidation_Export()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertType<ExportAddInfoJobComInvoiceLineValidation>(invoiceLine.AddInfoValidation);
		}

		public void TestGetNewValidation_Import()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertType<ImportAddInfoJobComInvoiceLineValidation>(invoiceLine.AddInfoValidation);
		}

		public void TestGetNewValidation_Miscellaneous()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<AddInfoJobComInvoiceLineValidation>(invoiceLine.AddInfoValidation);
		}

		public void TestGetNewValidation_WAD()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertType<WarehouseAdjustmentAddInfoJobComInvoiceLineValidation>(invoiceLine.AddInfoValidation);
		}

		public void TestGetNewLookups()
		{
			AssertType<AddInfoJobComInvoiceLineLookups>(invoiceLine.AddInfoLookups);
		}

		protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
