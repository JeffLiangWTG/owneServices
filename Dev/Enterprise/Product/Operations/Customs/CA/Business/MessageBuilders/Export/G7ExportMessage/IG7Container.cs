using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface IG7Container
	{
		ZString ContainerNumber { get; }
		ZString CountryOfRegistration { get; }
		ZString ContainerSizeCode { get; }
	}
}
