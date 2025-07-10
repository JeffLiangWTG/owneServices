using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class ArrivalTransportMeansWrapper : IArrivalTransportMeans
	{
		ArrivalTransportMeansWrapper(CusTransportMeans arrivalTransportMeans)
		{
			this.arrivalTransportMeans = Argument.NotNull(arrivalTransportMeans, nameof(arrivalTransportMeans));
		}
		readonly CusTransportMeans arrivalTransportMeans;

		public string IdentificationNumber => identificationNumber ??= arrivalTransportMeans.TPM_IdentificationNumber;
		string identificationNumber;

		public byte TypeOfIdentification => typeOfIdentification.Equals(ZByte.Zero) ? typeOfIdentification = ZByte.ParseSafe(arrivalTransportMeans.TPM_TypeOfIdentification.ToString(), ZByte.Zero) : typeOfIdentification;
		byte typeOfIdentification;

		public static ArrivalTransportMeansWrapper New(CusTransportMeans arrivalTransportMeans) => arrivalTransportMeans == null ? null : new ArrivalTransportMeansWrapper(arrivalTransportMeans);
	}
}
