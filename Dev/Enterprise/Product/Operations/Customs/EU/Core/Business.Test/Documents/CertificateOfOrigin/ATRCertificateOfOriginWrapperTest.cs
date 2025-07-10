using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	public class ATRCertificateOfOriginWrapperTest : CertificateOfOriginWrapperTest
	{
		[ExpectNoExceptions]
		public void TestDeclaration()
		{
			declaration.JE_TransportMode = "ROA";
			declaration.JE_GoodsOrigin = "IT";
			declaration.JE_GoodsDestination = "DE";
			declaration.JE_VesselName = "JAZZ";

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

			CombineAssertions("Declaration must match Wrapper declaration", () =>
			{
				NUnit.Framework.Assert.That(wrapper.Declaration.GoodsOrigin, NUnit.Framework.Is.EqualTo("Italy").Using(CustomComparers.TypeComparison), "GoodsOrigin");
				NUnit.Framework.Assert.That(wrapper.Declaration.GoodsDestination, NUnit.Framework.Is.EqualTo("Germany").Using(CustomComparers.TypeComparison), "GoodsDestination");
				var exporterAddressString = declaration.SupplierDocumentaryAddress.Address.AddressFullFormatted;
				NUnit.Framework.Assert.That(wrapper.Declaration.FullFormattedExporterAddress, NUnit.Framework.Is.EqualTo(exporterAddressString), "Exporter");

				var importerAddressString = declaration.ImporterDocumentaryAddress.Address.AddressFullFormatted;
				NUnit.Framework.Assert.That(wrapper.Declaration.FullFormattedImporterDocumentaryAddress, NUnit.Framework.Is.EqualTo(importerAddressString), "Importer");
			});
		}

		[ExpectNoExceptions]
		public void TestATRBoxItemBuilder()
		{
			NUnit.Framework.Assert.That(wrapper.ATRBoxItemBuilder, NUnit.Framework.Is.TypeOf<ATRBoxItemsWrapper>());
		}

		[ExpectNoExceptions]
		public void TestTotalATRCertificateItem()
		{
			NUnit.Framework.Assert.That(wrapper.TotalATRCertificateItem, NUnit.Framework.Is.EqualTo(default(IATRCertificateItem)));
		}

		[ExpectNoExceptions]
		public void TestARTNumberCaption()
		{
			NUnit.Framework.Assert.That(wrapper.ARTNumberCaption, NUnit.Framework.Is.EqualTo("A.TR.No").Using(CustomComparers.TypeComparison), "ART Number Caption expected A.TR.No");
		}

		[ExpectNoExceptions]
		public void TestARTEuropeanUnionCaption()
		{
			NUnit.Framework.Assert.That(wrapper.ARTEuropeanUnionCaption, NUnit.Framework.Is.EqualTo("EUROPEAN UNION").Using(CustomComparers.TypeComparison), "ART European Union Caption expected EUROPEAN UNION");
		}

		[ExpectNoExceptions]
		public void TestShouldAddTotalCertificateItem()
		{
			NUnit.Framework.Assert.That(wrapper.ShouldAddTotalCertificateItem, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "ShouldAddTotalCertificateItem expected false");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			wrapper = new ATRCertificateOfOriginWrapper(entryHeader);
		}
		IATRCertificateOfOrigin wrapper;
		CusEntryHeader entryHeader;
		JobDeclaration declaration;
	}
}
