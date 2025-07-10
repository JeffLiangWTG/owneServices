using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Module.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(ArchivedRecordsModule))]
	class ArchivedRecordsModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
			=> ModuleIDs.ArchivedRecords;

		protected override void AddTestObjects(CargoWise.EntityFramework.IBusinessObjectCollection collection)
		{
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);

			var storage1 = docFactory.New<ArchiveStorageMain>();
			storage1.SM_Archived = ZDateTime.Today;
			storage1.SM_DB = 1;
			docFactory.Save();

			var storagecollection = new ArchiveStorageMainCollection(docFactory);

			base.AddTestObjects(storagecollection);
		}
	}
}
