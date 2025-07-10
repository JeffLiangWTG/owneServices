using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Module.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Module.Test.Records
{
	[TestedType(typeof(ArchivedRecordsModule))]
	sealed class ArchivedRecordsModuleTest : TestCaseWithFactory
	{
		public void TestExcelExport()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var asm = factory.New<ArchiveStorageMain>();
			asm.SM_DB = 1;
			factory.Save();

			using var form = new ZChildForm();
			using var testModule = (ArchivedRecordsModule)ZModuleFactory.Instance.Create(ModuleIDs.ArchivedRecords);
			var reader = new BusinessObjectListReader(new BusinessObjectFactoryProvider(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory())), new ZQuery(), typeof(ArchiveStorageMain));
			var result = false;
			AssertNoExceptionThrown("ArchiveStorageMain allows itself to be stored in DocumentFactory", () => result = reader.HasRecords);
			AssertEquals("Has an ArchiveStorageMain", true, result);
			AssertEquals("Has exactly one ArchiveStorageMain", 1, reader.OfType<BusinessObject>().Count());
		}

		public void TestArchiveOffline()
		{
			Env.Security.ArchivedRecordsArchiveOffline.IsAllowed = true;
			using (SystemDataRegistry.Instance.EnableOfflineArchiving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using var testModule = (ArchivedRecordsModule)ZModuleFactory.Instance.Create(ModuleIDs.ArchivedRecords);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.HandleArchiveOffline_ForTestOnly(this, new EventArgs());
				Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Archive Offline should require being enabled in registry", "The Archive Offline functionality is currently disabled. Archive Offline can be enabled in the Registry at:  System > Archive Manager > Enable Offline Archiving.\r\nPlease note that the Archive Offline functionality is only available to self-hosted clients.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
