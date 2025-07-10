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
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeader : EU.NCTS.Business.NctsHeader
	, Integration.Customs.IT.ICusInBondHeader
	, ICustomsProfileDataProvider
	, IDeclarantProvider
	, IAeoCertificateSupporter
	, ISingleWindowRequestDataProvider
	, IAuthorizationListDataProvider
	, IAuthorizationHeaderDataProvider
	, IAutHeaderWithCusOfficeProvider
	, ICustomsEntryApplicationReference
	, ICustomsLinkedObjectAdapterProvider
	, ICustomsEntryTransmissionFailable
	, IMovementReferenceNumberProvider
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsHeader.Schema
	{
		public const string Authorization = "Authorization";
		public const int AuthorizationMaxLength = 7;
		public const string DeclarantAddressPK = "DeclarantAddressPK";
		public const string DeclarantOrgPK = "DeclarantOrgPK";
		public const string RepresentationType = "RepresentationType";
		public const int RepresentationTypeMaxLength = 3;
		public const string Subscriber = "Subscriber";
		public const string IrildesArrivalDateForBinding = "EntryNumbersProvider+IrildesWrapper+Date";
		public const string IrildesArrivalOfficeCodeForBinding = "EntryNumbersProvider+IrildesWrapper+Office";
		public const string IrildesArrivalOfficeDescriptionForBinding = "EntryNumbersProvider+IrildesWrapper+OfficeDescription";
		public const string IrildesArrivalStatusForBinding = "EntryNumbersProvider+IrildesWrapper+Status";
		public const string DeclarantCodeForBinding = "DeclarantAddress+Header+OH_Code";
		public const string ApprovalDeferNoForBinding = "MovementHeader+DefermentAccountNumber";
		public const string RegistrationDateForBinding = "EntryNumbersProvider+RegistrationInfo+CE_IssueDate";
		public const string ReleaseDateForBinding = "EntryNumbersProvider+ReleaseInfo+CE_IssueDate";
		public const string RepresentativeForBinding = "MovementHeader+BM_GS_NKCusAgent";
		public const int SubscriberMaxLength = 3;
		public const int NodeMaxLength = 20;
	}

	#region Properties

	public new ICusCodeDataCollection<CountryOfRouting> CountriesOfRouting
		=> (ICusCodeDataCollection<CountryOfRouting>)base.CountriesOfRouting;

	protected override ICusCodeDataCollection<EU.NCTS.Business.CountryOfRouting> GetCountryOfRoutingCollection()
		=> new CountryOfRoutingCollection<CountryOfRouting>(this);

	#region RepresentationType

	public bool IsSelfRepresentationType => RepresentationType == RepresentationTypeList.Codes._1Self;

	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.RepresentationTypeList))]
	[MaxLength(Schema.RepresentationTypeMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsHeader|RepresentationType", Caption = "Rep. Type")]
	public ZString RepresentationType
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.RepresentationType); }
		set
		{
			var oldValue = RepresentationType;
			CheckMaximumLength(RepresentationTypeInfo, value);
			this.SetSystemDefinedValue(Schema.RepresentationType, value);

			if (!IsValidationSuspended)
			{
				Validation.RunActionIfValidationOfType<NctsHeaderValidation>((validation) => validation.ValidateRepresentationType());
			}

			RepresentationTypeInfo.RefreshBinding(oldValue);
			if (!IsCopying && oldValue != RepresentationType)
			{
				UpdateDeclarantAddressIfNeeded();
				DefaultDefermentAccountNumberIfNeeded();
			}
		}
	}

	public ZPropertyInfo RepresentationTypeInfo
	{
		get { return GetZPropertyInfo(Schema.RepresentationType); }
	}

	void DefaultDefermentAccountNumberIfNeeded()
	{
		if (IsDepartureMovement)
		{
			MovementHeader.ResetOrDefaultDefermentAccountNumberIfSingleCustomsCodeFound();
		}
	}

	#endregion

	#region DeclarantOrgPK

	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.Organisations))]
	[ReadOnlyMember(nameof(DeclarantOrgPK_ReadOnly))]
	public ZGuid DeclarantOrgPK
	{
		get { return DeclarantAddressPK_ZAddress.OrgPK; }
		set { DeclarantAddressPK_ZAddress.OrgPK = value; }
	}

	public ZPropertyInfo DeclarantOrgPKInfo
	{
		get { return GetWrappedZPropertyInfo(Schema.DeclarantOrgPK, x => DeclarantAddressPK_ZAddress.OrgPKInfo); }
	}

	protected bool DeclarantOrgPK_ReadOnly => IsSelfRepresentationType;

	#endregion

	#region DeclarantAddressPK

	[RelatedBusinessObject("DeclarantAddress")]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.OrganizationsFindBoxList))]
	[ReadOnlyMember(nameof(DeclarantAddressPK_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsHeader|DeclarantAddress", Caption = "Declarant", FullDescription = "Declarant Name & Address")]
	public ZGuid DeclarantAddressPK
	{
		get { return this.GetSystemDefinedValue<ZGuid>(Schema.DeclarantAddressPK); }
		set
		{
			var oldValue = DeclarantAddressPK;
			if (!IsCopying && oldValue != value)
			{
				this.SetSystemDefinedValue(Schema.DeclarantAddressPK, value);
				if (!IsValidationSuspended)
				{
					Validation.RunActionIfValidationOfType<NctsHeaderValidation>((validation) => validation.ValidateDeclarantAddressPK());
				}
				DeclarantAddressPKInfo.RefreshBinding(oldValue);
				ResetDeclarantAddress();
				GoodsItems.ForEach(x => x.AeoCertificateManager.AddAeoCertificateFromDeclarantIfNeeded());
				DefaultCustomsProfile();
			}
		}
	}

	void ResetDeclarantAddress() => declarantAddress = null;

	public OrgAddress DeclarantAddress => declarantAddress ?? (declarantAddress = Factory.Load<OrgAddress>(DeclarantAddressPK));
	OrgAddress declarantAddress;

	protected bool DeclarantAddressPK_ReadOnly => IsSelfRepresentationType;

	#region ZAddress

	public ZAddress DeclarantAddressPK_ZAddress
	{
		get
		{
			if (declarantAddressPK_ZAddress == null)
			{
				declarantAddressPK_ZAddress = new ZAddress(DeclarantAddressPKInfo);
				declarantAddressPK_ZAddress.DefaultAddressType = AddressType.OFC;
			}
			return declarantAddressPK_ZAddress;
		}
	}
	ZAddress declarantAddressPK_ZAddress;

	#endregion

	public ZPropertyInfo DeclarantAddressPKInfo => GetZPropertyInfo(Schema.DeclarantAddressPK);

	#endregion

	#region Authorization

	[ResourceStringData("Enterprise.Customs.IT.Business.NctsHeader|Authorization", Caption = "Authorization")]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.AuthorisationNumberList))]
	[MaxLength(Schema.AuthorizationMaxLength)]
	[ReadOnlyMember(nameof(IsAuthorizationReadOnly))]
	public ZString Authorization
	{
		get => AuthorizationManager.GetAuthorizationNumber();
		set
		{
			var oldValue = Authorization;
			CheckMaximumLength(AuthorizationInfo, value);
			AuthorizationManager.SetOrDeleteAuthorization(value);
			if (!IsValidationSuspended)
			{
				Validation.RunActionIfValidationOfType<NctsHeaderValidation>((validation) => validation.ValidateAuthorization());
			}
			AuthorizationInfo.RefreshBinding();
			if (oldValue != Authorization)
			{
				MarkAsDirty();
			}
		}
	}

	public void EmptyAuthorizationIfNecessary()
	{
		if (IsAuthorizationReadOnly)
		{
			Authorization = ZString.Empty;
		}
	}

	public ZPropertyInfo AuthorizationInfo => GetZPropertyInfo(Schema.Authorization);

	NctsAuthorizationManager AuthorizationManager => authorizationManager ?? (authorizationManager = new NctsAuthorizationManager(this));
	NctsAuthorizationManager authorizationManager;

	class NctsAuthorizationManager
	{
		public NctsAuthorizationManager(NctsHeader nctsHeader)
		{
			this.nctsHeader = nctsHeader;
			factory = nctsHeader.Factory;
		}

		readonly NctsHeader nctsHeader;
		readonly BusinessObjectFactory factory;

		public ZString GetAuthorizationNumber() => Authorization?.CFR_Reference ?? ZString.Empty;

		public void SetOrDeleteAuthorization(ZString authorizationNumber)
		{
			if (authorizationNumber.IsEmpty)
			{
				DeleteAuthorization();
			}
			else
			{
				var effectiveAuthorization = Authorization ?? CreateNewAuthorization();
				effectiveAuthorization.CFR_Reference = authorizationNumber;
			}
		}

		public void DeleteAuthorization()
		{
			Authorization?.Delete();
			authorization = null;
		}

		NctsAuthorization CreateNewAuthorization()
		{
			var authorization = factory.New<NctsAuthorization>();
			authorization.CFR_ParentID = nctsHeader.PK;
			authorization.CFR_ParentTableCode = nctsHeader.TablePrefix;

			return authorization;
		}

		NctsAuthorization Authorization => authorization ?? (authorization = LoadAuthorization());
		NctsAuthorization authorization;

		NctsAuthorization LoadAuthorization()
		{
			var query = new ZQuery(CusReferenceSchema.CFR_Type, NctsAuthorization.Constants.CfrType);
			query.AddToFilter(CusReferenceSchema.CFR_Code, NctsAuthorization.Constants.CfrCode);
			query.AddToFilter(CusReferenceSchema.CFR_ParentID, nctsHeader.PK);
			return factory.LoadTop1<NctsAuthorization>(query);
		}
	}

	#endregion

	#region Subscriber

	[ResourceStringData("Enterprise.Customs.IT.Business.NctsHeader|Subscriber", Caption = "Subscriber")]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.Subscribers))]
	[MaxLength(Schema.SubscriberMaxLength)]
	public ZString Subscriber
	{
		get => SubscriberManager.GetSubscriberCode();
		set
		{
			var oldValue = Subscriber;
			SubscriberManager.SetOrDeleteSubscriber(value);
			if (!IsValidationSuspended)
			{
				Validation.RunActionIfValidationOfType<NctsHeaderValidation>((validation) => validation.ValidateSubscriber());
			}
			SubscriberInfo.RefreshBinding();
			if (oldValue != Subscriber)
			{
				MarkAsDirty();
			}
		}
	}

	public ZPropertyInfo SubscriberInfo => GetZPropertyInfo(Schema.Subscriber);

	NctsSubscriberManager SubscriberManager => subscriberManager ?? (subscriberManager = new NctsSubscriberManager(this));
	NctsSubscriberManager subscriberManager;

	class NctsSubscriberManager
	{
		public NctsSubscriberManager(NctsHeader nctsHeader)
		{
			this.nctsHeader = nctsHeader;
			factory = nctsHeader.Factory;
		}

		readonly NctsHeader nctsHeader;
		readonly BusinessObjectFactory factory;

		public ZString GetSubscriberCode() => Subscriber?.CP_GS_NKStaff ?? ZString.Empty;

		public void SetOrDeleteSubscriber(ZString staffCode)
		{
			if (staffCode.IsEmpty)
			{
				DeleteSubscriber();
			}
			else
			{
				var effectiveSubscriber = Subscriber ?? CreateNewSubscriber();
				effectiveSubscriber.CP_GS_NKStaff = staffCode;
			}
		}

		public void DeleteSubscriber()
		{
			Subscriber?.Delete();
			subscriber = null;
		}

		NctsSubscriber CreateNewSubscriber()
		{
			var subscriber = factory.New<NctsSubscriber>();
			subscriber.CP_BH_Header = nctsHeader.PK;
			return subscriber;
		}

		NctsSubscriber Subscriber => subscriber ?? (subscriber = LoadSubscriber());
		NctsSubscriber subscriber;

		NctsSubscriber LoadSubscriber()
		{
			var query = new ZQuery(CusInBondPersonSchema.CP_BH_Header, nctsHeader.PK);
			query.AddToFilter(CusInBondPersonSchema.CP_Type, NctsSubscriber.Constants.CodeType);
			return factory.LoadTop1<NctsSubscriber>(query);
		}
	}

	#endregion

	#region BH_CustomsProfile

	[ResourceStringData("0954F538-FAEA-4824-94CB-C3F6FCDF82CD", Caption = "Node", MultipleKey = Phase4CaptionKey)]
	[ResourceStringData("DB45C889-032B-472F-9860-C371ABBB2FD1", Caption = "Account", MultipleKey = Phase5CaptionKey)]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.ProfileList))]
	[MaxLength(Schema.NodeMaxLength)]
	public override ZString BH_CustomsProfile
	{
		get => base.BH_CustomsProfile;
		set
		{
			var oldValue = BH_CustomsProfile;
			base.BH_CustomsProfile = value;
			if (!IsCopying && oldValue != BH_CustomsProfile)
			{
				node = null;
				DefaultSubscriberFromNode();
			}
		}
	}

	void DefaultSubscriberFromNode()
	{
		var subscribers = Lookups.Subscribers;
		var subscribersCount = subscribers.Count;

		if (subscribersCount == 1)
		{
			Subscriber = subscribers[0].GS_Code;
		}
		else if (subscribersCount > 1)
		{
			Subscriber = ZString.Empty;
		}
	}

	#endregion

	#endregion

	public override void Delete()
	{
		if (!IsDeleted)
		{
			SubscriberManager.DeleteSubscriber();
		}
		base.Delete();
	}

	public bool AllBillsHaveSameDepartureTransportMeans => Factory.GetCached(
		ref allBillsHaveSameDepartureTransportMeans,
		() =>
		{
			if (Bills.Count < 2)
			{
				return true;
			}

			var comparer = new DepartureTransportMeansComparer();
			return Bills.Cast<IDepartureTransportMeansProvider>()
				.Skip(1)
				.All(bill => comparer.Equals(Bills[0], bill));
		}
	);

	CachedProperty<bool> allBillsHaveSameDepartureTransportMeans;

	public void SetAsAmendment(ZString? movementReferenceNumber = null)
	{
		if (movementReferenceNumber is ZString value && !value.IsEmpty)
		{
			MovementReferenceEntryNumber.CE_EntryNum = value;
		}

		MovementHeader.BM_CustomsStatus = ZString.Empty;
		EffectiveMessageStatus = ZString.Empty;
		MovementHeader.BM_Phase = EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Amendment;
	}

	public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	protected override EU.NCTS.Business.NctsDepartureMovementHeader GetNewDepartureMovementHeader()
	{
		var movementHeader = (NctsDepartureMovementHeader)base.GetNewDepartureMovementHeader();
		movementHeader?.BillsDepartureTransportMeansWiper.Initialize();
		return movementHeader;
	}

	public new ITEDIMessageCollection Messages => (ITEDIMessageCollection)base.Messages;
	protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new ITEDIMessageCollection(this);

	public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

	protected override CusInBondHeaderValidation GetNewPhase4Validation() => new NctsHeaderValidation(this);

	protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

	protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

	protected override CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new NctsHeaderCustomsOfficeRequirementHelper(this);

	protected override ZInt MaximumGuaranteeCountCore => 1;

	protected override ZBool IsBrokerNeededCore => false;

	internal bool IsDeclarantAddressPKRequired
	{
		get
		{
			var code = RepresentationType;
			return code == RepresentationTypeList.Codes._2Direct || code == RepresentationTypeList.Codes._3Indirect;
		}
	}

	#region IDeclarantProvider

	OrgAddress IDeclarantProvider.DeclarantAddress => DeclarantAddress;

	ZString IDeclarantProvider.RepresentativeType => RepresentationType;

	#endregion

	IEnumerable<NctsDepartureCargoDesc> GoodsItems => MovementHeader?.GoodsItems?.Cast<NctsDepartureCargoDesc>() ?? Enumerable.Empty<NctsDepartureCargoDesc>();

	void UpdateDeclarantAddressIfNeeded()
	{
		if (IsDeclarantAddressPKRequired)
		{
			if (DeclarantAddressPK.IsEmpty && Principal.HasRealAddress)
			{
				DeclarantAddressPK = Principal.E2_OA_Address;
			}
		}
		else if (!DeclarantAddressPK.IsEmpty)
		{
			DeclarantOrgPK = ZGuid.Empty;
		}
	}

	protected override void OnChangedConsigneeDocumentaryAddress()
	{
		base.OnChangedConsigneeDocumentaryAddress();

		GoodsItems.ForEach(x => x.AeoCertificateManager.AddAeoCertificateFromImporterIfNeeded());
	}

	protected override void OnChangedConsignorDocumentaryAddress()
	{
		base.OnChangedConsignorDocumentaryAddress();

		GoodsItems.ForEach(x => x.AeoCertificateManager.AddAeoCertificateFromSupplierIfNeeded());
	}

	protected override void ValidateConsignor(JobDocAddressValidation validation)
	{
		base.ValidateConsignor(validation);

		new TraderJobDocAddressValidation(Consignor, ValidationCaptions.Shared.ConsignorCaption, MovementHeader)
			.ValidateRequiredCustomsCode();

		Validation.RunActionIfValidationOfType<NctsHeaderValidation>((headerValidation) => headerValidation.ValidateConsignorMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage());
	}

	protected override void ValidateConsignee(JobDocAddressValidation validation)
	{
		base.ValidateConsignee(validation);

		Validation.RunActionIfValidationOfType<NctsHeaderValidation>((headerValidation) => headerValidation.ValidateConsigneeMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage());
	}

	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();
		Consignee.Validation.ValidateOrganisationPK();
	}

	protected override void CheckConditionC001()
	{
		var movementHeader = MovementHeader;

		if (movementHeader != null && !ShouldSkipConditionC001())
		{
			base.CheckConditionC001();
		}

		bool ShouldSkipConditionC001()
		{
			return MovementHeader.IsGroupage && !Consignor.IsEmpty && HasMoreThanOneGoodsItemWhereConsigneeIsNotEmpty();

			bool HasMoreThanOneGoodsItemWhereConsigneeIsNotEmpty() => movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Where(x => !x.Consignee.IsEmpty).Skip(1).Any();
		}
	}

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<NctsGuarantee>(this);

	protected bool IsAuthorizationReadOnly => MovementHeader?.IsTIRDeclaration ?? false;

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[EU.NCTS.Business.CusCodeDataTypeList.Codes.CountryOfRouting] = typeof(CountryOfRouting);
		return result;
	}

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	protected override ZDateTime NCTSPhase5TransitionPeriodEffectiveDate
	{
		get
		{
			var mrnDate = MovementHeader?.BM_EntryDate ?? ZDateTime.Empty;
			return mrnDate.IsEmpty ? base.NCTSPhase5TransitionPeriodEffectiveDate : mrnDate;
		}
	}

	protected override bool IsDepartureTabReadOnlyCountrySpecificRules(ZString messageStatus, ZString departureStatus) => departureStatus == NctsTransitStatusList.Codes.NbRejected || departureStatus == NctsTransitStatusList.Codes.GoodsWrittenOff;

	public bool IsNbRejectedAndMok => (EffectiveMessageStatus == EU.NCTS.Business.NctsMessageStatusList.Codes.Ok && MovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.NbRejected);

	#region ICustomsProfileDataProvider Members

	ZString ICustomsProfileDataProvider.CustomsProfile => BH_CustomsProfile;

	#endregion

	#region IAeoCertificateSupporter Members

	OrgHeader IAeoCertificateSupporter.Supplier => Consignor?.Organisation;

	OrgHeader IAeoCertificateSupporter.Importer => Consignee?.Organisation;

	OrgHeader IAeoCertificateSupporter.Declarant => DeclarantAddress?.Header;

	ZString IAeoCertificateSupporter.RepresentationType => RepresentationType;

	ZBool IAeoCertificateSupporter.ShouldAddY022Certificate => ZBool.True;

	ZBool IAeoCertificateSupporter.ShouldAddY023Certificate => ZBool.True;

	#endregion

	public ZString GetApplicationReference() => ApplicationReferenceHelper.GetNew(Node, Subscriber, DepartureCustomsOfficeCode);

	public ZString Node => node ?? (node = CustomsCredentialHelper.GetNodeFromInternalCode(BH_CustomsProfile));
	string node;

	public NctsHeaderEntryNumbersProvider EntryNumbersProvider => entryNumbersProvider ?? (entryNumbersProvider = new NctsHeaderEntryNumbersProvider(this));
	NctsHeaderEntryNumbersProvider entryNumbersProvider;

	#region Implementation

	ZString AuthorisationType => IT.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

	ZGuid HolderPk => Consignor.Organisation?.PK ?? ZGuid.Empty;

	AuthorizationHeaderProvider AuthorizationProvider => authorizationProvider ?? (authorizationProvider = new AuthorizationHeaderProvider(this, Factory));
	AuthorizationHeaderProvider authorizationProvider;

	protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

	void MarkAsDirty()
	{
		HasChanges = true;
	}

	#endregion

	#region ISingleWindowRequestDataProvider Members

	ZGuid ISingleWindowRequestDataProvider.PK => PK;

	ZString ISingleWindowRequestDataProvider.ApplicationReference => GetApplicationReference();

	ZDate ISingleWindowRequestDataProvider.IssueDate => EntryNumbersProvider.RegistrationInfoWrapper.IssueDate;

	ZString ISingleWindowRequestDataProvider.RegisterIncludingSeries => EntryNumbersProvider.RegistrationInfoWrapper.RegisterIncludingSeries;

	ZString ISingleWindowRequestDataProvider.RegistrationNumberWithoutCin => EntryNumbersProvider.RegistrationInfoWrapper.RegistrationNumberWithoutCin;

	ZString ISingleWindowRequestDataProvider.CustomsOffice => DepartureCustomsOfficeCode;

	ZString ISingleWindowRequestDataProvider.TableName => NctsHeader.Schema.TableName;

	#endregion

	#region IAuthorizationListDataProvider

	IEnumerable<ZString> IAuthorizationListDataProvider.AuthorizationTypes => new[] { AuthorisationType };

	IEnumerable<ZGuid> IAuthorizationListDataProvider.GetEligibleHolders() => new ZGuid[] { HolderPk }.Where(x => !x.IsEmpty);

	#endregion

	#region IAuthorisationDataProvider Members

	ZString IAuthorizationHeaderDataProvider.AuthorizationNumber => Authorization;

	IEnumerable<ZString> IAuthorizationHeaderDataProvider.AuthorizationTypes => new[] { AuthorisationType };

	ZGuid IAuthorizationHeaderDataProvider.HolderPk => HolderPk;

	#endregion

	#region IAuthorisationWithCustomsOfficeProvider Members

	CusAuthorisationHeader IAutHeaderWithCusOfficeProvider.Authorization => AuthorizationProvider.Authorization;

	ZString IAutHeaderWithCusOfficeProvider.AuthorizationNumber => Authorization;

	ZString IAutHeaderWithCusOfficeProvider.CustomsOffice
	{
		get
		{
			var customsOffices = IsPhase5 ? CommonMovementHeader.CustomsOffices : CustomsOffices;
			return customsOffices.GetFirstElementHaving(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)?.CY_Data ?? ZString.Empty;
		}
	}

	ZBool IAutHeaderWithCusOfficeProvider.IsExport => true;

	#endregion

	#region ICustomsEntryApplicationReference Members

	ZString ICustomsEntryApplicationReference.Node => Node;

	ZString ICustomsEntryApplicationReference.Subscriber => Subscriber;

	ZString ICustomsEntryApplicationReference.CustomsOffice => DepartureCustomsOfficeCode;

	ZString ICustomsEntryApplicationReference.CustomsProfile => BH_CustomsProfile;

	#endregion

	#region ICustomsLinkedObjectAdapterProvider Members

	ISadCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter() => new NctsHeaderCustomsLinkedObjectAdapter(this);

	ISingleWindowCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter() => new NctsHeaderCustomsLinkedObjectAdapter(this);

	IXmlCustomsLinkedObjectAdapter ICustomsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter() => new NctsHeaderPhase5CustomsLinkedObjectAdapter(this);

	#endregion

	#region ITransmissionFailureSettable

	public void SetAsFailedFromTransmission()
	{
		EffectiveMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;
		if (MovementHeader != null)
		{
			MovementHeader.BM_EntryDate = ZDateTime.Empty;
		}
	}

	#endregion

	public ZString DepartureMovementStatus => MovementHeader?.BM_CustomsStatus ?? ZString.Empty;

	public ZBool StatusAllowsSending
	{
		get
		{
			var messageStatus = EffectiveMessageStatus;
			return
					 IsDepartureDeclarationNotSent
				|| messageStatus == EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors
				|| (messageStatus == EU.NCTS.Business.NctsMessageStatusList.Codes.Ok && DepartureMovementStatus == NctsTransitStatusList.Codes.NbRejected);
		}
	}

	public void AssignDeclarationGoodsItemNumbers(bool reassignNumbers = false)
	{
		var goodsItems = GetGoodsItems();
		if (reassignNumbers)
		{
			goodsItems.ForEach(item => item.BY_DeclarationGoodsItemNumber = ZInt.Zero);
		}

		EU.NCTS.Business.NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(this, goodsItems);
	}

	bool IsDepartureDeclarationNotSent => IsPhase5 ? (CommonMovementHeader.BM_MessageStatus.IsEmpty || CommonMovementHeader.BM_MessageStatus == EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent) : BH_MessageStatus == EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent;

	public new INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> DepartureHeaderContainers => (INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>)base.DepartureHeaderContainers;
	protected override INctsDepartureHeaderContainerCollection<EU.NCTS.Business.NctsDepartureHeaderContainer, EU.NCTS.Business.NctsHeader> GetDepartureHeaderContainersCore() => new NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>(this);

	public new INctsBillCollection<NctsBill> Bills => (INctsBillCollection<NctsBill>)base.Bills;
	protected override INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new NctsBillCollection<NctsBill>(this);

	protected override Type BillTypeCore => typeof(NctsBill);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		DefaultCustomsProfile();
	}

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var clonedNctsHeader = (NctsHeader)base.CloneInternal(args);
		clonedNctsHeader.SetSystemDefinedValue(Schema.RepresentationType, RepresentationType);
		clonedNctsHeader.SetSystemDefinedValue(Schema.DeclarantAddressPK, DeclarantAddressPK);
		return clonedNctsHeader;
	}

	void DefaultCustomsProfile()
	{
		var profileList = Lookups.ProfileList;
		if (BH_CustomsProfile.IsEmpty && profileList.Count == 1)
		{
			BH_CustomsProfile = profileList[0].Code;
		}
	}

	protected override Type DepartureContainerTypeCore => typeof(NctsDepartureHeaderContainer);

	[ResourceStringData("20EE4395-998A-4B9F-A5A3-7A51201B8FF6", Caption = "Release Code", ShortCaption = "Rel. Code")]
	public ZString ReleaseCode => EntryNumbersProvider.ReleaseInfo?.CE_EntryNum ?? ZString.Empty;

	[ResourceStringData("78A14525-31C3-47F9-AC09-A3DFE9A08955", Caption = "Release Date", ShortCaption = "Rel. Date")]
	public ZDateTime CustomsReleaseIssueDate => EntryNumbersProvider.ReleaseInfo?.CE_IssueDate ?? ZDateTime.Empty;

	[ResourceStringData("7CB18D67-48ED-44F5-9A6A-35ACF4087A12", Caption = "Write-off Date", ShortCaption = "W.O. Date")]
	public ZDateTime CustomsWriteOffDate => EntryNumbersProvider.IrildesWrapper?.Date ?? ZDateTime.Empty;

	public ZPropertyInfo CustomsWriteOffDateInfo => GetZPropertyInfo(nameof(CustomsWriteOffDate));
}
