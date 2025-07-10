using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NctsMovementFilterStripBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region SuppressResourceStringsCheckRegion

		static public class FilterConstants
		{
			public const string Principal = "Principal";
			public const string Consignor = "Consignor";
			public const string Consignee = "Consignee";
			public const string DestinationTrader = "Destination Trader";
			public const string JobNumber = "Job Number";
			public const string LocalReferenceNumber = "Customer Reference Number";
			public const string MovementReferenceNumber = "Movement Reference Number";
			public const string MovementType = "Movement Type Includes";
			public const string DeclarationType = "Declaration Type";
			public const string DepartureDate = "Departure Date";
			public const string ArrivalDate = "Arrival Date";
			public const string DateLimit = "Date Limit";
			public const string DepartureStatus = "Departure Status";
			public const string ArrivalStatus = "Arrival Status";
			public const string DeparturePhaseStatus = "Departure Phase Status";
			public const string ArrivalPhaseStatus = "Arrival Phase Status";
			public const string IsSimplifiedDeparture = "Is Simplified Departure?";
			public const string IsSimplifiedArrival = "Is Simplified Arrival?";
			public const string IsSecurityDeclaration = "Is Security Declaration?";
			public const string ShowOnlyJobsWithMRN = "Show only jobs with an MRN?";
			public const string DispatchCountry = "Dispatch Country";
			public const string DestinationCountry = "Destination Country";
			public const string PortOfLoading = "Port of Loading";
			public const string DeparturePortOfUnloading = "Departure Port of Unloading";
			public const string ArrivalPortOfUnloading = "Arrival Port of Unloading";
			public const string CommercialReferenceNumber = "Commercial Reference Number";
			public const string MessagingStatus = "Message Status";
			public const string JobStatus = "Job Status";
			public const string Representative = "Representative";
			public const string CustomsOfficeOfDeparture = "Customs Office of Departure";
			public const string CustomsOfficeOfDestination = "Customs Office of Destination";
			public const string CustomsOfficeOfDestinationForArrival = "Customs Office of Destination For Arrival";
			public const string CustomsOfficeOfTransit = "Customs Office of Transit";
			public const string ContainerNum = "Container #";
			public const string GuaranteeReference = "Guarantee Reference";
			public const string SealNumber = "Seal #";
			public const string TariffGoodItem = "Tariff - Good Item";
			public const string DepartureGoodsLocation = "Departure Goods Location";
			public const string ArrivalGoodsLocation = "Arrival Goods Location";
			public const string SupportingDocumentType = "Supporting Document Type";
			public const string SupportingDocumentReference = "Supporting Document Reference";
			public const string AdditionalDocumentKind = "Additional Document Kind";
			public const string AdditionalDocumentType = "Additional Document Type";
			public const string AdditionalDocumentReference = "Additional Document Reference";
			public const string PreviousDocumentClass = "Previous Document Class";
			public const string PreviousDocumentType = "Previous Document Type";
			public const string PreviousDocumentRef = "Previous Document Reference";
			public const string PreviousDocumentLineNo = "Previous Document Line No";
			public const string ApplicationCode = "Application Code";
			public const string MRNReleaseDate = "MRN Release Date";
			public const string AdditionalIdentifier = "Additional Identifier";
			public const string AdditionalDeclarationType = "Additional Declaration Type";
			public const string UniqueConsignmentReference = "Unique Consignment Reference (UCR)";
			public const string AuthorisationNumber = "Authorization Number";
			public const string TransportAtDeparture = "Departure Transport ID";
			public const string TypeOfSecurity = "Security type (phase 5)";
			public const string ConsignmentBillNumber = "Consignment/Bill Number";
			public const string DeclarationBranch = "Declaration Branch";
			public const string ConsignorFullName = "Consignor (Full Name)";
			public const string ConsigneeFullName = "Consignee (Full Name)";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddOrganizationFilters(filters);
			AddNumberFilters(filters);
			AddTypeFilters(filters);
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddBooleanFilters(filters);
			AddStatusFilters(filters);
			CustomsOfficeFilter.AddFilters(filters);
			AddSealFilters(filters);
			AddTariffGoodItemFilter(filters);
			AddGoodsLocationFilter(filters);
			AddAdditionalDocumentsFilters(filters);
			AddSupportingDocumentsFilters(filters);
			AddPreviousDocumentsFilter(filters);
			AddBillingFilters(filters);
			AddBranchFilters(filters);
			return filters;
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var messagingStatus = filters.AddTextFilter(FilterConstants.MessagingStatus, v => NctsHeader.GetEffectiveMessageStatusQuery(v), GetMessagingStatusList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_MessageStatus);
			messagingStatus.Category = FilterCategories.StatusAndFlags;
			messagingStatus.MultilingualDescription = ResString.GetMultilingualString("F2747972-A84D-4F90-BE74-9DA99A9E0B36", FilterConstants.MessagingStatus);

			var jobStatus = filters.AddTextFilter(FilterConstants.JobStatus, GetJobStatusQuery, JobStatusList).WithMaxLengthOf<ModuleTextFilter>(JobHeaderSchema.JH_Status);
			jobStatus.Category = FilterCategories.StatusAndFlags;
			jobStatus.MultilingualDescription = ResString.GetMultilingualString("DBB4090B-D8FD-42D0-953A-9733A1DDC190", FilterConstants.JobStatus);
			jobStatus.ComparisonOperator_List.Clear();
			jobStatus.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			jobStatus.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
		}

		void AddBranchFilters(ModuleFilterCollection filters)
		{
			var declarationBranchFilter = filters.AddGuidFilter(FilterConstants.DeclarationBranch, ModuleIDs.GlbBranch, CusInBondHeaderSchema.BH_GB, new GlbBranchCollection(Factory));
			declarationBranchFilter.Category = FilterCategories.Organisations;
			declarationBranchFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			declarationBranchFilter.MultilingualDescription = ResString.GetMultilingualString("538A4761-867A-45DD-B67E-DAF5B1361955", FilterConstants.DeclarationBranch);
		}

		void AddOrganizationFilters(ModuleFilterCollection filters)
		{
			var principalFilter = AddGuidOrganizationFilter(FilterConstants.Principal, PrincipalSubGroup);
			principalFilter.MultilingualDescription = ResString.GetMultilingualString("7718E499-8730-450E-BD08-97ADC1828482", FilterConstants.Principal);

			var consignorFilter = AddGuidOrganizationFilter(FilterConstants.Consignor, ConsignorSubGroup);
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("67273741-375E-45AA-80EC-A881DB7780A5", FilterConstants.Consignor);

			var consigneeFilter = AddGuidOrganizationFilter(FilterConstants.Consignee, ConsigneeSubGroup);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("7880AB63-3DAF-4A57-85EB-3E3F974A3A3A", FilterConstants.Consignee);

			var destinationFilter = AddGuidOrganizationFilter(FilterConstants.DestinationTrader, DestinationTraderSubGroup);
			destinationFilter.MultilingualDescription = ResString.GetMultilingualString("E0C90134-C73D-4CD7-AE5A-B25BF4C22208", FilterConstants.DestinationTrader);

			var representativeFilter = AddGuidOrganizationFilter(FilterConstants.Representative, RepresentativeSubGroup);
			representativeFilter.MultilingualDescription = ResString.GetMultilingualString("70002626-8B60-478B-812E-2848E0F73A1C", FilterConstants.Representative);

			ModuleGuidFilter AddGuidOrganizationFilter(ZString description, BlueprintModuleFilterSubGroup subGroup)
			{
				var organizationFilter = filters.AddGuidFilter(description, ModuleIDs.Organisation, GetOrgHeaderQuery, new OrganisationsFindBoxCollection(Factory));
				organizationFilter.Category = FilterCategories.Organisations;
				organizationFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
				organizationFilter.SubGroup = subGroup;
				return organizationFilter;
			}
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var jobNumberFilter = filters.AddTextFilter(FilterConstants.JobNumber, GetJobNumberFilter);
			jobNumberFilter.MaxLength = new[] { CusInBondHeaderSchema.BH_JobReference.MaxLength, JobShipmentSchema.JS_UniqueConsignRef.MaxLength }.Min();
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.SupportsBlankComparisonOperators = false;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("8856B103-4ADC-4AA8-AB3D-28FCD25C9887", FilterConstants.JobNumber);

			var lrnFilter = filters.AddTextFilter(FilterConstants.LocalReferenceNumber, GetLocalReferenceNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusInBondHeaderSchema.BH_JobReference);
			lrnFilter.Category = FilterCategories.NumbersAndReferences;
			lrnFilter.SupportsBlankComparisonOperators = false;
			lrnFilter.MultilingualDescription = ResString.GetMultilingualString("BAE2FEAA-84E7-49EA-9E5D-387D77B96EBB", FilterConstants.LocalReferenceNumber);

			var mrnFilter = filters.AddTextFilter(FilterConstants.MovementReferenceNumber, GetMovementReferenceNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			mrnFilter.Category = FilterCategories.NumbersAndReferences;
			mrnFilter.MultilingualDescription = ResString.GetMultilingualString("081AC8CA-88B0-454F-B997-37CF3D26B65C", FilterConstants.MovementReferenceNumber);
		}

		static ZQuery GetJobNumberFilter(SQLComparisonOperator comparisonOperator, ZString jobNumber)
		{
			var nctsQuery = new ZQuery();

			var nctsReferenceQuery = new ZQuery(CusInBondHeaderSchema.BH_JobReference, comparisonOperator, jobNumber);
			nctsReferenceQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentID, DBNull.Value);
			nctsQuery.AddToFilter(nctsReferenceQuery);

			var shipmentQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			shipmentQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			var shipmentUniqueConsignRefQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentUniqueConsignRefQuery.AddToFilter(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, jobNumber);
			shipmentQuery.AddSubQuery(CusInBondHeaderSchema.BH_ParentID, shipmentUniqueConsignRefQuery, JoinCondition.And);

			nctsQuery.AddToFilter(shipmentQuery, JoinCondition.Or);

			return nctsQuery;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			if (ShouldAddWorkflowFilters)
			{
				var workflowFilterStripsHelper = new WorkflowFilterStripsHelper(typeof(NctsHeader), WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode, Factory)
				{
					ShouldAddMilestoneFilters = false,
					ShouldAddMiscFilters = false,
					ShouldAddRelatedMilestoneFilters = false
				};

				var linkSubQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);

				var arrivalWorkflowFilterStripsHelper = new WorkflowFilterStripsHelper(typeof(NctsHeader), WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor, Factory)
				{
					ShouldAddMilestoneFilters = false,
					ShouldAddRelatedMilestoneFilters = false,
					ShouldAddMiscFilters = false
				};
				var departureWorkflowFilterStripsHelper = new WorkflowFilterStripsHelper(typeof(NctsHeader), WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor, Factory)
				{
					ShouldAddMilestoneFilters = false,
					ShouldAddRelatedMilestoneFilters = false,
					ShouldAddMiscFilters = false
				};
				arrivalWorkflowFilterStripsHelper.AddRelatedParentJoiningQuery(linkSubQuery);
				arrivalWorkflowFilterStripsHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
				departureWorkflowFilterStripsHelper.AddRelatedParentJoiningQuery(linkSubQuery);
				departureWorkflowFilterStripsHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
				workflowFilterStripsHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
				helpers.Add(workflowFilterStripsHelper);
				helpers.Add(arrivalWorkflowFilterStripsHelper);
				helpers.Add(departureWorkflowFilterStripsHelper);
			}
			return helpers;
		}

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;
				if (query.FilterString.Contains(nameof(GenCustomAddOnValue)))
				{
					AddMoveHeaderCustomFieldSubQueryForNotOperator(base.Filter, query);
				}
				return query;
			}
		}

		void AddMoveHeaderCustomFieldSubQueryForNotOperator(ZQuery filter, ZQuery query)
		{
			var clonedQuery = filter.DeepClone();
			clonedQuery.Simplify();
			var zQuery = new ZQuery();
			AddMoveHeaderCustomFieldSubQueryForNotOperatorCore(((IFilterPartsProvider)clonedQuery).FilterParts, JoinCondition.Or, ref zQuery);
			query.AddToFilter(zQuery, JoinCondition.Or);
		}

		void AddMoveHeaderCustomFieldSubQueryForNotOperatorCore(IFilterPart[] parts, JoinCondition joinCondition, ref ZQuery zDBQuery)
		{
			for (var i = 0; i < parts.Length; i++)
			{
				var condition = joinCondition;
				if (i - 1 > 0)
				{
					condition = parts[i - 1] as JoinCondition;
				}

				if (parts[i] is ZSqlParameter parameter)
				{
					if (parameter.SchemaColumn != GenCustomAddOnValueSchema.XV_ParentTableCode)
					{
						zDBQuery.AddToFilter(new ZQuery(parameter.SchemaColumn, parameter.ComparisonOperator, parameter.Value));
					}
				}
				else if (parts[i] is ZQuery zQuery)
				{
					zDBQuery.AddToFilter(GetMoveHeaderCustomFieldFilterQuery(zQuery));
				}
				else if (parts[i] is ZNonPersistentDataQuery zNonPersistentDataQuery)
				{
					zDBQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, zNonPersistentDataQuery.ParameterisedQueryText), new ZSqlParameterCollection());
				}
				else if (parts[i] is IFilterPartsProvider provider)
				{
					AddMoveHeaderCustomFieldSubQueryForNotOperatorCore(provider.FilterParts, condition, ref zDBQuery);
				}
			}
		}

		ZQuery GetMoveHeaderCustomFieldFilterQuery(ZQuery filter)
		{
			var query = new ZQuery();
			var zDBQuery = new ZQuery();
			var cusInBondQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var zBool = ZBool.ParseSafe(filter.FilterString.Contains((NoResString)"NOT IN").ToString(), ZBool.True);
			AddMoveHeaderCustomFieldSubQueryForNotOperatorCore(((IFilterPartsProvider)filter).FilterParts, JoinCondition.And, ref zDBQuery);
			if (filter.FilterString.Contains(nameof(GenCustomAddOnValue)))
			{
				var zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, notIn: zBool);
				_ = zDBOnlySubQuery.AddToFilter(zDBQuery);
				_ = zDBOnlySubQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(typeof(CusInBondMoveHeader)));
				var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				moveHeaderQuery.AddSubQuery(CusInBondMoveHeaderSchema.PK, zDBOnlySubQuery, JoinCondition.And);

				cusInBondQuery.AddSubQuery(moveHeaderQuery, JoinCondition.Or);
				_ = query.AddToFilter(cusInBondQuery);
			}
			else
			{
				var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH, notIn: zBool);
				_ = moveHeaderQuery.AddToFilter(zDBQuery);
				cusInBondQuery.AddSubQuery(moveHeaderQuery, JoinCondition.Or);
				_ = query.AddToFilter(cusInBondQuery);
			}

			return query;
		}
		#endregion // Filter

		protected virtual bool ShouldAddWorkflowCustomFieldsFilters => true;

		protected virtual bool ShouldAddWorkflowFilters => true;
		void AddTypeFilters(ModuleFilterCollection filters)
		{
			var movementTypeFilter = filters.AddTextFilter(FilterConstants.MovementType, GetMovementTypeQuery, NctsMovementTypeList).WithMaxLengthOf<ModuleTextFilter>(CusInBondHeaderSchema.BH_HeaderType);
			movementTypeFilter.Category = FilterCategories.ModesAndTypes;
			movementTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			RemoveFilterComparisonOperatorsForTypeFilter(movementTypeFilter);
			movementTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			movementTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			movementTypeFilter.MultilingualDescription = ResString.GetMultilingualString("991FD9D3-A8A6-4A2F-BEC1-2E7479A9B5A5", FilterConstants.MovementType);

			var departureStatusFilter = filters.AddTextFilter(FilterConstants.DepartureStatus, GetDepartureStatusQuery, GetDepartureStatusList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_CustomsStatus);
			departureStatusFilter.Category = FilterCategories.ModesAndTypes;
			departureStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			RemoveFilterComparisonOperatorsForTypeFilter(departureStatusFilter);
			departureStatusFilter.MultilingualDescription = ResString.GetMultilingualString("4D343F76-040E-474B-A76D-073C3F4FB355", FilterConstants.DepartureStatus);

			var arrivalStatusFilter = filters.AddTextFilter(FilterConstants.ArrivalStatus, GetArrivalStatusQuery, GetArrivalStatusList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_CustomsStatus);
			arrivalStatusFilter.Category = FilterCategories.ModesAndTypes;
			arrivalStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			RemoveFilterComparisonOperatorsForTypeFilter(arrivalStatusFilter);
			arrivalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("E3ACA6AE-D09E-4654-9340-E094EFA85648", FilterConstants.ArrivalStatus);

			var departurePhaseStatusFilter = filters.AddTextFilter(FilterConstants.DeparturePhaseStatus, GetDeparturePhaseStatusQuery, GetNctsDeparturePhaseStatusList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_Phase);
			departurePhaseStatusFilter.Category = FilterCategories.ModesAndTypes;
			departurePhaseStatusFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			departurePhaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("c93b696a-f509-4c2f-9ff4-279cd323fc29", FilterConstants.DeparturePhaseStatus);

			var arrivalPhaseStatusFilter = filters.AddTextFilter(FilterConstants.ArrivalPhaseStatus, GetArrivalPhaseStatusQuery, GetNctsArrivalPhaseStatusList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_Phase);
			arrivalPhaseStatusFilter.Category = FilterCategories.ModesAndTypes;
			arrivalPhaseStatusFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			arrivalPhaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("58377a78-1588-4af5-8765-1ee35141b682", FilterConstants.ArrivalPhaseStatus);

			var declarationTypeFilter = filters.AddTextFilter(FilterConstants.DeclarationType, GetDeclarationTypeQuery, DeclarationTypeList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_InBondEntryType);
			declarationTypeFilter.Category = FilterCategories.ModesAndTypes;
			declarationTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			RemoveFilterComparisonOperatorsForTypeFilter(declarationTypeFilter);
			declarationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("9D42537E-DBD9-4E22-AE51-A75732BDD529", FilterConstants.DeclarationType);

			var applicationCodeFilter = filters.AddTextFilter(FilterConstants.ApplicationCode, GetApplicationCodeQuery, ApplicationCodeList).WithMaxLengthOf<ModuleTextFilter>(CusInBondHeaderSchema.BH_ApplicationCode);
			applicationCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
			applicationCodeFilter.Category = FilterCategories.ModesAndTypes;
			applicationCodeFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			RemoveFilterComparisonOperatorsForTypeFilter(applicationCodeFilter);
			applicationCodeFilter.MultilingualDescription = ResString.GetMultilingualString("F4E53449-D92F-4DBE-9E1F-F06D84E6DE63", FilterConstants.ApplicationCode);
			applicationCodeFilter.DefaultProperty = DefaultApplicationCode;

			var additionalDeclarationType = filters.AddTextFilter(FilterConstants.AdditionalDeclarationType, GetAdditionalDeclarationTypeQuery, AdditionalDeclarationTypeList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_AdditionalDeclarationType);
			additionalDeclarationType.Category = FilterCategories.ModesAndTypes;
			additionalDeclarationType.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			additionalDeclarationType.MultilingualDescription = ResString.GetMultilingualString("296DBBF6-8A13-41C7-AB01-06FECDFD04C2", FilterConstants.AdditionalDeclarationType);

			var typeOfSecurityFilter = filters.AddTextFilter(FilterConstants.TypeOfSecurity, GetTypeOfSecurityQuery, TypeOfSecurityList).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_TypeOfSecurity);
			typeOfSecurityFilter.Category = FilterCategories.ModesAndTypes;
			typeOfSecurityFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			typeOfSecurityFilter.MultilingualDescription = ResString.GetMultilingualString("D036EE77-7AA2-422A-9A5E-81387C3B32E4", FilterConstants.TypeOfSecurity);
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var dispatchCountryFilter = filters.AddTextFilter(FilterConstants.DispatchCountry, GetDispatchCountry);
			dispatchCountryFilter.MultilingualDescription = ResString.GetMultilingualString("587AFC17-86AF-4C17-A50F-DB451E96F2E3", FilterConstants.DispatchCountry);
			dispatchCountryFilter.MaxLength = 2;

			var destinationCountryFilter = filters.AddTextFilter(FilterConstants.DestinationCountry, GetDestinationCountry);
			destinationCountryFilter.MultilingualDescription = ResString.GetMultilingualString("FC78CD95-1889-4C51-96A8-588006F8A845", FilterConstants.DestinationCountry);

			var portOfLoadingFilter = filters.AddTextFilter(FilterConstants.PortOfLoading, GetPortOfLoading).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_RL_NKForeignDestPort);
			portOfLoadingFilter.MultilingualDescription = ResString.GetMultilingualString("54629B54-E272-4AAE-A590-65A36FA8A1E2", FilterConstants.PortOfLoading);

			var departurePortOfUnloading = filters.AddTextFilter(FilterConstants.DeparturePortOfUnloading, GetDeparturePortOfUnloading).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_PlaceOfUnloading);
			departurePortOfUnloading.MultilingualDescription = ResString.GetMultilingualString("9AE26398-268C-4AC5-BD5B-A24CD59FCCEA", FilterConstants.DeparturePortOfUnloading);

			var arrivalPortOfUnloadingFilter = filters.AddTextFilter(FilterConstants.ArrivalPortOfUnloading, GetArrivalPortOfUnloading).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_PlaceOfUnloading);
			arrivalPortOfUnloadingFilter.MultilingualDescription = ResString.GetMultilingualString("C92C0D39-5430-4B1A-B224-4376E5DD47BF", FilterConstants.ArrivalPortOfUnloading);

			var commercialReferenceNumberFilter = filters.AddTextFilter(FilterConstants.CommercialReferenceNumber, GetCommercialReferenceNumber).WithMaxLengthOf<ModuleTextFilter>(CusInBondCargoDescSchema.BY_CommercialReferenceNumber);
			commercialReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("F54B6EEF-5616-48E9-B9BD-4295B493CDF9", FilterConstants.CommercialReferenceNumber);

			var containerNumFilter = filters.AddTextFilter(FilterConstants.ContainerNum, GetContainerNumQuery).WithMaxLengthOf<ModuleTextFilter>(CusInBondContainerSchema.BC_ContainerNum);
			containerNumFilter.MultilingualDescription = ResString.GetMultilingualString("017DE11E-AED0-4E03-8EBC-4C8B73D6306B", FilterConstants.ContainerNum);

			var guaranteeReferenceNumberFilter = filters.AddTextFilter(FilterConstants.GuaranteeReference, GetGuaranteeReferenceQuery).WithMaxLengthOf<ModuleTextFilter>(CusBondDetailSchema.PW_BondNumber);
			guaranteeReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("C7A22658-DCC8-4374-8805-D169DE4CAB12", FilterConstants.GuaranteeReference);

			var additionalIdentifierFilter = filters.AddTextFilter(FilterConstants.AdditionalIdentifier, GetAdditionalIdentifierQuery).WithMaxLengthOf<ModuleTextFilter>(CusGoodsLocationSchema.CGL_AdditionalIdentifier);
			additionalIdentifierFilter.MultilingualDescription = ResString.GetMultilingualString("19C87BDE-DFB7-4FE6-AD80-C0306651E737", FilterConstants.AdditionalIdentifier);

			var uniqueConsignmentReferenceFilter = filters.AddTextFilter(FilterConstants.UniqueConsignmentReference, GetUniqueConsignmentReferenceQuery).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_UniqueConsignmentReference);
			uniqueConsignmentReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("D1D3D3A4-3A3D-4A3D-8A3D-4A3D3A3D3A3D", FilterConstants.UniqueConsignmentReference);

			var authorisationNumberFilter = filters.AddTextFilter(FilterConstants.AuthorisationNumber, GetAuthorisationNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusAuthorizationUsageSchema.AGC_Number);
			authorisationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("0757D458-DF94-4A22-AA76-B82020525685", FilterConstants.AuthorisationNumber);

			var transportAtDepartureFilter = filters.AddTextFilter(FilterConstants.TransportAtDeparture, GetTransportAtDepartureQuery).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_TransportAtDeparture);
			transportAtDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("E2B3D0E4-99BF-4C3A-A373-4E7BA0F43A78", FilterConstants.TransportAtDeparture);

			var consignmentBillNumberFilter = filters.AddTextFilter(FilterConstants.ConsignmentBillNumber, GetConsignmentBillNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_ReferenceID);
			consignmentBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("8CE6E539-BADF-4BC2-ADFA-42062A0C879B", FilterConstants.ConsignmentBillNumber);

			var consigneeFullNameFilter = filters.AddTextFilter(FilterConstants.ConsigneeFullName, GetConsigneeFullNameQuery).WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			consigneeFullNameFilter.Category = FilterCategories.Organisations;
			consigneeFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("51C175AF-BDE8-46F9-99CC-6D3CF65B7EF6", FilterConstants.ConsigneeFullName);

			var consignorFullNameFilter = filters.AddTextFilter(FilterConstants.ConsignorFullName, GetConsignorFullNameQuery).WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			consignorFullNameFilter.Category = FilterCategories.Organisations;
			consignorFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("5694B99C-3FC8-47F8-B8DC-2D32F247CB0D", FilterConstants.ConsignorFullName);
		}

		ZQuery GetDispatchCountry(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			if (comparisonOperator == SQLComparisonOperator.NotEqual ||
				comparisonOperator == SQLComparisonOperator.NotContains ||
				comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				return GetDispatchCountryForNotEqual(comparisonOperator, countryCode);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				return GetDispatchCountryForIsBlank(comparisonOperator, countryCode);
			}
			else
			{
				return GetDispatchCountryForEqual(comparisonOperator, countryCode);
			}
		}

		ZQuery GetDispatchCountryForNotEqual(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var query = new ZQuery();
			query.AddToFilter(GetDispatchCountryForNotEqual_Phase4(comparisonOperator, countryCode), JoinCondition.Or);
			query.AddToFilter(GetDispatchCountryForEqual_Phase5(comparisonOperator, countryCode, JoinCondition.Or), JoinCondition.Or);
			return query;
		}

		ZQuery GetDispatchCountryForIsBlank(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var query = new ZQuery();
			query.AddToFilter(GetDispatchCountryForIsBlank_Phase4(comparisonOperator, countryCode), JoinCondition.Or);
			query.AddToFilter(GetDispatchCountryForEqual_Phase5(comparisonOperator, ZString.Empty, JoinCondition.And), JoinCondition.Or);
			return query;
		}

		ZQuery GetDispatchCountryForEqual(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var query = new ZQuery();
			query.AddToFilter(GetDispatchCountryForEqual_Phase4(comparisonOperator, countryCode), JoinCondition.Or);
			query.AddToFilter(GetDispatchCountryForEqual_Phase5(comparisonOperator, countryCode, JoinCondition.Or), JoinCondition.Or);
			return query;
		}

		ZQuery GetDispatchCountryForNotEqual_Phase4(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var phase4Query = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS4);

			var nctsHeaderQuery = GetDispatchCountryNctsHeaderQuery(comparisonOperator, countryCode);
			nctsHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_RL_NKImportLoadPort, SQLComparisonOperator.NotEqual, ZString.Empty);

			var goodsSubQuery = GetDispatchCountryGoodsQuery(comparisonOperator, countryCode);
			goodsSubQuery.AddToFilter(CusInBondCargoDescSchema.BY_RN_NKCountryOfDispatch, SQLComparisonOperator.NotEqual, ZString.Empty);

			var moveHeaderQuery = GetDepartureMovementWithGoodsQuery(goodsSubQuery);

			nctsHeaderQuery.AddSubQuery(moveHeaderQuery, JoinCondition.Or);

			phase4Query.AddToFilter(nctsHeaderQuery);
			return phase4Query;
		}

		ZQuery GetDispatchCountryForIsBlank_Phase4(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var phase4Query = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS4);
			var nctsHeaderQuery = GetDispatchCountryNctsHeaderQuery(comparisonOperator, countryCode);
			var goodsSubQuery = GetDispatchCountryGoodsQuery(comparisonOperator, countryCode);
			var moveHeaderQuery = GetDepartureMovementWithGoodsQuery(goodsSubQuery);

			nctsHeaderQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			phase4Query.AddToFilter(nctsHeaderQuery);
			return phase4Query;
		}

		ZQuery GetDispatchCountryForEqual_Phase4(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var phase4Query = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS4);
			var nctsHeaderQuery = GetDispatchCountryNctsHeaderQuery(comparisonOperator, countryCode);
			var goodsSubQuery = GetDispatchCountryGoodsQuery(comparisonOperator, countryCode);
			var moveHeaderQuery = GetDepartureMovementWithGoodsQuery(goodsSubQuery);

			nctsHeaderQuery.AddSubQuery(moveHeaderQuery, JoinCondition.Or);

			phase4Query.AddToFilter(nctsHeaderQuery);
			return phase4Query;
		}

		ZQuery GetDispatchCountryForEqual_Phase5(SQLComparisonOperator comparisonOperator, ZString countryCode, JoinCondition joinCondition)
		{
			var moveHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			moveHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS5);
			var moveHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Departure);
			AddFilterForCountry(moveHeaderSubQuery, CusInBondMoveHeaderSchema.BM_RN_NKCountryOfDispatch, comparisonOperator, countryCode);
			moveHeaderQuery.AddSubQuery(moveHeaderSubQuery, JoinCondition.And);

			var billQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			billQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS5);
			var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
			AddFilterForCountry(billSubQuery, CusInBondBillSchema.B0_RN_NKCountryOfExport, comparisonOperator, countryCode);
			var itemSubQuery = new ZDBOnlySubQuery(typeof(CusInBondCargoDesc), CusInBondCargoDescSchema.BY_ParentID);
			AddFilterForCountry(itemSubQuery, CusInBondCargoDescSchema.BY_RN_NKCountryOfDispatch, comparisonOperator, countryCode);
			billSubQuery.AddSubQuery(itemSubQuery, joinCondition);
			billQuery.AddSubQuery(billSubQuery, JoinCondition.And);

			var phase5Query = new ZQuery();
			phase5Query.AddToFilter(moveHeaderQuery, joinCondition);
			phase5Query.AddToFilter(billQuery, joinCondition);

			return phase5Query;
		}

		ZDBOnlyQuery GetDispatchCountryNctsHeaderQuery(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var nctsHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			nctsHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_RL_NKImportLoadPort, comparisonOperator, countryCode);
			return nctsHeaderQuery;
		}

		ZDBOnlySubQuery GetDispatchCountryGoodsQuery(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var goodsSubQuery = new ZDBOnlySubQuery(typeof(CusInBondCargoDesc), CusInBondCargoDescSchema.BY_ParentID);
			goodsSubQuery.AddToFilter(CusInBondCargoDescSchema.BY_RN_NKCountryOfDispatch, comparisonOperator, countryCode);
			return goodsSubQuery;
		}

		ZDBOnlySubQuery GetDepartureMovementQuery()
		{
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Departure);
			return moveHeaderQuery;
		}

		ZDBOnlySubQuery GetDepartureMovementWithGoodsQuery(ZDBOnlySubQuery goodsSubQuery)
		{
			var moveHeaderQuery = GetDepartureMovementQuery();
			moveHeaderQuery.AddSubQuery(goodsSubQuery, JoinCondition.And);
			return moveHeaderQuery;
		}

		ZQuery GetCommercialReferenceNumber(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var moveHeaderQuery1 = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery1.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Departure);
			moveHeaderQuery1.AddToFilter(CusInBondMoveHeaderSchema.BM_AdditionalText, comparisonOperator, value);

			var moveHeaderQuery2 = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			var goodsItemQuery = new ZDBOnlySubQuery(typeof(NctsCommonCargoDesc), CusInBondCargoDescSchema.BY_ParentID);
			goodsItemQuery.AddToFilter(CusInBondCargoDescSchema.BY_ParentTableCode, CusInBondMoveHeaderSchema.Constants.Prefix);
			goodsItemQuery.AddToFilter(CusInBondCargoDescSchema.BY_CommercialReferenceNumber, comparisonOperator, value);
			moveHeaderQuery2.AddSubQuery(CusInBondMoveHeaderSchema.PK, goodsItemQuery, JoinCondition.And);

			moveHeaderQuery1.AddAsUnionQuery(moveHeaderQuery2);
			headerQuery.AddSubQuery(moveHeaderQuery1, JoinCondition.And);

			return headerQuery;
		}

		ZQuery GetDestinationCountry(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var countryCode = value.Left(2);
			if (comparisonOperator == SQLComparisonOperator.NotEqual ||
				comparisonOperator == SQLComparisonOperator.NotContains ||
				comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				return GetDestinationCountryForNotEqual(comparisonOperator, countryCode);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				return GetDestinationCountryForIsBlank(comparisonOperator, countryCode);
			}
			else
			{
				return GetDestinationCountryForEqual(comparisonOperator, countryCode);
			}
		}

		ZQuery GetDestinationCountryForNotEqual(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var nctsHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery1 = GetDepartureMovementQuery();
			moveHeaderQuery1.AddToFilter(CusInBondMoveHeaderSchema.BM_RL_NKDestinationPort, comparisonOperator, countryCode);
			moveHeaderQuery1.AddToFilter(CusInBondMoveHeaderSchema.BM_RL_NKDestinationPort, SQLComparisonOperator.NotEqual, ZString.Empty);

			var goodsSubQuery = GetDestinationCountryGoodsQuery(comparisonOperator, countryCode);
			goodsSubQuery.AddToFilter(CusInBondCargoDescSchema.BY_RN_NKCountryOfDestination, SQLComparisonOperator.NotEqual, ZString.Empty);

			var moveHeaderQuery2 = GetDepartureMovementWithGoodsQuery(goodsSubQuery);

			moveHeaderQuery1.AddAsUnionQuery(moveHeaderQuery2);
			nctsHeaderQuery.AddSubQuery(moveHeaderQuery1, JoinCondition.And);

			return nctsHeaderQuery;
		}

		ZQuery GetDestinationCountryForIsBlank(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var nctsHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var goodsSubQuery = GetDestinationCountryGoodsQuery(comparisonOperator, countryCode);
			var moveHeaderQuery = GetDepartureMovementWithGoodsQuery(goodsSubQuery);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_RL_NKDestinationPort, comparisonOperator, countryCode);

			nctsHeaderQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return nctsHeaderQuery;
		}

		ZQuery GetDestinationCountryForEqual(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var nctsHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery1 = GetDepartureMovementQuery();
			moveHeaderQuery1.AddToFilter(CusInBondMoveHeaderSchema.BM_RL_NKDestinationPort, comparisonOperator, countryCode);

			var goodsSubQuery = GetDestinationCountryGoodsQuery(comparisonOperator, countryCode);
			var moveHeaderQuery2 = GetDepartureMovementWithGoodsQuery(goodsSubQuery);

			moveHeaderQuery1.AddAsUnionQuery(moveHeaderQuery2);
			nctsHeaderQuery.AddSubQuery(moveHeaderQuery1, JoinCondition.And);

			return nctsHeaderQuery;
		}

		ZDBOnlySubQuery GetDestinationCountryGoodsQuery(SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			var goodsSubQuery = new ZDBOnlySubQuery(typeof(NctsCommonCargoDesc), CusInBondCargoDescSchema.BY_ParentID);
			goodsSubQuery.AddToFilter(CusInBondCargoDescSchema.BY_RN_NKCountryOfDestination, comparisonOperator, countryCode);
			return goodsSubQuery;
		}

		ZQuery GetPortOfLoading(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Departure);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_RL_NKForeignDestPort, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetDeparturePortOfUnloading(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Departure);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PlaceOfUnloading, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetArrivalPortOfUnloading(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Arrival);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PlaceOfUnloading, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetLocalReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();

			var phase4HeaderQuery = new ZQuery();
			var jobNumberQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			jobNumberQuery.AddToFilter(CusInBondHeaderSchema.BH_JobReference, comparisonOperator, value);

			jobNumberQuery.AddSubQuery(GenAddOnColumnQueryHelper.GetContainsValueForAnyQuery(true, NctsHeader.Schema.LocalReferenceNumber), JoinCondition.And);
			phase4HeaderQuery.AddToFilter(jobNumberQuery, JoinCondition.Or);

			var genAddOnQuery = GenAddOnColumnQueryHelper.GetQueryOnGenAddOnColumn(NctsHeader.Schema.LocalReferenceNumber, comparisonOperator, value.Left(NctsHeader.Schema.LocalReferenceNumberMaxLength));
			phase4HeaderQuery.AddToFilter(genAddOnQuery, JoinCondition.Or);

			var phase4ApplicationCodeQuery = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS4);
			phase4HeaderQuery.AddToFilter(phase4ApplicationCodeQuery, JoinCondition.And);

			var phase5HeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			phase5HeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS5);
			var phase5MoveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			phase5MoveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum, comparisonOperator, value.Left(CusInBondMoveHeader.Schema.BM_PaperlessInbondNumMaxLength));
			phase5HeaderQuery.AddSubQuery(phase5MoveHeaderQuery, JoinCondition.And);

			query.AddToFilter(phase4HeaderQuery, JoinCondition.Or);
			query.AddToFilter(phase5HeaderQuery, JoinCondition.Or);
			return query;
		}

		GenAddOnColumnQueryHelper GenAddOnColumnQueryHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnQueryHelper(typeof(CusInBondHeader))); }
		}
		GenAddOnColumnQueryHelper helper;

		ZQuery GetContainerNumQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var containerSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondContainer), CusInBondContainerSchema.BC_ParentID);
			containerSubQuery.AddToFilter(CusInBondContainerSchema.BC_ContainerNum, comparisonOperator, value);

			headerQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

			return headerQuery;
		}

		ZQuery GetGuaranteeReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_BondNumber, comparisonOperator, value);

			headerQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			return headerQuery;
		}

		ZQuery GetJobStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var cusInBondHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_IsActive, true);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_Status, comparisonOperator, value);

			cusInBondHeaderQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);

			return cusInBondHeaderQuery;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var departureDateFilter = filters.AddDateFilter(FilterConstants.DepartureDate, GetDepartureDateQuery);
			departureDateFilter.Category = FilterCategories.Dates;
			departureDateFilter.MultilingualDescription = ResString.GetMultilingualString("34FE9984-98F7-42C1-A12B-B0698EEC119E", FilterConstants.DepartureDate);

			var arrivalDateFilter = filters.AddDateFilter(FilterConstants.ArrivalDate, GetArrivalDateQuery);
			arrivalDateFilter.Category = FilterCategories.Dates;
			arrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("604289D5-E5E1-4563-908A-B8D1AE491708", FilterConstants.ArrivalDate);

			var dateLimitFilter = filters.AddDateFilter(FilterConstants.DateLimit, GetDateLimitQuery);
			dateLimitFilter.Category = FilterCategories.Dates;
			dateLimitFilter.MultilingualDescription = ResString.GetMultilingualString("00B66C8D-A340-47DF-B6AE-4FEBB680A577", FilterConstants.DateLimit);

			var releaseDateFilter = filters.AddDateFilter(FilterConstants.MRNReleaseDate, CusEntryNumSchema.CE_IssueDate);
			releaseDateFilter.Category = FilterCategories.Dates;
			releaseDateFilter.MultilingualDescription = MRNReleaseDateFilterDescription;
			releaseDateFilter.SubGroup = MRNSubGroup;
		}

		void AddBooleanFilters(ModuleFilterCollection filters)
		{
			var flagNames = new[] { Res.GetString("7ae2e83b-df41-4e91-b2f5-58aa07ac4ce9", "Ticked for yes, unticked for no") };

			var isSimplifiedDepartureFilter = filters.AddFlagsFilter(FilterConstants.IsSimplifiedDeparture, flagNames, new GetFlagsQuery[] { GetIsSimplifiedDepartureQuery });
			isSimplifiedDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("C8EEDD69-49F5-42F3-B959-35FB372B2DFE", FilterConstants.IsSimplifiedDeparture);

			var isSimplifiedArrivalFilter = filters.AddFlagsFilter(FilterConstants.IsSimplifiedArrival, flagNames, new GetFlagsQuery[] { GetIsSimplifiedArrivalQuery });
			isSimplifiedArrivalFilter.MultilingualDescription = ResString.GetMultilingualString("59710068-1F14-48BF-8111-75394E78FD56", FilterConstants.IsSimplifiedArrival);

			var isSecurityDeclarationFilter = filters.AddFlagsFilter(FilterConstants.IsSecurityDeclaration, flagNames, new GetFlagsQuery[] { GetIsSecurityDeclarationQuery });
			isSecurityDeclarationFilter.MultilingualDescription = ResString.GetMultilingualString("E45544B1-1A18-43AC-90FE-C91FC61DB5DC", FilterConstants.IsSecurityDeclaration);

			var showOnlyJobsWithMRNFilter = filters.AddFlagsFilter(FilterConstants.ShowOnlyJobsWithMRN, flagNames, new GetFlagsQuery[] { GetShowOnlyJobsWithMRNQuery });
			showOnlyJobsWithMRNFilter.MultilingualDescription = ResString.GetMultilingualString("658C71AE-2E83-4C44-9C10-8B7A70017055", FilterConstants.ShowOnlyJobsWithMRN);
			showOnlyJobsWithMRNFilter.SubGroup = MRNSubGroup;
		}

		ZQuery GetIsSimplifiedDepartureQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.Contains, NctsMovementType.Codes.Departure);
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Departure);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_GONumber, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, NctsControlResult.Codes.AuthorizedTrader);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetIsSimplifiedArrivalQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.Contains, NctsMovementType.Codes.Arrival);
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMoveHeaderType.Codes.Arrival);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_GONumber, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, NctsControlResult.Codes.AuthorizedTrader);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetIsSecurityDeclarationQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.Contains, NctsMovementType.Codes.Departure);
			query.AddToFilter(CusInBondHeaderSchema.BH_FTZMove, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, true);
			return query;
		}

		ZQuery GetShowOnlyJobsWithMRNQuery(ZBool value)
		{
			if (value)
			{
				return new ZQuery(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			return new ZQuery();
		}

		ZQuery GetMovementTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.Contains, value);
			return query;
		}

		ZQuery GetDepartureStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetStatusQuery(comparisonOperator, value, NctsMoveHeaderType.Codes.Departure);
		}

		ZQuery GetArrivalStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetStatusQuery(comparisonOperator, value, NctsMoveHeaderType.Codes.Arrival);
		}

		static ZQuery GetStatusQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString movementType)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, movementType);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_CustomsStatus, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetDeparturePhaseStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPhaseStatusQuery(comparisonOperator, value, NctsMoveHeaderType.Codes.Departure);
		}

		ZQuery GetArrivalPhaseStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPhaseStatusQuery(comparisonOperator, value, NctsMoveHeaderType.Codes.Arrival);
		}

		static ZQuery GetPhaseStatusQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString movementType)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, movementType);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_Phase, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		static void RemoveFilterComparisonOperatorsForTypeFilter(ModuleTextFilter typeFilter)
		{
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
		}

		ZQuery GetDeclarationTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_InBondEntryType, comparisonOperator, value);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMovementType.Codes.Departure);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetMovementReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				return MovementReferenceNumberIsBlankQuery();
			}

			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var nctsHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);

			var mrnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);

			nctsHeaderQuery.AddSubQuery(mrnQuery, JoinCondition.And);
			query.AddSubQuery(nctsHeaderQuery, JoinCondition.And);
			return query;
		}

		static ZQuery MovementReferenceNumberIsBlankQuery()
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var nctsHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			var mrnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, true);
			mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			nctsHeaderQuery.AddSubQuery(mrnQuery, JoinCondition.And);
			query.AddSubQuery(nctsHeaderQuery, JoinCondition.And);

			var noMrnQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK, true);
			query.AddSubQuery(noMrnQuery, JoinCondition.Or);
			return query;
		}

		ZQuery GetDepartureDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMovementType.Codes.Departure);
			AddDateTimeRange(moveHeaderQuery, comparisonOperator, JoinCondition.And, CusInBondMoveHeaderSchema.BM_EntryDate, value1, value2);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SQLComparisonOperator.Equal, NctsMovementType.Codes.Arrival);
			AddDateTimeRange(moveHeaderQuery, comparisonOperator, JoinCondition.And, CusInBondMoveHeaderSchema.BM_ArrivalDate, value1, value2);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetDateLimitQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			AddDateTimeRange(moveHeaderQuery, comparisonOperator, JoinCondition.And, CusInBondMoveHeaderSchema.BM_ExportDate, value1, value2);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetApplicationCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, SQLComparisonOperator.Contains, value);
			return query;
		}

		ZQuery GetAdditionalIdentifierQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);

			var cusGoodsLocationQuery = new ZDBOnlySubQuery(typeof(Business.CusGoodsLocation), CusGoodsLocationSchema.CGL_ParentID);
			cusGoodsLocationQuery.AddToFilter(CusGoodsLocationSchema.CGL_AdditionalIdentifier, comparisonOperator, value);

			moveHeaderQuery.AddSubQuery(cusGoodsLocationQuery, JoinCondition.And);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetUniqueConsignmentReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_UniqueConsignmentReference, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetAdditionalDeclarationTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_AdditionalDeclarationType, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetAuthorisationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			var cusAuthorizationUsageQuery = new ZDBOnlySubQuery(typeof(CusAuthorizationUsage), CusAuthorizationUsageSchema.AGC_ParentID);
			cusAuthorizationUsageQuery.AddToFilter(CusAuthorizationUsageSchema.AGC_Number, comparisonOperator, value);
			moveHeaderQuery.AddSubQuery(cusAuthorizationUsageQuery, JoinCondition.And);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetTransportAtDepartureQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_TransportAtDeparture, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetTypeOfSecurityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_TypeOfSecurity, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetConsignmentBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var billQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
			billQuery.AddToFilter(CusInBondBillSchema.B0_ReferenceID, comparisonOperator, value);
			query.AddSubQuery(billQuery, JoinCondition.And);
			return query;
		}

		#region OrgHeader Filter

		ZQuery GetConsigneeFullNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(NctsHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, NctsMovementType.Codes.Departure);

			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			if (comparisonOperator != SpecialComparisonOperator.IsBlank)
			{
				orgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
			}

			var jobDocAddressSubQuery = GetJobDocAddressFilter(orgHeaderQuery, AutoDocAddressTypes.Codes.ConsigneeAddress, query, comparisonOperator == SpecialComparisonOperator.IsBlank);
			query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetConsignorFullNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(NctsHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, NctsMovementType.Codes.Departure);

			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			if (comparisonOperator != SpecialComparisonOperator.IsBlank)
			{
				orgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
			}

			var jobDocAddressSubQuery = GetJobDocAddressFilter(orgHeaderQuery, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress, query, comparisonOperator == SpecialComparisonOperator.IsBlank);
			query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetOrgHeaderQuery(ZGuid value) => new ZQuery(OrgHeaderSchema.PK, value);

		class PrincipalFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetOrgHeaderSubQuery(filter, NctsMovementType.Codes.Departure, AutoDocAddressTypes.Codes.Principal);
			}
		}

		ModuleFilterSubGroup PrincipalSubGroup => principalSubGroup ?? (principalSubGroup = new PrincipalFilterSubGroup());
		ModuleFilterSubGroup principalSubGroup;

		class ConsignorFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetOrgHeaderSubQuery(filter, NctsMovementType.Codes.Departure, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress);
			}
		}

		ModuleFilterSubGroup ConsignorSubGroup => consignorSubGroup ?? (consignorSubGroup = new ConsignorFilterSubGroup());
		ModuleFilterSubGroup consignorSubGroup;

		class ConsigneeFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetOrgHeaderSubQuery(filter, NctsMovementType.Codes.Departure, AutoDocAddressTypes.Codes.ConsigneeAddress);
			}
		}

		ModuleFilterSubGroup ConsigneeSubGroup => consigneeSubGroup ?? (consigneeSubGroup = new ConsigneeFilterSubGroup());
		ModuleFilterSubGroup consigneeSubGroup;

		class DestinationTraderFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetOrgHeaderSubQuery(filter, NctsMovementType.Codes.Arrival, AutoDocAddressTypes.Codes.ImporterDocumentaryAddress);
			}
		}

		ModuleFilterSubGroup DestinationTraderSubGroup => destinationTraderSubGroup ?? (destinationTraderSubGroup = new DestinationTraderFilterSubGroup());
		ModuleFilterSubGroup destinationTraderSubGroup;

		class RepresentativeFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(NctsHeader));
				query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, NctsMovementType.Codes.Departure);

				var jobDocAddressSubQuery = GetJobDocAddressFilter(filter, AutoDocAddressTypes.Codes.Representative, query);

				var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				moveHeaderQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

				query.AddSubQuery(moveHeaderQuery, JoinCondition.And);

				return query;
			}
		}

		ModuleFilterSubGroup RepresentativeSubGroup => representativeTraderSubGroup ?? (representativeTraderSubGroup = new RepresentativeFilterSubGroup());
		ModuleFilterSubGroup representativeTraderSubGroup;

		class MRNFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
				var mrnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				mrnQuery.AddToFilter(filter);
				query.AddSubQuery(mrnQuery, JoinCondition.And);
				return query;
			}
		}

		protected ModuleFilterSubGroup MRNSubGroup => mrnSubGroup ?? (mrnSubGroup = new MRNFilterSubGroup());
		ModuleFilterSubGroup mrnSubGroup;

		static ZQuery GetOrgHeaderSubQuery(ZQuery orgHeaderQuery, string headerType, string addressType)
		{
			var query = new ZDBOnlyQuery(typeof(NctsHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, headerType);

			var jobDocAddressSubQuery = GetJobDocAddressFilter(orgHeaderQuery, addressType, query);
			query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			return query;
		}

		static ZDBOnlySubQuery GetJobDocAddressFilter(ZQuery orgHeaderQuery, string addressType, ZDBOnlyQuery query, bool notIn = false)
		{
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderSubQuery.AddToFilter(orgHeaderQuery);

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			jobDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return jobDocAddressSubQuery;
		}

		#endregion

		#region Seal Filters

		void AddSealFilters(ModuleFilterCollection filters)
		{
			var sealNumberFilter = filters.AddNumberFilter(FilterConstants.SealNumber, GetSealsFilterQuery);
			sealNumberFilter.MaxLength = new[] { CusInBondContainerSchema.BC_Seal1.MaxLength, CusInBondContainerSchema.BC_Seal2.MaxLength, CusCodeDataSchema.CY_Data.MaxLength }.Min();
			sealNumberFilter.Category = FilterCategories.NumbersAndReferences;
			sealNumberFilter.MultilingualDescription = ResString.GetMultilingualString("6AC618F7-09B5-4A58-9B5F-ABD246F4F075", FilterConstants.SealNumber);
		}

		ZQuery GetSealsFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var sealsContainerSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondContainer), CusInBondContainerSchema.BC_ParentID);
			sealsContainerSubQuery.AddToFilter(JoinCondition.Or, CusInBondContainerSchema.BC_Seal1, comparisonOperator, value);
			sealsContainerSubQuery.AddToFilter(JoinCondition.Or, CusInBondContainerSchema.BC_Seal2, comparisonOperator, value);

			var sealsPackagesSubQuery = new ZDBOnlySubQuery(typeof(Business.CusSeal), CusSealSchema.BK_ParentID);
			sealsPackagesSubQuery.AddToFilter(CusSealSchema.BK_SealNumber, comparisonOperator, value);

			sealsContainerSubQuery.AddSubQuery(sealsPackagesSubQuery, JoinCondition.Or);

			var incidentSubQuery = new ZDBOnlySubQuery(typeof(EnRouteIncident), CusInBondEventSchema.BN_BH);
			incidentSubQuery.AddSubQuery(sealsContainerSubQuery, JoinCondition.And);

			result.AddSubQuery(sealsContainerSubQuery, JoinCondition.Or);
			result.AddSubQuery(incidentSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Tariff Filters

		void AddTariffGoodItemFilter(ModuleFilterCollection filters)
		{
			var tariffGoodItemFilter = filters.AddNumberFilter(FilterConstants.TariffGoodItem, GetTariffGoodItemQuery).WithMaxLengthOf<ModuleNumberFilter>(CusInBondCargoDescSchema.BY_HarmonisedTariff);
			tariffGoodItemFilter.Category = FilterCategories.NumbersAndReferences;
			tariffGoodItemFilter.MultilingualDescription = ResString.GetMultilingualString("AF8B0018-9150-4D2B-A4EB-E3F45C649F93", FilterConstants.TariffGoodItem);
		}

		ZQuery GetTariffGoodItemQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var moveHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
			var goodsItemSubQuery = new ZDBOnlySubQuery(typeof(NctsCommonCargoDesc), CusInBondCargoDescSchema.BY_ParentID);
			goodsItemSubQuery.AddToFilter(CusInBondCargoDescSchema.BY_HarmonisedTariff, comparisonOperator, value);

			moveHeaderSubQuery.AddSubQuery(goodsItemSubQuery, JoinCondition.And);
			billSubQuery.AddSubQuery(goodsItemSubQuery, JoinCondition.And);

			result.AddSubQuery(moveHeaderSubQuery, JoinCondition.Or);
			result.AddSubQuery(billSubQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Goods Location Filters

		void AddGoodsLocationFilter(ModuleFilterCollection filters)
		{
			var departureGoodLocationFilter = filters.AddNumberFilter(FilterConstants.DepartureGoodsLocation, GetDepartureGoodLocationQuery).WithMaxLengthOf<ModuleNumberFilter>(CusInBondMoveHeaderSchema.BM_LocationOfGoodsCode);
			departureGoodLocationFilter.Category = FilterCategories.TextSearch;
			departureGoodLocationFilter.MultilingualDescription = ResString.GetMultilingualString("C2880F00-DAF5-4190-BA20-281C166782D3", FilterConstants.DepartureGoodsLocation);

			var arrivalGoodLocationFilter = filters.AddNumberFilter(FilterConstants.ArrivalGoodsLocation, GetArrivalGoodLocationQuery);
			arrivalGoodLocationFilter.Category = FilterCategories.TextSearch;
			arrivalGoodLocationFilter.MultilingualDescription = ResString.GetMultilingualString("A740AC21-5486-4D05-91FA-271D2FA177A9", FilterConstants.ArrivalGoodsLocation);
		}

		ZQuery GetDepartureGoodLocationQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetGoodLocationQuery(comparisonOperator, value, NctsMovementType.Codes.Departure);

		ZQuery GetArrivalGoodLocationQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetGoodLocationQuery(comparisonOperator, value, NctsMovementType.Codes.Arrival);

		protected virtual ZQuery GetGoodLocationQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString nctsMovementType)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, nctsMovementType);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_LocationOfGoodsCode, comparisonOperator, value);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Supporting Documents

		void AddSupportingDocumentsFilters(ModuleFilterCollection filters)
		{
			var typeFilter = filters.AddNkFilter(FilterConstants.SupportingDocumentType, GetSupDocTypeQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, SupportingDocumentsType);
			typeFilter.Category = FilterCategories.SupportingDocument;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("A6B59067-6081-416C-9237-511C0CABC2CF", FilterConstants.SupportingDocumentType);

			var refFilter = filters.AddTextFilter(FilterConstants.SupportingDocumentReference, GetSupDocReferenceQuery).WithMaxLengthOf<ModuleTextFilter>(CusSupportingInfoSchema.CSI_ReferenceNumber);
			refFilter.Category = FilterCategories.SupportingDocument;
			refFilter.MultilingualDescription = ResString.GetMultilingualString("FC10705F-F768-4126-81CD-B64674040C10", FilterConstants.SupportingDocumentReference);
		}

		ZQuery GetSupDocTypeQuery(ZString value)
		{
			var suppotingDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			suppotingDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_Code, value);
			return GetDocumentMainQuery(suppotingDocumentQuery);
		}

		ZQuery GetSupDocReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var suppotingDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			suppotingDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);
			return GetDocumentMainQuery(suppotingDocumentQuery);
		}

		#endregion

		#region Additional Document

		void AddAdditionalDocumentsFilters(ModuleFilterCollection filters)
		{
			var addDocKindFilter = filters.AddTextFilter(FilterConstants.AdditionalDocumentKind, GetAddDocKindQuery, AdditionalDocumentKindList).WithMaxLengthOf<ModuleTextFilter>(CusSupportingInfoSchema.CSI_SubType);
			addDocKindFilter.Category = FilterCategories.SupportingDocument;
			addDocKindFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			addDocKindFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			addDocKindFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			RemoveFilterComparisonOperatorsForTypeFilter(addDocKindFilter);
			addDocKindFilter.MultilingualDescription = ResString.GetMultilingualString("1089B357-BC1E-4E01-89C5-30E048AD5B1D", FilterConstants.AdditionalDocumentKind);

			var addDocTypeFilter = new ModuleNkFilter(FilterConstants.AdditionalDocumentType, GetAddDocTypeQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetAdditionalDocumentTypes);
			filters.AddFilter(addDocTypeFilter);
			addDocTypeFilter.Category = FilterCategories.SupportingDocument;
			addDocTypeFilter.MultilingualDescription = ResString.GetMultilingualString("629B4F4A-E4A9-4369-AA89-805357B92FEA", FilterConstants.AdditionalDocumentType);

			addDocKindFilter.PropertyInfo.ValueChanged += (s, e) => { addDocTypeFilter.Refresh(); };

			var addDocRefFilter = filters.AddTextFilter(FilterConstants.AdditionalDocumentReference, GetAddDocReferenceQuery).WithMaxLengthOf<ModuleTextFilter>(CusSupportingInfoSchema.CSI_ReferenceNumber);
			addDocRefFilter.Category = FilterCategories.SupportingDocument;
			addDocRefFilter.MultilingualDescription = ResString.GetMultilingualString("01D46E9B-43FB-481D-8A85-9A1455D04539", FilterConstants.AdditionalDocumentReference);
		}

		ZQuery GetAddDocKindQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			additionalDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_SubType, value);
			return GetDocumentMainQuery(additionalDocumentQuery);
		}

		ZQuery GetAddDocTypeQuery(ZString value)
		{
			var additionalDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			additionalDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_Code, value);
			return GetDocumentMainQuery(additionalDocumentQuery);
		}

		ZQuery GetAddDocReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			additionalDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);
			return GetDocumentMainQuery(additionalDocumentQuery);
		}

		#endregion

		#region Previous Document

		void AddPreviousDocumentsFilter(ModuleFilterCollection filters)
		{
			var classFilter = filters.AddTextFilter(FilterConstants.PreviousDocumentClass, GetPreDocClassQuery);
			classFilter.Category = FilterCategories.SupportingDocument;
			classFilter.MultilingualDescription = ResString.GetMultilingualString("43FA71F6-8444-413C-9CF0-634E63B145F2", FilterConstants.PreviousDocumentClass);

			var typeFilter = filters.AddTextFilter(FilterConstants.PreviousDocumentType, GetPreDocTypeQuery);
			typeFilter.Category = FilterCategories.SupportingDocument;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("66532307-F75B-4A5A-9614-E9671A211F73", FilterConstants.PreviousDocumentType);

			var refFilter = filters.AddTextFilter(FilterConstants.PreviousDocumentRef, GetPreDocRefQuery);
			refFilter.Category = FilterCategories.SupportingDocument;
			refFilter.MultilingualDescription = ResString.GetMultilingualString("FA9DC781-0B2E-4C60-9E73-FDF25AAF5FE1", FilterConstants.PreviousDocumentRef);

			var lineNoFilter = filters.AddTextFilterForExactComparison(FilterConstants.PreviousDocumentLineNo, GetPreDocLineNoQuery);
			lineNoFilter.Category = FilterCategories.SupportingDocument;
			lineNoFilter.MultilingualDescription = ResString.GetMultilingualString("A4517472-5105-4455-8796-9A667CCE2533", FilterConstants.PreviousDocumentLineNo);
		}

		ZQuery GetPreDocClassQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var previousDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			previousDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_SubType, value);
			return GetDocumentMainQuery(previousDocumentQuery);
		}

		ZQuery GetPreDocTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var previousDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			previousDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_Code, comparisonOperator, value);
			return GetDocumentMainQuery(previousDocumentQuery);
		}

		ZQuery GetPreDocRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var previousDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			previousDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);
			return GetDocumentMainQuery(previousDocumentQuery);
		}

		ZQuery GetPreDocLineNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var previousDocumentQuery = GetDocumentSubQuery(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			previousDocumentQuery.AddToFilter(CusSupportingInfoSchema.CSI_LineNo, comparisonOperator, ZShort.ParseSafe(value, ZShort.Zero));
			return GetDocumentMainQuery(previousDocumentQuery);
		}

		#endregion

		ZDBOnlySubQuery GetDocumentSubQuery(string documentType)
		{
			var csiQ = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
			csiQ.AddToFilter(CusSupportingInfoSchema.CSI_Type, documentType);
			return csiQ;
		}

		ZDBOnlyQuery GetDocumentMainQuery(ZDBOnlySubQuery cusSupportingInfoSubQuery)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));

			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
			var goodsItemSubQuery = new ZDBOnlySubQuery(typeof(NctsCommonCargoDesc), CusInBondCargoDescSchema.BY_ParentID);

			moveHeaderQuery.AddSubQuery(cusSupportingInfoSubQuery, JoinCondition.Or);
			goodsItemSubQuery.AddSubQuery(cusSupportingInfoSubQuery, JoinCondition.And);
			billSubQuery.AddSubQuery(goodsItemSubQuery, JoinCondition.And);
			billSubQuery.AddSubQuery(cusSupportingInfoSubQuery, JoinCondition.Or);

			query.AddSubQuery(billSubQuery, JoinCondition.And);
			query.AddSubQuery(cusSupportingInfoSubQuery, JoinCondition.Or);
			query.AddSubQuery(moveHeaderQuery, JoinCondition.Or);

			return query;
		}

		void AddFilterForCountry(ZQuery query, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString countryCode)
		{
			query.AddToFilter(schemaColumn, comparisonOperator, countryCode);
			if (comparisonOperator == SQLComparisonOperator.NotEqual ||
				comparisonOperator == SQLComparisonOperator.NotContains ||
				comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				query.AddToFilter(schemaColumn, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
		}

		#region Billing

		void AddBillingFilters(ModuleFilterCollection filters)
		{
			var accountingFilterStrip = GetAccountingFilterStrip();
			accountingFilterStrip.AddBillingFilters(filters);
		}

		IAccountingFilterStrip GetAccountingFilterStrip()
		{
			var accountingFilterStrip = (IAccountingFilterStrip)Activator.CreateInstance(CargoWise.Application.ObjectFactory.GetType<IAccountingFilterStrip>(), this);
			accountingFilterStrip.Initialize(addOrganisationFilters: false, addDateFilters: false, addAmountFilters: false, addNumbersAndReferencesFilters: false);
			return accountingFilterStrip;
		}

		#endregion

		#region Filter Lookups

		Integration.Customs.Shared.INctsSettings NctsSettings => nctsSettings ?? (nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>());
		Integration.Customs.Shared.INctsSettings nctsSettings;

		bool IsUsingPhase5 => (isUsingPhase5 ?? (isUsingPhase5 = NctsSettings.IsUsingPhase5(CurrentCountry))).Value;
		bool? isUsingPhase5;

		ZString DefaultApplicationCode => IsUsingPhase5 ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;

		protected ZString ApplicationCode => ((ModuleTextFilter)this[FilterConstants.ApplicationCode])?.Property ?? ZString.Empty;

		public CodeDescriptionPairList ApplicationCodeList => ApplicationCodeListCore;

		protected virtual CodeDescriptionPairList ApplicationCodeListCore => Factory.GetCachedValue("NonPersistentJobTypeOptionLookups.JobTypeList", () => new NonPersistentJobTypeOptionLookups(this).JobTypeList);

		public CodeDescriptionPairList NctsMovementTypeList => Factory.GetCachedValue<NctsMovementType>();
		public CodeDescriptionPairList DeclarationTypeList => DeclarationTypeListCore;

		protected virtual CodeDescriptionPairList DeclarationTypeListCore => Universal.RefCusCodeListTypes.GetCachedList(Factory, CurrentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, ZDateTime.Today);

		CodeDescriptionPairList GetDepartureStatusList() => GetNctsStatusList(NctsMoveHeaderType.Codes.Departure, ApplicationCode, () => NctsDepartureStatusListPhase5Core);

		CodeDescriptionPairList GetArrivalStatusList() => GetNctsStatusList(NctsMoveHeaderType.Codes.Arrival, ApplicationCode, () => NctsArrivalStatusListPhase5Core);

		CodeDescriptionPairList GetNctsStatusList(string movementType, string applicationCode, Func<CodeDescriptionPairList> ncts5CodeList)
		{
			switch (applicationCode)
			{
				case CusInBondApplicationCodeList.Codes.NCTS4:
					return NctsStatusListPhase4Core;
				case CusInBondApplicationCodeList.Codes.NCTS5:
					return ncts5CodeList();
				default:
					return Factory.GetCachedValue($"EU.NctsMovementFilterStripBusinessObject.NctsStatusList.{CurrentCountry}.{movementType}.All", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(ncts5CodeList());
						list.AddRange(NctsStatusListPhase4Core);
						return list;
					});
			}
		}

		protected virtual CodeDescriptionPairList NctsStatusListPhase4Core => Factory.GetCachedValue<NctsTransitStatusList>();
		protected virtual CodeDescriptionPairList NctsDepartureStatusListPhase5Core => Factory.GetCachedValue<NCTS5DepartureCustomsStatusList>();
		protected virtual CodeDescriptionPairList NctsArrivalStatusListPhase5Core => Factory.GetCachedValue<NCTS5ArrivalCustomsStatusList>();

		protected virtual CodeDescriptionPairList GetNctsArrivalPhaseStatusList()
		{
			switch (ApplicationCode)
			{
				case CusInBondApplicationCodeList.Codes.NCTS4:
					return Factory.GetCachedValue($"EU.NctsMovementFilterStripBusinessObject.NctsArrivalPhaseStatusList.{CurrentCountry}.NCTS4", () => NctsArrivalPhaseStatusListPhase4Core);
				case CusInBondApplicationCodeList.Codes.NCTS5:
					return Factory.GetCachedValue<NCTS5ArrivalPhaseList>();
				default:
					return Factory.GetCachedValue($"EU.NctsMovementFilterStripBusinessObject.NctsArrivalPhaseStatusList.{CurrentCountry}.All", () =>
					{
						var list = new NCTS5ArrivalPhaseList();
						list.AddRange(NctsArrivalPhaseStatusListPhase4Core);
						return list;
					});
			}
		}

		protected virtual CodeDescriptionPairList NctsArrivalPhaseStatusListPhase4Core => new NctsArrivalMovementHeaderPhaseStatusList();

		CodeDescriptionPairList GetNctsDeparturePhaseStatusList()
		{
			switch (ApplicationCode)
			{
				case CusInBondApplicationCodeList.Codes.NCTS4:
					return Factory.GetCachedValue($"EU.NctsMovementFilterStripBusinessObject.NctsDeparturePhaseStatusList.{CurrentCountry}.NCTS4", () => NctsDeparturePhaseStatusListPhase4Core);
				case CusInBondApplicationCodeList.Codes.NCTS5:
					return Factory.GetCachedValue($"EU.NctsMovementFilterStripBusinessObject.NctsDeparturePhaseStatusList.{CurrentCountry}.NCTS5", () => NctsDeparturePhaseStatusListPhase5Core);
				default:
					return Factory.GetCachedValue($"EU.NctsMovementFilterStripBusinessObject.NctsDeparturePhaseStatusList.{CurrentCountry}.All", () =>
					{
						var list = NctsDeparturePhaseStatusListPhase5Core;
						list.AddRange(NctsDeparturePhaseStatusListPhase4Core);
						return list;
					});
			}
		}

		protected virtual CodeDescriptionPairList NctsDeparturePhaseStatusListPhase4Core => new NctsDepartureMovementHeaderPhaseStatusList();
		protected virtual CodeDescriptionPairList NctsDeparturePhaseStatusListPhase5Core => new NCTS5DeparturePhaseList();

		public CodeDescriptionPairList GetMessagingStatusList() => ApplicationCode.ToString() switch
		{
			CusInBondApplicationCodeList.Codes.NCTS5 or "" => GetMessagingStatusListNCTS5(),
			CusInBondApplicationCodeList.Codes.NCTS4 => GetMessagingStatusListNCTS4Core(),
			_ => new CodeDescriptionPairList(),
		};

		protected virtual CodeDescriptionPairList GetMessagingStatusListNCTS4Core() => Factory.GetCachedValue<NctsMessageStatusList>();

		CodeDescriptionPairList GetMessagingStatusListNCTS5() => Factory.GetCachedValue("EU.NctsMovementFilterStripBusinessObject.MessagingStatusList.NCTS5", () =>
		{
			var list = new LogicalStatusList();
			list.AddRangeOverwriteIfExists(new NctsMessageStatusList());
			return list;
		});

		public CodeDescriptionPairList JobStatusList => JobStatusListCore;

		protected virtual CodeDescriptionPairList JobStatusListCore => Factory.GetCachedValue<JobHeaderStatusList>();

		public CodeDescriptionPairList AdditionalDeclarationTypeList => AdditionalDeclarationTypeListCore;

		protected virtual CodeDescriptionPairList AdditionalDeclarationTypeListCore => Factory.GetCachedValue<NctsTypeOfAdditionalDeclarationList>();

		public CodeDescriptionPairList TypeOfSecurityList => TypeOfSecurityListCore;

		protected virtual CodeDescriptionPairList TypeOfSecurityListCore => Factory.GetCachedValue<NctsTypeOfSecurityList>();

		ZZRefCusCodeListCombinedCollection SupportingDocumentsType => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
				, CurrentCountry
				, RefCusCodeListType.Code.SupportingDocumentOfNCTS
				, ZDateTime.Today);

		public CodeDescriptionPairList AdditionalDocumentKindList => AdditionalDocumentKindListCore;

		protected virtual CodeDescriptionPairList AdditionalDocumentKindListCore => Factory.GetCachedValue<EU.Business.AdditionalInfoSubTypeList>();

		ZZRefCusCodeListCombinedCollection GetAdditionalDocumentTypes(ModuleFilterWithList filter)
		{
			var additionalDocumentKind = ((ModuleTextFilter)this[FilterConstants.AdditionalDocumentKind])?.Property ?? ZString.Empty;
			return GetAdditionalDocumentTypesCore(additionalDocumentKind);
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetAdditionalDocumentTypesCore(ZString additionalDocumentKind)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
				, CurrentCountry
				, GetAdditionalDocumentCodeTypesListCore(additionalDocumentKind)
				, ZDateTime.Today
				, null);
		}

		protected virtual ZString[] GetAdditionalDocumentCodeTypesListCore(ZString additionalDocumentKind)
		{
			switch (additionalDocumentKind)
			{
				case EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument:
					return new ZString[] { Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N };
				case EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					return new ZString[] { Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N };
				case EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference:
					return new ZString[] { Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N };
				default:
					return new ZString[]
					{
						Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N,
						Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N,
						Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N,
					};
			}
		}

		#endregion

		NctsMovementCustomsOfficeFilter CustomsOfficeFilter => customsOfficeFilter ?? (customsOfficeFilter = new NctsMovementCustomsOfficeFilter(Factory));
		NctsMovementCustomsOfficeFilter customsOfficeFilter;

		protected virtual MultilingualString MRNReleaseDateFilterDescription => ResString.GetMultilingualString("80958A2F-071E-4C08-9DBF-46DD6B4704D6", "Release Date");

		ZString CurrentCountry => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#region IAccountingFilterStripHolder Members

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		BusinessObjectFactory IAccountingFilterStripHolder.Factory => Factory;

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>();

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			var result = new ZDBOnlyQuery(typeof(NctsHeader));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);
			return result;
		}

		#endregion
	}
}
