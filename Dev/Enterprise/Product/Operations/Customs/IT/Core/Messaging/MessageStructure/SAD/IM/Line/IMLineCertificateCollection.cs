using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineCertificateCollection
{
	public IMLineCertificateCollection(IEnumerable<ICertificate> certificates)
	{
		this.certificates = Argument.NotNull(certificates, nameof(certificates));
	}

	readonly IEnumerable<ICertificate> certificates;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZInt NumberOfOccurences => certificates.Count();

	[MessageLayout(Order = 1)]
	public IEnumerable<IMLineCertificate> Certificates => certificates.Select(certificate => new IMLineCertificate(certificate));
}
