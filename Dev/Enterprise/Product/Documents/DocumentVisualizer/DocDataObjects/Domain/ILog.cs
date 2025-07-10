using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ILog
	{
		ZDateTime EventTime { get; }
		ZDateTime PostedTime { get; }
		ZString Reference { get; }
		ZString EventDetails { get; }
		IEvent Event { get; }
		IUser User { get; }
	}
}