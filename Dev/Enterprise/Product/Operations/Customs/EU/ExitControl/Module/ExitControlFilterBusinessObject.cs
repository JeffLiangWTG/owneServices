using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class FilterConstants
		{
			public const string JobNumber = "Job Number";
			public const string Carrier = "Carrier";
			public const string Exporter = "Exporter";
			public const string Branch = "Branch";
			public const string Status = "Status";
			public const string Discrepancies = "Discrepancies";
			public const string ContainerNumber = "ContainerNumber";
			public const string SealNumber = "SealNumber";
			public const string MovementReferenceNumber = "MovementReferenceNumber";
			public const string ReferenceNumberUCR = "ReferenceNumberUCR";
			public const string RegistrationNumberExt = "Registration Number (ext.)";
			public const string Location = "Location";
			public const string OfficeOfExit = "OfficeOfExit";
			public const string Broker = "Broker";
			public const string ModeOfTransport = "ModeOfTransport";
			public const string TransportID = "TransportID";
			public const string ArrivalDate = "ArrivalDate";
			public const string MessageStatus = "Message Status";
			public const string EntryConsignment = "Entry/Consignment";
		}

		public ExitControlFilterLookups Lookups => lookups ?? (lookups = GetNewLookups());
		ExitControlFilterLookups lookups;

		protected virtual ExitControlFilterLookups GetNewLookups() => new ExitControlFilterLookups(this);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddJobNumberFilter(result);
			AddCarrierOrganisationFilter(result);
			AddExporterOrganisationFilter(result);
			AddBranchFilter(result);
			var reportSpecificFieldSubGroup = new ReportSpecificFieldSubGroup();
			var consigmentFilterSubGroup = new ConsigmentFilterSubGroup();
			AddEntryConsignmentFilter(result, reportSpecificFieldSubGroup);
			AddStatusFilter(result, reportSpecificFieldSubGroup);
			AddMessageStatusFilter(result, reportSpecificFieldSubGroup);
			AddDiscrepanciesFilter(result, reportSpecificFieldSubGroup);
			AddLocationFilter(result, reportSpecificFieldSubGroup);
			AddOfficeOfExitFilter(result, reportSpecificFieldSubGroup);
			AddModeOfTransportFilter(result, reportSpecificFieldSubGroup);
			AddTransportIDFilter(result, reportSpecificFieldSubGroup);
			AddContainerNumberFilter(result);
			AddSealNumberFilter(result);
			AddMovementReferenceNumberFilter(result, consigmentFilterSubGroup);
			AddReferenceNumberUCRFilter(result);
			AddRegistrationNumberExtFilter(result, consigmentFilterSubGroup);
			AddBrokerFilter(result);
			AddArrivalDateFilter(result);
			return result;
		}

		void AddCarrierOrganisationFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var agentFilter = moduleFilterCollection.AddGuidFilter(FilterConstants.Carrier, ModuleIDs.Organisation, GetOrgAddressQuery, new OrganisationsFindBoxCollection(Factory));
			agentFilter.MultilingualDescription = ResString.GetMultilingualString("F0643292-ABEE-45BF-832A-9C8E3BD87B57", FilterConstants.Carrier);
			agentFilter.Category = FilterCategories.Organisations;
			agentFilter.SubGroup = new CarrierFilterSubGroup();
		}

		void AddExporterOrganisationFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var exporterFilter = moduleFilterCollection.AddGuidFilter(FilterConstants.Exporter, ModuleIDs.Organisation, GetOrgHeaderQuery, new OrganisationsFindBoxCollection(Factory));
			exporterFilter.MultilingualDescription = ResString.GetMultilingualString("19DB5796-DE37-4AAC-BD50-AF522D100A31", FilterConstants.Exporter);
			exporterFilter.Category = FilterCategories.Organisations;
			exporterFilter.SubGroup = new ExporterFilterSubGroup();
		}

		void AddJobNumberFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var jobNumberFilter = moduleFilterCollection.AddTextFilter(FilterConstants.JobNumber, FilterBusinessObjectHelper.GetJobNumberQuery);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("703B8EE5-9F11-4F49-8B61-2488A56E204B", FilterConstants.JobNumber);
			jobNumberFilter.MaxLength = CusExitHeaderSchema.CXH_JobReference.MaxLength;
			jobNumberFilter.SupportsBlankComparisonOperators = false;
		}

		void AddBranchFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var branchFilter = moduleFilterCollection.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, CusExitHeaderSchema.CXH_GB_Branch, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("f9f36171-5014-4e85-99f1-97ca43948af0", "Branch");
			branchFilter.Property = GlbBranch.CurrentBranch.PK;
		}

		void AddStatusFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var schemaColumn = CusExitReportSchema.CER_Status;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.Status, schemaColumn, Lookups.StatusCodesList).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.StatusAndFlags;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("BB330F52-2DFB-479B-8F51-4755CD374EC9", "Status");
		}

		void AddEntryConsignmentFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.EntryConsignment,
				(comparisonOperator, value) =>
				{
					var reportSubQuery = new ZDBOnlyQuery(typeof(CusExitReport));

					var consignmentSubQuery = new ZDBOnlySubQuery(typeof(CusExitConsignment), CusExitConsignmentSchema.PK);
					consignmentSubQuery.AddToFilter(CusExitConsignmentSchema.CXC_MovementReference, comparisonOperator, value);
					reportSubQuery.AddSubQuery(CusExitReportSchema.CER_CXC_Consignment, consignmentSubQuery, JoinCondition.Or);
					return reportSubQuery;
				}
				).WithMaxLengthOf<ModuleTextFilter>(CusExitConsignmentSchema.CXC_MovementReference);

			filter.Category = FilterCategories.NumbersAndReferences;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("036888a7-9dd9-4471-b851-48aab29b4c16", "Entry/Consignment");
		}

		void AddMessageStatusFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var schemaColumn = CusExitReportSchema.CER_MessageStatus;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.MessageStatus, schemaColumn, MessageStatusList).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.StatusAndFlags;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("c3dad19b-4df9-4657-b94e-59aec290464f", "Message Status");
		}

		protected ICodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<LogicalStatusList>();

		void AddDiscrepanciesFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var flagNames = new[] { Enterprise.Customs.EU.ExitControl.Module.Res.GetString("AE53EC80-3718-4054-B3C3-982509D5AEE9", "Discrepancies") };
			var queries = new GetFlagsQuery[] { GetDiscrepanciesFlagsQuery };

			var discrepanciesFlagsFilter = new ModuleFlagsFilter(FilterConstants.Discrepancies, flagNames, queries);
			moduleFilterCollection.AddFilter(discrepanciesFlagsFilter);
			discrepanciesFlagsFilter.Category = FilterCategories.StatusAndFlags;
			discrepanciesFlagsFilter.SubGroup = subGroup;
			discrepanciesFlagsFilter.MultilingualDescription = ResString.GetMultilingualString("3DB70276-8898-4040-B3E6-DF54728185B3", "Discrepancies");
		}

		public ZQuery GetDiscrepanciesFlagsQuery(ZBool value)
		{
			if (value)
			{
				var exitReportSubQuery = new ZDBOnlySubQuery(typeof(CusExitReport), CusExitReportSchema.CER_ClusterKey);
				exitReportSubQuery.AddToFilter(CusExitReportSchema.CER_Behavior, SQLComparisonOperator.Equal, Common.EU.ExitReportDiscrepancyTypeList.Codes.Discrepancies);
				var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
				exitHeaderQuery.AddSubQuery(CusExitHeaderSchema.CXH_ClusterKey, exitReportSubQuery, JoinCondition.And);
				return exitHeaderQuery;
			}
			return new ZQuery();
		}

		void AddLocationFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var schemaColumn = CusExitReportSchema.CER_Location;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.Location, schemaColumn).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.Locations;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("E7824759-A100-46AC-ABDF-18A9340FD05D", "Location");
		}

		void AddOfficeOfExitFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var schemaColumn = CusExitReportSchema.CER_OfficeOfExit;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.OfficeOfExit, schemaColumn).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.Locations;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("049188E2-C733-420E-BD86-B496E9891A19", "Office Of Exit");
		}

		void AddModeOfTransportFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var schemaColumn = CusExitReportSchema.CER_TransportMode;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.ModeOfTransport, schemaColumn, Lookups.TransportModeList).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.ModesAndTypes;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("907680DE-D6F0-4E20-BBEA-C1CED11445F7", "Mode of Transport");
		}

		void AddTransportIDFilter(ModuleFilterCollection moduleFilterCollection, ReportSpecificFieldSubGroup subGroup)
		{
			var schemaColumn = CusExitReportSchema.CER_TransportID;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.TransportID, schemaColumn).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.ModesAndTypes;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("00ED867D-453F-4831-A140-AFB7EA27A88D", "Transport ID");
		}

		void AddContainerNumberFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var filter = moduleFilterCollection.AddTextFilter
				(
					FilterConstants.ContainerNumber,
					(comparisonOperator, value) =>
					{
						var containerSubQuery = new ZDBOnlySubQuery(typeof(CusExitContainer), CusExitContainerSchema.CXN_ClusterKey);
						containerSubQuery.AddToFilter(CusExitContainerSchema.CXN_ContainerNumber, comparisonOperator, value);
						containerSubQuery.AddToFilter(CusExitContainerSchema.CXN_IsEquipment, SQLComparisonOperator.Equal, false);

						var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
						exitHeaderQuery.AddSubQuery(CusExitHeaderSchema.CXH_ClusterKey, containerSubQuery, JoinCondition.And);
						return exitHeaderQuery;
					}
				)
				.WithMaxLengthOf<ModuleTextFilter>(CusExitContainerSchema.CXN_ContainerNumber);

			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("29F0FD52-E40B-4296-9884-9A8DEC708BD7", "Container Number");
		}

		void AddSealNumberFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var filter = moduleFilterCollection.AddTextFilter
				(
					FilterConstants.SealNumber,
					(comparisonOperator, value) =>
					{
						var containerSubQuery = new ZDBOnlySubQuery(typeof(CusExitContainer), CusExitContainerSchema.CXN_ClusterKey, CusExitHeaderSchema.CXH_ClusterKey);
						var additionalSealsContainerSubQuery = new ZDBOnlySubQuery(typeof(CusExitSeal), CusSealSchema.BK_ParentID);
						additionalSealsContainerSubQuery.AddToFilter(CusSealSchema.BK_SealNumber, comparisonOperator, value);
						containerSubQuery.AddSubQuery(additionalSealsContainerSubQuery, JoinCondition.And);

						var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
						exitHeaderQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
						return exitHeaderQuery;
					}
				)
				.WithMaxLengthOf<ModuleTextFilter>(CusSealSchema.BK_SealNumber);

			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("AE854530-D760-4DCF-BEB4-5E1C5D4F98F7", "Seal Number");
		}

		void AddMovementReferenceNumberFilter(ModuleFilterCollection moduleFilterCollection, ConsigmentFilterSubGroup subGroup)
		{
			var schemaColumn = CusExitConsignmentSchema.CXC_MovementReference;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.MovementReferenceNumber, schemaColumn).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.Category = FilterCategories.NumbersAndReferences;
			filter.SubGroup = subGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("B6C7F292-8C12-4AB1-BBDD-C0E73694D0C1", "Movement Reference Number");
		}

		void AddReferenceNumberUCRFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var filter = moduleFilterCollection.AddTextFilter
				(
					FilterConstants.ReferenceNumberUCR,
					(comparisonOperator, value) =>
					{
						var consignmentSubQuery = new ZDBOnlySubQuery(typeof(CusExitConsignment), CusExitConsignmentSchema.CXC_ClusterKey);
						consignmentSubQuery.AddToFilter(CusExitConsignmentSchema.CXC_UniqueConsignmentReference, comparisonOperator, value);

						var consignmentItemSubQuery = new ZDBOnlySubQuery(typeof(CusExitConsignmentItem), CusExitConsignmentItemSchema.CCI_ClusterKey);
						consignmentItemSubQuery.AddToFilter(CusExitConsignmentItemSchema.CCI_UniqueConsignmentReference, comparisonOperator, value);
						consignmentSubQuery.AddSubQuery(CusExitConsignmentSchema.CXC_ClusterKey, consignmentItemSubQuery, JoinCondition.Or);

						var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
						exitHeaderQuery.AddSubQuery(CusExitHeaderSchema.CXH_ClusterKey, consignmentSubQuery, JoinCondition.And);
						return exitHeaderQuery;
					}
				).WithMaxLengthOf<ModuleTextFilter>(CusExitConsignmentSchema.CXC_UniqueConsignmentReference);

			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("4330CD06-CA8A-4DCC-B4D4-3489C19877DB", "Reference Number UCR");
		}

		void AddRegistrationNumberExtFilter(ModuleFilterCollection moduleFilterCollection, ConsigmentFilterSubGroup subGroup)
		{
			var schemaColumn = CusExitConsignmentSchema.CXC_ReferenceNumber;
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.RegistrationNumberExt, schemaColumn).WithMaxLengthOf<ModuleTextFilter>(schemaColumn);

			filter.SubGroup = subGroup;

			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("cf9a6413-3704-4310-8a2c-2ea8b28307ab", "Registration Number (ext.)");
		}

		void AddBrokerFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var brokerFilter = moduleFilterCollection.AddNkFilter
				(
					FilterConstants.Broker,
					(comparisonOperator, value) =>
					{
						var query = new ZDBOnlyQuery(typeof(CusExitHeader));
						query.AddToFilter(CusExitHeaderSchema.CXH_GS_NKCustomsAgent, comparisonOperator, value);
						return query;
					},
					ModuleIDs.GlbStaff,
					Lookups.StaffList
			);
			brokerFilter.Category = FilterCategories.Organisations;
			brokerFilter.IsPublishedOnWeb = false;
			brokerFilter.MultilingualDescription = ResString.GetMultilingualString("28300CE0-DE62-49B3-936F-4D873C3F9056", "Broker");
		}

		void AddArrivalDateFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var filter = moduleFilterCollection.AddDateFilter
				(
					FilterConstants.ArrivalDate,
					(comparisonOperator, fromDate, toDate) =>
					{
						var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
						var exitReportSubQuery = new ZDBOnlySubQuery(typeof(CusExitReport), CusExitReportSchema.CER_ClusterKey, CusExitHeaderSchema.CXH_ClusterKey);
						AddDateTimeOffsetRange(exitReportSubQuery, comparisonOperator, JoinCondition.And, CusExitReportSchema.CER_DateTime, fromDate, toDate, false, false);
						exitHeaderQuery.AddSubQuery(exitReportSubQuery, JoinCondition.And);

						return exitHeaderQuery;
					},
					true
				);

			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("AE332454-3C87-4E30-9CBD-4372EDD0C521", "Exit/Arrival Date");
		}

		ZQuery GetOrgAddressQuery(ZGuid orgHeaderPK) => new ZQuery(OrgAddressSchema.OA_OH, orgHeaderPK);

		ZQuery GetOrgHeaderQuery(ZGuid orgHeaderPK) => new ZQuery(OrgHeaderSchema.PK, orgHeaderPK);

		class ConsigmentFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var consignmentSubQuery = new ZDBOnlySubQuery(typeof(CusExitConsignment), CusExitConsignmentSchema.CXC_ClusterKey);
				consignmentSubQuery.AddToFilter(filter);
				var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
				exitHeaderQuery.AddSubQuery(CusExitHeaderSchema.CXH_ClusterKey, consignmentSubQuery, JoinCondition.And);
				return exitHeaderQuery;
			}
		}

		class ReportSpecificFieldSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var exitReportSubQuery = new ZDBOnlySubQuery(typeof(CusExitReport), CusExitReportSchema.CER_ClusterKey);
				exitReportSubQuery.AddToFilter(filter);
				var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
				exitHeaderQuery.AddSubQuery(CusExitHeaderSchema.CXH_ClusterKey, exitReportSubQuery, JoinCondition.And);
				return exitHeaderQuery;
			}
		}

		class CarrierFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter) => GetOrganisationSubQuery(typeof(OrgAddress), filter, CusExitHeaderSchema.CXH_OA_Carrier);
		}

		class ExporterFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter) => GetOrganisationSubQuery(typeof(OrgHeader), filter, CusExitHeaderSchema.CXH_OH_Exporter);
		}

		static ZDBOnlyQuery GetOrganisationSubQuery(Type type, ZQuery organisationQuery, SchemaGuidColumn exitHeaderOrganisationColumn)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(type, exitHeaderOrganisationColumn);
			orgHeaderSubQuery.AddToFilter(organisationQuery);

			var result = new ZDBOnlyQuery(typeof(CusExitHeader));
			result.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			return result;
		}
	}
}
