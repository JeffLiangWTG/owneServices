using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc
	, Integration.Customs.IT.IDepartureCargoDesc
	, EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentsProvider
	, IPackageProvider
	, IMergedPreviousDocumentsProvider
	, INBWrappableBusinessObject
	, IDocAddresses
{
	public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public INctsDepartureCargoDescLookups ITLookups => (INctsDepartureCargoDescLookups)base.Lookups;

	public new EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
	protected override EU.NCTS.Business.INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments()
		=> new EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>(this);
	protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);

	public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
	protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);
	protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

	public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection()
	{
		const int collectionMaxCount = 1;
		var additionalInfoCollection = new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
		if (!IsPhase5Departure)
		{
			additionalInfoCollection.EnableMaxCountValidation(collectionMaxCount, warnAtHalfway: false, CargoWise.ComponentModel.NotificationType.Error, ValidationCaptions.AdditionalInfo.OnlyOneLineOfAdditionalInfoIsAllowed);
		}

		return additionalInfoCollection;
	}
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	public override void OnSaving()
	{
		if (Header != null)
		{
			Header.ShouldResetGoodsItemNumbers = false;
		}
		base.OnSaving();
	}

	public override void Delete()
	{
		Header.ShouldResetGoodsItemNumbers = false;
		base.Delete();
	}

	public override ZGuid BY_ParentID
	{
		get => base.BY_ParentID;
		set
		{
			var oldValue = BY_ParentID;
			base.BY_ParentID = value;
			if (!IsCopying && oldValue != BY_ParentID)
			{
				ResetAeoCertificateManager();
				Fees.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString BY_ParentTableCode
	{
		get => base.BY_ParentTableCode;
		set
		{
			var oldValue = BY_ParentTableCode;
			base.BY_ParentTableCode = value;
			if (!IsCopying && oldValue != BY_ParentTableCode)
			{
				Fees.MarkAsNeedingValidation();
			}
		}
	}

	[ChildEditable(true)]
	public new NctsCargoDescFeeCollection Fees => (NctsCargoDescFeeCollection)base.Fees;

	protected override ICusInBondFeeCollection<EU.NCTS.Business.NctsCargoDescFee> GetNctsCargoDescFeeCollection() => new NctsCargoDescFeeCollection(this);

	protected override EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescPhase5Validation(this);

	protected override EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation GetNewPhase4Validation() => new NctsDepartureCargoDescPhase4Validation(this);

	protected override EU.NCTS.Business.NctsCommonCargoDescLookups GetNewPhase4Lookups() => new NctsDepartureCargoDescPhase4Lookups(this);

	protected override EU.NCTS.Business.NctsCommonCargoDescLookups GetNewPhase5Lookups() => new NctsDepartureCargoDescPhase5Lookups(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[ITCusSupportingInfoTypeList.Codes.Remarks] = typeof(CusSupportingInfo);
		return result;
	}

	public new NctsDepartureMovementHeader MoveHeader => (NctsDepartureMovementHeader)base.MoveHeader;

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsBill Bill => (NctsBill)base.Bill;

	public AeoCertificateManager AeoCertificateManager => aeoCertificatesManager ?? (aeoCertificatesManager = new AeoCertificateManager(Header, this));
	AeoCertificateManager aeoCertificatesManager;

	void ResetAeoCertificateManager() => aeoCertificatesManager = null;

	public ZBool OriginStateReadOnly => BY_RN_NKCountryOfOrigin != Core.Constants.CountryCodes.Italy;

	public void ClearOriginStateIfRequired()
	{
		if (BY_RN_NKCountryOfOrigin != Core.Constants.CountryCodes.Italy)
		{
			BY_RW_NKOriginState = ZString.Empty;
		}
	}

	public override ZString BY_RN_NKCountryOfOrigin
	{
		get => base.BY_RN_NKCountryOfOrigin;
		set
		{
			var oldValue = BY_RN_NKCountryOfOrigin;
			base.BY_RN_NKCountryOfOrigin = value;
			if (!IsCopying && oldValue != BY_RN_NKCountryOfOrigin)
			{
				ClearOriginStateIfRequired();
			}
		}
	}

	[ReadOnlyMember(nameof(OriginStateReadOnly))]
	[List(nameof(ITLookups) + "." + nameof(INctsDepartureCargoDescLookups.OriginStates))]
	public override ZString BY_RW_NKOriginState
	{
		get => base.BY_RW_NKOriginState;
		set => base.BY_RW_NKOriginState = value;
	}

	EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentsProvider.SupportingDocuments => SupportingDocuments;

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureCargoDescLookups.CPCList))]
	public override ZString BY_Procedure { get => base.BY_Procedure; set => base.BY_Procedure = value; }

	protected override void ValidateConsignor(JobDocAddressValidation validation)
	{
		base.ValidateConsignor(validation);

		new IT.Business.Declaration.TraderJobDocAddressValidation(Consignor, ValidationCaptions.Shared.ConsignorCaption, MoveHeader)
			.ValidateRequiredCustomsCode();

		TraderValidator.ValidateConsignor();
	}

	protected override void ValidateConsignee(JobDocAddressValidation validation)
	{
		base.ValidateConsignee(validation);

		TraderValidator.ValidateConsignee();
	}

	NctsDepartureCargoDescTraderValidator TraderValidator => traderValidator ?? (traderValidator = new NctsDepartureCargoDescTraderValidator(this));
	NctsDepartureCargoDescTraderValidator traderValidator;

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureCargoDescLookups.StatusList))]
	[ReadOnly(true)]
	[ResourceStringData("CD1EA534-4705-40F1-B453-9E716767C2F8", Caption = "Status", FullDescription = "Goods item status")]
	public override ZString BY_Status { get => base.BY_Status; set => base.BY_Status = value; }

	public ZDecimal TotalTaxedAmount => Fees.GetTotalAmount();

	public RefCusProcedure CusProcedure
	{
		get
		{
			var procedure = BY_Procedure;
			if (procedure.IsEmpty)
			{
				cusProcedure = null;
			}
			else
			{
				if (cusProcedure == null || cusProcedure.FullCodeCurrentPlusPreviousPlusConcession != procedure)
				{
					var procedureCode = procedure.Left(2);
					var prevProcedure = procedure.SubstringSafe(2, 2);
					var concession = procedure.SubstringSafe(4, 3);
					cusProcedure = new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(procedureCode, prevProcedure, Header.CountryCode, ZDateTime.Today, concession: concession);
				}
			}
			return cusProcedure;
		}
	}
	RefCusProcedure cusProcedure;

	#region IPackageProvider Members

	public NctsPackage Package => Packages.Cast<NctsPackage>().FirstOrDefault();

	ZInt IPackageProvider.NumberOfPackages => Package?.B5_UnitCount.ToZInt() ?? ZInt.Zero;

	ZString IPackageProvider.PackageType => Package?.B5_UnitType ?? ZString.Empty;

	ZString IPackageProvider.MarksAndNumbers => Package?.B5_MarksAndNumbers ?? ZString.Empty;

	#endregion

	public GroupedPreviousDocumentCollection GroupedPreviousDocuments
	{
		get
		{
			if (groupedPreviousDocuments == null)
			{
				groupedPreviousDocuments = new CachedProperty<GroupedPreviousDocumentCollection>(Factory, () => IT.Business.GroupedPreviousDocumentCollection.LoadNew(this, Factory));
			}
			return groupedPreviousDocuments.Value;
		}
	}
	CachedProperty<GroupedPreviousDocumentCollection> groupedPreviousDocuments;

	#region IMergedPreviousDocumentsProvider Members

	ZString IMergedPreviousDocumentsProvider.NBStatus => BY_Status;

	IEnumerable<IMergedPreviousDocument> IMergedPreviousDocumentsProvider.MergedPreviousDocuments => PreviousDocuments.Cast<IMergedPreviousDocument>();

	ZInt IMergedPreviousDocumentsProvider.LineNumber => BY_LineNo;

	ZBool IMergedPreviousDocumentsProvider.IsExport => IsExport;

	#endregion

	#region INBWrappableBusinessObject Members

	public ZInt LineNumber => BY_LineNo;

	public ZBool IsImport => false;

	public ZBool IsExport => true;

	public IEnumerable<GroupedPreviousDocument> NBGroupedPreviousDocuments => new TypedEnumerable<GroupedPreviousDocument>(GroupedPreviousDocuments);

	public IT.Business.Declaration.RegCusEntryNumberWrapper EntryNumberWrapper => Header.EntryNumbersProvider?.RegistrationInfoWrapper;

	#endregion

	NctsDepartureCargoDescAcrManager NctsDepartureCargoDescAcrManager => nctsDepartureCargoDescAcrManager ?? (nctsDepartureCargoDescAcrManager = new NctsDepartureCargoDescAcrManager(this));
	NctsDepartureCargoDescAcrManager nctsDepartureCargoDescAcrManager;

	#region IDocAddresses Members

	void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
	{
		if (docAddress.DocAddressType == MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress)
		{
			NctsDepartureCargoDescAcrManager.AddSupportingDocumentIfNeeded();
		}
	}
	#endregion

	public override ZString BY_Description
	{
		get => base.BY_Description;
		set
		{
			var oldValue = BY_Description;
			base.BY_Description = value;
			if (!IsCopying && oldValue != BY_Description)
			{
				NctsDepartureCargoDescAcrManager.AddSupportingDocumentIfNeeded();
			}
		}
	}

	public override ZString BY_HarmonisedTariff
	{
		get => base.BY_HarmonisedTariff;
		set
		{
			var oldValue = BY_HarmonisedTariff;
			base.BY_HarmonisedTariff = value;
			if (!IsCopying && oldValue != BY_HarmonisedTariff)
			{
				NctsDepartureCargoDescAcrManager.AddSupportingDocumentIfNeeded();
			}
		}
	}

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureCargoDescLookups.PortTaxRateList))]
	[ResourceStringData("F785A29D-05E8-4257-926C-46B975C8A6B3", Caption = "Port Tax Rate")]
	[ReadOnlyMember(nameof(CommodityCodeReadOnly))]
	public override ZString BY_CommodityCode
	{
		get => base.BY_CommodityCode;
		set => base.BY_CommodityCode = value;
	}

	public ZBool CommodityCodeReadOnly => !ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.Value;

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		args.AddExcludedColumns(new string[] { Schema.BY_Status });
		var newGoodsItem = (NctsDepartureCargoDesc)base.CloneInternal(args);
		newGoodsItem.Remarks = Remarks;
		return newGoodsItem;
	}

	[ResourceStringData("97605025-8AA3-43F1-8C42-B4D0F0750E17", Caption = "Description")]
	[MaxLength(CusSupportingInfo.Schema.CSI_DescriptionMaxLength)]
	public ZString Remarks
	{
		get => NctsDepartureCargoDescRemark.Remarks;
		set => NctsDepartureCargoDescRemark.Remarks = value;
	}

	NctsDepartureCargoDescRemark NctsDepartureCargoDescRemark => nctsDepartureCargoDescRemark ?? (nctsDepartureCargoDescRemark = new NctsDepartureCargoDescRemark(this));
	NctsDepartureCargoDescRemark nctsDepartureCargoDescRemark;

	public ZString ActualCountryOfDispatch => BY_RN_NKCountryOfDispatch.FallbackTo(Header?.BH_RL_NKImportLoadPort ?? ZString.Empty);

	public IAttachmentPrintingSupporter AttachmentPrintingSupporter
	{
		get
		{
			if (attachmentPrintingSupporter == null)
			{
				attachmentPrintingSupporter = new CachedProperty<IAttachmentPrintingSupporter>(Factory, () => new NctsDepartureCargoDescAttachmentPrintingSupporter(this));
			}
			return attachmentPrintingSupporter.Value;
		}
	}
	CachedProperty<IAttachmentPrintingSupporter> attachmentPrintingSupporter;

	public void ClearCustomsDeletionStatus()
	{
		BY_Status = ZString.Empty;
	}

	public void SetAsCustomsDeletionRequest()
	{
		BY_Status = NctsDeletionStatusList.Codes.DeletionRequest;
	}

	public bool IsCustomsStatusDeleted => BY_Status == NctsDeletionStatusList.Codes.Deleted;

	public bool IsCustomsStatusDeletionRequested => BY_Status == NctsDeletionStatusList.Codes.DeletionRequest;

	bool IsPhaseInAmendment => Header.MovementHeader.BM_Phase == EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Amendment;

	#region ICanDelete

	public override bool CanDelete
	{
		get { return base.CanDelete && !IsPhaseInAmendment; }
	}

	public override MultilingualString ReasonForNotAbleToDelete
	{
		get
		{
			if (IsPhaseInAmendment)
			{
				return ResString.GetMultilingualString("063E262B-46E1-4AA4-B6F7-13CA80FDA128",
					"It is not possible to delete a Goods Item in Amendment phase. You can request a deletion of the Goods Item setting its Status to DLR (deletion request)");
			}
			return base.ReasonForNotAbleToDelete;
		}
	}

	#endregion
}
