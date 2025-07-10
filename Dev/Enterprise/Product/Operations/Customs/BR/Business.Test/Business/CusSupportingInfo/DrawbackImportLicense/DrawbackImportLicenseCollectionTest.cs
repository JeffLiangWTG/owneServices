using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DrawbackImportLicenseCollection))]
	public class DrawbackImportLicenseCollectionTest : CusSupportingInfoCollectionTest<DrawbackImportLicense>
	{
		protected override Customs.Business.CusSupportingInfoCollection<DrawbackImportLicense> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new DrawbackImportLicenseCollection(jobComInvoice);
		}
	}
}

