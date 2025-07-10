using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.ExitControl.Business;

[CodeProperty(nameof(MessageDescriptionForEdocs))]
[DescriptionProperty(nameof(MessageDescriptionForEdocs))]
public class CusExitReport : ExitControlBase.Business.CusExitReport
	, EUExitControl.ICusExitReport
	, ICusCodeDataTypeSupporter
	, ICusSupportingInfoTypeSupporter
	, IDocAddresses
	, IDocManagerSupport
	, IValidationModesSupporter
	, IWorkflowTriggerFieldChangeSource
	, IProcessHandlingInfoProvider
	, IUcc6ValueProvider
	, ISupportMultipleResourceStringData
	, ICusGoodsLocationProvider
	, IDocumentSupportable
	, ICusAuthorizationUsageMaster
{
	public CusExitReport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsUCC6)
			{
				SetReadOnlyIncludingChildrenIfUcc6();
			}
		}

	public ZString CountryCode => Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	public new class Schema : AutoCusExitReport.Schema
	{
		public const string CER_Calc_Discrepancies = nameof(CusExitReport.CER_Calc_Discrepancies);
		public const string TypeDescription = nameof(CusExitReport.TypeDescription);
		public const string StatusDescription = nameof(CusExitReport.StatusDescription);
		public const string MessageStatusDescription = nameof(CusExitReport.MessageStatusDescription);
	}

	public static readonly CusExitReportTypeDecider TypeDecider = new CusExitReportTypeDecider();

	public new CusExitHeader Header => Factory.Load<CusExitHeader>(CER_CXH_Header);

	public new CusExitConsignment Consignment => Factory.Load<CusExitConsignment>(CER_CXC_Consignment);

	protected override ZString HumanReadableNameCore => Res.GetString("D029168F-EFCE-4A1E-A199-2C93BE125BEF", "Exit Report {0}", Header?.CXH_JobReference ?? ZString.Empty);

	[ResourceStringData("{6C24370D-8034-4CCE-978B-03696940C8E5}", Caption = "Discrepancies")]
	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.DiscrepancyTypeList))]
	[ReadOnlyMember(nameof(CER_Calc_Discrepancies_ReadOnly))]
	public ZBool CER_Calc_Discrepancies
	{
		get => CER_Behavior.EqualsIgnoringCase(ExitReportDiscrepancyTypeList.Codes.Discrepancies);
		set
		{
			var oldValue = CER_Calc_Discrepancies;
			if (!IsCopying && oldValue != value)
			{
				CER_Behavior = value ? ExitReportDiscrepancyTypeList.Codes.Discrepancies : ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
				CER_Calc_DiscrepanciesInfo.RefreshBinding(oldValue);
			}
			OnDiscrepanciesChanged();
		}
	}

	protected virtual void OnDiscrepanciesChanged()
	{
		if (IsUCC6)
		{
			SetReadOnlyIncludingChildrenForDiscrepancies();
		}
	}

	[ResourceStringData("4415AFCE-B5E6-4F5B-9BC9-11E6EED9AD4A", Caption = "Finalization", ShortCaption = "Final")]
	public override ZBool CER_IsFinalized
	{
		get => base.CER_IsFinalized;
		set => base.CER_IsFinalized = value;
	}

	public ZPropertyInfo CER_Calc_DiscrepanciesInfo => GetZPropertyInfo(Schema.CER_Calc_Discrepancies);
	protected virtual bool CER_Calc_Discrepancies_ReadOnly => CER_Type.EqualsIgnoringCase(ExitReportTypeList.Codes.InformationOnNonExitedExport);

	[List(nameof(Header) + "." + nameof(CusExitHeader.CusExitConsignments))]
	[ReadOnlyMember(nameof(IsCER_CXC_ConsignmentReadOnly))]
	[ResourceStringData("{5D85EC63-934B-4B78-BA29-C5899692A150}", Caption = "Entry/Consignment")]
	public override ZGuid CER_CXC_Consignment { get => base.CER_CXC_Consignment; set => base.CER_CXC_Consignment = value; }

	protected virtual bool IsCER_CXC_ConsignmentReadOnly => !CER_Status.IsEmpty || CER_MessageStatus == LogicalStatusList.Codes.Sent;

	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.DeclarantTypeList))]
	[ReadOnlyMember(nameof(CER_DeclarantType_ReadOnly))]
	public override ZString CER_DeclarantType { get => base.CER_DeclarantType; set => base.CER_DeclarantType = value; }
	protected virtual bool CER_DeclarantType_ReadOnly => false;

	[ReadOnlyMember(nameof(CER_EnquiryInformationCode_ReadOnly))]
	[ResourceStringData("92C8CD89-BAD0-4D0D-AF65-0327BD14A7A5", Caption = "Enquiry Info.")]
	public override ZString CER_EnquiryInformationCode { get => base.CER_EnquiryInformationCode; set => base.CER_EnquiryInformationCode = value; }
	protected virtual bool CER_EnquiryInformationCode_ReadOnly => true;

	public bool IsCER_EnquiryInformationCodeRequired => !CER_EnquiryInformationCode_ReadOnly;

	[ResourceStringData("705B14F7-863A-4E6E-BEF9-5EAAFAAA47AB", Caption = "Report Type", MediumCaption = "Rpt. Type", ShortCaption = "Type")]
	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.TypeList))]
	public override ZString CER_Type
	{
		get => base.CER_Type;
		set
		{
			var oldValue = CER_Type;
			base.CER_Type = value;
			if (!IsCopying && oldValue != CER_Type)
			{
				OnCER_TypeChanged();
			}
		}
	}

	[ResourceStringData("D4782C3E-59C0-49C5-8ECC-EFEAA17E6C9E", Caption = "Report Type Description", MediumCaption = "Type Description", ShortCaption = "Type Desc.")]
	public ZString TypeDescription
	{
		get
		{
			var result = ZString.Empty;
			var type = CER_Type;
			if (!type.IsEmpty)
			{
				result = Lookups.TypeList.GetDescriptionFromCode(type) ?? UnknownDescription;
			}
			return result;
		}
	}

	[ResourceStringData("8AB1BA42-503B-46CE-9556-72F743B6ACC4", Caption = "Mode of Transport", MediumCaption = "Mode of Trans.", ShortCaption = "M.O.T.")]
	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.TransportModeList))]
	public override ZString CER_TransportMode
	{
		get => base.CER_TransportMode;
		set
		{
			var oldValue = CER_TransportMode;
			base.CER_TransportMode = value;
			if (!IsCopying && oldValue != CER_TransportMode)
			{
				if (SupportSetDefaultCER_TransportType)
				{
					SetDefaultCER_TransportType();
				}
			}
		}
	}

	protected virtual bool SupportSetDefaultCER_TransportType => true;

	[ResourceStringData("E5DC2511-0A8E-41D8-AD86-ED147D19C3F3", Caption = "Transport Type", MediumCaption = "Trans. Type")]
	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.TransportTypeList))]
	[ReadOnlyMember(nameof(DiscrepanciesNotTickedFieldsReadOnly))]
	public override ZString CER_TransportType
	{
		get => base.CER_TransportType;
		set => base.CER_TransportType = value;
	}

	[ResourceStringData("9C16D11E-820E-40EF-B321-68A1E945DA7F", Caption = "Transport ID", MediumCaption = "Trans. ID")]
	[ReadOnlyMember(nameof(DiscrepanciesNotTickedFieldsReadOnly))]
	public override ZString CER_TransportID
	{
		get => base.CER_TransportID;
		set => base.CER_TransportID = value;
	}

	[ResourceStringData("DAD37D5B-ECF5-4195-B6D0-969062B6325F", Caption = "Transport Nationality", MediumCaption = "Trans. Nationality", ShortCaption = "Nationality")]
	[ReadOnlyMember(nameof(DiscrepanciesNotTickedFieldsReadOnly))]
	public override ZString CER_RN_NKTransportNationality
	{
		get => base.CER_RN_NKTransportNationality;
		set => base.CER_RN_NKTransportNationality = value;
	}

	[ResourceStringData("5BA0EF71-043E-4254-8EB9-94B93D46EFD1", Caption = "Location", MediumCaption = "Loc.")]
	[ReadOnlyMember(nameof(CER_Location_ReadOnly))]
	public override ZString CER_Location
	{
		get => base.CER_Location;
		set => base.CER_Location = value;
	}
	protected virtual bool CER_Location_ReadOnly => false;

	[ResourceStringData("E163F997-5779-4517-8244-F2BC6966160B", Caption = "Date & Time", ShortCaption = "Date", MultipleKey = CusExitReport.NotUcc6CaptionKey)]
	[ResourceStringData("81450BA8-5FE0-41F7-A91E-0CF0D0A03D2D", Caption = "Arrival Date", ShortCaption = "Arr. Date", MultipleKey = CusExitReport.Ucc6CaptionKey)]
	[ReadOnlyMember(nameof(IsUCC6))]
	public override ZDateTimeOffset CER_DateTime
	{
		get => base.CER_DateTime;
		set => base.CER_DateTime = value;
	}

	[ResourceStringData("58E87EA2-A153-4C49-A861-4415A304BE9B", Caption = "Office of Exit")]
	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.OfficeOfExitList))]
	public override ZString CER_OfficeOfExit
	{
		get => base.CER_OfficeOfExit;
		set => base.CER_OfficeOfExit = value;
	}

	[ResourceStringData("41639A51-DE4F-454F-A960-DD52E27351CA", Caption = "Status")]
	[ReadOnly(true)]
	[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.StatusList))]
	public override ZString CER_Status
	{
		get => base.CER_Status;
		set
		{
			var oldValue = CER_Status;
			if (oldValue != value)
			{
				base.CER_Status = value;
				if (IsUCC6)
				{
					SetReadOnlyIncludingChildrenIfUcc6();
				}
			}
		}
	}

	[ResourceStringData("45A377CC-03BC-49EF-8E8B-40435618F7A1", Caption = "Status Description", MediumCaption = "Status Desc.")]
	public ZString StatusDescription => StatusDescriptionCore;
	protected virtual ZString StatusDescriptionCore => Factory.GetValue(ref statusDescriptionCached, () =>
	{
		if (IsUCC6
			&& CER_Status is ZString status && !status.IsEmpty
			&& Lookups.StatusList.GetDescriptionFromCode(status) is string statusDescription
			&& statusDescription is not null)
		{
			return statusDescription;
		}

		return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CER_Status, CountryCode,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today, includeParentDataGrouping: false)?.ZZD_Description ?? ZString.Empty;
	});
	CachedProperty<ZString> statusDescriptionCached;

	[ResourceStringData("34537061-EAE1-49A6-A0A2-569DCEFAEE20", Caption = "Message Status", MediumCaption = "Msg. Status")]
	[ReadOnly(true)]
	public override ZString CER_MessageStatus
	{
		get => base.CER_MessageStatus;
		set
		{
			var oldValue = CER_MessageStatus;
			if (oldValue != value)
			{
				base.CER_MessageStatus = value;
				if (IsUCC6)
				{
					SetReadOnlyIncludingChildrenIfUcc6();
				}
			}
		}
	}

	[ResourceStringData("07BBEF28-EC8D-49FF-AD57-02E1E667BBDA", Caption = "Message Status Description", MediumCaption = "Msg. Status Description", ShortCaption = "Msg. Status Desc.")]
	public ZString MessageStatusDescription
	{
		get
		{
			var result = ZString.Empty;
			var messageStatus = CER_MessageStatus;
			if (!messageStatus.IsEmpty)
			{
				result = Lookups.MessageStatusList.GetDescriptionFromCode(messageStatus) ?? UnknownDescription;
			}
			return result;
		}
	}
	protected string UnknownDescription => Res.GetString("9FFAD3AD-F3C2-4613-9892-714A4C1C1E0B", "Unknown");

	[ResourceStringData("663B5B0A-CB8B-454D-ACAC-CCBF1AC2A8BE", Caption = "Show only packages related to the item")]
	public ZBool IsPackageRelatedToConsignmentItem
	{
		get => isPackageRelatedToConsignmentItem;
		set
		{
			using (SuspendSettingHasChanges())
			{
				SetNonPersistentPropertyValue(IsPackageRelatedToConsignmentItemInfo, ref isPackageRelatedToConsignmentItem, value);
			}
		}
	}
	ZBool isPackageRelatedToConsignmentItem = true;

	public ZPropertyInfo IsPackageRelatedToConsignmentItemInfo => GetZPropertyInfo(nameof(IsPackageRelatedToConsignmentItem));

	public override ZGuid CER_CXH_Header
	{
		get => base.CER_CXH_Header;
		set
		{
			var oldValue = CER_CXH_Header;
			base.CER_CXH_Header = value;
			if (!IsCopying && oldValue != CER_CXH_Header)
			{
				AlternativeEvidences.MarkAsNeedingValidation();
			}
		}
	}

	EUExitControl.ICusExitConsignment EUExitControl.ICusExitReport.Consignment => Consignment;

	IActiveBusinessObjectCollection<EUExitControl.ICusExitReportItem> EUExitControl.ICusExitReport.CusExitReportItems => CusExitReportItems;

	[ChildEditable(true)]
	public ICusExitReportItemCollection<CusExitReportItem> CusExitReportItemsForBinding
	{
		get
		{
			if (cusExitReportItemsForBinding is null)
			{
				cusExitReportItemsForBinding = (ICusExitReportItemCollection<CusExitReportItem>)CreateNewCusExitReportItemCollection(new ZQuery(CusExitReportItemSchema.ERI_CCI_ConsignmentItem, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				ResetCusExitReportItemsForBindingAdditionalFilter();
				cusExitReportItemsForBinding.ApplySort(nameof(CusExitReportItem.ConsignmentItemLineNumber), System.ComponentModel.ListSortDirection.Ascending);
				RegisterEditableChildObject(cusExitReportItemsForBinding);
			}
			return cusExitReportItemsForBinding;
		}
	}
	ICusExitReportItemCollection<CusExitReportItem> cusExitReportItemsForBinding;

	public void ResetCusExitReportItemsForBindingAdditionalFilter()
	{
		if (cusExitReportItemsForBinding is not null)
		{
			var distinctByConsignmentItemPKs = CusExitReportItems.DistinctBy(x => x.ERI_CCI_ConsignmentItem).Select(x => x.PK);
			cusExitReportItemsForBinding.AdditionalFilter = new ZQuery(CusExitReportItemSchema.PK, distinctByConsignmentItemPKs);
		}
	}

	[ChildEditable(true)]
	public ICusExitReportItemCollection<CusExitReportItem> CusExitReportItemPackages
	{
		get
		{
			if (cusExitReportItemPackages is null)
			{
				cusExitReportItemPackages = CreateNewCusExitReportItemPackageCollection();
				cusExitReportItemPackages.ApplySort(nameof(CusExitReportItem.ConsignmentItemLineNumber), System.ComponentModel.ListSortDirection.Ascending);
				RegisterEditableChildObject(cusExitReportItemPackages);
			}
			return cusExitReportItemPackages;
		}
	}
	ICusExitReportItemCollection<CusExitReportItem> cusExitReportItemPackages;

	protected virtual ICusExitReportItemCollection<CusExitReportItem> CreateNewCusExitReportItemPackageCollection() => new CusExitReportItemCollection<CusExitReportItem>(this, new ZQuery(CusExitReportItemSchema.ERI_CXP_Package, SQLComparisonOperator.NotEqual, ZGuid.Empty));

	public void UpdateAdditionalFilterForCusExitReportItemPackages(ZGuid exitConsignmentItemPK)
	{
		CusExitReportItemPackages.AdditionalFilter = new ZQuery(CusExitReportItemSchema.ERI_CCI_ConsignmentItem, exitConsignmentItemPK);
	}

	public void ClearAdditionalFilterForCusExitReportItemPackages()
	{
		CusExitReportItemPackages.AdditionalFilter = new ZQuery();
	}

	public EDIMessageCollection Messages
	{
		get
		{
			if (messages is null)
			{
				messages = new EDIMessageCollection(this, Factory);
				messages.Load();
				messages.IsManagedForDataRefresh = true;
			}
			return messages;
		}
	}
	EDIMessageCollection messages;

	public bool IsAlternativeEvidenceRequired => CER_Type == ExitReportTypeList.Codes.InformationOnNonExitedExport && IsAlternativeEvidenceRequiredCore;

	protected virtual bool IsAlternativeEvidenceRequiredCore => false;

	[ChildEditable(true)]
	public IAlternativeEvidenceCollection<AlternativeEvidence> AlternativeEvidences
	{
		get
		{
			if (alternativeEvidences is null)
			{
				alternativeEvidences = CreateNewAlternativeEvidenceCollection();
				alternativeEvidences.Load();
				RegisterEditableChildObject(alternativeEvidences);
			}
			return alternativeEvidences;
		}
	}
	IAlternativeEvidenceCollection<AlternativeEvidence> alternativeEvidences;

	protected virtual IAlternativeEvidenceCollection<AlternativeEvidence> CreateNewAlternativeEvidenceCollection() => new AlternativeEvidenceCollection<AlternativeEvidence>(this);

	public bool IsAdditionalInfosRequiredForReportAndItems => CER_Type == ExitReportTypeList.Codes.Presentation;

	[ChildEditable(true)]
	public IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos
	{
		get
		{
			if (additionalInfos is null)
			{
				additionalInfos = CreateNewAdditionalInfoCollection();
				additionalInfos.Load();
				RegisterEditableChildObject(additionalInfos);
			}
			return additionalInfos;
		}
	}
	IAdditionalInfoCollection<AdditionalInfo> additionalInfos;

	protected virtual IAdditionalInfoCollection<AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

	[ChildEditable(true)]
	public ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReport> CusAuthorizationUsages
	{
		get
		{
			if (cusAuthorizationUsages is null)
			{
				cusAuthorizationUsages = GetCusAuthorizationUsages();
				cusAuthorizationUsages.Load();
				cusAuthorizationUsages.IsManagedForDataRefresh = true;
				RegisterEditableChildObject(cusAuthorizationUsages);
			}
			return cusAuthorizationUsages;
		}
	}
	ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReport> cusAuthorizationUsages;
	protected virtual ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReport> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitReport>(this, Factory);

	SchemaGuidColumn ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

	protected virtual void OnCER_TypeChanged()
	{
		AlternativeEvidences.MarkAsNeedingValidation();
		ClearCER_BehaviorIfNeeded();
	}

	protected virtual void ClearCER_BehaviorIfNeeded()
	{
		if (CER_Type.EqualsIgnoringCase(ExitReportTypeList.Codes.InformationOnNonExitedExport) && !CER_Behavior.EqualsIgnoringCase(ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies))
		{
			CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;
	}

	void SetDefaultCER_TransportType()
	{
		switch (CER_TransportMode)
		{
			case TransportTypeList.Codes.Air:
				CER_TransportType = CusExitReportTransportTypeList.Codes._40;
				break;
			case TransportTypeList.Codes.InlandWaterwayTransport:
				CER_TransportType = CusExitReportTransportTypeList.Codes._80;
				break;
			case TransportTypeList.Codes.Rail:
				CER_TransportType = CusExitReportTransportTypeList.Codes._21;
				break;
			case TransportTypeList.Codes.Road:
				CER_TransportType = CusExitReportTransportTypeList.Codes._30;
				break;
			case TransportTypeList.Codes.Sea:
				CER_TransportType = CusExitReportTransportTypeList.Codes._10;
				break;
			case TransportTypeList.Codes.FixedTransportInstallations:
			case TransportTypeList.Codes.OwnPropulsion:
			case TransportTypeList.Codes.Mail:
				CER_TransportType = string.Empty;
				break;
		}
	}

	public override bool CanDelete => Messages.Count == 0;

	public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("8EFEC54B-DBEB-46EB-8788-CD0A30943335", "Exit Report cannot be deleted as it has already been sent to Customs.");

	public override void Delete()
	{
		if (!IsDeleted)
		{
			FetchForLoadChildEditableObjectsIfNeeded();
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			DocAddresses.RemoveAndDeleteAll();
			CusExitReportItems.DeleteAll();
			this.DeleteChildren<CusGoodsLocation>(CusGoodsLocationSchema.CGL_ParentID);
		}
		base.Delete();
	}

	public void SetReportBehaviorFromConsignmentItems() => SetReportBehaviorFromConsignmentItemsCore();

	protected virtual void SetReportBehaviorFromConsignmentItemsCore()
	{
		var consignmentItems = Consignment.CusExitConsignmentItems;
		CER_Behavior = consignmentItems.Any(x => x.CCI_Calc_ShouldReportItem) ? CusExitReportBehaviorList.Codes.DIS : CusExitReportBehaviorList.Codes.STD;
	}

	public bool IsUCC6 => IsUCC6Core;
	protected virtual bool IsUCC6Core => Header?.IsUCC6 ?? false;

	public bool IsAccepted => AcceptedStatusList.Contains(CER_Status);

	public bool IsSentOrAccepted => IsAwaitingResponse || IsAccepted;

	public bool IsSentOrNotEmpty => IsAwaitingResponse || CER_Status != ZString.Empty;

	public new CusExitReportLookups Lookups => (CusExitReportLookups)base.Lookups;

	public new CusExitReportValidation Validation => (CusExitReportValidation)base.Validation;

	protected override ExitControlBase.Business.CusExitReportLookups GetNewLookups() => IsUCC6
			? new CusExitReportUcc6Lookups(this)
			: new CusExitReportLookups(this);

	protected override ExitControlBase.Business.CusExitReportValidation GetNewValidation() => new CusExitReportValidation(this);

	public new ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems => (ICusExitReportItemCollection<CusExitReportItem>)base.CusExitReportItems;

	protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection(ZQuery filter) => new CusExitReportItemCollection<CusExitReportItem>(this, filter);

	protected virtual IEnumerable<ZString> AcceptedStatusList => new ZString[]
	{
		AESEntryStatusList.Codes.ReleasedForExit,
		AESEntryStatusList.Codes.ControlledForExit,
		AESEntryStatusList.Codes.Refused,
	};

	void SetReadOnlyIncludingChildrenForDiscrepancies()
	{
		AdditionalInfos.SetReadOnlyIncludingChildren(DiscrepanciesNotTickedFieldsReadOnly);
		foreach (var item in CusExitReportItemsForBinding)
		{
			item.SetReadOnlyIncludingChildren(DiscrepanciesNotTickedFieldsReadOnly);
		}
	}

	bool IsAwaitingResponse => CER_MessageStatus == LogicalStatusList.Codes.Sent;

	protected bool DiscrepanciesNotTickedFieldsReadOnly => IsUCC6 && !CER_Calc_Discrepancies;

	void SetReadOnlyIncludingChildrenIfUcc6()
	{
		if (IsUCC6)
		{
			SetReadOnlyIncludingChildren(IsSentOrAccepted);
		}
	}

	#region ICusSupportingInfoTypeSupporter Implementation

	IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

	protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
	{
		{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) }
	};

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

	protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	#region Declarant

	public JobDocAddress Declarant
	{
		get
		{
			if (declarantJobDocAddress is null || declarantJobDocAddress.IsDeleted)
			{
				if (declarantJobDocAddress is not null)
				{
					declarantJobDocAddress.DocAddressChanged -= new EventHandler(DeclarantJobDocAddressChanged);
				}
				declarantJobDocAddress = DocAddresses.FindOrCreateWithRequirement(DeclarantJobDocAddressRequirement);
				declarantJobDocAddress.DocAddressChanged += DeclarantJobDocAddressChanged;
				declarantJobDocAddress.AdditionalValidation = GetDeclarantJobDocAddressAdditionalValidation(declarantJobDocAddress);
			}
			return declarantJobDocAddress;
		}
	}
	JobDocAddress declarantJobDocAddress;

	protected virtual ZValidation GetDeclarantJobDocAddressAdditionalValidation(JobDocAddress declarantJobDocAddress) => null;

	public JobDocAddressRequirement DeclarantJobDocAddressRequirement
	{
		get
		{
			if (declarantJobDocAddressRequirement is null)
			{
				declarantJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Declarant, ContactType.NotifyParty)
				{
					CanOverride = DeclarantJobDocAddressCanOverride,
					ValidateOrganisationPK = ValidateDeclarant,
				};
				JobDocAddressManager.AddRequirement(declarantJobDocAddressRequirement);
			}
			return declarantJobDocAddressRequirement;
		}
	}
	JobDocAddressRequirement declarantJobDocAddressRequirement;

	protected virtual bool DeclarantJobDocAddressCanOverride => true;

	protected virtual void ValidateDeclarant(JobDocAddressValidation validation)
	{
	}

	void DeclarantJobDocAddressChanged(object sender, EventArgs e)
	{
		OnChangedDeclarantJobDocAddressRequirement();
	}

	protected virtual void OnChangedDeclarantJobDocAddressRequirement()
	{
	}

	#endregion

	#region Representative

	public JobDocAddress Representative
	{
		get
		{
			if (representativeJobDocAddress is null || representativeJobDocAddress.IsDeleted)
			{
				if (representativeJobDocAddress is not null)
				{
					representativeJobDocAddress.DocAddressChanged -= new EventHandler(RepresentativeJobDocAddressChanged);
				}
				representativeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(RepresentativeJobDocAddressRequirement);
				representativeJobDocAddress.DocAddressChanged += new EventHandler(RepresentativeJobDocAddressChanged);
				representativeJobDocAddress.AdditionalValidation = GetRepresentativeJobDocAddressAdditionalValidation(representativeJobDocAddress);
			}
			return representativeJobDocAddress;
		}
	}
	JobDocAddress representativeJobDocAddress;

	protected virtual ZValidation GetRepresentativeJobDocAddressAdditionalValidation(JobDocAddress representativeJobDocAddress) => null;

	public JobDocAddressRequirement RepresentativeJobDocAddressRequirement
	{
		get
		{
			if (representativeJobDocAddressRequirement is null)
			{
				representativeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Representative, ContactType.Administration)
				{
					CanOverride = RepresentativeJobDocAddressCanOverride,
					ValidateOrganisationPK = ValidateRepresentative,
				};
				JobDocAddressManager.AddRequirement(representativeJobDocAddressRequirement);
			}
			return representativeJobDocAddressRequirement;
		}
	}
	JobDocAddressRequirement representativeJobDocAddressRequirement;

	protected virtual bool RepresentativeJobDocAddressCanOverride => true;

	protected virtual void ValidateRepresentative(JobDocAddressValidation validation)
	{
	}

	void RepresentativeJobDocAddressChanged(object sender, EventArgs e)
	{
		OnChangedRepresentativeJobDocAddressRequirement();
	}

	protected virtual void OnChangedRepresentativeJobDocAddressRequirement()
	{
	}

	#endregion

	JobDocAddressManager JobDocAddressManager => jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager());
	JobDocAddressManager jobDocAddressManager;

	#region IDocAddresses Implementation

	[ChildEditable(true)]
	public JobDocAddressDependentCollection DocAddresses
	{
		get
		{
			if (jobDocAddressDependentCollection is null)
			{
				jobDocAddressDependentCollection = new JobDocAddressDependentCollection(this);
				jobDocAddressDependentCollection.Load();
				RegisterEditableChildObject(jobDocAddressDependentCollection);
			}
			return jobDocAddressDependentCollection;
		}
	}
	JobDocAddressDependentCollection jobDocAddressDependentCollection;

	void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
	{
	}

	bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => true;

	void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
	{
	}

	void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
	{
	}

	Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
	{
		return Environment.Env.Security.None;
	}

	JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
	{
		switch (addressType)
		{
			case DocAddressType.Declarant:
				return DeclarantJobDocAddressRequirement;
			case DocAddressType.Representative:
				return RepresentativeJobDocAddressRequirement;
			default:
				return null;
		}
	}

	OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
	{
		return new OrgHeaderCollection(Factory);
	}

	ZString IDocAddresses.HumanReadableName => ZString.Empty;

	void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
	{
	}

	void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
	{
	}

	ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		return null;
	}

	IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[]
	{
		DocAddressType.Declarant,
		DocAddressType.Representative
	};

	#endregion

	#region ICusCodeDataTypeSupporter Members

	IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypes();

	protected virtual Dictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = new Dictionary<ZString, Type>();
		result.Add(CusCodeDataTypeList.Codes.AlternativeEvidence, typeof(AlternativeEvidence));
		return result;
	}

	#endregion

	#region IValidationModesSupporter
	public ValidationModes ValidationModes
	{
		get
		{
			if (!fValidationModes.HasValue)
			{
				ValidationModesCalculator.RecalculateValidationModes();
			}
			return fValidationModes.Value;
		}
		set
		{
			fValidationModes = value;
		}
	}
	ValidationModes? fValidationModes;

	public ExitReportValidationModesCalculator ValidationModesCalculator => validationModesCalculator ?? (validationModesCalculator = CreateNewValidationModesCalculator());
	ExitReportValidationModesCalculator validationModesCalculator;

	protected virtual ExitReportValidationModesCalculator CreateNewValidationModesCalculator() => new ExitReportValidationModesCalculator(this);

	public bool IsOriginalValidationMode
	{
		get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Original); }
	}

	#endregion

	#region IDocManager Implementation

	public ZString MessageDescriptionForEdocs => $"{CER_Type}-{Consignment?.CXC_MovementReference}";
	public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsExitReport));
	DocManagerInfo docManagerInfo;

	#endregion

	#region IProcessHandlingInfoProvider

	public ProcessHandlingInfo ProcessHandlingInfo => new CusExitReportProcessHandlingInfo(this);

	#endregion

	public void DefaultDataFromParent()
	{
		var parent = Header?.Parent;
		var shipment = parent as ForwardingShipment;
		var declaration = parent as JobDeclaration ?? shipment?.DeclarationForDocuments as JobDeclaration;

		if (declaration is not null)
		{
			DefaultDataFromDeclaration(declaration);
		}

		if (shipment is not null)
		{
			DefaultDataFromShipment(shipment);
		}
	}

	protected virtual void DefaultDataFromDeclaration(JobDeclaration declaration)
	{
	}

	protected virtual void DefaultDataFromShipment(ForwardingShipment shipment)
	{
	}

	public IReadOnlyList<IWorkflowProvider> ParentWorkflowProviders => new IWorkflowProvider[] { Header };

	public IReadOnlyList<string> MultipleKeysToUse => IsUCC6
		? new[] { Ucc6CaptionKey }
		: new[] { NotUcc6CaptionKey };

	public const string NotUcc6CaptionKey = "932ECA74-9C60-46EA-A779-C05C3FF47C59";
	public const string Ucc6CaptionKey = "25D5129A-4D69-47FD-B8F8-7F284FF1C9ED";

	public ICusExitReportValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);

	CachedValue<ICusExitReportValidationDecider> validationDeciderCached;

	ICusExitReportValidationDecider GetValidationDecider() => Header?.Configuration.CusExitReportConfiguration.GetValidationDecider(this);

	#region ICusGoodsLocationProvider

	EU.Business.CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => GoodsLocation;

	public CusGoodsLocation GoodsLocation
	{
		get
		{
			if (goodsLocation is null && IsUCC6)
			{
				goodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.ExitControl);
				RegisterEditableChildObject(goodsLocation);
			}
			return goodsLocation;
		}
	}
	CusGoodsLocation goodsLocation;

	[ResourceStringData("B20ACF47-97D5-45E1-A622-4F6EA310C08D", Caption = "Location of Goods", ShortCaption = "Location")]
	public ZString GoodsLocationDescription
	{
		get => GoodsLocation?.DisplayText ?? ZString.Empty;
	}

	public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

	public void ValidateGoodsLocationDescription()
	{
		Validation.ValidateGoodsLocationDescription();
	}

	ZString ICusGoodsLocationProvider.ProviderKey => DefaultDataGroupingCode + GoodsLocationProviderApplications.Codes.ExitControl;

	public ZBool AllowMixedCaseAuthorisationNumbers => AllowMixedCaseAuthorisationNumbersCore;

	protected virtual ZBool AllowMixedCaseAuthorisationNumbersCore => ZBool.False;

	#endregion

	public ZString DefaultDataGroupingCode => Header?.Declaration?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	public DocumentSupporter DocumentSupporter => documentSupporter ??= CreateNewDocumentSupporter();

	DocumentSupporter documentSupporter;

	protected virtual DocumentSupporter CreateNewDocumentSupporter() => new CusExitReportDocumentSupporter(this);
}
