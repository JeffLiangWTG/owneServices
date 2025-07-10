using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class DiagramShapeTypeList : ShapeTypeList
	{
		public DiagramShapeTypeList()
		{
			RemoveCode(Codes.Annotation);
			RemoveCode(Codes.DefaultWorkflow);
		}
	}
}
