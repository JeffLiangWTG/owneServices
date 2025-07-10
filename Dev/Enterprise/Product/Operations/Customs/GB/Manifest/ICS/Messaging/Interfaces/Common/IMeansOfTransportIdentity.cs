using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface IMeansOfTransportIdentity
	{
		ZString Nationality { get; }
		ZString Identity { get; }
		ZString IdentityLNG { get; }
	}
}
