using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleAuditFilterProviderTest : TestCaseWithFactory
	{
		public void TestAddAuditFilters()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, AccTransactionHeaderSchema.Instance, Factory, typeof(BusinessObject));

			AssertNotNull(filters["Creating User"]);
			AssertNotNull(filters["Creating Branch"]);
			AssertNotNull(filters["Creating Department"]);
			AssertNotNull(filters["Last Edit User"]);
			AssertNotNull(filters["Created Time"]);
			AssertNotNull(filters["Last Edit Time"]);
			AssertNotNull(filters["Created On Web/Internal"]);
			AssertNotNull(filters["Cashier"]);
			AssertNotNull(filters["Auditing User"]);
		}

		public void TestAddCreatingUserFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleNkFilter)filters["Creating User"];

			AssertEquals("Should not be published on Web", false, filter.IsPublishedOnWeb);
			filter.Property = "ZA";
			filter.IsActive = true;

			AssertEquals("TH_SystemCreateUser = 'ZA'", filter.Query.LiteralTextADO);
			AssertNotNull(filter.List);

			AssertEquals("STR_SystemCreateUser = 'ZA'", filter.XQuery.LiteralTextADO);

			filter.Property = "";
			AssertEquals("", filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals("STR_SystemCreateUser = ''", filter.XQuery.LiteralTextADO);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals("STR_SystemCreateUser <> ''", filter.XQuery.LiteralTextADO);
		}

		public void TestCreatingBranchFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleNkFilter)filters["Creating Branch"];

			AssertEquals("Should not be published on Web", false, filter.IsPublishedOnWeb);
			filter.Property = "ZA";
			filter.IsActive = true;

			AssertEquals("TH_SystemCreateBranch = 'ZA'", filter.Query.LiteralTextADO);
			AssertNotNull(filter.List);
		}

		public void TestCreatingDepartmentFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleNkFilter)filters["Creating Department"];

			AssertEquals("Should not be published on Web", false, filter.IsPublishedOnWeb);
			filter.Property = "ZA";
			filter.IsActive = true;

			AssertEquals("TH_SystemCreateDepartment = 'ZA'", filter.Query.LiteralTextADO);
			AssertNotNull(filter.List);
		}

		public void TestCreatedOnWebFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleTextFilter)filters["Created On Web/Internal"];

			filter.IsActive = true;
			AssertEquals("Default Property", "ALL", filter.Property);
			filter.Property = "WEB";
			AssertEquals("TH_SystemCreateUser = 'ZZ'", filter.Query.LiteralTextADO);

			filter.Property = "ALL";
			AssertEquals("", filter.Query.LiteralTextADO);

			filter.Property = "ENT";
			AssertEquals("TH_SystemCreateUser <> 'ZZ'", filter.Query.LiteralTextADO);

			var filterOptions = filter.List as CodeDescriptionPairList;
			AssertNotNull(filterOptions);

			AssertEquals(3, filterOptions.Count);

			AssertEquals("ALL", filterOptions[0].Code);
			AssertEquals("All Records", filterOptions[0].Description);

			AssertEquals("WEB", filterOptions[1].Code);
			AssertEquals("Created using WebTracker", filterOptions[1].Description);

			AssertEquals("ENT", filterOptions[2].Code);
			AssertEquals($"Created using {Constants.ProductName}", filterOptions[2].Description);
		}

		public void TestAddLastEditUserFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleNkFilter)filters["Last Edit User"];

			AssertEquals("Should not be published on Web", false, filter.IsPublishedOnWeb);
			filter.Property = "ZA";
			filter.IsActive = true;

			AssertEquals("TH_SystemLastEditUser = 'ZA'", filter.Query.LiteralTextADO);
			AssertNotNull(filter.List);
		}

		public void TestAddAuditingUserFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, AccTransactionHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleNkFilter)filters["Auditing User"];

			AssertEquals("Should not be published on Web", false, filter.IsPublishedOnWeb);
			filter.Property = "JYW";
			filter.IsActive = true;

			AssertEquals("AH_GS_NKAuditedBy = 'JYW'", filter.Query.LiteralTextADO);
			AssertNotNull(filter.List);
		}

		public void TestAddCashierFilter()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, AccTransactionHeaderSchema.Instance, Factory, typeof(BusinessObject));
			var filter = (ModuleNkFilter)filters["Cashier"];

			AssertEquals("Should not be published on Web", false, filter.IsPublishedOnWeb);
			filter.Property = "JYW";
			filter.IsActive = true;

			AssertEquals("AH_GS_NKCashier = 'JYW'", filter.Query.LiteralTextADO);
			AssertNotNull(filter.List);
		}

		#region TestAddCreateDateTimeFilter

		[TestUtcOffset(10, 1, 0)]
		public void TestAddCreateDateTimeFilter_WithUtcValueAndUtcDisplay()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(DummyBusinessObject_WithUtcValueAndUtcDisplay));
			var filter = (ModuleDateFilter)filters["Created Time"];

			filter.Property1 = new ZDateTime(2007, 1, 2);
			filter.Property2 = new ZDateTime(2007, 1, 5);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("Created Time (UTC)", filter.MultilingualDescription);
			AssertEquals("TH_SystemCreateTimeUtc >= #2007-01-02 00:00:00.000# and TH_SystemCreateTimeUtc < #2007-01-06 00:00:00.000#", filter.Query.LiteralTextADO);
		}

		[TestUtcOffset(10, 1, 0)]
		public void TestAddCreateDateTimeFilter_WithUtcValueAndLocalDisplay()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(DummyBusinessObject_WithUtcValueAndLocalDisplay));
			var filter = (ModuleDateFilter)filters["Created Time"];

			filter.Property1 = new ZDateTime(2007, 1, 2);
			filter.Property2 = new ZDateTime(2007, 1, 5);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("Created Time", filter.MultilingualDescription);
			AssertEquals("TH_SystemCreateTimeUtc >= #2007-01-01 13:59:00.000# and TH_SystemCreateTimeUtc <= #2007-01-05 13:58:00.000#", filter.Query.LiteralTextADO);
		}

		#endregion

		#region TestAddLastEditDateTimeFilter

		[TestUtcOffset(10, 1, 0)]
		public void TestAddLastEditDateTimeFilter_WithUtcValueAndUtcDisplay()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(DummyBusinessObject_WithUtcValueAndUtcDisplay));
			var filter = (ModuleDateFilter)filters["Last Edit Time"];

			filter.Property1 = new ZDateTime(2007, 1, 2);
			filter.Property2 = new ZDateTime(2007, 1, 5);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("Last Edit Time (UTC)", filter.MultilingualDescription);
			AssertEquals("TH_SystemLastEditTimeUtc >= #2007-01-02 00:00:00.000# and TH_SystemLastEditTimeUtc < #2007-01-06 00:00:00.000#", filter.Query.LiteralTextADO);
		}

		[TestUtcOffset(10, 1, 0)]
		public void TestAddLastEditDateTimeFilter_WithUtcValueAndLocalDisplay()
		{
			var filters = new ModuleFilterCollection();
			ModuleAuditFilterProvider.AddAuditFilters(filters, RatingHeaderSchema.Instance, Factory, typeof(DummyBusinessObject_WithUtcValueAndLocalDisplay));
			var filter = (ModuleDateFilter)filters["Last Edit Time"];

			filter.Property1 = new ZDateTime(2007, 1, 2);
			filter.Property2 = new ZDateTime(2007, 1, 5);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("Last Edit Time", filter.MultilingualDescription);
			AssertEquals("TH_SystemLastEditTimeUtc >= #2007-01-01 13:59:00.000# and TH_SystemLastEditTimeUtc <= #2007-01-05 13:58:00.000#", filter.Query.LiteralTextADO);
		}

		#endregion

		#region Implementation

		#region DummyBusinessObject Classes

		[ShouldDisplayInUtcTimeForEditAndCreateLogFields]
		class DummyBusinessObject_WithUtcValueAndUtcDisplay : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObject_WithUtcValueAndUtcDisplay(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummyBusinessObject_WithUtcValueAndLocalDisplay : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObject_WithUtcValueAndLocalDisplay(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion

		#endregion
	}
}
