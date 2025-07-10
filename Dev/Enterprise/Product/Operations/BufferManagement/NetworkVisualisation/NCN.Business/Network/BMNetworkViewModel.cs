using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNetworkViewModel : NonPersistentBusinessObject
	{
		public BMNetworkViewModel(IBMNCNShape diagramShape)
			: base(diagramShape.Factory)
		{
			this.diagramShape = diagramShape.AsShape();
			this.diagramShape.Factory.AddFetchHint(BMNCNAttachmentSchema.BNA_BNS_Owner, diagramShape.Identifier);
		}

		readonly BMNCNShape diagramShape;

		#region Related Business Objects

		public BMNCNShape DiagramShape
		{
			get { return diagramShape; }
		}

		#endregion
	}
}
