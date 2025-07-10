using System.Collections.Generic;
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
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusSeal : Customs.Business.CusSeal, IShortSequenceNumberLine, ISynchroniserReadOnlyMembersProvider
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static new readonly CusSealTypeDecider TypeDecider = new CusSealTypeDecider();

		[ResourceStringData("0CACE2CF-C6B7-4132-BAA4-1B6978662747", Caption = "Seal Number", MediumCaption = "Seal Number", ShortCaption = "Seal No.")]
		[ReadOnlyMember(nameof(SealNumberReadOnly))]
		public override ZString BK_SealNumber { get => base.BK_SealNumber; set => base.BK_SealNumber = value; }

		[ResourceStringData("3FA79EFB-73A5-4F0E-99FE-99DD0420BE37", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq.No.")]
		[ReadOnly(true)]
		public override ZShort BK_SequenceNumber { get => base.BK_SequenceNumber; set => base.BK_SequenceNumber = value; }

		[ResourceStringData("239FDE56-976B-4653-A5D3-29E6CFFFC927", Caption = "Unloaded State", MediumCaption = "Unloaded State", ShortCaption = "State")]
		[List(nameof(Lookups) + "." + nameof(CusSealLookups.UnloadedStates))]
		[ReadOnlyMember(nameof(UnloadedStateReadOnly))]
		public override ZString BK_UnloadingState
		{
			get => base.BK_UnloadingState;
			set => base.BK_UnloadingState = value;
		}

		public bool UnloadedStateReadOnly
		{
			get
			{
				if (Parent is NctsDepartureHeaderContainer departureContainer)
				{
					return departureContainer.IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC ? NctsHelper.IsUnloadedStateAccepted(BK_UnloadingState) || departureContainer.Header.MessageHasBeenSent : BK_UnloadingState.Equals(NctsUnloadedStateList.Codes.NEW) || BK_UnloadingState.IsEmpty;
				}
				if (Parent is NctsArrivalHeaderContainer arrivalContainer)
				{
					return arrivalContainer.IsUnloadingRemarksReadOnly || BK_UnloadingState.Equals(NctsUnloadedStateList.Codes.NEW);
				}
				return BK_UnloadingState.Equals(NctsUnloadedStateList.Codes.NEW);
			}
		}

		public bool SealNumberReadOnly =>
			   BK_UnloadingState.Equals(NctsUnloadedStateList.Codes.MIS)
			|| BK_UnloadingState.Equals(NctsUnloadedStateList.Codes.DEC)
			|| BK_UnloadingState.Equals(NctsUnloadedStateList.Codes.DAM)
			|| BK_UnloadingState.IsEmpty
			|| (Parent is NctsDepartureHeaderContainer container && container.IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC)
			|| (Parent is NctsArrivalHeaderContainer arrivalContainer && arrivalContainer.IsUnloadingRemarksReadOnly)
			|| IsArrivalNotificationDisabled;

		public new CusSealLookups Lookups => (CusSealLookups)base.Lookups;

		public new CusSealValidation Validation => (CusSealValidation)base.Validation;

		public BusinessObject Parent => Factory.Load(ParentType, BK_ParentID);

		public NctsHeader Header
		{
			get
			{
				NctsHeader header = null;
				if (Parent is BusinessObject sealParent)
				{
					switch (sealParent)
					{
						case NctsDepartureHeaderContainer departureContainer:
							header = departureContainer.Header;
							break;
						case NctsContainer nctsContainer:
							if (nctsContainer.Parent is EnRouteIncident incident)
							{
								header = incident.Header;
							}
							break;
						case NctsArrivalHeaderContainer parentArrivalHeaderContainer:
							header = parentArrivalHeaderContainer.NctsArrival;
							break;
					}
				}
				return header;
			}
		}

		protected override Customs.Business.CusSealLookups GetNewLookups()
		{
			return new CusSealLookups(this);
		}

		public bool IsPhase5 => Header is NctsHeader header && header.IsPhase5;

		protected sealed override Customs.Business.CusSealValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual Customs.Business.CusSealValidation GetNewPhase4Validation() => new CusSealValidation(this);

		protected virtual Customs.Business.CusSealValidation GetNewPhase5Validation() => new CusSealPhase5Validation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		}

		ZGuid ISequenceNumberLine.FKToHeader => BK_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => BK_SequenceNumber; set => BK_SequenceNumber = value; }

		internal System.Type ParentType { get; set; }

		public override bool CanDelete => !(Parent is NctsArrivalHeaderContainer container && container.NctsArrival.IsPhase5Arrival && (!UnloadedStateReadOnly || !NctsHelper.UnloadedStateInitiallyNew(BK_UnloadingStateInfo)));

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("D164E9A1-4EF1-456B-AD10-2E67A3816017", "Cannot delete seals which were entered by customs");

		public bool IsArrivalNotificationDisabled => Parent is NctsContainer container && container.IsArrivalNotificationDisabled;

		public ICusSealValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<ICusSealValidationDecider> validationDeciderCached;

		ICusSealValidationDecider GetValidationDecider() => Header?.Configuration.CusSealConfiguration.GetValidationDecider(Header);

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
	}
}
