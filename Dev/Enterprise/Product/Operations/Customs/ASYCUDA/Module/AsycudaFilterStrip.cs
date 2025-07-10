using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaFilterStrip : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string AgentType = "Agent Type";
			public const string MasterBillNumber = "Master Bill Number";
			public const string Carrier = "Carrier";
			public const string MessageStatus = "Message Status";
			public const string ArrivalStatus = "Arrival Status";
			public const string CustomsStatus = "Customs Status";
			public const string JobReference = "Job Reference";
			public const string VesselName = "Vessel Name";
			public const string VoyageNumber = "Voyage Number / Flight No";
			public const string EstimatedDateofDeparture = "Estimated Date of Departure";
			public const string EstimatedTimeArrival = "Estimated Time Arrival";
			public const string PortOfLoadingDischarge = "Port of Loading / Discharge";
			public const string TransportMode = "Transport Mode";
			public const string ManifestApplicationType = "Manifest Application Type";
			public const string ManifestType = "Manifest Type";
			public const string SpecificCircumstanceType = "Type";
			public const string Country = "Country/Region";
			public const string ContainerMode = "Container Mode";
			public const string ContainerNumber = "Container Number";
			public const string VehicleReg = "Vehicle Reg";
			public const string IssueDate = "Issue Date";
			public const string Nature = "Nature";
			public const string RegistrationNumber = "Registration Number";
			public const string RegistrationDate = "Registration Date";
			public const string CustomsOffice = "Customs Office";
			public const string FirstArrivalPort = "First Arrival Port";
			public const string ShippingAgentName = "Shipping Agent Name";
			public const string ShippingAgentAddress = "Shipping Agent Address";
			public const string CarrierCode = "Carrier Code";
			public const string LocalReferenceNumber = "Local Reference Number";
			public const string RegisteredUser = "Registered User";
			public const string VesselImoNumber = "Vessel IMO Number";
			public const string ConsignorCode = "Consignor Code";
			public const string ConsignorName = "Consignor Name";
			public const string ConsigneeCode = "Consignee Code";
			public const string ConsigneeName = "Consignee Name";
			public const string BillNumber = "Bill Number";
			public const string UniqueConsignmentNumber = "Unique Consignment Number";
			public const string OriginDestinationBills = "Origin / Destination (Bills)";
		}

		public AsycudaFilterStrip(bool enableCountryFilter = true)
		{
			shouldAddCountryFilter = enableCountryFilter;
		}

		protected readonly bool shouldAddCountryFilter;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new AsycudaFilterStrip(shouldAddCountryFilter);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			if (shouldAddCountryFilter)
			{
				var countryFilter = result.AddNkFilter(FilterConstants.Country, AsycudaManifestHeaderSchema.AMA_RN_NKCountry, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
				countryFilter.Category = FilterCategories.NumbersAndReferences;
				countryFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|CountryFilter", FilterConstants.Country);
			}

			var carrierFilter = result.AddGuidFilter(FilterConstants.Carrier, ModuleIDs.Organisation, GetCarrierQuery, OrganizationList);
			carrierFilter.Category = FilterCategories.NumbersAndReferences;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|CarrierFilter", "Carrier");

			var messageStatusFilter = new CountryRelatedFilter(
				FilterConstants.MessageStatus,
				GetMessageStatusQuery,
				Factory,
				ZArchitecture.FieldType.TextDropEdit,
				AsycudaManifestHeaderSchema.AMA_MessageStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.MessageStatusGetter);
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|MessageStatusFilter", FilterConstants.MessageStatus);
			result.AddFilter(messageStatusFilter);

			var customsStatusFilter = new CountryRelatedFilter(
				FilterConstants.CustomsStatus,
				GetCustomsStatusQuery,
				Factory,
				ZArchitecture.FieldType.TextDropEdit,
				AsycudaManifestHeader.Schema.AMA_CustomsStatusMaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsStatusGetter);
			customsStatusFilter.Category = FilterCategories.StatusAndFlags;
			customsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|CustomsStatusFilter", FilterConstants.CustomsStatus);
			result.AddFilter(customsStatusFilter);

			var arrivalStatusFilter = new CountryRelatedFilter(
				FilterConstants.ArrivalStatus,
				GetArrivalStatusQuery,
				Factory,
				ZArchitecture.FieldType.TextDropEdit,
				AsycudaArrivalLine.Schema.ATL_MessageStatusMaxLength,
				CountryRelatedFilterHelper.ListGetters.ArrivalStatusGetter);
			arrivalStatusFilter.Category = FilterCategories.StatusAndFlags;
			arrivalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ArrivalStatusFilter", FilterConstants.ArrivalStatus);
			result.AddFilter(arrivalStatusFilter);

			var jobReferenceFilter = result.AddTextFilter(FilterConstants.JobReference, AsycudaManifestHeaderSchema.AMA_JobReference);
			jobReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|JobReferenceFilter", FilterConstants.JobReference);

			var vesselNameFilter = result.AddTextFilter(FilterConstants.VesselName, AsycudaManifestHeaderSchema.AMA_VesselName);
			vesselNameFilter.Category = FilterCategories.NumbersAndReferences;
			vesselNameFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|VesselNameFilter", FilterConstants.VesselName);

			var voyageNumber = result.AddTextFilter(FilterConstants.VoyageNumber, AsycudaManifestHeaderSchema.AMA_Voyage);
			voyageNumber.Category = FilterCategories.NumbersAndReferences;
			voyageNumber.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|VoyageNumberFilter", FilterConstants.VoyageNumber);
			voyageNumber.UseMultiSearch = true;

			var vesselImoNumberFilter = result.AddTextFilter(FilterConstants.VesselImoNumber, AsycudaManifestHeaderSchema.AMA_LloydsNumber)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_LloydsNumber);
			vesselImoNumberFilter.Category = FilterCategories.NumbersAndReferences;
			vesselImoNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|VesselImoNumberFilter", FilterConstants.VesselImoNumber);

			var estimatedDateofDepartureFilter = result.AddDateFilter(FilterConstants.EstimatedDateofDeparture, GetETDQuery);
			estimatedDateofDepartureFilter.Category = FilterCategories.Dates;
			estimatedDateofDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|EstimatedDateofDepartureFilter", FilterConstants.EstimatedDateofDeparture);

			var estimatedTimeArrivalFilter = result.AddDateFilter(FilterConstants.EstimatedTimeArrival, GetETAQuery);
			estimatedTimeArrivalFilter.Category = FilterCategories.Dates;
			estimatedTimeArrivalFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|EstimatedTimeArrivalFilter", FilterConstants.EstimatedTimeArrival);

			var locationFilter = result.AddLocationFilter(FilterConstants.PortOfLoadingDischarge, GetLoadDischargePortFilter, Location_List, Location_List);
			locationFilter.SetItemDescriptions(Res.GetData("AsycudaFilterStrip|Load", "Loading"), Res.GetData("AsycudaFilterStrip|Discharge", "Discharge"));
			locationFilter.Category = FilterCategories.Locations;
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|PortOfLoadingDischarge", FilterConstants.PortOfLoadingDischarge);

			var transportModeFilter = result.AddTextFilter(FilterConstants.TransportMode, AsycudaManifestHeaderSchema.AMA_TransportMode, TransportModeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|TransportModeFilter", FilterConstants.TransportMode);

			var agentTypeFilter = result.AddTextFilter(FilterConstants.AgentType, AsycudaManifestHeaderSchema.AMA_AgentType)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_AgentType);
			agentTypeFilter.Category = FilterCategories.NumbersAndReferences;
			agentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|AgentTypeFilter", FilterConstants.AgentType);

			var masterBillNumberFilter = result.AddTextFilter(FilterConstants.MasterBillNumber, GetMasterBillQuery)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			masterBillNumberFilter.Category = FilterCategories.NumbersAndReferences;
			masterBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|MasterBillNumberFilter", FilterConstants.MasterBillNumber);
			masterBillNumberFilter.UseMultiSearch = true;

			var manifestApplicationTypeFilter = result.AddTextFilter(FilterConstants.ManifestApplicationType, AsycudaManifestHeaderSchema.AMA_ApplicationCode, ManifestApplicationTypeList);
			manifestApplicationTypeFilter.Category = FilterCategories.ModesAndTypes;
			manifestApplicationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ManifestApplicationTypeFilter", FilterConstants.ManifestApplicationType);

			var containerModeFilter = new HideComparisonOperatorModuleTextFilter(FilterConstants.ContainerMode, AsycudaManifestHeaderSchema.AMA_ContainerMode, AsycudaManifestHeaderLookups.ContainerModeList)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_ContainerMode);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ContainerModeFilter", FilterConstants.ContainerMode);
			result.AddFilter(containerModeFilter);

			var containerNumberFilter = result.AddTextFilter(FilterConstants.ContainerNumber, AsycudaContainerSchema.ACN_ContainerNumber)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaContainerSchema.ACN_ContainerNumber);
			containerNumberFilter.SubGroup = new ContainerSubGroup();
			containerNumberFilter.Category = FilterCategories.NumbersAndReferences;
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ContainerNumber", FilterConstants.ContainerNumber);

			var vehicleRegFilter = result.AddTextFilter(FilterConstants.VehicleReg, AsycudaManifestHeaderSchema.AMA_VehicleRegistration)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_VehicleRegistration);
			vehicleRegFilter.Category = FilterCategories.ModesAndTypes;
			vehicleRegFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|VehicleRegFilter", FilterConstants.VehicleReg);

			var manifestTypeFilter = new DataGroupingRelatedFilter(
				FilterConstants.ManifestType,
				GetManifestTypeQuery,
				Factory,
				ZArchitecture.FieldType.TextDropEdit,
				AsycudaManifestHeaderSchema.AMA_ManifestType.MaxLength,
				Res.GetData("AsycudaFilterStrip|ManifestTypeFilter", FilterConstants.ManifestType),
				ApplicationBusinessProvider.GetManifestTypeListByCountry);
			manifestTypeFilter.UseProperty2ListGetterWhenProperty1IsEmpty = true;
			manifestTypeFilter.Category = FilterCategories.ModesAndTypes;
			manifestTypeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ManifestTypeFilter", FilterConstants.ManifestType);
			result.AddFilter(manifestTypeFilter);

			var specificCircumstanceTypeFilter = result.AddTextFilter(FilterConstants.SpecificCircumstanceType, GetSpecificCircumstanceTypeQuery)
				.WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
			specificCircumstanceTypeFilter.Category = FilterCategories.ModesAndTypes;
			specificCircumstanceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|SpecificCircumstanceTypeFilter", FilterConstants.SpecificCircumstanceType);

			var natureFilter = new HideComparisonOperatorModuleTextFilter(FilterConstants.Nature, AsycudaManifestHeaderSchema.AMA_Nature, AsycudaManifestHeaderLookups.GetNatureList(Factory))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_Nature);
			natureFilter.Category = FilterCategories.ModesAndTypes;
			natureFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|NatureFilter", FilterConstants.Nature);
			result.AddFilter(natureFilter);

			var customsOfficeFilter = new CountryRelatedFilter(
				FilterConstants.CustomsOffice,
				GetCustomsOfficeQuery,
				Factory,
				ZArchitecture.FieldType.TextDropEdit,
				AsycudaManifestHeaderSchema.AMA_CustomsOffice.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsOfficeGetter);
			customsOfficeFilter.Category = FilterCategories.Locations;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|CustomsOfficeFilter", FilterConstants.CustomsOffice);
			result.AddFilter(customsOfficeFilter);

			var firstArrivalPortFilter = result.AddNkFilter(FilterConstants.FirstArrivalPort, AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival);
			firstArrivalPortFilter.Category = FilterCategories.Locations;
			firstArrivalPortFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|FirstArrivalPortFilter", FilterConstants.FirstArrivalPort);

			var carrierModeFilter = result.AddTextFilter(FilterConstants.CarrierCode, AsycudaManifestHeaderSchema.AMA_CarrierCode)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_CarrierCode);
			carrierModeFilter.Category = FilterCategories.NumbersAndReferences;
			carrierModeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|CarrierCodeFilter", FilterConstants.CarrierCode);

			var shippingAgentAddressFilter = result.AddGuidFilter(FilterConstants.ShippingAgentAddress, ModuleIDs.OrgAddresses, GetShippingAgentAddressQuery, OrgAddressList)
				.WithMaxLengthOf<ModuleGuidFilter>(AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent);
			shippingAgentAddressFilter.Category = FilterCategories.Locations;
			shippingAgentAddressFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ShippingAgentAddressFilter", FilterConstants.ShippingAgentAddress);

			var shippingAgentNameFilter = result.AddTextFilter(FilterConstants.ShippingAgentName, GetShippingAgentNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			shippingAgentNameFilter.Category = FilterCategories.NumbersAndReferences;
			shippingAgentNameFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ShippingAgentNameFilter", FilterConstants.ShippingAgentName);

			var registrationNumberFilter = result.AddTextFilter(FilterConstants.RegistrationNumber, GetRegistrationNumberQuery)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|RegistrationNumberFilter", FilterConstants.RegistrationNumber);

			var registrationDateFilter = result.AddDateFilter(FilterConstants.RegistrationDate, GetRegistrationDateQuery);
			registrationDateFilter.Category = FilterCategories.Dates;
			registrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|RegistrationDateFilter", FilterConstants.RegistrationDate);

			var issueDateFilter = result.AddDateFilter(FilterConstants.IssueDate, GetManifestRegistrationDateQuery);
			issueDateFilter.Category = FilterCategories.Dates;
			issueDateFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|IssueDateFilter", FilterConstants.IssueDate);

			var localReferenceNumberFilter = result.AddTextFilter(FilterConstants.LocalReferenceNumber, GetLocalReferenceNumberQuery);
			localReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			localReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|LocalReferenceNumberFilter", FilterConstants.LocalReferenceNumber);

			AddRegisteredUserFilters(result, Factory);

			AddBillFilters(result);

			return result;
		}

		void AddBillFilters(ModuleFilterCollection result)
		{
			var billCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("AsycudaFilterStrip|BillFilterCategory", "Bill"));

			var consignorCodeFilter = result.AddGuidFilter(FilterConstants.ConsignorCode, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
			consignorCodeFilter.Category = billCategory;
			consignorCodeFilter.SubGroup = new BillOrganisationSubGroup(AsycudaBillSchema.ABL_OA_Shipper);
			consignorCodeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ConsignorCodeFilter", FilterConstants.ConsignorCode);

			var consignorNameFilter = result.AddTextFilter(FilterConstants.ConsignorName, AsycudaBillSchema.ABL_ShipperName)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperName);
			consignorNameFilter.SubGroup = new BillSubGroup();
			consignorNameFilter.Category = billCategory;
			consignorNameFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ConsignorNameFilter", FilterConstants.ConsignorName);

			var consigneeCodeFilter = result.AddGuidFilter(FilterConstants.ConsigneeCode, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
			consigneeCodeFilter.Category = billCategory;
			consigneeCodeFilter.SubGroup = new BillOrganisationSubGroup(AsycudaBillSchema.ABL_OA_Consignee);
			consigneeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ConsigneeCodeFilter", FilterConstants.ConsigneeCode);

			var consigneeNameFilter = result.AddTextFilter(FilterConstants.ConsigneeName, AsycudaBillSchema.ABL_ConsigneeName)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneeName);
			consigneeNameFilter.SubGroup = new BillSubGroup();
			consigneeNameFilter.Category = billCategory;
			consigneeNameFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|ConsigneeNameFilter", FilterConstants.ConsigneeName);

			var billNumberFilter = result.AddTextFilter(FilterConstants.BillNumber, AsycudaBillSchema.ABL_BillNumber)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			billNumberFilter.SubGroup = new BillSubGroup();
			billNumberFilter.Category = billCategory;
			billNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|BillNumberFilter", FilterConstants.BillNumber);

			var uniqueConsignmentNumberFilter = result.AddTextFilter(FilterConstants.UniqueConsignmentNumber, AsycudaBillSchema.ABL_UCRNumber)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_UCRNumber);
			uniqueConsignmentNumberFilter.SubGroup = new BillSubGroup();
			uniqueConsignmentNumberFilter.Category = billCategory;
			uniqueConsignmentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|UniqueConsignmentNumberFilter", FilterConstants.UniqueConsignmentNumber);

			var originDestinationFilter = result.AddLocationFilter(FilterConstants.OriginDestinationBills, GetOriginDestinationFilter, Location_List, Location_List);
			originDestinationFilter.SetItemDescriptions(Res.GetData("AsycudaFilterStrip|Origin", "Origin"), Res.GetData("AsycudaFilterStrip|Destination", "Destination"));
			originDestinationFilter.Category = billCategory;
			originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|OriginDestination", FilterConstants.OriginDestinationBills);
		}

		void AddRegisteredUserFilters(ModuleFilterCollection filters, BusinessObjectFactory factory)
		{
			filters.AddNkFilter("Registered User", AsycudaManifestHeaderSchema.AMA_GS_NKCustomsAgent, ModuleIDs.GlbStaff, GetStaffList(factory))
				.MultilingualDescription = ResString.GetMultilingualString("AsycudaFilterStrip|RegisteredUserFilter", "Registered User");
		}

		IBusinessObjectCollection GetStaffList(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbStaffCollection>(), new object[] { factory });
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowFilterHelper = new WorkflowFilterStripsHelper(typeof(AsycudaManifestHeader), AsycudaManifestWorkflowDescriptor.Constants.Code, Factory)
			{
				ShouldAddMilestoneFilters = true,
				ShouldAddRelatedMilestoneFilters = false,
				ShouldAddMiscFilters = true
			};
			helpers.Add(workflowFilterHelper);

			return helpers;
		}

		ZQuery GetLoadDischargePortFilter(ZString loadPort, ZString discPort)
		{
			return GetPortFilter(
				loadPort,
				discPort,
				(billQuery, comparisonOperator) =>
				{
					billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
					billQuery.AddToFilter(AsycudaBillSchema.ABL_RL_NKPortOfLoading, comparisonOperator, loadPort);
				},
				(billQuery, comparisonOperator) =>
				{
					billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
					billQuery.AddToFilter(AsycudaBillSchema.ABL_RL_NKPortOfDischarge, comparisonOperator, discPort);
				});
		}

		ZQuery GetOriginDestinationFilter(ZString loadPort, ZString discPort)
		{
			return GetPortFilter(
				loadPort,
				discPort,
				(billQuery, comparisonOperator) =>
				{
					billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
					billQuery.AddToFilter(AsycudaBillSchema.ABL_RL_NKOrigin, comparisonOperator, loadPort);
				},
				(billQuery, comparisonOperator) =>
				{
					billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
					billQuery.AddToFilter(AsycudaBillSchema.ABL_RL_NKFinalDestination, comparisonOperator, discPort);
				});
		}

		ZQuery GetPortFilter(ZString loadPort, ZString discPort, Action<ZQuery, SQLComparisonOperator> configureBillLoadPortQuery, Action<ZQuery, SQLComparisonOperator> configureBillDischargePortQuery)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			if (!loadPort.IsEmpty || !discPort.IsEmpty)
			{
				if (!loadPort.IsEmpty)
				{
					var comparisonOperator = (loadPort.Length == 2) ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					var masterBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
					configureBillLoadPortQuery(masterBillQuery, comparisonOperator);
					headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, masterBillQuery, JoinCondition.And);
				}

				if (!discPort.IsEmpty)
				{
					var comparisonOperator = (discPort.Length == 2) ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					var masterBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
					configureBillDischargePortQuery(masterBillQuery, comparisonOperator);
					headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, masterBillQuery, JoinCondition.And);
				}
			}
			return headerQuery;
		}

		ZQuery GetETDQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var masterBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			masterBillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			AddDateRange(masterBillQuery, comparisonOperator, JoinCondition.And, AsycudaBillSchema.ABL_E_DEP, value1.Date, value2.Date);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, masterBillQuery, JoinCondition.And);
			return headerQuery;
		}

		ZQuery GetETAQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var masterBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			masterBillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			AddDateRange(masterBillQuery, comparisonOperator, JoinCondition.And, AsycudaBillSchema.ABL_E_ARV, value1.Date, value2.Date);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, masterBillQuery, JoinCondition.And);
			return headerQuery;
		}

		static ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			subQuery.AddToFilter_PossiblyCommaSeparated(AsycudaBillSchema.ABL_BillNumber, comparisonOperator, value);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA, subQuery, JoinCondition.And);
			return headerQuery;
		}

		ZDBOnlyQuery GetShippingAgentAddressQuery(ZGuid agentPK)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);

			orgAddressQuery.AddToFilter(OrgAddressSchema.PK, agentPK);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent, OrgAddressSchema.PK, orgAddressQuery, JoinCondition.And);

			return headerQuery;
		}

		ZDBOnlyQuery GetShippingAgentNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

			subQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, OrgHeaderSchema.PK, subQuery, JoinCondition.And);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent, OrgAddressSchema.PK, orgAddressQuery, JoinCondition.And);
			return headerQuery;
		}

		ZDBOnlyQuery GetRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			bool notIn = false;
			if (comparisonOperator != null)
			{
				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					notIn = true;
				}
			}

			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);

			if (!value.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);
			return headerQuery;
		}

		ZQuery GetRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_IssueDate);

			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);
			return headerQuery;
		}

		ZQuery GetManifestRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, AsycudaBillSchema.ABL_BillIssueDate, value1, value2);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA, subQuery, JoinCondition.And);
			return headerQuery;
		}

		ZDBOnlyQuery GetCarrierQuery(ZGuid carrierPK)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var carrierAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), AsycudaManifestHeaderSchema.AMA_OA_Carrier);
			carrierAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);
			result.AddSubQuery(carrierAddressSubQuery, JoinCondition.And);
			return result;
		}

		ZDBOnlyQuery GetMessageStatusQuery(ZString countryCode, ZString status)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			if (status == MessageStatusCodeList.Codes.NotSent)
			{
				billQuery.AddToFilter(AsycudaBillSchema.ABL_MessageStatus, new[] { status, ZString.Empty });
			}
			else
			{
				billQuery.AddToFilter(AsycudaBillSchema.ABL_MessageStatus, status);
			}

			result.AddSubQuery(AsycudaManifestHeaderSchema.PK, billQuery, JoinCondition.And);
			return result;
		}

		ZDBOnlyQuery GetArrivalStatusQuery(ZString countryCode, ZString status)
		{
			var arrivalLine = new ZDBOnlySubQuery(typeof(AsycudaArrivalLine), AsycudaArrivalLineSchema.ATL_ATH);
			if (status == MessageStatusCodeList.Codes.NotSent)
			{
				arrivalLine.AddToFilter(AsycudaArrivalLineSchema.ATL_MessageStatus, new[] { status, ZString.Empty });
			}
			else
			{
				arrivalLine.AddToFilter(AsycudaArrivalLineSchema.ATL_MessageStatus, status);
			}

			var arrivalHeader = new ZDBOnlySubQuery(typeof(AsycudaArrivalHeader), AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader);
			arrivalHeader.AddSubQuery(AsycudaArrivalHeaderSchema.PK, arrivalLine, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			result.AddSubQuery(AsycudaManifestHeaderSchema.PK, arrivalHeader, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetCustomsStatusQuery(ZString countryCode, ZString status)
		{
			var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			billQuery.AddToFilter(AsycudaBillSchema.ABL_BillStatus, status);

			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			result.AddSubQuery(AsycudaManifestHeaderSchema.PK, billQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetCustomsOfficeQuery(ZString countryCode, ZString office)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			result.AddToFilter(AsycudaManifestHeaderSchema.AMA_CustomsOffice, office);
			return result;
		}

		ZDBOnlyQuery GetManifestTypeQuery(ZString countryCode, ZString type)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			if (!countryCode.IsEmpty)
			{
				result.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, countryCode);
			}

			result.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, type);
			return result;
		}

		protected ZDBOnlyQuery GetLocalReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.LocalReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);

			result.AddSubQuery(AsycudaManifestHeaderSchema.PK, cusEntryNumQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetSpecificCircumstanceTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			var specificCircumstanceQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			specificCircumstanceQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator);
			specificCircumstanceQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);

			result.AddSubQuery(AsycudaManifestHeaderSchema.PK, specificCircumstanceQuery, JoinCondition.And);

			return result;
		}

		CodeDescriptionPairList ManifestApplicationTypeList
		{
			get
			{
				return Factory.GetCachedValue("AsycudaFilterStrip|ManifestTypeList", delegate
				{
					var allPossibleTypes = new CodeDescriptionPairList();
					allPossibleTypes.AddPairIfNotExist(ApplicationCodeTypeList.Codes.Consolidator, "Forwarder ");
					allPossibleTypes.AddPairIfNotExist(ApplicationCodeTypeList.Codes.ShippingLine, "Carrier");
					return allPossibleTypes;
				});
			}
		}

		sealed class BillOrganisationSubGroup(SchemaColumn schemaColumn) : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddToFilter(filter);

				var asycudaBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
				asycudaBillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
				asycudaBillQuery.AddSubQuery(schemaColumn, orgAddressQuery, JoinCondition.And);

				var asycudaManifestHeaderQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				asycudaManifestHeaderQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, asycudaBillQuery, JoinCondition.And);
				return asycudaManifestHeaderQuery;
			}
		}

		sealed class BillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var asycudaBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
				asycudaBillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
				asycudaBillQuery.AddToFilter(filter);

				var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				query.AddSubQuery(AsycudaManifestHeaderSchema.PK, asycudaBillQuery, JoinCondition.And);

				return query;
			}
		}

		sealed class ContainerSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var asycudaContainerQuery = new ZDBOnlySubQuery(typeof(AsycudaContainer), AsycudaContainerSchema.ACN_AMA_Manifest);
				asycudaContainerQuery.AddToFilter(filter);

				var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				query.AddSubQuery(AsycudaManifestHeaderSchema.PK, asycudaContainerQuery, JoinCondition.And);

				return query;
			}
		}

		#region Lookups

		#region OrgHeaderList

		public OrgHeaderCollection OrganizationList
		{
			get { return fOrganizationList ?? (fOrganizationList = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection fOrganizationList;

		#endregion

		#region RefCountryList

		public RefCountryCollection RefCountryList
		{
			get { return fRefCountryList ?? (fRefCountryList = new RefCountryCollection(Factory)); }
		}
		RefCountryCollection fRefCountryList;

		#endregion

		#region OrgAddressList

		public OrgAddressCollection OrgAddressList
		{
			get { return fOrgAddressList ?? (fOrgAddressList = new OrgAddressCollection(Factory)); }
		}
		OrgAddressCollection fOrgAddressList;

		LocationCollection Location_List
		{
			get { return fLocation_List ?? (fLocation_List = new LocationCollection(Factory)); }
		}
		LocationCollection fLocation_List;

		#endregion

		#region TransportModeList
		CodeDescriptionPairList TransportModeList
		{
			get
			{
				return Factory.GetCachedValue("AsycudaFilterStrip|TransportModeList", delegate
				{
					var allPossibleModes = new CodeDescriptionPairList();
					allPossibleModes.AddRange(new Customs.Business.TransportTypeList());
					allPossibleModes.AddPairIfNotExist(Core.Constants.TransportModes.Road, "Road");
					return allPossibleModes;
				});
			}
		}
		#endregion

		#endregion
	}
}
