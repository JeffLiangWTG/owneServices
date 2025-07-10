using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class WorkflowRelatedDiagramsViewModel : NonPersistentBusinessObject
	{
		public WorkflowRelatedDiagramsViewModel(ProcessHeader selectedWorkflow)
		{
			this.selectedWorkflow = selectedWorkflow;
		}

		readonly ProcessHeader selectedWorkflow;

		[ChildEditable]
		public BMNCNShapeCollection RelatedDiagrams
		{
			get
			{
				if (relatedDiagrams == null)
				{
					var factory = selectedWorkflow.Factory;
					var shapesQuery = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, selectedWorkflow.PK);

					shapesQuery.AddToFilter(new ZQuery(BMNCNShapeSchema.BNS_ShapeType, SQLComparisonOperator.NotEqual, ShapeTypeList.Codes.DefaultDiagram));
					shapesQuery.AddToFilter(new ZQuery(BMNCNShapeSchema.BNS_ShapeType, SQLComparisonOperator.NotEqual, ShapeTypeList.Codes.DefaultWorkflow));

					relatedDiagrams = new BMNCNShapeCollection(factory, shapesQuery);
					relatedDiagrams.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(relatedDiagrams);
				}
				return relatedDiagrams;
			}
		}
		BMNCNShapeCollection relatedDiagrams;
	}
}
