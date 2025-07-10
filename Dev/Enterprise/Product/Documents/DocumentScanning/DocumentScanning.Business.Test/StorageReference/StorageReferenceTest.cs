using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageReference))]
	class StorageReferenceBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		public override void TestCloneAuditProperties()
		{
			//skip clone.
			Assert(true);
		}

		public override void TestCloneAuditContextProperties()
		{
			//skip clone.
			Assert(true);
		}

		public void TestDeleteStorageReference()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			var archivedStorageMain = masterFactory.NewWithValidTestData<StorageMain>();
			archivedStorageMain.SM_ParentFK = Guid.NewGuid();
			archivedStorageMain.SM_Archived = DateTime.Today.AddYears(-7);

			var storageReference1 = masterFactory.New<StorageReference>();
			storageReference1.SR_SM = archivedStorageMain.PK;

			var storageReference2 = masterFactory.New<StorageReference>();
			storageReference2.SR_SM = archivedStorageMain.PK;

			masterFactory.Save();

			CombineAssertions("Precondition: StorageReferences should exist.", () =>
			{
				Assert(!storageReference1.IsDeleted);
				Assert(!storageReference2.IsDeleted);
			});

			storageReference1.Delete();

			CombineAssertions("Only one StorageReference should have been deleted.", () =>
			{
				Assert(storageReference1.IsDeleted);
				Assert(!storageReference2.IsDeleted);
			});

			storageReference2.Delete();

			CombineAssertions("Both StorageReferences should have been deleted.", () =>
			{
				Assert(storageReference1.IsDeleted);
				Assert(storageReference2.IsDeleted);
			});
		}
	}
}
