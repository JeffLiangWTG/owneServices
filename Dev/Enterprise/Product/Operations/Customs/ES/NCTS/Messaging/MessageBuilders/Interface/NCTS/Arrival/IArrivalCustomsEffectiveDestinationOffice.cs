using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalCustomsEffectiveDestinationOffice
	{
		ZString CustomsDestinationOfficeCode { get; }
		ZString CustomsDestinationLocationCode { get; }
	}
}
