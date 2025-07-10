using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	public class ClientDeviceHeaderTemplateFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Model", DmgDeviceHeaderSchema.CDH_ModelID);
			filters.AddTextFilter("Description", DmgDeviceHeaderSchema.CDH_Description);
			filters.AddTextFilter("Status", DmgDeviceHeaderSchema.CDH_Status, () => new ClientDeviceHeaderLookups(null).StatusTypes);
			filters.AddTextFilter("Component Batch", GetComponentBatchQuery)
				.WithMaxLengthOf<ModuleTextFilter>(DmgDeviceComponentSchema.CDC_BatchNumber);
			filters.AddTextFilter("Component Identifier", GetIdentifierQuery)
				.WithMaxLengthOf<ModuleTextFilter>(DmgDeviceComponentIdentificationSchema.CDD_Identifier);

			return filters;
		}

		ZQuery GetComponentBatchQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(ClientDeviceHeader));
				var subQuery = new ZDBOnlySubQuery(typeof(ClientDeviceComponent), DmgDeviceComponentSchema.CDC_CDH_Device);
				subQuery.AddToFilter(DmgDeviceComponentSchema.CDC_BatchNumber, comparisonOperator, value);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}

			return null;
		}

		ZQuery GetIdentifierQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(ClientDeviceHeader));
				var componentSubQuery = new ZDBOnlySubQuery(typeof(ClientDeviceComponent), DmgDeviceComponentSchema.CDC_CDH_Device);
				var identifierSubQuery = new ZDBOnlySubQuery(typeof(ClientDeviceComponentIdentification), DmgDeviceComponentIdentificationSchema.CDD_CDC_Component);
				identifierSubQuery.AddToFilter(DmgDeviceComponentIdentificationSchema.CDD_Identifier, comparisonOperator, value);
				componentSubQuery.AddSubQuery(identifierSubQuery, JoinCondition.And);
				query.AddSubQuery(componentSubQuery, JoinCondition.And);
				return query;
			}

			return null;
		}
	}
}
