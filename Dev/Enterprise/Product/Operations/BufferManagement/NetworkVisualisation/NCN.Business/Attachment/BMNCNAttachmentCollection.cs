using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNAttachmentCollection : ActiveBusinessObjectCollection<BMNCNAttachment>
	{
		public BMNCNAttachmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BMNCNAttachmentCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public BMNCNAttachmentCollection(BMNCNShape shape, SchemaColumn attachmentShapeColumn)
			: base(shape.Factory, shape, new ZQuery(), attachmentShapeColumn)
		{
		}

		public BMNCNAttachmentCollection(BMNCNShape shape, params string[] attachmentTypes)
			: base(shape.Factory, GetQuery(shape, attachmentTypes))
		{
		}

		static ZQuery GetQuery(BMNCNShape shape, string[] attachmentTypes)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(BMNCNAttachmentSchema.BNA_BNS_FromShape, shape.PK), JoinCondition.Or);
			query.AddToFilter(new ZQuery(BMNCNAttachmentSchema.BNA_BNS_ToShape, shape.PK), JoinCondition.Or);
			query.AddToFilter(new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, shape.PK), JoinCondition.Or);

			if (attachmentTypes.Length > 0)
			{
				query.AddToFilter(BMNCNAttachmentSchema.BNA_Type, attachmentTypes);
			}

			query.FetchOnlyFromLocalCache = !shape.IsInDatabase;
			query.IncludeBlob(BMNCNAttachmentSchema.BNA_LayoutData);

			return query;
		}
	}
}
