using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationPartyFilterBusinessObject : FilterStripBusinessObject
	{
		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override bool IsActiveStatusFilterAlwaysApplied() => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			ModuleFilter connectionTypeFilter = result.AddTextFilter("Connection Type", FilterByConnectionType, ConnectionTypes);
			connectionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|ConnectionType", "Connection Type");

			ModuleFilter nameFilter = result.AddTextFilter("Name", EDICommunicationPartySchema.ECP_Name);
			nameFilter.Visibility = FilterVisibility.AlwaysVisible;
			nameFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|Name", "Name");
			nameFilter.MaxLength = EDICommunicationPartySchema.ECP_Name.MaxLength;

			ModuleFilter summaryFilter = result.AddTextFilter("Description", EDICommunicationPartySchema.ECP_Summary);
			summaryFilter.Visibility = FilterVisibility.AlwaysVisible;
			summaryFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|Description", "Description");
			summaryFilter.MaxLength = EDICommunicationPartySchema.ECP_Summary.MaxLength;

			ModuleFilter branchFilter = result.AddGuidFilter("Inbound Branch", ModuleIDs.GlbBranch, FilterByBranch, Branches, EDICommunicationPartyConfigSchema.ECC_GB_Branch);
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|InboundBranch", "Inbound Branch");

			ModuleFilter departmentFilter = result.AddGuidFilter("Inbound Department", ModuleIDs.GlbDepartment, FilterByDepartment, Departments, EDICommunicationPartyConfigSchema.ECC_GE_Department);
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|InboundDepartment", "Inbound Department");

			ModuleFilter inboundModeFilter = result.AddTextFilter("Inbound Mode", FilterByInboundMode, AuthModes);
			inboundModeFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|InboundMode", "Inbound Mode");
			ModuleFilter outboundModeFilter = result.AddTextFilter("Outbound Mode", FilterByOutboundMode, AuthModes);
			outboundModeFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|OutboundMode", "Outbound Mode");

			ModuleFilter inboundStatusFilter = result.AddTextFilter("Active Status: Inbound", FilterByInboundActive, CancelledStatusList);
			inboundStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|InboundStatus", "Active Status: Inbound");
			ModuleFilter outboundStatusFilter = result.AddTextFilter("Active Status: Outbound", FilterByOutboundActive, CancelledStatusList);
			outboundStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationPartyFilter|OutboundStatus", "Active Status: Outbound");

			return result;
		}

		CodeDescriptionPairList ConnectionTypes
		{
			get
			{
				if (connectionTypes == null)
				{
					connectionTypes = new EDIClientApplicationDescriptorList();
				}
				return connectionTypes;
			}
		}

		CodeDescriptionPairList connectionTypes;

		CodeDescriptionPairList AuthModes
		{
			get
			{
				if (fAuthModes == null)
				{
					fAuthModes = new EDICommunicationAuthModesList();
				}
				return fAuthModes;
			}
		}

		CodeDescriptionPairList fAuthModes;

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}
		GlbBranchCollection fBranches;

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}

				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		ZQuery FilterByConnectionType(ZString value)
		{
			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationParty));
			dbQuery.AddToFilter(EDICommunicationPartySchema.ECP_ApplicationCode, value);
			return dbQuery;
		}

		ZQuery FilterByBranch(SQLComparisonOperator comparisonOperator, object branch)
		{
			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationParty));
			ZDBOnlySubQuery subQuery = GetCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			subQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_GB_Branch, comparisonOperator, (ZGuid)branch);
			dbQuery.AddSubQuery(EDICommunicationPartySchema.PK, subQuery, JoinCondition.And);
			return dbQuery;
		}

		ZQuery FilterByDepartment(SQLComparisonOperator comparisonOperator, object department)
		{
			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationParty));
			ZDBOnlySubQuery subQuery = GetCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			subQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_GE_Department, comparisonOperator, (ZGuid)department);
			dbQuery.AddSubQuery(EDICommunicationPartySchema.PK, subQuery, JoinCondition.And);
			return dbQuery;
		}

		ZDBOnlySubQuery GetCommunicationPartyConfig(ZString direction)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(EDICommunicationPartyConfig),
				EDICommunicationPartyConfigSchema.ECC_ECP_Party);
			subQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_Direction, direction);
			return subQuery;
		}

		ZQuery FilterByInboundMode(ZString value)
		{
			return GetCommunicationPartyConfigWithAuth(value, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
		}

		ZQuery FilterByOutboundMode(ZString value)
		{
			return GetCommunicationPartyConfigWithAuth(value, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
		}

		ZQuery GetCommunicationPartyConfigWithAuth(ZString authType, ZString direction)
		{
			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationParty));
			var baseQuery = GetCommunicationPartyConfig(direction);

			if (authType == (ZString)EDICommunicationAuthModesList.Descriptions.NoAuthentication)
			{
				// Check for empty
				baseQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, SQLComparisonOperator.Equal, null);
			}
			else
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth),
					EDICommunicationAuthSchema.PK);
				subQuery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, authType);
				baseQuery.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subQuery, JoinCondition.And);
			}
			dbQuery.AddSubQuery(EDICommunicationPartySchema.PK, baseQuery, JoinCondition.And);
			return dbQuery;
		}

		ZQuery FilterByInboundActive(ZString value)
		{
			return GetCommunicationPartyConfigByActive(value, EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
		}

		ZQuery FilterByOutboundActive(ZString value)
		{
			return GetCommunicationPartyConfigByActive(value, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
		}

		ZQuery GetCommunicationPartyConfigByActive(ZString active, ZString direction)
		{
			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationParty));
			ZDBOnlySubQuery subQuery = GetCommunicationPartyConfig(direction);

			subQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_IsActive, SQLComparisonOperator.Equal, active == (ZString)StatusActive ? 1 : 0);

			dbQuery.AddSubQuery(EDICommunicationPartySchema.PK, subQuery, JoinCondition.And);
			return dbQuery;
		}
	}
}
