using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using CusSeal = Enterprise.Customs.EU.NCTS.Business.CusSeal;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
	, Integration.Customs.ES.IArrivalMovementHeader, ISupportMultipleResourceStringData
{
	public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos
		=> (EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)base.ArrivalTransportInfos;

	public new EU.NCTS.Business.INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc> GoodsItems
		=> (EU.NCTS.Business.INctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>)base.GoodsItems;

	public new NctsArrivalMovementHeaderLookups Lookups => (NctsArrivalMovementHeaderLookups)base.Lookups;

	[ChildEditable(true)]
	public G4PreviousDocumentCollection G4PreviousDocuments
	{
		get
		{
			if (g4PreviousDocuments == null)
			{
				g4PreviousDocuments = new G4PreviousDocumentCollection(this);
				g4PreviousDocuments.Load();
				RegisterEditableChildObject(g4PreviousDocuments);
			}

			return g4PreviousDocuments;
		}
	}
	G4PreviousDocumentCollection g4PreviousDocuments;

	protected override void OnChangedRepresentativeJobDocAddressRequirement()
	{
		if (Header.IsPhase5 && Header.ESNctsHeader.CEN_TNNArrival && HeaderTNN != null)
		{
			HeaderTNN.MovementHeader.Representative.OrganisationPK = Representative.OrganisationPK;
		}
	}

	public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments
		=> (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

	protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments()
		=> new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

	protected override ZBool ShouldSetSupportingDocumentsReadOnly => false;

	public new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments => (EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalDocuments;
	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	protected override ZBool ShouldSetAdditionalDocumentsReadOnly => false;

	public bool IsUnloadingRemarksReadOnlySpain => IsUnloadingRemarksReadOnly || BM_CustomsStatus.In(readOnlyStatuses) || Header.IsSent;

	readonly ZString[] readOnlyStatuses = new ZString[]
	{
		EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
		EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease
	};

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var dictionary = base.GetCusSupportingInfoTypes();
		dictionary.Add(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(G4PreviousDocument));
		return dictionary;
	}
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
	protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

	[MaxLength(3)]
	public override ZString BM_InBondEntryType
	{
		get => base.BM_InBondEntryType;
		set => base.BM_InBondEntryType = value;
	}

	[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.LocationsList))]
	public override ZString BM_LocationOfGoodsCode { get => base.BM_LocationOfGoodsCode; set => base.BM_LocationOfGoodsCode = value; }

	public new NctsCusGoodsLocation GoodsLocation => (NctsCusGoodsLocation)base.GoodsLocation;

	[ResourceStringData("A0101551-CD7B-4570-92C7-C6BC2ED9F4E4", Caption = "Broker", MediumCaption = "Broker", ShortCaption = "Broker", FullDescription = "The broker selected will be the responsible of declarations to Customs in this Job")]
	public override ZString BM_GS_NKCusAgent
	{
		get => base.BM_GS_NKCusAgent;
		set
		{
			var oldValue = base.BM_GS_NKCusAgent;
			base.BM_GS_NKCusAgent = value;

			if (!IsCopying && oldValue != value)
			{
				SetDefaultBH_CustomsProfile();
			}
		}
	}

	void SetDefaultBH_CustomsProfile()
	{
		var certificateNamesList = Header.Lookups.CertificateNames;
		var customsProfile = Header.BH_CustomsProfile;
		if (customsProfile.IsEmpty || !certificateNamesList.GetAllCodesZString().Contains(customsProfile))
		{
			if (certificateNamesList.Count == 1)
			{
				Header.BH_CustomsProfile = certificateNamesList[0].Code;
			}
			else
			{
				Header.BH_CustomsProfile = ZString.Empty;
			}
		}
	}

	public override ZBool BM_NoChangesToReport
	{
		get => base.BM_NoChangesToReport;
		set
		{
			var oldValue = BM_NoChangesToReport;
			base.BM_NoChangesToReport = value;
			if (!IsCopying && oldValue != BM_NoChangesToReport)
			{
				SetReadOnlyForTransportContainersAndDocuments(value);
				SetReadOnlyForUnloadingDifferencesDataCore(value);
				RefreshBindingIncludingChildren();
			}
		}
	}

	public override ZString BM_CustomsStatus
	{
		get => base.BM_CustomsStatus;
		set
		{
			var oldValue = BM_CustomsStatus;
			base.BM_CustomsStatus = value;
			if (!IsCopying && oldValue != BM_CustomsStatus)
			{
				SetReadOnlyForTransportContainersAndDocuments(BM_NoChangesToReport);
				SetReadOnlyForUnloadingDifferencesDataCore(BM_NoChangesToReport);
				RefreshBindingIncludingChildren();
			}
		}
	}

	public override void OnSaving()
	{
		base.OnSaving();
		if (IsPhase5 && BM_PaperlessInbondNum.IsEmpty)
		{
			BM_PaperlessInbondNum = Header.BH_JobReference.SubstringSafe(0, Schema.BM_PaperlessInbondNumMaxLength);
		}
		SetReadOnlyForTransportContainersAndDocuments(BM_NoChangesToReport);
		SetReadOnlyForUnloadingDifferencesDataCore(BM_NoChangesToReport);
		RefreshBindingIncludingChildren();
	}

	public override void OnLoaded()
	{
		base.OnLoaded();
		SetReadOnlyForTransportContainersAndDocuments(BM_NoChangesToReport);
		SetReadOnlyForUnloadingDifferencesDataCore(BM_NoChangesToReport);
		RefreshBindingIncludingChildren();
		Header?.Bills.RefreshBindingIncludingChildren();
	}

	void SetReadOnlyForTransportContainersAndDocuments(ZBool value)
	{
		value = value || IsUnloadingRemarksReadOnlySpain;
		var containers = Header?.ArrivalHeaderContainers;
		if (containers != null)
		{
			containers.SetReadOnlyIncludingChildren(value);
			foreach (EU.NCTS.Business.NctsArrivalHeaderContainer container in containers)
			{
				container.Seals.SetReadOnlyIncludingChildren(IsUnloadingRemarksReadOnlySpain || BM_StateOfSealsBoolean);
			}
		}
		ArrivalTransportInfos.SetReadOnlyIncludingChildren(value);
		AdditionalDocuments.SetReadOnlyIncludingChildren(value);
		SupportingDocuments.SetReadOnlyIncludingChildren(value);
		RefreshBindingIncludingChildren();
	}

	protected override ZBool ShouldGuaranteeForArrivalBeVisibleCore => EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode) && ArrivalGoodsLocationIsInPremises;

	ZBool ArrivalGoodsLocationIsInPremises
	{
		get
		{
			var result = false;
			var arrivalGoodsLocation = GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty;
			var isTemporaryStorageEnabled = EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode);

			result = Factory.GetCachedValue(FormattableString.Invariant($"ArrivalGoodsLocationIsInPremises_{arrivalGoodsLocation}_{isTemporaryStorageEnabled}"), () =>
			{
				var res = false;
				if (!arrivalGoodsLocation.IsEmpty)
				{
					res = EU.Business.TemporaryStorageHelper.IsLocationManagedInPremises(Factory, arrivalGoodsLocation, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);
				}

				return res;
			});

			return result;
		}
	}

	protected override void SetReadOnlyForUnloadingDifferencesDataCore(bool readOnly)
	{
		if (Header is NctsHeader header)
		{
			readOnly = readOnly || IsUnloadingRemarksReadOnlySpain;

			foreach (var bill in header.Bills)
			{
				bill.ArrivalGoodsItems.ForEach(g =>
				{
					g.SupportingDocuments.SetReadOnlyIncludingChildren(readOnly);
					g.AdditionalInfos.SetReadOnlyIncludingChildren(readOnly);
				});
				bill.SupportingDocuments.SetReadOnlyIncludingChildren(readOnly);
				bill.AdditionalDocuments.SetReadOnlyIncludingChildren(readOnly);
				bill.PreviousDocuments.SetReadOnlyIncludingChildren(readOnly);
				bill.ArrivalTransportInfos.SetReadOnlyIncludingChildren(readOnly);
			}
			header.Bills.RefreshBinding();
		}
	}

	protected override EU.NCTS.Business.INctsCommonCargoDescCollection<EU.NCTS.Business.NctsCommonCargoDesc> CreateGoodsItems()
		=> new EU.NCTS.Business.NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>(this);

	protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsArrivalMovementHeaderLookups(this);

	protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);

	protected override EU.NCTS.Business.IArrivalCusTransportMeansCollection<EU.NCTS.Business.ArrivalCusTransportMeans> GetNewArrivalTransportInfos()
		=> new EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

	protected override Type CusInBondCargoDescTypeCore => typeof(NctsArrivalAndUnloadingCargoDesc);

	protected override void CopyDocumentsToArrivalGoodsItems(EU.NCTS.Business.NctsDepartureCargoDesc goodsItem, EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc arrivalGoodsItem)
	{
		var supportingDocs = goodsItem.SupportingDocuments;
		for (ZShort i = 0; i < supportingDocs.Count; i++)
		{
			var newDoc = CopySupportingDocument(supportingDocs[i], arrivalGoodsItem);
			newDoc.CSI_LineNo = i + 1;
		}
	}

	[ResourceStringData("ES.CusESNctsHeader.IsSimplifiedNctsProcedure", Caption = "Simplified Procedure", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZBool IsSimplifiedNctsProcedure { get => base.IsSimplifiedNctsProcedure; set => base.IsSimplifiedNctsProcedure = value; }

	public void GenerateTNNDeparture(TnnDataCodeInfo tnnDataCodeInfo)
	{
		if (!Header.ESNctsHeader.CEN_TNNArrival)
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			Header.ESNctsHeader.CEN_TNNArrival = true;
			departureHeader.ESNctsHeader.CEN_TNNArrival = true;
			departureHeader.MovementReferenceEntryNumber.CE_EntryNum = Header.MovementReferenceNumber;
			departureHeader.MovementReferenceEntryNumber.CE_IssueDate = tnnDataCodeInfo.AcceptanceDate;
			departureHeader.ClearanceEntryNumber.CE_IssueDate = tnnDataCodeInfo.ClearanceDate;
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.Consignee.OrganisationPK = Header.DestinationTrader.OrganisationPK;

			var depMovement = departureHeader.MovementHeader;
			depMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			depMovement.Representative.OrganisationPK = Representative.OrganisationPK;
			depMovement.GoodsLocation.CGL_AdditionalIdentifier = GoodsLocation.CGL_AdditionalIdentifier;

			BM_BM_DepartureMovement = depMovement.PK;
		}
	}

	public NctsHeader HeaderTNN => headerTNN ??= FindRelevantDepartureTNN();
	NctsHeader headerTNN;

	NctsHeader FindRelevantDepartureTNN()
	{
		NctsHeader nctsHeader = null;
		if (Header.ESNctsHeader.CEN_TNNArrival)
		{
			nctsHeader = Factory.Load<NctsDepartureMovementHeader>(BM_BM_DepartureMovement)?.Header;
			RegisterEditableChildObject(nctsHeader);
		}

		return nctsHeader;
	}

	public void LoadDataForUnloadingFromDeparture(NctsHeader departureHeader)
	{
		var departureMovement = departureHeader.MovementHeader;
		BM_InlandTransportMode = departureMovement.BM_InlandTransportMode;
		BM_GrossWeight = departureMovement.BM_GrossWeight;
		LoadTransportInfoDataForUnloadingForHeader(departureHeader);
		LoadContainerDataForUnloading(departureHeader);
		LoadBillDataForUnloading(departureHeader);
		if (!IsInPhase5TransitionPeriod)
		{
			LoadSupportingDocumentDataForUnloadingForHeader(Header, departureHeader);
			LoadAdditionalInfoDataForUnloadingHeader(Header, departureHeader, AdditionalInfoSubTypeList.Codes.TransportDocument);
			LoadAdditionalInfoDataForUnloadingHeader(Header, departureHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference);
		}
	}

	bool AnyBillTransportInfoDifferentFromHeader(NctsHeader departureHeader, TransportInfo departureMovementTransportInfo) =>
		departureHeader.Bills
			.Select(TransportInfo.CreateFromBill)
			.Any(x => !x.IsEmpty && x != departureMovementTransportInfo);

	void LoadTransportInfoDataForUnloadingForHeader(NctsHeader departureHeader)
	{
		var departureMovementTransportInfo = TransportInfo.CreateFromHeader(departureHeader.MovementHeader);
		if (AnyBillTransportInfoDifferentFromHeader(departureHeader, departureMovementTransportInfo))
		{
			ArrivalTransportInfos.Clear();
			return;
		}

		var departureTransportMeans = TransportMeansInfo.CreateFromTransportInfo(departureMovementTransportInfo);

		CopyTransportMeans(departureTransportMeans, ArrivalTransportInfos);
	}

	void LoadTransportInfoDataForUnloadingFromBill(NctsHeader departureHeader, NctsBill departureBill, NctsBill arrivalBill)
	{
		var departureBillTransportInfo = TransportInfo.CreateFromBill(departureBill)
			.FillEmptyValues(TransportInfo.CreateFromHeader(departureHeader.MovementHeader));
		var departureTransportMeans = TransportMeansInfo.CreateFromTransportInfo(departureBillTransportInfo);

		CopyTransportMeans(departureTransportMeans, arrivalBill.ArrivalTransportInfos);
	}

	void CopyTransportMeans(IReadOnlyCollection<TransportMeansInfo> departureTransportMeans, EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> arrivalTransportInfos)
	{
		foreach (var transportMean in departureTransportMeans)
		{
			var transportInfo = arrivalTransportInfos.Cast<ArrivalCusTransportMeans>().FirstOrDefault(x => x.TPM_SequenceNumber == transportMean.SequenceNumber);
			if (transportInfo == null)
			{
				transportInfo = arrivalTransportInfos.AddNew();
				transportInfo.TPM_SequenceNumber = transportMean.SequenceNumber;
			}

			transportInfo.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			transportInfo.TPM_TypeOfIdentification = transportMean.IdentificationType;
			transportInfo.TPM_IdentificationNumber = transportMean.IdentificationNumber;
			transportInfo.TPM_RN_NKTransportNationality = transportMean.Nationality;
		}
	}

	void LoadContainerDataForUnloading(NctsHeader departureHeader)
	{
		var arrivalContainers = Header.ArrivalHeaderContainers;

		var departureContainersList = departureHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Where(x => ContainerSelectedInItems(x.BC_ContainerNum));
		foreach (var departureContainer in departureContainersList)
		{
			var departureContainerSeqNum = departureContainer.BC_SequenceNumber;
			var container = arrivalContainers.Cast<EU.NCTS.Business.NctsArrivalHeaderContainer>().FirstOrDefault(x => x.BC_SequenceNumber == departureContainerSeqNum);
			if (container == null)
			{
				container = arrivalContainers.AddNew();
				container.BC_SequenceNumber = departureContainerSeqNum;
			}

			container.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			container.BC_ContainerNum = departureContainer.BC_ContainerNum;
			container.BC_Mode = departureContainer.BC_Mode;

			var containerSeals = container.Seals;

			var departureContainerSeals = GetDepartureContainerSealsList(departureContainer);

			foreach (var departureSeal in departureContainerSeals)
			{
				var departureSealSeqNum = departureSeal.Item1;
				var seal = containerSeals.Cast<CusSeal>().FirstOrDefault(x => x.BK_SequenceNumber == departureSealSeqNum);
				if (seal == null)
				{
					seal = containerSeals.AddNew();
					seal.BK_SequenceNumber = departureSealSeqNum;
				}

				seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
				seal.BK_SealNumber = departureSeal.Item2;
			}
		}

		ZBool ContainerSelectedInItems(ZString containerNum)
		{
			return departureHeader.Bills.Any(x => x.GoodsItems.Cast<NctsDepartureCargoDesc>().Any(y => y.ContainersSelected.Contains(containerNum)));
		}

		List<Tuple<ZShort, ZString>> GetDepartureContainerSealsList(NctsDepartureHeaderContainer departureContainer)
		{
			var departureContainerSeals = new List<Tuple<ZShort, ZString>>();

			var seal1 = departureContainer.BC_Seal1;
			if (!seal1.IsEmpty)
			{
				var seqNumForSeal1 = (ZShort)1;
				departureContainerSeals.Add(seqNumForSeal1, seal1);
			}
			var seal2 = departureContainer.BC_Seal2;
			if (!seal2.IsEmpty)
			{
				var seqNumForSeal2 = (ZShort)2;
				departureContainerSeals.Add(seqNumForSeal2, seal2);
			}

			departureContainerSeals.AddRange(departureContainer.AdditionalSeals.Select(s => new Tuple<ZShort, ZString>(s.BK_SequenceNumber, s.BK_SealNumber)).ToList());

			return departureContainerSeals;
		}
	}

	void LoadBillDataForUnloading(NctsHeader departureHeader)
	{
		var bills = Header.Bills;

		var departureMovementTransportInfo = TransportInfo.CreateFromHeader(departureHeader.MovementHeader);
		var anyDifferentDepartureBillTransportInfo = AnyBillTransportInfoDifferentFromHeader(departureHeader, departureMovementTransportInfo);

		if (!anyDifferentDepartureBillTransportInfo)
		{
			foreach (var arrivalBill in bills)
			{
				arrivalBill.ArrivalTransportInfos.Clear();
			}
		}

		foreach (var departureBill in departureHeader.Bills)
		{
			var houseConsignmentSeqNum = departureBill.SequenceNumber.ToString();
			var bill = bills.Cast<NctsBill>().FirstOrDefault(x => x.MovementDetail.B9_SeqNo == houseConsignmentSeqNum);
			if (bill == null)
			{
				bill = bills.AddNew();
				bill.MovementDetail.B9_SeqNo = houseConsignmentSeqNum;
			}

			bill.MovementDetail.B9_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			bill.B0_Weight = departureBill.B0_Weight;
			bill.B0_WeightUQ = departureBill.B0_WeightUQ;

			LoadGoodsItemDataForUnloading(bill, departureBill);
			if (!IsInPhase5TransitionPeriod)
			{
				if (anyDifferentDepartureBillTransportInfo)
				{
					LoadTransportInfoDataForUnloadingFromBill(departureHeader, departureBill, bill);
				}

				LoadSupportingDocumentDataForUnloadingForBills(bill, departureBill);
				LoadAdditionalInfoDataForUnloadingBill(bill, departureBill, AdditionalInfoSubTypeList.Codes.TransportDocument);
				LoadAdditionalInfoDataForUnloadingBill(bill, departureBill, AdditionalInfoSubTypeList.Codes.AdditionalReference);
			}
		}
	}

	void LoadGoodsItemDataForUnloading(NctsBill arrivalBill, NctsBill departureBill)
	{
		var goodsItems = arrivalBill.ArrivalGoodsItems;
		foreach (var departureItem in departureBill.GoodsItems)
		{
			var departureItemSeqNum = departureItem.BY_LineNo;
			var goodsItem = goodsItems.Cast<NctsArrivalCargoDesc>().FirstOrDefault(x => x.BY_LineNo == departureItemSeqNum);
			if (goodsItem == null)
			{
				goodsItem = goodsItems.AddNew();
				goodsItem.BY_LineNo = departureItemSeqNum;
			}

			goodsItem.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			goodsItem.BY_DeclarationGoodsItemNumber = departureItem.BY_DeclarationGoodsItemNumber;
			goodsItem.BY_Description = departureItem.BY_Description;
			goodsItem.BY_CusC4Number = departureItem.BY_CusC4Number;
			goodsItem.BY_HarmonisedTariff = departureItem.BY_HarmonisedTariff;
			var previousDocumentN337WithQtyAndKGM = departureItem.PreviousDocuments.FirstOrDefault(x => x.CSI_Code == ES.Business.PreviousDocumentHelper.PreviousDocumentCodeN337 &&
																										!x.CSI_Quantity.IsEmpty &&
																										x.CSI_UnitOfQuantity == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram);

			var isIsMovementReferenceNumberESAndHasPreviousDocumentN337WithQtyAndKGM = arrivalBill.Header.IsMovementReferenceNumberES && previousDocumentN337WithQtyAndKGM != null;
			goodsItem.BY_GrossWeight = isIsMovementReferenceNumberESAndHasPreviousDocumentN337WithQtyAndKGM ? previousDocumentN337WithQtyAndKGM.CSI_Quantity : departureItem.BY_GrossWeight;
			goodsItem.BY_GrossWeightUnit = isIsMovementReferenceNumberESAndHasPreviousDocumentN337WithQtyAndKGM ? Core.Constants.Weight.Kilograms : departureItem.BY_GrossWeightUnit;
			goodsItem.BY_NetWeight = departureItem.BY_NetWeight;
			goodsItem.BY_NetWeightUnit = departureItem.BY_NetWeightUnit;

			LoadPackageDataForUnloading(goodsItem, departureItem);
			LoadSupportingDocumentDataForUnloadingForGoodsItems(goodsItem, departureItem);
			LoadAdditionalInfoDataForUnloadingGoodsItemsOrAllToGoodsItems(goodsItem, departureItem, AdditionalInfoSubTypeList.Codes.TransportDocument);
			LoadAdditionalInfoDataForUnloadingGoodsItemsOrAllToGoodsItems(goodsItem, departureItem, AdditionalInfoSubTypeList.Codes.AdditionalReference);
			if (!IsInPhase5TransitionPeriod)
			{
				LoadLiabilityDataForUnloading(goodsItem, departureItem);
			}
		}
	}

	void LoadLiabilityDataForUnloading(NctsArrivalCargoDesc arrivalGoodsItem, NctsDepartureCargoDesc departureGoodsItem)
	{
		if (ShouldGuaranteeForArrivalBeVisibleCore)
		{
			arrivalGoodsItem.BY_RN_NKCountryOfOrigin = departureGoodsItem.BY_RN_NKCountryOfOrigin;
			arrivalGoodsItem.BY_HarmonisedTariff = departureGoodsItem.BY_HarmonisedTariff;
			arrivalGoodsItem.BY_CustomsSecondQuantity = departureGoodsItem.BY_CustomsSecondQuantity;
			arrivalGoodsItem.BY_CustomsSecondUnitQty = departureGoodsItem.BY_CustomsSecondUnitQty;
			arrivalGoodsItem.BY_CustomsThirdQuantity = departureGoodsItem.BY_CustomsThirdQuantity;
			arrivalGoodsItem.BY_CustomsThirdUnitQty = departureGoodsItem.BY_CustomsThirdUnitQty;
			arrivalGoodsItem.BY_CustomsFourthQuantity = departureGoodsItem.BY_CustomsFourthQuantity;
			arrivalGoodsItem.BY_CustomsFourthUnitQty = departureGoodsItem.BY_CustomsFourthUnitQty;
			arrivalGoodsItem.BY_MonetaryValue = departureGoodsItem.BY_MonetaryValue;
			foreach (var departureAdditionalSupplementaryCode in departureGoodsItem.AdditionalSupplementaryCodes.ToArray())
			{
				arrivalGoodsItem.AdditionalSupplementaryCodes.Add(departureAdditionalSupplementaryCode.Clone());
			}
		}
	}

	void LoadPackageDataForUnloading(NctsArrivalCargoDesc arrivalGoodsItem, NctsDepartureCargoDesc departureGoodsItem)
	{
		var packages = arrivalGoodsItem.Packages;

		var arrivalContainers = Header.ArrivalHeaderContainers.Cast<EU.NCTS.Business.NctsArrivalHeaderContainer>();
		var departureGoodsItemIsVehicles = departureGoodsItem.IsVehicles;

		foreach (var departurePackage in departureGoodsItem.Packages)
		{
			var departurePackageSeqNum = departurePackage.B5_SequenceNumber;
			var pack = packages.Cast<NctsPackage>().FirstOrDefault(x => x.B5_SequenceNumber == departurePackageSeqNum);
			if (pack == null)
			{
				pack = packages.AddNew();
				pack.B5_SequenceNumber = departurePackageSeqNum;
			}

			pack.B5_TypeOfDifference = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			pack.B5_UnitType = departureGoodsItemIsVehicles ? RefCusCodeList.PackageType.Frame : departurePackage.B5_UnitType;
			pack.B5_UnitCount = departureGoodsItemIsVehicles ? (ZLong)1 : departurePackage.B5_UnitCount;
			pack.B5_MarksAndNumbers = departurePackage.B5_MarksAndNumbers;
			pack.B5_PackageID = departurePackage.B5_PackageID;
			pack.B5_Brand = departurePackage.B5_Brand;
			pack.B5_Model = departurePackage.B5_Model;
			pack.IsDataLoadFromDeparture = true;

			var departureContainersSeqNumForPackage = departurePackage.ContainersPivot.Containers.Select(x => x.BC_SequenceNumber);
			var arrivalContainersFromDeparture = arrivalContainers.Where(x => departureContainersSeqNumForPackage.Contains(x.BC_SequenceNumber));

			var pivots = pack.ContainersPivot.Cast<GenPivot>().GroupBy(x => x.XX_Relation2ID).ToDictionary(x => x.Key, y => y.ToList());
			foreach (var cont in arrivalContainersFromDeparture)
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

	void LoadSupportingDocumentDataForUnloadingForHeader(NctsHeader arrivalHeader, NctsHeader departureHeader)
	{
		var supportingDocuments = arrivalHeader.ArrivalMovementHeader.SupportingDocuments;
		foreach (var departureSupDoc in departureHeader.MovementHeader.SupportingDocuments)
		{
			var departureSupDocSeqNum = departureSupDoc.CSI_LineNo;
			var supDoc = supportingDocuments.FirstOrDefault(x => x.CSI_LineNo == departureSupDocSeqNum) ?? supportingDocuments.AddNew();
			supDoc.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			supDoc.CSI_Code = departureSupDoc.CSI_Code;
			supDoc.CSI_ReferenceNumber = departureSupDoc.CSI_ReferenceNumber;
			supDoc.CSI_LineNo = departureSupDocSeqNum;
		}
	}

	void LoadSupportingDocumentDataForUnloadingForGoodsItems(NctsArrivalCargoDesc arrivalGoodsItem, NctsDepartureCargoDesc departureGoodsItem)
	{
		var supportingDocuments = arrivalGoodsItem.SupportingDocuments;
		foreach (var departureSupDoc in departureGoodsItem.SupportingDocuments)
		{
			var departureSupDocSeqNum = departureSupDoc.CSI_LineNo;
			var supDoc = supportingDocuments.FirstOrDefault(x => x.CSI_LineNo == departureSupDocSeqNum) ?? supportingDocuments.AddNew();
			supDoc.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			supDoc.CSI_Code = departureSupDoc.CSI_Code;
			supDoc.CSI_ReferenceNumber = departureSupDoc.CSI_ReferenceNumber;
			supDoc.CSI_LineNo = departureSupDocSeqNum;
		}
	}

	void LoadSupportingDocumentDataForUnloadingForBills(NctsBill arrivalBill, NctsBill departureBill)
	{
		var supportingDocuments = arrivalBill.SupportingDocuments;
		foreach (var departureSupDoc in departureBill.SupportingDocuments)
		{
			var departureSupDocSeqNum = departureSupDoc.CSI_LineNo;
			var supDoc = supportingDocuments.FirstOrDefault(x => x.CSI_LineNo == departureSupDocSeqNum) ?? supportingDocuments.AddNew();
			supDoc.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			supDoc.CSI_Code = departureSupDoc.CSI_Code;
			supDoc.CSI_ReferenceNumber = departureSupDoc.CSI_ReferenceNumber;
			supDoc.CSI_LineNo = departureSupDocSeqNum;
		}
	}

	void LoadAdditionalInfoDataForUnloadingGoodsItemsOrAllToGoodsItems(NctsArrivalCargoDesc arrivalGoodsItem, NctsDepartureCargoDesc departureGoodsItem, ZString subType)
	{
		var departureAddInfosToLoad = departureGoodsItem.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == subType).ToList();
		if (IsInPhase5TransitionPeriod)
		{
			departureAddInfosToLoad.AddRange(departureGoodsItem.Bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList());
			departureAddInfosToLoad.AddRange(departureGoodsItem.Header.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList());
		}

		var additionalInfos = arrivalGoodsItem.AdditionalInfos.Where(x => x.CSI_SubType == subType);
		foreach (var departureAddInfo in departureAddInfosToLoad)
		{
			var departureAddInfoSeqNum = departureAddInfo.CSI_LineNo;
			var addInfo = additionalInfos.FirstOrDefault(x => x.CSI_LineNo == departureAddInfoSeqNum);
			if (addInfo == null)
			{
				addInfo = arrivalGoodsItem.AdditionalInfos.AddNew();
				addInfo.CSI_SubType = subType;
			}

			addInfo.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			addInfo.CSI_Code = departureAddInfo.CSI_Code;
			addInfo.CSI_ReferenceNumber = departureAddInfo.CSI_ReferenceNumber;
			addInfo.CSI_LineNo = departureAddInfoSeqNum;
		}
	}

	void LoadAdditionalInfoDataForUnloadingBill(NctsBill arrivalBill, NctsBill departureBill, ZString subType)
	{
		var departureAddInfosToLoad = departureBill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList();

		var additionalInfos = arrivalBill.AdditionalDocuments.Where(x => x.CSI_SubType == subType);
		foreach (var departureAddInfo in departureAddInfosToLoad)
		{
			var departureAddInfoSeqNum = departureAddInfo.CSI_LineNo;
			var addInfo = additionalInfos.FirstOrDefault(x => x.CSI_LineNo == departureAddInfoSeqNum);
			if (addInfo == null)
			{
				addInfo = arrivalBill.AdditionalDocuments.AddNew();
				addInfo.CSI_SubType = subType;
			}

			addInfo.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			addInfo.CSI_Code = departureAddInfo.CSI_Code;
			addInfo.CSI_ReferenceNumber = departureAddInfo.CSI_ReferenceNumber;
			addInfo.CSI_LineNo = departureAddInfoSeqNum;
		}
	}

	void LoadAdditionalInfoDataForUnloadingHeader(NctsHeader arrivalHeader, NctsHeader departureHeader, ZString subType)
	{
		var departureAddInfosToLoad = departureHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == subType).ToList();

		var additionalInfos = arrivalHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == subType);
		foreach (var departureAddInfo in departureAddInfosToLoad)
		{
			var departureAddInfoSeqNum = departureAddInfo.CSI_LineNo;
			var addInfo = additionalInfos.FirstOrDefault(x => x.CSI_LineNo == departureAddInfoSeqNum);
			if (addInfo == null)
			{
				addInfo = arrivalHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
				addInfo.CSI_SubType = subType;
			}

			addInfo.CSI_Status = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			addInfo.CSI_Code = departureAddInfo.CSI_Code;
			addInfo.CSI_ReferenceNumber = departureAddInfo.CSI_ReferenceNumber;
			addInfo.CSI_LineNo = departureAddInfoSeqNum;
		}
	}

	protected override ZBool ShouldConsiderDIFUnloadedStateForPackageCount => true;

	protected override ICustomsOffice GetDestinationCustomsOfficeForArrival() => CustomsOffices.Cast<NctsESOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);

	protected override EU.NCTS.Business.INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForArrivalCore()
		=> new EU.NCTS.Business.NctsGuaranteeCollection<NctsGuarantee>(this);

	public ZBool CreateTemporaryStorageData()
	{
		var arrivalGoodsItemsWithNotMISPackages = Header.Bills
															.Where(bill => bill.UnloadedStatus != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS)
															.SelectMany(bill => bill.ArrivalGoodsItems.Where(g => g.UnloadedStatus != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS
																												&& g.Packages.ToArray().Any(p => ((NctsPackage)p).UnloadedStatus != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS)));
		var result = arrivalGoodsItemsWithNotMISPackages.Any();

		if (result)
		{
			const string previousReferenceTypeCode = "NCTS5";

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_AppCode = CusTempStorageRegHeader.ESAppCode;
			regHeader.SRH_Reference = Header.SummaryEntryNumber.CE_EntryNum;
			var issueDate = Header.MovementReferenceIssueDate;
			regHeader.SRH_ArrivalDate = issueDate.Date;
			regHeader.SRH_PresentationDate = issueDate;
			regHeader.SRH_PreviousReferenceType = previousReferenceTypeCode;
			regHeader.SRH_PreviousReference = Header.MovementReferenceNumber;
			regHeader.SRH_Status = EU.Business.TempStorageDeclarationStatusList.Codes.Open;

			var arrivalGoodsLocation = GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty;
			var arrivalGuarantee = GuaranteesForArrival.FirstOrDefault();
			ES.Business.CusTempStorage.TemporaryStorageHelper.SetPremisesAndGuaranteeIntoRegHeader(Factory, regHeader, arrivalGoodsLocation, arrivalGuarantee, false);

			var traderId = OrgHeaderExtension.GetIDCode(Header.DestinationTrader?.Address?.Header);
			var nctsHeaderReferenceNum = Header.BH_JobReference;
			var branchHomePort = HeaderBranch.HomePort;
			var unloadingDate = BM_UnloadingDate;

			var goodsItemsWithLiabilities = GetLiabilitiesForGoodsItemsFromGuarantee(arrivalGoodsItemsWithNotMISPackages);

			var seqNumForNEWItems = (ZInt)99001;

			var regLineSeq = (ZInt)1;
			foreach (var (goodsItem, liability) in goodsItemsWithLiabilities)
			{
				var regLineItem = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
				var goodsItemStatusIsDIF = goodsItem.UnloadedStatus == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
				regLineItem.SRI_Tariff = !goodsItem.LiabilityTariff.IsEmpty ? goodsItem.LiabilityTariff : goodsItemStatusIsDIF ? goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff : goodsItem.BY_HarmonisedTariff;
				regLineItem.SRI_CusC4Number = goodsItemStatusIsDIF ? goodsItem.UnloadedGoodsItem.BY_CusC4Number : goodsItem.BY_CusC4Number;
				regLineItem.SRI_GoodsDescription = goodsItemStatusIsDIF ? goodsItem.UnloadedGoodsItem.BY_Description : goodsItem.BY_Description;

				var itemNumber = goodsItem.UnloadedStatus == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW
															? seqNumForNEWItems
															: goodsItem.BY_DeclarationGoodsItemNumber;
				var packages = goodsItem.Packages.Cast<NctsPackage>().Where(p => p.UnloadedStatus != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS);

				var packTotalQtyInItem = packages.Sum(p => PackageHelper.PackTypeIsBulk(p.EffectiveUnitType, Factory)
																? 1
																: p.UnloadedStatus == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF ? p.PackDifference.B5_UnitCount : p.B5_UnitCount);
				var packTotalGrossWeightInItem = (ZDecimal)packages.Sum(p => p.B5_GrossWeight);
				var goodsItemGrossWeight = goodsItemStatusIsDIF ? goodsItem.UnloadedGoodsItem.BY_GrossWeight : goodsItem.BY_GrossWeight;
				var totalItemGrossWeight = packTotalGrossWeightInItem.IsEmpty ? goodsItemGrossWeight : packTotalGrossWeightInItem;

				var transactionPKsForItem = new List<ZGuid>();
				var transactionsTotalBondAmount = ZDecimal.Zero;
				var transactionsAndPivotsTotalGrossWeight = ZDecimal.Zero;

				foreach (var package in packages)
				{
					var packageStatusIsDIF = package.UnloadedStatus == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
					var packType = package.EffectiveUnitType;
					var packQty = packageStatusIsDIF ? package.PackDifference.B5_UnitCount : package.B5_UnitCount;
					var packQtyWithBulkCheck = PackageHelper.PackTypeIsBulk(packType, Factory) ? 1 : packQty;
					var packGrossWeight = package.B5_GrossWeight;
					var pivotAndTransactionGrossWeightCalculated = packGrossWeight.IsEmpty
															? packTotalQtyInItem == packQtyWithBulkCheck
																? goodsItemGrossWeight
																: (ZDecimal)((goodsItemGrossWeight / packTotalQtyInItem) * packQtyWithBulkCheck)
															: packGrossWeight;

					(itemNumber, regLineSeq, var transactionPK, var transactionAndPivotGrossWeight, var transactionBondAmount, _) =
							ES.Business.CusTempStorage.TemporaryStorageHelper.CreateRegisterDataForPackage(Factory,
																				packType,
																				packQty.ToZInt(),
																				package.EffectiveMarksAndNumbers,
																				pivotAndTransactionGrossWeightCalculated,
																				totalItemGrossWeight,
																				liability,
																				regHeader.PK,
																				regHeader.SRH_ArrivalDate,
																				traderId,
																				regLineItem.PK,
																				itemNumber,
																				regLineSeq,
																				nctsHeaderReferenceNum,
																				issueDate.ToDateTimeOffset(branchHomePort),
																				unloadingDate,
																				CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration,
																				ZGuid.Empty,
																				Enumerable.Empty<Tuple<ZGuid, ZGuid>>(),
																				false,
																				goodsItemGrossWeight.IsEmpty);

					transactionPKsForItem.Add(transactionPK);
					transactionsTotalBondAmount += transactionBondAmount;
					transactionsAndPivotsTotalGrossWeight += transactionAndPivotGrossWeight;
				}

				var transactionsForItem = ES.Business.CusTempStorage.TemporaryStorageHelper.GetLineTransactionsFromListOfPks(Factory, transactionPKsForItem);
				ES.Business.CusTempStorage.TemporaryStorageHelper.CorrectBondAmountInLineTransactions(transactionsForItem, liability, transactionsTotalBondAmount);
				ES.Business.CusTempStorage.TemporaryStorageHelper.CorrectGrossWeightInLineTransactions(transactionsForItem, totalItemGrossWeight, transactionsAndPivotsTotalGrossWeight);
				ES.Business.CusTempStorage.TemporaryStorageHelper.CorrectGrossWeightInItemPivots(Factory, regLineItem.PK, totalItemGrossWeight, transactionsAndPivotsTotalGrossWeight);

				regLineItem.SRI_GoodsItemNumber = itemNumber;

				if (regLineItem.SRI_GoodsItemNumber == seqNumForNEWItems)
				{
					seqNumForNEWItems++;
				}
			}
		}

		return result;
	}

	List<(NctsArrivalCargoDesc item, ZDecimal liability)> GetLiabilitiesForGoodsItemsFromGuarantee(IEnumerable<NctsArrivalCargoDesc> arrivalGoodsItems)
	{
		const int decimalsForCalculatedLiabilityAmount = 2;
		var result = new List<(NctsArrivalCargoDesc item, ZDecimal liability)>();

		var guarantee = GuaranteesForArrival.FirstOrDefault();
		if (guarantee != null && guarantee.PW_Override)
		{
			var goodsItemsTotalGrossWeight = ZDecimal.Zero;

			arrivalGoodsItems.ForEach(g => goodsItemsTotalGrossWeight += GetItemEffectiveGrossWeight(g));

			var guaranteeBondAmount = guarantee.PW_BondAmount;
			var totalLiabilityInItems = ZDecimal.Zero;

			arrivalGoodsItems.ForEach(g =>
			{
				var itemLiability = ((ZDecimal)(guaranteeBondAmount / goodsItemsTotalGrossWeight * GetItemEffectiveGrossWeight(g))).Truncate(decimalsForCalculatedLiabilityAmount);
				result.Add((g, itemLiability));
				totalLiabilityInItems += itemLiability;
			});

			return CorrectLiabilityAmountForGoodsItems(result, guaranteeBondAmount, totalLiabilityInItems);
		}
		else
		{
			arrivalGoodsItems.ForEach(g => result.Add((g, g.LiabilityAmount)));
			return result;
		}
	}

	ZDecimal GetItemEffectiveGrossWeight(NctsArrivalCargoDesc item)
	{
		var goodsItemGrossWeight = item.UnloadedStatus == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF ? item.UnloadedGoodsItem.BY_GrossWeight : item.BY_GrossWeight;
		var packages = item.Packages.Cast<NctsPackage>().Where(p => p.UnloadedStatus != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS);
		var packTotalGrossWeightInItem = packages.Sum(p => p.B5_GrossWeight);

		return packTotalGrossWeightInItem == 0 ? goodsItemGrossWeight : packTotalGrossWeightInItem;
	}

	List<(NctsArrivalCargoDesc item, ZDecimal liability)> CorrectLiabilityAmountForGoodsItems(List<(NctsArrivalCargoDesc item, ZDecimal liability)> itemsWithLiability, ZDecimal guaranteeBondAmount, ZDecimal itemsTotalLiability)
	{
		var diff_amount = itemsTotalLiability - guaranteeBondAmount;
		var correctionAmount = (decimal)0.01;
		if (diff_amount != 0)
		{
			var result = new List<(NctsArrivalCargoDesc item, ZDecimal liability)>();

			itemsWithLiability.ForEach(x =>
			{
				var liability = x.liability;

				(diff_amount, liability) = ES.Business.CusTempStorage.TemporaryStorageHelper.CorrectDecimalValue(diff_amount, correctionAmount, liability);

				result.Add((x.item, liability));
			});

			return result;
		}
		else
		{
			return itemsWithLiability;
		}
	}

	public void AddNewGuaranteeTransactionForTemporaryStorage()
	{
		var header = Header;
		var arrivalGuarantee = GuaranteesForArrival.FirstOrDefault();
		var bondAmount = arrivalGuarantee.PW_BondAmount;
		if (arrivalGuarantee != null & bondAmount > 0)
		{
			var summaryEntryNumber = header.SummaryEntryNumber;
			var summaryDate = summaryEntryNumber.CE_IssueDate;
			var transactionDate = summaryDate.IsEmpty ? ZDateTime.Now : summaryDate;
			var comment = string.Format((NoResString)"NCTS Arrival {0}. MRN: {1}", header.BH_JobReference, header.MovementReferenceNumber);
			arrivalGuarantee.CusGuarantee?.AddTransaction(summaryEntryNumber.CE_EntryNum, comment, ZString.Empty, ZString.Empty,
															bondAmount * -1, ZDecimal.Zero, status: PermitTransactionStatusList.Codes.Confirmed, transactionDate: transactionDate,
															checkBursting: false);
		}
	}

	sealed record TransportInfo : IEquatable<TransportInfo>
	{
		public ZString TransportId { get; }
		public ZString TransportType { get; }
		public ZString TransportNationality { get; }
		public ZString TransportMode { get; }
		public ZString Trailer1Id { get; }
		public ZString Trailer1Nationality { get; }
		public ZString Trailer2Id { get; }
		public ZString Trailer2Nationality { get; }

		public TransportInfo(
			ZString transportId,
			ZString transportType,
			ZString transportNationality,
			ZString transportMode,
			ZString trailer1Id,
			ZString trailer1Nationality,
			ZString trailer2Id,
			ZString trailer2Nationality)
		{
			TransportId = transportId;
			TransportType = transportType;
			TransportNationality = transportNationality;
			TransportMode = transportMode;
			Trailer1Id = trailer1Id;
			Trailer1Nationality = trailer1Nationality;
			Trailer2Id = trailer2Id;
			Trailer2Nationality = trailer2Nationality;
		}

		public bool IsEmpty =>
			TransportId.IsEmpty &&
			TransportType.IsEmpty &&
			TransportNationality.IsEmpty &&
			(TransportMode != ModeOfTransportList.Codes._3_RoadTransport ||
				(Trailer1Id.IsEmpty &&
				Trailer1Nationality.IsEmpty &&
				Trailer2Id.IsEmpty &&
				Trailer2Nationality.IsEmpty));

		public static TransportInfo CreateFromHeader(NctsDepartureMovementHeader departureMovement) =>
			new TransportInfo(
				departureMovement.BM_TransportAtDeparture,
				departureMovement.BM_TransportAtDepartureType,
				departureMovement.BM_RN_NKTransportAtDepartureCountry,
				departureMovement.BM_InlandTransportMode,
				departureMovement.BM_TransportAtDepartureTrailer1RegNo,
				departureMovement.BM_RN_NKTransportAtDepartureTrailer1Nationality,
				departureMovement.BM_TransportAtDepartureTrailer2RegNo,
				departureMovement.BM_RN_NKTransportAtDepartureTrailer2Nationality);

		public static TransportInfo CreateFromBill(NctsBill departureBill) =>
			new TransportInfo(
				departureBill.TransportAtDeparture,
				departureBill.TransportTypeAtDeparture,
				departureBill.TransportCountryAtDeparture,
				departureBill.InlandTransportModeAtDeparture,
				departureBill.Trailer1IDAtDeparture,
				departureBill.Trailer1NationalityAtDeparture,
				departureBill.Trailer2IDAtDeparture,
				departureBill.Trailer2NationalityAtDeparture);

		public TransportInfo FillEmptyValues(TransportInfo defaultValues) =>
			new TransportInfo(
				TransportId.IsEmpty ? defaultValues.TransportId : TransportId,
				TransportType.IsEmpty ? defaultValues.TransportType : TransportType,
				TransportNationality.IsEmpty ? defaultValues.TransportNationality : TransportNationality,
				TransportMode.IsEmpty ? defaultValues.TransportMode : TransportMode,
				Trailer1Id.IsEmpty ? defaultValues.Trailer1Id : Trailer1Id,
				Trailer1Nationality.IsEmpty ? defaultValues.Trailer1Nationality : Trailer1Nationality,
				Trailer2Id.IsEmpty ? defaultValues.Trailer2Id : Trailer2Id,
				Trailer2Nationality.IsEmpty ? defaultValues.Trailer2Nationality : Trailer2Nationality);
	}

	sealed record TransportMeansInfo
	{
		public ZShort SequenceNumber { get; }
		public ZString IdentificationType { get; }
		public ZString IdentificationNumber { get; }
		public ZString Nationality { get; }

		public TransportMeansInfo(
			ZShort sequenceNumber,
			ZString identificationType,
			ZString identification,
			ZString nationality)
		{
			SequenceNumber = sequenceNumber;
			IdentificationType = identificationType;
			IdentificationNumber = identification;
			Nationality = nationality;
		}

		public static IReadOnlyCollection<TransportMeansInfo> CreateFromTransportInfo(TransportInfo transportInfo)
		{
			var transportMeansList = new List<TransportMeansInfo>();

			if (!transportInfo.TransportId.IsEmpty)
			{
				transportMeansList.Add(new TransportMeansInfo(1, transportInfo.TransportType, transportInfo.TransportId, transportInfo.TransportNationality));
			}

			if (transportInfo.TransportMode == EU.Business.ModeOfTransportList.Codes._3_RoadTransport)
			{
				var trailerMode = EU.NCTS.Business.NctsTransportTypeOfIdList.Codes._31;

				if (!transportInfo.Trailer1Id.IsEmpty)
				{
					transportMeansList.Add(new TransportMeansInfo(2, trailerMode, transportInfo.Trailer1Id, transportInfo.Trailer1Nationality));
				}

				if (!transportInfo.Trailer2Id.IsEmpty)
				{
					transportMeansList.Add(new TransportMeansInfo(3, trailerMode, transportInfo.Trailer2Id, transportInfo.Trailer2Nationality));
				}
			}

			return transportMeansList;
		}
	}

	public void PopulateGuaranteeWhenLocationIsSelectedWithTemporaryStorage()
	{
		if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode)
			&& !Header.ArrivalMovementHeader.GoodsLocation.IsNull
			&& !Header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier.IsEmpty
			&& (Header.ArrivalMovementHeader.SingleGuaranteeForArrival.IsNull || Header.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber.IsEmpty))
		{
			var guaranteeNumber = CusGuaranteeHeaderHelper.PopulateGuaranteeFromLocation(Factory, Header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			if (!guaranteeNumber.IsEmpty)
			{
				Header.ArrivalMovementHeader.SingleGuaranteeForArrival.PW_BondNumber = guaranteeNumber;
			}
		}
	}
}
