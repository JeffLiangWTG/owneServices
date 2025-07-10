using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportAddInfoJobComInvoiceHeaderValidation>(invoiceHeaderAddInfo.Validation);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportAddInfoJobComInvoiceHeaderValidation>(invoiceHeaderAddInfo.Validation);
			declaration.JE_MessageType = "XY$";
			AssertType<AddInfoJobComInvoiceHeaderValidation>(invoiceHeaderAddInfo.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return invoiceHeaderAddInfo;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeaderAddInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
		}
		JobDeclaration declaration;
		AddInfoJobComInvoiceHeader invoiceHeaderAddInfo;
	}
}
