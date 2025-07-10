using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(MercosulForeignDeclarationCollection))]
	class MercosulForeignDeclarationCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<MercosulForeignDeclaration>
	{
		protected override Customs.Business.CusSupportingInfoCollection<MercosulForeignDeclaration> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new MercosulForeignDeclarationCollection(jobComInvoice);
		}

		public void TestSetDefaultsForNewChild()
		{
			var oDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			oDeclaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			var oInvoice = oDeclaration.Invoices.AddNew();
			var oInvoiceLine = oInvoice.InvoiceLines.AddNew();
			var merc = oInvoiceLine.MercosulForeignDeclarations.AddNew();
			AssertEquals("CSI_SubType", ZString.Empty, merc.CSI_SubType);

			oInvoiceLine.MercosulForeignDeclarationType = CertificateTypeList.Codes.CCPTC;
			merc = oInvoiceLine.MercosulForeignDeclarations.AddNew();
			AssertEquals("CSI_SubType", CertificateTypeList.Codes.CCPTC, merc.CSI_SubType);
		}
	}
}
