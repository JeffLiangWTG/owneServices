using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
	, Integration.Customs.CH.IArrivalMovementHeader
	, ICusGoodsLocationProvider
{
	public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : NctsCommonMovementHeader.Schema
	{
		public const string MultipleMRNIndicator = nameof(NctsArrivalMovementHeader.MultipleMRNIndicator);
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		MultipleMRNIndicator = true;
		BM_ArrivalDate = ZDateTime.Now;
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	[ChildEditable]
	public new CusInBondMoveDetailCollection MovementDetails => (CusInBondMoveDetailCollection)base.MovementDetails;

	protected override ICusInBondMoveDetailCollection CreateMovementDetails() => new CusInBondMoveDetailCollection(this);

	protected override Type MovementDetailTypeCore => typeof(CusInBondMoveDetail);

	protected override ZBool ShouldConsiderDIFUnloadedStateForPackageCount => true;

	[ResourceStringData("CH.NctsArrivalMovementHeader.BM_TransportAtArrivalType", Caption = "Type of ID", FullDescription = "Identification Type of the Transport Means")]
	[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.TransportAtArrivalTypeList))]
	[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
	public override ZString BM_TransportAtArrivalType { get => base.BM_TransportAtArrivalType; set => base.BM_TransportAtArrivalType = value; }

	[ResourceStringData("CH.NctsArrivalMovementHeader.BM_TransportAtArrivalID", Caption = "Transport ID", FullDescription = "Identification of the Transport Means")]
	[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
	public override ZString BM_TransportAtArrivalID { get => base.BM_TransportAtArrivalID; set => base.BM_TransportAtArrivalID = value; }

	[ResourceStringData("CH.NctsArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality", Caption = "Nationality", FullDescription = "Nationality of the Transport Means")]
	[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.NationalityList))]
	[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
	public override ZString BM_RN_NKTransportAtArrivalIDNationality { get => base.BM_RN_NKTransportAtArrivalIDNationality; set => base.BM_RN_NKTransportAtArrivalIDNationality = value; }

	public override ZBool BM_NoChangesToReport
	{
		get => base.BM_NoChangesToReport;
		set
		{
			var oldValue = BM_NoChangesToReport;
			base.BM_NoChangesToReport = value;
			if (!IsCopying && oldValue != value)
			{
				Header?.ArrivalHeaderContainers.MarkAsNeedingValidation();
			}
		}
	}

	public new NctsArrivalMovementHeaderLookups Lookups => (NctsArrivalMovementHeaderLookups)base.Lookups;

	protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsArrivalMovementHeaderLookups(this);

	public new NctsArrivalMovementHeaderValidation Validation => (NctsArrivalMovementHeaderValidation)base.Validation;

	protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);

	protected override bool ShouldGenerateLocalReferenceNumberOnFactorySavingCore => true;

	protected override ZString GenerateLocalReferenceNumberCore()
	{
		if (ShouldGenerateLrnNumberByAuthorizedLocationCode)
		{
			return ArrivalCustomerReferenceGeneratorHelper.GenerateArrivalCustomerReferenceNumber(Factory, AuthorisationLocationCode);
		}
		else
		{
			return CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, LrnNumberFountain, Header.DestinationTrader.Organisation);
		}
	}

	protected override bool ForceRegenerateLocalReferenceNumber => ShouldGenerateLrnNumberByAuthorizedLocationCode && AuthorisationLocationCode != (ZString)GoodsLocation?.Address?.AuthorisationNumberInfo.OriginalValue;

	public ZString AuthorisationLocationCode => GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty;

	public bool ShouldGenerateLrnNumberByAuthorizedLocationCode => !CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.Value.UseSystemDefinedFormat;

	protected override INumberFountainProxy LrnNumberFountain => Env.NumberFountains.CHLocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForArrivalCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

	protected override IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> GetNewArrivalTransportInfos()
	{
		var arrivalTransportInfos = base.GetNewArrivalTransportInfos();
		arrivalTransportInfos.SetReadOnlyIncludingChildren(true);
		return arrivalTransportInfos;
	}

	protected override INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	#region Properties

	public ZBool MultipleMRNIndicator
	{
		get
		{
			return this.GetSystemDefinedValue<ZBool>(GenAddOnHelper.MultipleMRNIndicator);
		}
		set
		{
			var oldValue = MultipleMRNIndicator;
			this.SetSystemDefinedValue(GenAddOnHelper.MultipleMRNIndicator, value);
			MultipleMRNIndicatorInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo MultipleMRNIndicatorInfo => GetZPropertyInfo(Schema.MultipleMRNIndicator);

	[ResourceStringData("CH.NctsArrivalMovementHeader.BM_StateOfSeals", Caption = "Seals State Valid")]
	[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.YesNoList))]
	[MaxLength(1)]
	[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
	public override ZString BM_StateOfSeals
	{
		get => base.BM_StateOfSeals;
		set => base.BM_StateOfSeals = value;
	}

	[ResourceStringData("CH.NctsArrivalMovementHeader.BM_AdditionalText", Caption = "Additional Text")]
	[ReadOnlyMember(nameof(IsArrivalNotificationDisabled))]
	public override ZString BM_AdditionalText
	{
		get => base.BM_AdditionalText;
		set => base.BM_AdditionalText = value;
	}

	#endregion

	[ChildEditable(true)]
	public MovementReferenceNumberSupportingInfoCollection MovementReferenceNumbers
	{
		get
		{
			if (movementReferenceNumbers == null)
			{
				movementReferenceNumbers = new MovementReferenceNumberSupportingInfoCollection(this);
				movementReferenceNumbers.Load();
				RegisterEditableChildObject(movementReferenceNumbers);
			}
			return movementReferenceNumbers;
		}
	}
	MovementReferenceNumberSupportingInfoCollection movementReferenceNumbers;

	internal HugeSequenceNumberGenerator MovementReferenceNumberLineNumberGenerator => movementReferenceNumberLineNumberGenerator ?? (movementReferenceNumberLineNumberGenerator = new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(MovementReferenceNumbers)));
	HugeSequenceNumberGenerator movementReferenceNumberLineNumberGenerator;

	[ChildEditable(true)]
	public SupernumeraryGoodsCollection SupernumeraryGoods
	{
		get
		{
			if (supernumeraryGoods == null)
			{
				supernumeraryGoods = new SupernumeraryGoodsCollection(this);
				supernumeraryGoods.Load();
				RegisterEditableChildObject(supernumeraryGoods);
			}
			return supernumeraryGoods;
		}
	}
	SupernumeraryGoodsCollection supernumeraryGoods;

	internal HugeSequenceNumberGenerator SupernumeraryGoodsLineNumberGenerator => supernumeraryGoodsLineNumberGenerator ?? (supernumeraryGoodsLineNumberGenerator = new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(SupernumeraryGoods)));
	HugeSequenceNumberGenerator supernumeraryGoodsLineNumberGenerator;

	[ChildEditable(true)]
	public AdditionalTransitOperationCollection AdditionalTransitOperations
	{
		get
		{
			if (additionalTransitOperations == null)
			{
				additionalTransitOperations = new AdditionalTransitOperationCollection(this);
				additionalTransitOperations.Load();
				RegisterEditableChildObject(additionalTransitOperations);
			}
			return additionalTransitOperations;
		}
	}
	AdditionalTransitOperationCollection additionalTransitOperations;

	internal HugeSequenceNumberGenerator AdditionalTransitOperationLineNumberGenerator => additionalTransitOperationLineNumberGenerator ?? (additionalTransitOperationLineNumberGenerator = new HugeSequenceNumberGenerator(() => AdditionalTransitOperations));
	HugeSequenceNumberGenerator additionalTransitOperationLineNumberGenerator;

	[ChildEditable(true)]
	public RelatedArrivalMovementGenPivotCollection RelatedArrivalMovements
	{
		get
		{
			if (relatedArrivalMovements == null)
			{
				relatedArrivalMovements = new RelatedArrivalMovementGenPivotCollection(this);
				relatedArrivalMovements.Load();
				RegisterEditableChildObject(relatedArrivalMovements);
			}

			return relatedArrivalMovements;
		}
	}
	RelatedArrivalMovementGenPivotCollection relatedArrivalMovements;

	#region Carrier

	public JobDocAddress Carrier
	{
		get
		{
			if (carrierJobDocAddress == null || carrierJobDocAddress.IsDeleted)
			{
				carrierJobDocAddress = DocAddresses.FindOrCreateWithRequirement(CarrierJobDocAddressRequirement);
			}
			return carrierJobDocAddress;
		}
	}
	JobDocAddress carrierJobDocAddress;

	public JobDocAddressRequirement CarrierJobDocAddressRequirement
	{
		get
		{
			if (carrierJobDocAddressRequirement == null)
			{
				carrierJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Carrier);
				carrierJobDocAddressRequirement.ValidateCompanyName = noValidation;
				carrierJobDocAddressRequirement.ValidateAddress1 = noValidation;
				carrierJobDocAddressRequirement.ValidateAddress2 = noValidation;
				carrierJobDocAddressRequirement.ValidateCity = noValidation;
				carrierJobDocAddressRequirement.ValidatePostCode = noValidation;
				carrierJobDocAddressRequirement.ValidateState = noValidation;
				carrierJobDocAddressRequirement.ValidateCountry = noValidation;
				JobDocAddressManager.AddRequirement(carrierJobDocAddressRequirement);
			}
			return carrierJobDocAddressRequirement;
		}
	}
	JobDocAddressRequirement carrierJobDocAddressRequirement;

	void noValidation(JobDocAddressValidation validation)
	{
	}

	#endregion

	#region IDocAddresses Memebers

	protected override JobDocAddressRequirement IDocAddressesGetDocAddressRequirementCore(DocAddressType addressType)
	{
		switch (addressType)
		{
			case DocAddressType.Carrier:
				return CarrierJobDocAddressRequirement;
			default:
				return base.IDocAddressesGetDocAddressRequirementCore(addressType);
		}
	}

	protected override IReadOnlyList<DocAddressType> IDocAddressesSupportedAddressTypesCore => base.IDocAddressesSupportedAddressTypesCore.Append(DocAddressType.Carrier).ToArray();

	#endregion

	public NctsArrivalMovementHeader MasterArrivalMovementHeader => masterArrivalMovementHeader ?? (masterArrivalMovementHeader = GetMasterArrivalMovementHeader());
	NctsArrivalMovementHeader masterArrivalMovementHeader;

	NctsArrivalMovementHeader GetMasterArrivalMovementHeader()
	{
		return MultipleMRNIndicator ? null : (NctsArrivalMovementHeader)GenPivot.LoadRelation2Pivot(this, GenPivotTypeDecider.Types.NctsRelatedArrivalGenPivot)?.Relation1Object;
	}

	protected override void SetReadOnlyForUnloadingDifferencesDataCore(bool readOnly)
	{
		base.SetReadOnlyForUnloadingDifferencesDataCore(readOnly);

		if (Header is NctsHeader header)
		{
			header.ArrivalHeaderContainers.SetReadOnlyIncludingChildren(readOnly);

			if (!BM_NoChangesToReport)
			{
				foreach (var bill in header.Bills)
				{
					bill.AdditionalDocuments.SetReadOnlyIncludingChildren(true);
					bill.PreviousDocuments.SetReadOnlyIncludingChildren(true);
					bill.SupportingDocuments.SetReadOnlyIncludingChildren(true);
					bill.ArrivalTransportInfos.SetReadOnlyIncludingChildren(true);

					foreach (var goodsItem in bill.ArrivalGoodsItems)
					{
						goodsItem.AdditionalInfos.SetReadOnlyIncludingChildren(true);
						goodsItem.PreviousDocuments.SetReadOnlyIncludingChildren(true);
						goodsItem.SupportingDocuments.SetReadOnlyIncludingChildren(true);
					}
				}
			}
			header.RefreshBindingIncludingChildren();
		}
	}

	public override void Delete()
	{
		using (SupernumeraryGoodsLineNumberGenerator.GetLineNumberSuspender())
		using (MovementReferenceNumberLineNumberGenerator.GetLineNumberSuspender())
		using (AdditionalTransitOperationLineNumberGenerator.GetLineNumberSuspender())
		{
			base.Delete();
		}
	}

	protected override void OnFactorySaving()
	{
		if (!MultipleMRNIndicator)
		{
			MovementReferenceNumbers.RemoveAndDeleteAll();
		}

		base.OnFactorySaving();
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.CH.CusSupportingInfoTypeList.Codes.SupernumeraryGoods] = typeof(SupernumeraryGoods);
		result[Common.CH.CusSupportingInfoTypeList.Codes.MovementReferenceNumber] = typeof(MovementReferenceNumberSupportingInfo);
		result[Common.CH.CusSupportingInfoTypeList.Codes.AdditionalTransitOperation] = typeof(AdditionalTransitOperation);
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(NctsAdditionalInfo);
		return result;
	}

	protected override CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new NctsMovementHeaderCustomsOfficeRequirementHelper(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	public ZDateTime ValuationDate => BM_ValuationDate.IsValid ? BM_ValuationDate : ZDateTime.Today;

	public bool IsAdditionalGoodsInformationLocked => IsArrivalNotificationDisabled && CanLockUnlockAGO;

	public bool IsClosedRelease => BM_CustomsStatus.ToString() is NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease or NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;

	public bool CanLockUnlockAGO => Factory.GetValue(ref canLockUnlockAGO, () => CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(EUJobMessageTypeList.Codes.NctsArrivalNotification)?.TabInfos.Cast<DeclarationTabLockInfo>()
		.Any(x => x.TabPage.ToString() is DeclarationTabPages.Codes.NctsArrivalAdditionalGoodsInformation or DeclarationTabPages.Codes.All) ?? false);
	CachedProperty<bool> canLockUnlockAGO;

	protected override bool DestinationCustomsOfficeCodeForArrivalReadOnlyCore => true;
}
