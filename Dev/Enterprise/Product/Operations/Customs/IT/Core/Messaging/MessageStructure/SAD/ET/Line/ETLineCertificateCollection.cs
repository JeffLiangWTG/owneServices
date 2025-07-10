using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineCertificateCollection
{
	readonly IEnumerable<ICertificate> iCertificates;

	public ETLineCertificateCollection(IEnumerable<ICertificate> iCertificates)
	{
		this.iCertificates = Argument.NotNull(iCertificates, "iCertificates");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt NumberOfOccurences => iCertificates.Count();

	[MessageLayout(Order = 1)]
	public IEnumerable<ETLineCertificate> Certificates
	{
		get
		{
			foreach (var certificate in iCertificates)
			{
				yield return new ETLineCertificate(certificate);
			}
		}
	}
}
