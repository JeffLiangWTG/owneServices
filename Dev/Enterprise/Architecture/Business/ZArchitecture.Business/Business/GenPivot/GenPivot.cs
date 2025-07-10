using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class GenPivot : AutoGenPivot, IGenPivot, IPivotBusinessObject
	{
		public GenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new GenPivotTypeDecider();

		#region Properties

		public ZGuid Relation1ID
		{
			get => XX_Relation1ID;
			set => XX_Relation1ID = value;
		}

		/// <summary>
		/// Load the Relation1 object. Relation1TableCode must be mapped to an object type
		/// in BusinessObjectPrefixTypesConfiguration.xml under EnterpriseBusinessObjectPrefixTypes.
		/// </summary>
		public BusinessObject Relation1Object
		{
			get
			{
				if (relation1Object == null || relation1Object.IsDeleted || relation1Object.PK != XX_Relation1ID || relation1Object.TablePrefix != XX_Relation1TableCode)
				{
					relation1Object = Factory.Load(XX_Relation1TableCode, XX_Relation1ID);
				}
				return relation1Object;
			}
			set
			{
				relation1Object = value;
				if (relation1Object != null)
				{
					XX_Relation1TableCode = relation1Object.TablePrefix;
					XX_Relation1ID = relation1Object.PK;
				}
			}
		}
		BusinessObject relation1Object;

		public ZGuid Relation2ID
		{
			get => XX_Relation2ID;
			set => XX_Relation2ID = value;
		}

		public BusinessObject Relation2Object
		{
			get
			{
				if (relation2Object == null || relation2Object.IsDeleted || relation2Object.PK != XX_Relation2ID || relation2Object.TablePrefix != XX_Relation2TableCode)
				{
					relation2Object = Factory.Load(XX_Relation2TableCode, XX_Relation2ID);
				}
				return relation2Object;
			}
			set
			{
				relation2Object = value;
				if (relation2Object != null)
				{
					XX_Relation2TableCode = relation2Object.TablePrefix;
					XX_Relation2ID = relation2Object.PK;
				}
			}
		}
		BusinessObject relation2Object;

		#endregion

		#region Load

		/// <summary>
		/// Load the top GenPivot for the given Relation1Object and relationType
		/// </summary>
		public static GenPivot LoadRelation1Pivot(BusinessObject relation1Object, string relationType)
		{
			return LoadRelation1Pivot(relation1Object.Factory, relation1Object.PK, relation1Object.TablePrefix, relationType);
		}

		public static GenPivot LoadRelation1Pivot(BusinessObjectFactory factory, ZGuid relation1ID, string relation1TableCode, string relationType)
		{
			var query = new ZQuery(GenPivotSchema.XX_RelationType, relationType);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, relation1ID);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, relation1TableCode);
			return factory.LoadTop1<GenPivot>(query);
		}

		/// <summary>
		/// Load the top GenPivot for the given Relation2Object and relationType
		/// </summary>
		public static GenPivot LoadRelation2Pivot(BusinessObject relation2Object, string relationType)
		{
			return LoadRelation2Pivot(relation2Object.Factory, relation2Object.PK, relation2Object.TablePrefix, relationType);
		}

		public static GenPivot LoadRelation2Pivot(BusinessObjectFactory factory, ZGuid relation2ID, string relation2TableCode, string relationType)
		{
			var query = new ZQuery(GenPivotSchema.XX_RelationType, relationType);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, relation2ID);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2TableCode);
			return factory.LoadTop1<GenPivot>(query);
		}

		#endregion

		#region Queries

		public static class Query
		{
			/// <summary>
			/// Build a query to return main records given a filter on related records.
			/// Main records are linked directly to related records via a two-way pivot.
			/// </summary>
			public static void AddPivotFilter(ZDBOnlyQuery mainQuery, string mainTableCode, string relationType,
				Type relatedType, string relatedTableCode, ZQuery relatedFilter)
			{
				// Two possible link directions:
				//		Main <- Filter On related record
				//		Main -> Filter On related record

				var filterQuery1 = GetPivotQuery(true, mainTableCode, relationType, relatedType, relatedTableCode, relatedFilter);
				var filterQuery2 = GetPivotQuery(false, mainTableCode, relationType, relatedType, relatedTableCode, relatedFilter);

				mainQuery.AddSubQuery(filterQuery1, JoinCondition.Or);
				mainQuery.AddSubQuery(filterQuery2, JoinCondition.Or);
			}

			/// <summary>
			/// Build a query to return main records given a filter on related records.
			/// Main records are linked indirectly to related records via a connecting table.
			/// So there is a pivot between Main and Connected, and a second pivot between Connected and Related.
			/// Both pivots are two way.
			/// </summary>
			public static void AddPivotFilter(ZDBOnlyQuery mainQuery, string mainTableCode, string relationType,
				Type connectType, string connectTableCode, ZQuery connectFilter,
				Type relatedType, string relatedTableCode, ZQuery relatedFilter)
			{
				// Four possible link directions:
				//		Main <- Connect Record <- Filter On related record
				//		Main <- Connect Record -> Filter On related record
				//		Main -> Connect Record <- Filter On related record
				//		Main -> Connect Record -> Filter On related record

				// Inner most subquery - returns connected records from related records
				var filterQuery1 = GetPivotQuery(true, connectTableCode, relationType, relatedType, relatedTableCode, relatedFilter);
				var filterQuery2 = GetPivotQuery(false, connectTableCode, relationType, relatedType, relatedTableCode, relatedFilter);

				// Outer subquery - returns main records from connected records
				var connectQuery1 = GetPivotQuery(true, mainTableCode, relationType, connectType, connectTableCode, connectFilter, filterQuery1);
				var connectQuery2 = GetPivotQuery(true, mainTableCode, relationType, connectType, connectTableCode, connectFilter, filterQuery2);
				var connectQuery3 = GetPivotQuery(false, mainTableCode, relationType, connectType, connectTableCode, connectFilter, filterQuery1);
				var connectQuery4 = GetPivotQuery(false, mainTableCode, relationType, connectType, connectTableCode, connectFilter, filterQuery2);

				mainQuery.AddSubQuery(connectQuery1, JoinCondition.Or);
				mainQuery.AddSubQuery(connectQuery2, JoinCondition.Or);
				mainQuery.AddSubQuery(connectQuery3, JoinCondition.Or);
				mainQuery.AddSubQuery(connectQuery4, JoinCondition.Or);
			}

			public static ZDBOnlySubQuery GetPivotQuery(bool asParent, string mainTableCode, string relationType, Type relatedType, string relatedTableCode, ZQuery relatedFilter, ZDBOnlySubQuery extraSubQuery = null, bool notIn = false)
			{
				var column = asParent ? GenPivotSchema.XX_Relation2ID : GenPivotSchema.XX_Relation1ID;
				var otherSideColumn = asParent ? GenPivotSchema.XX_Relation1ID : GenPivotSchema.XX_Relation2ID;

				var outerLinkQuery = new ZDBOnlySubQuery(typeof(GenPivot), column, notIn);
				outerLinkQuery.AddToFilter(GenPivotSchema.XX_RelationType, relationType);
				outerLinkQuery.AddToFilter(asParent ? GenPivotSchema.XX_Relation1TableCode : GenPivotSchema.XX_Relation2TableCode, relatedTableCode);
				outerLinkQuery.AddToFilter(asParent ? GenPivotSchema.XX_Relation2TableCode : GenPivotSchema.XX_Relation1TableCode, mainTableCode);
				var innerLinkQuery = new ZDBOnlySubQuery(relatedType, otherSideColumn);

				if (relatedFilter != null)
				{
					var subQuery = relatedFilter as ZDBOnlySubQuery;
					if (subQuery != null)
					{
						innerLinkQuery.AddSubQuery(subQuery, JoinCondition.And);
					}
					else
					{
						innerLinkQuery.AddToFilter(relatedFilter);
					}
				}

				if (extraSubQuery != null)
				{
					innerLinkQuery.AddSubQuery(extraSubQuery, JoinCondition.And);
				}

				outerLinkQuery.AddSubQuery(innerLinkQuery, JoinCondition.And);

				return outerLinkQuery;
			}
		}

		#endregion
	}
}
