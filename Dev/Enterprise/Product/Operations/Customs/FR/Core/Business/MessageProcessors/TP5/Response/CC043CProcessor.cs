using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC043C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	internal class CC043CProcessor : TP5BaseProcessor<Cc043CType>
	{
		public CC043CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.UnloadingPermission;

		protected override ZString GetMRNFromResponseMessage(Cc043CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewArrivalStatus(Cc043CType messageObject) => NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

		protected override ZString GetNewMessageStatus(Cc043CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc043CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override void DoExtraProcessing(NctsHeader nctsHeader, Cc043CType messageObject, EDIMessage inboundMessage)
		{
			base.DoExtraProcessing(nctsHeader, messageObject, inboundMessage);

			var moveHeader = nctsHeader.ArrivalMovementHeader;

			if (moveHeader != null)
			{
				var consignment = messageObject.Consignment;
				if (consignment != null)
				{
					moveHeader.BM_InlandTransportMode = consignment.InlandModeOfTransport;
					moveHeader.BM_GrossWeight = consignment.GrossMass != null ? consignment.GrossMass.Value : ZDecimal.Zero;

					nctsHeader.ArrivalMovementHeader.BM_SealQty = 0;
					foreach (var equipment in consignment.TransportEquipment)
					{
						CreateContainer(equipment, nctsHeader);
					}

					CreateDocumentsOnMovementHeader(moveHeader, consignment);

					foreach (var houseConsignment in consignment.HouseConsignment)
					{
						ProcessHouseConsignment(houseConsignment, nctsHeader, moveHeader);
					}

					LinkEquipmentAndGoodItems(consignment.TransportEquipment, nctsHeader);
				}
			}
		}

		static void ProcessHouseConsignment(HouseConsignmentType04 houseConsignment, NctsHeader nctsHeader, NctsArrivalMovementHeader arrivalMovementHeader)
		{
			var bill = nctsHeader.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			moveDetail.B9_BM = arrivalMovementHeader.PK;
			moveDetail.B9_B9_InBondMoveDetail = ZGuid.Empty;
			moveDetail.B9_SeqNo = houseConsignment.SequenceNumber;
			moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			ZDecimal grossMass = houseConsignment.GrossMass;
			ZString grossMassUnit = Core.Constants.Weight.Kilograms;

			if (grossMass.DecimalPlaces > 3)
			{
				grossMass *= 1000;
				grossMassUnit = Core.Constants.Weight.Grams;
			}

			bill.B0_Weight = grossMass;
			bill.B0_WeightUQ = grossMassUnit;
			bill.B0_SecurityIndicatorFromExport = houseConsignment.SecurityIndicatorFromExportDeclaration == "1";

			foreach (var departureTransportMean in houseConsignment.DepartureTransportMeans)
			{
				CreateTransportMean(departureTransportMean, bill);
			}

			foreach (var document in houseConsignment.PreviousDocument)
			{
				CreatePreviousDocument(bill.PreviousDocuments, document);
			}

			foreach (var document in houseConsignment.SupportingDocument)
			{
				CreateSupportingDocument(bill.SupportingDocuments, document);
			}

			foreach (var document in houseConsignment.TransportDocument)
			{
				CreateTransportDocument(bill.AdditionalDocuments, document, EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument);
			}

			foreach (var document in houseConsignment.AdditionalInformation)
			{
				CreateAdditionalInformation(bill.AdditionalDocuments, document, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			}

			foreach (var document in houseConsignment.AdditionalReference)
			{
				CreateAdditionalReference(bill.AdditionalDocuments, document, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference);
			}

			foreach (var item in houseConsignment.ConsignmentItem)
			{
				CreateGoodsItem(bill, item);
			}
		}

		static void CreateGoodsItem(NCTS.NctsBill bill, ConsignmentItemType04 item)
		{
			var goodItem = bill.ArrivalGoodsItems.AddNew();

			goodItem.BY_LineNo = ZShort.Parse(item.GoodsItemNumber);
			goodItem.BY_DeclarationGoodsItemNumber = ZInt.Parse(item.DeclarationGoodsItemNumber);
			goodItem.BY_Description = item.Commodity?.DescriptionOfGoods ?? ZString.Empty;
			goodItem.BY_CusC4Number = item.Commodity?.CusCode ?? ZString.Empty;
			goodItem.BY_HarmonisedTariff = item.Commodity?.CommodityCode?.HarmonizedSystemSubHeadingCode + item.Commodity?.CommodityCode?.CombinedNomenclatureCode ?? ZString.Empty;
			goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			ZDecimal grossMass = item.Commodity?.GoodsMeasure?.GrossMass ?? ZDecimal.Zero;
			ZString grossMassUnit = Core.Constants.Weight.Kilograms;

			if (grossMass.DecimalPlaces > 3)
			{
				grossMass *= 1000;
				grossMassUnit = Core.Constants.Weight.Grams;
			}
			goodItem.BY_GrossWeight = grossMass;
			goodItem.BY_GrossWeightUnit = grossMassUnit;

			ZDecimal netMass = item.Commodity?.GoodsMeasure?.NetMass != null ? item.Commodity.GoodsMeasure.NetMass.Value : ZDecimal.Zero;
			ZString netMassUnit = Core.Constants.Weight.Kilograms;

			if (netMass.DecimalPlaces > 3)
			{
				netMass *= 1000;
				netMassUnit = Core.Constants.Weight.Grams;
			}
			goodItem.BY_NetWeight = netMass;
			goodItem.BY_NetWeightUnit = netMassUnit;

			foreach (var xmlPackage in item.Packaging)
			{
				CreatePackage(goodItem, xmlPackage);
			}

			foreach (var document in item.PreviousDocument)
			{
				CreatePreviousDocument(goodItem.PreviousDocuments, document, ZInt.ParseEmptyAsZero(item.GoodsItemNumber));
			}

			foreach (var document in item.SupportingDocument)
			{
				CreateSupportingDocument(goodItem.SupportingDocuments, document);
			}

			foreach (var document in item.TransportDocument)
			{
				CreateTransportDocument(goodItem.AdditionalInfos, document, EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument);
			}

			foreach (var document in item.AdditionalInformation)
			{
				CreateAdditionalInformation(goodItem.AdditionalInfos, document, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			}

			foreach (var document in item.AdditionalReference)
			{
				CreateAdditionalReference(goodItem.AdditionalInfos, document, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference);
			}
		}

		static void CreateContainer(TransportEquipmentType05 equipment, NctsHeader header)
		{
			var container = header.ArrivalHeaderContainers.AddNew();
			container.BC_SequenceNumber = ZShort.Parse(equipment.SequenceNumber);
			container.BC_ContainerNum = equipment.ContainerIdentificationNumber;
			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			foreach (var seal in equipment.Seal)
			{
				CreateSeal(seal, container, header);
			}
		}

		static void CreateSeal(SealType04 pSeal, NctsArrivalHeaderContainer container, NctsHeader header)
		{
			var seal = container.Seals.AddNew();
			seal.BK_SequenceNumber = ZShort.Parse(pSeal.SequenceNumber);
			seal.BK_SealNumber = pSeal.Identifier;
			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
			header.ArrivalMovementHeader.BM_SealQty += 1;
		}

		static void CreateDocumentsOnMovementHeader(NctsArrivalMovementHeader moveHeader, ConsignmentType05 consignment)
		{
			foreach (var means in consignment.DepartureTransportMeans)
			{
				CreateTransportMean(means, moveHeader);
			}

			foreach (var document in consignment.PreviousDocument)
			{
				CreatePreviousDocument(moveHeader.Header.PreviousDocuments, document);
			}

			foreach (var document in consignment.SupportingDocument)
			{
				CreateSupportingDocument(moveHeader.SupportingDocuments, document);
			}

			foreach (var document in consignment.TransportDocument)
			{
				CreateTransportDocument(moveHeader.AdditionalDocuments, document, EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument);
			}

			foreach (var document in consignment.AdditionalInformation)
			{
				CreateAdditionalInformation(moveHeader.AdditionalDocuments, document, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			}

			foreach (var document in consignment.AdditionalReference)
			{
				CreateAdditionalReference(moveHeader.AdditionalDocuments, document, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference);
			}
		}

		static void CreateTransportMean(DepartureTransportMeansType02 pTransportMean, NctsArrivalMovementHeader moveheader)
		{
			var transportMean = moveheader.ArrivalTransportInfos.AddNew();
			CreateTransportMeanDetail(pTransportMean, transportMean);
		}

		static void CreateTransportMean(DepartureTransportMeansType02 pTransportMean, NctsBill bill)
		{
			var transportMean = bill.ArrivalTransportInfos.AddNew();
			CreateTransportMeanDetail(pTransportMean, transportMean);
		}

		static void CreateTransportMeanDetail(DepartureTransportMeansType02 pTransportMean, ArrivalCusTransportMeans transportMean)
		{
			transportMean.TPM_SequenceNumber = ZShort.ParseSafe(pTransportMean.SequenceNumber, ZShort.Zero);
			transportMean.TPM_TypeOfIdentification = pTransportMean.TypeOfIdentification;
			transportMean.TPM_IdentificationNumber = pTransportMean.IdentificationNumber;
			transportMean.TPM_RN_NKTransportNationality = pTransportMean.Nationality;
			transportMean.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreatePreviousDocument<T>(ICusSupportingInfoCollection<T> documentCollection, PreviousDocumentType06 pDocument) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber.LeftEmptyIfNull(document.CSI_ReferenceNumberInfo.MaxLength);
			document.CSI_ReferenceNumber2 = pDocument.ComplementOfInformation.LeftEmptyIfNull(document.CSI_ReferenceNumber2Info.MaxLength);
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreatePreviousDocument<T>(ICusSupportingInfoCollection<T> documentCollection, PreviousDocumentType07 pDocument) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber.LeftEmptyIfNull(document.CSI_ReferenceNumberInfo.MaxLength);
			document.CSI_ReferenceNumber2 = pDocument.ComplementOfInformation.LeftEmptyIfNull(document.CSI_ReferenceNumber2Info.MaxLength);
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreatePreviousDocument<T>(ICusSupportingInfoCollection<T> documentCollection, PreviousDocumentType04 pDocument, ZInt itemNumber) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ItemNumber = itemNumber;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber;
			document.CSI_ReferenceNumber2 = pDocument.ComplementOfInformation;
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreateSupportingDocument<T>(ICusSupportingInfoCollection<T> documentCollection, SupportingDocumentType02 pDocument) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber.LeftEmptyIfNull(document.CSI_ReferenceNumberInfo.MaxLength);
			document.CSI_ReferenceNumber2 = pDocument.ComplementOfInformation.LeftEmptyIfNull(document.CSI_ReferenceNumber2Info.MaxLength);
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreateTransportDocument<T>(ICusSupportingInfoCollection<T> documentCollection, TransportDocumentType02 pDocument, ZString subType) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_SubType = subType;
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber.LeftEmptyIfNull(document.CSI_ReferenceNumberInfo.MaxLength);
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreateAdditionalInformation<T>(ICusSupportingInfoCollection<T> documentCollection, AdditionalInformationType02 pDocument, ZString subType) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_SubType = subType;
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Code;
			document.CSI_ReferenceNumber = pDocument.Text.LeftEmptyIfNull(document.CSI_ReferenceNumberInfo.MaxLength);
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreateAdditionalReference<T>(ICusSupportingInfoCollection<T> documentCollection, AdditionalReferenceType04 pDocument, ZString subType) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_SubType = subType;
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber.LeftEmptyIfNull(document.CSI_ReferenceNumberInfo.MaxLength);
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreateAdditionalReference<T>(ICusSupportingInfoCollection<T> documentCollection, AdditionalReferenceType03 pDocument, ZString subType) where T : CusSupportingInfo
		{
			var document = documentCollection.AddNew();
			document.CSI_LineNo = ZInt.ParseEmptyAsZero(pDocument.SequenceNumber);
			document.CSI_Code = pDocument.Type;
			document.CSI_ReferenceNumber = pDocument.ReferenceNumber;
			document.CSI_SubType = subType;
			document.CSI_Status = NctsUnloadedStateList.Codes.DEC;
		}

		static void CreatePackage(NctsArrivalCargoDesc goodItem, PackagingType02 pPackage)
		{
			var package = goodItem.Packages.AddNew();

			package.B5_SequenceNumber = ZShort.Parse(pPackage.SequenceNumber);
			package.B5_UnitType = pPackage.TypeOfPackages;
			package.B5_UnitCount = ZLong.Parse(pPackage.NumberOfPackages);
			package.B5_MarksAndNumbers = pPackage.ShippingMarks;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		}

		static void LinkEquipmentAndGoodItems(IReadOnlyCollection<TransportEquipmentType05> transportEquipments, NctsHeader nctsHeader)
		{
			foreach (var equipment in transportEquipments)
			{
				var containerNumber = equipment.ContainerIdentificationNumber;
				var container = nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().FirstOrDefault(x => x.BC_ContainerNum == containerNumber);
				if (container != null)
				{
					var goodsReferences = equipment.GoodsReference.Select(x => int.Parse(x.DeclarationGoodsItemNumber)).ToList();
					var packages = nctsHeader.Bills
						.SelectMany(x => x.ArrivalGoodsItems)
						.Where(x => goodsReferences
						.Contains(x.BY_DeclarationGoodsItemNumber))
						.SelectMany(x => x.Packages);

					foreach (var package in packages)
					{
						package.ContainersPivot.AddPivotFor(container);
					}
				}
			}
		}
	}
}
