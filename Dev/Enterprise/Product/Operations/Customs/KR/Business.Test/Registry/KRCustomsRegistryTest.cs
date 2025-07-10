using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.IO;
using Enterprise.Customs.Business.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.KR.Business.KRCustomsRegistry;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(KRCustomsRegistry))]
	sealed class KRCustomsRegistryTest : RegistryItemSetTestCaseWithFactory<KRCustomsRegistry>
	{
		public void TestExportEmailGroup()
		{
			TestRegistryItem(ItemSet.ExportEmailGroup,
				"ExportEmailGroup",
				Categories.Customs_KoreaSouth_Export,
				"Email Recipients Group",
				"The staff group that will be notified when no original sender has been found while processing the response message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.CannotCallParameterlessValueGetter,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
			AssertEquals(1, ItemSet.ExportEmailGroup.CountryFilterPKs.Count());
			Assert(ItemSet.ExportEmailGroup.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestImportEmailGroup()
		{
			TestRegistryItem(ItemSet.ImportEmailGroup,
				"ImportEmailGroup",
				Categories.Customs_KoreaSouth_Import,
				"Email Recipients Group",
				"The staff group that will be notified when no original sender has been found while processing the response message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.CannotCallParameterlessValueGetter,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
			AssertEquals(1, ItemSet.ImportEmailGroup.CountryFilterPKs.Count());
			Assert(ItemSet.ImportEmailGroup.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestImportStatementEmailGroup()
		{
			TestRegistryItem(ItemSet.ImportStatementEmailGroup,
				"ImportStatementEmailGroup",
				Categories.Customs_KoreaSouth_ImportStatement,
				"Email Recipients Group",
				"The staff group that will be notified when no original sender has been found while processing the response message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.CannotCallParameterlessValueGetter,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
			AssertEquals(1, ItemSet.ImportStatementEmailGroup.CountryFilterPKs.Count());
			Assert(ItemSet.ImportStatementEmailGroup.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestLocalExportEmailGroup()
		{
			TestRegistryItem(ItemSet.LocalExportEmailGroup,
				"LocalExportEmailGroup",
				Categories.Customs_KoreaSouth_LocalExport,
				"Email Recipients Group",
				"The staff group that will be notified when no original sender has been found while processing the response message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.CannotCallParameterlessValueGetter,
				RegistryFindBoxCollection.GlbGroup,
				RegistryConstants.GroupPKs.Notification);
			AssertEquals(1, ItemSet.LocalExportEmailGroup.CountryFilterPKs.Count());
			Assert(ItemSet.LocalExportEmailGroup.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestUnispassDeclarantID()
		{
			TestRegistryItem(ItemSet.UNIPASSDeclarantID,
				"UNIPASSDeclarantID",
				Categories.Customs_KoreaSouth,
				"UNIPASS Declarant ID",
				"5-character UNIPASS Declarant ID",
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				TextEditorType.TextBox,
				"");
			AssertEquals(1, ItemSet.UNIPASSDeclarantID.CountryFilterPKs.Count());
			Assert(ItemSet.UNIPASSDeclarantID.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestCompanyName()
		{
			TestRegistryItem(ItemSet.CompanyName,
				"CompanyName",
				Categories.Customs_KoreaSouth_Declarant,
				"Company Name",
				"Company Name",
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				TextEditorType.TextBox,
				"");
			AssertEquals(1, ItemSet.CompanyName.CountryFilterPKs.Count());
			Assert(ItemSet.CompanyName.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestRepresentativeName()
		{
			TestRegistryItem(ItemSet.RepresentativeName,
				"RepresentativeName",
				Categories.Customs_KoreaSouth_Declarant,
				"Representative Name",
				"Representative Name",
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				TextEditorType.TextBox,
				"");
			AssertEquals(1, ItemSet.RepresentativeName.CountryFilterPKs.Count());
			Assert(ItemSet.RepresentativeName.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestCustomsWebAddress()
		{
			TestRegistryItem(ItemSet.CustomsWebAddress,
				"KRCustomsPublicKeyWebAddress",
				Categories.Customs_KoreaSouth,
				"Customs Certificate Address",
				"Web address to get Customs certificate",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				TextEditorType.TextBox,
				"https://gsg.customs.go.kr:38120/mediate/gsg/usw/getResponse/message?code=X509");
		}

		public void TestCustomsCertificate()
		{
			TestRegistryItem(ItemSet.CustomsCertificate,
				"KRCustomsPublicKeyCertificate",
				Categories.Customs_KoreaSouth,
				"Customs Public Certificate",
				"Customs Public Certificate",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory | RegistryOptions.IsOnlyForSupport,
				Array.Empty<byte>());

			var certServer = ItemSet.CustomsCertificate.Value;
			var x509Certificate2 = new X509Certificate2(certServer);
#if NETFRAMEWORK
			AssertEquals("x509Certificate2 has no handle", (IntPtr)0, x509Certificate2.Handle);
#else
			AssertEquals("x509Certificate2 has no handle", 0, x509Certificate2.Handle);
#endif
			var fileReader = new TestFileReader(typeof(KRCustomsRegistryTest));
			var certFromTestFile = fileReader.GetEmbeddedFileData(TestFilesPath, "x509_Server.der");
			ItemSet.CustomsCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certFromTestFile);
			certServer = ItemSet.CustomsCertificate.Value;
			x509Certificate2 = new X509Certificate2(certServer);

#if NETFRAMEWORK
			AssertNotEquals("x509Certificate2 has a public key", (IntPtr)0, x509Certificate2.Handle);
#else
			AssertNotEquals("x509Certificate2 has a public key", 0, x509Certificate2.Handle);
#endif
			AssertEquals("CN=signGATE CA5, OU=AccreditedCA, O=KICA, C=KR", x509Certificate2.IssuerName.Name);
			AssertEquals("063B83C5", x509Certificate2.SerialNumber);
		}

		public void TestEnableToSaveImportDeclaration()
		{
			TestRegistryItem(ItemSet.EnableToSaveImportDeclaration,
				"Enable to save Import declaration",
				Categories.Customs_KoreaSouth_Import,
				"Enable to save Import declaration",
				"As development is in progress, Import cannot be saved if it is not CW1 Support",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
			AssertEquals(1, ItemSet.EnableToSaveImportDeclaration.CountryFilterPKs.Count());
			Assert(ItemSet.EnableToSaveImportDeclaration.CountryFilterPKs.Contains(Core.CountryGuids.Instance.KoreaRepublicof));
		}

		public void TestFamilyRelations()
		{
			FamilyRelationRegistryItem item = ItemSet.FamilyRelations;

			AssertEquals("FamilyRelations", item.Name);
			AssertEquals("Family Relations", item.Caption);
			AssertEquals("This list indicates the relationships to the declarant who moves to Korea. Users can add items except codes 00 to 10.", item.Hint);
			AssertEquals(Categories.Customs_KoreaSouth_Import, item.Category);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);

			AssertEquals(11, item.Value.Count);
			AssertEquals("00", item.Value[0].Code);
			AssertEquals("01", item.Value[1].Code);
			AssertEquals("02", item.Value[2].Code);
			AssertEquals("03", item.Value[3].Code);
			AssertEquals("04", item.Value[4].Code);
			AssertEquals("05", item.Value[5].Code);
			AssertEquals("06", item.Value[6].Code);
			AssertEquals("07", item.Value[7].Code);
			AssertEquals("08", item.Value[8].Code);
			AssertEquals("09", item.Value[9].Code);
			AssertEquals("10", item.Value[10].Code);
		}

		public TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(KRCustomsRegistryTest)));
		TestFileReader fileReader;

		public TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Certificate";

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}
	}
}
