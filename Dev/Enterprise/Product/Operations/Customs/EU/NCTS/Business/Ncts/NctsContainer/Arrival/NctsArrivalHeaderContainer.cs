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
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalHeaderContainer : NctsCusInBondContainer, IShortSequenceNumberLine, ISequenceNumberHeader, ICusSealTypeSupporter
	{
		public NctsArrivalHeaderContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondContainer.Schema
		{
			public new const int BC_ContainerNumMaxLength = 17;
		}

		[RelatedBusinessObject(nameof(NctsArrival))]
		public override ZGuid BC_ParentID
		{
			get => base.BC_ParentID;
			set => base.BC_ParentID = value;
		}

		[ResourceStringData("NctsArrivalHeaderContainer.BC_SequenceNumber", Caption = "Sequence Number", MediumCaption = "Sequence No", ShortCaption = "Seq.No.")]
		[ReadOnly(true)]
		public override ZShort BC_SequenceNumber { get => base.BC_SequenceNumber; set => base.BC_SequenceNumber = value; }

		[ResourceStringData("NctsArrivalHeaderContainer.BC_UnloadedState", Caption = "Unloaded State", MediumCaption = "Unloaded State", ShortCaption = "State")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalHeaderContainerLookups.UnloadedStates))]
		[ReadOnlyMember(nameof(UnloadedStateReadOnly))]
		public override ZString BC_UnloadedState
		{
			get => base.BC_UnloadedState;
			set => base.BC_UnloadedState = value;
		}

		[ResourceStringData("NctsArrivalHeaderContainer.BC_ContainerNum[Phase4]", Caption = "Container Number", ShortCaption = "Container", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("NctsArrivalHeaderContainer.BC_ContainerNum[Phase5]", Caption = "Container/Equipment Number", MediumCaption = "Container/Equipment No.", ShortCaption = "Container/Equipment", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[MaxLength(Schema.BC_ContainerNumMaxLength)]
		[ReadOnlyMember(nameof(ContainerInfoReadOnly))]
		public override ZString BC_ContainerNum
		{
			get => base.BC_ContainerNum;
			set => base.BC_ContainerNum = value;
		}

		[ResourceStringData("NctsArrivalHeaderContainer.BC_Mode", Caption = "Container/Equipment Mode", MediumCaption = "Container Mode", ShortCaption = "Mode")]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalHeaderContainerLookups.CargoIdTypeList))]
		[ReadOnlyMember(nameof(ContainerInfoReadOnly))]
		public override ZString BC_Mode
		{
			get => base.BC_Mode;
			set => base.BC_Mode = value;
		}

		[ResourceStringData("NctsArrivalHeaderContainer.TotalSealCount", Caption = "Seal Quantity", MediumCaption = "Seal Qty.", ShortCaption = "Seal Qty.")]
		[ReadOnly(true)]
		public ZInt TotalSealCount => Factory.GetValue(ref cachedTotalSealCount, GetTotalSealCount);
		CachedProperty<ZInt> cachedTotalSealCount;

		public ZPropertyInfo TotalSealCountInfo => GetZPropertyInfo(nameof(TotalSealCount));

		ZInt GetTotalSealCount()
		{
			return Seals.Cast<CusSeal>().Count(seal => !string.Equals(seal.BK_UnloadingState, NctsUnloadedStateList.Codes.MIS, StringComparison.OrdinalIgnoreCase));
		}

		public bool IsContainerised => BC_Mode == Core.Constants.ContainerModes.Containerised;

		protected bool ContainerInfoReadOnly => BC_UnloadedState.Equals(NctsUnloadedStateList.Codes.DEC) || BC_UnloadedState.Equals(NctsUnloadedStateList.Codes.MIS) || BC_UnloadedState.IsEmpty || IsUnloadingRemarksReadOnly;

		[ChildEditable]
		public CusSealCollection Seals
		{
			get
			{
				if (additionalSeals == null)
				{
					additionalSeals = GetAdditionalSealsCore();
					additionalSeals.Load();
					RegisterEditableChildObject(additionalSeals);
					additionalSeals.CountChanged += (o, e) =>
					{
						TotalSealCountInfo.RefreshBinding();
					};
					additionalSeals.HasChangesChanged += (o, e) =>
					{
						TotalSealCountInfo.RefreshBinding();
					};
					EnableOrDisableSealsEdit();
				}
				return additionalSeals;
			}
		}
		CusSealCollection additionalSeals;

		public IEnumerable<CusSeal> SealsForMessaging => Seals.Where(s => s.BK_UnloadingState != NctsUnloadedStateList.Codes.DAM);

		Type ICusSealTypeSupporter.CusSealType => CusSealTypeCore;

		protected virtual Type CusSealTypeCore => typeof(CusSeal);
		public ShortSequenceNumberGenerator SealsLineNumberGenerator => sealsLineNumberGenerator ??= SealsLineNumberGeneratorCore;
		protected virtual ShortSequenceNumberGenerator SealsLineNumberGeneratorCore => new ShortSequenceNumberGenerator(() => Seals.Cast<IShortSequenceNumberLine>());
		ShortSequenceNumberGenerator sealsLineNumberGenerator;

		public void EnableOrDisableSealsEdit() => EnableOrDisableSealsEditCore();

		protected virtual void EnableOrDisableSealsEditCore()
		{
			if (NctsArrival != null)
			{
				var arrivalMovementHeader = NctsArrival.IsPhase5Arrival ? NctsArrival.ArrivalMovementHeader : null;
				if (arrivalMovementHeader != null)
				{
					Seals.SetReadOnlyIncludingChildren(arrivalMovementHeader.BM_StateOfSealsBoolean);
				}
			}
		}

		protected bool UnloadedStateReadOnly => BC_UnloadedState.Equals(NctsUnloadedStateList.Codes.NEW) || IsUnloadingRemarksReadOnly;

		public new NctsArrivalHeaderContainerLookups Lookups => (NctsArrivalHeaderContainerLookups)base.Lookups;

		protected override CusInBondContainerLookups GetNewLookups()
		{
			return new NctsArrivalHeaderContainerLookups(this);
		}

		protected virtual CusSealCollection GetAdditionalSealsCore()
		{
			return new CusSealCollection(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BC_TypeOfService = ContainerTypeOfServiceList.Codes.DepartureContainer;
			BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		}

		public NctsHeader NctsArrival => Factory.Load<NctsHeader>(BC_ParentID);

		protected override bool SupportsCloneCore() => true;

		public override void Delete()
		{
			Seals.RemoveAndDeleteAll();
			base.Delete();
		}

		public override bool CanDelete => !(NctsArrival.IsPhase5Arrival && (!UnloadedStateReadOnly || !NctsHelper.UnloadedStateInitiallyNew(BC_UnloadedStateInfo)));

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("1FB6F2FE-E7C6-45B8-838D-E961A3615DFF", "Cannot delete containers which were entered by customs");

		public new NctsArrivalHeaderContainerValidation Validation => (NctsArrivalHeaderContainerValidation)base.Validation;

		protected sealed override CusInBondContainerValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual CusInBondContainerValidation GetNewPhase4Validation() => new NctsArrivalHeaderContainerValidation(this);

		protected virtual CusInBondContainerValidation GetNewPhase5Validation() => new NctsArrivalHeaderContainerPhase5Validation(this);

		public bool IsPhase5 => NctsArrival is NctsHeader header && header.IsPhase5;

		public ZShort SequenceNumber
		{
			get => BC_SequenceNumber;
			set => BC_SequenceNumber = value;
		}

		ZGuid ISequenceNumberLine.FKToHeader => BC_ParentID;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => Seals as IEnumerable<ISequenceNumberLine>;

		public bool IsUnloadingRemarksReadOnly => NctsArrival.ArrivalMovementHeader.IsUnloadingRemarksReadOnly;

		public bool AreUnloadingRemarksFullyAccepted => NctsArrival.ArrivalMovementHeader.AreUnloadingRemarksFullyAccepted;

		internal INctsArrivalHeaderContainerPhase5ValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsArrivalHeaderContainerPhase5ValidationDecider> validationDeciderCached;

		INctsArrivalHeaderContainerPhase5ValidationDecider GetValidationDecider() => (INctsArrivalHeaderContainerPhase5ValidationDecider)NctsArrival.Configuration.NctsContainerConfiguration.GetHeaderValidationDecider(NctsArrival);
	}
}
