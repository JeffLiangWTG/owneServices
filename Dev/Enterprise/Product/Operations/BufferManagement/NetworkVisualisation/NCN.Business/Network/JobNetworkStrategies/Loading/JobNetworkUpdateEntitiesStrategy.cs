using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class JobNetworkUpdateEntitiesStrategy
	{
		internal JobNetworkUpdateEntitiesStrategy(JobNetwork jobNetwork)
		{
			this.jobNetwork = jobNetwork;
		}

		protected readonly JobNetwork jobNetwork;

		internal virtual void OnFullRefresh()
		{
		}

		internal virtual void OnEntityEdited()
		{
		}

		internal virtual bool IsRelatedEntityPresentOnMultipleDiagrams(BMNCNShape shape)
		{
			var query = new ZDBOnlyQuery(typeof(BMNCNShape));
			query.AddToFilter(BMNCNShapeSchema.BNS_RelatedEntityID, shape.BNS_RelatedEntityID);
			query.AddToFilter(BMNCNShapeSchema.PK, SQLComparisonOperator.NotEqual, shape.PK);

			var rootShapeSubQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.PK);
			rootShapeSubQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.Diagram); // Excludes default diagrams

			query.AddSubQuery(BMNCNShapeSchema.BNS_BNS_RootShape, rootShapeSubQuery, JoinCondition.And);

			return shape.Factory.Exists(typeof(BMNCNShape), query, mergeDbAndCacheResult: false);
		}

		internal virtual bool IsPresentOnMultipleDiagrams(BMNCNAttachment attachment)
		{
			var query = new ZDBOnlyQuery(typeof(BMNCNAttachment));
			query.AddToFilter(BMNCNAttachmentSchema.BNA_FP_ProcessHeaderLink, attachment.BNA_FP_ProcessHeaderLink);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_Owner, SQLComparisonOperator.NotEqual, attachment.BNA_BNS_Owner);
			query.AddToFilter(BMNCNAttachmentSchema.PK, SQLComparisonOperator.NotEqual, attachment.PK);

			var parentShapeSubQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNAttachmentSchema.BNA_BNS_Owner);
			parentShapeSubQuery.AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.Diagram);

			query.AddSubQuery(parentShapeSubQuery, JoinCondition.And);

			return attachment.Factory.Exists(typeof(BMNCNAttachment), query, mergeDbAndCacheResult: false);
		}
	}
}
