using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public interface IVerticalPlacementClient
	{
		ZGrid[] Grids { get; }

		[DpiState(DpiState.ScaleY)]
		int GridMaxHeight { get; }

		void SetGridHeight(int height);
		void SetAdditionalControlsPosition(int top);
	}
}