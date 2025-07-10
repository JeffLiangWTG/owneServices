using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINCustomsDocument
	{
		ZString Reference { get; }
		ZString Type { get; }
	}
}
