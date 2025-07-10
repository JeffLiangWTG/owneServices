using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public interface INBWrappableBusinessObject
{
	ZInt LineNumber { get; }
	ZBool IsImport { get; }
	ZBool IsExport { get; }
	IEnumerable<GroupedPreviousDocument> NBGroupedPreviousDocuments { get; }
	RegCusEntryNumberWrapper EntryNumberWrapper { get; }
}
