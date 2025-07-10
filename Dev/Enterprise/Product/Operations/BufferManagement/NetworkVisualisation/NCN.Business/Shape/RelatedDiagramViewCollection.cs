using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class RelatedDiagramViewCollection : NonPersistentBusinessObjectCollection<RelatedDiagramView>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public RelatedDiagramViewCollection(BMNCNShape diagram)
			: base(diagram.Factory)
		{
			mainDiagram = diagram;
			PopulateCollection();
		}

		readonly BMNCNShape mainDiagram;

		void PopulateCollection()
		{
			var relatedDiagrams = GetRelatedDiagrams();

			foreach (var relatedDiagram in relatedDiagrams)
			{
				Add(new RelatedDiagramView(mainDiagram, relatedDiagram));
			}
		}

		IEnumerable<BMNCNShape> GetRelatedDiagrams()
		{
			var parentShape = mainDiagram.ParentShape;

			if (parentShape != null)
			{
				yield return parentShape;
			}

			var scaledQuery = new ZDBOnlyQuery(typeof(BMNCNShape));

			var toSubQuery = new ZDBOnlySubQuery(typeof(BMNCNAttachment), BMNCNAttachmentSchema.BNA_BNS_ToShape);
			toSubQuery.AddToFilter(BMNCNAttachmentSchema.BNA_Type, AttachmentTypeList.Codes.SwitchToScaled);
			toSubQuery.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_FromShape, mainDiagram.PK);

			var fromSubQuery = new ZDBOnlySubQuery(typeof(BMNCNAttachment), BMNCNAttachmentSchema.BNA_BNS_FromShape);
			fromSubQuery.AddToFilter(BMNCNAttachmentSchema.BNA_Type, AttachmentTypeList.Codes.SwitchToScaled);
			fromSubQuery.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_ToShape, mainDiagram.PK);

			toSubQuery.AddAsUnionQuery(fromSubQuery);
			scaledQuery.AddSubQuery(toSubQuery, JoinCondition.And);

			var linkedQuery = new ZDBOnlyQuery(typeof(BMNCNShape));
			linkedQuery.AddToFilter(BMNCNShapeSchema.BNS_RelatedEntityID, mainDiagram.PK);
			scaledQuery.AddToFilter(linkedQuery, JoinCondition.Or);

			foreach (var shape in Factory.Load<BMNCNShape>(scaledQuery))
			{
				yield return shape;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RelatedDiagramView(mainDiagram, Factory.New<BMNCNShape>());
		}
	}
}
