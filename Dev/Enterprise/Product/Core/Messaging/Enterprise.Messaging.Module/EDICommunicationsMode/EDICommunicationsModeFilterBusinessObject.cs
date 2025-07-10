using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeFilterBusinessObject : FilterStripBusinessObject
	{
		protected override bool ShouldAddUserDefinedFiltersCore => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddTextFilter("HiddenFilter", BuildOrgHeaderFilterQuery).Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			result.AddGuidFilter("EDI Client", ModuleIDs.Messaging.EDICommunicationParty, GetCommunicationPartyConfigQuery, new EDICommunicationPartyCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationsModeFilter|EDIClient", "EDI Client");

			result.AddGuidFilter("Organisation", ModuleIDs.Organisation, EDICommunicationsModeSchema.EK_ParentID, new OrgHeaderCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDICommunicationsModeFilter|Organisation", "Organization");

			result.AddFilter(new EDICommunicationsModeModuleTextFilter("Module", EDICommunicationsModeSchema.EK_Module, Lookups.ModuleList, ResString.GetMultilingualString("Messaging|EDICommunicationsModeFilter|Module", "Module")));
			result.AddFilter(new EDICommunicationsModeModuleTextFilter("Comm. Direction", EDICommunicationsModeSchema.EK_CommsDirection, Lookups.CommsDirectionList, ResString.GetMultilingualString("Messaging|EDICommunicationsModeFilter|Comm.Direction", "Comm. Direction")));
			result.AddFilter(new EDICommunicationsModeModuleTextFilter("File Format", EDICommunicationsModeSchema.EK_FileFormat, Lookups.AllFileFormatList, ResString.GetMultilingualString("Messaging|EDICommunicationsModeFilter|FileFormat", "File Format")));
			result.AddFilter(new EDICommunicationsModeModuleTextFilter("Comm. Transport", EDICommunicationsModeSchema.EK_CommunicationsTransport, Lookups.CommunicationsTransportList, ResString.GetMultilingualString("Messaging|EDICommunicationsModeFilter|CommunicationsTransport", "Comm. Transport")));

			return result;
		}

		ZQuery BuildOrgHeaderFilterQuery(SQLComparisonOperator comparisonToBeIgnored, ZString valueToBeIgnored)
		{
			var filter1 = new ZQuery(EDICommunicationsModeSchema.EK_ParentTableCode, SQLComparisonOperator.Equal, "OH");
			var filter2 = new ZQuery(EDICommunicationsModeSchema.EK_ParentID, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			return new ZQuery(filter1, filter2);
		}

		ZQuery GetCommunicationPartyConfigQuery(ZGuid value)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationsMode));
			var partyConfigSubQuery = new ZDBOnlySubQuery(typeof(EDICommunicationPartyConfig), EDICommunicationPartyConfigSchema.PK);
			partyConfigSubQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_Direction, EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			partyConfigSubQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_ECP_Party, value);

			dbQuery.AddSubQuery(EDICommunicationsModeSchema.EK_ECC_CommunicationPartyConfig, partyConfigSubQuery, JoinCondition.And);
			return dbQuery;
		}

		EDICommunicationsModeLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new EDICommunicationsModeLookups(Factory.New<EDICommunicationsMode>());
				}
				return lookups;
			}
		}
		EDICommunicationsModeLookups lookups;

		public sealed class EDICommunicationsModeModuleTextFilter : ModuleTextFilter
		{
			public EDICommunicationsModeModuleTextFilter(ZString description, SchemaStringColumn schemaStringColumn, IList list, MultilingualString multilingualString) : base(description, schemaStringColumn, list)
			{
				Category = FilterCategories.StatusAndFlags;
				MultilingualDescription = multilingualString;
			}

			public override bool HasComparisonOperator => false;
		}
	}
}
