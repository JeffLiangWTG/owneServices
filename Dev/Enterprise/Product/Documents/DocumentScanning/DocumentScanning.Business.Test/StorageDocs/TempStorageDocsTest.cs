using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(TempStorageDocs))]
	sealed class TempStorageDocsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var doc = MasterFactory.NewWithParent(typeof(TempStorageDocs));
			return doc;
		}

		public void TestTempDocNotSavedOnSaving()
		{
			var tempDoc = MasterFactory.New<TempStorageDocs>();
			Assert("Before save - Temp doc is not in db", !tempDoc.IsInDatabase);
			MasterFactory.Save();
			Assert("After save - temp doc is still not in db", !tempDoc.IsInDatabase);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		DocumentFactory MasterFactory { get { return (DocumentFactory)Factory; } }
	}
}
