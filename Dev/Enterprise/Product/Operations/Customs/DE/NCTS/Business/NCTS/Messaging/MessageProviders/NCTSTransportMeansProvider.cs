using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSTransportMeansProvider : INCTSTransportMeans
	{
		public NCTSTransportMeansProvider(string typeOfIdentification, string identification, string nationality)
		{
			TypeOfIdentification = typeOfIdentification;
			IdentificationNumber = identification;
			Nationality = nationality;
		}

		public string TypeOfIdentification { get; }

		public string IdentificationNumber { get; }

		public string Nationality { get; }
	}
}
