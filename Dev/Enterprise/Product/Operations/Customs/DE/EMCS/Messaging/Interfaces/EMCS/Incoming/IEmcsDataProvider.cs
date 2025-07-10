using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IEmcsDataProvider : IDataProvider
	{
		ZString MessageGroup { get; }
	}
}
