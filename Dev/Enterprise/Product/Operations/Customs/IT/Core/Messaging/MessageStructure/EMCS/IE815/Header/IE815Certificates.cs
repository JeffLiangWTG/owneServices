using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815Certificates
{
	public IE815Certificates(ICertificates certificates)
	{
		this.certificates = Argument.NotNull(certificates, "certificates");
	}
	readonly ICertificates certificates;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R", "R036")]
	public ZInt TotalDocumentCertificatesIterations => certificates.DocumentCertificates.Count();

	[MessageLayout(Order = 1)]
	public IEnumerable<IE815DocumentCertificate> DocumentCertificates
	{
		get
		{
			int i = 1;
			foreach (var documentCertificate in certificates.DocumentCertificates)
			{
				yield return new IE815DocumentCertificate(documentCertificate, i++);
			}
		}
	}
}
