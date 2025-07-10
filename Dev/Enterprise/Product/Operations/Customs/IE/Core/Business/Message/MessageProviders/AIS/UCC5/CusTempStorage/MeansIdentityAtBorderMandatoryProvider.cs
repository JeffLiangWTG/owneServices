using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class MeansIdentityAtBorderMandatoryProvider : IMeansIdentityAtBorderMandatory
	{
		public static MeansIdentityAtBorderMandatoryProvider New(TemporaryStorageHeader header)
			=> header == null ? null : new MeansIdentityAtBorderMandatoryProvider(header.TransportType, header.ArrivalTransportMeans.TPM_IdentificationNumber);

		public string Type { get; }

		public string Number { get; }

		MeansIdentityAtBorderMandatoryProvider(string type, string number)
		{
			Type = type;
			Number = number;
		}
	}
}
