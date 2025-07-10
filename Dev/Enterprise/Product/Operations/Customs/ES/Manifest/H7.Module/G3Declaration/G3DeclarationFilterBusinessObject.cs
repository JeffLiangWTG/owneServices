using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class G3DeclarationFilterBusinessObject : FilterStripBusinessObject
	{
		static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string JobNumber = "Job #";
			public const string InterchangeNumber = "G3 Interchange Number";
			public const string MessageNumber = "G3 Message Number";
			public const string DSDTFlightNo = "DSDT/Flight No.";

			public const string Declarant = "Declarant";
			public const string Presenter = "Presenter";

			public const string MessageStatus = "G3 Message Status";

			public const string Direction = "G3 Direction";
			public const string MessageType = "G3 Message Type";

			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var moduleFilterCollection =  new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(moduleFilterCollection);
			AddOrganizationFilter(moduleFilterCollection);
			AddStatusAndFlagFilters(moduleFilterCollection);
			AddModesAndTypesFilters(moduleFilterCollection);

			return moduleFilterCollection;
		}

		void AddNumbersAndReferencesFilters(ModuleFilterCollection moduleFilterCollection)
		{
			var jobNumberFilter = moduleFilterCollection.AddTextFilter(Descriptions.JobNumber, AsycudaManifestHeaderSchema.AMA_JobReference);
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("2afefe8f-d894-4441-babc-e8f231a1e698", Descriptions.JobNumber);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.SubGroup = HeaderSubGroup;

			var interchangeNumberFilter = moduleFilterCollection.AddTextFilter(Descriptions.InterchangeNumber, EDIInterchangeSchema.EI_InterchangeNum);
			interchangeNumberFilter.MultilingualDescription = ResString.GetMultilingualString("0798fd72-138c-4567-bce6-e06a530b16c6", Descriptions.InterchangeNumber);
			interchangeNumberFilter.Category = FilterCategories.NumbersAndReferences;
			interchangeNumberFilter.SubGroup = InterchangeSubGroup;

			var messageNumberFilter = moduleFilterCollection.AddTextFilter(Descriptions.MessageNumber, EDIMessageSchema.EM_MessageNum);
			messageNumberFilter.MultilingualDescription = ResString.GetMultilingualString("c0f1a2b4-3d7e-4f5b-8c9d-6a0e2f3b1c5d", Descriptions.MessageNumber);
			messageNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var flightNoFilter = moduleFilterCollection.AddTextFilter(Descriptions.DSDTFlightNo, AsycudaManifestHeaderSchema.AMA_MasterInformation);
			flightNoFilter.MultilingualDescription = ResString.GetMultilingualString("c461ec16-aa39-4204-9fbf-aaf42146cb87", Descriptions.DSDTFlightNo);
			flightNoFilter.Category = FilterCategories.NumbersAndReferences;
			flightNoFilter.SubGroup = HeaderSubGroup;
		}

		void AddOrganizationFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var declarantFilter = moduleFilterCollection.AddGuidFilter(Descriptions.Declarant, ModuleIDs.Organisation, (op, value) => GetOrgQuery(value, AsycudaManifestHeaderSchema.AMA_OA_Declarant, op), OrganizationList);
			declarantFilter.MultilingualDescription = ResString.GetMultilingualString("f1a0b2c3-4d5e-6f7g-8h9i-j0k1l2m3n4o5", Descriptions.Declarant);
			declarantFilter.Category = FilterCategories.Organisations;
			declarantFilter.SubGroup = HeaderSubGroup;

			var presenterFilter = moduleFilterCollection.AddGuidFilter(Descriptions.Presenter, ModuleIDs.Organisation, (op, value) => GetOrgQuery(value, AsycudaManifestHeaderSchema.AMA_OA_Presenter, op), OrganizationList);
			presenterFilter.MultilingualDescription = ResString.GetMultilingualString("p6q7r8s9-t0u1-v2w3-x4y5-z6a7b8c9d0e1", Descriptions.Presenter);
			presenterFilter.Category = FilterCategories.Organisations;
			presenterFilter.SubGroup = HeaderSubGroup;
		}

		void AddStatusAndFlagFilters(ModuleFilterCollection moduleFilterCollection)
		{
			var messageStatusFilter = moduleFilterCollection.AddTextFilter(Descriptions.MessageStatus, EDIMessageSchema.EM_Status, MessageStatusList);
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("e2f3g4h5-i6j7-k8l9-m0n1-o2p3q4r5s6t7", Descriptions.MessageStatus);
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection moduleFilterCollection)
		{
			var directionFilter = moduleFilterCollection.AddTextFilter(Descriptions.Direction, EDIMessageSchema.EM_ReceiveTransmit, MessageDirectionList);
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("u8v9w0x1-y2z3-a4b5-c6d7-e8f9g0h1i2j3", Descriptions.Direction);
			directionFilter.Category = FilterCategories.ModesAndTypes;

			var messageTypeFilter = moduleFilterCollection.AddTextFilter(Descriptions.MessageType, EDIMessageSchema.EM_MessageType, MessageTypeList);
			messageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("k4l5m6n7-o8p9-q0r1-s2t3-u4v5w6x7y8z9", Descriptions.MessageType);
			messageTypeFilter.Category = FilterCategories.ModesAndTypes;
		}

		ZQuery GetOrgQuery(object orgPK, SchemaGuidColumn column, SQLComparisonOperator comparisonOperator)
		{
			var headerSubQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			if (ZGuid.TryParse(orgPK, out var orgGuid))
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, orgGuid);
				headerSubQuery.AddSubQuery(column, OrgAddressSchema.PK, orgAddressQuery, JoinCondition.And);
			}
			else
			{
				headerSubQuery.AddToFilter(column, comparisonOperator, DBNull.Value);
			}

			return headerSubQuery;
		}

		OrgHeaderCollection OrganizationList => organizationList ??= new OrgHeaderCollection(Factory);
		OrgHeaderCollection organizationList;

		CodeDescriptionPairList MessageStatusList => messageStatusList ??= Factory.GetCachedValue<EDIMessageStatusList>();
		CodeDescriptionPairList messageStatusList;

		CodeDescriptionPairList MessageDirectionList => messageDirectionList ??= GetMessageDirectionList();
		CodeDescriptionPairList messageDirectionList;

		CodeDescriptionPairList GetMessageDirectionList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ReceiveTransmitList.Codes.Transmit, ReceiveTransmitList.Descriptions.Transmit);
			result.AddPair(ReceiveTransmitList.Codes.Receive, ReceiveTransmitList.Descriptions.Receive);

			return result;
		}

		CodeDescriptionPairList MessageTypeList => messageTypeList ??= GetMessageTypeList();
		CodeDescriptionPairList messageTypeList;

		CodeDescriptionPairList GetMessageTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(G3MessageTypes.Codes.G3Declaration, G3MessageTypes.Descriptions.G3Declaration);
			result.AddPair(G3MessageTypes.Codes.G3Revoke, G3MessageTypes.Descriptions.G3Revoke);

			return result;
		}

		#region Interchange Sub Group

		public ModuleFilterSubGroup InterchangeSubGroup => interchangeSubGroup ??= new EDIInterchangeSubGroup();
		ModuleFilterSubGroup interchangeSubGroup;

		class EDIInterchangeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var ediMessageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

				var ediInterchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
				ediInterchangeQuery.AddToFilter(filter);

				ediMessageQuery.AddSubQuery(EDIMessageSchema.EM_EI, ediInterchangeQuery, JoinCondition.And);

				return ediMessageQuery;
			}
		}

		#endregion

		#region Header Sub Group

		public ModuleFilterSubGroup HeaderSubGroup => headerSubGroup ??= new AsycudaManifestHeaderSubGroup();
		ModuleFilterSubGroup headerSubGroup;

		class AsycudaManifestHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var ediMessageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

				var headerSubQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
				headerSubQuery.AddToFilter(filter);

				ediMessageQuery.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, headerSubQuery, JoinCondition.And);

				return ediMessageQuery;
			}
		}

		#endregion
	}
}
