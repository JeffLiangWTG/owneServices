using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDUAImportQueryMessageDataProvider : IImportCommonDataProvider
	{
		ZString RequestATCData { get; }
	}
}
