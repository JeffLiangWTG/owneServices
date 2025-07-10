using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class ActiveTransportMeansProvider : ITransportMeans
	{
		public ActiveTransportMeansProvider(JobDeclaration declaration)
		{
			TypeOfIdentification = declaration.ZG_BorderTransportMeans;
			IdentificationNumber = declaration.ActiveTransportMeansID;
			Nationality = declaration.JE_RN_NKTransportNationality;
		}

		public string TypeOfIdentification { get; }
		public string IdentificationNumber { get; }
		public string Nationality { get; }
	}
}
