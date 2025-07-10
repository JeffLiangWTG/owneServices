using System;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(ImportLicenseJobComInvoiceHeaderValidation);

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
		}
	}
}
