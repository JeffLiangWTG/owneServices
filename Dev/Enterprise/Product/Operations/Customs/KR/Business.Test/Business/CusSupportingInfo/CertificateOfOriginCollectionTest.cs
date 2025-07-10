using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CertificateOfOriginCollection))]
	sealed class CertificateOfOriginCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CertificateOfOrigin>
	{
		public void TestConstructors()
		{
			CertificateOfOriginCollection collection = null;
			var invoiceHeader = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			AssertNoExceptionThrown(() => collection = new CertificateOfOriginCollection(invoiceHeader));
			var certificateOfOrigin = collection.AddNew();
			AssertEquals(JobComInvoiceHeaderSchema.Constants.Prefix, certificateOfOrigin.CSI_ParentTableCode);
			AssertEquals(CusSupportingInfoTypeList.Codes.CertificateOfOrigin, certificateOfOrigin.CSI_Type);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertNoExceptionThrown(() => collection = new CertificateOfOriginCollection(invoiceLine));
			certificateOfOrigin = collection.AddNew();
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, certificateOfOrigin.CSI_ParentTableCode);
			AssertEquals(CusSupportingInfoTypeList.Codes.CertificateOfOrigin, certificateOfOrigin.CSI_Type);
		}

		protected override CusSupportingInfoCollection<CertificateOfOrigin> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new CertificateOfOriginCollection(jobComInvoice);
		}
	}
}
