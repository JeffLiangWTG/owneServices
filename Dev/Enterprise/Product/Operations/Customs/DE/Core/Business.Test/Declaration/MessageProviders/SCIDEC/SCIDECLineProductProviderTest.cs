using System;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SCIDECLineProductProviderTest : Customs.Business.Testing.DataProviderTestCase<SCIDECLineProductProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIDECLineProductProvider(null));
		}

		public void TestGoodsDescription()
		{
			product.CSI_Description = "Some goods description";
			AssertEquals("Some goods description", Provider.GoodsDescription);
		}

		public void TestYieldType()
		{
			product.CSI_SubType = ImportSubStyleList.Codes.F;
			AssertEquals(ImportSubStyleList.Codes.F, Provider.YieldType);
		}

		public void TestYieldRate()
		{
			product.CSI_AdditionalDescription = "Some yield rate";
			AssertEquals("Some yield rate", Provider.YieldRate);
		}

		public void TestCombinedNomenclatureCode()
		{
			product.CSI_Tariff = "19059090";
			AssertEquals("19059090", Provider.CombinedNomenclatureCode);
		}

		protected override SCIDECLineProductProvider GetProvider() => new SCIDECLineProductProvider(product);

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			product = invoiceLine.InwardProcessingProducts.AddNew();
		}
		InwardProcessingProduct product;
	}
}
