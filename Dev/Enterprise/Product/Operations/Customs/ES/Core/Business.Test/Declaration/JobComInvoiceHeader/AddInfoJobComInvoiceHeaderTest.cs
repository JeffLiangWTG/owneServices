using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceHeaderAddInfo = invoiceHeader.AddInfo;
			AssertType<AddInfoJobComInvoiceHeaderValidation>(invoiceHeaderAddInfo.Validation);
		}

		public void TestValidation_Export()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceHeaderAddInfo = invoiceHeader.AddInfo;
			AssertType<ExportAddInfoJobComInvoiceHeaderValidation>(invoiceHeaderAddInfo.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			return new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
