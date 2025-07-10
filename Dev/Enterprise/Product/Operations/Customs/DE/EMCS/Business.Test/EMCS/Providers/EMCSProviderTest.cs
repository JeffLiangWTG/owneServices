using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	internal class EMCSProviderTest : TestCaseWithFactory
	{
		public void TestEMCSProvider() => AssertType<EMCSProvider>(provider);

		public void TestEMCSJobComInvHeaderValidation() => AssertType<EMCSJobComInvoiceHeaderValidation>(provider.GetNewInvoiceHeaderValidation(invoiceHeader));

		public void TestEMCSAddInfoJobComInvoiceLineValidation()
		{
			var addInfo = invoiceLine.AddInfo;
			AssertType<EMCSAddInfoJobComInvoiceLineValidation>(provider.GetNewAddInfoValidation(addInfo));
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<EMCSJobDeclaration>().InvoiceHeader;
			invoiceLine = Factory.New<EMCSJobComInvoiceLineForTest>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			provider = invoiceLine.EMCSProvider;
		}
		EU.EMCS.Business.EMCSProvider provider;
		EU.EMCS.Business.EMCSJobComInvoiceHeader invoiceHeader;
		EMCSJobComInvoiceLineForTest invoiceLine;
	}

	class EMCSJobComInvoiceLineForTest : EU.EMCS.Business.EMCSJobComInvoiceLine
	{
		public EMCSJobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EU.EMCS.Business.EMCSAddInfoJobComInvoiceLine AddInfo => base.AddInfo;
	}
}
