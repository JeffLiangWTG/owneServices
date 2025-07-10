using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	internal class EMCSProviderTest : TestCaseWithFactory
	{
		public void TestEMCSProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var invoiceHeader = Factory.New<EMCSJobDeclaration>().InvoiceHeader;
				var invoiceLine = Factory.New<EMCSJobComInvoiceLine_Exposed>();
				invoiceLine.JI_JZ = invoiceHeader.PK;
				var addInfo = invoiceLine.AddInfo;
				var provider = invoiceLine.EMCSProvider;

				CombineAssertions(() =>
				{
					AssertNotNull("EMCSProvider not null", provider);
					AssertType<EMCSProvider>("EMCSProvider-type", provider);
					AssertType<EMCSJobComInvoiceLineValidation>("EMCSJobComInvoiceLineValidation-type", provider.GetNewValidation(invoiceLine));
					AssertType<EMCSJobComInvoiceLineLookups>("EMCSJobComInvoiceLineLookups-type", provider.GetNewLookups(invoiceLine));
					AssertType<EMCSAddInfoJobComInvoiceLineValidation>("EMCSAddInfoJobComInvoiceLineValidation-type", provider.GetNewAddInfoValidation(addInfo));
					AssertType<EMCSAddInfoJobComInvoiceLineLookups>("EMCSAddInfoJobComInvoiceLineLookups-type", provider.GetNewAddInfoLookups(addInfo));
					AssertType<EMCSJobComInvoiceHeaderValidation>("EMCSJobComInvoiceHeaderValidation-type", provider.GetNewInvoiceHeaderValidation(invoiceHeader));
				});
			}
		}
	}

	class EMCSJobComInvoiceLine_Exposed : EMCSJobComInvoiceLine
	{
		public EMCSJobComInvoiceLine_Exposed(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EMCSAddInfoJobComInvoiceLine AddInfo => base.AddInfo;
	}
}
