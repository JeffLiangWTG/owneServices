using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging
{
	public interface IESMessageBusinessObject
	{
		ZString EntryReference { get; }
	}
}
