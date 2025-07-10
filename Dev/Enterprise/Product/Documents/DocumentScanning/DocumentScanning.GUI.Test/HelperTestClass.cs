using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	internal sealed class HelperTestClass : IDragDropSupport, IDocumentManipulationSupport
	{
		public void ResetCounts()
		{
			AddCount = 0;
			AppendToExistingCount = 0;

			DeleteDocumentsQuietlyCount = 0;
			DeleteDocumentsPermanentlyCount = 0;
			RestoreDocumentsCount = 0;
			AllocateDocumentsCount = 0;
			UnallocateDocumentsCount = 0;
		}

		#region IDragDropSupport Members

		public void Add(SerializableEDocCollection collection)
		{
			AddCount++;
		}
		public int AddCount;

		public void AppendToExisting(SerializableEDocCollection collection, StorageDocs document)
		{
			AppendToExistingCount++;
		}
		public int AppendToExistingCount;

		public string[] Add(string[] files)
		{
			AddCount++;
			return null;
		}

		public string[] AppendToExisting(string[] files, StorageDocs document)
		{
			AppendToExistingCount++;
			return null;
		}

		#endregion

		#region IDocumentManipulationSupport Members

		public void DeleteDocumentsQuietly(ICollection<BusinessObject> documents)
		{
			DeleteDocumentsQuietlyCount++;
			DeleteDocumentsQuietlyValue = documents;
		}
		public int DeleteDocumentsQuietlyCount;
		public IEnumerable<BusinessObject> DeleteDocumentsQuietlyValue;

		public ICollection<BusinessObject> DeleteDocumentsPermanently(ICollection<BusinessObject> documents)
		{
			DeleteDocumentsPermanentlyCount++;
			DeleteDocumentsPermanentlyValue = documents;

			return System.Array.Empty<BusinessObject>();
		}
		public int DeleteDocumentsPermanentlyCount;
		public IEnumerable<BusinessObject> DeleteDocumentsPermanentlyValue;

		public void RestoreDocuments(ICollection<BusinessObject> documents)
		{
			RestoreDocumentsCount++;
		}
		public int RestoreDocumentsCount;

		public IEnumerable<BusinessObject> AllocateDocuments(IEnumerable<BusinessObject> documents)
		{
			AllocateDocumentsCount++;
			return null;
		}
		public int AllocateDocumentsCount;

		public IEnumerable<BusinessObject> UnallocateDocuments(IEnumerable<BusinessObject> documents)
		{
			UnallocateDocumentsCount++;
			return null;
		}
		public int UnallocateDocumentsCount;

		#endregion
	}
}
