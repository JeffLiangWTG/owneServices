using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business.Declaration;

public static class SupportingDocumentCollectionExtensions
{
	public static IEnumerable<SupportingDocument> GetDeclarationOfIntentSupportingDocuments(this IEnumerable<SupportingDocument> supportingDocuments)
	{
		Argument.NotNull(supportingDocuments, nameof(supportingDocuments));
		return supportingDocuments.Where(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent);
	}
}
