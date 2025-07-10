using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.AES;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class AlternativeEvidenceProvider : IAlternativeEvidence
	{
		public AlternativeEvidenceProvider(AlternativeEvidence alternativeEvidence)
		{
			this.alternativeEvidence = alternativeEvidence;
		}
		readonly AlternativeEvidence alternativeEvidence;

		public string Type => alternativeEvidence.CY_Code;

		public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = alternativeEvidence.AdditionalInfos.Cast<AdditionalInfo>().Select(x => new AdditionalReferenceProvider(x)).ToArray());
		IReadOnlyCollection<IDocument> transportDocument;
	}
}
