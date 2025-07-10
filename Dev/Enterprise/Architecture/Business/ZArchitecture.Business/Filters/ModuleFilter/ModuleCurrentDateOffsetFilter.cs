using System;
using System.Globalization;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleCurrentDateOffsetFilter : ModuleFilter
	{
		#region Schema

		public abstract class Schema
		{
			public const string Offset = "Offset";
			public const string FromCompareDate = "FromCompareDate";
			public const string ToCompareDate = "ToCompareDate";
		}

		#endregion

		public ModuleCurrentDateOffsetFilter(ZString description, SchemaDateTimeColumn parentSchemaColumn)
			: base(description, new BusinessObjectFactory())
		{
			this.ParentSchemaColumn = parentSchemaColumn;
			SetDefaultFilterValues();
		}

		readonly SchemaDateTimeColumn ParentSchemaColumn;

		#region CopyPersistantValuesFromFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleCurrentDateOffsetFilter)filterToCopyFrom;
			Offset = filter.Offset;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		void SetDefaultFilterValues()
		{
			Offset = 0;
		}

		protected override void ClearCore()
		{
			SetDefaultFilterValues();
		}

		protected override bool IsEmptyCore => OffsetInfo.HasErrors();

		public bool IsDateEmpty
		{
			get { return base.IsEmpty; }
		}

		#region Offset

		public ZInt Offset
		{
			get { return offset; }
			set
			{
				if (Offset != value)
				{
					InvalidateCachedQuery();
				}

				offset = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOffset();
				}
				OffsetInfo.RefreshBinding();
			}
		}
		ZInt offset;

		public ZPropertyInfo OffsetInfo => GetZPropertyInfo(nameof(Offset));

		#endregion

		#region FromCompareDate

		public ZDateTime FromCompareDate
		{
			get
			{
				return ZDateTime.UtcToday.AddDays(-1 * offset);
			}
		}

		public ZPropertyInfo FromCompareDateInfo => GetZPropertyInfo(nameof(FromCompareDate));

		#endregion

		#region FromCompareDate

		public ZDateTime ToCompareDate
		{
			get
			{
				return ZDateTime.UtcToday.AddDays(offset);
			}
		}

		public ZPropertyInfo ToCompareDateInfo => GetZPropertyInfo(nameof(ToCompareDate));

		#endregion

		#region Validation

		public new ModuleCurrentDateOffsetFilterValidation Validation
		{
			get { return (ModuleCurrentDateOffsetFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleCurrentDateOffsetFilterValidation(this);
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var query = new ZDBOnlyQuery(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ParentSchemaColumn.ColumnPrefix));
				query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "ABS(DATEDIFF(day, {0}, GetUTCDate())) <= {1}", ParentSchemaColumn.Name, Offset), null);
				return query;
			}
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Offset }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (Offset > 0)
			{
				return new ZDBOnlyQuery(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(ParentSchemaColumn.ColumnPrefix)).AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "ABS(DATEDIFF(day, {0}, GetUTCDate())) <= {1}", ParentSchemaColumn.Name, Offset), null);
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Offset, Offset.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			Offset = ZInt.Parse(reader.ReadElementString(Schema.Offset));
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Offset = 1;
		}

#endif
		#endregion

		#region static create control function
		public static void AddCurrentDateOffsetFilter(ModuleFilterCollection filters, ZString description, SchemaDateTimeColumn parentSchemaColumn, FilterCategory category, ModuleFilterSubGroup subGroup)
		{
			var filter = new ModuleCurrentDateOffsetFilter(description, parentSchemaColumn);
			filter.Category = category;
			filter.SubGroup = subGroup;

			filters.AddFilter(filter);
		}
		#endregion
	}
}
