using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.DE.Business.Documents;
public static class DocumentSupporterHelper
{
	public static DocumentWrapper[] GetSADHWrappers(CusEntryHeader entryHeader)
	{
		var result = GetSADHWrapper(entryHeader);
		return result != null ? new[] { result } : Array.Empty<DocumentWrapper>();
	}

	public static DocumentWrapper[] GetSADHWrappers(IEnumerable<CusEntryHeader> entryHeaders)
	{
		var documentWrappers = new List<DocumentWrapper>();
		foreach (var entryHeader in entryHeaders)
		{
			var wrapper = GetSADHWrapper(entryHeader);
			if (wrapper != null)
			{
				documentWrappers.Add(wrapper);
			}
		}
		return documentWrappers.ToArray();
	}

	public static DocumentWrapper[] GetCusEntryHeaderWrappers(CusEntryHeader entryHeader)
	{
		var result = GetCusEntryHeaderWrapper(entryHeader);
		return result != null ? new[] { result } : Array.Empty<DocumentWrapper>();
	}

	public static DocumentWrapper[] GetCusEntryHeaderWrappers(IEnumerable<CusEntryHeader> entryHeaders)
	{
		var documentWrappers = new List<DocumentWrapper>();
		foreach (var entryHeader in entryHeaders.ToArray())
		{
			var wrapper = GetCusEntryHeaderWrapper(entryHeader);
			if (wrapper != null)
			{
				documentWrappers.Add(wrapper);
			}
		}
		return documentWrappers.ToArray();
	}

	static DocumentWrapper GetSADHWrapper(CusEntryHeader entryHeader) => DEDocSADH.New(entryHeader, entryHeader.Factory);

	static DocumentWrapper GetCusEntryHeaderWrapper(CusEntryHeader entryHeader) => DocCusEntryHeader.New(entryHeader, entryHeader.Factory);
}
