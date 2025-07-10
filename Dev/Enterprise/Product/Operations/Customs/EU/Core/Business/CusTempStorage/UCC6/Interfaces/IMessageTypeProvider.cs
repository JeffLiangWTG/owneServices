using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface IMessageTypeProvider
	{
		ZString MessageType { get; }
	}
}
