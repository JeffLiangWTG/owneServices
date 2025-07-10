using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Module.EntryHeaderFilterBusinessObject;

namespace Enterprise.Customs.EU.Module
{
	public class EntryHeaderAddressesFilterApplier
	{
		public EntryHeaderAddressesFilterApplier(EntryHeaderFilterBusinessObject parentFilter)
		{
			this.parentFilter = Argument.NotNull(parentFilter, nameof(parentFilter));
		}

		readonly EntryHeaderFilterBusinessObject parentFilter;

		public void AddFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			AddFromWarehouseFilter(filters);
			AddToWarehouseFilter(filters);
			AddLocalClientCodeFilter(filters);
		}

		#region From Warehouse

		void AddFromWarehouseFilter(ModuleFilterCollection filters)
		{
			var fromWarehouseFilter = filters.AddGuidFilter(EUFilterConstants.AddressFromWarehouseFilter, ModuleIDs.Organisation, GetFromWarehouseFilterQuery, Lookups.Warehouses);
			fromWarehouseFilter.MultilingualDescription = ResString.GetMultilingualString("41924E1D-D1D8-4D33-9733-15687655EE35", EUFilterConstants.AddressFromWarehouseFilter);
			fromWarehouseFilter.Category = AddressCategory;

			ZQuery GetFromWarehouseFilterQuery(ZGuid warehouse) => GetWarehouseFilterQuery(CusEntryInstructionSchema.CEI_OA_Warehouse, warehouse);
		}

		#endregion

		#region To Warehouse

		void AddToWarehouseFilter(ModuleFilterCollection filters)
		{
			var toWarehouseFilter = filters.AddGuidFilter(EUFilterConstants.AddressToWarehouseFilter, ModuleIDs.Organisation, GetToWarehouseFilterQuery, Lookups.Warehouses);
			toWarehouseFilter.MultilingualDescription = ResString.GetMultilingualString("165BE312-AC4B-4701-893B-D53661734039", EUFilterConstants.AddressToWarehouseFilter);
			toWarehouseFilter.Category = AddressCategory;

			ZQuery GetToWarehouseFilterQuery(ZGuid warehouse) => GetWarehouseFilterQuery(CusEntryInstructionSchema.CEI_OA_Warehouse2, warehouse);
		}

		#endregion

		#region Local Client Code

		void AddLocalClientCodeFilter(ModuleFilterCollection filters)
		{
			var localClientFilter = new ModuleGuidFilterForOrg(EUFilterConstants.AddressLocalClientCodeFilter, ModuleIDs.Organisation, GetLocalClientFilterQuery, Lookups.LocalClients);
			localClientFilter.Category = AddressCategory;
			localClientFilter.MultilingualDescription = ResString.GetMultilingualString("C82FDDA9-7741-4946-B3E7-B49BDFF68B50", EUFilterConstants.AddressLocalClientCodeFilter);
			filters.AddFilter(localClientFilter);
		}

		#endregion

		ZQuery GetWarehouseFilterQuery(SchemaColumn columnName, ZGuid warehouse)
		{
			var query = GetEntryHeaderQuery();
			var entryInstructionQuery = GetEntryInstrcutionSubQuery();
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), columnName);

			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, warehouse);
			entryInstructionQuery.AddSubQuery(addressQuery, JoinCondition.And);
			query.AddSubQuery(entryInstructionQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetLocalClientFilterQuery(ZGuid localClient)
		{
			var cusHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, CusEntryHeaderSchema.CH_JE);
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);

			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, localClient);
			jobHeaderSubQuery.AddSubQuery(addressQuery, JoinCondition.And);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_IsActive, true);
			cusHeaderQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);

			return cusHeaderQuery;
		}

		EntryHeaderFilterLookups Lookups => parentFilter.Lookups;

		ZDBOnlyQuery GetEntryHeaderQuery() => new ZDBOnlyQuery(typeof(CusEntryHeader));

		ZDBOnlySubQuery GetEntryInstrcutionSubQuery() => new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction);

		FilterCategory AddressCategory => addressCategory ?? (addressCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("EntryHeaderFilter|Address", "Address")));
		FilterCategory addressCategory;
	}
}
