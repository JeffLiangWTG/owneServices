using System.Drawing;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class NetworkScaleBackground : INetworkScaleBackground
	{
		public NetworkScaleBackground(Color color, int startColumn, int endColumn)
		{
			this.color = color;
			this.startColumn = startColumn;
			this.endColumn = endColumn;
		}
		readonly Color color;
		readonly int startColumn;
		readonly int endColumn;

		#region INetworkScaleBackground

		public Color Color => color;
		public int StartColumn => startColumn;
		public int EndColumn => endColumn;

		#endregion
	}
}
