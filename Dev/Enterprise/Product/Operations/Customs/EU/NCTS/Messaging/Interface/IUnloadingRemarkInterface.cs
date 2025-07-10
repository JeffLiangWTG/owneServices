using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IUnloadingRemarkInterface
	{
		ZString StateOfSealsOk { get; }
		ZString Conform { get; }
		ZString UnloadingCompletion { get; }
		ZString UnloadingDate { get; }
		ZInt NoOfSeals { get; }
		ZInt RawNoOfSeals { get; }
	}
}
