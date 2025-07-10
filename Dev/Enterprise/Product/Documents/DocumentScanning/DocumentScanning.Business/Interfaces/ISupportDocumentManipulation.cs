using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public interface IDocumentManipulationSupport
	{
		void DeleteDocumentsQuietly(ICollection<BusinessObject> documents);
		ICollection<BusinessObject> DeleteDocumentsPermanently(ICollection<BusinessObject> documents);
		void RestoreDocuments(ICollection<BusinessObject> documents);
		IEnumerable<BusinessObject> AllocateDocuments(IEnumerable<BusinessObject> documents);
		IEnumerable<BusinessObject> UnallocateDocuments(IEnumerable<BusinessObject> documents);
	}
}
