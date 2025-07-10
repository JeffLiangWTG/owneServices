using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	static class StorageDocsCollectionHelper
	{
		public static IeDoc GetFromUniqueKey(Guid uniqueKey, IEnumerable elements)
		{
			foreach (IeDoc eDoc in elements)
			{
				if (eDoc.UniqueKey == uniqueKey)
				{
					return eDoc;
				}
			}
			return null;
		}

		public static IeDoc GetMostRecentEDoc(string docType, IEnumerable elements)
		{
			IeDoc result = null;
			foreach (IeDoc eDoc in elements)
			{
				if (!eDoc.IsDeleted && (eDoc.DocType == docType))
				{
					if ((result == null) || (GetLastChanged(eDoc) > GetLastChanged(result)))
					{
						result = eDoc;
					}
				}
			}
			return result;
		}

		public static bool ContainsEDocType(string docType, IEnumerable eDocs)
		{
			foreach (IeDoc eDoc in eDocs)
			{
				if(!eDoc.IsDeleted && (eDoc.DocType == docType))
				{
					return true;
				}
			}
			return false;
		}

		static ZDateTime GetLastChanged(IeDoc eDoc)
		{
			ZDateTime result = eDoc.LastEdited;
			if (result.IsEmpty)
			{
				result = eDoc.DateAdded;
			}
			return result;
		}
	}
}
