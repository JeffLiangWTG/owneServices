namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class DefaultDiagramUpdateEntitiesStrategy : JobNetworkUpdateEntitiesStrategy
	{
		internal DefaultDiagramUpdateEntitiesStrategy(JobNetwork jobNetwork)
			: base(jobNetwork)
		{
		}

		internal override void OnFullRefresh()
		{
			var diagramShape = (BMNCNShapeDefaultDiagram)jobNetwork.DiagramEntity.Shape;
			diagramShape.UpdateDefaultNetwork();
		}

		internal override bool IsRelatedEntityPresentOnMultipleDiagrams(BMNCNShape shape)
		{
			return false;
		}

		internal override bool IsPresentOnMultipleDiagrams(BMNCNAttachment attachment)
		{
			return false;
		}
	}
}
