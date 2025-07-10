using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class ArrivalTransportMeansProvider : IIdType
	{
		public static ArrivalTransportMeansProvider New(JobDeclaration declaration) => new ArrivalTransportMeansProvider(declaration);

		ArrivalTransportMeansProvider(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public string Type => declaration.JE_TransportMeans;

		public string Id => declaration.JE_TransportIDInland;
	}
}
