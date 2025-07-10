using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

[SystemDefinedValues]
public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
	, Integration.Customs.IT.IDepartureMovementHeader
	, IElectronicFolderSupporter
	, IUCC6AndTransitionPeriodProvider
	, ICusGoodsLocationProvider
	, ICustomsLinkedObjectAdapterProvider
	, ITopLevelBusinessObjectProvider
{
	public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsDepartureMovementHeader.Schema
	{
		public const string UseElectronicFolder = "UseElectronicFolder";
		public const string PaymentParty = "PaymentParty";
		public const int PaymentPartyMaxLength = 1;
		public const string DefermentAccountNumber = "DefermentAccountNumber";
		public const int DefermentAccountNumberMaxLength = 35;
		public new const int BM_LocationOfGoodsCodeMaxLength = 7;
		public const string ParticipantType = "ParticipantType";
		public const int ParticipantTypeMaxLength = 3;
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsDepartureCargoDescCollection GoodsItems => (NctsDepartureCargoDescCollection)base.GoodsItems;

	protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection(this);

	public INctsDepartureMovementHeaderLookups ITLookups => (INctsDepartureMovementHeaderLookups)Lookups;

	protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderPhase5Lookups(this);

	protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderPhase4Lookups(this);

	protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation GetNewPhase4Validation() => new NctsDepartureMovementHeaderPhase4Validation(this);

	protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderPhase5Validation(this);

	protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

	public new NctsDeparturePayInfoCollection PayInfoCollection => (NctsDeparturePayInfoCollection)base.PayInfoCollection;

	protected override EU.NCTS.Business.NctsDeparturePayInfoCollection GetNewPayInfoCollection() => new NctsDeparturePayInfoCollection(this);

	protected override Type PayInfoTypeCore => typeof(NctsDeparturePayInfo);

	public new IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> AdditionalTransportAtBorderList => (DepartureCusTransportMeansCollection<DepartureCusTransportMeans>)base.AdditionalTransportAtBorderList;

	protected override IDepartureCusTransportMeansCollection<EU.NCTS.Business.DepartureCusTransportMeans> GetNewCusTransportMeansCollection() => new DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(this);

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

	public new ITEDIMessageCollection Messages => (ITEDIMessageCollection)base.Messages;
	protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new ITEDIMessageCollection(this);

	public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
	protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);
	protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

	#region UseElectronicFolder

	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader|UseElectronicFolder", Caption = "Electronic Folder")]
	public ZBool UseElectronicFolder
	{
		get => this.GetSystemDefinedValue<ZBool>(Schema.UseElectronicFolder);
		set
		{
			var oldValue = UseElectronicFolder;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(Schema.UseElectronicFolder, value);
				UseElectronicFolderInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo UseElectronicFolderInfo => GetZPropertyInfo(Schema.UseElectronicFolder);

	#endregion

	#region BM_AdditionalText

	public override ZString BM_AdditionalText
	{
		get => base.BM_AdditionalText;
		set
		{
			var oldValue = BM_AdditionalText;
			base.BM_AdditionalText = value;
			if (!IsCopying && oldValue != BM_AdditionalText)
			{
				GoodsItems?.MarkAsNeedingValidationIncludingChildren();
			}
		}
	}

	#endregion

	#region Representative

	protected override ZValidation GetRepresentativeJobDocAddressAdditionalValidation(JobDocAddress representativeJobDocAddress) => new RepresentativeJobDocAddressValidation(Representative, this);

	#endregion

	#region BM_InBondEntryType

	public override ZString BM_InBondEntryType
	{
		get => base.BM_InBondEntryType;
		set
		{
			var oldValue = BM_InBondEntryType;
			base.BM_InBondEntryType = value;
			if (!IsCopying && oldValue != BM_InBondEntryType)
			{
				if (IsTIRDeclaration)
				{
					BM_ExportDate = ZDateTime.Empty;
				}
				MarkRelatedItemsAsNeedingValidationIfValidationIsSuspended();
				RefreshRepresentative();
				Header.EmptyAuthorizationIfNecessary();
				MarkBillsAsNeedingValidation();
			}
		}
	}

	#endregion

	#region BM_InlandTransportMode

	public override ZString BM_InlandTransportMode
	{
		get => base.BM_InlandTransportMode;
		set
		{
			var oldValue = BM_InlandTransportMode;
			base.BM_InlandTransportMode = value;
			if (!IsCopying && oldValue != BM_InlandTransportMode)
			{
				MarkBillsAsNeedingValidation();
			}
		}
	}

	#endregion

	#region PaymentParty

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureMovementHeaderLookups.PaymentPartyList))]
	[MaxLength(Schema.PaymentPartyMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader|PaymentParty", Caption = "Payment Party")]
	public ZString PaymentParty
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.PaymentParty);
		set
		{
			var oldValue = PaymentParty;
			if (oldValue != value)
			{
				CheckMaximumLength(PaymentPartyInfo, value);
				this.SetSystemDefinedValue(Schema.PaymentParty, value);
				PaymentPartyInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.RunActionIfValidationOfType<NctsDepartureMovementHeaderPhase4Validation>((validation) => validation.ValidatePaymentParty());
				}
				ResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound();
			}
		}
	}

	public ZPropertyInfo PaymentPartyInfo => GetZPropertyInfo(Schema.PaymentParty);

	public void ResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound()
	{
		var defermentApprovalNumberList = ITLookups.DefermentApprovalNumberList;
		if (defermentApprovalNumberList.Count == 1)
		{
			DefermentAccountNumber = defermentApprovalNumberList[0].Code;
		}
		else if (!DefermentAccountNumber.IsEmpty)
		{
			DefermentAccountNumber = ZString.Empty;
		}
	}

	#endregion

	#region DefermentAccountNumber

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureMovementHeaderLookups.DefermentApprovalNumberList))]
	[MaxLength(Schema.DefermentAccountNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader|DefermentAccountNumber", Caption = "Approval Defer No.")]
	public ZString DefermentAccountNumber
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.DefermentAccountNumber);
		set
		{
			var oldValue = DefermentAccountNumber;
			if (oldValue != value)
			{
				CheckMaximumLength(DefermentAccountNumberInfo, value);
				this.SetSystemDefinedValue(Schema.DefermentAccountNumber, value);
				DefermentAccountNumberInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.RunActionIfValidationOfType<NctsDepartureMovementHeaderPhase4Validation>((validation) => validation.ValidateDefermentAccountNumber());
				}
			}
		}
	}

	public ZPropertyInfo DefermentAccountNumberInfo => GetZPropertyInfo(Schema.DefermentAccountNumber);

	#endregion

	#region BM_LocationOfGoodsCode

	[MaxLength(Schema.BM_LocationOfGoodsCodeMaxLength)]
	[ResourceStringData("f772dadc-23f5-4779-8378-e3dd87b3f5c6", Caption = "[30] Goods Location", MultipleKey = EU.NCTS.Business.NctsHeader.Phase4CaptionKey)]
	public override ZString BM_LocationOfGoodsCode { get => base.BM_LocationOfGoodsCode; set => base.BM_LocationOfGoodsCode = value; }

	#endregion

	#region PRE-LODGEMENT (not applicable for Italy)

	protected override ZBool GetPreLodgedForAgreedLocationOfGoodsCode() => false;

	protected override void SetPreLodgedForAgreedLocationOfGoodsCode(bool value)
	{
	}

	#endregion

	#region BM_OA_WarehouseAddress

	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader|BM_OA_WarehouseAddress", Caption = "[49] Customs Warehouse")]
	public override ZGuid BM_OA_WarehouseAddress { get => base.BM_OA_WarehouseAddress; set => base.BM_OA_WarehouseAddress = value; }

	public ZString WarehouseCode
	{
		get
		{
			var warehouseAddress = WarehouseAddress;
			return warehouseAddress?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, warehouseAddress.OA_RN_NKCountryCode) ?? ZString.Empty;
		}
	}

	protected override ZAddress GetNewBM_OA_WarehouseAddress_ZAddress()
	{
		var zAddress = base.GetNewBM_OA_WarehouseAddress_ZAddress();
		zAddress.GetDefaultAddress = GetDefaultWarehouseAddress;
		return zAddress;
	}

	ZGuid GetDefaultWarehouseAddress(IOrgHeader orgHeader)
	{
		if (orgHeader is OrgHeader organisation)
		{
			var addresses = organisation.Addresses;
			if (addresses.Count == 1)
			{
				return addresses[0].PK;
			}
		}

		return ZGuid.Empty;
	}

	#endregion

	#region BM_ControlChannel

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureMovementHeaderLookups.CustomsChannelCodeList))]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader|BM_ControlChannel", Caption = "Control Channel")]
	[ReadOnly(true)]
	public override ZString BM_ControlChannel { get => base.BM_ControlChannel; set => base.BM_ControlChannel = value; }

	#endregion

	#region BM_ExportDate

	public override ZDateTime BM_ExportDate
	{
		get => base.BM_ExportDate;
		set
		{
			base.BM_ExportDate = value.Date;
			BM_ExportDateInfo.RefreshBinding();
		}
	}

	#endregion

	#region ParticipantType

	[List(nameof(ITLookups) + "." + nameof(INctsDepartureMovementHeaderLookups.NctsParticipantTypeList))]
	[MaxLength(Schema.ParticipantTypeMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsDepartureMovementHeader|ParticipantType", Caption = "Participants")]
	public ZString ParticipantType
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.ParticipantType);
		set
		{
			var oldValue = ParticipantType;
			if (oldValue != value)
			{
				CheckMaximumLength(ParticipantTypeInfo, value);
				this.SetSystemDefinedValue(Schema.ParticipantType, value);
				ParticipantTypeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.RunActionIfValidationOfType<NctsDepartureMovementHeaderPhase4Validation>((validation) => validation.ValidateParticipantType());
				}
			}
		}
	}

	public ZPropertyInfo ParticipantTypeInfo => GetZPropertyInfo(Schema.ParticipantType);

	#endregion

	#region BM_PlaceOfUnloading

	[MaxLength(Schema.BM_PlaceOfUnloadingMaxLength)]
	public override ZString BM_PlaceOfUnloading { get => base.BM_PlaceOfUnloading; set => base.BM_PlaceOfUnloading = value; }

	#endregion

	#region BM_ForeignDestPortKCode

	[ResourceStringData("574E8B14-4B84-4687-9930-F7F567EB941D", Caption = "Place of Unloading")]
	public override ZString BM_ForeignDestPortKCode { get => base.BM_ForeignDestPortKCode; set => base.BM_ForeignDestPortKCode = value; }

	#endregion

	#region BM_EntryDate

	public override ZDateTime BM_EntryDate
	{
		get => base.BM_EntryDate;
		set
		{
			var oldValue = BM_EntryDate;
			base.BM_EntryDate = value;
			if (!IsCopying && oldValue != BM_EntryDate)
			{
				var header = Header;
				header.CountriesOfRouting.MarkAsNeedingValidation();
				header.DepartureHeaderContainers.MarkAsNeedingValidation();
				GoodsItems.MarkAsNeedingValidation();
			}

			BM_EntryDateInfo.RefreshBinding();
		}
	}

	#endregion

	#region TirCarnetNumber

	protected override ZString TIRCarnetSupportingDocumentCode => IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.TIRCarnet;

	#endregion

	#region BM_PlaceOfLoading

	[List(nameof(Lookups) + "." + nameof(EU.NCTS.Business.INctsDepartureMovementHeaderLookups.ForeignDestPorts))]
	public override ZString BM_PlaceOfLoading { get => base.BM_PlaceOfLoading; set => base.BM_PlaceOfLoading = value; }

	protected override ZString PlaceOfLoadingCore() => BM_PlaceOfLoading;

	#endregion

	#region BM_ExportTransportMode

	[ResourceStringData("15B18D8E-EB6D-4430-A72A-FB97CB3776DB", Caption = "Border Method of Transport", ShortCaption = "M.O.T.", MediumCaption = "Border M.O.T.", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString BM_ExportTransportMode
	{
		get => base.BM_ExportTransportMode;
		set => base.BM_ExportTransportMode = value;
	}

	#endregion

	#region BM_PresentationDateTime

	[ResourceStringData("DB50BB15-ABD5-419E-BA75-F02A2E293E88", FullDescription = "Date and Time of presentation of the goods", Caption = "Date and Time of presentation", MediumCaption = "Presentation Date", ShortCaption = "Pres. Date", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZDateTimeOffset BM_PresentationDateTime
	{
		get => base.BM_PresentationDateTime;
		set => base.BM_PresentationDateTime = value;
	}

	#endregion

	#region BM_TOLCarrierID

	[ResourceStringData("A58E6339-A309-49C4-AD5B-3887986BC860", Caption = "Transport Identification", ShortCaption = "Transp. ID", MediumCaption = "Transport ID", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString BM_TOLCarrierID
	{
		get => base.BM_TOLCarrierID;
		set => base.BM_TOLCarrierID = value;
	}

	#endregion

	#region BM_CustomsOfficeAtBorder

	[ResourceStringData("29DB7CB3-CE17-48F0-AD2B-FAB597C3199B", Caption = "Customs Office", ShortCaption = "Office", MultipleKey = NctsHeader.Phase5CaptionKey)]
	public override ZString BM_CustomsOfficeAtBorder
	{
		get => base.BM_CustomsOfficeAtBorder;
		set => base.BM_CustomsOfficeAtBorder = value;
	}

	#endregion

	#region Implementation

	void MarkRelatedItemsAsNeedingValidationIfValidationIsSuspended()
	{
		if (!IsMarkingAsNeedingValidationSuspended)
		{
			Header.MarkAsNeedingValidation();
			GoodsItems.MarkAsNeedingValidationIncludingChildren();
			Representative.MarkAsNeedingValidation();
			var customsOffices = Header.IsPhase5 ? Header.CommonMovementHeader.CustomsOffices : Header.CustomsOffices;
			customsOffices.MarkAsNeedingValidation();
		}
	}

	void RefreshRepresentative()
	{
		if (IsTIRDeclaration)
		{
			Representative.OrganisationPK = ZGuid.Empty;
		}
		Representative.ReadOnly = IsTIRDeclaration;
		Representative.RefreshBinding();
	}

	void MarkBillsAsNeedingValidation()
	{
		Header?.Bills?.MarkAsNeedingValidation();
	}

	#endregion

	#region IElectronicFolderSupporter Members

	ZBool IElectronicFolderSupporter.UseElectronicFolder => UseElectronicFolder;

	#endregion

	#region IUCC6AndTranstionPeriodProvider

	bool IUCC6AndTransitionPeriodProvider.IsUCC6AndIsExport => false;

	bool IUCC6AndTransitionPeriodProvider.IsTransitionPeriodAES30 => false;

	bool IUCC6AndTransitionPeriodProvider.IsImport => false;

	#endregion

	public override ZBool BM_ReducedDatasetIndicator
	{
		get => base.BM_ReducedDatasetIndicator;
		set
		{
			var oldValue = BM_ReducedDatasetIndicator;
			base.BM_ReducedDatasetIndicator = value;
			if (!IsCopying && oldValue != BM_ReducedDatasetIndicator)
			{
				GoodsItems?.MarkAsNeedingValidation();
			}
		}
	}

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		args.AddExcludedColumns(new string[] { Schema.BM_EntryDate });
		var clonedMoveHeader = (NctsDepartureMovementHeader)base.CloneInternal(args);
		clonedMoveHeader.SetSystemDefinedValue(Schema.UseElectronicFolder, UseElectronicFolder);
		clonedMoveHeader.SetSystemDefinedValue(Schema.PaymentParty, PaymentParty);
		clonedMoveHeader.SetSystemDefinedValue(Schema.DefermentAccountNumber, DefermentAccountNumber);
		return clonedMoveHeader;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		ParticipantType = NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter;
	}

	public const int ExportDateDaysGap = 8;

	public ZBool IsGroupage => ParticipantType == NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;

	protected override void MarkAsNeedingValidationCore()
	{
		base.MarkAsNeedingValidationCore();

		GoodsItems.MarkAsNeedingValidation();
	}

	void ICusGoodsLocationProvider.ClearGoodsLocation()
	{
		var goodsLocation = GoodsLocation;
		goodsLocation.CGL_Qualifier = ZString.Empty;
		goodsLocation.CGL_Type = ZString.Empty;
		goodsLocation.CGL_CustomsOffice = ZString.Empty;
	}

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public int? BM_ExportTransportModeAsNumber
		=> int.TryParse(BM_ExportTransportMode, out var exportTransportMode) ? exportTransportMode : null;

	protected override bool BM_PaperlessInbondNumReadOnly => false;

	protected override bool ShouldBM_ExportDateBeEmpty => false;

	internal IEnumerable<ZPropertyInfo> GetDepartureTransportMeansProperties()
	{
		yield return BM_TransportAtDepartureTypeInfo;
		yield return BM_TransportAtDepartureInfo;
		yield return BM_RN_NKTransportAtDepartureCountryInfo;
		yield return BM_AircraftIDAtDepartureInfo;
		yield return BM_RN_NKTransportAtDepartureTrailer1NationalityInfo;
		yield return BM_RN_NKTransportAtDepartureTrailer2NationalityInfo;
		yield return BM_TransportAtDepartureTrailer1RegNoInfo;
		yield return BM_TransportAtDepartureTrailer2RegNoInfo;
	}

	internal NctsBillsDepartureTransportMeansWiper BillsDepartureTransportMeansWiper => billsDepartureTransportMeansWiper ?? (billsDepartureTransportMeansWiper = new NctsBillsDepartureTransportMeansWiper(this));
	NctsBillsDepartureTransportMeansWiper billsDepartureTransportMeansWiper;

	public bool IsInAmendmentPhase => BM_Phase == EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Amendment;

	#region ICustomsLinkedObjectAdapterProvider Members

	ISadCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter() => throw new NotSupportedException();

	ISingleWindowCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter() => throw new NotSupportedException();

	IXmlCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter() => new NctsHeaderPhase5CustomsLinkedObjectAdapter(Header);

	#endregion

	#region ITopLevelBusinessObjectProvider Members

	BusinessObject ITopLevelBusinessObjectProvider.TopLevelBusinessObject
	{
		get
		{
			var header = Header;
			if (header is null)
			{
				return null;
			}

			return header.Shipment as BusinessObject ?? header;
		}
	}

	#endregion
}
