using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.DocumentScanning.Web
{
	public class EDocsWebHelper : IEDocsWebHelper
	{
		public IEnumerable<IeDocBase> GetEDocs(List<ZGuid> parentPKs)
		{
			var parentDocuments = GetPublishedEDocsAndFiles(parentPKs);
			if (parentDocuments != null)
			{
				return parentDocuments.ToList<IeDocBase>();
			}
			else
			{
				return Enumerable.Empty<IeDocBase>();
			}
		}

		public bool HasEDocs(List<ZGuid> parentPKs)
		{
			var parentDocuments = GetPublishedEDocsAndFiles(parentPKs);
			if (parentDocuments != null)
			{
				return parentDocuments.Any();
			}
			else
			{
				return false;
			}
		}

		public IEnumerable<IeDocBase> GetEDocsByDocType(List<ZGuid> parentPKs, string docType)
		{
			var parentDocuments = GetPublishedEDocsAndFiles(parentPKs);
			if (parentDocuments != null)
			{
				return parentDocuments.Where(doc => doc.SC_DocType == docType).ToList<IeDocBase>();
			}
			else
			{
				return Enumerable.Empty<IeDocBase>();
			}
		}

		public bool HasEDocsByDocType(List<ZGuid> parentPKs, string docType)
		{
			var parentDocuments = GetPublishedEDocsAndFiles(parentPKs);
			if (parentDocuments != null)
			{
				return parentDocuments.Any(doc => doc.SC_DocType == docType);
			}
			else
			{
				return false;
			}
		}

		IEnumerable<StorageDocsBase> GetPublishedEDocsAndFiles(List<ZGuid> parentPKs)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			List<StorageDocsBase> result = new List<StorageDocsBase>();

			foreach (var parentPK in parentPKs)
			{
				var parent = masterFactory.GetStorageMainForPK(parentPK);
				if (parent != null)
				{
					var publishedEdocs = parent.PublishedEDocsAndFiles.Cast<StorageDocsBase>();

					var siteUser = WebEnv.AppInstance?.SiteUser as OrgContactWebUser;
					var filteredEDocs = siteUser != null ?
						publishedEdocs.Where(doc => siteUser.CanViewDocument(masterFactory.FactoryForEverythingExceptEDocs, doc.DocType)) :
						publishedEdocs;

					if (filteredEDocs != null)
					{
						result.AddRange(filteredEDocs);
					}
				}
			}
			return result;
		}
	}
}
