using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	internal static class StorageDocsBaseExtensions
	{
		public static StorageDocsBase GetDocumentInNewFactory(this StorageDocsBase element)
		{
			NumberedBusinessObjectFactory factory = new NumberedBusinessObjectFactory(element.ParentMain.SM_DB, new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()))
			{
				NameForDebugging = "SizeOfDocInDifferentFactory",
				RefreshEnabled = false
			};

			return element.IsImageFile ? factory.Load<StorageDocs>(element.PK) : factory.Load<StorageFile>(element.PK);
		}
	}
}
