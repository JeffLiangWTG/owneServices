using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ArrivalCusTransportMeans : CusTransportMeans, IShortSequenceNumberLine
	{
		public ArrivalCusTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void OnElementChanged()
		{
			base.OnElementChanged();
			RefreshParentBizObj();
		}

		protected override void OnElementReset()
		{
			base.OnElementReset();
			RefreshParentBizObj();
		}

		void RefreshParentBizObj()
		{
			if (Parent is NctsBill bill && bill.MovementDetail is CusInBondMoveDetail detail && !detail.ShouldValidateOnSave && !IsCopying && !IsValidationSuspended)
			{
				detail.MarkAsNeedingValidation();
			}
		}

		public IArrivalCusTransportMeansValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<IArrivalCusTransportMeansValidationDecider> validationDeciderCached;

		IArrivalCusTransportMeansValidationDecider GetValidationDecider() => MovementHeader?.Header.Configuration.ArrivalCusTransportMeansConfiguration().GetValidationDecider(this);

		[ReadOnly(true)]
		[ResourceStringData("052D4109-A598-4084-B66A-77056ECBA1B5", Caption = "Sequence", ShortCaption = "Seq.")]
		public override ZShort TPM_SequenceNumber
		{
			get => base.TPM_SequenceNumber;
			set => base.TPM_SequenceNumber = value;
		}

		[ResourceStringData("2E219848-4A95-4016-9F4C-8A4CF26F30D6", Caption = "Unloaded state", ShortCaption = "State")]
		[List(nameof(Lookups) + "." + nameof(ArrivalCusTransportMeansLookups.TransportStateList))]
		[ReadOnlyMember(nameof(UnloadedStateReadOnly))]
		public override ZString TPM_TransportState
		{
			get => base.TPM_TransportState;
			set => base.TPM_TransportState = value;
		}

		[List(nameof(Lookups) + "." + nameof(ArrivalCusTransportMeansLookups.TypeOfIdentificationList))]
		[ResourceStringData("60EB3D8D-1AAB-42D8-B83C-6B4CF4BFF07E", Caption = "Type of Identification", MediumCaption = "Type of ID", ShortCaption = "Type")]
		[ReadOnlyMember(nameof(UnloadedStateAndUnloadingRemarksFullyAcceptedReadOnly))]
		public override ZString TPM_TypeOfIdentification
		{
			get => base.TPM_TypeOfIdentification;
			set => base.TPM_TypeOfIdentification = value;
		}

		[ResourceStringData("5ABECBC5-D356-4921-9979-3D43196D2215", Caption = "Transport Identification", MediumCaption = "Transport ID", ShortCaption = "Transp. ID")]
		[ReadOnlyMember(nameof(UnloadedStateAndUnloadingRemarksFullyAcceptedReadOnly))]
		public override ZString TPM_IdentificationNumber
		{
			get => base.TPM_IdentificationNumber;
			set => base.TPM_IdentificationNumber = value;
		}

		[ResourceStringData("88D78D3C-37EF-4E15-8EF0-4ABF483968D1", Caption = "Nationality", ShortCaption = "Nat.")]
		[List(nameof(Lookups) + "." + nameof(ArrivalCusTransportMeansLookups.TransportNationalityList))]
		[ReadOnlyMember(nameof(UnloadedStateAndUnloadingRemarksFullyAcceptedReadOnly))]
		public override ZString TPM_RN_NKTransportNationality
		{
			get => base.TPM_RN_NKTransportNationality;
			set => base.TPM_RN_NKTransportNationality = value;
		}

		protected override CusTransportMeansValidation GetNewValidation() => new ArrivalCusTransportMeansValidation(this);

		public new ArrivalCusTransportMeansLookups Lookups => (ArrivalCusTransportMeansLookups)base.Lookups;

		protected override CusTransportMeansLookups GetNewLookups() => new ArrivalCusTransportMeansLookups(this);

		public ZShort SequenceNumber
		{
			get => TPM_SequenceNumber;
			set => TPM_SequenceNumber = value;
		}

		public ZGuid FKToHeader => TPM_ParentID;

		BusinessObject fParent;
		public BusinessObject Parent
		{
			get
			{
				if (fParent == null && !TPM_ParentTableCode.IsEmpty && !TPM_ParentID.IsEmpty)
				{
					fParent = Factory.Load(TPM_ParentTableCode, TPM_ParentID);
				}
				return fParent;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		}

		protected bool UnloadedStateAndUnloadingRemarksFullyAcceptedReadOnly => (TPM_TransportState == NctsUnloadedStateList.Codes.DEC || TPM_TransportState == NctsUnloadedStateList.Codes.MIS || TPM_TransportState.IsEmpty || IsUnloadingRemarksReadOnly);

		protected bool UnloadedStateReadOnly => (TPM_TransportState == NctsUnloadedStateList.Codes.NEW && TPM_TransportStateInfo.OriginalValue.ToString() == NctsUnloadedStateList.Codes.NEW) || IsUnloadingRemarksReadOnly;

		public bool IsUnloadingRemarksReadOnly => MovementHeader?.IsUnloadingRemarksReadOnly ?? false;

		public bool AreUnloadingRemarksFullyAccepted => MovementHeader?.AreUnloadingRemarksFullyAccepted ?? false;

		public NctsArrivalMovementHeader MovementHeader
		{
			get
			{
				if (Parent is NctsBill nctsBill)
				{
					return nctsBill.Header.ArrivalMovementHeader;
				}
				else if (Parent is NctsArrivalMovementHeader movementHeader)
				{
					return movementHeader;
				}

				return null;
			}
		}

		public override bool CanDelete => MovementHeader.Header.IsPhase5Arrival && TPM_TransportStateInfo.ReadOnly && base.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("FA512C46-D455-42F2-B934-513C9FACE974", "Cannot delete Transports which were entered by customs.");
	}
}
