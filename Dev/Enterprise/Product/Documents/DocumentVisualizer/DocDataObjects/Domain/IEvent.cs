using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IEvent
	{
		ZString Code { get; }
		ZString Description { get; }
	}
}