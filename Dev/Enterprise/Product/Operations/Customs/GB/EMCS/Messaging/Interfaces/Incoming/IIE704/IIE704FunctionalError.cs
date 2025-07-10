using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE704FunctionalError
	{
		ZString ErrorLocation { get; }
		ZString ErrorType { get; }
		ZString ErrorReason { get; }
		ZString OriginalAttributeValue { get; }
	}
}
