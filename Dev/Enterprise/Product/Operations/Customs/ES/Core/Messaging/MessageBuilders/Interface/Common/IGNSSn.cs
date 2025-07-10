using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface ICommonGNSS
	{
		ZString Latitude { get; }
		ZString Longitude { get; }
	}
}
