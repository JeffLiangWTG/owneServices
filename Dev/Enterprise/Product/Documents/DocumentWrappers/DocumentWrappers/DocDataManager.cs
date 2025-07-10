using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocDataManager : IDocDataManager
	{
		public DocDataManager(BusinessObject parentObject)
		{
			if (parentObject != null && parentObject is IDocumentSupportable)
			{
				DocNote = DocumentNote.LoadNote((IStmNoteParent)parentObject);
				Cache = new Dictionary<ZString, ZString>();
			}
		}

		readonly DocumentNote DocNote;
		readonly Dictionary<ZString, ZString> Cache;

		public ZString GetValue(ZString docDataIdentifier)
		{
			ZString result = ZString.Empty;
			if (DocNote != null)
			{
				if (!Cache.TryGetValue(docDataIdentifier, out result))
				{
					result = DocNote.GetSystemDefinedFieldValue(docDataIdentifier);
					Cache[docDataIdentifier] = result;
				}
			}
			return result;
		}
	}
}
