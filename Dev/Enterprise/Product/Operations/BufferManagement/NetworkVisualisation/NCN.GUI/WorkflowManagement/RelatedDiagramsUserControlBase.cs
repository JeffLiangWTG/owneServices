using System.Windows.Forms;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class RelatedDiagramsUserControlBase : ZUserControl
	{
		protected void DiagramsGrid_MouseDoubleClickBase(object sender, MouseEventArgs e)
		{
			var diagramsGrid = (ZDisplayGrid)sender;
			var currentShape = diagramsGrid.ListManager.GetCurrent() as BMNCNShape;
			if (currentShape != null)
			{
				NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(currentShape);
			}
		}
	}
}
