using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IEMCSTransportDetails
	{
		ZString UnitCode { get; }
		ZString IdentityOfUnit { get; }
		ZString CommercialSealIdentification { get; }
		ZString ComplementaryInformation { get; }
		ZString SealInformation { get; }
	}
}
