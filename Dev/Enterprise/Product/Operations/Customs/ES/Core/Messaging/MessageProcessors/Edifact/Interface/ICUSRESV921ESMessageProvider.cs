using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface ICUSRESV921ESMessageProvider : ICUSRESMessageProvider
	{
		ZDateTime TransitMaxDate { get; }
		ZString PrintActionRequired { get; }
		ZString RegistrationNumber { get; }
		ZString CSVReleaseCode { get; }
		ZDateTime CSVReleaseCreationDate { get; }
	}
}
