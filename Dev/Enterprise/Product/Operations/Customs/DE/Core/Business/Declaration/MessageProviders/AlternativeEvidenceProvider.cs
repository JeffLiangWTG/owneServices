using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using static Enterprise.Customs.DE.Business.AlternativeEvidenceTypeList.Codes;

namespace Enterprise.Customs.DE.Business
{
	public class AlternativeEvidenceProvider : IAlternativeEvidence
	{
		public AlternativeEvidenceProvider(IEnumerable<AlternativeEvidence> alternativeEvidences)
		{
			Argument.NotNull(alternativeEvidences, nameof(alternativeEvidences));
			Argument.GreaterThanZero(alternativeEvidences.Count(), nameof(alternativeEvidences));
			this.alternativeEvidences = alternativeEvidences;
		}
		readonly IEnumerable<AlternativeEvidence> alternativeEvidences;

		public string Type => alternativeEvidences.First().EvidenceType;

		public IReadOnlyCollection<IReference> TransportDocuments => transportDocument ?? (transportDocument = alternativeEvidences.Select(y => new ReferenceProvider(y)).ToArray());
		IReadOnlyCollection<IReference> transportDocument;

		public bool TransportDocumentsSpecified => Type == _11 || Type == _14 || Type == _15 || Type == _17;
	}
}
