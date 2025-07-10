using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;

public class CC044CArrivalTransportMeansWrapper : ArrivalTransportMeansWrapper
{
	protected readonly CusTransportMeans transportMeans;
	public CC044CArrivalTransportMeansWrapper(ArrivalCusTransportMeans transportMeans) : base(transportMeans)
	{
		this.transportMeans = Argument.NotNull(transportMeans, nameof(transportMeans));
	}
	public new static CC044CArrivalTransportMeansWrapper New(ArrivalCusTransportMeans arrivalCusTransportMeans) => arrivalCusTransportMeans == null ? null : new CC044CArrivalTransportMeansWrapper(arrivalCusTransportMeans);

	bool StatusIsNew => transportMeans.TPM_TransportState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;

	public override string TypeOfIdentification => typeOfIdentification ?? (typeOfIdentification = StatusIsNew ? arrivalCusTransportMeans.TPM_TypeOfIdentification : null);
	string typeOfIdentification;

	public override string IdentificationNumber => identificationNumber ?? (identificationNumber = StatusIsNew ? arrivalCusTransportMeans.TPM_IdentificationNumber : null);
	string identificationNumber;

	public override string Nationality => nationality ?? (nationality = StatusIsNew ? arrivalCusTransportMeans.TPM_RN_NKTransportNationality : null);
	string nationality;
}
