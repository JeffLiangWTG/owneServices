using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Schema;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNShapeCollection : ActiveBusinessObjectCollection<BMNCNShape>
	{
		public static BMNCNShapeCollection ForChildShapes(BMNCNShape shape)
		{
			return new BMNCNShapeCollection(shape, BMNCNShapeSchema.BNS_BNS_ParentShape, makeNewShapesChildrenOfParent: true);
		}

		public static BMNCNShapeCollection ForDescendantShapes(BMNCNShape shape)
		{
			if (shape.HasParent)
			{
				return new BMNCNShapeCollection(shape.Factory, new DescendantOverrideRelationship(shape));
			}
			else
			{
				return new BMNCNShapeCollection(shape, BMNCNShapeSchema.BNS_BNS_RootShape, makeNewShapesChildrenOfParent: false);
			}
		}

		public BMNCNShapeCollection(BusinessObjectFactory factory)
			: this(factory, (ZQuery)null)
		{
		}

		public BMNCNShapeCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, GetDefaultShapeQuery(query))
		{
		}

		public BMNCNShapeCollection(BusinessObjectFactory factory, CollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		BMNCNShapeCollection(BMNCNShape shape, SchemaGuidColumn fkColumn, bool makeNewShapesChildrenOfParent)
			: base(shape.Factory, shape, GetDefaultShapeQuery(), fkColumn)
		{
			if (makeNewShapesChildrenOfParent)
			{
				parentShape = shape;
			}
		}

		readonly BMNCNShape parentShape;

		#region Implementation

		static ZQuery GetDefaultShapeQuery(ZQuery query = null)
		{
			var defaultQuery = new ZQuery();
			defaultQuery.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);

			if (query != null)
			{
				defaultQuery.AddToFilter(query);
			}

			return defaultQuery;
		}

		protected override void SetDefaultsForNewElementCore(BMNCNShape newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (parentShape != null)
			{
				newElement.MakeChildOf(parentShape);
			}
		}

		class DescendantOverrideRelationship : DependentRelationship
		{
			internal DescendantOverrideRelationship(BMNCNShape diagramSurfaceShape)
				: base(diagramSurfaceShape.RootShape, typeof(BMNCNShape), new ZQuery(), BMNCNShapeSchema.BNS_BNS_RootShape)
			{
				if (diagramSurfaceShape.BNS_BNS_RootShape.IsEmpty)
				{
					throw new InvalidOperationException("Can't have a parent shape specified without having a root shape specified");
				}

				this.diagramSurfaceShape = diagramSurfaceShape;
			}

			readonly BMNCNShape diagramSurfaceShape;

			HashSet<BMNCNShape> DiagramSurfaceDescendants
			{
				get
				{
					var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(DescendantOverrideRelationship), diagramSurfaceShape.PK);
					var policy = CacheStalenessPolicy.StaleWhenDataTableChanges(BMNCNShapeSchema.Constants.TableName, diagramSurfaceShape.Factory);

					return diagramSurfaceShape.Factory.GetCachedValue(cacheKey, () => diagramSurfaceShape.Descendants(new BMNCNShapeDescendantsStrategy()).ToHashSet(), policy);
				}
			}

			protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
			{
				return base.MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache)
					&& IsWithinDiagramSurfaceHierarchy((BMNCNShape)businessObject);
			}

			bool IsWithinDiagramSurfaceHierarchy(BMNCNShape shape)
			{
				return DiagramSurfaceDescendants.Contains(shape);
			}

			protected override bool IsAdditionalContextEqual(DependentRelationship other)
			{
				return base.IsAdditionalContextEqual(other) && diagramSurfaceShape.PK == ((DescendantOverrideRelationship)other).diagramSurfaceShape.PK;
			}
		}

		#endregion
	}
}
