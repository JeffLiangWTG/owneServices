using System;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class LocalCartageImportManagerTest : TestCaseWithFactory
	{
		public void TestBookingImport()
		{
			var jobCartageBookingSamplePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.JobCartageBookingSample.xml");
			AssertImportIsRanProperly(jobCartageBookingSamplePath, "LocalCartageBookingImporter");
		}

		public void TestStatusImport()
		{
			var jobCartageStatusSamplePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.JobCartageStatusSample.xml");
			AssertImportIsRanProperly(jobCartageStatusSamplePath, "LocalCartageStatusImporter");
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		void AssertImportIsRanProperly(ZString pathToFile, ZString expectedNotification)
		{
			var result = false;
			var importer = new LocalCartageDataImporterForTest();
			var buffer = new NotificationBuffer(new NotificationBuffer());
			using (TextReader reader = new StreamReader(pathToFile, Encoding.UTF8))
			{
				result = importer.ImportDataToFactory(reader, pathToFile, buffer, SourceInfo.EmptySourceInfo, out var additionalTransactionActions);
			}
			AssertEquals("Import should have been successful", true, result);
			AssertEquals(expectedNotification, true, buffer.AsString.IndexOf(expectedNotification) > -1);
		}

		sealed class LocalCartageDataImporterForTest : LocalCartageDataImporter
		{
			protected override LocalCartageBookingImporter GetNewBookingImporter()
			{
				return new LocalCartageBookingImporterForTest();
			}

			protected override LocalCartageStatusImporter GetNewStatusImporter()
			{
				return new LocalCartageStatusImporterForTest();
			}
		}

		sealed class LocalCartageBookingImporterForTest : LocalCartageBookingImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				notifications.Notify(new InfoNotification("LocalCartageBookingImporter"));
				return base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}
		}

		sealed class LocalCartageStatusImporterForTest : LocalCartageStatusImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				notifications.Notify(new InfoNotification("LocalCartageStatusImporter"));
				return base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}
		}
	}
}
