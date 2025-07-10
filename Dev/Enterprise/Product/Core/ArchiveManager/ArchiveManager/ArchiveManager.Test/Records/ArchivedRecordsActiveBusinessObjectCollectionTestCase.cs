using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.DocumentScanning.Business;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(ArchiveStorageMainCollection))]
	class ArchivedRecordsActiveBusinessObjectCollectionTestCase : ActiveBusinessObjectCollectionTestCase<ArchiveStorageMainCollection>
	{
		protected override BusinessObjectFactory NewFactory()
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		protected override ArchiveStorageMainCollection GetCollectionToTest()
		{
			var storage1 = ((DocumentFactory)Factory).New<ArchiveStorageMain>();
			storage1.SM_Archived = ZDateTime.Today;
			storage1.SM_DB = 1;
			Factory.Save();

			return new ArchiveStorageMainCollection(Factory);
		}

		public override void TestDelete()
		{
			// cant delete
			Assert(true);
		}

		public override void TestAdd()
		{
			// cant add to it.
			Assert(true);
		}

		public override void TestAddNew()
		{
			// cant add to it
			Assert(true);
		}
	}
}
