using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SuspensionDrawbackCollection))]
	class SuspensionDrawbackCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SuspensionDrawback>
	{
		protected override Customs.Business.CusSupportingInfoCollection<SuspensionDrawback> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new SuspensionDrawbackCollection(jobComInvoice);
		}

		public void TestSetDefaultsForNewChild()
		{
			var oDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			oDeclaration.JE_MessageType = "EXP";
			var oInvoice = oDeclaration.Invoices.AddNew();
			oInvoice.JZ_IncoTerm = "TET";
			oInvoice.JZ_InvoiceAmount = 1000;
			oInvoice.JZ_RX_NKInvoice_Currency = "BRL";
			var oInvoiceLine = oInvoice.InvoiceLines.AddNew();
			oInvoiceLine.JI_Tariff = "11111111";
			var oDrawback = oInvoiceLine.SuspensionDrawbackCollection.AddNew();
			AssertEquals("CSI_Tariff", oDrawback.CSI_Tariff, oInvoiceLine.JI_Tariff);
		}
	}
}
