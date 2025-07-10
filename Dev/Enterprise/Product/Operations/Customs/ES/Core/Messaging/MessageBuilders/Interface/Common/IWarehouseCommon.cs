using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IWarehouseCommon
	{
		ZString Type { get; }
		ZString Identifier { get; }
	}
}
