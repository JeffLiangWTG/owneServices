using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleLocationFilter : ModuleCodeFilter
	{
		#region Construction

		protected ModuleLocationFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleLocationFilter(ZString description, SchemaStringColumn location1FilterColumn, IBusinessObjectCollection location1List, SchemaStringColumn location2FilterColumn, IBusinessObjectCollection location2List, bool allowInternationalZones = false)
			: base(description, location1FilterColumn, location1List, location2FilterColumn, location2List)
		{
			this.allowInternationalZones = allowInternationalZones;
			EnsureListsAreLocationCollections(location1List, location2List);
		}

		public ModuleLocationFilter(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection location1List, IBusinessObjectCollection location2List)
			: base(description, queryDelegate, location1List, location2List)
		{
			EnsureListsAreLocationCollections(location1List, location2List);
		}

		void EnsureListsAreLocationCollections(IBusinessObjectCollection location1List, IBusinessObjectCollection location2List)
		{
			var iLocationCollectionType = ObjectFactory.GetType<ILocationCollection>();
			Argument.NotNull(location1List, "location1List");
			if (!iLocationCollectionType.IsInstanceOfType(location1List))
			{
				throw new ArgumentException("location1List is not a LocationCollection.");
			}

			Argument.NotNull(location2List, "location2List");
			if (!iLocationCollectionType.IsInstanceOfType(location2List))
			{
				throw new ArgumentException("location2List is not a LocationCollection.");
			}
		}

		readonly bool allowInternationalZones;

		#endregion

		#region GetNewCommonModuleFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleLocationFilter(category, parentCollection);
		}

		#endregion

		[MaxLength(5)]
		public override ZString Property1 { get => base.Property1; set => base.Property1 = value; }

		[MaxLength(5)]
		public override ZString Property2 { get => base.Property2; set => base.Property2 = value; }

		public bool IsEmptyProperty1ComparisonOperation
		{
			get { return isEmptyProperty1ComparisonOperation; }
			set
			{
				if (isEmptyProperty1ComparisonOperation != value)
				{
					isEmptyProperty1ComparisonOperation = value;
					InvalidateCachedQuery();
				}
			}
		}

		bool isEmptyProperty1ComparisonOperation;

		public bool IsEmptyProperty2ComparisonOperation
		{
			get { return isEmptyProperty2ComparisonOperation; }
			set
			{
				if (isEmptyProperty2ComparisonOperation != value)
				{
					isEmptyProperty2ComparisonOperation = value;
					InvalidateCachedQuery();
				}
			}
		}

		bool isEmptyProperty2ComparisonOperation;

		protected override bool IsEmptyCore => base.IsEmptyCore && !(IsEmptyProperty1ComparisonOperation || IsEmptyProperty2ComparisonOperation);

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Locations; }
		}

		#endregion

		#region Get Query for RefUNLOCO (Unloco), RefCountry (Country), and RefZoneHeader (International Zones)

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			result.AddToFilter(GetLocationFilter(Property1, FilterColumn1, IsEmptyProperty1ComparisonOperation, allowInternationalZones));
			result.AddToFilter(GetLocationFilter(Property2, FilterColumn2, IsEmptyProperty2ComparisonOperation, allowInternationalZones));

			return result;
		}

		public static ZQuery GetLocationFilter(ZString property, SchemaColumn column, bool isEmptyComparisonOperation, bool allowInternationalZones)
		{
			var result = new ZQuery();
			if (!property.IsEmpty)
			{
				var isCountryCode = property.Length == 2;
				var comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
				result.AddToFilter(column, comparisonOperator, property);

				var internationalZonesFilter = GetInternationalZonesFilter(property, column, allowInternationalZones);
				if (internationalZonesFilter != null)
				{
					result.AddToFilter(internationalZonesFilter, JoinCondition.Or);
				}
			}
			else if (isEmptyComparisonOperation)
			{
				if (column.IsNullable)
				{
					var nullOrEmptyFilter = new ZQuery(column, null);
					nullOrEmptyFilter.AddToFilter(JoinCondition.Or, column, property);
					result.AddToFilter(nullOrEmptyFilter);
				}
				else
				{
					result.AddToFilter(column, SQLComparisonOperator.Equal, property);
				}
			}

			return result;
		}

		#region Internationl Zones Filtering

		static ZDBOnlyQuery GetInternationalZonesFilter(ZString property, SchemaColumn column, bool allowInternationalZones)
		{
			if (!allowInternationalZones || string.IsNullOrEmpty(column.Name))
			{
				return null;
			}

			ZDBOnlySubQuery locationQuery;
			switch (property.Length)
			{
				case 2:
					locationQuery = GetCountryZonesSubQuery(property);
					break;
				case 5:
					locationQuery = GetUnlocoZonesSubQuery(property);
					break;

				default:
					locationQuery = null;
					break;
			}

			if (locationQuery != null)
			{
				var zoneHeaderSubQuery = new ZDBOnlySubQuery(typeof(IRefZoneHeader), RefZoneHeaderSchema.FZ_Code);
				zoneHeaderSubQuery.AddSubQuery(RefZoneHeaderSchema.PK, locationQuery, JoinCondition.And);

				var prefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(column.Name);
				var parentBizObjType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix);
				if (parentBizObjType != null)
				{
					var internationalZonesQuery = new ZDBOnlyQuery(parentBizObjType);
					internationalZonesQuery.AddSubQuery(column, zoneHeaderSubQuery, JoinCondition.And);

					return internationalZonesQuery;
				}
			}

			return null;
		}

		protected static ZDBOnlySubQuery GetCountryZonesSubQuery(ZString property)
		{
			var countrySubQuery = new ZDBOnlySubQuery(typeof(IRefCountry), RefCountrySchema.PK);
			countrySubQuery.AddToFilter(RefCountrySchema.RN_Code, property);

			var zonePivotSubQuery = new ZDBOnlySubQuery(typeof(IRefZonePivot), RefZonePivotSchema.F2_FZ);
			zonePivotSubQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefCountrySchema.Constants.Prefix);
			zonePivotSubQuery.AddSubQuery(RefZonePivotSchema.F2_ParentID, countrySubQuery, JoinCondition.And);

			return zonePivotSubQuery;
		}

		protected static ZDBOnlySubQuery GetUnlocoZonesSubQuery(ZString property)
		{
			var unlocoSubQuery = new ZDBOnlySubQuery(typeof(IRefUNLOCO), RefUNLOCOSchema.PK);
			unlocoSubQuery.AddToFilter(RefUNLOCOSchema.RL_Code, property);

			var zonePivotSubQuery = new ZDBOnlySubQuery(typeof(IRefZonePivot), RefZonePivotSchema.F2_FZ);
			zonePivotSubQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefUNLOCOSchema.Constants.Prefix);
			zonePivotSubQuery.AddSubQuery(RefZonePivotSchema.F2_ParentID, unlocoSubQuery, JoinCondition.And);

			return zonePivotSubQuery;
		}

		#endregion

		#endregion
	}
}
