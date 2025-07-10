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
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondMoveDetail : Customs.Business.CusInBondMoveDetail, IDocAddresses
	{
		public CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondMoveDetail.Schema
		{
			public const string DifferenceWeight = nameof(CusInBondMoveDetail.DifferenceWeight);
			public const string DifferenceWeightUnit = nameof(CusInBondMoveDetail.DifferenceWeightUnit);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("7F8C866A-1648-43A5-B34B-3C80B6B81BCE", "Customs In Bond Move Details");

		[ResourceStringData("62575b45-b979-490b-9a9e-6128392a00ed", Caption = "Sequence No.", FullDescription = "Sequence Number of House Bill", ShortCaption = "Seq. No.")]
		[ReadOnly(true)]
		public override ZString B9_SeqNo
		{
			get => base.B9_SeqNo;
			set => base.B9_SeqNo = value;
		}

		[RelatedBusinessObject("MoveHeader")]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject MoveHeader (BusinessObject) is an abstract class")]
		public override ZGuid B9_BM
		{
			get => base.B9_BM;
			set => base.B9_BM = value;
		}

		public static new readonly TypeDecider TypeDecider = new CusInBondMoveDetailTypeDecider();

		protected override Type ContainerTypeCore => typeof(CusInBondContainer);

		protected override Type PackTypeCore => typeof(CusInvPack);

		protected override Type MoveLineItemTypeCore => typeof(CusInBondMoveLineItem);

		protected override ICusInBondContainerCollection GetContainersCollection() => new CusInBondContainerCollection(this);

		#region IDocAddresses Implementation

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (jobDocAddressDependentCollection == null)
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

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Environment.Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsigneeAddress:
					return ConsigneeDocAddressRequirement;
				case DocAddressType.ConsignorDocumentaryAddress:
					return ConsignorDocAddressRequirement;
				default:
					return null;
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => new OrgHeaderCollection(Factory);

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[]
		{
			DocAddressType.ConsignorDocumentaryAddress,
			DocAddressType.ConsigneeAddress
		};

		#endregion

		[ResourceStringData("b391102d-e78b-4736-b6a6-959e5ce2586d", Caption = "Consignor")]
		public ZString ConsignorName => ConsignorDocAddress.CompanyName;

		public ZPropertyInfo ConsignorNameInfo => GetZPropertyInfo(nameof(ConsignorName));

		public JobDocAddress ConsignorDocAddress
		{
			get
			{
				if (fConsignorDocAddress == null || fConsignorDocAddress.IsDeleted)
				{
					fConsignorDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorDocAddressRequirement);
					fConsignorDocAddress.ReadOnly = IsPhase5;
				}

				return fConsignorDocAddress;
			}
		}
		JobDocAddress fConsignorDocAddress;

		public JobDocAddressRequirement ConsignorDocAddressRequirement
		{
			get
			{
				if (fConsignorDocAddressRequirement == null)
				{
					fConsignorDocAddressRequirement = GetJobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress);
					JobDocAddressManager.AddRequirement(fConsignorDocAddressRequirement);
				}
				return fConsignorDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fConsignorDocAddressRequirement;

		[ResourceStringData("3558a872-1533-4ca5-aa21-9e9f123a9a29", Caption = "Consignee")]
		public ZString ConsigneeName => ConsigneeDocAddress.CompanyName;

		public ZPropertyInfo ConsigneeNameInfo => GetZPropertyInfo(nameof(ConsigneeName));

		public JobDocAddress ConsigneeDocAddress
		{
			get
			{
				if (fConsigneeDocAddress == null || fConsigneeDocAddress.IsDeleted)
				{
					fConsigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeDocAddressRequirement);
					fConsigneeDocAddress.ReadOnly = IsPhase5;
				}

				return fConsigneeDocAddress;
			}
		}
		JobDocAddress fConsigneeDocAddress;

		public JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get
			{
				if (fConsigneeDocAddressRequirement == null)
				{
					fConsigneeDocAddressRequirement = GetJobDocAddressRequirement(DocAddressType.ConsigneeAddress);
					JobDocAddressManager.AddRequirement(fConsigneeDocAddressRequirement);
				}
				return fConsigneeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fConsigneeDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType docAddressType) => new JobDocAddressRequirement(docAddressType);

		public JobDocAddressManager JobDocAddressManager => jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager());

		JobDocAddressManager jobDocAddressManager;

		public new CusInBondMoveDetailLookups Lookups => (CusInBondMoveDetailLookups)base.Lookups;

		protected override Customs.Business.CusInBondMoveDetailLookups GetNewLookups() => new CusInBondMoveDetailLookups(this);

		public new CusInBondMoveDetailValidation Validation => (CusInBondMoveDetailValidation)base.Validation;

		protected override Customs.Business.CusInBondMoveDetailValidation GetNewValidation() => new CusInBondMoveDetailValidation(this);

		public NctsDepartureMovementHeader DepartureMoveHeader => base.MoveHeader as NctsDepartureMovementHeader;

		public NctsArrivalMovementHeader ArrivalMoveHeader => base.MoveHeader as NctsArrivalMovementHeader;

		public new NctsBill Bill => (NctsBill)base.Bill;

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("09EC8642-63E4-4D5B-8B4C-984396898C8B", Caption = "Transport ID")]
		public override ZString B9_TransportAtDepartureID
		{
			get => base.B9_TransportAtDepartureID;
			set => base.B9_TransportAtDepartureID = value;
		}

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("CF329B49-9A2A-4B35-A591-9632A2F716BD", Caption = "Nationality")]
		public override ZString B9_RN_NKTransportAtDepartureIDNationality
		{
			get => base.B9_RN_NKTransportAtDepartureIDNationality;
			set => base.B9_RN_NKTransportAtDepartureIDNationality = value;
		}

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("81C5AC83-376A-48E6-A1A2-129DC9CCF9A7", Caption = "Trailer ID 1")]
		public override ZString B9_TransportAtDepartureTrailer1RegNo
		{
			get => base.B9_TransportAtDepartureTrailer1RegNo;
			set => base.B9_TransportAtDepartureTrailer1RegNo = value;
		}

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("75B898D4-973A-4C4A-9693-8C0A09B5CF87", Caption = "Nationality")]
		public override ZString B9_RN_NKTransportAtDepartureTrailer1Nationality
		{
			get => base.B9_RN_NKTransportAtDepartureTrailer1Nationality;
			set => base.B9_RN_NKTransportAtDepartureTrailer1Nationality = value;
		}

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("17FAD0B1-F3EC-4A9F-8089-8BAAD08B2AE9", Caption = "Trailer ID 2")]
		public override ZString B9_TransportAtDepartureTrailer2RegNo
		{
			get => base.B9_TransportAtDepartureTrailer2RegNo;
			set => base.B9_TransportAtDepartureTrailer2RegNo = value;
		}

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("F70AB7F0-E39B-4D6D-BFDB-67C47BF8AF78", Caption = "Nationality")]
		public override ZString B9_RN_NKTransportAtDepartureTrailer2Nationality
		{
			get => base.B9_RN_NKTransportAtDepartureTrailer2Nationality;
			set => base.B9_RN_NKTransportAtDepartureTrailer2Nationality = value;
		}

		[ReadOnlyMember(nameof(InBondDetailsReadOnly))]
		[ResourceStringData("51DFCCCC-79F5-4D13-BAF4-605F1849D980", Caption = "Aircraft ID")]
		public override ZString B9_AircraftIDAtDeparture
		{
			get => base.B9_AircraftIDAtDeparture;
			set => base.B9_AircraftIDAtDeparture = value;
		}

		[ResourceStringData("4A7ECC60-C383-4CA3-B54F-ECC71B161A72", Caption = "Vessel")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.Vessels))]
		public virtual ZString VesselNameAtDeparture
		{
			get => B9_TransportAtDepartureID;
			set => B9_TransportAtDepartureID = value;
		}

		public ZPropertyInfo VesselNameAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselNameAtDeparture), x => B9_TransportAtDepartureIDInfo);

		[ReadOnlyMember(nameof(B9_UnloadedStateReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.UnloadedStatesList))]
		[ResourceStringData("29767f2a-b271-45c6-b064-03da073ed4cf", Caption = "Unloaded State", ShortCaption = "State")]
		public override ZString B9_UnloadedState
		{
			get => base.B9_UnloadedState;
			set
			{
				var previousValue = B9_UnloadedState;
				base.B9_UnloadedState = value;
				if (previousValue != value && !IsCopying)
				{
					Header?.Bills.RefreshMaxCountValidation();
					HandleUnloadedStateChange(previousValue, B9_UnloadedState);

					UpdateArrivalGoodsItemsUnloadedStateForMIS(previousValue);

					B9_UnloadedStateInfo.RefreshBinding();
				}
			}
		}

		void UpdateArrivalGoodsItemsUnloadedStateForMIS(ZString previousValue)
		{
			if (previousValue == NctsUnloadedStateListForHouseConsignment.Codes.MIS)
			{
				UpdateArrivalGoodsItemsUnloadedState(NctsUnloadedStateListForHouseConsignment.Codes.DEC);
			}
			else if (B9_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.MIS)
			{
				UpdateArrivalGoodsItemsUnloadedState(NctsUnloadedStateListForHouseConsignment.Codes.MIS);
			}

			void UpdateArrivalGoodsItemsUnloadedState(ZString unloadedState)
			{
				foreach (var item in Bill.ArrivalGoodsItems)
				{
					if (item.BY_UnloadedState != NctsUnloadedStateListForHouseConsignment.Codes.NEW)
					{
						item.BY_UnloadedState = unloadedState;
					}
				}
			}
		}

		public ZDecimal DifferenceWeight
		{
			get
			{
				var result = ZDecimal.Zero;
				if (!B9_B9_InBondMoveDetail.IsEmpty)
				{
					result = Bill.ArrivalGoodsItems.Where(x => x.BY_UnloadedState != NctsUnloadedStateListForHouseConsignment.Codes.MIS)
					.Cast<NctsArrivalCargoDesc>()
					.Sum(x => x.BY_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.DIF ? x.UnloadedGoodsItem.GrossMassInKilograms : x.GrossMassInKilograms);
					result = new ZWeight(result, Core.Constants.Weight.Kilograms).ConvertTo(DifferenceWeightUnit);
				}
				return result;
			}
		}

		public ZPropertyInfo DifferenceWeightInfo => GetZPropertyInfo(Schema.DifferenceWeight);

		public ZString DifferenceWeightUnit => Bill.B0_WeightUQ;

		public ZPropertyInfo DifferenceWeightUnitInfo => GetZPropertyInfo(Schema.DifferenceWeightUnit);

		bool B9_UnloadedStateReadOnly => IsPhase5 && (B9_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.NEW || IsUnloadingRemarksReadOnly);

		bool InBondDetailsReadOnly => IsPhase5 && ((!B9_UnloadedState.IsEmpty && B9_UnloadedState != NctsUnloadedStateListForHouseConsignment.Codes.NEW) || IsUnloadingRemarksReadOnly);

		public bool IsRoad => Header.ArrivalMovementHeader?.BM_InlandTransportMode.Equals(ModeOfTransportList.Codes._3_RoadTransport) ?? false;

		public bool IsAir => Header.ArrivalMovementHeader?.BM_InlandTransportMode.Equals(ModeOfTransportList.Codes._4_AirTransport) ?? false;

		void HandleUnloadedStateChange(string previousValue, string newValue)
		{
			if (IsPhase5 && !B9_B0.IsEmpty)
			{
				var valueChanged = newValue != previousValue;
				if (valueChanged && newValue == NctsUnloadedStateListForHouseConsignment.Codes.DIF)
				{
					_ = DifferenceMoveDetailCollection.AddNew(GetType());
				}

				if (previousValue == NctsUnloadedStateListForHouseConsignment.Codes.DIF && (newValue == NctsUnloadedStateListForHouseConsignment.Codes.DEC || newValue == NctsUnloadedStateListForHouseConsignment.Codes.MIS))
				{
					DifferenceMoveDetailCollection.RemoveAndDeleteAll();
				}
			}
		}

		[ChildEditable]
		public CusInBondDifferenceMoveDetailCollection DifferenceMoveDetailCollection
		{
			get
			{
				if (differenceMoveDetailCollection is null)
				{
					differenceMoveDetailCollection = new CusInBondDifferenceMoveDetailCollection(this);
					differenceMoveDetailCollection.Load();
					RegisterEditableChildObject(differenceMoveDetailCollection);
				}
				return differenceMoveDetailCollection;
			}
		}
		CusInBondDifferenceMoveDetailCollection differenceMoveDetailCollection;

		NctsHeader Header => (NctsHeader)MoveHeader?.Header;

		internal bool IsPhase5 => Header?.IsPhase5 ?? false;

		internal bool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;

		public CusInBondMoveDetail DifferenceMoveDetail => (CusInBondMoveDetail)DifferenceMoveDetailCollection.ElementAtOrDefault(0);

		public bool IsUnloadingRemarksReadOnly => IsPhase5 && (ArrivalMoveHeader?.IsUnloadingRemarksReadOnly ?? true);

		public bool AreUnloadingRemarksFullyAccepted => IsPhase5 && (ArrivalMoveHeader?.AreUnloadingRemarksFullyAccepted ?? false);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		}
	}
}
