using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface ISCRContainer
	{
		ZString ContainerNumber { get; }
		ZString CountryOfRegistration { get; }
		ZString ContainerSizeCode { get; }
		ZBool IsEmpty { get; }
	}
}
