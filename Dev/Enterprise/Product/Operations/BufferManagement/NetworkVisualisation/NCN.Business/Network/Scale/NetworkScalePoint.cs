using CargoWise.NetworkVisualisation.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[Immutable]
	public class NetworkScalePoint : INetworkScalePoint
	{
		internal NetworkScalePoint(int column, string label, string toolTip)
		{
			this.column = column;
			this.label = label;
			this.toolTip = toolTip;
		}

		readonly int column;
		readonly string label;
		readonly string toolTip;

		#region INetworkScalePoint Implementation

		public int Column => column;
		public string Label => label;
		public string ToolTip => toolTip;

		#endregion
	}
}
