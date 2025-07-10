using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
	, Integration.Customs.ES.IDepartureMovementHeader
	, ICusCodeDataTypeSupporter
	, IEuOfficeCodeProvider
{
	public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

	protected override void Guarantees_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = Guarantees.Count;
		Header.ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousGuaranteesCount, currentCount);
		previousGuaranteesCount = currentCount;
	}
	int previousGuaranteesCount;

	public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments
		=> (INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

	protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments()
		=> new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

	protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

	protected override void SupportingDocuments_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = SupportingDocuments.Count;
		Header.ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousSupportingDocumentsCount, currentCount);
		previousSupportingDocumentsCount = currentCount;
	}
	int previousSupportingDocumentsCount;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		SetDefaultBM_AdditionalDeclarationType();
	}

	void SetDefaultBM_AdditionalDeclarationType()
	{
		BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
	}

	public override ZString BM_Phase
	{
		get => base.BM_Phase;
		set
		{
			var oldValue = base.BM_Phase;
			base.BM_Phase = value;

			if (!IsCopying && oldValue != value)
			{
				var customsOffices = Header.IsPhase5 ? CustomsOffices : Header.CustomsOffices;
				customsOffices.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString BM_InBondEntryType
	{
		get => base.BM_InBondEntryType;
		set
		{
			var oldValue = base.BM_InBondEntryType;
			base.BM_InBondEntryType = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZString TirCarnetNumber
	{
		get => base.TirCarnetNumber;
		set
		{
			var oldValue = base.TirCarnetNumber;
			base.TirCarnetNumber = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZBool BM_ReducedDatasetIndicator
	{
		get => base.BM_ReducedDatasetIndicator;
		set
		{
			var oldValue = base.BM_ReducedDatasetIndicator;
			base.BM_ReducedDatasetIndicator = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZString BM_RN_NKCountryOfDispatch
	{
		get => base.BM_RN_NKCountryOfDispatch;
		set
		{
			var oldValue = base.BM_RN_NKCountryOfDispatch;
			base.BM_RN_NKCountryOfDispatch = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZString BM_RL_NKDestinationPort
	{
		get => base.BM_RL_NKDestinationPort;
		set
		{
			var oldValue = base.BM_RL_NKDestinationPort;
			base.BM_RL_NKDestinationPort = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZDecimal BM_GrossWeight
	{
		get => base.BM_GrossWeight;
		set
		{
			var oldValue = base.BM_GrossWeight;
			base.BM_GrossWeight = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZString BM_CustomsStatus
	{
		get => base.BM_CustomsStatus;
		set
		{
			var oldValue = base.BM_CustomsStatus;
			base.BM_CustomsStatus = value;

			if (!IsCopying && oldValue != BM_CustomsStatus)
			{
				if (IsPhase5)
				{
					CustomsOffices.MarkAsNeedingValidation();
					CustomsOfficesForDeparture.MarkAsNeedingValidation();
				}
				else
				{
					Header.CustomsOffices.MarkAsNeedingValidation();
					Header.CustomsOfficesForDeparture.MarkAsNeedingValidation();
				}
			}
		}
	}

	[RelatedBusinessObject("ForeignDestPort")]
	[List(nameof(Lookups) + "." + nameof(NctsDepartureMovementHeaderPhase4Lookups.ForeignDestPorts))]
	[ResourceStringData("9610A583-CC07-470D-821C-4D1ECFCDFFF5", Caption = "Place of Loading", ShortCaption = "Loading", MultipleKey = NctsHeader.Phase5CaptionKey)]
	[MaxLength(Schema.BM_PlaceOfLoadingMaxLength)]
	public override ZString BM_PlaceOfLoading
	{
		get => base.BM_PlaceOfLoading;
		set
		{
			var oldValue = base.BM_PlaceOfLoading;
			base.BM_PlaceOfLoading = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	[MaxLength(3)]
	public override ZString BM_SpecificCircumstance
	{
		get => base.BM_SpecificCircumstance;
		set
		{
			var oldValue = base.BM_SpecificCircumstance;
			base.BM_SpecificCircumstance = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZString BM_UniqueConsignmentReference
	{
		get => base.BM_UniqueConsignmentReference;
		set
		{
			var oldValue = base.BM_UniqueConsignmentReference;
			base.BM_UniqueConsignmentReference = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	public override ZString BM_MethodOfPayment
	{
		get => base.BM_MethodOfPayment;
		set
		{
			var oldValue = base.BM_MethodOfPayment;
			base.BM_MethodOfPayment = value;

			if (!IsCopying && oldValue != value)
			{
				Header.ModifyReleaseStatusInPredeclaration();
			}
		}
	}

	protected override void OnChangedCarrierJobDocAddressRequirement()
	{
		base.OnChangedCarrierJobDocAddressRequirement();
		Header.ModifyReleaseStatusInPredeclaration();
	}

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
		Header.MarkAsNeedingValidation();
	}

	public override void OnSaving()
	{
		base.OnSaving();
		if (IsPhase5 && BM_PaperlessInbondNum.IsEmpty)
		{
			BM_PaperlessInbondNum = Header.BH_JobReference.SubstringSafe(0, Schema.BM_PaperlessInbondNumMaxLength);
		}
	}

	protected override bool BM_PaperlessInbondNumReadOnly => base.BM_PaperlessInbondNumReadOnly && !EnableCustomerReference;

	protected override EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(Header, Factory);

	bool EnableCustomerReference => !Header.IsSent && BM_CustomsStatus.IsEmpty;

	public ZBool CustomsStatusIsPRE => BM_CustomsStatus == EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;

	[ReadOnlyMember(nameof(CustomsStatusIsPRE))]
	public override ZString BM_TypeOfSecurity
	{
		get => base.BM_TypeOfSecurity;
		set
		{
			var oldValue = BM_TypeOfSecurity;
			base.BM_TypeOfSecurity = value;
			if (!IsCopying && oldValue != BM_TypeOfSecurity)
			{
				var customsOffices = Header.IsPhase5 ? CustomsOffices : Header.CustomsOffices;
				customsOffices.MarkAsNeedingValidation();
			}
		}
	}

	protected override ZBool ShouldTotalWeightOfBillsBeRecalculated => false;

	protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

	public new NctsCusGoodsLocation GoodsLocation => (NctsCusGoodsLocation)base.GoodsLocation;

	protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderPhase5Lookups(this);

	protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderPhase4Lookups(this);

	protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation GetNewPhase4Validation() => new NctsDepartureMovementHeaderPhase4Validation(this);

	protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() =>  new NctsDepartureMovementHeaderPhase5Validation(this);

	protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

	protected override void AddTirSupportingDocIfNeeded()
	{
	}

	[MaxLength(Schema.BM_PlaceOfUnloadingMaxLength)]
	public override ZString BM_PlaceOfUnloading { get => base.BM_PlaceOfUnloading; set => base.BM_PlaceOfUnloading = value; }

	protected override void ValidateRepresentative(JobDocAddressValidation validation)
	{
		base.ValidateRepresentative(validation);
		if (IsPhase5 && IsPhaseStatusTNN && Representative.IsEmpty)
		{
			Representative.OrganisationPKInfo.AddWarning(Res.GetString("E5937D4D-CEB2-4FDB-9AB4-C82F9740C789", "If Representative is needed, it must be added in Arrival Notification/Arrival Details/ Rep. Trader. Representative cannot be used in public Arrival Goods Locations."));
		}
	}

	protected override void ValidateRepresentativeContact(JobDocAddressValidation validation)
	{
		base.ValidateRepresentativeContact(validation);

		if (IsPhase5Departure && !Representative.IsEmpty)
		{
			var contact = Representative.E2_Contact;
			if (contact.IsEmpty)
			{
				Representative.E2_ContactInfo.AddMessageError(Res.GetString("F8710576-1D58-4C49-AEA2-63B3CB3C9FD7", "Contact data are required if Representative is used."));
			}
		}
	}

	protected override bool IsRepresentativeReadOnly => IsPhaseStatusTNNAndPhase5 || CustomsStatusIsPRE;

	protected override void CusAuthorizationUsages_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = CusAuthorizationUsages.Count;
		Header.ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousAuthorizationUsagesCount, currentCount);
		previousAuthorizationUsagesCount = currentCount;
	}
	int previousAuthorizationUsagesCount;

	protected override void CusSupplyChainActors_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = CusSupplyChainActors.Count;
		Header.ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousCusSupplyChainActorsCount, currentCount);
		previousCusSupplyChainActorsCount = currentCount;
	}
	int previousCusSupplyChainActorsCount;

	protected override ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> GetCusSupplyChainActorsCore()
	{
		return new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	}

	public bool IsPhaseStatusTNNAndPhase5 => IsPhaseStatusTNN && IsPhase5;

	public ZBool IsPhaseStatusTNN => BM_Phase == ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;

	public ZBool IsCustomsStatusPRE => BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;

	public NctsHeader ArrivalHeaderForTNN => arrivalHeaderForTNN ??= GetRelevantArrivalForTNN();
	NctsHeader arrivalHeaderForTNN;

	NctsHeader GetRelevantArrivalForTNN()
	{
		NctsHeader nctsHeader = null;
		if (Header.ESNctsHeader.CEN_TNNArrival)
		{
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BM_DepartureMovement, PK);
			_ = query.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Common.EU.NctsMoveHeaderType.Codes.Arrival);

			nctsHeader = Factory.LoadTop1<NctsArrivalMovementHeader>(query)?.Header;
		}

		return nctsHeader;
	}

	[ChildEditable(true)]
	public new NctsESOfficeCodeCollection CustomsOffices => (NctsESOfficeCodeCollection)base.CustomsOffices;

	protected override NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsESOfficeCodeCollection(this);

	public new NctsESOfficeCodeCollectionForDepartureGrid CustomsOfficesForDeparture => (NctsESOfficeCodeCollectionForDepartureGrid)base.CustomsOfficesForDeparture;

	protected override NctsEuOfficeCodeCollectionForDepartureGrid GetNewCustomsOfficesForDeparture() => new NctsESOfficeCodeCollectionForDepartureGrid(this);

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		return new Dictionary<ZString, Type>
		{
			{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsESOfficeCode) },
		};
	}

	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	#region DestinationCustomsOffice
	protected override void CustomsOfficesForDeparture_ListChanged(object sender, ListChangedEventArgs e)
	{
		base.CustomsOfficesForDeparture_ListChanged(sender, e);
		var listChangedType = e.ListChangedType;
		if (listChangedType == ListChangedType.ItemChanged)
		{
			Header.Bills?.ForEach(b => b.GoodsItems?.ForEach(item => item.SetDefaultTaxType()));
		}
	}

	protected override ICustomsOffice GetDestinationCustomsOfficeForDeparture() => NctsESOfficeCode.Load<NctsESOfficeCode>(this, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

	IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices => CustomsOffices.Cast<NctsESOfficeCode>();

	#endregion

	#region Confirm/Reserve Temporary Storage Goods

	protected override ZString TemporaryStorageTransactionInternalReferenceNumberCore => BM_PaperlessInbondNum;

	protected override ZString TemporaryStorageTransactionInternalReferenceTypeCore => CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;

	protected override (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetGoodsItemDataDeclaredForDepartureToReserveTSGoodsCore()
	{
		var resultList = new List<DeclarationDataToReserveTSGoods>();

		var docAndItemList = new List<(NctsPreviousDocument doc, NctsDepartureCargoDesc item)>();

		var goodsItemsWithPrevDoc = Header.Bills.SelectMany(bill => bill.GoodsItems.Where(g => g.PreviousDocuments.Count > 0));

		foreach (var item in goodsItemsWithPrevDoc)
		{
			var doc = item.PreviousDocuments.FirstOrDefault();
			if (doc != null)
			{
				docAndItemList.Add((doc, item));
			}
		}

		var distinctDocAndItemListByDoc = docAndItemList.GroupBy(x => new { x.doc.CSI_ReferenceNumber, x.doc.CSI_ItemNumber });

		foreach (var distinctDocAndItem in distinctDocAndItemListByDoc)
		{
			var distinctDocAndItemArray = distinctDocAndItem.ToArray();
			var docToReturn = distinctDocAndItemArray.First().doc;
			var grossWeight = ZDecimal.Zero;
			var grossWeightForVINs = ZDecimal.Zero;
			var packagesAll = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

			foreach (var (doc, item) in distinctDocAndItemArray)
			{
				var grossWeightItemOrDoc = !doc.CSI_Quantity.IsEmpty && doc.CSI_UnitOfQuantity == Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram
													? doc.CSI_Quantity
													: item.GrossMassInKilograms;
				grossWeight += grossWeightItemOrDoc;
				grossWeightForVINs += item.IsVehicles ? grossWeightItemOrDoc : 0;
				packagesAll.AddRange(GetPackages(item));
			}

			var packagesGroupBy = packagesAll.GroupBy(p => new { p.type, p.marksOrVin });

			var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();
			foreach (var pack in packagesGroupBy)
			{
				packages.Add((pack.Key.type, pack.ToArray().Sum(p => p.qty), pack.Key.marksOrVin, pack.First().isBulk));
			}

			resultList.Add(new DeclarationDataToReserveTSGoods()
			{
				Document = docToReturn,
				TotalGrossWeight = grossWeight,
				TotalGrossWeightForVINs = grossWeightForVINs,
				Packages = packages
			});
		}

		return (resultList, ZString.Empty);

		IEnumerable<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> GetPackages(NctsDepartureCargoDesc item)
		{
			var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>();

			if (item.IsVehicles)
			{
				foreach (var vehiclePack in item.Packages)
				{
					packages.Add(((ZString)RefCusCodeList.PackageType.Frame, 1, vehiclePack.B5_PackageID, false));
				}
			}
			else
			{
				var packagingDetails = item.Packages.GroupBy(x => new { x.B5_UnitType, x.B5_MarksAndNumbers });
				foreach (var pack in packagingDetails)
				{
					var packType = pack.Key.B5_UnitType;
					var packqty = ZInt.Zero;
					var piecesqty = ZInt.Zero;
					foreach (var p in pack.ToArray())
					{
						packqty += p.B5_UnitCount.ToZInt();
					}
					packages.Add((packType, packqty, pack.Key.B5_MarksAndNumbers, PackageHelper.PackTypeIsBulk(packType, Factory)));
				}
			}

			return packages;
		}
	}

	protected override ZBool ShouldConfirmTemporaryStorageGoodsConsumption => !IsPhaseStatusTNN;

	protected override IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => new ZString[] { ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit,
																											ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
																											ESNCTS5DepartureCustomsStatusList.Codes.Invalidated,
																											ESNCTS5DepartureCustomsStatusList.Codes.Cancelled };

	protected override IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => new ZString[] { ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit };

	protected override IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => new ZString[] { ESNCTS5DepartureCustomsStatusList.Codes.PreLodged, ZString.Empty };

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	protected override ZString TemporaryStorageTransactionCommentPrefix => "TRANSIT JOB:";

	protected override ZDateTime ReleaseDateForTemporaryStorage => Header.ClearanceDate;

	protected override ZString PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => ES.Business.PreviousDocumentHelper.PreviousDocumentCodeN337;

	protected override ZString TemporaryStorageWriteOffTransactionCommentReferenceNumber => " / " + TemporaryStorageTransactionInternalReferenceType;

	protected override ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => true;

	protected override ZString GetDocumentNumberFormat(ZString dsdtMRN) => DocumentHelper.GetDsdtMRNNumberFormat(dsdtMRN);

	#endregion
}
