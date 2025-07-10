using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[CountrySpecificTest("AU")]
	sealed class ProductFlatFileImporterTest : TestCaseWithFactory
	{
		public void TestImportProductFlatFile()
		{
			SetupLookUp();
			SetupNotificationGroup();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var productsCsvPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Products.csv", "Products.csv");
				var fileInfo = new FileInfo(productsCsvPath);
				Assert("Test file not exists", fileInfo.Exists);

				var numberOfProducts = Factory.GetDatabaseCount(typeof(OrgSupplierPart));

				var buffer = new NotificationBuffer();
				FlatFileImporter importer = new ProductFlatFileImporter();

				SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				importer.ImportFlatFile(fileInfo, buffer);

				var numberOfProductsAfterImport = Factory.GetDatabaseCount(typeof(OrgSupplierPart));
				SystemDataRegistry.Instance.UpdateProductsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				AssertEquals("Products were created in the import", numberOfProducts + 2, numberOfProductsAfterImport);
				AssertEquals("No Emails should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				numberOfProducts = Factory.GetDatabaseCount(typeof(OrgSupplierPart));
				AssertEquals("Precondition: Registry 'update product on import' is set to true", true, SystemDataRegistry.Instance.UpdateProductsDuringAutomaticImport.Value);
				importer.ImportFlatFile(fileInfo, buffer);
				numberOfProductsAfterImport = Factory.GetDatabaseCount(typeof(OrgSupplierPart));

				AssertEquals("Products were not created in the import", numberOfProducts, numberOfProductsAfterImport);
				AssertEmailSent("Product CSV Import Succeeded", "T O T A L : Products created = 0, Products updated = 2, Products excluded = 0");

				Env.OutgoingMailManager.EmailsCreated.Clear();
				numberOfProducts = Factory.GetDatabaseCount(typeof(OrgSupplierPart));
				SystemDataRegistry.Instance.UpdateProductsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				importer.ImportFlatFile(fileInfo, buffer);
				numberOfProductsAfterImport = Factory.GetDatabaseCount(typeof(OrgSupplierPart));

				AssertEquals("Products were not created in the import", numberOfProducts, numberOfProductsAfterImport);
				AssertEmailSent("Product CSV Import Failed", "T O T A L : Products created = 0, Products updated = 0, Products excluded = 2");
			}
		}

		public void TestImportProductFlatFile_InvalidHeader()
		{
			SetupNotificationGroup();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine("Invalid Header");
					streamWriter.Flush();
				}

				var dataImporter = new ProductFlatFileImporter();
				dataImporter.ImportFlatFile(new FileInfo(testFile.Filename), new NotificationBuffer());

				AssertEquals("1 Email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertNotNull("Email should not be null", email);
				AssertContains("Email Subject", "Product CSV Import Failed", email.Subject);
				AssertContains("Email Body:", "File Header information is incorrect. The import of product data requires a specific .CSV format file: \r\nPlease check the format in the template file attached.", email.Body);
				AssertEquals("Email should have 2 attachments", 2, email.Attachments.Count);
				AssertEquals("Email Attachment File Name 1", email.Attachments[0].DisplayName, Path.GetFileName(testFile.Filename));
				AssertEquals("Email Attachment File Name 2", email.Attachments[1].DisplayName, "Template File.csv");
				AssertMultilineASCIIEquals("Email Attachment Data 2", OrgSupplierPartDataLoad.New().CSVTemplateHeading, Encoding.ASCII.GetString(email.Attachments[1].Data));
			}
		}

		#region Implementation

		void AssertEmailSent(string expectedPartOfSubject, string expectedPartOfBody)
		{
			AssertEquals("1 Email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Email should not be null", email);
			Assert("Email Subject", email.Subject.Contains(expectedPartOfSubject));
			AssertEquals("Email Body", true, email.Body.IndexOf(expectedPartOfBody) > 0);
			AssertEquals("Email should have 1 attachment", 1, email.Attachments.Count);
			AssertEquals("Email Attachment File Name", email.Attachments[0].DisplayName, "Products.csv");
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

		#region SetupLookUp

		void SetupLookUp()
		{
			Classification class1 = Factory.New<Classification>();
			class1.CC_Description = "PIG";
			class1.CC_IsActive = true;
			class1.CC_LookupCode = "PIG";
			class1.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			class1.CC_TariffNum = "2001.10.00.90";
			class1.CC_ClassificationType = Classification.ClassificationType.EXP;

			Classification class2 = Factory.New<Classification>();
			class2.CC_Description = "CATTLE";
			class2.CC_IsActive = true;
			class2.CC_LookupCode = "CATTLE";
			class2.CC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			class2.CC_TariffNum = "7013.39.00 41";
			class2.CC_ClassificationType = Classification.ClassificationType.IMP;

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			OrgHeader owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";

			Factory.Save();
		}

		#endregion

		#endregion
	}
}
