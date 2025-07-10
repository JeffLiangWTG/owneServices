using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObject : NctsHeaderCommonMessageSendingObject
	, ICusGoodsLocationProvider
	, IDepartureTransportMeansProvider
	, ICusGoodsLocationProviderWithValidationDecider
{
	public NctsHeaderDepartureMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
		MessageType = Lookups.MessageTypeList?.Count > 0 ? (ZString)Lookups.MessageTypeList[0].Code : ZString.Empty;
		SendingObjectHelper.CheckHeaderNotNullAndDepartureType(nctsHeader);
	}

	public new class Schema : NctsHeaderCommonMessageSendingObject.Schema
	{
		public const string ActualConsignee = nameof(NctsHeaderDepartureMessageSendingObject.ActualConsignee);
	}

	public NctsDepartureMovementHeader MovementHeader => NctsHeader.MovementHeader;

	public new NctsHeaderDepartureMessageSendingObjectLookups Lookups => (NctsHeaderDepartureMessageSendingObjectLookups)base.Lookups;

	protected override NctsHeaderMessageSendingObjectLookups GetNewLookups() => new NctsHeaderDepartureMessageSendingObjectLookups(this);

	protected override NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderDepartureMessageSendingObjectValidation(this);

	[List(nameof(Lookups) + "." + nameof(NctsHeaderDepartureMessageSendingObjectLookups.MessageTypeList))]
	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			var oldValue = MessageType;
			base.MessageType = value;
			if (oldValue != MessageType)
			{
				ClearReasonIfReadOnly();
				SetDefaultsForMessageType();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(NctsHeaderDepartureMessageSendingObjectLookups.ActualDestinationCustomsOfficeList))]
	public override ZString ActualDestinationCustomsOffice
	{
		get => base.ActualDestinationCustomsOffice;
		set
		{
			base.ActualDestinationCustomsOffice = value;
			if (!IsValidationSuspended)
			{
				ActualConsignee.Validation.ValidateE2_OA_Address();
			}
		}
	}

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|TC11DeliveryDate", Caption = "TC11 Delivery Date")]
	public override ZDateTime TC11DeliveryDate
	{
		get => tc11DeliveryDate;
		set
		{
			SetNonPersistentPropertyValue(TC11DeliveryDateInfo, ref tc11DeliveryDate, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateReasonText();
				Validation.ValidateActualDestinationCustomsOffice();
			}
		}
	}
	ZDateTime tc11DeliveryDate;

	public override ZPropertyInfo TC11DeliveryDateInfo => GetZPropertyInfo(Schema.TC11DeliveryDate);

	[List(nameof(Lookups) + "." + nameof(NctsHeaderDepartureMessageSendingObjectLookups.Consignees))]
	public new JobDocAddress ActualConsignee
	{
		get
		{
			if (actualConsigneeJobDocAddress == null || actualConsigneeJobDocAddress.IsDeleted)
			{
				actualConsigneeJobDocAddress = Factory.New<JobDocAddress>();
				actualConsigneeJobDocAddress.E2_ParentID = PK;
				actualConsigneeJobDocAddress.MakeNonPersistent();
				actualConsigneeJobDocAddress.HasChanges = false;
				actualConsigneeJobDocAddress.OverrideRequirement = new ActualConsigneeJobDocAddressRequirement(this);
				RegisterEditableChildObject(actualConsigneeJobDocAddress);
			}

			return actualConsigneeJobDocAddress;
		}
	}
	JobDocAddress actualConsigneeJobDocAddress;

	[ReadOnlyMember(nameof(IsReasonReadOnly))]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderDepartureMessageSendingObjectLookups.ReasonCodeList))]
	public override ZString ReasonCode
	{
		get => base.ReasonCode;
		set
		{
			base.ReasonCode = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateDoubleEntryMRN();
			}
		}
	}

	[ReadOnlyMember(nameof(IsReasonReadOnly))]
	public override ZString ReasonText
	{
		get => base.ReasonText;
		set
		{
			base.ReasonText = value;
			if (!IsValidationSuspended)
			{
				ActualConsignee.Validation.ValidateE2_OA_Address();
			}
		}
	}

	bool IsReasonReadOnly
	{
		get
		{
			switch (MessageType)
			{
				case PassarMessageTypeList.Codes.NT013:
				case PassarMessageTypeList.Codes.NT014:
				case PassarMessageTypeList.Codes.NT141:
				case PassarMessageTypeList.Codes.NT513:
					return false;
				default:
					return true;
			}
		}
	}

	void ClearReasonIfReadOnly()
	{
		if (IsReasonReadOnly)
		{
			ReasonCode = ZString.Empty;
			ReasonText = ZString.Empty;
		}
	}

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|IdentificationNumber", Caption = "Identification Number")]
	public ZString IdentificationNumber => NctsHeader.CommonMovementHeader.Representative?.Organisation?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID) ?? ZString.Empty;

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|ContactName", Caption = "Contact Name")]
	public ZString ContactName => GlbStaff.CurrentUser.GS_FullName;

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|PhoneNumber", Caption = "Phone Number")]
	public ZString PhoneNumber => GlbStaff.CurrentUser.GS_WorkPhone_Formatted;

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|EmailAddress", Caption = "Email")]
	public ZString EmailAddress => GlbStaff.CurrentUser.GS_EmailAddress;

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|InlandTransportModeAtDeparture", Caption = "Inland M.O.T.")]
	[List($"{nameof(MovementHeader)}.{nameof(NctsDepartureMovementHeader.Lookups)}.{nameof(INctsDepartureMovementHeaderLookups.ModeOfTransportList)}")]
	public override ZString InlandTransportModeAtDeparture { get => base.InlandTransportModeAtDeparture; set => base.InlandTransportModeAtDeparture = value; }

	[List($"{nameof(MovementHeader)}.{nameof(NctsDepartureMovementHeader.Lookups)}.{nameof(INctsDepartureMovementHeaderLookups.TransportAtDepartureTypeOfIdList)}")]
	public override ZString TransportTypeAtDeparture { get => base.TransportTypeAtDeparture; set => base.TransportTypeAtDeparture = value; }

	[List(nameof(MovementHeader) + "." + nameof(NctsDepartureMovementHeader.Lookups) + "." + nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList))]
	public override ZString TransportCountryAtDeparture { get => base.TransportCountryAtDeparture; set => base.TransportCountryAtDeparture = value; }

	[List(nameof(NctsHeader) + "." + nameof(Business.NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.CommunicationLanguageList))]
	public override ZString CommunicationLanguage { get => base.CommunicationLanguage; set => base.CommunicationLanguage = value; }

	[MaxLength(nameof(VesselNameAtDeparture_MaxLength))]
	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|VesselNameAtDeparture", Caption = "Vessel Name", ShortCaption = "Vessel")]
	[List($"{nameof(MovementHeader)}.{nameof(Lookups)}.{nameof(INctsDepartureMovementHeaderLookups.Vessels)}")]
	public virtual ZString VesselNameAtDeparture { get => TransportAtDeparture; set => TransportAtDeparture = value; }

	public ZPropertyInfo VesselNameAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselNameAtDeparture), x => TransportAtDepartureInfo);

	int VesselNameAtDeparture_MaxLength => MovementHeader.VesselNameAtDepartureInfo.MaxLength;

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|VesselCountryAtDeparture", Caption = "Vessel Nationality", ShortCaption = "Nationality")]
	[List($"{nameof(MovementHeader)}.{nameof(Lookups)}.{nameof(INctsDepartureMovementHeaderLookups.TransportNationalityList)}")]
	public virtual ZString VesselCountryAtDeparture
	{
		get => TransportCountryAtDeparture;
		set => TransportCountryAtDeparture = value;
	}

	public ZPropertyInfo VesselCountryAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselCountryAtDeparture), x => TransportCountryAtDepartureInfo);

	void SetDefaultsForMessageType()
	{
		SetDefaultActualDestinationCustomsOffice();
		SetDefaultActualConsignee();
		SetDefaultInlandMethodOfTransport();
		SetDefaultTransportNationality();
		SetDefaultCommunicationLanguage();
		SetDefaultTransportTypeAndID();
	}

	void SetDefaultActualDestinationCustomsOffice()
	{
		if (IsNT141 && ActualDestinationCustomsOffice.IsEmpty)
		{
			ActualDestinationCustomsOffice = MovementHeader.DestinationCustomsOfficeCodeForDeparture;
		}
	}

	void SetDefaultActualConsignee()
	{
		if (IsNT141 && ActualConsignee.IsEmpty)
		{
			ActualConsignee.CopyPersistentValuesFrom(NctsHeader.Consignee);
		}
	}

	void SetDefaultInlandMethodOfTransport()
	{
		if (IsNC123 && InlandTransportModeAtDeparture.IsEmpty)
		{
			InlandTransportModeAtDeparture = MovementHeader.BM_InlandTransportMode.Left(NctsHeaderDepartureMessageSendingObject.Schema.InlandTransportModeAtDeparture.Length);
		}
	}

	void SetDefaultTransportNationality()
	{
		if (IsNC123 && TransportCountryAtDeparture.IsEmpty)
		{
			TransportCountryAtDeparture = MovementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._7_FixedTransportInstallations ? ZString.Empty : MovementHeader.BM_RN_NKTransportAtDepartureCountry;
		}
	}

	void SetDefaultCommunicationLanguage()
	{
		if (IsNC123 && CommunicationLanguage.IsEmpty)
		{
			CommunicationLanguage = NctsHeader.BH_CommunicationLanguage.Left(NctsHeaderDepartureMessageSendingObject.Schema.CommunicationLanguageMaxLength);
		}
	}

	void SetDefaultTransportTypeAndID()
	{
		if (IsNC123)
		{
			var movementHeader = MovementHeader;
			if (TransportTypeAtDeparture.IsEmpty)
			{
				TransportTypeAtDeparture = movementHeader.BM_TransportAtDepartureType;
			}
			if (TransportAtDeparture.IsEmpty)
			{
				TransportAtDeparture = movementHeader.BM_TransportAtDeparture;
			}
			if (AircraftIDAtDeparture.IsEmpty)
			{
				AircraftIDAtDeparture = movementHeader.BM_AircraftIDAtDeparture;
			}
		}
	}

	public bool ShowValidationErrors
	{
		get
		{
			switch (MessageType)
			{
				case PassarMessageTypeList.Codes.NT014:
				case PassarMessageTypeList.Codes.NT141:
				case PassarMessageTypeList.Codes.NC016:
				case PassarMessageTypeList.Codes.NC123:
					return false;
				default:
					return true;
			}
		}
	}

	public ZBool IsNT141 => MessageType == PassarMessageTypeList.Codes.NT141;

	public ZBool IsNC123 => MessageType == PassarMessageTypeList.Codes.NC123;

	public ZBool IsNT014 => MessageType == PassarMessageTypeList.Codes.NT014;

	protected override bool IsAmendCore => MessageType.ToString() is PassarMessageTypeList.Codes.NT013 or PassarMessageTypeList.Codes.NT513;

	protected override ZString GetMessageSubTypeForEDIMessage() => PassarMessageTypeList.GetMessageSubType(MessageTypeCodeList.Codes.PassarNcts, MessageType);

	#region ICusGoodsLocationProvider

	public CusGoodsLocation GoodsLocation
	{
		get
		{
			if (goodsLocation == null)
			{
				goodsLocation = CloneGoodsLocation();
			}
			return goodsLocation;
		}
	}
	CusGoodsLocation goodsLocation;

	CusGoodsLocation CloneGoodsLocation()
	{
		var newGoodsLocation = (CusGoodsLocation)MovementHeader.GoodsLocation.Clone();
		var newGoodsLocationAddress = (CusGoodsLocationAddress)MovementHeader.GoodsLocation.Address.Clone();
		newGoodsLocationAddress.E2_ParentID = newGoodsLocation.PK;
		newGoodsLocationAddress.E2_ParentTableCode = CusGoodsLocationSchema.Constants.Prefix;
		newGoodsLocationAddress.E2_AddressType = DocAddressTypes.Codes.Location;
		newGoodsLocation.MakeNonPersistent();
		newGoodsLocationAddress.MakeNonPersistent();
		newGoodsLocation.IsForActivationSending = true;
		return newGoodsLocation;
	}

	[ResourceStringData("CH.NCTS.NctsHeaderDepartureMessageSendingObject|GoodsLocationDescription", Caption = "Approved Location")]
	public ZString GoodsLocationDescription => GoodsLocation.DisplayText;

	public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

	EU.Business.CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => GoodsLocation;

	ZString ICusGoodsLocationProvider.GoodsLocationDescription => GoodsLocation.DisplayText;

	ZPropertyInfo ICusGoodsLocationProvider.GoodsLocationDescriptionInfo => GoodsLocationDescriptionInfo;

	ZString ICusGoodsLocationProvider.ProviderKey => ((ICusGoodsLocationProvider)MovementHeader).ProviderKey;

	void ICusGoodsLocationProvider.ValidateGoodsLocationDescription()
	{
	}

	#endregion ICusGoodsLocationProvider

	public bool IsTransitOperation { get; set; } = true;

	IBusinessObjectCollection<IAdditionalWagonProvider> IDepartureTransportMeansProvider.AdditionalWagons => MovementHeader.AdditionalWagons;

	ZString IDepartureTransportMeansProvider.Trailer1IDAtDeparture => ZString.Empty;

	ZString IDepartureTransportMeansProvider.Trailer1NationalityAtDeparture => ZString.Empty;

	ZString IDepartureTransportMeansProvider.Trailer2IDAtDeparture => ZString.Empty;

	ZString IDepartureTransportMeansProvider.Trailer2NationalityAtDeparture => ZString.Empty;

	ZBool IDepartureTransportMeansProvider.IsInPhase5TransitionPeriod => NctsHeader.IsInPhase5TransitionPeriod;

	ICusGoodsLocationValidationDecider ICusGoodsLocationProviderWithValidationDecider.GoodsLocationValidationDecider => NctsHeader.Configuration.MovementHeaderConfiguration.GetGoodsLocationValidationDecider(MovementHeader);
}
