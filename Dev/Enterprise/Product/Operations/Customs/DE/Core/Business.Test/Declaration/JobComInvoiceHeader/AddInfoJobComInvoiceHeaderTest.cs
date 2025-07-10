using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_AgreedPlaceCode_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Import", 1, invoiceHeader.ZG_AgreedPlaceCodeInfo.MaxLength);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export", 5, invoiceHeader.ZG_AgreedPlaceCodeInfo.MaxLength);
			});
		}
		public void TestZG_AgreedPlaceCode_MaxLength_Fallback()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("Not Set", 5, invoiceHeader.ZG_AgreedPlaceCodeInfo.MaxLength);

				invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export", 5, invoiceHeader.ZG_AgreedPlaceCodeInfo.MaxLength);

				invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import", 1, invoiceHeader.ZG_AgreedPlaceCodeInfo.MaxLength);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
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
