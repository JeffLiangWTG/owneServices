using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageFileCollection))]
	public class StorageFileCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return StorageFile.New_DEBUG(MasterFactory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageFileCollection(MasterFactory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		public void TestTypeOfElements()
		{
			StorageFileCollection collection = new StorageFileCollection(MasterFactory);
			AssertEquals("TypeOfElements", typeof(StorageFile), collection.TypeOfElements);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		DocumentFactory MasterFactory;
	}
}
