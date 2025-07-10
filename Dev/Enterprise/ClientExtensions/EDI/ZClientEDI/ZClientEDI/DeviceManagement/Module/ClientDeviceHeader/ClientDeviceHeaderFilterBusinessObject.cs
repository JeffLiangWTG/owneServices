using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	public class ClientDeviceHeaderFilterBusinessObject : ClientDeviceHeaderTemplateFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			filters.AddTextFilter("Hardware ID", DmgDeviceHeaderSchema.CDH_DeviceIdentifier);
			filters.AddTextFilter("Identifier", DmgDeviceHeaderSchema.CDH_Identifier);
			filters.AddTextFilter("Device Kind", DmgDeviceHeaderSchema.CDH_DeviceKind, () => new ClientDeviceHeaderLookups(null).DeviceKinds);

			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, GetOrganisationQuery, () => new OrgHeaderCollection(Factory));

			filters.AddTextFilter("Allocated Status", GetAllocatedStatusQuery, AllocationStatusList);

			return filters;
		}

		ZQuery GetOrganisationQuery(ZGuid value)
		{
			if (value.IsValid)
			{
				var licenceEnterpriseQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.LE_EnterpriseCode);
				licenceEnterpriseQuery.AddToFilter(LicenceEnterpriseSchema.LE_OH, value);

				var query = new ZDBOnlyQuery(typeof(ClientDeviceHeader));
				query.AddSubQuery(DmgDeviceHeaderSchema.CDH_EnterpriseCode, licenceEnterpriseQuery, JoinCondition.And);
				query.AddToFilter(JoinCondition.And, DmgDeviceHeaderSchema.CDH_EnterpriseCode, SQLComparisonOperator.NotEqual, string.Empty);
				return query;
			}

			return null;
		}

		#region Allocation Status Query

		CodeDescriptionPairList AllocationStatusList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(AllDevices);
				result.AddPair(Allocated);
				result.AddPair(NotAllocated);
				return result;
			}
		}

		const string AllDevices = "All Devices";
		const string Allocated = "Allocated to a Customer";
		const string NotAllocated = "Not allocated";

		ZQuery GetAllocatedStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == Allocated)
			{
				return new ZQuery(DmgDeviceHeaderSchema.CDH_ServerCode, SQLComparisonOperator.NotEqual, null);
			}
			else if (value == NotAllocated)
			{
				return new ZQuery(DmgDeviceHeaderSchema.CDH_ServerCode, null);
			}

			return ZQuery.NoResultQuery;
		}

		#endregion
	}
}
