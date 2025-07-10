using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class ProductLastCostFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestImportProductLastCostFlatFile()
		{
			SetupTariffData();
			SetupNotificationGroup();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var partLastCostCsvPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.PartLastCost.csv", "PartLastCost.csv");
				var fileInfo = new FileInfo(partLastCostCsvPath);
				Assert("Test file not exists", fileInfo.Exists);

				var buffer = new NotificationBuffer();
				var importer = new ProductLastCostFlatFileDataImporter();
				importer.ImportFlatFile(fileInfo, buffer);
				AssertEmailSent("Product Last Cost CSV Import Succeeded", "T O T A L : Products Updated = 2, Part Last Cost Data Records Excluded = 0");

				var newTestFactory = new BusinessObjectFactory();
				var enterprisePart = LoadPart(newTestFactory, "BIC PEN");
				AssertEquals("Product Last Cost", 1.35m, enterprisePart.OP_LastCost);
				enterprisePart = LoadPart(newTestFactory, "912.226");
				AssertEquals("Product Last Cost", 56.90m, enterprisePart.OP_LastCost);
			}
		}

		public void TestImportProductLastCostFlatFile_InvalidHeader()
		{
			SetupNotificationGroup();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine("Invalid Header");
					streamWriter.Flush();
				}

				var dataImporter = new ProductLastCostFlatFileDataImporter();
				dataImporter.ImportFlatFile(new FileInfo(testFile.Filename), new NotificationBuffer());

				AssertEquals("1 Email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertNotNull("Email should not be null", email);
				AssertContains("Email Subject", "Product Last Cost CSV Import Failed", email.Subject);
				AssertContains("Email Body:", "File Header information is incorrect. The import of product last cost data requires a specific .CSV format file: \r\nPlease check the format in the template file attached.", email.Body);
				AssertEquals("Email should have 2 attachments", 2, email.Attachments.Count);
				AssertEquals("Email Attachment File Name 1", email.Attachments[0].DisplayName, Path.GetFileName(testFile.Filename));
				AssertEquals("Email Attachment File Name 2", email.Attachments[1].DisplayName, "Template File.csv");
				AssertMultilineASCIIEquals("Email Attachment Data 2", new OrgSupplierPartLastCostDataLoad().CSVTemplateHeading, Encoding.ASCII.GetString(email.Attachments[1].Data));
			}
		}

		#region Implementation

		void AssertEmailSent(string expectedPartOfSubject, string expectedPartOfBody)
		{
			AssertEquals("1 Email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Email should not be null", email);
			Assert("Email Subject", email.Subject.Contains(expectedPartOfSubject));
			AssertEquals("Email Body:", true, email.Body.IndexOf(expectedPartOfBody) > 0);
			AssertEquals("Email should have 1 attachment", 1, email.Attachments.Count);
			AssertEquals("Email Attachment File Name", email.Attachments[0].DisplayName, "PartLastCost.csv");
		}

		#region SetupNotificationGroup

		void SetupNotificationGroup()
		{
			Guid notificationGroupPK = NotificationDataRegistry.Instance.ProductImportNotificationGroup.Value;

			if (notificationGroupPK.GetHashCode() == 0)
			{
				notificationGroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery()).PK.ToGuid();
				NotificationDataRegistry.Instance.ProductImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroupPK);
			}
			GlbGroup notificationGroup = Factory.Load<GlbGroup>(notificationGroupPK);

			AssertNotNull("Debug Precondition: Group should not be null", notificationGroup);

			if (notificationGroup.Staff.Count == 0)
			{
				GlbStaff newStaffMember = Factory.New<GlbStaff>();
				notificationGroup.Staff.Add(newStaffMember);
			}

			foreach (GlbStaff staffMember in notificationGroup.Staff)
			{
				if (staffMember.GS_EmailAddress.IsEmpty)
				{
					staffMember.GS_EmailAddress = "test@test.com";
				}
			}
			Factory.Save();

			AssertNotNull("Debug Precondition: Group should not be null", notificationGroup);
			Assert("Debug Precondition: Group should have at least one staff member that was added in SetUp()", notificationGroup.Staff.Count > 0);
		}

		#endregion

		#region SetupTariffData

		void SetupTariffData()
		{
			int numberOfProducts = Factory.GetDatabaseCount(typeof(OrgSupplierPart));

			Classification class1 = Factory.New<Classification>();
			class1.CC_Description = "SHOES";
			class1.CC_IsActive = true;
			class1.CC_LookupCode = "SHOES";
			class1.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			class1.CC_TariffNum = "6402.19.00 02";
			class1.CC_ClassificationType = Classification.ClassificationType.IMP;

			Classification class2 = Factory.New<Classification>();
			class2.CC_Description = "PENS";
			class2.CC_IsActive = true;
			class2.CC_LookupCode = "PENS";
			class2.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			class2.CC_TariffNum = "9608.10.00 47";
			class2.CC_ClassificationType = Classification.ClassificationType.IMP;

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			OrgHeader owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";

			OrgSupplierPart testPart1 = Factory.New<OrgSupplierPart>();
			testPart1.OP_PartNum = "BIC PEN";
			testPart1.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Owner);

			OrgSupplierPart testPart2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart2.OP_PartNum = "912.226";
			testPart2.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

			Factory.Save();
			int numberOfProductsAfterSetup = Factory.GetDatabaseCount(typeof(OrgSupplierPart));
			AssertEquals("Precondition - Products expected", numberOfProducts + 2, numberOfProductsAfterSetup);
		}

		#endregion

		OrgSupplierPart LoadPart(BusinessObjectFactory testFactory, ZString lookupPart)
		{
			ZQuery partFilter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, lookupPart);
			OrgSupplierPart enterprisePart = testFactory.LoadTop1<OrgSupplierPart>(partFilter);
			AssertNotNull("Expecting Part " + lookupPart + " to be found", enterprisePart);
			return enterprisePart;
		}

		#endregion
	}
}
