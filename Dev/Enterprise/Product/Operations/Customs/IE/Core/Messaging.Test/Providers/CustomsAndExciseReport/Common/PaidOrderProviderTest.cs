using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class PaidOrderProviderTest : TestCaseWithFactory
	{
		PaidOrderProvider provider;

		public void TestMrn()
		{
			provider = new PaidOrderProvider(new PaidOrder { Mrn = "22IEDUB4BBFC22PER2" });
			AssertEquals("22IEDUB4BBFC22PER2", provider.Mrn);
		}
		public void TestMrn_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Mrn), 1, "MRN");

		public void TestVersion()
		{
			provider = new PaidOrderProvider(new PaidOrder { Version = 1 });
			AssertEquals(1, provider.Version);
		}
		public void TestVersion_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Version), 2, "Version");

		public void TestAmendment()
		{
			provider = new PaidOrderProvider(new PaidOrder { Amendment = false });
			AssertEquals(false, provider.Amendment);
		}
		public void TestAmendment_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Amendment), 3, "Amendment");

		public void TestDeclarationMsgType()
		{
			provider = new PaidOrderProvider(new PaidOrder { DeclarationMsgType = "H1" });
			AssertEquals("H1", provider.DeclarationMsgType);
		}
		public void TestDeclarationMsgType_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.DeclarationMsgType), 4, "Declaration Message Type");

		public void TestPayer()
		{
			provider = new PaidOrderProvider(new PaidOrder { Payer = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Payer);
		}
		public void TestPayer_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Payer), 5, "Payer");

		public void TestImporter()
		{
			provider = new PaidOrderProvider(new PaidOrder { Importer = "IE1234567A" });
			AssertEquals("IE1234567A", provider.Importer);
		}
		public void TestImporter_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Importer), 6, "Importer");

		public void TestDeclarant()
		{
			provider = new PaidOrderProvider(new PaidOrder { Declarant = "IE7654321A" });
			AssertEquals("IE7654321A", provider.Declarant);
		}
		public void TestDeclarant_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Declarant), 7, "Declarant");

		public void TestDeclarantName()
		{
			provider = new PaidOrderProvider(new PaidOrder { DeclarantName = "MR Test Murphy" });
			AssertEquals("MR Test Murphy", provider.DeclarantName);
		}
		public void TestDeclarantName_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.DeclarantName), 8, "Declarant Name");

		public void TestDtReceived()
		{
			provider = new PaidOrderProvider(new PaidOrder { DtReceived = "2022-08-11T10:56:11.903+0100" });
			AssertEquals("2022-08-11T10:56:11.903+0100", provider.DtReceived);
		}
		public void TestDtReceived_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.DtReceived), 9, "Received Date");

		public void TestTaxTotal()
		{
			provider = new PaidOrderProvider(new PaidOrder { TaxTotal = 200.0M });
			AssertEquals(200.0M, provider.TaxTotal);
		}
		public void TestTaxTotal_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.TaxTotal), 10, "Tax Total");

		public void TestTotalDuty()
		{
			provider = new PaidOrderProvider(new PaidOrder { TotalDuty = 150.0M });
			AssertEquals(150.0M, provider.TotalDuty);
		}
		public void TestTotalDuty_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.TotalDuty), 11, "Total Duty");

		public void TestVatOnDuty()
		{
			provider = new PaidOrderProvider(new PaidOrder { VatOnDuty = 50.0M });
			AssertEquals(50.0M, provider.VatOnDuty);
		}
		public void TestVatOnDuty_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.VatOnDuty), 12, "VAT On Duty");

		public void TestTotalExcise()
		{
			provider = new PaidOrderProvider(new PaidOrder { TotalExcise = 50.0M });
			AssertEquals(50.0M, provider.TotalExcise);
		}
		public void TestTotalExcise_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.TotalExcise), 13, "Total Excise");

		public void TestVatOnExcise()
		{
			provider = new PaidOrderProvider(new PaidOrder { VatOnExcise = 50.0M });
			AssertEquals(50.0M, provider.VatOnExcise);
		}
		public void TestVatOnExcise_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.VatOnExcise), 14, "VAT On Excise");

		public void TestPostponedVat()
		{
			provider = new PaidOrderProvider(new PaidOrder { PostponedVat = 150.0M });
			AssertEquals(150.0M, provider.PostponedVat);
		}
		public void TestPostponedVat_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.PostponedVat), 15, "Postponed VAT");

		public void TestLrn()
		{
			provider = new PaidOrderProvider(new PaidOrder { Lrn = "EXA214094_06AaxY" });
			AssertEquals("EXA214094_06AaxY", provider.Lrn);
		}
		public void TestLrn_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Lrn), 16, "LRN");

		public void TestUcr()
		{
			provider = new PaidOrderProvider(new PaidOrder { Ucr = "123422342" });
			AssertEquals("123422342", provider.Ucr);
		}
		public void TestUcr_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.Ucr), 17, "UCR");

		public void TestCommercialTransportDoc()
		{
			provider = new PaidOrderProvider(new PaidOrder { CommercialTransportDoc = "N703124242" });
			AssertEquals("N703124242", provider.CommercialTransportDoc);
		}
		public void TestCommercialTransportDoc_XlsxField() => typeof(PaidOrderProvider).TestXlsxField(nameof(PaidOrderProvider.CommercialTransportDoc), 18, "Commercial Transport Document");
	}
}
