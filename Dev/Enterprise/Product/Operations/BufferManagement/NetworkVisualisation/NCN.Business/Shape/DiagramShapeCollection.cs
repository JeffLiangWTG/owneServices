using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[ModuleID(ModuleId.NetworkDiagram)]
	public class DiagramShapeCollection : BMNCNShapeCollection
	{
		public DiagramShapeCollection(BusinessObjectFactory factory)
			: base(factory, GetCollectionQuery())
		{
		}

		static ZQuery GetCollectionQuery()
		{
			var query = new ZQuery(BMNCNShapeSchema.BNS_ShapeType, new[]
			{
				ShapeTypeList.Codes.DefaultDiagram, ShapeTypeList.Codes.Diagram, ShapeTypeList.Codes.Shape, ShapeTypeList.Codes.Buffer
			});

			query.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);

			return query;
		}
	}
}
