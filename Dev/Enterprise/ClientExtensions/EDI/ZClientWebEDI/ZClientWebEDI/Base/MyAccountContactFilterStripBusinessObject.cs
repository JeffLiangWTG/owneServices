using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class MyAccountContactFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected MyAccountContactFilterStripBusinessObject(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			ModuleFilter filter = filters.AddTextFilter(FilterDescription.ContactName, OrgContactSchema.OC_ContactName);
			filter.MultilingualDescription = ResString.GetMultilingualString("59c9f3fb-d3ef-4b38-9bde-c3c8c35f54e9", "Contact Name");
			filter.Category = ContactCategory;

			filter = filters.AddTextFilter(FilterDescription.Email, OrgContactSchema.OC_Email);
			filter.MultilingualDescription = ResString.GetMultilingualString("0a3919ab-e555-40b7-a5b7-a595d4eca2e2", "Email");
			filter.Category = ContactCategory;

			var companyFilter = filters.AddTextFilter(FilterDescription.CompanyName, (comparisonOperator, value) =>
				 GetQueryByPredicate((x) => comparisonOperator.GetPredicate(value)(x.CompanyName)), GetCompanyNameList);
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("ea963dc8-492a-4288-8f9a-cd150b55f793", "Company Name");
			companyFilter.Category = FilterCategories.Other;
			RemoveComparisonOperators(companyFilter);

			var branchFilter = filters.AddTextFilter(FilterDescription.Branch, (comparisonOperator, value) =>
				 GetQueryByPredicate((x) => comparisonOperator.GetPredicate(value)(x.Branch)), GetBranchList);
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("87e56bad-65b2-4f65-b6f0-2d126bd3a540", "Branch");
			branchFilter.Category = FilterCategories.Other;
			RemoveComparisonOperators(branchFilter);

			filter = filters.AddNkFilter(FilterDescription.UNLOCO, (comparisonOperator, value) =>
				 GetQueryByPredicate((x) => comparisonOperator.GetPredicate(value)(x.UNLOCO)),
				 ModuleIDs.Location, new RefUNLOCOCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("16699f65-37dd-433f-a7ca-fbfd83751e40", "UNLOCO");
			filter.Category = FilterCategories.Other;

			var notificationRoleFilter = filters.AddTextFilter(FilterDescription.NotificationRole, GetNotificationRoleFilterQuery, () =>
			{
				return ContactsModuleColumnProvider.NotificationGroupList;
			});
			notificationRoleFilter.MultilingualDescription = ResString.GetMultilingualString("0eabe344-1da7-4919-958b-977071a85eaf", "Notification Role");
			notificationRoleFilter.Category = FilterCategories.Other;
			RemoveComparisonOperators(notificationRoleFilter, new[] { ModuleTextFilter.ComparisonConstants.IsBlank, ModuleTextFilter.ComparisonConstants.IsNotBlank, ModuleTextFilter.ComparisonConstants.Exact });

			#region AlwaysAppliedAndHidden

			if (MasterOrg != null)
			{
				var headerFilter = filters.AddGuidFilter(FilterDescription.OrgHeader, ModuleIDs.Organisation, OrgContactSchema.OC_OH,
					new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.PK, MasterOrg.PK)));
				headerFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
				headerFilter.DefaultProperty = MasterOrg.PK;
			}

			var activeFilter = filters.AddFlagsFilter(FilterDescription.Active, new[] { FilterDescription.Active }, new[] { OrgContactSchema.OC_IsActive });
			activeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			activeFilter.DefaultProperties[FilterDescription.Active] = true;

			#endregion AlwaysAppliedAndHidden

			return filters;
		}

		#region NotificationRoleFilter

		ZQuery GetNotificationRoleFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var notificationGroups = new NotificationRolesContactsModuleColumnProvider().NotificationGroupList.GetAllCodes().Select(x => (ZString)x).ToArray();
			var contacts = MasterOrg.ContactsActive.OfType<OrgContact>();

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				contacts = contacts.Where(x => !notificationGroups.Any(n => x.Documents.ContainsDocGroup(n)));
			}
			else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				contacts = contacts.Where(x => notificationGroups.Any(n => x.Documents.ContainsDocGroup(n)));
			}
			else if (comparisonOperator == SQLComparisonOperator.Equal)
			{
				contacts = contacts.Where(x => x.Documents.ContainsDocGroup(value));
			}
			else
			{
				contacts = Enumerable.Empty<OrgContact>();
			}

			return GetQueryByPKs(contacts.Select(x => x.PK).ToArray());
		}

		readonly NotificationRolesContactsModuleColumnProvider ContactsModuleColumnProvider = new NotificationRolesContactsModuleColumnProvider();

		#endregion

		protected static void RemoveComparisonOperators(ModuleTextFilter filter, IEnumerable<string> expectedOperators = null)
		{
			if (expectedOperators == null)
			{
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			}
			else
			{
				foreach (var code in filter.ComparisonOperator_List.GetAllCodes().Except(expectedOperators))
				{
					filter.ComparisonOperator_List.RemoveCode(code);
				}
			}
		}

		static FilterCategory ContactCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("3cb1f926-d074-438c-9780-3e27aaac1788", "Contact"));

		protected override IReadOnlyList<FilterCategory> CategorySortOrderCore => new[] { ContactCategory };

		#region Lists

		IList GetCompanyNameList() => MasterOrg.Factory.GetCachedValue("MyAccountContactFilter.CompanyNameList", () =>
		{
			var list = new CodeDescriptionPairList();

			foreach (var name in ContactSnapshots.Select(x => x.CompanyName).Distinct().Where(x => !x.IsEmpty))
			{
				list.AddPair(name, name);
			}

			return list;
		});

		IList GetBranchList() => MasterOrg.Factory.GetCachedValue("MyAccountContactFilter.BranchList", () =>
		{
			var list = new CodeDescriptionPairList();

			foreach (var name in ContactSnapshots.Select(x => x.Branch).Distinct().Where(x => !x.IsEmpty))
			{
				list.AddPair(name, name);
			}

			return list;
		});

		#endregion Lists

		ZQuery GetQueryByPredicate(Predicate<ContactSnapshot> predicate)
		{
			var pks = ContactSnapshots.Where(x => predicate(x)).Select(x => x.PK).ToArray();
			return GetQueryByPKs(pks);
		}

		protected static ZQuery GetQueryByPKs(IEnumerable<ZGuid> contactPKs)
		{
			if (contactPKs.Any())
			{
				var tvpName = FormattableString.Invariant($"@ContactPKs_{ZGuid.NewZGuid()}").Replace("-", "");
				var parameterCollection = new ZSqlParameterCollection();
				var sql = FormattableString.Invariant($" OC_PK IN (SELECT Value FROM {tvpName}) ");

				parameterCollection.Add(ZSqlParameter.New(tvpName, contactPKs, OrgContactSchema.PK, isTableValued: true));
				var query = new ZQuery();
				query.AddFilterAndZSQLParameterCollection(sql, parameterCollection);

				return query;
			}
			else
			{
				return new ZQuery(OrgContactSchema.PK, ZGuid.Empty);
			}
		}

		IEnumerable<ContactSnapshot> ContactSnapshots
			=> MasterOrg.Factory.GetCachedValue("MyAccountContactFilter.ContactSnapshots", () => MasterOrg.ContactsActive.OfType<EDIOrgContact>().Select(x => new ContactSnapshot(x)).ToArray());

		protected virtual OrgHeader MasterOrg { get; }

		class ContactSnapshot
		{
			public ContactSnapshot(EDIOrgContact contact)
			{
				PK = contact.PK;
				CompanyName = contact.CompanyNameForBindingOnly;
				Branch = contact.BranchForBindingOnly;
				UNLOCO = contact.OrgClosestPort?.Code ?? ZString.Empty;
			}

			public readonly ZGuid PK;
			public readonly ZString CompanyName;
			public readonly ZString Branch;
			public readonly ZString UNLOCO;
		}
	}
}
