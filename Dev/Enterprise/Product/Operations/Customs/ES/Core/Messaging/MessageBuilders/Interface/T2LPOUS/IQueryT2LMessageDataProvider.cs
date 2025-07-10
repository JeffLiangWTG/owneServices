using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IQueryT2LMessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString MRN { get; }
	}
}
