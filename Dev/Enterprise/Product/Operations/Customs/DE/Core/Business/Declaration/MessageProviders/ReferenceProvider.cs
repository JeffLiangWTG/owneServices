using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	public class ReferenceProvider : IReference
	{
		public ReferenceProvider(AlternativeEvidence alternativeEvidence)
		{
			this.alternativeEvidence = alternativeEvidence;
		}
		readonly AlternativeEvidence alternativeEvidence;

		public string FullType => string.Empty;

		public string Type => alternativeEvidence.DocType.Left(4);

		public string Qualifier => alternativeEvidence.DocType.SubstringSafe(4, 3);

		public string ReferenceNumber => alternativeEvidence.Reference;

		public string Detail => string.Empty;

		public string Complement => string.Empty;

		public string Currency => string.Empty;

		public decimal Amount => 0m;
	}
}
