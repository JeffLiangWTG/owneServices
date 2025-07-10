using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	#region class ModuleNumberFilter

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModulePeriodFilter : ModuleTextFilter
	{
		#region Construction

		protected ModulePeriodFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModulePeriodFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModulePeriodFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#endregion

		#region GetNewCommonModuleFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModulePeriodFilter(category, parentCollection);
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Messages created for sql queries or support only, not customers")]
		protected override void FillWithValidTestFilterValueCore()
		{
			var period = (ZDateTime.Today.Year * 100 + RandomInt(79) + 20).ToString(CultureInfo.InvariantCulture);
			var sql = $"INSERT INTO {AccPeriodManagementSchema.Constants.SqlSchemaName}.{AccPeriodManagementSchema.Constants.TableName} ({AccPeriodManagementSchema.Constants.PK}, {AccPeriodManagementSchema.Constants.AM_Period}, {AccPeriodManagementSchema.Constants.AM_StartDate}, {AccPeriodManagementSchema.Constants.AM_EndDate}, {AccPeriodManagementSchema.Constants.AM_GC_Company}) VALUES (NEWID(), '{period}', 'Aug 16 2016 3:44:00:000PM','Aug 31 2016 3:44:00:000PM','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC') ";
			Db.Connection.ExecuteNonQuery(sql);

			Property = period;
		}

#endif
		#endregion
	}

	#endregion
}
