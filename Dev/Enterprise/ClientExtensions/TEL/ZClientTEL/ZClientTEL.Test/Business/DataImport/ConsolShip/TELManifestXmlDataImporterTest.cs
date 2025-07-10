using System;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TEL.Import.Testing
{
	public class TELManifestXmlDataImporterTest : DataTransfer.Business.Testing.XmlDataImporterTest
	{
		public void TestWithInvalidEnvironment()
		{
			Assert("has errors", !importer.CheckEnvironmentValid(Factory, notify));
			Assert("notification group pk error", notify.AsString.Contains("Please set the Registry > TEL Client Extensions > Import of Consols and Shipments > Notification Group"));
			Assert("notification group registry error", notify.AsString.Contains("Please set the Registry > TEL Client Extensions > Import of Consols and Shipments > Email Subject Identifier"));
		}

		public void TestImportData()
		{
			TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "bob the builder");
			GlbGroup notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = "NOT";
			GlbStaff user = notificationGroup.Staff.AddNew();
			user.GS_Code = "BOB";
			user.GS_IsActive = true;
			user.GS_EmailAddress = "test@test.com";
			TELDataRegistry.Instance.ConsolShipImportNotificationGroupPKItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());
			int consolCount = Factory.GetDatabaseCount(typeof(ForwardingConsol));
			int shipmentCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			using (StreamReader reader = new StreamReader(pathToFile))
			{
				importer.ImportData(reader, pathToFile, notify, SourceInfo.EmptySourceInfo);
			}

			AssertEquals("A new consol has been imported", consolCount + 1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
			AssertEquals("A new shipment has been imported", shipmentCount + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
		}

#region Implementation
		ZString pathToFile = ZString.Empty;
		TELConsolShipXmlDataImporter importer;
		NotificationBuffer notify;
		EmbeddedResourceRetriever resourceRetriever;
		protected override void SetUp()
		{
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			pathToFile = resourceRetriever.SaveResourceToFile("single.xml");
			importer = new TELConsolShipXmlDataImporter();
			notify = new NotificationBuffer();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
#endregion
	}
}
