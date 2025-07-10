using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteIncident : CusInBondEvent, Integration.Customs.EU.NCTS.IEnRouteIncident, ICusInBondContainerTypeSupporter, ICusGoodsLocationProviderWithValidationDecider, ISupportMultipleResourceStringData, ICusSealTypeSupporter
	{
		public EnRouteIncident(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		EnRouteIncidentNctsContainerCollection incidentContainers;
		[ChildEditable]
		public EnRouteIncidentNctsContainerCollection IncidentContainers
		{
			get
			{
				if (incidentContainers == null)
				{
					incidentContainers = GetNewNctsContainerCollection();
					RegisterEditableChildObject(incidentContainers);
				}
				return incidentContainers;
			}
		}

		protected virtual EnRouteIncidentNctsContainerCollection GetNewNctsContainerCollection() => new EnRouteIncidentNctsContainerCollection(this);

		Type ICusInBondContainerTypeSupporter.ContainerType => NctsContainer.TypeDecider.GetTypeForCountryCode(DefaultDataGroupingCode);

		[ChildEditable]
		public CusSealCollection Seals
		{
			get
			{
				if (seals == null)
				{
					seals = GetNewCusSealCollection();
					seals.Load();
					RegisterEditableChildObject(seals);
				}
				return seals;
			}
		}
		CusSealCollection seals;

		protected virtual CusSealCollection GetNewCusSealCollection() => new CusSealCollection(this);

		Type ICusSealTypeSupporter.CusSealType => typeof(CusSeal);

		[ResourceStringData("be3c9728-833f-49ba-8542-d9becc1e9d7b", Caption = "Event Place")]
		public override ZString BN_EventPlace
		{
			get => base.BN_EventPlace;
			set => base.BN_EventPlace = value;
		}

		[List(nameof(Lookups) + "." + nameof(EnRouteIncidentLookups.EventCountries))]
		[ResourceStringData("def2f975-54a6-4e5c-9dd2-524555fb3cb4", Caption = "Event Ctry./Rgn.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("2CA45BD7-8851-487D-9746-507253E8113C", Caption = "Country/Region where incident was reported", MediumCaption = "Ctry./Rgn. Report", ShortCaption = "Ctry./Rgn. Rep.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_EventCountryCode
		{
			get => base.BN_EventCountryCode;
			set => base.BN_EventCountryCode = value;
		}

		[ResourceStringData("85802a21-87df-43b4-953f-5007e998bf91", Caption = "In NCTS?")]
		public ZBool IsInNCTS
		{
			get => BN_CustomsStatus == SubmittedToNCTSCustomsStatus;
			set => BN_CustomsStatus = value ? SubmittedToNCTSCustomsStatus : string.Empty;
		}
		const string SubmittedToNCTSCustomsStatus = "SUB";

		public ZWrappedPropertyInfo IsInNCTSInfo => GetWrappedZPropertyInfo(nameof(IsInNCTS), x => BN_CustomsStatusInfo);

		[ResourceStringData("D644114F-9A8F-4343-B96C-C9C874EE23BB", Caption = "Event Date", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("08B205B5-4AC4-4DB7-9DE1-A8F6BF2201DB", Caption = "Date incident occurred", MediumCaption = "Date", ShortCaption = "Date", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZDateTime BN_EndorsementDate
		{
			get => base.BN_EndorsementDate;
			set => base.BN_EndorsementDate = value;
		}

		[ResourceStringData("8176e695-a25e-4e73-bd41-1dafcde62feb", Caption = "Reported By", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("FB19A12D-8017-4631-962C-0779B7C50844", Caption = "Authority", MediumCaption = "Authority", ShortCaption = "Authority", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_EndorsementAuthority
		{
			get => base.BN_EndorsementAuthority;
			set => base.BN_EndorsementAuthority = value;
		}

		[ResourceStringData("12ced23d-1214-4377-a24e-b8a7fc91e80d", Caption = "Place Reported", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("A2149C0B-3E22-467C-8FDF-B8CD10175686", Caption = "Place where incident was reported", MediumCaption = "Place", ShortCaption = "Place", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_EndorsementPlace
		{
			get => base.BN_EndorsementPlace;
			set => base.BN_EndorsementPlace = value;
		}

		[List(nameof(Lookups) + "." + nameof(EnRouteIncidentLookups.IncidentEndorsementCountries))]
		[ResourceStringData("78e11dc8-3d48-4f94-ad96-6e1861a07303", Caption = "Ctry./Rgn. Reported", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("42535D1F-B628-4A3F-8139-1674272015E0", Caption = "Country/Region where incident was reported", MediumCaption = "Country/Region", ShortCaption = "Ctry./Rgn.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_EndorsementCountryCode
		{
			get => base.BN_EndorsementCountryCode;
			set => base.BN_EndorsementCountryCode = value;
		}

		[ResourceStringData("1ee32014-b758-4b9a-9cee-dfb5dee231a8", Caption = "Info")]
		[ResourceStringData("05C5DD51-9606-4DEF-A96B-7D101D008234", Caption = "Information", MediumCaption = "Info", ShortCaption = "Info", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_Information
		{
			get => base.BN_Information;
			set => base.BN_Information = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(EnRouteIncidentLookups.IncidentCodeList))]
		[ResourceStringData("B86F976E-7763-4307-9DF7-5325C10B1F33", Caption = "Code")]
		[ResourceStringData("1A3B6B3E-D0AA-450F-A5C0-6A639ADBE71B", Caption = "Incident Code", MediumCaption = "Incident", ShortCaption = "Incident", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_IncidentCode
		{
			get => base.BN_IncidentCode;
			set => base.BN_IncidentCode = value;
		}

		[ReadOnlyMember(nameof(BN_CustomsStatus_ReadOnly))]
		[ResourceStringData("6252A40A-110F-4D52-B5F6-C151B472F105", Caption = "Created")]
		[ResourceStringData("68373BD5-A4E4-4559-8239-48A1C609CEF6", Caption = "Created by", MediumCaption = "Created", ShortCaption = "Created", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BN_CustomsStatus
		{
			get => base.BN_CustomsStatus;
			set => base.BN_CustomsStatus = value;
		}

		[ResourceStringData("7593B23A-CDC7-436D-9E1A-5AB61A75C094", Caption = "Transport Means Nationality", MediumCaption = "Nationality", ShortCaption = "Nationality", FullDescription = "Nationality of the Transport Means")]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_RN_NKTransportAtDepartureIDNationality { get => base.BN_RN_NKTransportAtDepartureIDNationality; set => base.BN_RN_NKTransportAtDepartureIDNationality = value; }

		[ResourceStringData("41729D9A-0B45-4F39-95A9-01231642454B", Caption = "Transport Means Identification", MediumCaption = "Identification", ShortCaption = "Identification", FullDescription = "Identification of the Transport Means")]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_TransportAtDepartureID { get => base.BN_TransportAtDepartureID; set => base.BN_TransportAtDepartureID = value; }

		[ResourceStringData("3DC16B60-BB07-42F5-82BA-440E3FEC75DB", Caption = "Transport Means Type", MediumCaption = "Type", ShortCaption = "Type", FullDescription = "Type of Identification of the Transport Means")]
		[List(nameof(Lookups) + "." + nameof(EnRouteIncidentLookups.TransportAtDepartureTypes))]
		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public override ZString BN_TransportAtDepartureType { get => base.BN_TransportAtDepartureType; set => base.BN_TransportAtDepartureType = value; }

		internal bool BN_CustomsStatus_ReadOnly => true;

		public bool IsEventFlagYes => Header != null && Header.BH_ExportFlag == EventFlagList.Codes.Yes;

		public new EnRouteIncidentLookups Lookups => (EnRouteIncidentLookups)base.Lookups;

		protected override CusInBondEventLookups GetNewLookups() => new EnRouteIncidentLookups(this);

		protected override CusInBondEventValidation GetNewValidation() => new EnRouteIncidentValidation(this);

		internal ZBool IsPhase5 => Header?.IsPhase5 ?? ZBool.False;

		public override bool CanDelete => !IsPhase5 || !IsCreatedByCustomsMessage;

		public void SetGoodsLocationReadOnly()
		{
			GoodsLocation?.SetReadOnlyIncludingChildren(Phase5FieldsReadOnly);
		}

		protected override Type GoodsLocationTypeCore => CusGoodsLocation.TypeDecider.GetTypeForCountryCode(DefaultDataGroupingCode);

		public bool Phase5FieldsReadOnly => IsPhase5 && (!IsEventFlagYes || BN_CustomsStatus != IncidentCustomsStatusList.Codes.ONA || Header.IsArrivalDetailsReadOnly || Header.IsArrivalNotificationDisabled);

		ZBool IsCreatedByCustomsMessage => BN_CustomsStatus == IncidentCustomsStatusList.Codes.CUS;

		ZString DefaultDataGroupingCode => Header?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#region ICusGoodsLocationProvider

		[ReadOnlyMember(nameof(Phase5FieldsReadOnly))]
		public EU.Business.CusGoodsLocation GoodsLocation
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Arrival);
					RegisterEditableChildObject(goodsLocation);
					SetGoodsLocationReadOnly();
				}
				return goodsLocation;
			}
		}
		CusGoodsLocation goodsLocation;

		[ResourceStringData("E47DB54B-59D4-42A6-B881-260873476E49", Caption = "Location of Goods", ShortCaption = "Location", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("07D99EEE-65C4-43D0-BF5B-96968FAF6E89", Caption = "Location", MediumCaption = "Location", ShortCaption = "Loc.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public ZString GoodsLocationDescription => GoodsLocation?.DisplayText ?? ZString.Empty;

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		void ICusGoodsLocationProvider.ValidateGoodsLocationDescription()
		{
		}

		ZString ICusGoodsLocationProvider.ProviderKey => DefaultDataGroupingCode + GoodsLocationProviderApplications.Codes.NCTSMovement;

		#endregion

		#region ISupportMultipleResourceStringData

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => Header?.MultipleKeysToUse ?? Array.Empty<string>();

		#endregion

		public ICusGoodsLocationValidationDecider GoodsLocationValidationDecider => Header?.Configuration.EnRouteIncidentConfiguration.GetGoodsLocationValidationDecider();

		public IEnRouteIncidentValidationDecider EnRouteIncidentValidationDecider => Header?.Configuration.EnRouteIncidentConfiguration.GetEnRouteIncidentValidationDecider();
	}
}
