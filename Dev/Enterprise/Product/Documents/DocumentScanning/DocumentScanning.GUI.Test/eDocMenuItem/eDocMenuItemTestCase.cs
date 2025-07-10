using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	public abstract class eDocMenuItemTestCase : TestCaseWithDocumentFactory
	{
		[ExpectNoExceptions]
		public void TestGetEnabledStatusDoesNotThrowNullException()
		{
			eDocMenuItem menuItem = GetMenuItem();
			menuItem.GetEnabledStatus(null, false);
			menuItem.GetEnabledStatus(null, true);

			StorageDocs document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			menuItem.GetEnabledStatus(document, false);
			menuItem.GetEnabledStatus(document, true);

			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			menuItem.GetEnabledStatus(file, false);
			menuItem.GetEnabledStatus(file, true);

			StorageDocsUnallocated documentUnallocated = MasterFactory.New<StorageDocsUnallocated>();
			menuItem.GetEnabledStatus(documentUnallocated, false);
			menuItem.GetEnabledStatus(documentUnallocated, true);
		}

		protected abstract eDocMenuItem GetMenuItem();

		[RequiresSTA]
		public void TestGetEnabledStatus()
		{
			eDocMenuItem menuItem = GetMenuItem();
			StorageDocs document = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			RunAssertionsForGetEnabledStatus(menuItem, document);
			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			RunAssertionsForGetEnabledStatus(menuItem, file);
			StorageDocsUnallocated documentUnallocated = MasterFactory.New<StorageDocsUnallocated>();
			RunAssertionsForGetEnabledStatus(menuItem, documentUnallocated);
		}

		protected abstract void RunAssertionsForGetEnabledStatus(eDocMenuItem menuItem, StorageDocsBase bizO);

		public virtual void TestGetEnabledStatusWhenMultipleElementsSelected()
		{
			StorageDocs document1 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			StorageDocs document2 = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);

			eDocMenuItem menuItem = GetMenuItem();
			Assert(menuItem.GetEnabledStatusWhenMultipleElementsSelected(new StorageDocs[] { document1, document2 }));
		}
	}
}
