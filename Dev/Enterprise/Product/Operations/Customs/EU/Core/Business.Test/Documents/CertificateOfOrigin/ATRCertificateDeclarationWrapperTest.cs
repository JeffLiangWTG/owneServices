using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	sealed class ATRCertificateDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception with null declaration", () => new ATRCertificateDeclarationWrapper(null));
		}

		[ExpectNoExceptions]
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_TransportMode = "AIR";
			declaration.JE_VesselName = "ZIZZI";
			declaration.JE_GoodsOrigin = "IT";
			declaration.JE_GoodsDestination = "DE";

			OrgHeader testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testImporter.MainAddress.OA_Address1 = "Changi Airport";
			testImporter.MainAddress.OA_Address2 = "Building 3C";
			testImporter.OH_Code = "TEST";
			testImporter.PrimaryRegistrationNumber.Number = string.Empty;

			var importerAddress = Factory.New<OrgAddress>();
			importerAddress.OA_OH = testImporter.PK;
			importerAddress.OA_Address1 = "Eugene Leroy Street ";
			importerAddress.CompanyName = "test Declarant";

			OrgHeader testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Changi Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 3C";
			testSupplier.OH_Code = "TEST";

			var supplierAddress = Factory.New<OrgAddress>();
			supplierAddress.OA_OH = testSupplier.PK;
			supplierAddress.OA_Address1 = "Eugene Leroy Street ";
			supplierAddress.CompanyName = "test Declarant";

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;

			CombineAssertions("All properties", () =>
			{
				var wrapper = (IATRCertificateDeclaration)new ATRCertificateDeclarationWrapper(declaration);

				NUnit.Framework.Assert.That(wrapper.GoodsDestination, NUnit.Framework.Is.EqualTo("Germany").Using(CustomComparers.TypeComparison), "GoodsDestination");
				NUnit.Framework.Assert.That(wrapper.GoodsOrigin, NUnit.Framework.Is.EqualTo("Italy").Using(CustomComparers.TypeComparison), "GoodsOrigin");

				var exporterAddressString = declaration.SupplierDocumentaryAddress.Address.AddressFullFormatted;
				NUnit.Framework.Assert.That(wrapper.FullFormattedExporterAddress, NUnit.Framework.Is.EqualTo(exporterAddressString), "Exporter");

				var importerAddressString = declaration.ImporterDocumentaryAddress.Address.AddressFullFormatted;
				NUnit.Framework.Assert.That(wrapper.FullFormattedImporterDocumentaryAddress, NUnit.Framework.Is.EqualTo(importerAddressString), "Importer");
			});
		}
	}
}
