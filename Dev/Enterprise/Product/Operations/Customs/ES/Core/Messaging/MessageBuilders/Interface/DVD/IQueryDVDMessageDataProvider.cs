using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IQueryDVDMessageDataProvider : IDVDCommonDataProvider
	{
		ZBool RequestATCData { get; }
	}
}
