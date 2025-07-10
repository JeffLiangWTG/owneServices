using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetDateTimeOffsetQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2);

	public class ModuleDateTimeOffsetFilter : ModuleDateFilter
	{
		//TODO: 'ConvertFromLocalToUTC' doesn't make much sense when it's already time zone aware on both ends. What would make more sense is like 'assume inputted values are UTC' or 'use time zone of a different UNLOCO'.

		#region Construction

		protected ModuleDateTimeOffsetFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: this(category, parentCollection, false)
		{
		}

		public ModuleDateTimeOffsetFilter(ZString description, SchemaDateTimeOffsetColumn filterColumn)
			: this(description, filterColumn, false)
		{
		}

		public ModuleDateTimeOffsetFilter(ZString description, GetDateTimeOffsetQuery queryDelegate, bool isNullable = true)
			: this(description, queryDelegate, false, isNullable)
		{
		}

		protected ModuleDateTimeOffsetFilter(FilterCategory category, ModuleFilterCollection parentCollection, bool convertFromLocalToUTC)
			: base(category, parentCollection)
		{
			ConvertFromLocalToUTC = convertFromLocalToUTC;
		}

		protected ModuleDateTimeOffsetFilter(ZString description, bool convertFromLocalToUTC = false, bool isNullable = true)
			: base(description)
		{
			ConvertFromLocalToUTC = convertFromLocalToUTC;
			this.isNullable = isNullable;
		}

		public ModuleDateTimeOffsetFilter(ZString description, SchemaDateTimeOffsetColumn filterColumn, bool convertFromLocalToUTC)
			: base(description)
		{
			EnsureFilterColumnIsNotNull(filterColumn);
			FilterColumn = filterColumn;
			ConvertFromLocalToUTC = convertFromLocalToUTC;
			isNullable = filterColumn.IsNullable;
		}

		public ModuleDateTimeOffsetFilter(ZString description, GetDateTimeOffsetQuery queryDelegate, bool convertFromLocalToUTC, bool isNullable = true)
			: base(description)
		{
			EnsureQueryDelegateIsNotNull(queryDelegate);
			QueryDelegate = queryDelegate;
			ConvertFromLocalToUTC = convertFromLocalToUTC;
			this.isNullable = isNullable;
		}

		#endregion

		protected override IZType GetTypeForQuery(ZDateTime value)
		{
			//Note that if value is derived from UtcNow/UtcToday it will have Utc kind, which means it will have no offset when passed to ZDateTimeOffset constructor.
			//If it isn't, it will be treated as local time (to the current Branch UNLOCO).
			return new ZDateTimeOffset(value);
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { GetComparisonOperator(), new ZDateTimeOffset(FromDate), new ZDateTimeOffset(ToDate) }; }
		}
	}
}
