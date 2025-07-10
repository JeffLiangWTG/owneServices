using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CertificateOfOriginCollection))]
	class CertificateOfOriginCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CertificateOfOrigin>
	{
		protected override Customs.Business.CusSupportingInfoCollection<CertificateOfOrigin> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new CertificateOfOriginCollection(jobComInvoice);
		}
	}
}
