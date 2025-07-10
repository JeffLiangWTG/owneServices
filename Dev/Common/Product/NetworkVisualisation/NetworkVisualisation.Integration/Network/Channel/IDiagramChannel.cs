using System.Drawing;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IDiagramChannel
	{
		string Name { get; }
		int Height { get; }
		Color Color { get; }
	}
}
