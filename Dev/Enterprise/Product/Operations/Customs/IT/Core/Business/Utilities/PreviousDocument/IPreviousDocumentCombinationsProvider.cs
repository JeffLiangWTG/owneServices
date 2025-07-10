using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business;

public interface IPreviousDocumentCombinationsProvider
{
	IReadOnlyCollection<PreviousDocumentCombinationItem> GetAllowedPreviousDocumentCombinations();

	PreviousDocumentFieldsInfo GetNewSettings(PreviousDocumentCombinationTemplate? template = null);
}
