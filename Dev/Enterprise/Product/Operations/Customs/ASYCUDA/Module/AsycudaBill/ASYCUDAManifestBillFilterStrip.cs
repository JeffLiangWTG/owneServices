using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using AsycudaBill = Enterprise.Customs.ASYCUDA.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class ASYCUDAManifestBillFilterStrip : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string AgentType = "Agent Type";
			public const string Carrier = "Carrier";
			public const string BillCustomsStatus = "Bill Customs Status";
			public const string BillMsgStatus = "Bill Message Status";
			public const string ManifestMsgStatus = "Manifest Message Status";
			public const string ManifestStatus = "Manifest Customs Status";
			public const string HouseBillNumber = "House Bill Number";
			public const string JobReference = "Job Reference";
			public const string MasterBillNumber = "Master Bill Number";
			public const string VesselName = "Vessel Name";
			public const string VoyageNumber = "Voyage / Flight No";
			public const string EstimatedDateOfDeparture = "Estimated Date of Departure";
			public const string EstimatedTimeArrival = "Estimated Time Arrival";
			public const string LoadDischarge = "Port of Load / Discharge";
			public const string TransportMode = "Transport Mode";
			public const string Country = "Country/Region";
			public const string CreatedOnWeb = "Created On Web/Internal";
			public const string CreatedTime = "Created Time";
			public const string CreatingUser = "Creating User";
			public const string LastEditTime = "Last Edit Time";
			public const string LastEditUser = "Last Edit User";
			public const string CustomsValue = "Total Customs Value";
			public const string ContainerMode = "Container Mode";
			public const string VehicleReg = "Vehicle Reg";
			public const string IssueDate = "Issue Date";
			public const string ManifestType = "Manifest Type";
			public const string Nature = "Nature";
			public const string BillRegistrationNumber = "Bill Registration Number";
			public const string BillRegistrationDate = "Bill Registration Date";
			public const string RegistrationDate = "Manifest Registration Date";
			public const string ManifestRegistrationNumber = "Manifest Registration Number";
			public const string CustomsOffice = "Customs Office";
			public const string FirstArrivalPort = "First Arrival Port";
			public const string ShippingAgentName = "Shipping Agent Name";
			public const string ShippingAgentAddress = "Shipping Agent Address";
			public const string CarrierCode = "Carrier Code";
			public const string Origin = "Origin";
			public const string FinalDestination = "Final Destination";
			public const string TotalQty = "Total Qty";
			public const string TotalQtyUnit = "Total Qty Unit";
			public const string TotalGrossWeight = "Total Gross Weight";
			public const string TotalGrossWeightUnit = "Total Gross Weight Unit";
			public const string TotalVolume = "Total Volume";
			public const string TotalVolumeUnit = "Total Volume Unit";
			public const string GoodsDescription = "Goods Description";
			public const string Marks = "Marks";
			public const string Shipper = "Shipper";
			public const string Consignee = "Consignee";
			public const string NotifyParty = "Notify Party";
			public const string Remarks = "Remarks";
			public const string PrepaidCollect = "Prepaid / Collect";
			public const string TotalGoodsValue = "Total Goods Value";
			public const string TotalGoodsValueCurrency = "Total Goods Value Currency";
			public const string TotalFreightValue = "Total Freight Value";
			public const string TotalFreightValueCurrency = "Total Freight Value Currency";
			public const string TotalInsuranceValue = "Total Insurance Value";
			public const string TotalInsuranceValueCurrency = "Total Insurance Value Currency";
			public const string TotalCustomsValue = "Total Customs Value";
			public const string TotalCustomsValueCurrency = "Total Customs Value Currency";
			public const string DiscountValue = "Discount Value";
			public const string DiscountValueCurrency = "Discount Value Currency";
			public const string OtherChargesValue = "Other Charges Value";
			public const string OtherChargesValueCurrency = "Other Charges Value Currency";
			public const string CarrierReference = "Carrier Reference";
			public const string UCRNumber = "UCR Number";
			public const string CustomsJobNumber = "Customs Job Number";
			public const string BillIssuer = "Bill Issuer";
			public const string BillType = "Bill Type";
			public const string ShipmentType = "Shipment Type";
			public const string BillCargoStatus = "Bill Cargo Status";
			public const string ConsigneeName = "Consignee Name";
			public const string ConsigneeStreet = "Consignee Street";
			public const string ConsigneeStreet2 = "Consignee Street 2";
			public const string ConsigneeCity = "Consignee City";
			public const string ConsigneeState = "Consignee State";
			public const string ConsigneeCountry = "Consignee Ctry/Rgn.";
			public const string ConsigneePostcode = "Consignee Postcode";
			public const string ShipperName = "Shipper Name";
			public const string ShipperStreet = "Shipper Street";
			public const string ShipperStreet2 = "Shipper Street 2";
			public const string ShipperCity = "Shipper City";
			public const string ShipperState = "Shipper State";
			public const string ShipperCountry = "Shipper Ctry/Rgn.";
			public const string ShipperPostcode = "Shipper Postcode";
			public const string NotifyPartyName = "Notify Party Name";
			public const string NotifyPartyStreet = "Notify Party Street";
			public const string NotifyPartyStreet2 = "Notify Party Street 2";
			public const string NotifyPartyCity = "Notify Party City";
			public const string NotifyPartyState = "Notify Party State";
			public const string NotifyPartyCountry = "Notify Party Ctry/Rgn.";
			public const string NotifyPartyPostcode = "Notify Party Postcode";
			public const string GoodsLocation = "Goods Location";
			public const string LocationInformation = "Location Information";
			public const string CustomsNumber = "Customs Number";
			public const string CustomsNumberType = "Customs Number Type";
			public const string PackMessageStatus = "Pack Message Status";
			public const string PackCustomsStatus = "Pack Customs Status";
			public const string PackLinePrice = "Pack Line Price";
			public const string PackQty = "Pack Qty";
			public const string PackQtyUnit = "Pack Qty Unit";
			public const string PackWeight = "Pack Weight";
			public const string PackVolume = "Pack Volume";
			public const string PackGoodsDescription = "Pack Goods Description";
			public const string PackCommodityCode = "Pack Commodity Code";
			public const string PackOrigin = "Pack Origin";
			public const string PackCustomsDescription = "Pack Customs Description";
			public const string PackCustomsTariff = "Pack Customs Tariff";
			public const string PackCustomsQty = "Pack Customs Qty";
			public const string PackCustomsValue = "Pack Customs Value";
			public const string PackCustomsDutyAmount = "Pack Customs Duty Amount";
			public const string PackCustomsTaxAmount = "Pack Customs Tax Amount";
			public const string PackCustomsGoodsType = "Pack Customs Goods Type";
			public const string PackReferenceNumber = "Pack Reference Number";
			public const string PackReferenceNumberType = "Pack Reference Number Type";
			public const string LocalReferenceNumber = "Local Reference Number";
			public const string SpecialCargoCode = "Special Cargo Code";
		}

		public ASYCUDAManifestBillFilterStrip(bool enableCountryFilter = true)
		{
			shouldAddCountryFilter = enableCountryFilter;
			QueryObjectType = typeof(AsycudaBill);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new ASYCUDAManifestBillFilterStrip(shouldAddCountryFilter);

		readonly bool shouldAddCountryFilter;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			if (shouldAddCountryFilter)
			{
				var countryFilter = result.AddNkFilter(FilterConstants.Country, (op, value) => GetHeaderQuery(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, value, op), ModuleIDs.RefCountry, CountryList);
				countryFilter.Category = FilterCategories.NumbersAndReferences;
				countryFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CountryFilter", FilterConstants.Country);
				countryFilter.MaxLength = AsycudaManifestHeaderSchema.AMA_RN_NKCountry.MaxLength;
			}

			var carrierFilter = result.AddGuidFilter(FilterConstants.Carrier, ModuleIDs.Organisation, GetCarrierQuery, OrganizationList);
			carrierFilter.Category = FilterCategories.Organisations;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CarrierFilter", FilterConstants.Carrier);

			var billCustomsStatusFilter = new CountryRelatedFilter(
				FilterConstants.BillCustomsStatus,
				(country, status) =>
				{
					var query = new ZDBOnlyQuery(typeof(AsycudaBill));
					query.AddToFilter(AsycudaBillSchema.ABL_BillStatus, status);
					return query;
				},
				Factory,
				FieldType.TextDropEdit,
				AsycudaBillSchema.ABL_BillStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsStatusGetter);
			billCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			billCustomsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|BillCustomsStatusFilter", FilterConstants.BillCustomsStatus);
			result.AddFilter(billCustomsStatusFilter);

			var messageStatusFilter = new CountryRelatedFilter(
				FilterConstants.BillMsgStatus,
				GetBillMessageStatusQuery,
				Factory,
				FieldType.TextDropEdit,
				AsycudaBillSchema.ABL_MessageStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.MessageStatusGetter);
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|BillMessageStatusFilter", FilterConstants.BillMsgStatus);
			result.AddFilter(messageStatusFilter);

			var manifestMsgStatusFilter = new CountryRelatedFilter(
				FilterConstants.ManifestMsgStatus,
				GetManifestMsgStatusQuery,
				Factory,
				FieldType.TextDropEdit,
				AsycudaManifestHeaderSchema.AMA_MessageStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.MessageStatusGetter);
			manifestMsgStatusFilter.Category = FilterCategories.StatusAndFlags;
			manifestMsgStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ManifestMessageStatusFilter", FilterConstants.ManifestMsgStatus);
			result.AddFilter(manifestMsgStatusFilter);

			var billCargoStatusFilter = new CountryRelatedFilter(
				FilterConstants.BillCargoStatus,
				(country, status) =>
				{
					var query = new ZDBOnlyQuery(typeof(AsycudaBill));
					query.AddToFilter(AsycudaBillSchema.ABL_CargoStatus, status);
					return query;
				},
				Factory,
				FieldType.TextDropEdit,
				AsycudaBillSchema.ABL_CargoStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CargoStatusGetter);
			billCargoStatusFilter.Category = FilterCategories.StatusAndFlags;
			billCargoStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|BillCargoStatusFilter", FilterConstants.BillCargoStatus);
			result.AddFilter(billCargoStatusFilter);

			var manifestStatusFilter = new CountryRelatedFilter(
				FilterConstants.ManifestStatus,
				(country, status) =>
				{
					var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, status);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, new List<ZString> { "", country });
					return GetBillQuery(cusEntryNumQuery);
				},
				Factory,
				FieldType.TextDropEdit,
				AsycudaManifestHeader.Schema.AMA_CustomsStatusMaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsStatusGetter);
			manifestStatusFilter.Category = FilterCategories.StatusAndFlags;
			manifestStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ManifestCustomsStatusFilter", FilterConstants.ManifestStatus);
			result.AddFilter(manifestStatusFilter);

			var houseBillNumberFilter = result.AddTextFilter(FilterConstants.HouseBillNumber, GetHouseBillQuery)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			houseBillNumberFilter.Category = FilterCategories.NumbersAndReferences;
			houseBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|HouseBillNumberFilter", FilterConstants.HouseBillNumber);

			var jobReferenceFilter = result.AddTextFilter(FilterConstants.JobReference, (op, value) => GetHeaderQuery(AsycudaManifestHeaderSchema.AMA_JobReference, value, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_JobReference);
			jobReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|JobReferenceFilter", FilterConstants.JobReference);

			var masterBillNumberFilter = result.AddTextFilter(FilterConstants.MasterBillNumber, GetMasterBillQuery)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			masterBillNumberFilter.Category = FilterCategories.NumbersAndReferences;
			masterBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|MasterBillNumberFilter", FilterConstants.MasterBillNumber);

			var vesselNameFilter = result.AddTextFilter(FilterConstants.VesselName, (op, value) => GetHeaderQuery(AsycudaManifestHeaderSchema.AMA_VesselName, value, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_VesselName);
			vesselNameFilter.Category = FilterCategories.NumbersAndReferences;
			vesselNameFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|VesselNameFilter", FilterConstants.VesselName);

			var voyageNumberFilter = result.AddTextFilter(FilterConstants.VoyageNumber, (op, value) => GetHeaderQuery(AsycudaManifestHeaderSchema.AMA_Voyage, value, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_Voyage);
			voyageNumberFilter.Category = FilterCategories.NumbersAndReferences;
			voyageNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|VoyageNumberFilter", FilterConstants.VoyageNumber);

			var estimatedDateOfDepartureFilter = result.AddDateFilter(FilterConstants.EstimatedDateOfDeparture, (comparisonOperator, value1, value2) => GetMasterBillDateQuery(AsycudaBillSchema.ABL_E_DEP, comparisonOperator, value1, value2));
			estimatedDateOfDepartureFilter.Category = FilterCategories.Dates;
			estimatedDateOfDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|EstimatedDateOfDepartureFilter", FilterConstants.EstimatedDateOfDeparture);

			var estimatedTimeArrivalFilter = result.AddDateFilter(FilterConstants.EstimatedTimeArrival, (comparisonOperator, value1, value2) => GetMasterBillDateQuery(AsycudaBillSchema.ABL_E_ARV, comparisonOperator, value1, value2));
			estimatedTimeArrivalFilter.Category = FilterCategories.Dates;
			estimatedTimeArrivalFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|EstimatedTimeArrivalFilter", FilterConstants.EstimatedTimeArrival);

			var loadingFilter = result.AddLocationFilter(FilterConstants.LoadDischarge, GetLoadDischargePortFilter, LocationList, LocationList);
			loadingFilter.SetItemDescriptions(Res.GetData("ManifestBillFilterStrip|Loading", "Loading"), Res.GetData("ManifestBillFilterStrip|Discharge", "Discharge"));
			loadingFilter.Category = FilterCategories.Locations;
			loadingFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|LoadDischargeFilter", FilterConstants.LoadDischarge);

			var transportModeFilter = result.AddTextFilter(FilterConstants.TransportMode, value => GetHeaderQuery(AsycudaManifestHeaderSchema.AMA_TransportMode, value), new TransportTypeList());
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TransportModeFilter", FilterConstants.TransportMode);

			var agentTypeFilter = result.AddTextFilter(FilterConstants.AgentType, (op, value) => GetHeaderQuery(AsycudaManifestHeaderSchema.AMA_AgentType, value, op));
			agentTypeFilter.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_AgentType);
			agentTypeFilter.Category = FilterCategories.NumbersAndReferences;
			agentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|AgentTypeFilter", FilterConstants.AgentType);
			AddManifestHeaderFilters(result);
			AddAsycudaBillFilters(result);
			AddBillCountryFilters(result);
			AddPackedItemFilters(result);
			AddPackFilters(result);

			return result;
		}

		protected override ZQuery CombineModuleFilters()
		{
			var result = base.CombineModuleFilters();
			result.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelper(typeof(AsycudaBill), WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helper.ShouldAddMilestoneFilters = false;
			helper.ShouldAddMiscFilters = false;
			helper.ShouldAddRelatedMilestoneFilters = false;
			helpers.Add(helper);

			return helpers;
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters => new[] { typeof(IBMFilterStripsHelper) };

		protected override ZQuery GetActiveStatusQueryCore(ZString status)
		{
			var query = base.GetActiveStatusQueryCore(status);

			status = status.Trim();
			return StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true) ? query : GetActiveStatusOfManifestAndBillQuery(query, status);
		}

		void AddManifestHeaderFilters(ModuleFilterCollection result)
		{
			var headerQuerySubGroup = new HeaderQuerySubGroup();
			var containerModeFilter = new HideComparisonOperatorModuleTextFilter(FilterConstants.ContainerMode, AsycudaManifestHeaderSchema.AMA_ContainerMode, AsycudaManifestHeaderLookups.ContainerModeList)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_ContainerMode);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.SubGroup = headerQuerySubGroup;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ContainerModeFilter", FilterConstants.ContainerMode);
			result.AddFilter(containerModeFilter);

			var vehicleRegFilter = result.AddTextFilter(FilterConstants.VehicleReg, AsycudaManifestHeaderSchema.AMA_VehicleRegistration)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_VehicleRegistration);
			vehicleRegFilter.Category = FilterCategories.ModesAndTypes;
			vehicleRegFilter.SubGroup = headerQuerySubGroup;
			vehicleRegFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|VehicleRegFilter", FilterConstants.VehicleReg);

			var manifestTypeFilter = new DataGroupingRelatedFilter(
				FilterConstants.ManifestType,
				(country, type) =>
				{
					var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
					if (!country.IsEmpty)
					{
						headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, country);
					}
					headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, type);
					return GetBillQuery(headerQuery);
				},
				Factory,
				FieldType.TextDropEdit,
				AsycudaManifestHeaderSchema.AMA_ManifestType.MaxLength,
				Res.GetData("ASYCUDAManifestBillFilterStrip|ManifestTypeFilter", FilterConstants.ManifestType),
				ApplicationBusinessProvider.GetManifestTypeListByCountry);
			manifestTypeFilter.UseProperty2ListGetterWhenProperty1IsEmpty = true;
			manifestTypeFilter.Category = FilterCategories.ModesAndTypes;
			manifestTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ManifestTypeFilter", FilterConstants.ManifestType);
			result.AddFilter(manifestTypeFilter);

			var natureFilter = result.AddTextFilter(FilterConstants.Nature, AsycudaManifestHeaderSchema.AMA_Nature, AsycudaManifestHeaderLookups.GetNatureList(Factory))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_Nature);
			natureFilter.Category = FilterCategories.ModesAndTypes;
			natureFilter.SubGroup = headerQuerySubGroup;
			natureFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NatureFilter", FilterConstants.Nature);

			var customsOfficeFilter = new CountryRelatedFilter(
				FilterConstants.CustomsOffice,
				(country, office) =>
				{
					var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
					headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_CustomsOffice, office);
					return GetBillQuery(headerQuery);
				},
				Factory,
				FieldType.TextDropEdit,
				AsycudaManifestHeaderSchema.AMA_CustomsOffice.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsOfficeGetter);
			customsOfficeFilter.Category = FilterCategories.Locations;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CustomsOfficeFilter", FilterConstants.CustomsOffice);
			result.AddFilter(customsOfficeFilter);

			var firstArrivalPortFilter = result.AddNkFilter(FilterConstants.FirstArrivalPort, AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival);
			firstArrivalPortFilter.Category = FilterCategories.Locations;
			firstArrivalPortFilter.SubGroup = headerQuerySubGroup;
			firstArrivalPortFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|FirstArrivalPortFilter", FilterConstants.FirstArrivalPort);

			var carrierCodeFilter = result.AddTextFilter(FilterConstants.CarrierCode, AsycudaManifestHeaderSchema.AMA_CarrierCode)
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaManifestHeaderSchema.AMA_CarrierCode);
			carrierCodeFilter.Category = FilterCategories.NumbersAndReferences;
			carrierCodeFilter.SubGroup = headerQuerySubGroup;
			carrierCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CarrierCodeFilter", FilterConstants.CarrierCode);

			var shippingAgentAddressFilter = result.AddGuidFilter(FilterConstants.ShippingAgentAddress, ModuleIDs.OrgAddresses, GetShippingAgentAddressQuery, OrgAddressList)
				.WithMaxLengthOf<ModuleGuidFilter>(OrgAddressSchema.OA_OH);
			shippingAgentAddressFilter.Category = FilterCategories.Locations;
			shippingAgentAddressFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShippingAgentAddressFilter", FilterConstants.ShippingAgentAddress);

			var shippingAgentNameFilter = result.AddTextFilter(FilterConstants.ShippingAgentName, OrgHeaderSchema.OH_FullName)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			shippingAgentNameFilter.Category = FilterCategories.Organisations;
			shippingAgentNameFilter.SubGroup = new ShippingAgentNameFilterSubGroup();
			shippingAgentNameFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShippingAgentNameFilter", FilterConstants.ShippingAgentName);

			var registrationDateFilter = result.AddDateFilter(FilterConstants.RegistrationDate, GetRegistrationDateQuery);
			registrationDateFilter.Category = FilterCategories.Dates;
			registrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|RegistrationDateFilter", FilterConstants.RegistrationDate);

			var customsJobNumberFilter = result.AddTextFilter(FilterConstants.CustomsJobNumber, GetCustomsJobQuery);
			customsJobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			customsJobNumberFilter.MaxLength = AsycudaBill.Schema.CustomsJobNumberMaxLength;
			customsJobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CustomsJobNumberFilter", FilterConstants.CustomsJobNumber);

			var regNumFilter = result.AddTextFilter(FilterConstants.BillRegistrationNumber, GetBillRegistrationNumberQuery)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			regNumFilter.Category = FilterCategories.NumbersAndReferences;
			regNumFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|RegistrationNumberFilter", FilterConstants.BillRegistrationNumber);

			var localReferenceNumberFilter = result.AddTextFilter(FilterConstants.LocalReferenceNumber, GetLocalReferenceNumberQuery);
			localReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			localReferenceNumberFilter.SubGroup = headerQuerySubGroup;
			localReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|LocalReferenceNumber", FilterConstants.LocalReferenceNumber);

			var manifestRegNumFilter = result.AddTextFilter(FilterConstants.ManifestRegistrationNumber, GetManifestRegistrationNumberQuery)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			manifestRegNumFilter.Category = FilterCategories.NumbersAndReferences;
			manifestRegNumFilter.SubGroup = headerQuerySubGroup;
			manifestRegNumFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ManifestRegistrationNumber", FilterConstants.ManifestRegistrationNumber);
		}

		void AddBillCountryFilters(ModuleFilterCollection result)
		{
			var billIssuerFilter = result.AddTextFilter(FilterConstants.BillIssuer, AsycudaBillSchema.ABL_BillIssuer);
			billIssuerFilter.Category = FilterCategories.TextSearch;
			billIssuerFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|BillIssuerFilter", FilterConstants.BillIssuer);

			var shipmentTypeFilter = result.AddTextFilter(FilterConstants.ShipmentType, AsycudaBillSchema.ABL_ShipmentType, AsycudaBillLookups.GetShipmentTypes(Factory));
			shipmentTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipmentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipmentTypeFilter", FilterConstants.ShipmentType);

			var goodsLocationFilter = result.AddTextFilter(FilterConstants.GoodsLocation, AsycudaBillSchema.ABL_GoodsLocation);
			goodsLocationFilter.Category = FilterCategories.Locations;
			goodsLocationFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|GoodsLocationFilter", FilterConstants.GoodsLocation);

			var locationInformationFilter = result.AddTextFilter(FilterConstants.LocationInformation, AsycudaBillSchema.ABL_LocationInformation);
			locationInformationFilter.Category = FilterCategories.NumbersAndReferences;
			locationInformationFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|LocationInformationFilter", FilterConstants.LocationInformation);

			var customsNumberFilter = result.AddTextFilter(FilterConstants.CustomsNumber, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			customsNumberFilter.Category = FilterCategories.NumbersAndReferences;
			customsNumberFilter.SubGroup = new CusEntryNumFilterSubGroup(customsNumberFilter);
			customsNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CustomsNumberFilter", FilterConstants.CustomsNumber);
			var customsNumberTypeFilter = new CountryRelatedFilter(
				FilterConstants.CustomsNumberType,
				(country, type) => new ZQuery(CusEntryNumSchema.CE_EntryType, type),
				Factory,
				FieldType.TextDropEdit,
				CusEntryNumSchema.CE_EntryType.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsNumberTypeGetter);
			customsNumberTypeFilter.Category = FilterCategories.ModesAndTypes;
			customsNumberTypeFilter.SubGroup = new CusEntryNumFilterSubGroup();
			customsNumberTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CustomsNumberTypeFilter", FilterConstants.CustomsNumberType);
			result.AddFilter(customsNumberTypeFilter);
		}

		void AddPackedItemFilters(ModuleFilterCollection result)
		{
			var cusEntryPackedItemSubGroup = new CusEntryPackedItemFilterSubGroup();
			var packRefNoFilter = result.AddTextFilter(FilterConstants.PackReferenceNumber, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			packRefNoFilter.Category = FilterCategories.NumbersAndReferences;
			packRefNoFilter.SubGroup = cusEntryPackedItemSubGroup;
			packRefNoFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackReferenceNumberFilter", FilterConstants.PackReferenceNumber);

			var packRegNoTypeFilter = new CountryRelatedFilter(
				FilterConstants.PackReferenceNumberType,
				(country, type) => new ZQuery(CusEntryNumSchema.CE_EntryType, type),
				Factory,
				FieldType.TextDropEdit, CusEntryNumSchema.CE_EntryType.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsNumberTypeGetter);
			packRegNoTypeFilter.Category = FilterCategories.ModesAndTypes;
			packRegNoTypeFilter.SubGroup = cusEntryPackedItemSubGroup;
			packRegNoTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackReferenceNumberTypeFilter", FilterConstants.PackReferenceNumberType);
			result.AddFilter(packRegNoTypeFilter);
		}

		void AddAsycudaBillFilters(ModuleFilterCollection result)
		{
			AddBillDateFilters(result);
			AddBillGenAddOnFilters(result);
			AddBillNKFilters(result);
			AddBillTextFilters(result);
			AddBillNumberRangeQueries(result);
			AddBillShipperFilters(result);
			AddBillConsigneeFilters(result);
			AddBillNotifyPartyFilters(result);
		}

		void AddBillDateFilters(ModuleFilterCollection result)
		{
			var issueDateFilter = result.AddDateFilter(FilterConstants.IssueDate, GetManifestRegistrationDateQuery);
			issueDateFilter.Category = FilterCategories.Dates;
			issueDateFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|IssueDateFilter", FilterConstants.IssueDate);

			var billRegistrationDateFilter = result.AddDateFilter(FilterConstants.BillRegistrationDate, GetBillRegistrationDateQuery);
			billRegistrationDateFilter.Category = FilterCategories.Dates;
			billRegistrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|BillRegistrationDateFilter", FilterConstants.BillRegistrationDate);
		}

		void AddBillGenAddOnFilters(ModuleFilterCollection result)
		{
			var discountValueCurrencyFilter = result.AddTextFilter(FilterConstants.DiscountValueCurrency, (op, value) => GetBillQueryForGenAddOn(value, AsycudaBill.Schema.DiscountValueCurrency, op));
			discountValueCurrencyFilter.Category = FilterCategories.FinancialDetails;
			discountValueCurrencyFilter.MaxLength = AsycudaBill.Schema.DiscountValueCurrencyMaxLength;
			discountValueCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|DiscountValueCurrencyFilter", FilterConstants.DiscountValueCurrency);

			var otherChargesValueCurrencyFilter = result.AddTextFilter(FilterConstants.OtherChargesValueCurrency, (op, value) => GetBillQueryForGenAddOn(value, AsycudaBill.Schema.OtherChargesValueCurrency, op));
			otherChargesValueCurrencyFilter.Category = FilterCategories.FinancialDetails;
			otherChargesValueCurrencyFilter.MaxLength = AsycudaBill.Schema.OtherChargesValueCurrencyMaxLength;
			otherChargesValueCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|OtherChargesValueCurrencyFilter", FilterConstants.OtherChargesValueCurrency);

			var discountValueFilter = GetFilterForGenAddOnColumn(FilterConstants.DiscountValue, AsycudaBill.Schema.DiscountValue, AsycudaBillSchema.Constants.Prefix, typeof(AsycudaBill), AsycudaBillSchema.PK, 2);
			discountValueFilter.Category = FilterCategories.FinancialDetails;
			discountValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|DiscountValueFilter", FilterConstants.DiscountValue);
			result.AddFilter(discountValueFilter);

			var otherChargesValueFilter = GetFilterForGenAddOnColumn(FilterConstants.OtherChargesValue, AsycudaBill.Schema.OtherChargesValue, AsycudaBillSchema.Constants.Prefix, typeof(AsycudaBill), AsycudaBillSchema.PK, 2);
			otherChargesValueFilter.Category = FilterCategories.FinancialDetails;
			otherChargesValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|OtherChargesValueFilter", FilterConstants.OtherChargesValue);
			result.AddFilter(otherChargesValueFilter);
		}

		void AddBillNKFilters(ModuleFilterCollection result)
		{
			var originFilter = result.AddNkFilter(FilterConstants.Origin, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RL_NKOrigin, op), ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RL_NKOrigin);
			originFilter.Category = FilterCategories.Locations;
			originFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|OriginFilter", FilterConstants.Origin);

			var finalDestinationFilter = result.AddNkFilter(FilterConstants.FinalDestination, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RL_NKFinalDestination, op), ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RL_NKFinalDestination);
			finalDestinationFilter.Category = FilterCategories.Locations;
			finalDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|FinalDestinationFilter", FilterConstants.FinalDestination);

			var totalGoodsValueCurrencyFilter = result.AddNkFilter(FilterConstants.TotalGoodsValueCurrency, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RX_NKFreightValueCurrency, op), ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RX_NKFreightValueCurrency);
			totalGoodsValueCurrencyFilter.Category = FilterCategories.FinancialDetails;
			totalGoodsValueCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalGoodsValueCurrencyFilter", FilterConstants.TotalGoodsValueCurrency);

			var totalFreightValueCurrencyFilter = result.AddNkFilter(FilterConstants.TotalFreightValueCurrency, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RX_NKTransportValueCurrency, op), ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RX_NKTransportValueCurrency);
			totalFreightValueCurrencyFilter.Category = FilterCategories.FinancialDetails;
			totalFreightValueCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalFreightValueCurrencyFilter", FilterConstants.TotalFreightValueCurrency);

			var totalInsuranceValueCurrencyFilter = result.AddNkFilter(FilterConstants.TotalInsuranceValueCurrency, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency, op), ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency);
			totalInsuranceValueCurrencyFilter.Category = FilterCategories.FinancialDetails;
			totalInsuranceValueCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalInsuranceValueCurrencyFilter", FilterConstants.TotalInsuranceValueCurrency);

			var totalCustomsValueCurrencyFilter = result.AddNkFilter(FilterConstants.TotalCustomsValueCurrency, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RX_NKCustomsValueCurrency, op), ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RX_NKCustomsValueCurrency);
			totalCustomsValueCurrencyFilter.Category = FilterCategories.FinancialDetails;
			totalCustomsValueCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalCustomsValueCurrencyFilter", FilterConstants.TotalCustomsValueCurrency);
		}

		void AddBillTextFilters(ModuleFilterCollection result)
		{
			var totalQtyUnitFilter = result.AddTextFilter(FilterConstants.TotalQtyUnit, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ManifestUQ, op), Factory.GetPackageTypeList())
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ManifestUQ);
			totalQtyUnitFilter.Category = FilterCategories.NumbersAndReferences;
			totalQtyUnitFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalQtyUnitFilter", FilterConstants.TotalQtyUnit);
			var totalGrossWeightUnitFilter = result.AddTextFilter(FilterConstants.TotalGrossWeightUnit, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_GrossWeightUQ, op), Factory.GetWeightUQList())
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_GrossWeightUQ);
			totalGrossWeightUnitFilter.Category = FilterCategories.NumbersAndReferences;
			totalGrossWeightUnitFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalGrossWeightUnitFilter", FilterConstants.TotalGrossWeightUnit);
			var totalVolumeUnitFilter = result.AddTextFilter(FilterConstants.TotalVolumeUnit, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_VolumeUQ, op), Factory.GetVolumeUQList())
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_VolumeUQ);
			totalVolumeUnitFilter.Category = FilterCategories.NumbersAndReferences;
			totalVolumeUnitFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalVolumeUnitFilter", FilterConstants.TotalVolumeUnit);
			var goodsDescriptionFilter = result.AddTextFilter(FilterConstants.GoodsDescription, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_GoodsDescription, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_GoodsDescription);
			goodsDescriptionFilter.Category = FilterCategories.TextSearch;
			goodsDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|GoodsDescriptionFilter", FilterConstants.GoodsDescription);
			var marksFilter = result.AddTextFilter(FilterConstants.Marks, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_MarksAndNumbers, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_MarksAndNumbers);
			marksFilter.Category = FilterCategories.TextSearch;
			marksFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|MarksFilter", FilterConstants.Marks);
			var remarksFilter = result.AddTextFilter(FilterConstants.Remarks, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_Remarks, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_Remarks);
			remarksFilter.Category = FilterCategories.NumbersAndReferences;
			remarksFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|RemarksFilter", FilterConstants.Remarks);
			var prepaidCollectFilter = result.AddTextFilter(FilterConstants.PrepaidCollect, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_PrepaidCollect, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_PrepaidCollect);
			prepaidCollectFilter.Category = FilterCategories.StatusAndFlags;
			prepaidCollectFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PrepaidCollectFilter", FilterConstants.PrepaidCollect);
			var carrierReferenceFilter = result.AddTextFilter(FilterConstants.CarrierReference, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_CarrierReference, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_CarrierReference);
			carrierReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			carrierReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|CarrierReferenceFilter", FilterConstants.CarrierReference);

			var ucrNumberFilter = result.AddTextFilter(FilterConstants.UCRNumber, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_UCRNumber, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_UCRNumber);
			ucrNumberFilter.Category = FilterCategories.NumbersAndReferences;
			ucrNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|UCRNumberFilter", FilterConstants.UCRNumber);
			var billTypeFilter = result.AddTextFilter(FilterConstants.BillType, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_BolType, op), AsycudaBillLookups.GetBolTypes(Factory))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BolType);
			billTypeFilter.Category = FilterCategories.ModesAndTypes;
			billTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|BillTypeFilter", FilterConstants.BillType);

			var specialCargoCodeFilter = result.AddTextFilter(FilterConstants.SpecialCargoCode, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_SpecialCargoCode, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_SpecialCargoCode);
			specialCargoCodeFilter.Category = FilterCategories.NumbersAndReferences;
			specialCargoCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|SpecialCargoCode", FilterConstants.SpecialCargoCode);
		}

		void AddBillNumberRangeQueries(ModuleFilterCollection result)
		{
			var totalQtyFilter = result.AddNumberRangeFilter(FilterConstants.TotalQty, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_ManifestQty));
			totalQtyFilter.Category = FilterCategories.NumbersAndReferences;
			totalQtyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalQtyFilter", FilterConstants.TotalQty);

			var totalGrossWeightFilter = result.AddNumberRangeFilter(FilterConstants.TotalGrossWeight, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_GrossWeight));
			totalGrossWeightFilter.Category = FilterCategories.NumbersAndReferences;
			totalGrossWeightFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalGrossWeightFilter", FilterConstants.TotalGrossWeight);

			var totalVolumeFilter = result.AddNumberRangeFilter(FilterConstants.TotalVolume, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_Volume));
			totalVolumeFilter.Category = FilterCategories.NumbersAndReferences;
			totalVolumeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalVolumeFilter", FilterConstants.TotalVolume);

			var totalGoodsValueFilter = result.AddNumberRangeFilter(FilterConstants.TotalGoodsValue, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_FreightValue));
			totalGoodsValueFilter.Category = FilterCategories.FinancialDetails;
			totalGoodsValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalGoodsValueFilter", FilterConstants.TotalGoodsValue);

			var totalFreightValueFilter = result.AddNumberRangeFilter(FilterConstants.TotalFreightValue, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_TransportValue));
			totalFreightValueFilter.Category = FilterCategories.FinancialDetails;
			totalFreightValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalFreightValueFilter", FilterConstants.TotalFreightValue);

			var totalInsuranceValueFilter = result.AddNumberRangeFilter(FilterConstants.TotalInsuranceValue, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_InsuranceValue));
			totalInsuranceValueFilter.Category = FilterCategories.FinancialDetails;
			totalInsuranceValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalInsuranceValueFilter", FilterConstants.TotalInsuranceValue);

			var totalCustomsValueFilter = result.AddNumberRangeFilter(FilterConstants.TotalCustomsValue, (valueTo, valueFrom) => GetBillNumberRangeQuery(valueTo, valueFrom, AsycudaBillSchema.ABL_CustomsValue));
			totalCustomsValueFilter.Category = FilterCategories.FinancialDetails;
			totalCustomsValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|TotalCustomsValueFilter", FilterConstants.TotalCustomsValue);
		}

		void AddBillShipperFilters(ModuleFilterCollection result)
		{
			var shipperFilter = result.AddGuidFilter(FilterConstants.Shipper, ModuleIDs.Organisation, (op, value) => GetOrgQuery(value, AsycudaBillSchema.ABL_OA_Shipper, op), OrganizationList)
				.WithMaxLengthOf<ModuleGuidFilter>(AsycudaBillSchema.ABL_OA_Shipper);
			shipperFilter.Category = FilterCategories.Organisations;
			shipperFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperFilter", FilterConstants.Shipper);
			var shipperNameFilter = result.AddTextFilter(FilterConstants.ShipperName, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ShipperName, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperName);
			shipperNameFilter.Category = FilterCategories.Organisations;
			shipperNameFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperNameFilter", FilterConstants.ShipperName);

			var shipperStreetFilter = result.AddTextFilter(FilterConstants.ShipperStreet, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ShipperStreet1, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperStreet1);
			shipperStreetFilter.Category = FilterCategories.Locations;
			shipperStreetFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperStreetFilter", FilterConstants.ShipperStreet);

			var shipperStreet2Filter = result.AddTextFilter(FilterConstants.ShipperStreet2, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ShipperStreet2, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperStreet2);
			shipperStreet2Filter.Category = FilterCategories.Locations;
			shipperStreet2Filter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperStreet2Filter", FilterConstants.ShipperStreet2);

			var shipperCityFilter = result.AddTextFilter(FilterConstants.ShipperCity, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ShipperCity, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperCity);
			shipperCityFilter.Category = FilterCategories.Locations;
			shipperCityFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperCityFilter", FilterConstants.ShipperCity);

			var shipperStateFilter = result.AddTextFilter(FilterConstants.ShipperState, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ShipperState, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperState);
			shipperStateFilter.Category = FilterCategories.Locations;
			shipperStateFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperStateFilter", FilterConstants.ShipperState);

			var shipperCountryFilter = result.AddNkFilter(FilterConstants.ShipperCountry, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RN_NKShipperCountry, op), ModuleIDs.RefCountry, new RefCountryCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RN_NKShipperCountry);
			shipperCountryFilter.Category = FilterCategories.Locations;
			shipperCountryFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperCountryFilter", FilterConstants.ShipperCountry);

			var shipperPostcodeFilter = result.AddTextFilter(FilterConstants.ShipperPostcode, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ShipperPostcode, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ShipperPostcode);
			shipperPostcodeFilter.Category = FilterCategories.Locations;
			shipperPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ShipperPostcodeFilter", FilterConstants.ShipperPostcode);
		}

		void AddBillConsigneeFilters(ModuleFilterCollection result)
		{
			var consigneeFilter = result.AddGuidFilter(FilterConstants.Consignee, ModuleIDs.Organisation, (op, value) => GetOrgQuery(value, AsycudaBillSchema.ABL_OA_Consignee, op), OrganizationList)
				.WithMaxLengthOf<ModuleGuidFilter>(AsycudaBillSchema.ABL_OA_Consignee);
			consigneeFilter.Category = FilterCategories.Organisations;
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeFilter", FilterConstants.Consignee);

			var consigneeNameFilter = result.AddTextFilter(FilterConstants.ConsigneeName, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ConsigneeName, op))
							.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneeName);
			consigneeNameFilter.Category = FilterCategories.Organisations;
			consigneeNameFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeNameFilter", FilterConstants.ConsigneeName);

			var consigneeStreetFilter = result.AddTextFilter(FilterConstants.ConsigneeStreet, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ConsigneeStreet1, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneeStreet1);
			consigneeStreetFilter.Category = FilterCategories.Locations;
			consigneeStreetFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeStreetFilter", FilterConstants.ConsigneeStreet);
			var consigneeStreet2Filter = result.AddTextFilter(FilterConstants.ConsigneeStreet2, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ConsigneeStreet2, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneeStreet2);
			consigneeStreet2Filter.Category = FilterCategories.Locations;
			consigneeStreet2Filter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeStreet2Filter", FilterConstants.ConsigneeStreet2);

			var consigneeCityFilter = result.AddTextFilter(FilterConstants.ConsigneeCity, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ConsigneeCity, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneeCity);
			consigneeCityFilter.Category = FilterCategories.Locations;
			consigneeCityFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeCityFilter", FilterConstants.ConsigneeCity);

			var consigneeStateFilter = result.AddTextFilter(FilterConstants.ConsigneeState, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ConsigneeState, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneeState);
			consigneeStateFilter.Category = FilterCategories.Locations;
			consigneeStateFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeStateFilter", FilterConstants.ConsigneeState);

			var consigneeCountryFilter = result.AddNkFilter(FilterConstants.ConsigneeCountry, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RN_NKConsigneeCountry, op), ModuleIDs.RefCountry, new RefCountryCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RN_NKConsigneeCountry);
			consigneeCountryFilter.Category = FilterCategories.Locations;
			consigneeCountryFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneeCountryFilter", FilterConstants.ConsigneeCountry);

			var consigneePostcodeFilter = result.AddTextFilter(FilterConstants.ConsigneePostcode, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_ConsigneePostcode, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_ConsigneePostcode);
			consigneePostcodeFilter.Category = FilterCategories.Locations;
			consigneePostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|ConsigneePostcodeFilter", FilterConstants.ConsigneePostcode);
		}

		void AddBillNotifyPartyFilters(ModuleFilterCollection result)
		{
			var notifyPartyFilter = result.AddGuidFilter(FilterConstants.NotifyParty, ModuleIDs.Organisation, (op, value) => GetOrgQuery(value, AsycudaBillSchema.ABL_OA_NotifyParty, op), OrganizationList)
				.WithMaxLengthOf<ModuleGuidFilter>(AsycudaBillSchema.ABL_OA_NotifyParty);
			notifyPartyFilter.Category = FilterCategories.Organisations;
			notifyPartyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyFilter", FilterConstants.NotifyParty);

			var notifyPartyNameFilter = result.AddTextFilter(FilterConstants.NotifyPartyName, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_NotifyPartyName, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_NotifyPartyName);
			notifyPartyNameFilter.Category = FilterCategories.Organisations;
			notifyPartyNameFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyNameFilter", FilterConstants.NotifyPartyName);

			var notifyPartyStreetFilter = result.AddTextFilter(FilterConstants.NotifyPartyStreet, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_NotifyPartyStreet1, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_NotifyPartyStreet1);
			notifyPartyStreetFilter.Category = FilterCategories.Locations;
			notifyPartyStreetFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyStreetFilter", FilterConstants.NotifyPartyStreet);

			var notifyPartyStreet2Filter = result.AddTextFilter(FilterConstants.NotifyPartyStreet2, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_NotifyPartyStreet2, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_NotifyPartyStreet2);
			notifyPartyStreet2Filter.Category = FilterCategories.Locations;
			notifyPartyStreet2Filter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyStreet2Filter", FilterConstants.NotifyPartyStreet2);

			var notifyPartyCityFilter = result.AddTextFilter(FilterConstants.NotifyPartyCity, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_NotifyPartyCity, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_NotifyPartyCity);
			notifyPartyCityFilter.Category = FilterCategories.Locations;
			notifyPartyCityFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyCityFilter", FilterConstants.NotifyPartyCity);

			var notifyPartyStateFilter = result.AddTextFilter(FilterConstants.NotifyPartyState, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_NotifyPartyState, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_NotifyPartyState);
			notifyPartyStateFilter.Category = FilterCategories.Locations;
			notifyPartyStateFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyStateFilter", FilterConstants.NotifyPartyState);

			var notifyPartyCountryFilter = result.AddNkFilter(FilterConstants.NotifyPartyCountry, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_RN_NKNotifyPartyCountry, op), ModuleIDs.RefCountry, new RefCountryCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaBillSchema.ABL_RN_NKNotifyPartyCountry);
			notifyPartyCountryFilter.Category = FilterCategories.Locations;
			notifyPartyCountryFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyCountryFilter", FilterConstants.NotifyPartyCountry);

			var notifyPartyPostcodeFilter = result.AddTextFilter(FilterConstants.NotifyPartyPostcode, (op, value) => GetBillQuery(value, AsycudaBillSchema.ABL_NotifyPartyPostcode, op))
				.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_NotifyPartyPostcode);
			notifyPartyPostcodeFilter.Category = FilterCategories.Locations;
			notifyPartyPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|NotifyPartyPostcodeFilter", FilterConstants.NotifyPartyPostcode);
		}

		void AddPackFilters(ModuleFilterCollection result)
		{
			var packSubGroupFilter = new PackSubGroup();
			var packQtyFilter = result.AddNumberRangeFilter(FilterConstants.PackQty, AsycudaPackSchema.APA_PackQty);
			packQtyFilter.Category = FilterCategories.NumbersAndReferences;
			packQtyFilter.SubGroup = packSubGroupFilter;
			packQtyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackQtyFilter", FilterConstants.PackQty);
			var packQtyUnitFilter = new HideComparisonOperatorModuleTextFilter(FilterConstants.PackQtyUnit, AsycudaPackSchema.APA_PackUQ, Factory.GetPackageTypeList());
			packQtyUnitFilter.WithMaxLengthOf<ModuleTextFilter>(AsycudaPackSchema.APA_PackUQ);
			packQtyUnitFilter.Category = FilterCategories.NumbersAndReferences;
			packQtyUnitFilter.SubGroup = packSubGroupFilter;
			packQtyUnitFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackQtyUnitFilter", FilterConstants.PackQtyUnit);
			result.AddFilter(packQtyUnitFilter);

			var packWeightFilter = result.AddNumberRangeFilter(FilterConstants.PackWeight, AsycudaPackSchema.APA_Weight);
			packWeightFilter.Category = FilterCategories.NumbersAndReferences;
			packWeightFilter.SubGroup = packSubGroupFilter;
			packWeightFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackWeightFilter", FilterConstants.PackWeight);

			var packVolumeFilter = result.AddNumberRangeFilter(FilterConstants.PackVolume, AsycudaPackSchema.APA_Volume);
			packVolumeFilter.Category = FilterCategories.NumbersAndReferences;
			packVolumeFilter.SubGroup = packSubGroupFilter;
			packVolumeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackVolumeFilter", FilterConstants.PackVolume);

			var packGoodDescriptionFilter = result.AddTextFilter(FilterConstants.PackGoodsDescription, AsycudaPackSchema.APA_GoodsDescription);
			packGoodDescriptionFilter.Category = FilterCategories.NumbersAndReferences;
			packGoodDescriptionFilter.SubGroup = packSubGroupFilter;
			packGoodDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackGoodDescriptionFilter", FilterConstants.PackGoodsDescription);

			var packCommodityCodeFilter = result.AddTextFilter(FilterConstants.PackCommodityCode, AsycudaPackSchema.APA_CommodityCode);
			packCommodityCodeFilter.Category = FilterCategories.NumbersAndReferences;
			packCommodityCodeFilter.SubGroup = packSubGroupFilter;
			packCommodityCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCommodityCodeFilter", FilterConstants.PackCommodityCode);
			var linePriceFilter = GetFilterForGenAddOnColumn(FilterConstants.PackLinePrice, Business.AsycudaPack.Schema.LinePrice, AsycudaPackSchema.Constants.Prefix, typeof(AsycudaPack), null, 2);
			linePriceFilter.SubGroup = packSubGroupFilter;
			linePriceFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackLinePriceFilter", FilterConstants.PackLinePrice);
			result.AddFilter(linePriceFilter);

			var packLineSubGroupFilter = new PackLineSubGroup();

			var packMessageStatusFilter = new CountryRelatedFilter(
				FilterConstants.PackMessageStatus,
				GetPackMessageStatusQuery,
				Factory,
				FieldType.TextDropEdit,
				AsycudaPackedItemSchema.API_MessageStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.MessageStatusGetter);
			packMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
			packMessageStatusFilter.SubGroup = packLineSubGroupFilter;
			packMessageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackMessageStatusFilter", FilterConstants.PackMessageStatus);
			result.AddFilter(packMessageStatusFilter);

			var packCustomsStatusFilter = new CountryRelatedFilter(
				FilterConstants.PackCustomsStatus,
				(country, status) =>
				{
					var query = new ZDBOnlyQuery(typeof(AsycudaPackedItem));
					query.AddToFilter(AsycudaPackedItemSchema.API_PackStatus, status);
					return query;
				},
				Factory,
				FieldType.TextDropEdit,
				AsycudaPackedItemSchema.API_PackStatus.MaxLength,
				CountryRelatedFilterHelper.ListGetters.CustomsStatusGetter);
			packCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			packCustomsStatusFilter.SubGroup = packLineSubGroupFilter;
			packCustomsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCustomsStatusFilter", FilterConstants.PackCustomsStatus);
			result.AddFilter(packCustomsStatusFilter);

			var packOriginFilter = result.AddNkFilter(FilterConstants.PackOrigin, (op, value) => GetHeaderQuery(AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, value, op), ModuleIDs.RefCountry, new RefCountryCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(AsycudaPackedItemSchema.API_RN_NKGoodsOrigin);
			packOriginFilter.SubGroup = packLineSubGroupFilter;
			packOriginFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackOriginFilter", FilterConstants.PackOrigin);

			var packGoodsDescriptionFilter = result.AddTextFilter(FilterConstants.PackCustomsDescription, AsycudaPackedItemSchema.API_GoodsDescription);
			packGoodsDescriptionFilter.Category = FilterCategories.NumbersAndReferences;
			packGoodsDescriptionFilter.SubGroup = packLineSubGroupFilter;
			packGoodsDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackGoodsDescriptionFilter", FilterConstants.PackCustomsDescription);

			var packCustomsTariffFilter = result.AddTextFilter(FilterConstants.PackCustomsTariff, AsycudaPackedItemSchema.API_Tariff);
			packCustomsTariffFilter.Category = FilterCategories.NumbersAndReferences;
			packCustomsTariffFilter.SubGroup = packLineSubGroupFilter;
			packCustomsTariffFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCustomsTariffFilter", FilterConstants.PackCustomsTariff);

			var packCustomsQtyFilter = result.AddNumberRangeFilter(FilterConstants.PackCustomsQty, AsycudaPackedItemSchema.API_CustomsQty);
			packCustomsQtyFilter.Category = FilterCategories.NumbersAndReferences;
			packCustomsQtyFilter.SubGroup = packLineSubGroupFilter;
			packCustomsQtyFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCustomsQtyFilter", FilterConstants.PackCustomsQty);

			var packCustomsValueFilter = result.AddNumberRangeFilter(FilterConstants.PackCustomsValue, AsycudaPackedItemSchema.API_CustomsValue);
			packCustomsValueFilter.Category = FilterCategories.FinancialDetails;
			packCustomsValueFilter.SubGroup = packLineSubGroupFilter;
			packCustomsValueFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCustomsValueFilter", FilterConstants.PackCustomsValue);

			var packDutyAmountFilter = result.AddNumberRangeFilter(FilterConstants.PackCustomsDutyAmount, AsycudaPackedItemSchema.API_DutyAmount);
			packDutyAmountFilter.Category = FilterCategories.NumbersAndReferences;
			packDutyAmountFilter.SubGroup = packLineSubGroupFilter;
			packDutyAmountFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCustomsDutyAmountFilter", FilterConstants.PackCustomsDutyAmount);

			var packTaxAmountFilter = result.AddNumberRangeFilter(FilterConstants.PackCustomsTaxAmount, AsycudaPackedItemSchema.API_TaxAmount);
			packTaxAmountFilter.Category = FilterCategories.NumbersAndReferences;
			packTaxAmountFilter.SubGroup = packLineSubGroupFilter;
			packTaxAmountFilter.MultilingualDescription = ResString.GetMultilingualString("ManifestBillFilterStrip|PackCustomsTaxAmountFilter", FilterConstants.PackCustomsTaxAmount);
		}

		protected ModuleFilter GetFilterForGenAddOnColumn(string description, string column, string tablePrefix, Type bizObjectType, SchemaColumn schemaKeyOverride, int noOfDecimalPlaces)
		{
			var filter = new GenAddOnTextNumericFilter(description, this, column, tablePrefix, bizObjectType, schemaKeyOverride, noOfDecimalPlaces)
			{
				MaxLength = GenAddOnColumn.Schema.XA_DataMaxLength
			};
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			return filter;
		}

		ZQuery GetHeaderQuery(SchemaColumn column, IZType value, SQLComparisonOperator operat = null)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);

			if (operat == null)
			{
				headerQuery.AddToFilter(column, value);
			}
			else
			{
				headerQuery.AddToFilter(column, operat, value);
			}
			return GetBillQuery(headerQuery);
		}

		ZQuery GetCustomsJobQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			var referenceDataSubQuery = BillCountryGenAddOnColumnHelper.GetQueryHandlingBlanks(AsycudaBill.Schema.CustomsJobNumber, comparisonOperator, value);
			result.AddToFilter(referenceDataSubQuery);

			return result;
		}

		ZQuery GetBillQuery(ZDBOnlySubQuery headerQuery)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			result.AddSubQuery(AsycudaBillSchema.ABL_AMA, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetBillQuery(IZType value, SchemaColumn column, SQLComparisonOperator operat = null)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));

			if (operat == null)
			{
				result.AddToFilter(column, value);
			}
			else
			{
				result.AddToFilter(column, operat, value);
			}

			return result;
		}

		ZQuery GetOrgQuery(object agentPK, SchemaGuidColumn column, SQLComparisonOperator operat = null)
		{
			var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));

			if (ZGuid.TryParse(agentPK, out var agentGuid))
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);

				if (operat == null)
				{
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, agentGuid);
				}
				else
				{
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, operat, agentGuid);
				}
				billQuery.AddSubQuery(column, OrgAddressSchema.PK, orgAddressQuery, JoinCondition.And);
			}
			else
			{
				if (operat == null)
				{
					billQuery.AddToFilter(column, operat, DBNull.Value);
				}
				else
				{
					billQuery.AddToFilter(column, operat, DBNull.Value);
				}
			}

			return billQuery;
		}

		ZQuery GetBillNumberRangeQuery(INumericZType valueFrom, INumericZType valueTo, SchemaColumn column)
		{
			var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			ModuleNumberRangeFilter.AddToFilters(billQuery, column, valueFrom, valueTo);
			return billQuery;
		}

		ZQuery GetBillQueryForGenAddOn(IZType value, string column, SQLComparisonOperator operat = null)
		{
			var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			var referenceDataSubQuery = BillGenAddOnColumnHelper.GetQueryOnGenAddOnColumn(column, operat, value.ToString());
			billQuery.AddToFilter(referenceDataSubQuery);
			return billQuery;
		}

		ZQuery GetBillRegistrationNumberQuery(SQLComparisonOperator operat, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			bool notIn = false;
			if (operat != null)
			{
				if (operat == SpecialComparisonOperator.IsBlank)
				{
					notIn = true;
				}
			}

			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);

			if (!value.IsEmpty || operat == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, operat, value);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
			}

			result.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetManifestRegistrationNumberQuery(SQLComparisonOperator operat, ZString value)
		{
			bool notIn = false;
			if (operat != null)
			{
				if (operat == SpecialComparisonOperator.IsBlank)
				{
					notIn = true;
				}
			}
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);

			if (!value.IsEmpty || operat == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, operat, value);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
				subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeaderSchema.Constants.TableName);
			}

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_IssueDate);

			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetManifestRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, AsycudaBillSchema.ABL_BillIssueDate, value1, value2);

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA, subQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetBillRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));
			var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBill.Schema.TableName);
			AddDateTimeRange(entryNumQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
			result.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, entryNumQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCarrierQuery(ZGuid value)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var carrierAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), AsycudaManifestHeaderSchema.AMA_OA_Carrier);
			carrierAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
			headerQuery.AddSubQuery(carrierAddressSubQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetManifestMsgStatusQuery(ZString country, ZString status)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);

			if (status == MessageStatusCodeList.Codes.NotSent)
			{
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_MessageStatus, new[] { status, ZString.Empty });
			}
			else
			{
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_MessageStatus, status);
			}

			return GetBillQuery(headerQuery);
		}

		ZQuery GetBillMessageStatusQuery(ZString country, ZString status)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaBill));

			if (status == MessageStatusCodeList.Codes.NotSent)
			{
				result.AddToFilter(AsycudaBillSchema.ABL_MessageStatus, new[] { status, ZString.Empty });
			}
			else
			{
				result.AddToFilter(AsycudaBillSchema.ABL_MessageStatus, status);
			}

			return result;
		}

		ZQuery GetPackMessageStatusQuery(ZString country, ZString status)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaPackedItem));

			if (status == MessageStatusCodeList.Codes.NotSent)
			{
				result.AddToFilter(AsycudaPackedItemSchema.API_MessageStatus, new[] { status, ZString.Empty });
			}
			else
			{
				result.AddToFilter(AsycudaPackedItemSchema.API_MessageStatus, status);
			}

			return result;
		}

		public OrgAddressCollection OrgAddressList => fOrgAddressList ?? (fOrgAddressList = new OrgAddressCollection(Factory));
		OrgAddressCollection fOrgAddressList;

		OrgHeaderCollection OrganizationList => fOrganizationList ?? (fOrganizationList = new OrgHeaderCollection(Factory));
		OrgHeaderCollection fOrganizationList;

		RefCountryCollection CountryList => fCountryList ?? (fCountryList = new RefCountryCollection(Factory));
		RefCountryCollection fCountryList;

		ZQuery GetLoadDischargePortFilter(ZString loadPort, ZString discPort)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

			if (!loadPort.IsEmpty || !discPort.IsEmpty)
			{
				if (!loadPort.IsEmpty)
				{
					bool isCountryCode = loadPort.Length == 2;
					var comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					billQuery.AddToFilter(AsycudaBillSchema.ABL_RL_NKPortOfLoading, comparisonOperator, loadPort);
				}

				if (!discPort.IsEmpty)
				{
					bool isCountryCode = discPort.Length == 2;
					var comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					billQuery.AddToFilter(AsycudaBillSchema.ABL_RL_NKPortOfDischarge, comparisonOperator, discPort);
				}
			}

			headerQuery.AddSubQuery(billQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			billQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, comparisonOperator, value);
			return billQuery;
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, comparisonOperator, value);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA, subQuery, JoinCondition.And);
			return GetBillQuery(headerQuery);
		}

		ZQuery GetMasterBillDateQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			billQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					billQuery.AddToFilter(column, value1);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					billQuery.AddToFilter(column, ZDateTime.Empty);
					break;
				default:
					AddDateTimeRange(billQuery, comparisonOperator, JoinCondition.And, column, value1, value2);
					break;
			}

			headerQuery.AddSubQuery(billQuery, JoinCondition.And);

			return GetBillQuery(headerQuery);
		}

		ZQuery GetActiveStatusOfManifestAndBillQuery(ZQuery query, ZString status)
		{
			var dbQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbQuery.AddToFilter(query);

			var parentSubquery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaBillSchema.ABL_AMA);

			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				parentSubquery.AddToFilter(AsycudaManifestHeaderSchema.AMA_IsActive, !ReverseBoolsForActiveStatusFilter);
				parentSubquery.IgnoreActiveFilter = true;
				dbQuery.AddSubQuery(parentSubquery, JoinCondition.Or);
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				parentSubquery.AddToFilter(AsycudaManifestHeaderSchema.AMA_IsActive, ReverseBoolsForActiveStatusFilter);
				parentSubquery.IgnoreActiveFilter = false;
				dbQuery.AddSubQuery(parentSubquery, JoinCondition.And);
			}

			return dbQuery;
		}

		ZDBOnlyQuery GetLocalReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.LocalReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);

			result.AddSubQuery(AsycudaManifestHeaderSchema.PK, cusEntryNumQuery, JoinCondition.And);

			return result;
		}

		LocationCollection LocationList => _fLocationList ?? (_fLocationList = new LocationCollection(Factory));
		LocationCollection _fLocationList;

		ZDBOnlyQuery GetShippingAgentAddressQuery(ZGuid agentPK)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			subQuery.AddToFilter(OrgAddressSchema.PK, agentPK);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent, OrgAddressSchema.PK, subQuery, JoinCondition.And);
			var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			billQuery.AddSubQuery(AsycudaBillSchema.ABL_AMA, AsycudaManifestHeaderSchema.PK, headerQuery, JoinCondition.And);
			return billQuery;
		}

		GenAddOnColumnQueryHelper billCountryGenAddOnColumnHelper;
		protected GenAddOnColumnQueryHelper BillCountryGenAddOnColumnHelper =>
			billCountryGenAddOnColumnHelper
			?? (billCountryGenAddOnColumnHelper = new GenAddOnColumnQueryHelper(typeof(AsycudaBill)));

		GenAddOnColumnQueryHelper billGenAddOnColumnHelper;
		GenAddOnColumnQueryHelper BillGenAddOnColumnHelper => billGenAddOnColumnHelper ?? (billGenAddOnColumnHelper = new GenAddOnColumnQueryHelper(typeof(AsycudaBill)));

		GenAddOnColumnQueryHelper packLineGenAddOnHelper;
		protected GenAddOnColumnQueryHelper PackLineGenAddOnHelper => packLineGenAddOnHelper ?? (packLineGenAddOnHelper = new GenAddOnColumnQueryHelper(typeof(AsycudaPackedItem)));

		#region Sub Group Filters

		abstract class AsycudaModuleFilterSubGroup : ModuleFilterSubGroup
		{
			protected ZQuery GetBillCountryQuery(ZDBOnlySubQuery headerQuery)
			{
				var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				billQuery.AddSubQuery(AsycudaBillSchema.ABL_AMA, AsycudaManifestHeaderSchema.PK, headerQuery, JoinCondition.And);
				return billQuery;
			}
		}

		class HeaderQuerySubGroup : AsycudaModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
				headerQuery.AddToFilter(filter);
				return GetBillCountryQuery(headerQuery);
			}
		}

		class PackSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZQuery();

				var additionalSql = Invariant($@"{AsycudaBillSchema.Constants.PK} in (
										SELECT {AsycudaPackSchema.Constants.APA_ABL_Bill} from {AsycudaPackSchema.Constants.SqlSchemaName}.{AsycudaPackSchema.Constants.TableName} {filter.GetAsWhereClause(true)})");

				query.AddFilterAndZSQLParameterCollection(additionalSql, new ZSqlParameterCollection());

				return query;
			}
		}

		class PackLineSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZQuery();

				var additionalSql = Invariant($@"{AsycudaBillSchema.Constants.PK} in (
										SELECT {AsycudaPackSchema.Constants.APA_ABL_Bill} from {AsycudaPackSchema.Constants.SqlSchemaName}.{AsycudaPackSchema.Constants.TableName}
										INNER JOIN {AsycudaPackPackedItemPivotSchema.Constants.SqlSchemaName}.{AsycudaPackPackedItemPivotSchema.Constants.TableName} ON {AsycudaPackPackedItemPivotSchema.Constants.APP_APA_Pack} = {AsycudaPackSchema.Constants.PK}
										INNER JOIN {AsycudaPackedItemSchema.Constants.SqlSchemaName}.{AsycudaPackedItemSchema.Constants.TableName} ON {AsycudaPackPackedItemPivotSchema.Constants.APP_API_Item} = {AsycudaPackedItemSchema.Constants.PK} 
										{filter.GetAsWhereClause(true)})");

				query.AddFilterAndZSQLParameterCollection(additionalSql, new ZSqlParameterCollection());

				return query;
			}
		}

		class CusEntryPackedItemFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZQuery();

				var additionalSql = Invariant($@"{AsycudaBillSchema.Constants.PK} in (
									SELECT {AsycudaPackSchema.Constants.APA_ABL_Bill} from {AsycudaPackSchema.Constants.SqlSchemaName}.{AsycudaPackSchema.Constants.TableName}
									INNER JOIN {AsycudaPackPackedItemPivotSchema.Constants.SqlSchemaName}.{AsycudaPackPackedItemPivotSchema.Constants.TableName} ON {AsycudaPackPackedItemPivotSchema.Constants.APP_APA_Pack} = {AsycudaPackSchema.Constants.PK}
									INNER JOIN {AsycudaPackedItemSchema.Constants.SqlSchemaName}.{AsycudaPackedItemSchema.Constants.TableName} ON {AsycudaPackPackedItemPivotSchema.Constants.APP_API_Item} = {AsycudaPackedItemSchema.Constants.PK} 
									INNER JOIN {CusEntryNumSchema.Constants.SqlSchemaName}.{CusEntryNumSchema.Constants.TableName} ON {CusEntryNumSchema.Constants.CE_ParentID} = {AsycudaPackedItemSchema.Constants.PK} 
										AND {CusEntryNumSchema.Constants.CE_ParentTable} = '{AsycudaPackedItemSchema.Constants.TableName}' {filter.GetAsWhereClause(true)})");

				query.AddFilterAndZSQLParameterCollection(additionalSql, new ZSqlParameterCollection());

				return query;
			}
		}

		class CusEntryNumFilterSubGroup : ModuleFilterSubGroup
		{
			public CusEntryNumFilterSubGroup(ModuleTextFilter customsNumberFilter = null) : base()
			{
				this.customsNumberFilter = customsNumberFilter;
			}

			readonly ModuleTextFilter customsNumberFilter;

			bool IsNotIn
			{
				get
				{
					switch (customsNumberFilter?.ComparisonOperator)
					{
						case ModuleTextFilter.ComparisonConstants.IsBlank:
						case ModuleTextFilter.ComparisonConstants.NotContain:
						case ModuleTextFilter.ComparisonConstants.NotEqual:
						case ModuleTextFilter.ComparisonConstants.NotStartsWith:
							return true;
						default:
							return false;
					}
				}
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(AsycudaBill));

				var entrynumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entrynumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBill.Schema.TableName);
				entrynumQuery.AddToFilter(filter);
				result.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, entrynumQuery, JoinCondition.And);

				if (IsNotIn)
				{
					var notLinkedEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn: true);
					notLinkedEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBill.Schema.TableName);
					result.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, notLinkedEntryNumQuery, JoinCondition.Or);
				}

				return result;
			}
		}

		class ShippingAgentNameFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var headerQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

				subQuery.AddToFilter(filter);
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, OrgHeaderSchema.PK, subQuery, JoinCondition.And);
				headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent, OrgAddressSchema.PK, orgAddressQuery, JoinCondition.And);

				var billQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
				billQuery.AddSubQuery(AsycudaBillSchema.ABL_AMA, AsycudaManifestHeaderSchema.PK, headerQuery, JoinCondition.And);
				return billQuery;
			}
		}

		#endregion
	}
}
