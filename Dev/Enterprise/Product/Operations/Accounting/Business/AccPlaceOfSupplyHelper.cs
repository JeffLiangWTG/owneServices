using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public static class AccPlaceOfSupplyHelper
	{
		public static (ZString posType, ZString posCode) GetPlaceOfSupplyFromConfiguration(IJobInvoicingPlugIn invoicingPlugIn, AccChargeCode chargeCode, CostSell costSell, OrgHeader organization, ZString supplyType, GlbBranch branch)
		{
			Argument.NotNull(invoicingPlugIn, nameof(invoicingPlugIn));
			Argument.NotNull(invoicingPlugIn.InvoicingSupporter, nameof(invoicingPlugIn.InvoicingSupporter));
			Argument.NotNull(chargeCode, nameof(chargeCode));
			Argument.NotNull(organization, nameof(organization));

			if (!PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(chargeCode.Company))
			{
				return (ZString.Empty, ZString.Empty);
			}

			var posLocation = GetILocationFromPOSConfiguration(invoicingPlugIn, chargeCode, costSell, organization, supplyType, branch);
			return GetPlaceOfSupplyFromILocation(posLocation, chargeCode.Company);
		}

		public static (ZString posType, ZString posCode) GetPlaceOfSupplyFromConfiguration(IJobCostingPlugIn costingPlugIn, AccChargeCode chargeCode, OrgHeader creditor, ZString costSupplyType)
		{
			Argument.NotNull(costingPlugIn, nameof(costingPlugIn));
			Argument.NotNull(chargeCode, nameof(chargeCode));
			Argument.NotNull(creditor, nameof(creditor));

			if (!PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(chargeCode.Company))
			{
				return (ZString.Empty, ZString.Empty);
			}

			var posLocation = GetILocationFromPOSConfiguration(costingPlugIn, chargeCode, creditor, costSupplyType);
			return GetPlaceOfSupplyFromILocation(posLocation, chargeCode.Company);
		}

		public static (ZString posType, ZString posCode) GetPlaceOfSupplyFromConfiguration(AccChargeCode chargeCode, CostSell costSell, OrgHeader organization, ZString supplyType, GlbBranch branch)
		{
			Argument.NotNull(chargeCode, nameof(chargeCode));
			Argument.NotNull(organization, nameof(organization));

			if (!PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(chargeCode.Company))
			{
				return (ZString.Empty, ZString.Empty);
			}

			var posLocation = GetILocationFromPOSConfiguration(chargeCode, costSell, organization, supplyType, branch);
			return GetPlaceOfSupplyFromILocation(posLocation, chargeCode.Company);
		}

		#region LookupPlaceOfSupplyRule()

		public static AccPOSConfiguration LookupPlaceOfSupplyRule(AccChargeCode chargeCode, IJobInvoicingPlugIn jobPlugin, CostSell costOrSell, OrgHeader organization, ZString supplyType, GlbBranch branch = null)
		{
			Argument.NotNull(chargeCode, nameof(chargeCode));
			Argument.NotNull(jobPlugin, nameof(jobPlugin));
			Argument.NotNull(jobPlugin.InvoicingSupporter, nameof(jobPlugin.InvoicingSupporter));
			Argument.NotNull(organization, nameof(organization));

			var jobType = jobPlugin.InvoicingSupporter.ConsumerType.Code;
			var direction = jobPlugin.InvoicingSupporter.GetJobDirection();
			var transportMode = jobPlugin.InvoicingSupporter.TransportMode;
			var incoTerm = jobPlugin.GetINCOTermCode();

			return LookupPlaceOfSupplyRuleInternal(chargeCode, jobType, direction, transportMode, incoTerm, costOrSell, organization, supplyType, branch);
		}

		public static AccPOSConfiguration LookupPlaceOfSupplyRule(AccChargeCode chargeCode, IJobCostingPlugIn costingPlugIn, OrgHeader creditor, ZString costSupplyType, GlbBranch branch = null)
		{
			Argument.NotNull(chargeCode, nameof(chargeCode));
			Argument.NotNull(costingPlugIn, nameof(costingPlugIn));
			Argument.NotNull(creditor, nameof(creditor));

			if (costingPlugIn is IJobInvoicingPlugIn jobPlugin)
			{
				return LookupPlaceOfSupplyRule(chargeCode, jobPlugin, CostSell.Cost, creditor, costSupplyType, branch);
			}
			// IJobCostingPlugIn does not have an equivalent to InvoicingSupporter.ConsumerType. We cannot determine the job type in this case.
			// As all supported job types for FPOS defaulting rules implement IJobInvoicingPlugIn, this always returns null.
			return null;
		}

		public static AccPOSConfiguration LookupPlaceOfSupplyRule(AccChargeCode chargeCode, CostSell costOrSell, OrgHeader organization, ZString supplyType, GlbBranch branch = null)
		{
			Argument.NotNull(chargeCode, nameof(chargeCode));
			Argument.NotNull(organization, nameof(organization));

			var jobType = "ALL";    // Could not find a good way to get it as AllJobsConsumerType is internal to MasterFiles.Business

			return LookupPlaceOfSupplyRuleInternal(chargeCode, jobType, null, string.Empty, string.Empty, costOrSell, organization, supplyType, branch);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		static AccPOSConfiguration LookupPlaceOfSupplyRuleInternal(AccChargeCode chargeCode, string jobType, Directions? direction, string transportMode, string incoTerm, CostSell costOrSell, OrgHeader organization, ZString supplyType, GlbBranch branch = null)
		{
			var company = branch?.Company ?? chargeCode?.Company ?? GlbCompany.CurrentCompany;
			var group = chargeCode.PlaceOfSupplyGroup;
			var chargeType = costOrSell == CostSell.Revenue ? AccPOSChargeTypeList.Codes.Revenue
						   : costOrSell == CostSell.Cost ? AccPOSChargeTypeList.Codes.Cost
						   : throw new ArgumentException("Unsupported value of costOrSell: " + costOrSell, nameof(costOrSell));
			var directionCode = direction == null ? FreightShipmentDirection.Code.All
							  : direction == Directions.Import ? FreightShipmentDirection.Code.Import
							  : direction == Directions.Export ? FreightShipmentDirection.Code.Export
							  : direction == Directions.Domestic ? FreightShipmentDirection.Code.Domestic
							  : FreightShipmentDirection.Code.Other;
			var taxRegistrationCode = organization.PlaceOfSupplyTaxRegistrationCode(company);
			var includeBranch = AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty)
							  && branch != null;
			var branchCode = includeBranch ? branch.GB_Code : ZString.Empty;
			var parentIdList = new List<ZGuid> { chargeCode.PK, ZGuid.Empty };

			var traceLogBuilder = new StringBuilder();
			traceLogBuilder.AppendLine(TraceMessages.BuildChargeCodeInfoTraceMessage("Charge Code", company.GC_Code, chargeCode));

			if (group != null)
			{
				parentIdList.Add(group.PK);
				traceLogBuilder.AppendLine(TraceMessages.BuildChargeCodeInfoTraceMessage("Charge Code Group", company.GC_Code, null, group));
			}

			var factory = chargeCode.Factory;
			var cacheKey = $"{company.GC_Code}.{chargeCode.AC_Code}.{nameof(LookupPlaceOfSupplyRule)}";
			traceLogBuilder.AppendLine(TraceMessages.BuildLookupPlaceOfSupplyRuleTraceMessage("POS Configuration selection parameters", CreatePlaceOfSupplyConfigurationDetails()));

			var candidateRuleCollection = factory.GetCachedValue(cacheKey, () =>
			{
				var rules = new AccPOSConfigurationCollection(chargeCode);
				rules.Load();
				rules.SortByPrioritySpecificToGeneric();
				traceLogBuilder.AppendLine(TraceMessages.BuildCandidatePlaceOfSupplyRuleCollection($"POS Configurations for Charge Code '{chargeCode.AC_Code}'", rules));
				return rules;
			}, CacheStalenessPolicy.NeverStale);

			var result = candidateRuleCollection.LookupBestMatch(parentIdList, jobType, chargeType, incoTerm, directionCode, transportMode, taxRegistrationCode, branchCode, supplyType);

			traceLogBuilder.AppendLine(TraceMessages.BuildLookupPlaceOfSupplyResultRuleTraceMessage("Best matching POS Configuration", result));

			chargeCode.GetTracer().TraceVerbose(AccountingTraceSourceCodes.FPOS, () => traceLogBuilder.ToString());

			#region Create FPOSTracePlaceOfSupplyConfigurationDetailsInfo

			FPOSTracePlaceOfSupplyConfigurationDetailsInfo CreatePlaceOfSupplyConfigurationDetails()
			{
				return new FPOSTracePlaceOfSupplyConfigurationDetailsInfo()
				{
					BranchCode = branchCode,
					GroupCode = group?.GRO_Code,
					JobType = jobType,
					ChargeType = chargeType,
					IncoTerms = incoTerm,
					Direction = directionCode,
					TransportMode = transportMode,
					TaxRegistration = taxRegistrationCode,
					SupplyType = supplyType,
				};
			}

			#endregion

			return result;
		}

		#endregion

		public static ZString PlaceOfSupplyTaxRegistrationCode(this OrgHeader org, GlbCompany currentCompany = null)
		{
			if (org == null)
			{
				return ZString.Empty;
			}
			var company = currentCompany ?? GlbCompany.CurrentCompany;

			var vatCode = Country.GetConsumptionTaxRegistrationOrgCusCode(company.GC_RN_NKCountryCode);
			var orgCusCodePredicateProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIOrgCusCodePredicateProvider(company.GC_RN_NKCountryCode);

			var orgIsRegisteredForVatInCompanyCountry = org.CustomsCodes
															?.Cast<OrgCusCode>()
															?.Any(c => IsOrgCusCodeValidForCompanyCountry(c, orgCusCodePredicateProvider, company, vatCode))
															?? false;
			var orgIsLocalToCompanyCountry = org.CountryCode.EqualsIgnoringCase(company.GC_RN_NKCountryCode);

			return orgIsRegisteredForVatInCompanyCountry ? AccPOSTaxRegistrationList.Codes.LocalCustomer
				: !orgIsRegisteredForVatInCompanyCountry && orgIsLocalToCompanyCountry ? AccPOSTaxRegistrationList.Codes.Unregistered
				: !orgIsRegisteredForVatInCompanyCountry && !orgIsLocalToCompanyCountry ? AccPOSTaxRegistrationList.Codes.ForeignOrganization
				: throw new InvalidOperationException($"Invalid state in {nameof(PlaceOfSupplyTaxRegistrationCode)}: {orgIsRegisteredForVatInCompanyCountry}, {orgIsLocalToCompanyCountry}");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationFromPOSConfiguration(IJobInvoicingPlugIn invoicingPlugIn, AccChargeCode chargeCode, CostSell costSell, OrgHeader org, ZString supplyType, GlbBranch branch)
		{
			ILocation result = null;
			const string traceStepTitle = "Get Location from IJobInvoicingPlugIn";
			var applicableBranch = branch ?? GlbBranch.CurrentBranch;
			var config = LookupPlaceOfSupplyRule(chargeCode, invoicingPlugIn, costSell, org, supplyType, applicableBranch);
			var posRule = config?.PSC_PlaceOfSupplyRule ?? GetDefaultPosRule(applicableBranch.Company);

			if (posRule == AccPOSRuleList.Codes.OtherTerritories)
			{
				var location = new LocationRule(applicableBranch.Company, PlaceOfSupplyListProvider.Codes.OtherTerritories);
				chargeCode.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage(traceStepTitle + " - OtherTerritories rule", location, PlaceOfSupplyListProvider.Codes.OtherTerritories));
				return location;
			}

			var allConsols = invoicingPlugIn.GetAllLinkedConsols().OfType<IJobCostingPlugIn>().ToList();
			var costingPlugIn = allConsols.Count == 1 ? allConsols.First() : null;

			var invoicingSupporter = invoicingPlugIn?.InvoicingSupporter;

			if (costingPlugIn is CommonConsol consol)
			{
				result = GetILocationForPOSConfiguration(consol, posRule);
			}

			if (result == null && costingPlugIn != null)
			{
				result = GetILocationForPOSConfiguration(costingPlugIn, posRule);
			}

			if (result == null && invoicingPlugIn is BaseJobDeclaration declaration)
			{
				result = GetILocationForPOSConfiguration(declaration, posRule);
			}

			if (result == null && invoicingPlugIn is IConfirmAddressParent addressParent)
			{
				result = GetILocationForPOSConfiguration(addressParent, posRule);
			}

			if (result == null && invoicingPlugIn is ITransitWarehouseInstructionSupporter transitWarehouseSupporter)
			{
				result = GetILocationForPOSConfiguration(transitWarehouseSupporter, posRule);
			}

			if (result == null && invoicingPlugIn is IRoutingSupport routingSupporter)
			{
				result = GetILocationForPOSConfiguration(routingSupporter, posRule);
			}

			if (result == null && invoicingPlugIn is ICO2eLegBasedSupporter legBasedSupporter)
			{
				result = GetILocationForPOSConfiguration(legBasedSupporter, posRule);
			}

			if (result == null && invoicingPlugIn is ILocation locationBased)
			{
				result = GetILocationForPOSConfiguration(locationBased, posRule);
			}

			if (result == null && invoicingSupporter != null)
			{
				result = GetILocationForPOSConfiguration(invoicingSupporter, posRule);
			}

			if (!posRule.IsEmpty && result.IsNullOrEmpty()) // Fallback logic when we could not get a valid ILocation
			{
				if (posRule == AccPOSRuleList.Codes.SupplierLocation)
				{
					result = costSell == CostSell.Revenue ? applicableBranch.OrgProxy?.MainAddress : org.MainAddress;
				}

				if (posRule == AccPOSRuleList.Codes.BillToPartyLocation
					|| (result.IsNullOrEmpty()
						&& GetPlaceOfSupplyWhenMatchingRulesButMissingLocation(applicableBranch.Company) == AccPOSRuleList.Codes.BillToPartyLocation))
				{
					result = costSell == CostSell.Revenue ? org.MainAddress : applicableBranch.OrgProxy?.MainAddress;
				}
				chargeCode.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage(traceStepTitle + buildFallBackInfo(), result, posRule));

				string buildFallBackInfo()
				{
					var info = " - Fallback when could not get ILocation";
					if (!result.IsNullOrEmpty())
					{
						if (result == applicableBranch.OrgProxy?.MainAddress)
						{
							return info + " to Branch address";
						}
						else if (result == org.MainAddress)
						{
							return info += costSell == CostSell.Revenue ? " to Debtor address" : " to Creditor address";
						}
					}
					return info;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(IJobInvoicingSupporter invoicingSupporter, ZString posRule)
		{
			Argument.NotNull(invoicingSupporter, nameof(invoicingSupporter));
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.PickupAgent:
					location = invoicingSupporter.PickUpAgent?.GetAddressWithFallback(ZArchitecture.Business.AddressType.PIC);
					break;
				case AccPOSRuleList.Codes.DeliveryAgent:
					location = invoicingSupporter.DeliveryAgent?.GetAddressWithFallback(ZArchitecture.Business.AddressType.DLV);
					break;
				case AccPOSRuleList.Codes.Origin:
					location = invoicingSupporter.Origin;
					break;
				case AccPOSRuleList.Codes.Destination:
					location = invoicingSupporter.Destination;
					break;
				default:
					break;
			}

			GlbCompany.CurrentCompany.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from IJobInvoicingSupporter", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(IConfirmAddressParent addressParent, ZString posRule)
		{
			Argument.NotNull(addressParent, nameof(addressParent));
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.PickupLocation:
					location = addressParent.GetConsignorPickupDocAddress;
					break;
				case AccPOSRuleList.Codes.DeliveryLocation:
					location = addressParent.GetConsigneeDeliveryDocAddress;
					break;
				case AccPOSRuleList.Codes.PickupCFS:
					location = addressParent.GetDepartureCFSDocAddress;
					break;
				case AccPOSRuleList.Codes.PickupCTO:
					location = !addressParent.GetDepartureCTODocAddress.IsNullOrEmpty()
						? addressParent.GetDepartureCTODocAddress
						: addressParent.GetDepartureCFSDocAddress;
					break;
				case AccPOSRuleList.Codes.DeliveryCFS:
					location = addressParent.GetArrivalCFSDocAddress;
					break;
				case AccPOSRuleList.Codes.DeliveryCTO:
					location = !addressParent.GetArrivalCTODocAddress.IsNullOrEmpty()
						? addressParent.GetArrivalCTODocAddress
						: addressParent.GetArrivalCFSDocAddress;
					break;
				default:
					break;
			}

			GlbCompany.CurrentCompany.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from IConfirmAddressParent", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(IRoutingSupport routingSupporter, ZString posRule)
		{
			Argument.NotNull(routingSupporter, nameof(routingSupporter));
			var currentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry:
					location = routingSupporter.Transports.OfType<Transport>().OrderBy(x => x.JW_LegOrder)
						.FirstOrDefault(x => (x.LoadPort?.RL_RN_NKCountryCode ?? ZString.Empty) == currentCompanyCountryCode)?.LoadPort;
					break;
				case AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry:
					location = routingSupporter.Transports.OfType<Transport>().OrderByDescending(x => x.JW_LegOrder)
						.FirstOrDefault(x => (x.DiscPort?.RL_RN_NKCountryCode ?? ZString.Empty) == currentCompanyCountryCode)?.DiscPort;
					break;
				default:
					break;
			}

			((IBusiness)routingSupporter).GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from IRoutingSupport", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(ITransitWarehouseInstructionSupporter transitWarehouseSupporter, ZString posRule)
		{
			Argument.NotNull(transitWarehouseSupporter, nameof(transitWarehouseSupporter));
			var currentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.PickupTransitWarehouse:
					location = transitWarehouseSupporter.PickupTransitWarehouse;
					break;
				case AccPOSRuleList.Codes.DeliveryTransitWarehouse:
					location = transitWarehouseSupporter.DeliveryTransitWarehouse;
					break;
				default:
					break;
			}

			((IBusiness)transitWarehouseSupporter).GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from ITransitWarehouseInstructionSupporter", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(IJobCostingPlugIn costingPlugIn, ZString posRule)
		{
			Argument.NotNull(costingPlugIn, nameof(costingPlugIn));
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.ConsolPortOfLoading:
					location = costingPlugIn.LoadPort;
					break;
				case AccPOSRuleList.Codes.ConsolPortOfDischarge:
					location = costingPlugIn.DischargePort;
					break;
				case AccPOSRuleList.Codes.ConsolSendingAgent:
					location = costingPlugIn.SendingAgent?.MainAddress;
					break;
				case AccPOSRuleList.Codes.ConsolReceivingAgent:
					location = costingPlugIn.ReceivingAgent?.MainAddress;
					break;
				default:
					break;
			}

			((IBusiness)costingPlugIn).GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from IJobCostingPlugIn", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(CommonConsol consol, ZString posRule)
		{
			Argument.NotNull(consol, nameof(consol));
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.ConsolSendingAgent:
					location = consol.SendingForwarderAddress;
					break;
				case AccPOSRuleList.Codes.ConsolReceivingAgent:
					location = consol.ReceivingForwarderAddress;
					break;
				case AccPOSRuleList.Codes.DepartureCFS:
					location = consol.PackDepotAddress;
					break;
				case AccPOSRuleList.Codes.ArrivalCFS:
					location = consol.UnpackDepotAddress;
					break;
				case AccPOSRuleList.Codes.DepartureCTO:
					location = consol.DepartureCTOAddress;
					break;
				case AccPOSRuleList.Codes.ArrivalCTO:
					location = consol.ArrivalCTOAddress;
					break;
				default:
					break;
			}

			consol.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from CommonConsol", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationForPOSConfiguration(BaseJobDeclaration declaration, ZString posRule)
		{
			Argument.NotNull(declaration, nameof(declaration));
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.PickupLocation:
					location = declaration.SupplierPickupAddress;
					break;
				case AccPOSRuleList.Codes.DeliveryLocation:
					location = declaration.ImporterDeliveryAddress;
					break;
				case AccPOSRuleList.Codes.PortOfLoading:
					location = declaration.PortOfLoading;
					break;
				case AccPOSRuleList.Codes.PortOfDischarge:
					location = declaration.PortOfArrival;
					break;
				case AccPOSRuleList.Codes.PortOfFirstArrival:
					location = declaration.PortOfFirstArrival;
					break;
				case AccPOSRuleList.Codes.PortOfOrigin:
					location = declaration.Origin;
					break;
				case AccPOSRuleList.Codes.FinalDestination:
					location = declaration.FinalDestination;
					break;
				default:
					break;
			}

			declaration.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from BaseJobDeclaration", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationFromPOSConfiguration(IJobCostingPlugIn costingPlugIn, AccChargeCode chargeCode, OrgHeader creditor, ZString costSupplyType)
		{
			ILocation result = null;

			var branch = GlbBranch.CurrentBranch;
			var company = branch?.Company ?? chargeCode?.Company ?? GlbCompany.CurrentCompany;
			var config = LookupPlaceOfSupplyRule(chargeCode, costingPlugIn, creditor, costSupplyType, branch);
			var posRule = config != null ? config.PSC_PlaceOfSupplyRule : GetDefaultPosRule(company);

			if (costingPlugIn is CommonConsol consol)
			{
				result = GetILocationForPOSConfiguration(consol, posRule);
			}

			if (result == null)
			{
				result = GetILocationForPOSConfiguration(costingPlugIn, posRule);
			}

			if (!posRule.IsEmpty && result.IsNullOrEmpty()) // Fallback logic when we could not get a valid ILocation
			{
				if (posRule == AccPOSRuleList.Codes.SupplierLocation)
				{
					result = creditor.MainAddress;
				}

				if (posRule == AccPOSRuleList.Codes.BillToPartyLocation
					|| (result.IsNullOrEmpty()
						&& GetPlaceOfSupplyWhenMatchingRulesButMissingLocation(company) == AccPOSRuleList.Codes.BillToPartyLocation))
				{
					result = branch.OrgProxy?.MainAddress;
				}

				((IBusiness)costingPlugIn).GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location from IJobCostingPlugIn" + buildFallBackInfo(), result, posRule));

				string buildFallBackInfo()
				{
					var info = " - Fallback when could not get ILocation";
					if (!result.IsNullOrEmpty())
					{
						if (result == branch.OrgProxy?.MainAddress)
						{
							return info + " to Branch address";
						}
						else if (result == creditor.MainAddress)
						{
							return info += " to Creditor address";
						}
					}
					return info;
				}
			}

			return result;
		}

		internal static ILocation GetILocationForPOSConfiguration(ICO2eLegBasedSupporter legBasedSupporter, ZString posRule)
		{
			Argument.NotNull(legBasedSupporter, nameof(legBasedSupporter));
			ILocation location = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.Load:
					location = legBasedSupporter.LoadPort;
					break;
				case AccPOSRuleList.Codes.Discharge:
					location = legBasedSupporter.DischargePort;
					break;
				default:
					break;
			}

			((IBusiness)legBasedSupporter).GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage((NoResString)"Get Location from ICO2eLegBasedSupporter", location, posRule));
			return location;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static ILocation GetILocationFromPOSConfiguration(AccChargeCode chargeCode, CostSell costSell, OrgHeader org, ZString supplyType, GlbBranch branch)
		{
			var applicableBranch = branch ?? GlbBranch.CurrentBranch;

			var config = LookupPlaceOfSupplyRule(chargeCode, costSell, org, supplyType, applicableBranch);
			var posRule = config?.PSC_PlaceOfSupplyRule ?? GetDefaultPosRule(applicableBranch.Company);
			ILocation billToPartyLocation = costSell == CostSell.Revenue ? org.MainAddress : applicableBranch.OrgProxy?.MainAddress;
			ILocation supplierPartyLocation = costSell == CostSell.Revenue ? applicableBranch.OrgProxy?.MainAddress : org.MainAddress;
			ILocation result = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.BillToPartyLocation:
					{
						result = billToPartyLocation;
						break;
					}
				case AccPOSRuleList.Codes.SupplierLocation:
					{
						result = supplierPartyLocation?.OrFallbackOnEmpty(billToPartyLocation) ?? billToPartyLocation;
						break;
					}
				case AccPOSRuleList.Codes.OtherTerritories:
					{
						result = new LocationRule(applicableBranch.Company, PlaceOfSupplyListProvider.Codes.OtherTerritories);
						break;
					}
				default:
					break;
			}

			chargeCode.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage("Get Location for Charge Code and Supply Type", result, posRule));
			return result;
		}

		internal static ILocation GetILocationForPOSConfiguration(ILocation locationBased, ZString posRule)
		{
			Argument.NotNull(locationBased, nameof(locationBased));
			ILocation result = null;

			switch (posRule)
			{
				case AccPOSRuleList.Codes.CountryRegionPort:
					result = locationBased;
					break;
				default:
					break;
			}

			((IBusiness)locationBased).GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildLocationFromPOSConfigurationTraceMessage((NoResString)"Get Location from ILocation", result, posRule));
			return result;
		}

		static ILocation OrFallbackOnEmpty(this ILocation primary, ILocation fallback)
			=> !primary.IsNullOrEmpty()
			? primary
			: fallback;

		static bool IsNullOrEmpty(this ILocation location) => location == null || (location.CityTown == null && location.Country == null && location.State == null && location.UNLOCO == null);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		internal static (ZString posType, ZString posCode) GetPlaceOfSupplyFromILocation(ILocation location, GlbCompany company)
		{
			var companyWithFallback = company ?? GlbCompany.CurrentCompany;
			const string traceStepDescription = "Get POS from Location";
			if (location.IsNullOrEmpty() && !(location is LocationRule))
			{
				companyWithFallback.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription + " - Empty Location", string.Empty, string.Empty, string.Empty));
				return (ZString.Empty, ZString.Empty);
			}

			var companyCountryCode = companyWithFallback.Country?.RN_Code;
			var posTypesEnabled = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.GetValueWithoutFallback(companyWithFallback.PK.ToGuid(), Guid.Empty, Guid.Empty);

			if (posTypesEnabled.GetBoolFromCode(PlaceOfSupplyTypes.State.Code)
				&& companyCountryCode.HasValue && companyCountryCode.Value.EqualsIgnoringCase(location.Country?.RN_Code)
				&& location.State != null)
			{
				company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription, PlaceOfSupplyTypes.State.Code, location.State.RW_Code, PlaceOfSupplyTypes.State.Description));
				return (PlaceOfSupplyTypes.State.Code, location.State.RW_Code);
			}

			if (posTypesEnabled.GetBoolFromCode(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var bestTaxZone = location.Zones
										.Where(z => z.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Tax)
										.OrderByDescending(z => z.IsActive)
										.FirstOrDefault();
				if (bestTaxZone != null)
				{
					company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription, PlaceOfSupplyTypes.TaxZone.Code, bestTaxZone.Code, PlaceOfSupplyTypes.TaxZone.Description));
					return (PlaceOfSupplyTypes.TaxZone.Code, bestTaxZone.Code);
				}
			}

			if (posTypesEnabled.GetBoolFromCode(PlaceOfSupplyTypes.Country.Code)
				&& !(location.Country?.RN_Code.IsEmpty ?? true))
			{
				company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription, PlaceOfSupplyTypes.Country.Code, location.Country.RN_Code, PlaceOfSupplyTypes.Country.Description));
				return (PlaceOfSupplyTypes.Country.Code, location.Country.RN_Code);
			}

			if (posTypesEnabled.GetBoolFromCode(PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				if (location.Country == null
					&& location.IsActive
					&& location.Code.EqualsIgnoringCase(PlaceOfSupplyListProvider.Codes.OtherTerritories))
				{
					company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription, PlaceOfSupplyTypes.PredefinedRule.Code, PlaceOfSupplyListProvider.Codes.OtherTerritories, PlaceOfSupplyTypes.PredefinedRule.Description));
					return (PlaceOfSupplyTypes.PredefinedRule.Code, PlaceOfSupplyListProvider.Codes.OtherTerritories);
				}

				if (companyCountryCode.HasValue && !companyCountryCode.Value.EqualsIgnoringCase(location.Country?.RN_Code))
				{
					company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription, PlaceOfSupplyTypes.PredefinedRule.Code, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, PlaceOfSupplyTypes.PredefinedRule.Description));
					return (PlaceOfSupplyTypes.PredefinedRule.Code, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
				}
			}

			company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSFromLocationTraceMessage(traceStepDescription + " - Unable to get POS value", string.Empty, string.Empty));
			return (ZString.Empty, ZString.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tracing")]
		static ZString GetDefaultPosRule(GlbCompany company)
		{
			string result = string.Empty;
			string registryValue = AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			switch (registryValue)
			{
				case AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode:
					{
						result = AccPOSRuleList.Codes.BillToPartyLocation;
						break;
					}
				case AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode:
					{
						result = ZString.Empty;
						break;
					}
				default:
					throw new ArgumentException(Res.GetString("50870184-D231-4756-A45A-8869400E070F", "Invalid registry value for 'Configure default place of supply when no matching rules':[{0}].", AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value));
			}

			company.GetTracer().TraceInformation(AccountingTraceSourceCodes.FPOS, () => TraceMessages.BuildPOSDefaultRuleFromRegistryTraceMessage("Registry - Default POS rule when no matching rule", company, registryValue));
			return result;
		}

		static ZString GetPlaceOfSupplyWhenMatchingRulesButMissingLocation(GlbCompany company)
		{
			switch (AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				case AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode:
					return AccPOSRuleList.Codes.BillToPartyLocation;
				case AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode:
					return ZString.Empty;
				default:
					throw new ArgumentException(Res.GetString("140c014e-a36e-4181-bdef-521b5ab55edc", "Invalid registry value for 'Configure default place of supply when there is matching rules but missing attribute':[{0}].", AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.Value));
			}
		}

		static bool IsOrgCusCodeValidForCompanyCountry(OrgCusCode orgCusCode, IOrgCusCodePredicateProvider orgCusCodePredicateProvider, GlbCompany company, ZString vatCode)
		{
			if (orgCusCodePredicateProvider != null && !orgCusCodePredicateProvider.IncludeForPlaceOfSupplyTaxRegsistration(orgCusCode))
			{
				return false;
			}
			return orgCusCode.OK_CodeType.EqualsIgnoringCase(vatCode)
					&& orgCusCode.OK_RN_NKCodeCountry.EqualsIgnoringCase(company.GC_RN_NKCountryCode)
					&& !orgCusCode.OK_CustomsRegNo.IsEmpty;
		}

		static ITracer GetTracer(this IBusiness bizO) => bizO.Factory.GetCachedValue(nameof(ITracer), () => ObjectFactory.Get<ITracer>());

		internal static class TraceMessages
		{
			internal static string BuildLookupPlaceOfSupplyRuleTraceMessage(string traceStepTitle, FPOSTracePlaceOfSupplyConfigurationDetailsInfo traceResultObject)
			{
				var jsonContent = JsonConvert.SerializeObject(traceResultObject, Formatting.Indented);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}

			internal static string BuildCandidatePlaceOfSupplyRuleCollection(string traceStepTitle, AccPOSConfigurationCollection collection)
			{
				var resultObject = new List<FPOSTracePlaceOfSupplyConfigurationDetailsInfo>();
				foreach (AccPOSConfiguration posRule in collection)
				{
					resultObject.Add(new FPOSTracePlaceOfSupplyConfigurationDetailsInfo()
					{
						BranchCode = posRule.PSC_NK_Branch,
						LevelCode = posRule.LevelCode,
						LevelName = posRule.LevelName,
						JobType = posRule.PSC_JobType,
						ChargeType = posRule.PSC_ChargeType,
						IncoTerms = posRule.PSC_IncoTerm,
						Direction = posRule.PSC_ServiceDirection,
						TransportMode = posRule.PSC_TransportMode,
						TaxRegistration = posRule.PSC_TaxRegistrationType,
						SupplyType = posRule.PSC_SupplyType,
						POSRule = posRule.PSC_PlaceOfSupplyRule
					});
				}

				var jsonContent = JsonConvert.SerializeObject(resultObject, Formatting.Indented);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}

			internal static string BuildLookupPlaceOfSupplyResultRuleTraceMessage(string traceStepTitle, AccPOSConfiguration posRule = null)
			{
				if (posRule == null)
				{
					return FormattableString.Invariant($@"TRACED: {traceStepTitle} DETAILS:No Matching Place Of Supply Configuration");
				}

				var traceResultObject = new FPOSTracePlaceOfSupplyConfigurationDetailsInfo()
				{
					BranchCode = posRule.PSC_NK_Branch,
					LevelCode = posRule.LevelCode,
					LevelName = posRule.LevelName,
					JobType = posRule.PSC_JobType,
					ChargeType = posRule.PSC_ChargeType,
					IncoTerms = posRule.PSC_IncoTerm,
					Direction = posRule.PSC_ServiceDirection,
					TransportMode = posRule.PSC_TransportMode,
					TaxRegistration = posRule.PSC_TaxRegistrationType,
					SupplyType = posRule.PSC_SupplyType,
					POSRule = posRule.PSC_PlaceOfSupplyRule
				};

				var jsonContent = JsonConvert.SerializeObject(traceResultObject, Formatting.Indented);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}

			internal static string BuildLocationFromPOSConfigurationTraceMessage(string traceStepTitle, ILocation location, string posRule)
			{
				var traceResultObject = new FPOSTraceLocationDetailsInfo()
				{
					Code = location?.Code,
					IsActive = location != null ? location.IsActive : false,
					CountryCode = location?.Country?.Code,
					StateCode = location?.State?.RW_Code,
					TaxZoneCode = location?.Zones
										.Where(z => z.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Tax)
										.OrderByDescending(z => z.IsActive)
										.FirstOrDefault()?.Code,
					POSRule = posRule,
				};
				var jsonContent = JsonConvert.SerializeObject(traceResultObject, Formatting.Indented);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}

			internal static string BuildPOSFromLocationTraceMessage(string traceStepTitle, string posType, string posCode, string posTypeDescription = null)
			{
				var traceResultObject = new FPOSTracePlaceOfSupplyDetailsInfo()
				{
					POSTypeDescription = posTypeDescription,
					POSCode = posCode,
					POSType = posType,
				};
				var jsonContent = JsonConvert.SerializeObject(traceResultObject, Formatting.None);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}

			internal static string BuildPOSDefaultRuleFromRegistryTraceMessage(string traceStepTitle, GlbCompany company, string posDefaultRegistry)
			{
				var traceResultObject = new FPOSTraceDefaultPOSRegistryValueInfo()
				{
					CompanyCode = company.GC_Code,
					DefaultPOSRegistryRule = posDefaultRegistry
				};
				var jsonContent = JsonConvert.SerializeObject(traceResultObject, Formatting.Indented);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}

			internal static string BuildChargeCodeInfoTraceMessage(string traceStepTitle, string companyCode, AccChargeCode chargeCode = null, AccPOSChargeCodeGroup group = null)
			{
				var traceResultObject = new FPOSTraceChargeCodeDetailInfo()
				{
					CompanyCode = companyCode,
					ChargeCode = chargeCode?.AC_Code,
					ChargeCodeDescription = chargeCode?.AC_DescMultilingual,
					ChargeCodeGroup = group?.GRO_Code,
					ChargeCodeGroupDescription = group?.GRO_Description
				};
				var jsonContent = JsonConvert.SerializeObject(traceResultObject, Formatting.Indented);
				return FormattableString.Invariant($@"
TRACED: {traceStepTitle}
DETAILS:
{jsonContent}");
			}
		}

		internal sealed class FPOSTraceDefaultPOSRegistryValueInfo
		{
			[JsonProperty(PropertyName = "companyCode", NullValueHandling = NullValueHandling.Ignore)]
			public string CompanyCode { get; set; }

			[JsonProperty(PropertyName = "defaultPOSRegistryRule", NullValueHandling = NullValueHandling.Ignore)]
			public string DefaultPOSRegistryRule { get; set; }
		}

		internal sealed class FPOSTraceChargeCodeDetailInfo
		{
			[JsonProperty(PropertyName = "companyCode", NullValueHandling = NullValueHandling.Ignore)]
			public string CompanyCode { get; set; }

			[JsonProperty(PropertyName = "chargeCode", NullValueHandling = NullValueHandling.Ignore)]
			public string ChargeCode { get; set; }

			[JsonProperty(PropertyName = "chargeCodeDescription", NullValueHandling = NullValueHandling.Ignore)]
			public string ChargeCodeDescription { get; set; }

			[JsonProperty(PropertyName = "chargeCodeGroup", NullValueHandling = NullValueHandling.Ignore)]
			public string ChargeCodeGroup { get; set; }

			[JsonProperty(PropertyName = "chargeCodeGroupDescription", NullValueHandling = NullValueHandling.Ignore)]
			public string ChargeCodeGroupDescription { get; set; }
		}

		internal sealed class FPOSTracePlaceOfSupplyDetailsInfo
		{
			[JsonProperty(PropertyName = "posTypeDescription", NullValueHandling = NullValueHandling.Ignore)]
			public string POSTypeDescription { get; set; }

			[JsonProperty(PropertyName = "posType", NullValueHandling = NullValueHandling.Ignore)]
			public string POSType { get; set; }

			[JsonProperty(PropertyName = "posCode", NullValueHandling = NullValueHandling.Ignore)]
			public string POSCode { get; set; }
		}

		internal sealed class FPOSTraceLocationDetailsInfo
		{
			[JsonProperty(PropertyName = "plugInInfo", NullValueHandling = NullValueHandling.Ignore)]
			public string PlugInInfo { get; set; }

			[JsonProperty(PropertyName = "code", NullValueHandling = NullValueHandling.Ignore)]
			public string Code { get; set; }

			[JsonProperty(PropertyName = "isActive", NullValueHandling = NullValueHandling.Ignore)]
			public bool IsActive { get; set; }

			[JsonProperty(PropertyName = "countryCode", NullValueHandling = NullValueHandling.Ignore)]
			public string CountryCode { get; set; }

			[JsonProperty(PropertyName = "stateCode", NullValueHandling = NullValueHandling.Ignore)]
			public string StateCode { get; set; }

			[JsonProperty(PropertyName = "taxZoneCode", NullValueHandling = NullValueHandling.Ignore)]
			public string TaxZoneCode { get; set; }

			[JsonProperty(PropertyName = "posRule", NullValueHandling = NullValueHandling.Ignore)]
			public string POSRule { get; set; }
		}

		internal sealed class FPOSTracePlaceOfSupplyConfigurationDetailsInfo
		{
			[JsonProperty(PropertyName = "groupCode", NullValueHandling = NullValueHandling.Ignore)]
			public string GroupCode { get; set; }

			[JsonProperty(PropertyName = "jobType", NullValueHandling = NullValueHandling.Ignore)]
			public string JobType { get; set; }

			[JsonProperty(PropertyName = "chargeType", NullValueHandling = NullValueHandling.Ignore)]
			public string ChargeType { get; set; }
			[JsonProperty(PropertyName = "transportMode", NullValueHandling = NullValueHandling.Ignore)]
			public string TransportMode { get; set; }

			[JsonProperty(PropertyName = "incoTerms", NullValueHandling = NullValueHandling.Ignore)]
			public string IncoTerms { get; set; }

			[JsonProperty(PropertyName = "direction", NullValueHandling = NullValueHandling.Ignore)]
			public string Direction { get; set; }

			[JsonProperty(PropertyName = "costOrSell", NullValueHandling = NullValueHandling.Ignore)]
			public string CostOrSell { get; set; }

			[JsonProperty(PropertyName = "taxRegistration", NullValueHandling = NullValueHandling.Ignore)]
			public string TaxRegistration { get; set; }

			[JsonProperty(PropertyName = "supplyType", NullValueHandling = NullValueHandling.Ignore)]
			public string SupplyType { get; set; }

			[JsonProperty(PropertyName = "branchCode", NullValueHandling = NullValueHandling.Ignore)]
			public string BranchCode { get; set; }

			[JsonProperty(PropertyName = "levelCode", NullValueHandling = NullValueHandling.Ignore)]
			public string LevelCode { get; set; }

			[JsonProperty(PropertyName = "levelName", NullValueHandling = NullValueHandling.Ignore)]
			public string LevelName { get; set; }

			[JsonProperty(PropertyName = "posRule", NullValueHandling = NullValueHandling.Ignore)]
			public string POSRule { get; set; }
		}
	}
}
