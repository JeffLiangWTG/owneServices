using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTST : IUnderCustomsControl
	{
		ZString RecipientReferenceNumber { get; }
	}
}
