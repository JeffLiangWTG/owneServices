using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public interface INotificationForwardingPartySequenceNumberHeader
	{
		BusinessObjectFactory Factory { get; }
		ShortSequenceNumberGenerator SequenceNumberGenerator { get; }
	}
}
