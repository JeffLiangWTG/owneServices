using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC043CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC043CProvider>
	{
		public CC043CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC043CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC043CProvider provider) => NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

		protected override string MessageFriendlyNameCore => Res.GetString("02811A18-348F-4574-ABE0-062B9047AA80", "CC043C: UNLOADING PERMISSION");

		protected override Type MessageInterpreterType => typeof(CC043CMessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC043CProvider provider)
		{
			if (messageAttachee is NctsHeader header && header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				OverwriteConsignment(header, arrivalMovementHeader, provider);
				CreateCustomsRegistryNumber(header);
			}
		}

		void CreateCustomsRegistryNumber(NctsHeader nctsHeader)
		{
			if (nctsHeader.DestinationTrader?.Organisation is OrgHeader destinationTrader)
			{
				var regNumber = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Ireland);
				regNumber.CE_EntryLineReference = TransitArrival + "-" + destinationTrader.OH_Code;
				regNumber.CE_EntryNum = nctsHeader.LocalReferenceNumber;
				regNumber.CE_IssueDate = ZDateTime.Now;
			}
		}

		const string TransitArrival = "TA";

		void OverwriteConsignment(NctsHeader header, NctsArrivalMovementHeader arrivalMovementHeader, CC043CProvider provider)
		{
			var consignmentProvider = provider.Consignment;
			arrivalMovementHeader.BM_GrossWeight = consignmentProvider.GrossMass;
			arrivalMovementHeader.BM_InlandTransportMode = consignmentProvider.InlandModeOfTransport;
			OverwriteArrivalHeaderContainers(header.ArrivalHeaderContainers, arrivalMovementHeader, consignmentProvider);
			OverwriteArrivalTransportInfos(arrivalMovementHeader.ArrivalTransportInfos, consignmentProvider);
			OverwriteArrivalMovementHeaderSupportingDocuments(arrivalMovementHeader.SupportingDocuments, consignmentProvider);
			OverwriteArrivalMovementHeaderAdditionalDocuments(arrivalMovementHeader.AdditionalDocuments, consignmentProvider);
			OverwriteConsignment_PreviousDocuments(header.PreviousDocuments, consignmentProvider);
			header.BH_ExportFlag = consignmentProvider.Incidents.Count > 0 ? EventFlagList.Codes.Yes : EventFlagList.Codes.No;
			OverwriteConsignment_Incidents(header.EnRouteIncidents, consignmentProvider);
			OverwriteConsignment_HouseConsignment(header.Bills, provider);
		}

		void OverwriteArrivalHeaderContainers(NctsArrivalHeaderContainerCollection arrivalHeaderContainers, NctsArrivalMovementHeader arrivalMovementHeader, CC043CConsignmentProvider provider)
		{
			arrivalHeaderContainers.RemoveAndDeleteAll();
			ZShort totalSeals = 0;
			foreach (var transportEquipment in provider.TransportEquipment)
			{
				var container = arrivalHeaderContainers.AddNew();
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				container.BC_Mode = provider.ContainerIndicator ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
				container.BC_SequenceNumber = transportEquipment.SequenceNumber;
				container.BC_ContainerNum = transportEquipment.ContainerIdentificationNumber;
				totalSeals += transportEquipment.NumberOfSeals;

				foreach (var seal in transportEquipment.Seals)
				{
					var mySeal = container.Seals.AddNew();
					mySeal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
					mySeal.BK_SequenceNumber = seal.SequenceNumber;
					mySeal.BK_SealNumber = seal.Identifier;
				}
			}
			arrivalMovementHeader.BM_SealQty = totalSeals;
		}

		void OverwriteArrivalTransportInfos(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> arrivalTransportInfos, CC043CConsignmentProvider provider)
		{
			arrivalTransportInfos.RemoveAndDeleteAll();
			foreach (var departureTransportMeans in provider.DepartureTransportMeans)
			{
				var transportInfo = arrivalTransportInfos.AddNew();
				transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				transportInfo.TPM_SequenceNumber = departureTransportMeans.SequenceNumber;
				transportInfo.TPM_TypeOfIdentification = departureTransportMeans.TypeOfIdentification;
				transportInfo.TPM_IdentificationNumber = departureTransportMeans.IdentificationNumber;
				transportInfo.TPM_RN_NKTransportNationality = departureTransportMeans.Nationality;
			}
		}

		void OverwriteArrivalMovementHeaderSupportingDocuments(INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments, CC043CConsignmentProvider provider)
		{
			supportingDocuments.RemoveAndDeleteAll();
			foreach (var supportingDoc in provider.SupportingDocuments)
			{
				var mySupportingDoc = supportingDocuments.AddNew();
				mySupportingDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				mySupportingDoc.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
				mySupportingDoc.CSI_LineNo = supportingDoc.SequenceNumber;
				mySupportingDoc.CSI_Code = supportingDoc.Type;
				mySupportingDoc.CSI_ReferenceNumber = supportingDoc.ReferenceNumber;
				mySupportingDoc.CSI_ReferenceNumber2 = supportingDoc.ComplementOfInformation;
			}
		}

		void OverwriteArrivalMovementHeaderAdditionalDocuments(INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalDocuments, CC043CConsignmentProvider provider)
		{
			additionalDocuments.RemoveAndDeleteAll();
			foreach (var transportDoc in provider.TransportDocuments)
			{
				var mySupportingDoc = additionalDocuments.AddNew();
				mySupportingDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				mySupportingDoc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				mySupportingDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				mySupportingDoc.CSI_LineNo = transportDoc.SequenceNumber;
				mySupportingDoc.CSI_Code = transportDoc.Type;
				mySupportingDoc.CSI_ReferenceNumber = transportDoc.ReferenceNumber;
			}
			foreach (var additionalReference in provider.AdditionalReferences)
			{
				var mySupportingDoc = additionalDocuments.AddNew();
				mySupportingDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				mySupportingDoc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				mySupportingDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				mySupportingDoc.CSI_LineNo = additionalReference.SequenceNumber;
				mySupportingDoc.CSI_Code = additionalReference.Type;
				mySupportingDoc.CSI_ReferenceNumber = additionalReference.ReferenceNumber;
			}
			foreach (var additionalInfo in provider.AdditionalInformation)
			{
				var newDoc = additionalDocuments.AddNew();
				newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				newDoc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				newDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				newDoc.CSI_LineNo = ZInt.ParseSafe(additionalInfo.SequenceNumber, 0);
				newDoc.CSI_Code = additionalInfo.Code;
				newDoc.CSI_ReferenceNumber = additionalInfo.Text;
			}
		}

		void OverwriteConsignment_PreviousDocuments(ICommonPreviousDocumentCollection<CommonPreviousDocument> previousDocuments, CC043CConsignmentProvider provider)
		{
			previousDocuments.RemoveAndDeleteAll();
			foreach (var document in provider.PreviousDocument)
			{
				var newDoc = previousDocuments.AddNew();
				newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				newDoc.CSI_Type = CusSupportingInfoTypeList.Codes.PreviousDocument;
				newDoc.CSI_Code = document.Type;
				newDoc.CSI_ReferenceNumber = document.ReferenceNumber;
				newDoc.CSI_ReferenceNumber2 = document.ComplementOfInformation;
			}
		}

		void OverwriteConsignment_Incidents(EnRouteIncidentCollection enRouteIncidents, CC043CConsignmentProvider provider)
		{
			foreach (var incident in provider.Incidents)
			{
				var myIncident = enRouteIncidents.AddNew();
				myIncident.BN_IncidentCode = incident.Code;
				myIncident.BN_Information = incident.Text;
				myIncident.BN_LocationQualifier = incident.Location?.QualifierOfIdentification ?? ZString.Empty;
				myIncident.BN_EventPlace = incident.Location?.UNLocode ?? ZString.Empty;
				myIncident.BN_EventCountryCode = incident.Location?.Country ?? ZString.Empty;
				myIncident.BN_CustomsStatus = IncidentCustomsStatusList.Codes.CUS;
				myIncident.BN_EndorsementDate = incident.Endorsement?.Date ?? ZDate.Empty;
				myIncident.BN_EndorsementAuthority = incident.Endorsement?.Authority ?? ZString.Empty;
				myIncident.BN_EndorsementPlace = incident.Endorsement?.Place ?? ZString.Empty;
				myIncident.BN_EndorsementCountryCode = incident.Endorsement?.Country ?? ZString.Empty;
			}
		}

		void OverwriteConsignment_HouseConsignment(INctsBillCollection<NctsBill> bills, CC043CProvider provider)
		{
			bills.DeleteAll();
			var consignmentProvider = provider.Consignment;
			foreach (var houseConsignment in consignmentProvider.HouseConsignment)
			{
				var bill = bills.AddNew();
				bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				bill.MovementDetail.B9_SeqNo = houseConsignment.SequenceNumber;
				bill.B0_Weight = houseConsignment.GrossMass;
				bill.B0_SecurityIndicatorFromExport = !provider.Security.Equals(Security_0);
				SetHouseConsignmentConsignor(bill, consignmentProvider.Consignor);
				SetHouseConsignmentConsignee(bill, consignmentProvider.Consignee);
				AddHouseConsignmentAdditionalDocuments(bill.AdditionalDocuments, houseConsignment);
				AddHouseConsignmentSupportingDocs(bill, houseConsignment.SupportingDocument);
				AddHouseConsignmentDepartureTransportMeans(bill, houseConsignment.DepartureTransportMeans);
				CreateHouseConsignmentItems(houseConsignment, bill.ArrivalGoodsItems);
			}
		}

		const string Security_0 = "0";

		void SetHouseConsignmentConsignor(NctsBill bill, CC043CConsignorProvider provider)
		{
			if (provider != null)
			{
				bill.MovementDetail.ConsignorDocAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
				bill.MovementDetail.ConsignorDocAddress.E2_AddressOverride = true;
				bill.MovementDetail.ConsignorDocAddress.E2_CompanyName = provider.Name;
				bill.MovementDetail.ConsignorDocAddress.E2_GovRegNum = provider.IdentificationNumber;
				bill.MovementDetail.ConsignorDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				bill.MovementDetail.ConsignorDocAddress.E2_Address1 = provider.Address?.StreetAndNumber ?? ZString.Empty;
				bill.MovementDetail.ConsignorDocAddress.E2_Postcode = provider.Address?.Postcode ?? ZString.Empty;
				bill.MovementDetail.ConsignorDocAddress.E2_City = provider.Address?.City ?? ZString.Empty;
				bill.MovementDetail.ConsignorDocAddress.E2_RN_NKCountryCode = provider.Address?.Country ?? ZString.Empty;
				bill.MovementDetail.ConsignorDocAddress.E2_ValidationStatus = AddressValidationStatus.Verified;
			}
		}

		void SetHouseConsignmentConsignee(NctsBill bill, CC043CConsigneeProvider provider)
		{
			if (provider != null)
			{
				bill.MovementDetail.ConsigneeDocAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneeAddress;
				bill.MovementDetail.ConsigneeDocAddress.E2_AddressOverride = true;
				bill.MovementDetail.ConsigneeDocAddress.E2_CompanyName = provider.Name;
				bill.MovementDetail.ConsigneeDocAddress.E2_GovRegNum = provider.IdentificationNumber;
				bill.MovementDetail.ConsigneeDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				bill.MovementDetail.ConsigneeDocAddress.E2_Address1 = provider.Address?.StreetAndNumber ?? ZString.Empty;
				bill.MovementDetail.ConsigneeDocAddress.E2_Postcode = provider.Address?.Postcode ?? ZString.Empty;
				bill.MovementDetail.ConsigneeDocAddress.E2_City = provider.Address?.City ?? ZString.Empty;
				bill.MovementDetail.ConsigneeDocAddress.E2_RN_NKCountryCode = provider.Address?.Country ?? ZString.Empty;
				bill.MovementDetail.ConsigneeDocAddress.E2_ValidationStatus = AddressValidationStatus.Verified;
			}
		}

		void AddHouseConsignmentAdditionalDocuments(INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> docCollection, CC043CHouseConsignmentProvider provider)
		{
			foreach (var transport in provider.TransportDocument)
			{
				var doc = docCollection.AddNew();
				doc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				doc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				doc.CSI_LineNo = transport.SequenceNumber;
				doc.CSI_Code = transport.Type;
				doc.CSI_ReferenceNumber = transport.ReferenceNumber;
			}
			foreach (var addRef in provider.AdditionalReference)
			{
				var doc = docCollection.AddNew();
				doc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				doc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
				doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				doc.CSI_LineNo = addRef.SequenceNumber;
				doc.CSI_Code = addRef.Type;
				doc.CSI_ReferenceNumber = addRef.ReferenceNumber;
			}
		}

		void AddHouseConsignmentSupportingDocs(NctsBill bill, IReadOnlyCollection<CC043CSupportingDocumentProvider> provider)
		{
			foreach (var doc in provider)
			{
				var newDoc = bill.SupportingDocuments.AddNew();
				newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				newDoc.CSI_LineNo = doc.SequenceNumber;
				newDoc.CSI_Code = doc.Type;
				newDoc.CSI_ReferenceNumber = doc.ReferenceNumber;
				newDoc.CSI_ReferenceNumber2 = doc.ComplementOfInformation;
			}
		}

		void AddHouseConsignmentDepartureTransportMeans(NctsBill bill, IReadOnlyCollection<CC043CDepartureTransportMeansProvider> provider)
		{
			foreach (var transportMeans in provider)
			{
				var newTransportInfo = bill.DepartureTransportInfos.AddNew();
				newTransportInfo.TPM_SequenceNumber = transportMeans.SequenceNumber;
				newTransportInfo.TPM_TypeOfIdentification = transportMeans.TypeOfIdentification;
				newTransportInfo.TPM_IdentificationNumber = transportMeans.IdentificationNumber;
				newTransportInfo.TPM_RN_NKTransportNationality = transportMeans.Nationality;
			}
		}

		void CreateHouseConsignmentItems(CC043CHouseConsignmentProvider houseConsignment, INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> arrivalGoodsItems)
		{
			foreach (var item in houseConsignment.ConsignmentItem)
			{
				var newItem = arrivalGoodsItems.AddNew();
				newItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				newItem.BY_LineNo = ZShort.Parse(item.GoodsItemNumber);
				newItem.BY_DeclarationGoodsItemNumber = ZInt.Parse(item.DeclarationGoodsItemNumber);
				newItem.BY_Description = item.Commodity?.DescriptionOfGoods ?? ZString.Empty;
				newItem.BY_CusC4Number = item.Commodity?.CusCode ?? ZString.Empty;
				newItem.BY_Type = item.DeclarationType;
				newItem.BY_RN_NKCountryOfDestination = item.CountryOfDestination;
				newItem.BY_HarmonisedTariff = (item.Commodity?.CommodityCode?.HarmonizedSystemSubHeadingCode ?? ZString.Empty) + (item.Commodity?.CommodityCode?.CombinedNomenclatureCode ?? ZString.Empty);
				newItem.BY_GrossWeight = item.Commodity?.GoodsMeasure?.GrossMass ?? ZDecimal.Zero;
				newItem.BY_NetWeight = item.Commodity?.GoodsMeasure?.NetMassValue ?? ZDecimal.Zero;

				var packages = newItem.Packages;
				foreach (var package in item.Packaging)
				{
					var newPack = packages.AddNew();
					newPack.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
					newPack.B5_SequenceNumber = ZShort.ParseSafe(package.SequenceNumber, 0);
					newPack.B5_UnitType = package.TypeOfPackages;
					newPack.B5_UnitCount = ZLong.ParseSafe(package.NumberOfPackages, 0);
					newPack.B5_MarksAndNumbers = package.ShippingMarks;
				}

				var supportingDocuments = newItem.SupportingDocuments;
				foreach (var doc in item.SupportingDocument)
				{
					var newDoc = supportingDocuments.AddNew();
					newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
					newDoc.CSI_LineNo = doc.SequenceNumber;
					newDoc.CSI_Code = doc.Type;
					newDoc.CSI_ReferenceNumber = doc.ReferenceNumber;
					newDoc.CSI_ReferenceNumber2 = doc.ComplementOfInformation;
				}

				var additionalInfos = newItem.AdditionalInfos;
				foreach (var doc in item.TransportDocument)
				{
					var newDoc = additionalInfos.AddNew();
					newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
					newDoc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
					newDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
					newDoc.CSI_LineNo = doc.SequenceNumber;
					newDoc.CSI_Code = doc.Type;
					newDoc.CSI_ReferenceNumber = doc.ReferenceNumber;
				}

				foreach (var doc in item.AdditionalReference)
				{
					var newDoc = additionalInfos.AddNew();
					newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
					newDoc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
					newDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					newDoc.CSI_LineNo = doc.SequenceNumber;
					newDoc.CSI_Code = doc.Type;
					newDoc.CSI_ReferenceNumber = doc.ReferenceNumber;
				}

				foreach (var doc in item.AdditionalInformation)
				{
					var newDoc = additionalInfos.AddNew();
					newDoc.CSI_Status = NctsUnloadedStateList.Codes.DEC;
					newDoc.CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
					newDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					newDoc.CSI_Code = doc.Code;
					newDoc.CSI_Description = doc.Text;
				}
			}
		}
	}
}
