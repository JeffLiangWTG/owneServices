using System.Diagnostics;
using System.Linq;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Integration.Network;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public static class NetworkDiagramFormFactory
	{
		public static IZForm ShowNetworkFormForDiagramContainingShape(BMNCNShape shape)
		{
			var diagramShape = shape.FindTopmostDiagramShape();
			var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagramShape);

			var networkDiagramUserControl = (NetworkDiagramUserControl)form?.Controls.Find("NetworkDiagramControl", true).FirstOrDefault();
			var elementHost = (ZElementHost)form?.Controls.Find("WPFElementHost", true).FirstOrDefault();
#pragma warning disable CS0618
			var networkUserControl = (NetworkUserControl)elementHost?.Child;
#pragma warning restore CS0618

			if (networkUserControl != null && networkDiagramUserControl != null &&
				shape.PK != diagramShape.PK)
			{
				var shapeToSelect = networkDiagramUserControl.Network.Entities.SingleOrDefault(entity => entity.AsShape().PK == shape.PK);
				if (shapeToSelect != null)
				{
#if WINZOR
					networkUserControl.MainDiagramControl.SelectAndScrollToEntity(shapeToSelect, animated: true);
#else
					void SelectEntity(object sender, System.EventArgs e)
					{
						networkUserControl.MainDiagramControl.SelectEntity(shapeToSelect, animated: true);
						form.Shown -= SelectEntity;
					}

					form.Shown += SelectEntity;
#endif
				}
			}

			return form;
		}
	}

	class NetworkDiagramFormFactoryProvider : INetworkDiagramFormFactory
	{
		[DebuggerStepThrough]
		void INetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(IBMNCNShape shape)
		{
			NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape((BMNCNShape)shape);
		}
	}
}
