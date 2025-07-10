using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class CarrierWrapper : ICarrier
	{
		CarrierWrapper(OrgAddress carrier)
		{
			this.carrier = Argument.NotNull(carrier, nameof(carrier));
		}
		readonly OrgAddress carrier;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = carrier.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (name = carrier.Header?.OH_FullName ?? string.Empty);
		string name;

		public static CarrierWrapper New(OrgAddress carrier) => carrier != null ? new CarrierWrapper(carrier) : null;
	}
}
