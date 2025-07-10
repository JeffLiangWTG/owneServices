using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationAeoCertificateSupporterWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when JobDeclaration parameter is null", () => new JobDeclarationAeoCertificateSupporterWrapper(null));
	}

	public void TestProperties()
	{
		var supplier = Factory.New<OrgHeader>();
		var importer = Factory.New<OrgHeader>();
		var declarant = Factory.New<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions("Assert JobDeclaration as IAeoCertificateSupporter", () =>
		{
			var jobDeclarationAeoCertificateSupporter = new JobDeclarationAeoCertificateSupporterWrapper(declaration);

			AssertNull("Supplier", jobDeclarationAeoCertificateSupporter.Supplier);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNotNull("Supplier", jobDeclarationAeoCertificateSupporter.Supplier);
			AssertSame("Supplier", declaration.Supplier, jobDeclarationAeoCertificateSupporter.Supplier);

			AssertNull("Importer", jobDeclarationAeoCertificateSupporter.Importer);
			declaration.JE_OH_Importer = importer.PK;
			AssertNotNull("Importer", jobDeclarationAeoCertificateSupporter.Importer);
			AssertSame("Importer", declaration.Importer, jobDeclarationAeoCertificateSupporter.Importer);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNull("Declarant", jobDeclarationAeoCertificateSupporter.Declarant);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertNotNull("Declarant", jobDeclarationAeoCertificateSupporter.Declarant);
			AssertSame("Declarant", declaration.DeclarantAddress.Header, jobDeclarationAeoCertificateSupporter.Declarant);
		});
	}
}
