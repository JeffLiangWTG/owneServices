using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PRADataBuilder
	{
		public PRADataBuilder(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}
		protected readonly BusinessObjectFactory Factory;

		public RefVessel GetRefVessel(ZString vesselName, ZString lloydsNumber)
		{
			RefVessel result = Factory.New<RefVessel>();
			result.RV_Code = vesselName;
			result.RV_LloydsNumber = lloydsNumber;
			return result;
		}

		public OrgAddress GetCTOAddress(ZString oneStopCode, ZString countryCode)
		{
			OrgHeader cTOOrg = Factory.New<OrgHeader>();
			cTOOrg.OH_Code = "CTOORG";
			OrgAddress cTOAddress = cTOOrg.MainAddress;
			OrgCusCode cTOCustomsCode = cTOOrg.CustomsCodes.AddNew();
			cTOCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			cTOCustomsCode.OK_CustomsRegNo = oneStopCode;
			cTOCustomsCode.OK_RN_NKCodeCountry = countryCode;
			return cTOAddress;
		}

		public OrgHeader GetShippingLine(ZString oneStopCode, ZString countryCode)
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SHIPLINE";
			OrgCusCode shippingLineOneStopCode = shippingLine.CustomsCodes.AddNew();
			shippingLineOneStopCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			shippingLineOneStopCode.OK_CustomsRegNo = oneStopCode;
			shippingLineOneStopCode.OK_RN_NKCodeCountry = countryCode;
			return shippingLine;
		}

		public CFSLoadListConsol GetCFSConsol(ZString voyageFlight, RefVessel vessel, ZString portOfLoading, ZString portOfDischarge, ZString bookingReference, OrgAddress cTOAddress, OrgHeader shippingLine)
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_RL_NKLoadPort = portOfLoading;
			consol.JK_RL_NKDischargePort = portOfDischarge;

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_Vessel = vessel.RV_Code;
			consol.JK_BookingReference = bookingReference;
			consol.JK_OA_CTOAddress = cTOAddress.PK;
			consol.SetDefaultShippingLineAddress(shippingLine);
			return consol;
		}

		public ForwardingConsol GetFreightConsol(ZString voyageFlight, RefVessel vessel, ZString portOfLoading, ZString portOfDischarge, ZString bookingReference, OrgAddress cTOAddress, OrgHeader shippingLine)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = portOfLoading;
			consol.JK_RL_NKDischargePort = portOfDischarge;

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_Vessel = vessel.RV_Code;
			consol.JK_BookingReference = bookingReference;
			consol.JK_OA_DepartureCTOAddress = cTOAddress.PK;
			consol.SetDefaultShippingLineAddress(shippingLine);
			return consol;
		}

		public JobDeclaration GetJobDeclaration(ZString voyageFlight, RefVessel vessel, ZString portOfLoading, ZString portOfDischarge, ZString finalDestination, OrgAddress cTOAddress, OrgHeader shippingLine, ZString entryNumber, ZString customsCAN)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_VesselName = vessel.RV_Code;
			jobDeclaration.JE_VoyageFlightNo = voyageFlight;
			jobDeclaration.JE_RL_NKPortOfLoading = portOfLoading;
			jobDeclaration.JE_RL_NKPortOfArrival = portOfDischarge;
			jobDeclaration.JE_RL_NKFinalDestination = finalDestination;
			jobDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
			jobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			jobDeclaration.DeclarationNumber = customsCAN;
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = entryNumber;
			return jobDeclaration;
		}

		public CommonContainer GetFreightContainer(ZString cTOOneStopCode, ZString cTOCountryCode, ZString shippingLineOneStopCode, ZString shippingLineCountryCode, ZString vesselName, ZString lloydsNumber, ZString voyageFlight, ZString portOfLoading, ZString portOfDischarge, ZString bookingReference, ZString containerNumber, ZString sealNumber, ZString oneStopCommodityCode, ZString iSOType, ZDecimal tareWeight, ZDecimal dunnageWeight, ZString customsCAN)
		{
			OrgAddress cTOAddress = GetCTOAddress(cTOOneStopCode, cTOCountryCode);
			OrgHeader shippingLine = GetShippingLine(shippingLineOneStopCode, shippingLineCountryCode);
			RefVessel vessel = GetRefVessel(vesselName, lloydsNumber);
			ForwardingConsol consol = GetFreightConsol(voyageFlight, vessel, portOfLoading, portOfDischarge, bookingReference, cTOAddress, shippingLine);
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			refContainer.RC_ISOType = iSOType;

			CommonContainer container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = oneStopCommodityCode;
			container.JC_ContainerNum = containerNumber;
			container.JC_SealNum = sealNumber;
			container.JC_TareWeight = tareWeight;
			container.JC_DunnageWeight = dunnageWeight;
			container.JC_RC = refContainer.PK;
			container.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 5, 1);
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			container.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
			container.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
			container.GrossWeightVerifiedByAddress.E2_City = "Mascot";
			container.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
			container.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "AU";

			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.Containers.Add(container);

			shipment.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			shipment.CustomsEntryNumber = customsCAN;

			return container;
		}

		public CFSContainer GetCFSContainer(ZString ctoOneStopCode, ZString ctoCountryCode, ZString shippingLineOneStopCode, ZString shippingLineCountryCode, ZString vesselName, ZString lloydsNumber, ZString voyageFlight, ZString portOfLoading, ZString portOfDischarge, ZString bookingReference, ZString containerNumber, ZString sealNumber, ZString oneStopCommodityCode, ZString isoType, ZDecimal tareWeight, ZDecimal dunnageWeight, ZString customsCAN)
		{
			OrgAddress ctoAddress = GetCTOAddress(ctoOneStopCode, ctoCountryCode);
			OrgHeader shippingLine = GetShippingLine(shippingLineOneStopCode, shippingLineCountryCode);
			RefVessel vessel = GetRefVessel(vesselName, lloydsNumber);
			CFSLoadListConsol consol = GetCFSConsol(voyageFlight, vessel, portOfLoading, portOfDischarge, bookingReference, ctoAddress, shippingLine);

			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			refContainer.RC_ISOType = isoType;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_RH_NKContainerCommodityCode = oneStopCommodityCode;
			container.JC_ContainerNum = containerNumber;
			container.JC_ExportDepotCustomsReference = customsCAN;
			container.JC_SealNum = sealNumber;
			container.JC_TareWeight = tareWeight;
			container.JC_DunnageWeight = dunnageWeight;
			container.JC_RC = refContainer.PK;
			container.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 5, 1);
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			container.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
			container.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
			container.GrossWeightVerifiedByAddress.E2_City = "Mascot";
			container.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
			container.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "AU";

			var shipment = CommonShipment.New(Factory);
			var packLine = shipment.Factory.New<PackLine>();
			packLine.JL_JC = container.PK;
			packLine.JL_JS = shipment.PK;

			return container;
		}

		public CusContainer GetCustomsContainer(ZString ctoOneStopCode, ZString ctoCountryCode, ZString shippingLineOneStopCode, ZString shippingLineCountryCode, ZString vesselName, ZString lloydsNumber, ZString voyageFlight, ZString portOfLoading, ZString portOfDischarge, ZString finalDestination, ZString entryNumber, ZString bookingReference, ZString containerNumber, ZString sealNumber, ZString oneStopCommodityCode, ZString isoType, ZDecimal tareWeight, ZDecimal grossWeight, ZString customsCAN)
		{
			OrgAddress ctoAddress = GetCTOAddress(ctoOneStopCode, ctoCountryCode);
			OrgHeader shippingLine = GetShippingLine(shippingLineOneStopCode, shippingLineCountryCode);
			RefVessel vessel = GetRefVessel(vesselName, lloydsNumber);
			JobDeclaration jobDeclaration = GetJobDeclaration(voyageFlight, vessel, portOfLoading, portOfDischarge, finalDestination, ctoAddress, shippingLine, entryNumber, customsCAN);

			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TareWeight = tareWeight;
			refContainer.RC_GrossWeight = grossWeight;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_BookingReference = bookingReference;

			CommonContainer freightContainer = consol.Containers.AddNew();
			freightContainer.JC_RC = refContainer.PK;
			freightContainer.JC_SealNum = sealNumber;
			freightContainer.JC_RH_NKContainerCommodityCode = oneStopCommodityCode;
			freightContainer.JC_ContainerNum = containerNumber;
			freightContainer.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			freightContainer.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 5, 1);
			freightContainer.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			freightContainer.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
			freightContainer.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
			freightContainer.GrossWeightVerifiedByAddress.E2_City = "Mascot";
			freightContainer.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
			freightContainer.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "AU";

			CusContainer result = jobDeclaration.CusContainers.AddNew();
			result.RH_NKContainerCommodityCode = oneStopCommodityCode;
			result.CO_ContainerNumber = containerNumber;
			result.CO_Seal = sealNumber;
			result.CO_RC = refContainer.PK;
			result.CO_JC = freightContainer.PK;

			return result;
		}
	}
}
