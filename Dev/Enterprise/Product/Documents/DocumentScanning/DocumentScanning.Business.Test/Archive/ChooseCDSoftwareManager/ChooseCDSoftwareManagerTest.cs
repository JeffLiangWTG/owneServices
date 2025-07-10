using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(ChooseCDSoftwareManager))]
	public class ChooseCDSoftwareManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChooseCDSoftwareManager(MasterFactory, ArchiveManager);
		}

		public void TestArchiveManager()
		{
			AssertEquals("Archive manager is the same instance that was passed in", ArchiveManager, CDSoftwareManager.ArchiveManager);
		}

		public void TestMasterFactory()
		{
			AssertEquals("Factory is the same instance we passed in", MasterFactory, CDSoftwareManager.MasterFactory);
		}

		public void TestUseOwn()
		{
			Assert("UseOwn is default", CDSoftwareManager.UseOwn);
			Assert("UseXP should be set to false", !CDSoftwareManager.UseXP);

			CDSoftwareManager.UseXP = true;
			Assert("UseOwn should now be set to false", !CDSoftwareManager.UseOwn);
		}

		public void TestUseXP()
		{
			Assert("UseXP false by default", !CDSoftwareManager.UseXP);

			CDSoftwareManager.UseXP = true;
			Assert("UseXP now true", CDSoftwareManager.UseXP);
			Assert("UseOwn should be set to false", !CDSoftwareManager.UseOwn);

			CDSoftwareManager.UseOwn = true;
			Assert("UseXP now false", !CDSoftwareManager.UseXP);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			ArchiveManager = new ArchiveEDocsManager(MasterFactory);
			CDSoftwareManager = new ChooseCDSoftwareManager(MasterFactory, ArchiveManager);
		}

		DocumentFactory MasterFactory;
		ArchiveEDocsManager ArchiveManager;
		ChooseCDSoftwareManager CDSoftwareManager;
	}
}
