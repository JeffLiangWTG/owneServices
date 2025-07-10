using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobRelatedDiagramsViewModel : NonPersistentBusinessObject
	{
		public JobRelatedDiagramsViewModel(ProcessJobHeader jobHeader)
			: base(jobHeader.Factory)
		{
			this.jobHeader = jobHeader;
			SubscribeToProcessHeaders();
		}

		readonly ProcessJobHeader jobHeader;

		public BMNCNShapeCollection RelatedDiagrams => relatedDiagrams ?? (relatedDiagrams = new BMNCNShapeCollection(jobHeader.Factory));
		BMNCNShapeCollection relatedDiagrams;

		#region Implementation

		void SubscribeToProcessHeaders()
		{
			allProcessHeaders = ProcessHeaderCollection.GetCollectionForJobIncludingJobLevelWorkflow(jobHeader.Parent, jobHeader);
			((IBindingList)allProcessHeaders).ListChanged += JobRelatedDiagramsViewModel_ListChanged;
			JobRelatedDiagramsViewModel_ListChanged(null, null);
		}

		void JobRelatedDiagramsViewModel_ListChanged(object sender, ListChangedEventArgs e)
		{
			var shapesQuery = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, allProcessHeaders.Select(h => h.PK));
			shapesQuery.AddToFilter(new ZQuery(BMNCNShapeSchema.BNS_ShapeType, SQLComparisonOperator.NotEqual, ShapeTypeList.Codes.DefaultDiagram));
			shapesQuery.AddToFilter(new ZQuery(BMNCNShapeSchema.BNS_ShapeType, SQLComparisonOperator.NotEqual, ShapeTypeList.Codes.DefaultWorkflow));
			RelatedDiagrams.AdditionalFilter = shapesQuery;
		}

		ProcessHeaderCollection allProcessHeaders;

		#endregion
	}
}
