using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public static class ModuleAuditFilterProvider
	{
		public static void AddAuditFilters(ModuleFilterCollection filters, ITableSchema tableSchema, BusinessObjectFactory factory, Type typeOfElements, string webProductName = "WebTracker")
		{
			if (tableSchema != null && typeOfElements != null)
			{
				var tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableSchema.TableName);

				var createUserColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_SystemCreateUser"];
				var createBranchColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_SystemCreateBranch"];
				var createDepartmentColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_SystemCreateDepartment"];
				var createTimeColumn = (SchemaDateTimeColumn)tableSchema.All[tablePrefix + "_SystemCreateTimeUtc"];
				var lastEditUserColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_SystemLastEditUser"];
				var lastEditTimeColumn = (SchemaDateTimeColumn)tableSchema.All[tablePrefix + "_SystemLastEditTimeUtc"];
				var auditingUserColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_GS_NKAuditedBy"];
				var cashierColumn = (SchemaStringColumn)tableSchema.All[tablePrefix + "_GS_NKCashier"];

				var displayTimeKind = DateTimeKind.Unspecified;
				if (createTimeColumn != null || lastEditTimeColumn != null)
				{
					displayTimeKind = GetDisplayDateTimeKind(typeOfElements);
				}

				if (createUserColumn != null)
				{
					AddCreateUserFilter(filters, createUserColumn, factory);
				}

				if (createBranchColumn != null)
				{
					AddCreateBranchFilter(filters, createBranchColumn, factory);
				}

				if (createDepartmentColumn != null)
				{
					AddCreateDepartmentFilter(filters, createDepartmentColumn, factory);
				}

				if (createTimeColumn != null)
				{
					AddCreateTimeFilter(filters, createTimeColumn, displayTimeKind);
				}

				if (lastEditUserColumn != null)
				{
					AddLastEditUserFilter(filters, lastEditUserColumn, factory);
				}

				if (lastEditTimeColumn != null)
				{
					AddLastEditTimeFilter(filters, lastEditTimeColumn, displayTimeKind);
				}

				if (createUserColumn != null)
				{
					AddCreatedOnWebFilter(filters, createUserColumn, webProductName);
				}

				if (cashierColumn != null)
				{
					AddCashierFilter(filters, cashierColumn, factory);
				}

				if (auditingUserColumn != null)
				{
					AddAuditingUserFilter(filters, auditingUserColumn, factory);
				}
			}
		}

		#region Implementation

		static ModuleTextFilter AddCreatedOnWebFilter(ModuleFilterCollection filters, SchemaStringColumn creatingUserColumn, string webProductName)
		{
			var filter = new ModuleTextFilter(FilterDescriptions.CreatedOnWeb, delegate (ZString createdOn)
				{
					var result = new ZQuery();
					if (createdOn == CreatedOnCodes.WebTracker)
					{
						result.AddToFilter(creatingUserColumn, User.WebUserCode);
					}
					else if (createdOn == CreatedOnCodes.Enterprise)
					{
						result.AddToFilter(creatingUserColumn, SQLComparisonOperator.NotEqual, User.WebUserCode);
					}
					return result;
				},
				CreatedOnWebList(webProductName));

			filter.Category = FilterCategories.AuditInformation;
			filter.DefaultProperty = CreatedOnCodes.All;
			filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|CreatedOnWebInternal", "Created On Web/Internal");
			filters.AddFilter(filter);
			return filter;
		}

		static CodeDescriptionPairList CreatedOnWebList(string webProductName)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CreatedOnCodes.All, Res.GetString("f4d58f54-f2e9-4d33-8458-43912830aeae", "All Records"));
			result.AddPair(CreatedOnCodes.WebTracker, Res.GetString("485a2011-e94b-4523-ab24-823e0c27d13a", "Created using {0}", webProductName));
			result.AddPair(CreatedOnCodes.Enterprise, Res.GetString("09cdff7d-a84c-4561-91da-954aefaaec73", "Created using {0}", Constants.ProductName));

			return result;
		}

		static class CreatedOnCodes
		{
			public const string All = "ALL";
			public const string WebTracker = "WEB";
			public const string Enterprise = "ENT";
		}

		static void AddCreateUserFilter(ModuleFilterCollection filters, SchemaStringColumn creatingUserColumn, BusinessObjectFactory factory)
		{
			var filter = new ModuleNkFilter(FilterDescriptions.CreatingUser, creatingUserColumn, ModuleIDs.GlbStaff, GetStaffList(factory));
			filter.GetXQuery = GetSystemCreateUserXQuery;
			filter.Category = FilterCategories.AuditInformation;
			filter.IsPublishedOnWeb = false;
			filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|CreatingUser", "Creating User");
			filters.AddFilter(filter);
		}

		public static ZQuery GetSystemCreateUserXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleNkFilter;
			return filter.SqlComparisonOperator != SQLComparisonOperator.NotSpecified
				&& (!filter.Property.IsEmpty || filter.SqlComparisonOperator == SpecialComparisonOperator.IsBlank || filter.SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank)
				? new ZQuery(StmTemplateRecordSchema.STR_SystemCreateUser, filter.SqlComparisonOperator, filter.Property) : new ZQuery();
		}

		static void AddCreateBranchFilter(ModuleFilterCollection filters, SchemaStringColumn creatingBranchColumn, BusinessObjectFactory factory)
		{
			var filter = new ModuleNkFilter(FilterDescriptions.CreatingBranch, creatingBranchColumn, ModuleIDs.GlbBranch, GetBranchList(factory));
			filter.Category = FilterCategories.AuditInformation;
			filter.IsPublishedOnWeb = false;
			filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|CreatingBranch", "Creating Branch");
			filters.AddFilter(filter);
		}

		static void AddCreateDepartmentFilter(ModuleFilterCollection filters, SchemaStringColumn creatingDepartmentColumn, BusinessObjectFactory factory)
		{
			var filter = new ModuleNkFilter(FilterDescriptions.CreatingDepartment, creatingDepartmentColumn, ModuleIDs.GlbDepartment, GetDepartmentList(factory));
			filter.Category = FilterCategories.AuditInformation;
			filter.IsPublishedOnWeb = false;
			filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|CreatingDepartment", "Creating Department");
			filters.AddFilter(filter);
		}

		static void AddCreateTimeFilter(ModuleFilterCollection filters, SchemaDateTimeColumn createDateColumn, DateTimeKind displayDateTimeKind)
		{
			var filter = CreateAuditColumnFilter(displayDateTimeKind, FilterDescriptions.CreatedTime, createDateColumn);

			filter.MultilingualDescription = filter.UserEntersUtcValue
				? ResString.GetMultilingualString("ZArchitecture|AuditFilters|CreatedTimeUTC", "Created Time (UTC)")
				: ResString.GetMultilingualString("ZArchitecture|AuditFilters|CreatedTime", "Created Time");

			filters.AddFilter(filter);
		}

		static void AddCashierFilter(ModuleFilterCollection filters, SchemaStringColumn cashierColumn, BusinessObjectFactory factory)
		{
			var filter = new ModuleNkFilter(FilterDescriptions.Cashier, cashierColumn, ModuleIDs.GlbStaff, GetStaffList(factory))
			{
				Category = FilterCategories.AuditInformation,
				IsPublishedOnWeb = false,
				MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|Cashier", "Cashier")
			};
			filters.AddFilter(filter);
		}

		static void AddAuditingUserFilter(ModuleFilterCollection filters, SchemaStringColumn auditingUserColumn, BusinessObjectFactory factory)
		{
			var filter = new ModuleNkFilter(FilterDescriptions.AuditingUser, auditingUserColumn, ModuleIDs.GlbStaff, GetStaffList(factory))
			{
				Category = FilterCategories.AuditInformation,
				IsPublishedOnWeb = false,
				MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|AuditingUser", "Auditing User")
			};
			filters.AddFilter(filter);
		}

		static void AddLastEditUserFilter(ModuleFilterCollection filters, SchemaStringColumn lastEditUserColumn, BusinessObjectFactory factory)
		{
			var filter = new ModuleNkFilter(FilterDescriptions.LastEditUser, lastEditUserColumn, ModuleIDs.GlbStaff, GetStaffList(factory));
			filter.Category = FilterCategories.AuditInformation;
			filter.IsPublishedOnWeb = false;
			filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|AuditFilters|LastEditUser", "Last Edit User");
			filters.AddFilter(filter);
		}

		static void AddLastEditTimeFilter(ModuleFilterCollection filters, SchemaDateTimeColumn lastEditDateColumn, DateTimeKind displayDateTimeKind)
		{
			var filter = CreateAuditColumnFilter(displayDateTimeKind, FilterDescriptions.LastEditTime, lastEditDateColumn);

			filter.MultilingualDescription = filter.UserEntersUtcValue
				? ResString.GetMultilingualString("ZArchitecture|AuditFilters|LastEditTimeUTC", "Last Edit Time (UTC)")
				: ResString.GetMultilingualString("ZArchitecture|AuditFilters|LastEditTime", "Last Edit Time");

			filters.AddFilter(filter);
		}

		static ModuleDateFilter CreateAuditColumnFilter(DateTimeKind displayDateTimeKind, string filterName, SchemaDateTimeColumn schemaDateColumn)
		{
			var convertFromLocalToUtc = displayDateTimeKind == DateTimeKind.Local;
			var userEntersUtcValue = displayDateTimeKind == DateTimeKind.Utc;

			return new ModuleDateFilter(filterName, schemaDateColumn, convertFromLocalToUtc)
			{
				Category = FilterCategories.AuditInformation,
				UserEntersUtcValue = userEntersUtcValue,
			};
		}

		static IBusinessObjectCollection GetStaffList(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbStaffCollection>(), new object[] { factory });
		}

		static IBusinessObjectCollection GetBranchList(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbBranchCollection>(), new object[] { factory });
		}

		static IBusinessObjectCollection GetDepartmentList(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbDepartmentCollection>(), new object[] { factory });
		}

		internal static DateTimeKind GetDisplayDateTimeKind(Type typeOfElement)
		{
			var displayInUtc = ShouldDisplayInUtcTimeForEditAndCreateLogFieldsAttribute.IsApplied(typeOfElement);

			return displayInUtc ? DateTimeKind.Utc : DateTimeKind.Local;
		}

		#endregion
	}
}
