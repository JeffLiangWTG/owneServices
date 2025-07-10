using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTRAC_v515.CCTRACV1Sal;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business;

public class QueryNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cctracv1Sal, QueryNCTSMessagePrettyFormatter>
{
	public QueryNCTSResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	const string NewDataWeightUQ = "KG";
	ZString NotWrittenOffText => Res.GetString("3437055E-50ED-4946-89FB-F58E3A9908EF", "Not Written Off");

	protected override string MessageFriendlyNameCore => (NoResString)"NCTS Query Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCTRACV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.TransitNcts5Query };

	protected override ZBool IsQueryMessage => true;

	protected override QueryNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cctracv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new QueryNCTSMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Cctracv1Sal response, EDIMessage message, NctsHeader nctsHeader)
	{
		if (response.DatosRespuestaCorrecta is null)
		{
			return ZString.Empty;
		}

		var extraDataForPrettyFormatter = ZString.Empty;
		var correctResponseData = response.DatosRespuestaCorrecta;
		var managementData = correctResponseData.Ncts5DatosGestion;
		if (response.ControlRespuesta.TipoRespuesta == MessageStatusList.Codes.OK)
		{
			var isDepartureMovement = nctsHeader.IsDepartureMovement;

			var circuit = isDepartureMovement ? managementData.CircuitoExpedicion : managementData.CircuitoRecepcion;
			var mrnIssueDate = isDepartureMovement ? managementData.FechaAdmision : managementData.FechaHoraRecepcion;

			var csvClearance = isDepartureMovement ? managementData.CsVdeDat : string.Empty;
			var clearanceDate = isDepartureMovement ? managementData.FechaLevante : null;
			var limitDateOfArrival = isDepartureMovement ? managementData.FechaLimiteLlegada : null;

			SetEntryNumbers(message, nctsHeader, correctResponseData.TransitOperation.Mrn, circuit, mrnIssueDate, csvClearance, ZString.Empty, clearanceDate, limitDateOfArrival, ZString.Empty, clearanceNoDependOfResponseCode: true);

			var responseStatus = managementData.Estado;
			if (isDepartureMovement)
			{
				SetDepartureStatusQuery(nctsHeader.MovementHeader, responseStatus);

				var dataFromWriteOff = GetDataFromWriteOffAndAddGuaranteesWriteOffIfNeeded(nctsHeader, responseStatus, managementData.FechaUltimacionCompleta);
				extraDataForPrettyFormatter += MessageProcessorConstants.ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter + dataFromWriteOff + MessageProcessorConstants.ExtraDataFromProcessing.SymbolToSeparateExtraDataForPrettyFormatter;

				UpdatePreDeclarationIfNeeded(nctsHeader, correctResponseData);
			}
			else
			{
				var arrivalMovement = nctsHeader.ArrivalMovementHeader;
				var anyHouses = nctsHeader.ArrivalMovementHeader.MovementDetails.Any();
				SetArrivalStatusQuery(arrivalMovement, responseStatus);
				var customsStatus = arrivalMovement.BM_CustomsStatus;
				bool CheckCustomsStatusRequiringNoHouses() => ArrivalCustomsStatusRequiringNoHouses.Contains(customsStatus) && !anyHouses;

				if (customsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted || CheckCustomsStatusRequiringNoHouses())
				{
					SetDataForUnloading(correctResponseData, nctsHeader);
				}

				SetReleaseDate(managementData.FechaUltimacionCompleta, nctsHeader);
			}
		}

		return extraDataForPrettyFormatter;
	}

	internal ImmutableHashSet<string> ArrivalCustomsStatusRequiringNoHouses = ImmutableHashSet.Create(
			ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
			ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);

	protected void SetDepartureStatusQuery(NctsDepartureMovementHeader nctsCommonMovementHeader, ZString responseCode)
	{
		nctsCommonMovementHeader.BM_CustomsStatus = (string)responseCode switch
		{
			Ncts5TransitStatusList.Codes.PreDeclaration => ESNCTS5DepartureCustomsStatusList.Codes.PreLodged,
			Ncts5TransitStatusList.Codes.PendingGuarantee => ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance,
			Ncts5TransitStatusList.Codes.PendingDispatch => ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
			Ncts5TransitStatusList.Codes.DeclarationCancelled => ESNCTS5DepartureCustomsStatusList.Codes.Cancelled,
			Ncts5TransitStatusList.Codes.Invalidated or Ncts5TransitStatusList.Codes.PreDeclarationInvalidated => ESNCTS5DepartureCustomsStatusList.Codes.Invalidated,
			Ncts5TransitStatusList.Codes.InvalidatedByGuarantee => ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
			Ncts5TransitStatusList.Codes.NotCleared => ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit,
			_ => ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
		};
	}

	ZString GetDataFromWriteOffAndAddGuaranteesWriteOffIfNeeded(NctsHeader nctsHeader, string responseStatus, DateTime? admissionDate)
	{
		if (responseStatus == Ncts5TransitStatusList.Codes.Completed && nctsHeader.MovementHeader.Guarantees.Count > 0 && admissionDate.HasValue)
		{
			var writeOffTransactionCreator = new WriteOffTransactionCreator();
			return writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsNcts(nctsHeader, admissionDate.Value, Logger);
		}
		return NotWrittenOffText;
	}

	void UpdatePreDeclarationIfNeeded(NctsHeader nctsHeader, DatosRespuestaCorrectaType70 correctResponseData)
	{
		if (nctsHeader.UpdatePreDeclaration)
		{
			SetDataForPreDeclaration(correctResponseData, nctsHeader);
			nctsHeader.UpdatePreDeclaration = false;
			nctsHeader.BH_ReleaseStatus = ZString.Empty;
		}
	}

	protected void SetArrivalStatusQuery(NctsArrivalMovementHeader nctsCommonMovementHeader, ZString responseCode)
	{
		nctsCommonMovementHeader.BM_CustomsStatus = (string)responseCode switch
		{
			Ncts5TransitStatusList.Codes.PendingResolutionOfDiscrepancy => ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease,
			Ncts5TransitStatusList.Codes.PendingDispatch => ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl,
			Ncts5TransitStatusList.Codes.Received or Ncts5TransitStatusList.Codes.LiquidationInitiated => ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted,
			Ncts5TransitStatusList.Codes.Completed or Ncts5TransitStatusList.Codes.UltimatedForPayment => ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
			_ => ZString.Empty,
		};
	}

	void SetDataForUnloading(DatosRespuestaCorrectaType70 correctResponse, NctsHeader nctsHeader)
	{
		var responseConsignment = correctResponse.Consignment;
		nctsHeader.ArrivalMovementHeader.BM_GrossWeight = responseConsignment.GrossMass;
		nctsHeader.ArrivalMovementHeader.BM_InlandTransportMode = responseConsignment.InlandModeOfTransport;

		SetContainerDataForUnloading(responseConsignment, nctsHeader);
		SetTransportInfoDataForUnloading(responseConsignment, nctsHeader);
		SetSupportingDocumentData(responseConsignment, nctsHeader, true);
		SetTransportDocumentData(responseConsignment, nctsHeader, true);
		SetReferenceDocumentData(responseConsignment, nctsHeader, true);
		SetBillDataForUnloading(responseConsignment, nctsHeader);
	}

	void SetContainerDataForUnloading(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		var arrivalContainers = nctsHeader.ArrivalHeaderContainers;
		foreach (var transportEq in responseConsignment.TransportEquipment)
		{
			var transportEqSeqNum = ParseNumberToShort(transportEq.SequenceNumber);
			var container = arrivalContainers.Cast<NctsArrivalHeaderContainer>().FirstOrDefault(x => x.BC_SequenceNumber == transportEqSeqNum);
			if (container == null)
			{
				container = arrivalContainers.AddNew();
				container.BC_SequenceNumber = transportEqSeqNum;
			}

			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var containerNum = transportEq.ContainerIdentificationNumber;
			container.BC_ContainerNum = containerNum;
			container.BC_Mode = containerNum.IsEmpty() ? Core.Constants.ContainerModes.NonContainerised : Core.Constants.ContainerModes.Containerised;

			var containerSeals = container.Seals;
			foreach (var transportEqSeals in transportEq.Seal)
			{
				var transportEqSealsSeqNum = ParseNumberToShort(transportEqSeals.SequenceNumber);
				var seal = containerSeals.Cast<EU.NCTS.Business.CusSeal>().FirstOrDefault(x => x.BK_SequenceNumber == transportEqSealsSeqNum);
				if (seal == null)
				{
					seal = containerSeals.AddNew();
					seal.BK_SequenceNumber = transportEqSealsSeqNum;
				}

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				seal.BK_SealNumber = transportEqSeals.Identifier;
			}
		}
	}

	void SetTransportInfoDataForUnloading(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		var arrivalTransportInfos = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos;
		foreach (var transportMean in responseConsignment.DepartureTransportMeans)
		{
			var transportMeanSeqNum = ParseNumberToShort(transportMean.SequenceNumber);
			var transportInfo = arrivalTransportInfos.Cast<ArrivalCusTransportMeans>().FirstOrDefault(x => x.TPM_SequenceNumber == transportMeanSeqNum);
			if (transportInfo == null)
			{
				transportInfo = arrivalTransportInfos.AddNew();
				transportInfo.TPM_SequenceNumber = transportMeanSeqNum;
			}

			transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			transportInfo.TPM_TypeOfIdentification = transportMean.TypeOfIdentification;
			transportInfo.TPM_IdentificationNumber = transportMean.IdentificationNumber;
			transportInfo.TPM_RN_NKTransportNationality = transportMean.Nationality;
		}
	}

	void SetBillDataForUnloading(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		var bills = nctsHeader.Bills;
		foreach (var houseConsignment in responseConsignment.HouseConsignment)
		{
			var houseConsignmentSeqNum = houseConsignment.SequenceNumber;
			var bill = bills.Cast<NctsBill>().FirstOrDefault(x => x.MovementDetail.B9_SeqNo == houseConsignmentSeqNum);
			if (bill == null)
			{
				bill = bills.AddNew();
				bill.MovementDetail.B9_SeqNo = houseConsignmentSeqNum;
			}

			bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			bill.B0_Weight = houseConsignment.GrossMass;
			bill.B0_WeightUQ = NewDataWeightUQ;

			SetTransportInfoDataForUnloading(houseConsignment, bill);
			SetSupportingDocumentData(houseConsignment, bill, true);
			SetTransportDocumentData(houseConsignment, bill, true);
			SetReferenceDocumentData(houseConsignment, bill, true);
			SetGoodsItemDataForUnloading(responseConsignment, houseConsignment, bill, nctsHeader);
		}
	}

	void SetTransportInfoDataForUnloading(HouseConsignmentType70 responseHouseConsignment, NctsBill bill)
	{
		var arrivalTransportInfos = bill.ArrivalTransportInfos;
		foreach (var transportMean in responseHouseConsignment.DepartureTransportMeans)
		{
			var transportMeanSeqNum = ParseNumberToShort(transportMean.SequenceNumber);
			var transportInfo = arrivalTransportInfos.Cast<ArrivalCusTransportMeans>().FirstOrDefault(x => x.TPM_SequenceNumber == transportMeanSeqNum);
			if (transportInfo == null)
			{
				transportInfo = arrivalTransportInfos.AddNew();
				transportInfo.TPM_SequenceNumber = transportMeanSeqNum;
			}

			transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			transportInfo.TPM_TypeOfIdentification = transportMean.TypeOfIdentification;
			transportInfo.TPM_IdentificationNumber = transportMean.IdentificationNumber;
			transportInfo.TPM_RN_NKTransportNationality = transportMean.Nationality;
		}
	}

	void SetGoodsItemDataForUnloading(ConsignmentType70 responseConsignment, HouseConsignmentType70 responseHouseConsignment, NctsBill bill, NctsHeader nctsHeader)
	{
		var goodsItems = bill.ArrivalGoodsItems;
		foreach (var consignmentItem in responseHouseConsignment.ConsignmentItem)
		{
			var consignmentItemSeqNum = ParseNumberToShort(consignmentItem.GoodsItemNumber);
			var goodsItem = goodsItems.Cast<NctsArrivalCargoDesc>().FirstOrDefault(x => x.BY_LineNo == consignmentItemSeqNum);
			if (goodsItem == null)
			{
				goodsItem = goodsItems.AddNew();
				goodsItem.BY_LineNo = consignmentItemSeqNum;
			}

			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var consignmentItemDeclarationGoodsItemNumber = ParseNumberToShort(consignmentItem.DeclarationGoodsItemNumber);
			goodsItem.BY_DeclarationGoodsItemNumber = consignmentItemDeclarationGoodsItemNumber;
			var commodity = consignmentItem.Commodity;
			goodsItem.BY_Description = commodity.DescriptionOfGoods;
			goodsItem.BY_CusC4Number = commodity.CusCode;

			var commodityCode = commodity.CommodityCode;
			goodsItem.BY_HarmonisedTariff = commodityCode?.HarmonizedSystemSubHeadingCode + commodityCode?.CombinedNomenclatureCode;

			var previousDocumentN337WithQtyAndKGM = consignmentItem.PreviousDocument.FirstOrDefault(x => x.Type == ES.Business.PreviousDocumentHelper.PreviousDocumentCodeN337 &&
																											x.Quantity != 0 &&
																											x.MeasurementUnitAndQualifier == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram);

			var isIsMovementReferenceNumberESAndHasPreviousDocumentN337WithQtyAndKGM = nctsHeader.IsMovementReferenceNumberES && previousDocumentN337WithQtyAndKGM != null;
			var goodsMeasure = commodity.GoodsMeasure;
			goodsItem.BY_GrossWeight = (ZDecimal)(isIsMovementReferenceNumberESAndHasPreviousDocumentN337WithQtyAndKGM ? previousDocumentN337WithQtyAndKGM.Quantity : goodsMeasure?.GrossMass ?? ZDecimal.Zero);
			goodsItem.BY_GrossWeightUnit = isIsMovementReferenceNumberESAndHasPreviousDocumentN337WithQtyAndKGM ? Core.Constants.Weight.Kilograms : NewDataWeightUQ;
			var netmass = goodsMeasure?.NetMass;
			goodsItem.BY_NetWeight = netmass != null ? (ZDecimal)netmass : ZDecimal.Zero;
			goodsItem.BY_NetWeightUnit = NewDataWeightUQ;

			SetPackageDataForUnloading(responseConsignment, consignmentItem, goodsItem, nctsHeader);
			SetSupportingDocumentData(consignmentItem, goodsItem, true);
			SetTransportDocumentData(consignmentItem, goodsItem, true, bill);
			SetReferenceDocumentData(consignmentItem, goodsItem, true);
		}
	}

	void SetPackageDataForUnloading(ConsignmentType70 responseConsignment, ConsignmentItemType70 responseConsignmentItem, NctsArrivalCargoDesc goodsItem, NctsHeader nctsHeader)
	{
		var packages = goodsItem.Packages;

		var containersAsscosiated = new List<NctsArrivalHeaderContainer>();
		var declarationGoodsItemString = goodsItem.BY_DeclarationGoodsItemNumber.ToString();

		foreach (var transportEq in responseConsignment.TransportEquipment)
		{
			if (transportEq.GoodsReference.Select(x => x.DeclarationGoodsItemNumber).Contains(declarationGoodsItemString))
			{
				containersAsscosiated.Add(nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().FirstOrDefault(x => x.BC_SequenceNumber.ToString() == transportEq.SequenceNumber));
			}
		}

		foreach (var responsePackage in responseConsignmentItem.Packaging)
		{
			var responsePackageSeqNum = ParseNumberToShort(responsePackage.SequenceNumber);
			var pack = packages.Cast<NctsPackage>().FirstOrDefault(x => x.B5_SequenceNumber == responsePackageSeqNum);
			if (pack == null)
			{
				pack = packages.AddNew();
				pack.B5_SequenceNumber = responsePackageSeqNum;
			}

			var packType = responsePackage.TypeOfPackages;

			pack.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			pack.IsDataLoadFromDeparture = true;
			pack.B5_UnitType = packType;
			ZLong.TryParse(responsePackage.NumberOfPackages, out var responsePackageNumberOfPackages);
			pack.B5_UnitCount = responsePackageNumberOfPackages;

			if (packType == ES.Business.UniversalReferenceConstants.RefCusCodeList.PackageType.Frame)
			{
				SetVehicleMarks(responsePackage, pack);
				pack.B5_MarksAndNumbers = ZString.Empty;
			}
			else
			{
				pack.B5_MarksAndNumbers = responsePackage.ShippingMarks;
				pack.B5_PackageID = ZString.Empty;
				pack.B5_Brand = ZString.Empty;
				pack.B5_Model = ZString.Empty;
			}

			var pivots = pack.ContainersPivot.Cast<GenPivot>().GroupBy(x => x.XX_Relation2ID).ToDictionary(x => x.Key, y => y.ToList());
			foreach (var cont in containersAsscosiated)
			{
				var containerPK = cont.PK;
				if (pivots.TryGetValue(containerPK, out var list))
				{
					pivots.Remove(containerPK);
				}
				else
				{
					pack.ContainersPivot.AddPivotFor(cont);
				}
			}
			pivots.Values.ForEach(x => x.DeleteAll());
		}
	}

	void SetVehicleMarks(PackagingType70 responsePackage, NctsPackage pack)
	{
		var marksSplit = responsePackage.ShippingMarks.Split(VehiclesSeparator);
		var marksSplitCount = marksSplit.Length;
		pack.B5_PackageID = marksSplitCount > 0 ? marksSplit[0] : ZString.Empty;
		pack.B5_Brand = marksSplitCount > 1 ? marksSplit[1] : ZString.Empty;
		pack.B5_Model = marksSplitCount > 2 ? marksSplit[2] : ZString.Empty;
	}

	void SetSupportingDocumentData(ConsignmentItemType70 responseConsignmentItem, NctsCommonCargoDesc goodsItem, ZBool isArrival)
	{
		if (!isArrival)
		{
			goodsItem.SupportingDocuments.DeleteAll();
		}

		var supportingDocuments = goodsItem.SupportingDocuments;
		foreach (var responseSupDoc in responseConsignmentItem.SupportingDocument)
		{
			var responseSupDocSeqNum = ParseNumberToInt(responseSupDoc.SequenceNumber);
			var supDoc = supportingDocuments.FirstOrDefault(x => x.CSI_LineNo == responseSupDocSeqNum) ?? supportingDocuments.AddNew();

			if (isArrival)
			{
				supDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
			supDoc.CSI_Code = responseSupDoc.Type;
			supDoc.CSI_ReferenceNumber = responseSupDoc.ReferenceNumber;
			supDoc.CSI_LineNo = responseSupDocSeqNum;
		}
	}

	void SetTransportDocumentData(ConsignmentItemType70 responseConsignmentItem, NctsCommonCargoDesc goodsItem, ZBool isArrival, NctsBill bill)
	{
		var transportDocuments = isArrival
									? goodsItem.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
									: bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument);

		foreach (var responseTraDoc in responseConsignmentItem.TransportDocument)
		{
			var responseTraDocSeqNum = ParseNumberToInt(responseTraDoc.SequenceNumber);
			var docType = responseTraDoc.Type;
			var docRef = responseTraDoc.ReferenceNumber;

			var traDoc = isArrival
							? transportDocuments.FirstOrDefault(x => x.CSI_LineNo == responseTraDocSeqNum)
							: transportDocuments.FirstOrDefault(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docRef);
			if (traDoc == null)
			{
				traDoc = isArrival ? goodsItem.AdditionalInfos.AddNew() : bill.AdditionalDocuments.AddNew();
				traDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			}

			if (isArrival)
			{
				traDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
			traDoc.CSI_Code = docType;
			traDoc.CSI_ReferenceNumber = docRef;
			traDoc.CSI_LineNo = responseTraDocSeqNum;
		}
	}

	void SetReferenceDocumentData(ConsignmentItemType70 responseConsignmentItem, NctsCommonCargoDesc goodsItem, ZBool isArrival)
	{
		var referenceDocuments = goodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference);

		foreach (var responseRefDoc in responseConsignmentItem.AdditionalReference)
		{
			var responseRefDocSeqNum = ParseNumberToInt(responseRefDoc.SequenceNumber);
			var refDoc = referenceDocuments.FirstOrDefault(x => x.CSI_LineNo == responseRefDocSeqNum);
			if (refDoc == null)
			{
				refDoc = goodsItem.AdditionalInfos.AddNew();
				refDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			}

			if (isArrival)
			{
				refDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
			refDoc.CSI_Code = responseRefDoc.Type;
			refDoc.CSI_ReferenceNumber = responseRefDoc.ReferenceNumber;
			refDoc.CSI_LineNo = responseRefDocSeqNum;
		}
	}

	void SetDataForPreDeclaration(DatosRespuestaCorrectaType70 correctResponse, NctsHeader nctsHeader)
	{
		var responseTransitOperation = correctResponse.TransitOperation;
		var movementHeader = nctsHeader.MovementHeader;

		movementHeader.BM_InBondEntryType = responseTransitOperation.DeclarationType;
		movementHeader.TirCarnetNumber = responseTransitOperation.TirCarnetNumber;
		movementHeader.BM_ReducedDatasetIndicator = responseTransitOperation.ReducedDatasetIndicator == Flag.Item1;
		movementHeader.BM_SpecificCircumstance = responseTransitOperation.SpecificCircumstanceIndicator;

		SetAuthorizationsForPreDeclaration(correctResponse, nctsHeader);
		SetCustomsOfficesForPreDeclaration(correctResponse, nctsHeader);
		SetGuaranteesForPreDeclaration(correctResponse, nctsHeader);

		var responseConsignment = correctResponse.Consignment;

		var responseCountryOfDispatch = responseConsignment.CountryOfDispatch;
		if (!responseCountryOfDispatch.IsEmpty())
		{
			movementHeader.BM_RN_NKCountryOfDispatch = responseCountryOfDispatch;
		}

		var responseCountryOfDestination = responseConsignment.CountryOfDestination;
		if (!responseCountryOfDestination.IsEmpty())
		{
			movementHeader.BM_RL_NKDestinationPort = responseCountryOfDestination;
		}

		var responseInlandModeOfTransport = responseConsignment.InlandModeOfTransport;
		if (!responseInlandModeOfTransport.IsEmpty())
		{
			movementHeader.InlandTransportModeAtDeparture = responseInlandModeOfTransport;
		}

		var responseModeOfTransportAtTheBorder = responseConsignment.ModeOfTransportAtTheBorder;
		if (!responseModeOfTransportAtTheBorder.IsEmpty())
		{
			movementHeader.BM_ExportTransportMode = responseModeOfTransportAtTheBorder;
		}

		movementHeader.BM_GrossWeight = responseConsignment.GrossMass;
		movementHeader.BM_UniqueConsignmentReference = responseConsignment.ReferenceNumberUcr;

		var responseMethodOfPayment = responseConsignment.TransportCharges?.MethodOfPayment ?? ZString.Empty;
		if (!responseMethodOfPayment.IsEmpty())
		{
			movementHeader.BM_MethodOfPayment = responseMethodOfPayment;
		}

		SetGoodsLocation(responseConsignment, movementHeader);
		SetAdditionalSuppyChainActorsForPreDeclaration(responseConsignment, movementHeader);
		SetContainersForPreDeclaration(responseConsignment, nctsHeader);
		SetDeparturetransportMeansForPreDeclaration(responseConsignment, movementHeader);
		SetCountriesOfRoutingForPreDeclaration(responseConsignment, nctsHeader);
		SetBordertransportMeansForPreDeclaration(responseConsignment, movementHeader);
		SetPlacesOfLoadingAndUnloadingForPreDeclaration(responseConsignment, movementHeader);

		SetPreviousDocumentData(responseConsignment, nctsHeader);
		SetSupportingDocumentData(responseConsignment, nctsHeader, false);
		nctsHeader.AdditionalDocuments.RemoveAndDeleteAll();
		SetTransportDocumentData(responseConsignment, nctsHeader, false);
		SetReferenceDocumentData(responseConsignment, nctsHeader, false);
		SetAdditionalInformationDocumentData(responseConsignment, nctsHeader);

		SetBillsForPreDeclaration(responseConsignment, nctsHeader);

		ResetGuaranteesOverrideFlagIfNeeded(nctsHeader);
	}

	void SetAuthorizationsForPreDeclaration(DatosRespuestaCorrectaType70 correctResponse, NctsHeader nctsHeader)
	{
		nctsHeader.MovementHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
		foreach (var responseAuth in correctResponse.Authorisation)
		{
			var auth = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			var responseAuthType = responseAuth.Type;
			var mappedCode = GetMappedCode(responseAuthType);
			auth.AGC_Code = mappedCode.IsEmpty ? responseAuthType : mappedCode;
			auth.AGC_Number = responseAuth.ReferenceNumber;
		}

		ZString GetMappedCode(ZString customsCode)
		{
			var countryCode = nctsHeader.CountryCode;
			var countryMappedCode = ZZRefCusMapCombined.MapCustomsCodeToCW1Code(nctsHeader.Factory, countryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, customsCode, ZDateTime.Today);
			return countryMappedCode.IsEmpty
									? ZZRefCusMapCombined.MapCustomsCodeToCW1Code(nctsHeader.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, customsCode, ZDateTime.Today)
									: countryMappedCode;
		}
	}

	void SetCustomsOfficesForPreDeclaration(DatosRespuestaCorrectaType70 correctResponse, NctsHeader nctsHeader)
	{
		var customsOffices = nctsHeader.CommonMovementHeader.CustomsOffices;

		customsOffices.RemoveAndDeleteAll();

		AddOffice(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, correctResponse.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty);
		AddOffice(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, correctResponse.CustomsOfficeOfDestinationDeclared?.ReferenceNumber ?? ZString.Empty);

		foreach (var office in correctResponse.CustomsOfficeOfTransitDeclared)
		{
			var officeSequenceNumber = ParseNumberToShort(office.SequenceNumber);
			AddOffice(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, office.ReferenceNumber, officeSequenceNumber);
		}

		foreach (var office in correctResponse.CustomsOfficeOfExitForTransitDeclared)
		{
			var officeSequenceNumber = ParseNumberToShort(office.SequenceNumber);
			AddOffice(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit, office.ReferenceNumber, officeSequenceNumber);
		}

		void AddOffice(ZString code, ZString responseOffice, short seqNum = 0)
		{
			if (!responseOffice.IsEmpty)
			{
				var office = customsOffices.AddNew();
				office.CY_Code = code;
				office.CY_Data = responseOffice;
				if (seqNum != 0)
				{
					office.CY_Order = seqNum;
				}
			}
		}
	}

	void SetGuaranteesForPreDeclaration(DatosRespuestaCorrectaType70 correctResponse, NctsHeader nctsHeader)
	{
		nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
		foreach (var responseGuarantee in correctResponse.Guarantee)
		{
			var responseGuaranteeType = responseGuarantee.GuaranteeType;

			foreach (var responseGuaRef in responseGuarantee.GuaranteeReference)
			{
				var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee.PW_BondType = responseGuaranteeType;
				guarantee.PW_BondNumber = responseGuaRef.Grn;
				guarantee.PW_Override = true;
				guarantee.PW_Password = responseGuaRef.AccessCode;
				guarantee.PW_BondAmount = responseGuaRef.AmountToBeCovered ?? ZDecimal.Zero;
			}
		}
	}

	void ResetGuaranteesOverrideFlagIfNeeded(NctsHeader nctsHeader)
	{
		var guaranteesAmount = nctsHeader.MovementHeader.Guarantees.Sum(x => RoundToCurrency(x.PW_BondAmount));
		var goodsItemsLiabilityAmount = nctsHeader.Bills.Sum(b => b.GoodsItems.Sum(x => x.Fees.Sum(f => RoundToCurrency(f.BFE_ChargeAmount))));

		if (guaranteesAmount == goodsItemsLiabilityAmount)
		{
			nctsHeader.MovementHeader.Guarantees.ForEach(x => x.PW_Override = false);
		}

		ZDecimal RoundToCurrency(ZDecimal amount) => Utilities.Round(amount, nctsHeader.DecimalCurrency);
	}

	void SetGoodsLocation(ConsignmentType70 responseConsignment, NctsDepartureMovementHeader movementHeader)
	{
		var locationPrefix = "ES00";
		var responseLocation = responseConsignment.LocationOfGoods?.AuthorisationNumber ?? ZString.Empty;
		movementHeader.GoodsLocation.CGL_AdditionalIdentifier = !responseLocation.IsEmpty() && responseLocation.Length <= 10 ? locationPrefix + responseLocation : responseLocation;
	}

	void SetAdditionalSuppyChainActorsForPreDeclaration(ConsignmentType70 responseConsignment, NctsDepartureMovementHeader movementHeader)
	{
		var responseAdditionalSupplyChainActor = responseConsignment.AdditionalSupplyChainActor;

		var actorsInResponse = responseAdditionalSupplyChainActor.Select(x => (x.Role, x.IdentificationNumber));
		movementHeader.CusSupplyChainActors.Where(x => !actorsInResponse.Contains((x.CFR_Code, x.CFR_Reference))).DeleteAll();

		foreach (var responseActor in responseAdditionalSupplyChainActor)
		{
			var actor = movementHeader.CusSupplyChainActors.FirstOrDefault(x => x.CFR_Code == responseActor.Role && x.CFR_Reference == responseActor.IdentificationNumber)
						?? movementHeader.CusSupplyChainActors.AddNew();
			actor.CFR_Code = responseActor.Role;
			actor.CFR_Reference = responseActor.IdentificationNumber;
		}
	}

	void SetContainersForPreDeclaration(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		nctsHeader.DepartureHeaderContainers.DeleteAll();
		foreach (var transportEq in responseConsignment.TransportEquipment)
		{
			var container = nctsHeader.DepartureHeaderContainers.AddNew();

			var transportEqSeqNum = ParseNumberToShort(transportEq.SequenceNumber);
			container.BC_SequenceNumber = transportEqSeqNum;

			var containerNum = transportEq.ContainerIdentificationNumber;
			container.BC_ContainerNum = containerNum;
			container.BC_Mode = containerNum.IsEmpty() ? Core.Constants.ContainerModes.NonContainerised : Core.Constants.ContainerModes.Containerised;

			foreach (var transportEqSeals in transportEq.Seal)
			{
				var transportEqSealsSeqNum = ParseNumberToShort(transportEqSeals.SequenceNumber);
				if (transportEqSealsSeqNum == 1)
				{
					container.BC_Seal1 = transportEqSeals.Identifier;
				}
				else if (transportEqSealsSeqNum == 2)
				{
					container.BC_Seal2 = transportEqSeals.Identifier;
				}
				else
				{
					var seal = container.AdditionalSeals.AddNew();
					seal.BK_SequenceNumber = transportEqSealsSeqNum;
					seal.BK_SealNumber = transportEqSeals.Identifier;
				}
			}
		}
	}

	void SetDeparturetransportMeansForPreDeclaration(ConsignmentType70 responseConsignment, NctsDepartureMovementHeader movementHeader)
	{
		var transportType = ZString.Empty;
		var transportId = ZString.Empty;
		var transportNationality = ZString.Empty;
		var trailer1Id = ZString.Empty;
		var trailer1Nationality = ZString.Empty;
		var trailer2Id = ZString.Empty;
		var trailer2Nationality = ZString.Empty;

		var seqNumForTrailer1 = "2";
		var seqNumForTrailer2 = "3";

		foreach (var transportMeans in responseConsignment.DepartureTransportMeans)
		{
			var responseType = transportMeans.TypeOfIdentification;

			if (responseType != NctsTransportTypeOfIdList.Codes._31)
			{
				transportType = responseType;
				transportId = transportMeans.IdentificationNumber;
				transportNationality = transportMeans.Nationality;
			}
			else
			{
				transportType = NctsTransportTypeOfIdList.Codes._30;
				if (transportMeans.SequenceNumber == seqNumForTrailer1)
				{
					trailer1Id = transportMeans.IdentificationNumber;
					trailer1Nationality = transportMeans.Nationality;
				}
				else if (transportMeans.SequenceNumber == seqNumForTrailer2)
				{
					trailer2Id = transportMeans.IdentificationNumber;
					trailer2Nationality = transportMeans.Nationality;
				}
			}
		}

		movementHeader.TransportTypeAtDeparture = transportType;
		movementHeader.TransportAtDeparture = transportId;
		movementHeader.TransportCountryAtDeparture = transportNationality;
		movementHeader.Trailer1IDAtDeparture = trailer1Id;
		movementHeader.Trailer1NationalityAtDeparture = trailer1Nationality;
		movementHeader.Trailer2IDAtDeparture = trailer2Id;
		movementHeader.Trailer2NationalityAtDeparture = trailer2Nationality;
	}

	void SetCountriesOfRoutingForPreDeclaration(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		nctsHeader.CountriesOfRouting.RemoveAndDeleteAll();
		foreach (var responseCountry in responseConsignment.CountryOfRoutingOfConsignment)
		{
			var country = nctsHeader.CountriesOfRouting.AddNew();
			var responseCountrySeqNum = ParseNumberToShort(responseCountry.SequenceNumber);
			country.CY_Order = responseCountrySeqNum;
			country.CY_Data = responseCountry.Country;
		}
	}

	void SetBordertransportMeansForPreDeclaration(ConsignmentType70 responseConsignment, NctsDepartureMovementHeader movementHeader)
	{
		var customsOfficeAtBorder = ZString.Empty;
		var activeBorderIdentificationType = ZString.Empty;
		var carrierID = ZString.Empty;
		var carrierNationality = ZString.Empty;
		var conveyanceNumber = ZString.Empty;

		var borderTransport = responseConsignment.ActiveBorderTransportMeans.FirstOrDefault();
		if (borderTransport != null)
		{
			customsOfficeAtBorder = borderTransport.CustomsOfficeAtBorderReferenceNumber;
			activeBorderIdentificationType = borderTransport.TypeOfIdentification;
			carrierID = borderTransport.IdentificationNumber;
			carrierNationality = borderTransport.Nationality;
			conveyanceNumber = borderTransport.ConveyanceReferenceNumber;
		}

		movementHeader.BM_CustomsOfficeAtBorder = customsOfficeAtBorder;
		movementHeader.BM_ActiveBorderIdentificationType = activeBorderIdentificationType;
		movementHeader.BM_TOLCarrierID = carrierID;
		movementHeader.BM_RN_NKTOLCarrierNationality = carrierNationality;
		movementHeader.BM_ConveyanceNumber = conveyanceNumber;
	}

	void SetPlacesOfLoadingAndUnloadingForPreDeclaration(ConsignmentType70 responseConsignment, NctsDepartureMovementHeader movementHeader)
	{
		var responsePlaceOfLoading = responseConsignment.PlaceOfLoading;
		if (responsePlaceOfLoading != null)
		{
			var loading = GetPlace(responsePlaceOfLoading.UnLocode, responsePlaceOfLoading.Country, responsePlaceOfLoading.Location);
			if (!loading.IsEmpty)
			{
				movementHeader.BM_PlaceOfLoading = loading;
			}
		}

		var responsePlaceOfUnloading = responseConsignment.PlaceOfUnloading;
		if (responsePlaceOfUnloading != null)
		{
			var unloading = GetPlace(responsePlaceOfUnloading.UnLocode, responsePlaceOfUnloading.Country, responsePlaceOfUnloading.Location);
			if (!unloading.IsEmpty)
			{
				movementHeader.BM_PlaceOfUnloading = unloading;
			}
		}

		ZString GetPlace(string unloco, string country, string location) => !unloco.IsEmpty() ? unloco : country + location;
	}

	void SetPreviousDocumentData(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		nctsHeader.PreviousDocuments.RemoveAndDeleteAll();
		foreach (var responsePrevDoc in responseConsignment.PreviousDocument)
		{
			var prevDoc = nctsHeader.PreviousDocuments.AddNew();

			prevDoc.CSI_Code = responsePrevDoc.Type;
			prevDoc.CSI_ReferenceNumber = responsePrevDoc.ReferenceNumber;
			prevDoc.CSI_ReferenceNumber2 = responsePrevDoc.ComplementOfInformation;

			var responsePrevDocSeqNum = ParseNumberToInt(responsePrevDoc.SequenceNumber);
			prevDoc.CSI_LineNo = responsePrevDocSeqNum;
		}
	}

	void SetSupportingDocumentData(ConsignmentType70 responseConsignment, NctsHeader nctsHeader, ZBool isArrival)
	{
		if (!isArrival)
		{
			nctsHeader.MovementHeader.SupportingDocuments.RemoveAndDeleteAll();
		}

		var supportingDocuments = isArrival ? nctsHeader.ArrivalMovementHeader.SupportingDocuments : nctsHeader.MovementHeader.SupportingDocuments;
		foreach (var responseSupDoc in responseConsignment.SupportingDocument)
		{
			var responseSupDocSeqNum = ParseNumberToInt(responseSupDoc.SequenceNumber);
			var supDoc = supportingDocuments.FirstOrDefault(x => x.CSI_LineNo == responseSupDocSeqNum) ?? supportingDocuments.AddNew();

			if (isArrival)
			{
				supDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}

			supDoc.CSI_Code = responseSupDoc.Type;
			supDoc.CSI_ReferenceNumber = responseSupDoc.ReferenceNumber;
			supDoc.CSI_ReferenceNumber2 = responseSupDoc.ComplementOfInformation;

			var responseSupDocItemNum = ParseNumberToInt(responseSupDoc.DocumentLineItemNumber);
			supDoc.CSI_ItemNumber = responseSupDocItemNum;
			supDoc.CSI_LineNo = responseSupDocSeqNum;
		}
	}

	void SetTransportDocumentData(ConsignmentType70 responseConsignment, NctsHeader nctsHeader, ZBool isArrival)
	{
		var additionalDocuments = isArrival ? nctsHeader.ArrivalMovementHeader.AdditionalDocuments : nctsHeader.AdditionalDocuments;
		foreach (var responseTraDoc in responseConsignment.TransportDocument)
		{
			var responseTraDocSeqNum = ParseNumberToInt(responseTraDoc.SequenceNumber);
			var traDoc = additionalDocuments.FirstOrDefault(x => x.CSI_LineNo == responseTraDocSeqNum && x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument) ?? additionalDocuments.AddNew();
			traDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			traDoc.CSI_Code = responseTraDoc.Type;
			traDoc.CSI_ReferenceNumber = responseTraDoc.ReferenceNumber;
			traDoc.CSI_LineNo = responseTraDocSeqNum;
			if (isArrival)
			{
				traDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
		}
	}

	void SetReferenceDocumentData(ConsignmentType70 responseConsignment, NctsHeader nctsHeader, ZBool isArrival)
	{
		var additionalDocuments = isArrival ? nctsHeader.ArrivalMovementHeader.AdditionalDocuments : nctsHeader.AdditionalDocuments;
		foreach (var responseRefDoc in responseConsignment.AdditionalReference)
		{
			var responseRefDocSeqNum = ParseNumberToInt(responseRefDoc.SequenceNumber);
			var refDoc = additionalDocuments.FirstOrDefault(x => x.CSI_LineNo == responseRefDocSeqNum && x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference) ?? additionalDocuments.AddNew();
			refDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			refDoc.CSI_Code = responseRefDoc.Type;
			refDoc.CSI_ReferenceNumber = responseRefDoc.ReferenceNumber;
			refDoc.CSI_LineNo = responseRefDocSeqNum;
			if (isArrival)
			{
				refDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
		}
	}

	void SetAdditionalInformationDocumentData(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		foreach (var responseInfDoc in responseConsignment.AdditionalInformation)
		{
			var infDoc = nctsHeader.AdditionalDocuments.AddNew();
			infDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			infDoc.CSI_Code = responseInfDoc.Code;
			infDoc.CSI_Description = responseInfDoc.Text;

			var responseInfDocSeqNum = ParseNumberToInt(responseInfDoc.SequenceNumber);
			infDoc.CSI_LineNo = responseInfDocSeqNum;
		}
	}

	void SetBillsForPreDeclaration(ConsignmentType70 responseConsignment, NctsHeader nctsHeader)
	{
		var responseHouseConsignments = responseConsignment.HouseConsignment;

		var bills = nctsHeader.Bills;

		var seqNumsInResponse = responseHouseConsignments.Select(x => (ZString)x.SequenceNumber);
		bills.Where(x => !seqNumsInResponse.Contains(x.MovementDetail.B9_SeqNo)).DeleteAll();

		bills = nctsHeader.Bills;

		foreach (var responseHouseConsignment in responseHouseConsignments)
		{
			var houseConsignmentSeqNum = responseHouseConsignment.SequenceNumber;
			var bill = bills.Cast<NctsBill>().FirstOrDefault(x => x.MovementDetail.B9_SeqNo == houseConsignmentSeqNum);
			if (bill == null)
			{
				bill = bills.AddNew();
				bill.MovementDetail.B9_SeqNo = houseConsignmentSeqNum;
			}

			bill.B0_Weight = responseHouseConsignment.GrossMass;
			bill.B0_WeightUQ = NewDataWeightUQ;

			var responseMethodOfPayment = responseHouseConsignment.TransportCharges?.MethodOfPayment ?? ZString.Empty;
			if (!responseMethodOfPayment.IsEmpty())
			{
				bill.B0_TransportPaymentMethod = responseMethodOfPayment;
			}

			var responseCountryOfDispatch = responseHouseConsignment.CountryOfDispatch;
			if (!responseCountryOfDispatch.IsEmpty())
			{
				bill.B0_RN_NKCountryOfExport = responseCountryOfDispatch;
			}

			SetBillAdditionalSuppyChainActorsForPreDeclaration(responseHouseConsignment, bill);

			SetPreviousDocumentData(responseHouseConsignment, bill);
			SetSupportingDocumentData(responseHouseConsignment, bill, false);
			bill.AdditionalDocuments.RemoveAndDeleteAll();
			SetTransportDocumentData(responseHouseConsignment, bill, false);
			SetReferenceDocumentData(responseHouseConsignment, bill, false);
			SetAdditionalInformationDocumentData(responseHouseConsignment, bill);

			SetGoodsItemsForPreDeclaration(responseConsignment, responseHouseConsignment, bill, nctsHeader);
		}
	}

	void SetBillAdditionalSuppyChainActorsForPreDeclaration(HouseConsignmentType70 responseHouseConsignment, NctsBill bill)
	{
		var responseAdditionalSupplyChainActor = responseHouseConsignment.AdditionalSupplyChainActor;

		var actorsInResponse = responseAdditionalSupplyChainActor.Select(x => (x.Role, x.IdentificationNumber));
		bill.CusSupplyChainActorReferences.Where(x => !actorsInResponse.Contains((x.CFR_Code, x.CFR_Reference))).DeleteAll();

		foreach (var responseActor in responseAdditionalSupplyChainActor)
		{
			var actor = bill.CusSupplyChainActorReferences.FirstOrDefault(x => x.CFR_Code == responseActor.Role && x.CFR_Reference == responseActor.IdentificationNumber)
						?? bill.CusSupplyChainActorReferences.AddNew();
			actor.CFR_Code = responseActor.Role;
			actor.CFR_Reference = responseActor.IdentificationNumber;
		}
	}

	void SetPreviousDocumentData(HouseConsignmentType70 responseHouseConsignment, NctsBill bill)
	{
		bill.PreviousDocuments.RemoveAndDeleteAll();
		foreach (var responsePrevDoc in responseHouseConsignment.PreviousDocument)
		{
			var prevDoc = bill.PreviousDocuments.AddNew();

			prevDoc.CSI_Code = responsePrevDoc.Type;
			prevDoc.CSI_ReferenceNumber = responsePrevDoc.ReferenceNumber;
			prevDoc.CSI_ReferenceNumber2 = responsePrevDoc.ComplementOfInformation;

			var responsePrevDocSeqNum = ParseNumberToInt(responsePrevDoc.SequenceNumber);
			prevDoc.CSI_LineNo = responsePrevDocSeqNum;
		}
	}

	void SetSupportingDocumentData(HouseConsignmentType70 responseHouseConsignment, NctsBill bill, ZBool isArrival)
	{
		if (!isArrival)
		{
			bill.SupportingDocuments.RemoveAndDeleteAll();
		}

		var supportingDocuments = bill.SupportingDocuments;
		foreach (var responseSupDoc in responseHouseConsignment.SupportingDocument)
		{
			var responseSupDocSeqNum = ParseNumberToInt(responseSupDoc.SequenceNumber);
			var supDoc = supportingDocuments.FirstOrDefault(x => x.CSI_LineNo == responseSupDocSeqNum) ?? supportingDocuments.AddNew();

			if (isArrival)
			{
				supDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}

			supDoc.CSI_Code = responseSupDoc.Type;
			supDoc.CSI_ReferenceNumber = responseSupDoc.ReferenceNumber;
			supDoc.CSI_ReferenceNumber2 = responseSupDoc.ComplementOfInformation;
			supDoc.CSI_LineNo = responseSupDocSeqNum;

			var responseSupDocItemNum = ParseNumberToInt(responseSupDoc.DocumentLineItemNumber);
			supDoc.CSI_ItemNumber = responseSupDocItemNum;
		}
	}

	void SetTransportDocumentData(HouseConsignmentType70 responseHouseConsignment, NctsBill bill, ZBool isArrival)
	{
		var additionalDocuments = bill.AdditionalDocuments;
		foreach (var responseTraDoc in responseHouseConsignment.TransportDocument)
		{
			var responseTraDocSeqNum = ParseNumberToInt(responseTraDoc.SequenceNumber);
			var traDoc = additionalDocuments.FirstOrDefault(x => x.CSI_LineNo == responseTraDocSeqNum && x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument) ?? additionalDocuments.AddNew();
			traDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			traDoc.CSI_Code = responseTraDoc.Type;
			traDoc.CSI_ReferenceNumber = responseTraDoc.ReferenceNumber;
			traDoc.CSI_LineNo = responseTraDocSeqNum;
			if (isArrival)
			{
				traDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
		}
	}

	void SetReferenceDocumentData(HouseConsignmentType70 responseHouseConsignment, NctsBill bill, ZBool isArrival)
	{
		var additionalDocuments = bill.AdditionalDocuments;
		foreach (var responseRefDoc in responseHouseConsignment.AdditionalReference)
		{
			var responseRefDocSeqNum = ParseNumberToInt(responseRefDoc.SequenceNumber);
			var refDoc = additionalDocuments.FirstOrDefault(x => x.CSI_LineNo == responseRefDocSeqNum && x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference) ?? additionalDocuments.AddNew();
			refDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			refDoc.CSI_Code = responseRefDoc.Type;
			refDoc.CSI_ReferenceNumber = responseRefDoc.ReferenceNumber;
			refDoc.CSI_LineNo = responseRefDocSeqNum;
			if (isArrival)
			{
				refDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			}
		}
	}

	void SetAdditionalInformationDocumentData(HouseConsignmentType70 responseHouseConsignment, NctsBill bill)
	{
		foreach (var responseInfDoc in responseHouseConsignment.AdditionalInformation)
		{
			var infDoc = bill.AdditionalDocuments.AddNew();
			infDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			infDoc.CSI_Code = responseInfDoc.Code;
			infDoc.CSI_Description = responseInfDoc.Text;

			var responseInfDocSeqNum = ParseNumberToInt(responseInfDoc.SequenceNumber);
			infDoc.CSI_LineNo = responseInfDocSeqNum;
		}
	}

	void SetGoodsItemsForPreDeclaration(ConsignmentType70 responseConsignment, HouseConsignmentType70 responseHouseConsignment, NctsBill bill, NctsHeader nctsHeader)
	{
		var goodsItems = bill.GoodsItems;
		var responseConsignmentItems = responseHouseConsignment.ConsignmentItem;

		if (goodsItems.Any(x => x.BY_DeclarationGoodsItemNumber.IsEmpty))
		{
			bill.GoodsItems.DeleteAll();
		}
		else
		{
			var seqNumsInResponse = responseConsignmentItems.Select(x => ParseNumberToInt(x.DeclarationGoodsItemNumber));
			goodsItems.Where(x => !seqNumsInResponse.Contains(x.BY_DeclarationGoodsItemNumber)).DeleteAll();
		}

		goodsItems = bill.GoodsItems;

		foreach (var responseConsignmentItem in responseConsignmentItems)
		{
			var responseConsignmentItemDeclarationGoodsItemNumber = ParseNumberToInt(responseConsignmentItem.DeclarationGoodsItemNumber);
			var goodsItem = goodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_DeclarationGoodsItemNumber == responseConsignmentItemDeclarationGoodsItemNumber);
			if (goodsItem == null)
			{
				goodsItem = goodsItems.AddNew();
				goodsItem.BY_DeclarationGoodsItemNumber = responseConsignmentItemDeclarationGoodsItemNumber;
			}

			var consignmentItemSeqNum = ParseNumberToShort(responseConsignmentItem.GoodsItemNumber);
			goodsItem.BY_LineNo = consignmentItemSeqNum;

			goodsItem.BY_Type = responseConsignmentItem.DeclarationType;

			var responseCountryOfDispatch = responseConsignmentItem.CountryOfDispatch;
			if (!responseCountryOfDispatch.IsEmpty())
			{
				goodsItem.BY_RN_NKCountryOfDispatch = responseCountryOfDispatch;
			}

			var responseCountryOfDestination = responseConsignmentItem.CountryOfDestination;
			if (!responseCountryOfDestination.IsEmpty())
			{
				goodsItem.BY_RN_NKCountryOfDestination = responseCountryOfDestination;
			}

			var responseReferenceNumberUCR = responseConsignmentItem.ReferenceNumberUcr;
			if (!responseReferenceNumberUCR.IsEmpty())
			{
				goodsItem.BY_CommercialReferenceNumber = responseReferenceNumberUCR;
			}

			var commodity = responseConsignmentItem.Commodity;
			if (commodity != null)
			{
				goodsItem.BY_Description = commodity.DescriptionOfGoods;
				goodsItem.BY_CusC4Number = commodity.CusCode;

				var commodityCode = commodity.CommodityCode;
				var existingTariff = goodsItem.BY_HarmonisedTariff;
				var responseTariff = commodityCode?.HarmonizedSystemSubHeadingCode + commodityCode?.CombinedNomenclatureCode;
				if (existingTariff.SubstringSafe(0, 8) != responseTariff)
				{
					goodsItem.BY_HarmonisedTariff = responseTariff;
				}

				SetDangerousGoodsForPreDeclaration(commodity, goodsItem);

				var goodsMeasure = commodity.GoodsMeasure;
				if (goodsMeasure != null)
				{
					goodsItem.BY_GrossWeight = (ZDecimal)goodsMeasure.GrossMass;
					goodsItem.BY_GrossWeightUnit = NewDataWeightUQ;
					var netmass = goodsMeasure.NetMass;
					goodsItem.BY_NetWeight = netmass != null ? (ZDecimal)netmass : ZDecimal.Zero;
					goodsItem.BY_NetWeightUnit = NewDataWeightUQ;
					var suppUnits = goodsMeasure.SupplementaryUnits;
					if (suppUnits != null)
					{
						goodsItem.BY_CustomsSecondQuantity = (ZDecimal)suppUnits;
					}
				}
			}

			SetGoodsItemAdditionalSuppyChainActorsForPreDeclaration(responseConsignmentItem, goodsItem);
			SetPackagesForPreDeclaration(responseConsignment, responseConsignmentItem, goodsItem, nctsHeader);
			SetPreviousDocumentsForPreDeclaration(responseConsignmentItem, goodsItem);
			SetSupportingDocumentData(responseConsignmentItem, goodsItem, false);
			goodsItem.AdditionalInfos.DeleteAll();
			SetTransportDocumentData(responseConsignmentItem, goodsItem, false, bill);
			SetReferenceDocumentData(responseConsignmentItem, goodsItem, false);
			SetAdditionalInformationDocumentsForPreDeclaration(responseConsignmentItem, goodsItem);
		}
	}

	void SetDangerousGoodsForPreDeclaration(CommodityType70 commodity, NctsDepartureCargoDesc goodsItem)
	{
		goodsItem.UNDGs.DeleteAll();
		foreach (var responseDangerousGood in commodity.DangerousGoods)
		{
			var substance = UNDGSubstanceLoader.LoadSubstances(goodsItem.Factory, responseDangerousGood.UnNumber).FirstOrDefault();
			if (substance != null)
			{
				var dangerousGood = goodsItem.UNDGs.AddNew();
				dangerousGood.DI_DG = substance.PK;
			}
		}
	}

	void SetGoodsItemAdditionalSuppyChainActorsForPreDeclaration(ConsignmentItemType70 responseConsignmentItem, NctsDepartureCargoDesc goodsItem)
	{
		var responseAdditionalSupplyChainActor = responseConsignmentItem.AdditionalSupplyChainActor;

		var actorsInResponse = responseAdditionalSupplyChainActor.Select(x => (x.Role, x.IdentificationNumber));
		goodsItem.CusSupplyChainActorReferences.Where(x => !actorsInResponse.Contains((x.CFR_Code, x.CFR_Reference))).DeleteAll();

		foreach (var responseActor in responseAdditionalSupplyChainActor)
		{
			var actor = goodsItem.CusSupplyChainActorReferences.FirstOrDefault(x => x.CFR_Code == responseActor.Role && x.CFR_Reference == responseActor.IdentificationNumber)
						?? goodsItem.CusSupplyChainActorReferences.AddNew();
			actor.CFR_Code = responseActor.Role;
			actor.CFR_Reference = responseActor.IdentificationNumber;
		}
	}

	void SetPackagesForPreDeclaration(ConsignmentType70 responseConsignment, ConsignmentItemType70 responseConsignmentItem, NctsDepartureCargoDesc goodsItem, NctsHeader nctsHeader)
	{
		goodsItem.Packages.RemoveAndDeleteAll();

		var containersAsscosiated = new List<NctsDepartureHeaderContainer>();
		var declarationGoodsItemString = goodsItem.BY_DeclarationGoodsItemNumber.ToString();

		foreach (var transportEq in responseConsignment.TransportEquipment)
		{
			var goodsReference = transportEq.GoodsReference;
			if (goodsReference.Count == 0 || goodsReference.Select(x => x.DeclarationGoodsItemNumber).Contains(declarationGoodsItemString))
			{
				containersAsscosiated.Add(nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().FirstOrDefault(x => x.BC_SequenceNumber.ToString() == transportEq.SequenceNumber));
			}
		}

		foreach (var responsePackage in responseConsignmentItem.Packaging)
		{
			var type = responsePackage.TypeOfPackages;
			goodsItem.IsVehicles = type == ES.Business.UniversalReferenceConstants.RefCusCodeList.PackageType.Frame;

			var pack = goodsItem.Packages.AddNew();

			var responsePackageSeqNum = ParseNumberToShort(responsePackage.SequenceNumber);
			pack.B5_SequenceNumber = responsePackageSeqNum;
			pack.IsDataLoadFromDeparture = true;

			if (type != ES.Business.UniversalReferenceConstants.RefCusCodeList.PackageType.Frame)
			{
				pack.B5_UnitType = type;
				ZLong.TryParse(responsePackage.NumberOfPackages, out var responsePackageNumberOfPackages);
				pack.B5_UnitCount = responsePackageNumberOfPackages;
				pack.B5_MarksAndNumbers = responsePackage.ShippingMarks;
			}
			else
			{
				pack.B5_UnitCount = ZLong.Zero;
				SetVehicleMarks(responsePackage, pack);
			}

			var pivots = pack.ContainersPivot.Cast<GenPivot>().GroupBy(x => x.XX_Relation2ID).ToDictionary(x => x.Key, y => y.ToList());
			foreach (var cont in containersAsscosiated)
			{
				var containerPK = cont.PK;
				if (pivots.TryGetValue(containerPK, out var list))
				{
					pivots.Remove(containerPK);
				}
				else
				{
					pack.ContainersPivot.AddPivotFor(cont);
				}
			}
			pivots.Values.ForEach(x => x.DeleteAll());
		}
	}

	void SetPreviousDocumentsForPreDeclaration(ConsignmentItemType70 responseConsignmentItem, NctsDepartureCargoDesc goodsItem)
	{
		goodsItem.PreviousDocuments.DeleteAll();
		foreach (var responsePrevDoc in responseConsignmentItem.PreviousDocument)
		{
			var prevDoc = goodsItem.PreviousDocuments.AddNew();

			prevDoc.CSI_Code = responsePrevDoc.Type;
			prevDoc.CSI_ReferenceNumber = responsePrevDoc.ReferenceNumber;
			prevDoc.CSI_UnitOfQuantity = responsePrevDoc.MeasurementUnitAndQualifier;
			prevDoc.CSI_ReferenceNumber2 = responsePrevDoc.ComplementOfInformation;
			prevDoc.CSI_Quantity = responsePrevDoc.Quantity ?? ZDecimal.Zero;
			prevDoc.CSI_PackType = responsePrevDoc.TypeOfPackages;
			var responsePrevDocNumberOfPackages = ParseNumberToInt(responsePrevDoc.NumberOfPackages);
			prevDoc.CSI_PackQty = responsePrevDocNumberOfPackages;

			var responsePrevDocItemNum = ParseNumberToInt(responsePrevDoc.GoodsItemNumber);
			prevDoc.CSI_ItemNumber = responsePrevDocItemNum;

			var responsePrevDocSeqNum = ParseNumberToInt(responsePrevDoc.SequenceNumber);
			prevDoc.CSI_LineNo = responsePrevDocSeqNum;
		}
	}

	void SetAdditionalInformationDocumentsForPreDeclaration(ConsignmentItemType70 responseConsignmentItem, NctsCommonCargoDesc goodsItem)
	{
		foreach (var responseInfDoc in responseConsignmentItem.AdditionalInformation)
		{
			var infDoc = goodsItem.AdditionalInfos.AddNew();
			infDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			infDoc.CSI_Code = responseInfDoc.Code;
			infDoc.CSI_Description = responseInfDoc.Text;
			var responseInfDocSeqNum = ParseNumberToInt(responseInfDoc.SequenceNumber);
			infDoc.CSI_LineNo = responseInfDocSeqNum;
		}
	}

	ZShort ParseNumberToShort(string itemNumber) => ZShort.TryParse(itemNumber, out var returnItemNumber) ? returnItemNumber : ZShort.Zero;
	ZInt ParseNumberToInt(string itemNumber) => ZInt.TryParse(itemNumber, out var returnItemNumber) ? returnItemNumber : ZInt.Zero;

	const string XsdSchemaNameCCTRACV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CCTRACV1Sal.xsd";

	const char VehiclesSeparator = ':';
}
