using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.NADDataImport.Testing
{
	class NADDataImporterTest : TestCaseWithFactory
	{
		public void TestOrganisationsCreatedInFactory()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			int organisationsInFactory = Factory.GetDatabaseCount(typeof(OrgHeader));
			NotificationBuffer notify = new NotificationBuffer();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var fileName = "TNTMVS1.20030520051205NADUPDCMS.NADUPD";
				var sourcePath = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.NADInterface.TestFiles." + fileName);
				var destPath = Path.Combine(TNTDataRegistry.Instance.NADFileSourceDirectory, fileName);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
			}
			NADDataImporter importer = new NADDataImporter(notify);
			FileInfo[] files = (new DirectoryInfo(TNTDataRegistry.Instance.NADFileSourceDirectory)).GetFiles();
			importer.ProcessFiles(files);
			int noOfOrganisationsAfterImport = Factory.GetDatabaseCount(typeof(OrgHeader));
			AssertEquals("Organisations created should have been:", 23, noOfOrganisationsAfterImport - organisationsInFactory);
			OrgHeader org1 = LoadOrganisation("AUST SPORTS DRUG AGENCY *test");
			AssertEquals("Org name", "AUST SPORTS DRUG AGENCY *test".ToUpper(), org1.OH_FullName);
			AssertEquals("Org address", "L 6 866 MAIN RD", org1.MainAddress.OA_Address1);
			AssertEquals("Org city", "WOOLLOONGABBA", org1.MainAddress.OA_City);
			AssertEquals("Org state", "QLD", org1.MainAddress.OA_State);
			AssertEquals("Org locode", "AUBNE", org1.OH_RL_NKClosestPort);
			AssertEquals("Org postcode", "4102", org1.MainAddress.OA_PostCode);
			OrgHeader org2 = LoadOrganisation("ONESTEEL REINFORCING P/L *test");
			AssertEquals("Org name", "ONESTEEL REINFORCING P/L *test".ToUpper(), org2.OH_FullName);
			AssertEquals("Org address", "327 KIEWA ST", org2.MainAddress.OA_Address1);
			AssertEquals("Org city", "ALBURY", org2.MainAddress.OA_City);
			AssertEquals("Org state", "NSW", org2.MainAddress.OA_State);
			AssertEquals("Org locode", "AUSYD", org2.OH_RL_NKClosestPort);
			AssertEquals("Org phone", "+61 2 6021 1466", org2.MainAddress.OA_Phone_Formatted);
			AssertEquals("Org fax", "+61 2 6041 1594", org2.MainAddress.OA_Fax_Formatted);
			AssertEquals("Org should be a debtor", true, org2.OH_IsDebtor);
			AssertEquals("Org should be a consignee", true, org2.OH_IsConsignor);
			AssertEquals("Org postcode", "2640", org2.MainAddress.OA_PostCode);
			AssertEquals("Cuscode records", 2, org2.CustomsCodes.Count);
			if (org2.CustomsCodes[0].OK_CodeType == "GST")
			{
				AssertEquals("ABN value", "22 004 148 289", org2.CustomsCodes[0].OK_CustomsRegNo);
			}
			else
			{
				AssertEquals("ABN no type", "GST", org2.CustomsCodes[1].OK_CodeType);
				AssertEquals("ABN value", "22 004 148 289", org2.CustomsCodes[1].OK_CustomsRegNo);
			}

			if (org2.CustomsCodes[1].OK_CodeType == "LSC")
			{
				AssertEquals("Legacy Account value", "98762831", org2.CustomsCodes[1].OK_CustomsRegNo);
			}
			else
			{
				AssertEquals("Legacy Account type", "LSC", org2.CustomsCodes[0].OK_CodeType);
				AssertEquals("Legacy Account value", "98762831", org2.CustomsCodes[0].OK_CustomsRegNo);
			}

			AssertEquals(1, org2.Contacts.Count);
			OrgContact orgContact2 = org2.Contacts[0];
			AssertEquals(org2.PK, orgContact2.OC_OH);
			AssertEquals("MR PAUL RIDOUT", orgContact2.OC_ContactName);
			AssertEquals("+61260211466", orgContact2.OC_Phone);
			AssertEquals("Contact notification should be PRN", Core.Constants.ContactNotifyModes.Print, orgContact2.OC_NotifyMode);
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var fileName = "TNTMVS1.200410141220NADUPDCMS.NADUPD";
				var sourcePath = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.NADInterface.TestFiles." + fileName);
				var destPath = Path.Combine(TNTDataRegistry.Instance.NADFileSourceDirectory, fileName);
				TestHelper.CopySourceFileToImportDirectory(sourcePath, destPath);
			}
			files = (new DirectoryInfo(TNTDataRegistry.Instance.NADFileSourceDirectory)).GetFiles();
			NADDataImporter importer2 = new NADDataImporter(new NotificationBuffer());
			importer2.ProcessFiles(files);
			OrgHeader org3 = LoadOrganisation("ONESTEEL REINFORCING P/L *test");
			AssertEquals("Cuscode records", 2, org3.CustomsCodes.Count);
			if (org3.CustomsCodes[0].OK_CodeType == "GST")
			{
				AssertEquals("ABN value should not have been updated", "22 004 148 289", org3.CustomsCodes[0].OK_CustomsRegNo);
			}
			else
			{
				AssertEquals("ABN no type", "GST", org3.CustomsCodes[1].OK_CodeType);
				AssertEquals("ABN value should not have been updated", "22 004 148 289", org3.CustomsCodes[1].OK_CustomsRegNo);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			TestHelper.SetupNADDataImportRegistries();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestHelper.TidyUp();
		}

		OrgHeader LoadOrganisation(ZString lookupOrg)
		{
			ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, lookupOrg);
			OrgHeader enterpriseOrganisation = Factory.LoadTop1<OrgHeader>(orgFilter);
			AssertNotNull(enterpriseOrganisation);
			return enterpriseOrganisation;
		}

		TNTTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TNTTestHelper(Factory));
			}
		}

		TNTTestHelper testHelper;
		#endregion
	}
}
