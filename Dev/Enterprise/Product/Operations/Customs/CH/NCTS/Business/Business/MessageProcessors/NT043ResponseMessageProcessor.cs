using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using IAddress = CargoWise.Customs.CH.MessageContracts.Passar.Outgoing.IAddress;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT043ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT043ResponseDetail>
{
	public NT043ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT043 - Arrival Intentory Request Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarInventoryRequest };

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override bool UpdateApplicationReference => false;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INT043ResponseDetail response)
	{
		NctsHeader linkedNctsHeader;

		var movementReference = new MovementReferenceNumberSupportingInfo.Loader(message.Factory).FindByMovementReferenceNumber(response.MRN);
		if (movementReference != null)
		{
			var parentMovementHeader = movementReference.Parent;
			var parentNctsHeader = parentMovementHeader.Header;
			if (parentMovementHeader.MovementReferenceNumbers.Count == 1)
			{
				linkedNctsHeader = parentNctsHeader;
			}
			else
			{
				linkedNctsHeader = FindChildNctsHeaderByMRN(message.Factory, response.MRN);
				if (linkedNctsHeader == null)
				{
					linkedNctsHeader = NewArrivalMovementJob(parentNctsHeader, movementReference);
				}
				AddRelatedArrivalMovement(parentMovementHeader, linkedNctsHeader.ArrivalMovementHeader);
			}
			UpdateLinkedNctsHeaderDetails(linkedNctsHeader, movementReference, response);
		}
		else
		{
			linkedNctsHeader = FindChildNctsHeaderByMRN(message.Factory, response.MRN);
		}
		message.EM_ApplicationReference = response.MessageIdentification;
		return linkedNctsHeader;
	}

	NctsHeader FindChildNctsHeaderByMRN(BusinessObjectFactory factory, string mrn)
	{
		NctsHeader childNctsHeader = null;
		var existingNctsHeader = new NctsHeader.Loader(factory).FindByMovementReferenceNumber(NctsMovementType.Codes.Arrival, mrn);
		if (existingNctsHeader != null && !existingNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator)
		{
			childNctsHeader = existingNctsHeader;
		}
		return childNctsHeader;
	}

	NctsHeader NewArrivalMovementJob(NctsHeader initialNctsHeader, MovementReferenceNumberSupportingInfo movementReference)
	{
		var initialMovementHeader = initialNctsHeader.ArrivalMovementHeader;

			var newRelatedNctsHeader = initialNctsHeader.Factory.New<NctsHeader>();
			newRelatedNctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			newRelatedNctsHeader.BH_CommunicationLanguage = initialNctsHeader.BH_CommunicationLanguage;
			newRelatedNctsHeader.BH_SystemCreateUser = initialMovementHeader.Header.BH_SystemCreateUser;
			newRelatedNctsHeader.DestinationTrader.CopyPersistentValuesFrom(initialNctsHeader.DestinationTrader);

		var newRelatedMovement = newRelatedNctsHeader.ArrivalMovementHeader;
		newRelatedMovement.BM_PaperlessInbondNum = initialMovementHeader.BM_PaperlessInbondNum;
		newRelatedMovement.BM_ArrivalDate = initialMovementHeader.BM_ArrivalDate;
		newRelatedMovement.BM_TransportAtArrivalType = initialMovementHeader.BM_TransportAtArrivalType;
		newRelatedMovement.BM_TransportAtArrivalID = initialMovementHeader.BM_TransportAtArrivalID;
		newRelatedMovement.BM_RN_NKTransportAtArrivalIDNationality = initialMovementHeader.BM_RN_NKTransportAtArrivalIDNationality;
		newRelatedMovement.MultipleMRNIndicator = ZBool.False;
		newRelatedMovement.GoodsLocation.CopyPersistentValuesFrom(initialMovementHeader.GoodsLocation, new BusinessObjectCloneArgs(new[] { CusGoodsLocationSchema.Constants.CGL_ParentTableCode, CusGoodsLocationSchema.Constants.CGL_ParentID }));
		newRelatedMovement.GoodsLocation.Address.CopyPersistentValuesFrom(initialMovementHeader.GoodsLocation.Address);

		newRelatedNctsHeader.Logs.AddNew(Events.JobOpen, movementReference.CSI_ReferenceNumber);

		var initialJob = new JobHeader.Loader(initialNctsHeader).Load();
		if (initialJob != null)
		{
			var newJob = new JobHeader.Loader(newRelatedNctsHeader).TryCreate();
			if (newJob != null)
			{
				newJob.JH_GE = initialJob.JH_GE;
			}
		}

		return newRelatedNctsHeader;
	}

	void UpdateLinkedNctsHeaderDetails(NctsHeader linkedNctsHeader, MovementReferenceNumberSupportingInfo movementReference, INT043ResponseDetail response)
	{
		var movementHeader = linkedNctsHeader.ArrivalMovementHeader;
		movementHeader.BM_StateOfSeals = movementReference.CSI_Status;
		movementHeader.BM_NoChangesToReport = movementReference.CSI_Status == YesNoList.Codes.No ? ZBool.False : ZBool.True;
		movementHeader.BM_AdditionalText = movementReference.CSI_Description.Left(movementHeader.BM_AdditionalTextInfo.MaxLength);
		movementHeader.MultipleMRNIndicator = ZBool.False;

		movementHeader.BM_InBondEntryType = ((ZString)response.TransitOperationDeclarationType).Left(movementHeader.BM_InBondEntryTypeInfo.MaxLength);
		movementHeader.BM_ReducedDatasetIndicator = response.TransitOperationReducedDatasetIndicator;
		movementHeader.BM_TypeOfSecurity = GetTypeOfSecurity(response.TransitOperationSecurity).Left(movementHeader.BM_TypeOfSecurityInfo.MaxLength);
		movementHeader.BM_SpecificCircumstance = ((ZString)response.TransitOperationSpecificCircumstanceIndicator).Left(movementHeader.BM_SpecificCircumstanceInfo.MaxLength);

		UpdateOrCreateCustomsOffice(linkedNctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, response.CustomsOfficeOfDepartureReferenceNumber);
		UpdateOrCreateCustomsOffice(linkedNctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, response.CustomsOfficeOfDestinationDeclaredReferenceNumber);
		UpdateOrCreateCustomsOffice(linkedNctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, response.CustomsOfficeOfDestinationActualReferenceNumber);

		linkedNctsHeader.ArrivalMovementHeader.CustomsOffices.GetElementsHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).ToArray().ForEach(o => o.Delete());
		foreach (var customsOfficeOfTransit in response.CustomsOfficeOfTransit)
		{
			var customsOfficeOfTransitDeclared = linkedNctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, customsOfficeOfTransit.ReferenceNumber);
			customsOfficeOfTransitDeclared.CY_Order = (ZShort)customsOfficeOfTransit.SequenceNumber;

			if (customsOfficeOfTransit.ArrivalDateAndTime.HasValue)
			{
				customsOfficeOfTransitDeclared.CY_Date = customsOfficeOfTransit.ArrivalDateAndTime.Value;
			}
		}

		UpdatePrincipal(linkedNctsHeader, response);

		movementHeader.BM_RL_NKDestinationPort = response.ConsignmentCountryOfDestination.ToZString(movementHeader.BM_RL_NKDestinationPortInfo.MaxLength);
		movementHeader.BM_RN_NKCountryOfDispatch = response.ConsignmentCountryOfDispatch.ToZString(movementHeader.BM_RN_NKCountryOfDispatchInfo.MaxLength);
		movementHeader.BM_GrossWeight = response.ConsignmentGrossMass;
		movementHeader.BM_ExportTransportMode = response.ConsignmentModeOfTransportAtTheBorder.ToZString(movementHeader.BM_ExportTransportModeInfo.MaxLength);
		movementHeader.BM_InlandTransportMode = response.ConsignmentModeOfTransportInland.ToZString(movementHeader.BM_InlandTransportModeInfo.MaxLength);
		movementHeader.BM_UniqueConsignmentReference = response.ConsignmentReferenceNumberUCR.ToZString(movementHeader.BM_UniqueConsignmentReferenceInfo.MaxLength);

		var carrier = movementHeader.Carrier;
		UpdateJobDocAddress(carrier, response.ConsignmentCarrierIdentificationNumber);
		UpdateJobDocAddress(linkedNctsHeader.Consignor, response.ConsignmentConsignorIdentificationNumber, response.ConsignmentConsignorName, response.ConsignmentConsignorAddress);
		UpdateJobDocAddress(linkedNctsHeader.Consignee, response.ConsignmentConsigneeIdentificationNumber, response.ConsignmentConsigneeName, response.ConsignmentConsigneeAddress);

		movementHeader.BM_MethodOfPayment = response.ConsignmentTransportChargesMethodOfPayment;

		movementHeader.BM_PlaceOfLoading = (response.ConsignmentPlaceOfLoading?.Location ?? ZString.Empty).ToZString(movementHeader.BM_PlaceOfLoadingInfo.MaxLength);
		movementHeader.BM_PortOfPresentationCode = (response.ConsignmentPlaceOfLoading?.UNLocode ?? (response.ConsignmentPlaceOfLoading?.Country ?? ZString.Empty)).ToZString(movementHeader.BM_PortOfPresentationCodeInfo.MaxLength);

		movementHeader.BM_PlaceOfUnloading = (response.ConsignmentPlaceOfUnloading?.Location ?? ZString.Empty).ToZString(movementHeader.BM_PlaceOfUnloadingInfo.MaxLength);
		movementHeader.BM_ForeignDestPortKCode = (response.ConsignmentPlaceOfLoading?.UNLocode ?? (response.ConsignmentPlaceOfUnloading?.Country ?? ZString.Empty)).ToZString(movementHeader.BM_ForeignDestPortKCodeInfo.MaxLength);

		foreach (var supplyChainActor in response.ConsignmentAdditionalSupplyChainActors)
		{
			var cusSupplyChainActor = linkedNctsHeader.CusSupplyChainActors.AddNew();
			cusSupplyChainActor.CFR_Code = supplyChainActor.Role.ToZString(cusSupplyChainActor.CFR_CodeInfo.MaxLength);
			cusSupplyChainActor.CFR_Reference = supplyChainActor.IdentificationNumber.ToZString(cusSupplyChainActor.CFR_ReferenceInfo.MaxLength);
		}

		foreach (var transportEquipment in response.ConsignmentTransportEquipments)
		{
			var headerContainer = linkedNctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			headerContainer.BC_ContainerNum = transportEquipment.ContainerIdentificationNumber.ToZString(headerContainer.BC_ContainerNumInfo.MaxLength);
			headerContainer.BC_SequenceNumber = (ZShort)transportEquipment.SequenceNumber;
			headerContainer.BC_Mode = transportEquipment.ContainerIdentificationNumber.IsNullOrEmpty() ? Core.Constants.ContainerModes.NonContainerised : Core.Constants.ContainerModes.Containerised;

			foreach (var seal in transportEquipment.Seals)
			{
				var additionalSeal = headerContainer.Seals.AddNew();
				additionalSeal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				additionalSeal.BK_SealNumber = seal.Identifier.ToZString(additionalSeal.BK_SealNumberInfo.MaxLength);
				additionalSeal.BK_SequenceNumber = (ZShort)seal.SequenceNumber;
			}
		}

		var i = 1;
		foreach (var departureTransportMeans in response.ConsignmentDepartureTransportMeans)
		{
			var arrivalTransportInfo = movementHeader.ArrivalTransportInfos.AddNew();
			arrivalTransportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			arrivalTransportInfo.TPM_SequenceNumber = (ZShort)departureTransportMeans.SequenceNumber;
			arrivalTransportInfo.TPM_RN_NKTransportNationality = departureTransportMeans.Nationality.ToZString(arrivalTransportInfo.TPM_RN_NKTransportNationalityInfo.MaxLength);
			arrivalTransportInfo.TPM_IdentificationNumber = departureTransportMeans.IdentificationNumber.ToZString(arrivalTransportInfo.TPM_IdentificationNumberInfo.MaxLength);
			arrivalTransportInfo.TPM_TypeOfIdentification = departureTransportMeans.TypeOfIdentification.ToZString(arrivalTransportInfo.TPM_TypeOfIdentificationInfo.MaxLength);
			i++;
		}

		foreach (var activeBorderTransportMeans in response.ConsignmentActiveBorderTransportMeans)
		{
			var arrivalTransportInfo = movementHeader.ArrivalTransportInfos.AddNew();
			arrivalTransportInfo.TPM_SequenceNumber = (ZShort)i;
			arrivalTransportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			arrivalTransportInfo.TPM_RN_NKTransportNationality = activeBorderTransportMeans.Nationality.ToZString(arrivalTransportInfo.TPM_RN_NKTransportNationalityInfo.MaxLength);
			arrivalTransportInfo.TPM_IdentificationNumber = activeBorderTransportMeans.IdentificationNumber.ToZString(arrivalTransportInfo.TPM_IdentificationNumberInfo.MaxLength);
			arrivalTransportInfo.TPM_TypeOfIdentification = activeBorderTransportMeans.TypeOfIdentification.ToZString(arrivalTransportInfo.TPM_TypeOfIdentificationInfo.MaxLength);
			arrivalTransportInfo.TPM_ReferenceNumber = activeBorderTransportMeans.ConveyanceReferenceNumber.ToZString(arrivalTransportInfo.TPM_ReferenceNumberInfo.MaxLength);
			arrivalTransportInfo.TPM_CustomsOffice = activeBorderTransportMeans.CustomsOfficeAtBorderReferenceNumber.ToZString(arrivalTransportInfo.TPM_CustomsOfficeInfo.MaxLength);
			i++;
		}

		foreach (var countryOfRoutingOfConsignment in response.ConsignmentCountryOfRoutingOfConsignments)
		{
			var countryOfRouting = linkedNctsHeader.CountriesOfRouting.AddNew();
			countryOfRouting.CY_Order = (ZShort)countryOfRoutingOfConsignment.SequenceNumber;
			countryOfRouting.CY_Data = countryOfRoutingOfConsignment.Country.ToZString(countryOfRouting.CY_DataInfo.MaxLength);
		}

		AddDocuments(linkedNctsHeader.PreviousDocuments, response.ConsignmentPreviousDocuments);
		AddDocuments(movementHeader.SupportingDocuments, response.ConsignmentSupportingDocuments);
		AddDocuments(movementHeader.AdditionalDocuments, response.ConsignmentTransportDocuments, AdditionalInfoSubTypeList.Codes.TransportDocument);

		foreach (var additionalReference in response.ConsignmentAdditionalReferences)
		{
			var additionalInfo = movementHeader.AdditionalDocuments.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			additionalInfo.CSI_LineNo = (ZShort)additionalReference.SequenceNumber;
			additionalInfo.CSI_Code = additionalReference.Type.ToZString(additionalInfo.CSI_CodeInfo.MaxLength);
			additionalInfo.CSI_ReferenceNumber = additionalReference.ReferenceNumber.ToZString(additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
		}

		foreach (var additinalInformation in response.ConsignmentAdditionalInformations)
		{
			var additionalInfo = movementHeader.AdditionalDocuments.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			additionalInfo.CSI_LineNo = (ZShort)additinalInformation.SequenceNumber;
			additionalInfo.CSI_Code = additinalInformation.Code.ToZString(additionalInfo.CSI_CodeInfo.MaxLength);
			additionalInfo.CSI_Description = additinalInformation.Text.ToZString(additionalInfo.CSI_DescriptionInfo.MaxLength);
		}

		AddHouseConsignments(linkedNctsHeader, response);
		AddIncidents(linkedNctsHeader, response.ConsignmentIncidents);
	}

	void AddHouseConsignments(NctsHeader linkedNctsHeader, INT043ResponseDetail response)
	{
		foreach (var houseConsignment in response.ConsignmentHouseConsignment)
		{
			var nctsBill = linkedNctsHeader.Bills.AddNew();
			var movementDetail = nctsBill.MovementDetail;
			movementDetail.B9_SeqNo = houseConsignment?.SequenceNumber?.ToString() ?? ZString.Empty;
			movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			nctsBill.B0_RN_NKCountryOfExport = houseConsignment.CountryOfDispatch.ToZString(nctsBill.B0_RN_NKCountryOfExportInfo.MaxLength);
			nctsBill.B0_RN_NKCountryOfDestination = houseConsignment.CountryOfDestination.ToZString(nctsBill.B0_RN_NKCountryOfDestinationInfo.MaxLength);
			nctsBill.B0_Weight = houseConsignment.GrossMass <= 0.0009m ? new ZWeight(houseConsignment.GrossMass, Core.Constants.Weight.Kilograms).ConvertTo(Core.Constants.Weight.Grams) : (ZDecimal)houseConsignment.GrossMass;
			nctsBill.B0_WeightUQ = houseConsignment.GrossMass <= 0.0009m ? Core.Constants.Weight.Grams : Core.Constants.Weight.Kilograms;
			nctsBill.B0_SecurityIndicatorFromExport = houseConsignment.SecurityIndicatorFromExportDeclaration.In(new string[] { "1", "2", "3" });

			UpdateJobDocAddress(movementDetail.ConsignorDocAddress, houseConsignment.Consignor);
			UpdateJobDocAddress(movementDetail.ConsigneeDocAddress, houseConsignment.Consignee);

			foreach (var supplyChainActor in houseConsignment.AdditionalSupplyChainActors)
			{
				var supplyChainActorReference = nctsBill.CusSupplyChainActorReferences.AddNew();
				supplyChainActorReference.CFR_Reference = supplyChainActor.IdentificationNumber.ToZString(supplyChainActorReference.CFR_ReferenceInfo.MaxLength);
				supplyChainActorReference.CFR_Code = supplyChainActor.Role.ToZString(supplyChainActorReference.CFR_CodeInfo.MaxLength);
			}

			foreach (var departureTransportMeans in houseConsignment.DepartureTransportMeans)
			{
				var departureTransportInfo = nctsBill.DepartureTransportInfos.AddNew();
				departureTransportInfo.TPM_RN_NKTransportNationality = departureTransportMeans.Nationality.ToZString(departureTransportInfo.TPM_RN_NKTransportNationalityInfo.MaxLength);
				departureTransportInfo.TPM_SequenceNumber = (ZShort)departureTransportMeans.SequenceNumber;
				departureTransportInfo.TPM_IdentificationNumber = departureTransportMeans.IdentificationNumber.ToZString(departureTransportInfo.TPM_IdentificationNumberInfo.MaxLength);
				departureTransportInfo.TPM_TypeOfIdentification = departureTransportMeans.TypeOfIdentification.ToZString(departureTransportInfo.TPM_TypeOfIdentificationInfo.MaxLength);
				departureTransportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			}

			houseConsignment.PreviousDocuments?.ForEach(p => UpdateDocument(nctsBill.PreviousDocuments.AddNew(), p));
			houseConsignment.SupportingDocuments?.ForEach(s => UpdateDocument(nctsBill.SupportingDocuments.AddNew(), s));
			houseConsignment.TransportDocuments?.ForEach(t => UpdateAdditionalInformation(nctsBill.AdditionalDocuments.AddNew(), AdditionalInfoSubTypeList.Codes.TransportDocument, t.SequenceNumber, t.Type, t.ReferenceNumber, null));
			houseConsignment.AdditionalReferences?.ForEach(r => UpdateAdditionalInformation(nctsBill.AdditionalDocuments.AddNew(), AdditionalInfoSubTypeList.Codes.AdditionalReference, r.SequenceNumber, r.Type, r.ReferenceNumber, null));
			houseConsignment.AdditionalInformations?.ForEach(i => UpdateAdditionalInformation(nctsBill.AdditionalDocuments.AddNew(), AdditionalInfoSubTypeList.Codes.AdditionalInformation, i.SequenceNumber, i.Code, null, i.Text));
			houseConsignment.ConsignmentItems?.ForEach(c => AddConsignmentItem(nctsBill.ArrivalGoodsItems.AddNew(), c, response));

			nctsBill.B0_TransportPaymentMethod = houseConsignment.TransportChargesMethodOfPayment;
		}
	}

	void AddConsignmentItem(NctsArrivalCargoDesc goodsItem, IConsignmentItem consignmentItem, INT043ResponseDetail response)
	{
		goodsItem.BY_DeclarationGoodsItemNumber = consignmentItem.DeclarationGoodsItemNumber ?? 0;
		goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		goodsItem.BY_Type = consignmentItem.DeclarationType;
		goodsItem.BY_LineNo = (ZShort)consignmentItem.GoodsItemNumber;
		goodsItem.BY_RN_NKCountryOfDestination = consignmentItem.CountryOfDestination;
		goodsItem.BY_RN_NKCountryOfDispatch = consignmentItem.CountryOfDispatch;
		goodsItem.BY_CommercialReferenceNumber = consignmentItem.ReferenceNumberUCR;
		goodsItem.BY_TransportChargesMethodOfPayment = consignmentItem.TransportCharges?.MethodOfPayment ?? ZString.Empty;

		UpdateJobDocAddress(goodsItem.ConsigneeDocAddress, consignmentItem.Consignee);

		foreach (var supplyChainActor in consignmentItem.AdditionalSupplyChainActors)
		{
			var supplyChainActorReference = goodsItem.Bill.CusSupplyChainActorReferences.AddNew();
			supplyChainActorReference.CFR_ParentID = goodsItem.PK;
			supplyChainActorReference.CFR_Reference = supplyChainActor.IdentificationNumber.ToZString(supplyChainActorReference.CFR_ReferenceInfo.MaxLength);
			supplyChainActorReference.CFR_Code = supplyChainActor.Role.ToZString(supplyChainActorReference.CFR_CodeInfo.MaxLength);
		}

		consignmentItem.Packagings?.ForEach(p => AddPackage(response, goodsItem, p, (ZInt)consignmentItem.DeclarationGoodsItemNumber));
		consignmentItem.PreviousDocuments?.ForEach(p => UpdateDocument(goodsItem.PreviousDocuments.AddNew(), p));
		consignmentItem.SupportingDocuments?.ForEach(s => UpdateDocument(goodsItem.SupportingDocuments.AddNew(), s));
		consignmentItem.TransportDocuments?.ForEach(t => UpdateAdditionalInformation(goodsItem.AdditionalInfos.AddNew(), AdditionalInfoSubTypeList.Codes.TransportDocument, t.SequenceNumber, t.Type, t.ReferenceNumber, null));
		consignmentItem.AdditionalReferences?.ForEach(r => UpdateAdditionalInformation(goodsItem.AdditionalInfos.AddNew(), AdditionalInfoSubTypeList.Codes.AdditionalReference, r.SequenceNumber, r.Type, r.ReferenceNumber, null));
		consignmentItem.AdditionalInformations?.ForEach(i => UpdateAdditionalInformation(goodsItem.AdditionalInfos.AddNew(), AdditionalInfoSubTypeList.Codes.AdditionalInformation, i.SequenceNumber, i.Code, null, i.Text));

		AddCommodity(response.MRN, goodsItem, consignmentItem.Commodity);
	}

	void AddCommodity(string mrn, NctsArrivalCargoDesc goodsItem, ICommodity commodity)
	{
		goodsItem.BY_Description = commodity.DescriptionOfGoods;
		goodsItem.BY_CusC4Number = commodity.CUSCode;
		goodsItem.BY_FormattedHarmonisedTariff = GetFormattedHarmonisedTariff(mrn, commodity.CommodityCode?.ControlCode, commodity.CommodityCode?.NationalCustomsTariffNumber);

		if (commodity.GoodsMeasure != null)
		{
			if (commodity.GoodsMeasure?.GrossMass.HasValue ?? false)
			{
				goodsItem.BY_GrossWeight = commodity.GoodsMeasure.GrossMass <= 0.0009m ? new ZWeight((ZDecimal)commodity.GoodsMeasure.GrossMass, Core.Constants.Weight.Kilograms).ConvertTo(Core.Constants.Weight.Grams) : (ZDecimal)commodity.GoodsMeasure.GrossMass;
				goodsItem.BY_GrossWeightUnit = commodity.GoodsMeasure.GrossMass <= 0.0009m ? Weight.Grams : Weight.Kilograms;
			}
			if (commodity.GoodsMeasure?.NetMass.HasValue ?? false)
			{
				goodsItem.BY_NetWeight = commodity.GoodsMeasure.NetMass <= 0.0009m ? new ZWeight((ZDecimal)commodity.GoodsMeasure.NetMass, Core.Constants.Weight.Kilograms).ConvertTo(Core.Constants.Weight.Grams) : (ZDecimal)commodity.GoodsMeasure.NetMass;
				goodsItem.BY_NetWeightUnit = commodity.GoodsMeasure.NetMass <= 0.0009m ? Core.Constants.Weight.Grams : Core.Constants.Weight.Kilograms;
			}
		}

		commodity.DangerousGoods?.ForEach(d => AddDangerousGoods(goodsItem, d));
	}

	void AddDangerousGoods(NctsArrivalCargoDesc goodsItem, IDangerousGoods dangerousGoods)
	{
		var undgSubstance = UNDGSubstanceLoader.LoadSubstances(goodsItem.Factory, dangerousGoods.UNNumber, standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO).FirstOrDefault();

		if (undgSubstance != null)
		{
			goodsItem.UNDGs.AddNew().DI_DG = undgSubstance.PK;
		}
	}

	ZString GetFormattedHarmonisedTariff(ZString mrn, string controlCode, string nationalCustomsTariffNumber)
	{
		var isNationalTransit = mrn.SubstringSafe(2, 2) == CountryCodes.Switzerland && mrn.SubstringSafe(16, 1) == "N";
		var formattedTariff = nationalCustomsTariffNumber?.Replace(".", ZString.Empty) ?? ZString.Empty;

		if (isNationalTransit && !formattedTariff.IsNullOrEmpty())
		{
			formattedTariff = string.IsNullOrEmpty(controlCode) ? formattedTariff + "000" : formattedTariff + controlCode.PadLeft(3, (char)0);
		}

		return formattedTariff;
	}

	void AddPackage(INT043ResponseDetail response, NctsArrivalCargoDesc goodsItem, IPackaging packaging, int declarationGoodsItemNumber)
	{
		var package = goodsItem.Packages.AddNew();
		package.B5_SequenceNumber = (ZShort)packaging.SequenceNumber;
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		package.B5_UnitCount = (ZLong)packaging.NumberOfPackages;
		package.B5_MarksAndNumbers = packaging.ShippingMarks;
		package.B5_UnitType = packaging.TypeOfPackages;

		var referencedTransportEquipments = response.ConsignmentTransportEquipments?.Where(e => e.GoodsReferences.Any(r => r.DeclarationGoodsItemNumber == declarationGoodsItemNumber));

		foreach (var transportEquipment in referencedTransportEquipments)
		{
			if (goodsItem.Header.ArrivalHeaderContainers.Where(x => x.BC_SequenceNumber == transportEquipment.SequenceNumber).FirstOrDefault() is NctsArrivalHeaderContainer container)
			{
				package.ContainersPivot.AddPivotFor(container);
			}
		}
	}

	void UpdateAdditionalInformation(AdditionalInfo additionalInformation, string type, int sequenceNumber, string code, string referenceNumber, string description)
	{
		additionalInformation.CSI_SubType = type.ToZString(additionalInformation.CSI_SubTypeInfo.MaxLength);
		additionalInformation.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		additionalInformation.CSI_LineNo = sequenceNumber;
		additionalInformation.CSI_Code = code.ToZString(additionalInformation.CSI_CodeInfo.MaxLength);
		additionalInformation.CSI_ReferenceNumber = referenceNumber.ToZString(additionalInformation.CSI_ReferenceNumberInfo.MaxLength);
		additionalInformation.CSI_Description = description.ToZString(additionalInformation.CSI_DescriptionInfo.MaxLength);
	}

	void UpdateDocument(ImportExportAwareSupportingInfo supportingInfo, IDocument document)
	{
		supportingInfo.CSI_Code = document.Type.ToZString(supportingInfo.CSI_CodeInfo.MaxLength);
		supportingInfo.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		supportingInfo.CSI_LineNo = document.SequenceNumber;
		supportingInfo.CSI_ItemNumber = document.LineItemNumber ?? ZInt.Zero;
		supportingInfo.CSI_ReferenceNumber2 = document.ComplementOfInformation.ToZString(supportingInfo.CSI_ReferenceNumber2Info.MaxLength);
		supportingInfo.CSI_ReferenceNumber = document.ReferenceNumber.ToZString(supportingInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	void UpdateJobDocAddress(JobDocAddress docAddress, IParticipant participant)
	{
		if (participant != null)
		{
			UpdateJobDocAddress(docAddress, participant.IdentificationNumber, participant.Name, participant.Address);
		}
	}

	void UpdateJobDocAddress(JobDocAddress docAddress, string identificationNumber, string companyName = null, IAddress address = null)
	{
		if ((companyName != null && address != null) || identificationNumber != null)
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			docAddress.E2_GovRegNum = identificationNumber.ToZString(docAddress.E2_GovRegNumInfo.MaxLength);
			docAddress.CompanyName = companyName.ToZString(docAddress.E2_CompanyNameInfo.MaxLength);

			if (companyName != null && address != null)
			{
				docAddress.E2_AdditionalAddressInformation = address.CareOf.ToZString(docAddress.E2_AdditionalAddressInformationInfo.MaxLength);
				docAddress.E2_City = address.City.ToZString(docAddress.E2_CityInfo.MaxLength);
				docAddress.E2_RN_NKCountryCode = address.Country.ToZString(docAddress.E2_RN_NKCountryCodeInfo.MaxLength);
				docAddress.E2_Postcode = address.Postcode.ToZString(docAddress.E2_PostcodeInfo.MaxLength);
				docAddress.E2_Address1 = address.StreetAndNumber.ToZString(docAddress.E2_Address1Info.MaxLength);
			}
		}
	}

	void AddDocuments(ICusSupportingInfoCollection<CusSupportingInfo> cusSupportingDocuments, IEnumerable<IDocument> documents, string subType = "")
	{
		foreach (var supportingDocument in documents)
		{
			var nctsSupportingDocument = cusSupportingDocuments.AddNew();
			if (!subType.IsNullOrEmpty())
			{
				nctsSupportingDocument.CSI_SubType = subType;
			}
			nctsSupportingDocument.CSI_LineNo = (ZShort)supportingDocument.SequenceNumber;
			nctsSupportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			nctsSupportingDocument.CSI_ItemNumber = supportingDocument.LineItemNumber ?? ZInt.Zero;
			nctsSupportingDocument.CSI_Code = supportingDocument.Type.ToZString(nctsSupportingDocument.CSI_CodeInfo.MaxLength);
			nctsSupportingDocument.CSI_ReferenceNumber = supportingDocument.ReferenceNumber.ToZString(nctsSupportingDocument.CSI_ReferenceNumberInfo.MaxLength);
			nctsSupportingDocument.CSI_ReferenceNumber2 = supportingDocument.ComplementOfInformation.ToZString(nctsSupportingDocument.CSI_ReferenceNumber2Info.MaxLength);
		}
	}

	ZString GetTypeOfSecurity(string security)
	{
		switch (security)
		{
			case "0":
				return NctsTypeOfSecurityList.Codes.NON;
			case "1":
				return NctsTypeOfSecurityList.Codes.ENT;
			case "2":
				return NctsTypeOfSecurityList.Codes.EXI;
			case "3":
				return NctsTypeOfSecurityList.Codes.BTH;
			default:
				return ZString.Empty;
		}
	}

	void UpdatePrincipal(NctsHeader nctsHeader, INT043ResponseDetail response)
	{
		var principal = nctsHeader.Principal;

		principal.E2_AddressOverride = true;
		principal.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
		principal.E2_GovRegNum = ((ZString)response.HolderOfTheTransitProcedureId).Left(principal.E2_GovRegNumInfo.MaxLength);
		principal.E2_CompanyName = ((ZString)response.HolderOfTheTransitProcedureName).Left(principal.E2_CompanyNameInfo.MaxLength);
		principal.E2_AdditionalAddressInformation = ((ZString)response.HolderOfTheTransitProcedureAddressCareOf).Left(principal.E2_AdditionalAddressInformationInfo.MaxLength);
		principal.E2_City = ((ZString)response.HolderOfTheTransitProcedureAddressCity).Left(principal.E2_CityInfo.MaxLength);
		principal.E2_RN_NKCountryCode = ((ZString)response.HolderOfTheTransitProcedureAddressCountry).Left(principal.E2_RN_NKCountryCodeInfo.MaxLength);
		principal.E2_Postcode = ((ZString)response.HolderOfTheTransitProcedureAddressPostcode).Left(principal.E2_PostcodeInfo.MaxLength);
		principal.E2_Address1 = ((ZString)response.HolderOfTheTransitProcedureAddressStreetAndNumber).Left(principal.E2_Address1Info.MaxLength);

		ZString tirIdentificationNumber = response.HolderOfTheTransitProcedureTIRIdentificationNumber;
		if (tirIdentificationNumber.IsEmpty)
		{
			principal.DocAddressNumbers.Find(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, Core.Constants.CountryCodes.Switzerland)?.Delete();
		}
		else
		{
			var addressNumber = principal.DocAddressNumbers.FindOrCreate(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, Core.Constants.CountryCodes.Switzerland);
			addressNumber.E2N_Number = tirIdentificationNumber.Left(addressNumber.E2N_NumberInfo.MaxLength);
		}
	}

	void UpdateOrCreateCustomsOffice(NctsHeader nctsHeader, ZString code, ZString referenceNumber)
	{
		(nctsHeader.ArrivalMovementHeader.CustomsOffices.GetFirstElementHaving(code) ?? nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew(code)).CY_Data = referenceNumber;
	}

	void AddRelatedArrivalMovement(NctsArrivalMovementHeader initialMovementHeader, NctsArrivalMovementHeader relatedMovementHeader)
	{
		initialMovementHeader.RelatedArrivalMovements.AddPivotFor(relatedMovementHeader);
	}

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT043ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		var movementHeader = nctsHeader.ArrivalMovementHeader;
		nctsHeader.MovementReferenceNumberSetter(customsResponse.MRN);
		movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		movementHeader.BM_Phase = NCTS5ArrivalPhaseList.Codes.Arrival;
		UpdateMasterMovement(nctsHeader.ArrivalMovementHeader);
	}

	void UpdateMasterMovement(NctsArrivalMovementHeader movementHeader)
	{
		var masterMovement = movementHeader.MasterArrivalMovementHeader;
		if (masterMovement != null && masterMovement.RelatedArrivalMovements.Cast<RelatedArrivalMovementGenPivot>().All(x => IsNT043Received(x.ChildMovement)))
		{
			masterMovement.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		}
	}

	void AddIncidents(NctsHeader nctsHeader, IEnumerable<IIncident> incidents)
	{
		if (incidents != null)
		{
			if (incidents.Any())
			{
				nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			}
			foreach (var incident in incidents)
			{
				var inBondEvent = nctsHeader.EnRouteIncidents.AddNew();
				inBondEvent.BN_CustomsStatus = IncidentCustomsStatusList.Codes.CUS;
				inBondEvent.BN_Information = incident.Text.ToZString(inBondEvent.BN_InformationInfo.MaxLength);
				inBondEvent.BN_IncidentCode = incident.Code.ToStringInvariantCulture();
				inBondEvent.BN_EndorsementAuthority = incident.EndorsementAuthority.ToZString(inBondEvent.BN_EndorsementAuthorityInfo.MaxLength);
				inBondEvent.BN_EndorsementCountryCode = incident.EndorsementCountry.ToZString(inBondEvent.BN_EndorsementCountryCodeInfo.MaxLength);
				inBondEvent.BN_EndorsementDate = incident.EndorsementDate ?? ZDateTime.Empty;
				inBondEvent.BN_EndorsementPlace = incident.EndorsementPlace.ToZString(inBondEvent.BN_EndorsementPlaceInfo.MaxLength);
				inBondEvent.BN_EventCountryCode = incident.EndorsementCountry.ToZString(inBondEvent.BN_EventCountryCodeInfo.MaxLength);

				var qualifier = incident.LocationQualifierOfIdentification?.ToUpperInvariant() ?? ZString.Empty;
				var isValidQualifier = qualifier == CusGoodsLocationQualifierList.Codes.UnLocode || qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates || qualifier == CusGoodsLocationQualifierList.Codes.Address;
				if (isValidQualifier)
				{
					var goodsLocation = inBondEvent.GoodsLocation;
					goodsLocation.CGL_Qualifier = qualifier;

					switch (qualifier)
					{
						case CusGoodsLocationQualifierList.Codes.UnLocode:
							goodsLocation.CGL_AdditionalIdentifier = incident.LocationUNLocode.ToZString(inBondEvent.GoodsLocation.CGL_AdditionalIdentifierInfo.MaxLength);
							break;
						case CusGoodsLocationQualifierList.Codes.GnssCoordinates:
							goodsLocation.Address.E2_GeoLocation = ZGeography.CreatePoint($"{incident.LocationLongitude} {incident.LocationLatitude}");
							break;
						case CusGoodsLocationQualifierList.Codes.Address:
							var address = goodsLocation.Address;
							address.E2_Address1 = incident.LocationAddress?.StreetAndNumber.ToZString(address.E2_Address1Info.MaxLength) ?? ZString.Empty;
							address.E2_City = incident.LocationAddress?.City.ToZString(address.E2_CityInfo.MaxLength) ?? ZString.Empty;
							address.E2_Postcode = incident.LocationAddress?.Postcode.ToZString(address.E2_PostcodeInfo.MaxLength) ?? ZString.Empty;
							address.E2_RN_NKCountryCode = incident.LocationCountry.ToZString(address.E2_RN_NKCountryCodeInfo.MaxLength);
							break;
						default:
							break;
					}
				}

				if (incident.Transhipment != null)
				{
					inBondEvent.BN_TransportAtDepartureType = incident.Transhipment.TransportMeansTypeOfIdentification.ToZString(inBondEvent.BN_TransportAtDepartureTypeInfo.MaxLength);
					inBondEvent.BN_TransportAtDepartureID = incident.Transhipment.TransportMeansIdentificationNumber.ToZString(inBondEvent.BN_TransportAtDepartureIDInfo.MaxLength);
					inBondEvent.BN_RN_NKTransportAtDepartureIDNationality = incident.Transhipment.TransportMeansNationality.ToZString(inBondEvent.BN_RN_NKTransportAtDepartureIDNationalityInfo.MaxLength);
				}

				AddIncidentContainers(inBondEvent.IncidentContainers, incident);
			}
		}
	}

	void AddIncidentContainers(EnRouteIncidentNctsContainerCollection containers, IIncident incident)
	{
		if (incident.TransportEquipments != null)
		{
			foreach (var transportEquipment in incident.TransportEquipments)
			{
				var container = containers.AddNew();
				container.BC_ContainerNum = transportEquipment.ContainerIdentificationNumber.ToZString(container.BC_ContainerNumInfo.MaxLength);
				container.BC_SequenceNumber = (ZShort)transportEquipment.SequenceNumber;

				if (incident.Transhipment != null)
				{
					container.BC_Mode = incident.Transhipment.ContainerIndicator ? ContainerModes.Containerised : ContainerModes.NonContainerised;
				}
				else
				{
					container.BC_Mode = transportEquipment.ContainerIdentificationNumber.IsNullOrEmpty() ? ContainerModes.NonContainerised : ContainerModes.Containerised;
				}

				if (transportEquipment.Seals != null)
				{
					if (transportEquipment.Seals.Count >= 1)
					{
						container.BC_Seal1 = transportEquipment.Seals.ElementAt(0).Identifier.ToZString(container.BC_Seal1Info.MaxLength);
					}
					if (transportEquipment.Seals.Count >= 2)
					{
						container.BC_Seal2 = transportEquipment.Seals.ElementAt(1).Identifier.ToZString(container.BC_Seal2Info.MaxLength);
					}
					foreach (var transportSeal in transportEquipment.Seals.Skip(2))
					{
						var containerSeal = container.Seals.AddNew();
						containerSeal.BK_SequenceNumber = (ZShort)transportSeal.SequenceNumber;
						containerSeal.BK_SealNumber = transportSeal.Identifier.ToZString(containerSeal.BK_SealNumberInfo.MaxLength);
					}
				}

				if (transportEquipment.GoodsReferences != null)
				{
					foreach (var goodsReference in transportEquipment.GoodsReferences)
					{
						var itemNumber = container.ItemNumbers.AddNew();
						itemNumber.CY_Data = goodsReference.DeclarationGoodsItemNumber.ToStringInvariantCulture();
						itemNumber.CY_Order = (ZShort)goodsReference.SequenceNumber;
					}
				}
			}
		}
	}

	bool IsNT043Received(NctsArrivalMovementHeader movementHeader)
	{
		switch (movementHeader.BM_CustomsStatus)
		{
			case NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted:
			case NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks:
			case NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease:
				return true;
		}
		return false;
	}
}
