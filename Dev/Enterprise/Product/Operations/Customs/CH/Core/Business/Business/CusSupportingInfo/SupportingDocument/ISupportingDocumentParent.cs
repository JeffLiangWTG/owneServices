using System.Collections.Generic;

namespace Enterprise.Customs.CH.Business;

public interface ISupportingDocumentParent
{
	bool IsGSPCertificateRequired { get; }

	IEnumerable<SupportingDocument> SupportingDocumentsIncludingInherited { get; }
}
