using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IDV1Certificate
	{
		IEnumerable<EntryHeaderDataObject> Entries { get; }
	}
}
