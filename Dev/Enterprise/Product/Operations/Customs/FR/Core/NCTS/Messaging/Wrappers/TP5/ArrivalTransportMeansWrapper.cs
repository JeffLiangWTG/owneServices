using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class ArrivalTransportMeansWrapper : IDepartureTransportMeans
	{
		protected ArrivalTransportMeansWrapper(ArrivalCusTransportMeans arrivalCusTransportMeans)
		{
			this.arrivalCusTransportMeans = Argument.NotNull(arrivalCusTransportMeans, nameof(arrivalCusTransportMeans));
		}

		protected readonly ArrivalCusTransportMeans arrivalCusTransportMeans;

		public static ArrivalTransportMeansWrapper New(ArrivalCusTransportMeans arrivalCusTransportMeans) => arrivalCusTransportMeans == null ? null : new ArrivalTransportMeansWrapper(arrivalCusTransportMeans);

		public virtual string TypeOfIdentification => typeOfIdentification ?? (typeOfIdentification = arrivalCusTransportMeans.TPM_TypeOfIdentification);
		string typeOfIdentification;

		public virtual string IdentificationNumber => identificationNumber ?? (identificationNumber = arrivalCusTransportMeans.TPM_IdentificationNumber);
		string identificationNumber;

		public virtual string Nationality => nationality ?? (nationality = arrivalCusTransportMeans.TPM_RN_NKTransportNationality);
		string nationality;
	}
}
