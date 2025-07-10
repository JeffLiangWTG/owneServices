using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IMergedPreviousDocumentsProvider
{
	ZString NBStatus { get; }
	IEnumerable<IMergedPreviousDocument> MergedPreviousDocuments { get; }
	ZInt LineNumber { get; }
	ZBool IsExport { get; }
}
