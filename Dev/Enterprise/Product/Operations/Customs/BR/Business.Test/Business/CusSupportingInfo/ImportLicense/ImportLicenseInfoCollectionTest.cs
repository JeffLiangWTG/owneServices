using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseInfoCollection))]
	class ImportLicenseInfoCollectionTest : CusSupportingInfoCollectionTest<ImportLicenseInfo>
	{
		protected override CusSupportingInfoCollection<ImportLicenseInfo> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return new ImportLicenseInfoCollection(invoiceLine);
		}
	}
}
