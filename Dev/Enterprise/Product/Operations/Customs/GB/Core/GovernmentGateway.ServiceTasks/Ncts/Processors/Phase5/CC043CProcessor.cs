using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc043c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.custom_ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC043CProcessor : NctsBaseProcessor<Cc043CType>
	{
		public CC043CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Unloading Permission";

		protected override ZString LRN => null;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override ZString GetNewArrivalStatus(Cc043CType messageObject)
		{
			return NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		}

		protected override ZString GetNewMessageStatus(Cc043CType messageObject)
		{
			return LogicalStatusList.Codes.Accepted;
		}

		protected override string GetMessageId(Cc043CType messageObject) => messageObject.MessageIdentification;

		protected override string GetMessageTypeCode(Cc043CType messageObject) => messageObject.MessageType.ToString();

		protected override void AfterUpdateNCTSHeader(Cc043CType messageObject)
		{
			NctsHeaderItem.MovementReferenceEntryNumber.CE_EntryNum = messageObject.TransitOperation.Mrn;

			NctsHeaderItem.ArrivalMovementHeader.BM_InBondEntryType = messageObject.TransitOperation.DeclarationType;
			NctsHeaderItem.ArrivalMovementHeader.BM_EntryDate = messageObject.TransitOperation.DeclarationAcceptanceDate ?? ZDateTime.Empty;
			NctsHeaderItem.ArrivalMovementHeader.BM_TypeOfSecurity = GetTypeOfSecurity(messageObject);
			NctsHeaderItem.ArrivalMovementHeader.BM_ReducedDatasetIndicator = messageObject.TransitOperation?.ReducedDatasetIndicator == Flag.Item1;

			if (messageObject.Consignment != null)
			{
				NctsHeaderItem.ArrivalMovementHeader.BM_RL_NKDestinationPort = messageObject.Consignment.CountryOfDestination;
				NctsHeaderItem.ArrivalMovementHeader.BM_GrossWeight = messageObject.Consignment.GrossMass ?? 0;
				NctsHeaderItem.ArrivalMovementHeader.BM_GrossWeightUQ = Core.Constants.Weight.Kilograms;

				CreateArrivalContainers(NctsHeaderItem, messageObject.Consignment);
				UpdateArrivalTransportInfos(NctsHeaderItem.ArrivalMovementHeader.ArrivalTransportInfos, messageObject.Consignment.DepartureTransportMeans);
				CreateHouseConsignments(NctsHeaderItem, messageObject.Consignment.HouseConsignment);
			}
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Arrival;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		static ZString GetTypeOfSecurity(Cc043CType messageObject)
		{
			switch (messageObject.TransitOperation?.Security)
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
					throw new InvalidOperationException($"Unexpected {nameof(messageObject.TransitOperation.Security)}");
			}
		}

		static void UpdateJobDocAddress(JobDocAddress jobDocAddress, string identificationNumber, string companyName, AddressType07 address, bool suppressAddressValidationError = false)
		{
			if (!identificationNumber.IsNullOrEmpty())
			{
				jobDocAddress.E2_GovRegNum = identificationNumber;
				jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			}

			jobDocAddress.E2_AddressOverride = true;

			if (!companyName.IsNullOrEmpty())
			{
				jobDocAddress.E2_CompanyName = companyName;
			}

			if (address != null)
			{
				jobDocAddress.E2_Address1 = address.StreetAndNumber;
				jobDocAddress.E2_Postcode = address.Postcode;
				jobDocAddress.E2_City = address.City;
				jobDocAddress.E2_RN_NKCountryCode = address.Country;
			}

			jobDocAddress.E2_SuppressAddressValidationError = suppressAddressValidationError;
		}

		static void CreateArrivalContainers(NctsHeader header, CustomConsignmentType05 consignment)
		{
			header.ArrivalHeaderContainers.RemoveAndDeleteAll();

			var containerIndicator = consignment.ContainerIndicator == Flag.Item1;
			ZShort totalSeals = 0;
			foreach (var transportEquipment in consignment.TransportEquipment)
			{
				var container = header.ArrivalHeaderContainers.AddNew();
				container.BC_SequenceNumber = ZShort.Parse(transportEquipment.SequenceNumber);
				container.BC_ContainerNum = transportEquipment.ContainerIdentificationNumber;
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				container.BC_Mode = containerIndicator && !container.BC_ContainerNum.IsEmpty ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;

				foreach (var seal in transportEquipment.Seal)
				{
					CreateSeal(container, seal);
					totalSeals++;
				}
			}
			header.ArrivalMovementHeader.BM_SealQty = totalSeals;
		}

		static void CreateSeal(NctsArrivalHeaderContainer container, SealType04 xmlSeal)
		{
			if (container.BC_Seal1.IsEmpty)
			{
				container.BC_Seal1 = xmlSeal.Identifier;
			}
			if (container.BC_Seal2.IsEmpty)
			{
				container.BC_Seal2 = xmlSeal.Identifier;
			}

			var seal = container.Seals.AddNew();
			seal.BK_SequenceNumber = ZShort.Parse(xmlSeal.SequenceNumber);
			seal.BK_SealNumber = xmlSeal.Identifier;
			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		}

		static void UpdateArrivalTransportInfos(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> arrivalTransportInfos, IEnumerable<DepartureTransportMeansType02> departureTransportMeans)
		{
			arrivalTransportInfos?.RemoveAndDeleteAll();
			foreach (var departureTransportMean in departureTransportMeans)
			{
				var transportInfo = arrivalTransportInfos.AddNew();
				transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				transportInfo.TPM_SequenceNumber = ZShort.ParseSafe(departureTransportMean.SequenceNumber, 0);
				transportInfo.TPM_TypeOfIdentification = departureTransportMean.TypeOfIdentification;
				transportInfo.TPM_IdentificationNumber = departureTransportMean.IdentificationNumber;
				transportInfo.TPM_RN_NKTransportNationality = departureTransportMean.Nationality;
			}
		}

		static void CreateHouseConsignments(NctsHeader nctsHeader, IEnumerable<CustomHouseConsignmentType04> houseConsignments)
		{
			foreach (var houseConsignment in houseConsignments)
			{
				var bill = nctsHeader.Bills.AddNew();

				var sourceConsignee = houseConsignment.Consignee;
				if (sourceConsignee != null)
				{
					UpdateJobDocAddress(bill.Consignee, sourceConsignee.IdentificationNumber, sourceConsignee.Name, sourceConsignee.Address, suppressAddressValidationError: true);
				}

				var sourceConsignor = houseConsignment.Consignor;
				if (sourceConsignor != null)
				{
					UpdateJobDocAddress(bill.Consignor, sourceConsignor.IdentificationNumber, sourceConsignor.Name, sourceConsignor.Address, suppressAddressValidationError: true);
				}

				var moveDetail = bill.MovementDetail;

				moveDetail.B9_BM = nctsHeader.ArrivalMovementHeader.PK;
				moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				moveDetail.B9_B9_InBondMoveDetail = ZGuid.Empty;
				moveDetail.B9_SeqNo = houseConsignment.SequenceNumber;

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

				foreach (var consignmentItem in houseConsignment.ConsignmentItem)
				{
					CreateArrivalGoodsItem(bill, consignmentItem);
				}
			}
		}

		static void CreateArrivalGoodsItem(NctsBill bill, CustomConsignmentItemType04 consignmentItem)
		{
			var goodItem = bill.ArrivalGoodsItems.AddNew();

			goodItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodItem.BY_LineNo = ZShort.Parse(consignmentItem.GoodsItemNumber);
			goodItem.BY_DeclarationGoodsItemNumber = ZInt.ParseSafe(consignmentItem.DeclarationGoodsItemNumber, 0);
			goodItem.BY_RN_NKCountryOfDestination = consignmentItem.CountryOfDestination;
			goodItem.BY_Description = consignmentItem.Commodity?.DescriptionOfGoods ?? string.Empty;
			goodItem.BY_HarmonisedTariff = (consignmentItem.Commodity?.CommodityCode?.HarmonizedSystemSubHeadingCode ?? ZString.Empty) + (consignmentItem.Commodity?.CommodityCode?.CombinedNomenclatureCode ?? ZString.Empty);

			if (consignmentItem.Commodity?.GoodsMeasure?.GrossMass != null)
			{
				ZDecimal grossMass = consignmentItem.Commodity.GoodsMeasure.GrossMass;
				ZString grossMassUnit = Core.Constants.Weight.Kilograms;
				if (grossMass.DecimalPlaces > 3)
				{
					grossMass *= 1000;
					grossMassUnit = Core.Constants.Weight.Grams;
				}
				goodItem.BY_GrossWeight = grossMass;
				goodItem.BY_GrossWeightUnit = grossMassUnit;
			}

			if (consignmentItem.Commodity?.GoodsMeasure?.NetMass != null)
			{
				ZDecimal netMass = consignmentItem.Commodity.GoodsMeasure.NetMass ?? 0;
				ZString netMassUnit = Core.Constants.Weight.Kilograms;
				if (netMass.DecimalPlaces > 3)
				{
					netMass *= 1000;
					netMassUnit = Core.Constants.Weight.Grams;
				}
				goodItem.BY_NetWeight = netMass;
				goodItem.BY_NetWeightUnit = netMassUnit;
			}

			foreach (var xmlPackage in consignmentItem.Packaging)
			{
				CreatePackage(goodItem, xmlPackage);
			}
		}

		static void CreatePackage(NctsArrivalCargoDesc goodItem, PackagingType02 xmlPackage)
		{
			var package = goodItem.Packages.AddNew();
			package.B5_SequenceNumber = ZShort.Parse(xmlPackage.SequenceNumber);
			package.B5_UnitType = xmlPackage.TypeOfPackages;
			package.B5_UnitCount = ZLong.Parse(xmlPackage.NumberOfPackages);
			package.B5_MarksAndNumbers = xmlPackage.ShippingMarks;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		}
	}
}
