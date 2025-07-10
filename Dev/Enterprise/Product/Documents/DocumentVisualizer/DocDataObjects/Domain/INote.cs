using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface INote
	{
		ZString Text { get; }
		ZString Description { get; }
	}
}
