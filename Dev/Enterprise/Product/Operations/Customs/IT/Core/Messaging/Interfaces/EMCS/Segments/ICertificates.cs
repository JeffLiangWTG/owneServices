using System.Collections.Generic;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface ICertificates
{
	IEnumerable<IDocumentCertificate> DocumentCertificates { get; }
}
