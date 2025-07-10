using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class PODFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestPODFlatFileImportHasErrorsNotification()
		{
			SetupNotificationGroup();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var podsCsvPath = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.PODs.csv", "PODs.csv");
				var fileInfo = new FileInfo(podsCsvPath);
				Assert("Test file not exists", fileInfo.Exists);

				Importer.ImportFlatFile(fileInfo, Buffer);

				AssertEquals("One email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];

				AssertNotNull("Email should not be null", email);

				AssertEquals("1 attachment expected", 1, email.Attachments.Count);
				AssertEquals("Email Subject", string.Format("POD CSV Import Failed - {0}", fileInfo.Name), email.Subject);

				AssertMultilineEquals("Email.Body",
	@$"PODS to Import = 6
Row 2 ignored: Job Number 'C00001001' is invalid. The Job Number should begin with 'S' or 'B'.
Row 4 ignored: Delivery Date Time '20080303333333' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.
Row 5 ignored: Job # 'S00001003' could not be found in {Core.Constants.ProductName}.
Row 7 ignored: Delivery Date Time '20080606666666' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.

T O T A L : PODS created = 1, PODS updated = 1, PODS excluded = 4

", email.Body, '\n');
			}
		}

		public void TestPODFlatFileImport_InvalidHeader()
		{
			SetupNotificationGroup();

			using (var testFile = TempFile.New())
			{
				using (var streamWriter = new StreamWriter(testFile.Filename))
				{
					streamWriter.WriteLine("Invalid Header");
					streamWriter.Flush();
				}

				Importer.ImportFlatFile(new FileInfo(testFile.Filename), Buffer);

				AssertEquals("One Email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertNotNull("Email should not be null", email);
				AssertContains("Email Subject", "POD CSV Import Failed", email.Subject);
				AssertContains("Email Body:", "File Header information is incorrect. The import of POD data requires a specific .CSV format file. \r\nPlease check the format in the template file attached.", email.Body);
				AssertEquals("Email should have 2 attachments", 2, email.Attachments.Count);
				AssertEquals("Email Attachment File Name 1", email.Attachments[0].DisplayName, Path.GetFileName(testFile.Filename));
				AssertEndsWith("Email Attachment File Name 2", "csv", email.Attachments[1].DisplayName);
				AssertMultilineASCIIEquals("Email Attachment Data 2", new PODDataLoader().CSVTemplateHeading, Encoding.ASCII.GetString(email.Attachments[1].Data));
			}
		}

		public void TestPODFlatFileImportNoErrorsNotification()
		{
			SetupNotificationGroup();

			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (TempFile testFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFile.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S00001001,20080202020202");
					sw.WriteLine("B00001001,20080404040404");
					sw.Flush();
				}

				FileInfo fileInfo = new FileInfo(testFile.Filename);

				if (fileInfo.Exists)
				{
					Importer.ImportFlatFile(fileInfo, Buffer);

					AssertEquals("One email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

					AssertNotNull("Email should not be null", email);

					AssertEquals("1 attachment expected", 1, email.Attachments.Count);
					AssertEquals("Email Subject", String.Format("POD CSV Import Succeeded - {0}", fileInfo.Name), email.Subject);
					AssertMultilineEquals("Email.Body",
@"PODS to Import = 2

T O T A L : PODS created = 1, PODS updated = 1, PODS excluded = 0

", email.Body, '\n');
				}
			}
		}

		public void TestPODFlatFileImportNoNotificationsSent()
		{
			SetupNotificationGroup();

			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (TempFile testFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFile.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S00001001,20080202020202");
					sw.WriteLine("B00001001,20080404040404");
					sw.Flush();
				}

				FileInfo fileInfo = new FileInfo(testFile.Filename);

				if (fileInfo.Exists)
				{
					Importer.ImportFlatFile(fileInfo, Buffer);

					AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				}
			}
		}

		#region Implementation

		#region SetupNotificationGroup

		void SetupNotificationGroup()
		{
			Guid notificationGroupPK = NotificationDataRegistry.Instance.PODImportNotificationGroup.Value;

			if (notificationGroupPK.GetHashCode() == 0)
			{
				notificationGroupPK = Core.Constants.Groups.AllPK;
				NotificationDataRegistry.Instance.PODImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroupPK);
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

		NotificationBuffer Buffer;
		PODFlatFileDataImporter Importer;

		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		BaseJobDeclaration Declaration1;
		BaseJobDeclaration Declaration2;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			Shipment1.JS_UniqueConsignRef = "S00001001";
			Shipment2.JS_UniqueConsignRef = "S00001002";

			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment2.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;

			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			Shipment2.JS_OuterPacks = 20;
			Shipment2.JS_ActualWeight = 24;
			Shipment2.JS_ActualVolume = 28;

			Declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Declaration1.JE_DeclarationReference = "B00001001";
			Declaration2.JE_DeclarationReference = "B00001002";

			Declaration1.JE_CartageCompleted = ZDateTime.Empty;
			Declaration2.JE_CartageCompleted = ZDateTime.Empty;

			Factory.Save();

			Buffer = new NotificationBuffer();
			Importer = new PODFlatFileDataImporter();
		}

		#endregion
	}
}
