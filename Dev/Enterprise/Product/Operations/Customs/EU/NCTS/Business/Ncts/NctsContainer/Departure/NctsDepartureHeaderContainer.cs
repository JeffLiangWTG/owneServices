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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class NctsDepartureHeaderContainer : NctsCusInBondContainer,
		ISynchableContainer,
		ISynchroniserReadOnlyMembersProvider,
		ICusSealTypeSupporter,
		Integration.Customs.EU.NCTS.INctsDepartureHeaderContainer,
		IShortSequenceNumberLine
	{
		public NctsDepartureHeaderContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondContainer.Schema
		{
			public new const int BC_ContainerNumMaxLength = 17;
		}

		internal INctsDepartureHeaderContainerPhase5ValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsDepartureHeaderContainerPhase5ValidationDecider> validationDeciderCached;

		INctsDepartureHeaderContainerPhase5ValidationDecider GetValidationDecider() => Header.Configuration.NctsContainerConfiguration.GetHeaderValidationDecider(Header) as INctsDepartureHeaderContainerPhase5ValidationDecider;

		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid BC_ParentID
		{
			get => base.BC_ParentID;
			set => base.BC_ParentID = value;
		}

		[BusinessObjectTestExclude]
		public override ZString BC_ParentTableCode { get => base.BC_ParentTableCode; set => base.BC_ParentTableCode = value; }

		[ResourceStringData("BFA676AA-6333-4A9B-9618-1E456911513A", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq.No.")]
		[ReadOnly(true)]
		public override ZShort BC_SequenceNumber { get => base.BC_SequenceNumber; set => base.BC_SequenceNumber = value; }

		[ReadOnlyMember(nameof(IsUnloadedStateReadOnly))]
		public override ZString BC_UnloadedState { get => base.BC_UnloadedState; set => base.BC_UnloadedState = value; }
		bool IsUnloadedStateReadOnly => IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC && NctsHelper.IsUnloadedStateAccepted(BC_UnloadedState);

		[ResourceStringData("NctsHeaderContainer.BC_ContainerNum[Phase4]", Caption = "Container Number", ShortCaption = "Container", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("NctsHeaderContainer.BC_ContainerNum[Phase5]", Caption = "Container/Equipment Number", MediumCaption = "Container/Equipment No.", ShortCaption = "Container/Equipment", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[MaxLength(Schema.BC_ContainerNumMaxLength)]
		[ReadOnlyMember(nameof(IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC))]
		public override ZString BC_ContainerNum
		{
			get => base.BC_ContainerNum;
			set => base.BC_ContainerNum = value;
		}

		public bool IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC => Header.IsNctsPhase5ArrivalCustomsStatusARTPhaseFRC || Header.MessageHasBeenSent;

		[ResourceStringData("NctsHeaderContainer.Seal1", Caption = "Seal 1 Number", ShortCaption = "Seal 1")]
		public ZString Seal1
		{
			get => base.BC_Seal1;
			set => base.BC_Seal1 = value;
		}

		public ZPropertyInfo Seal1Info => GetWrappedZPropertyInfo(nameof(Seal1), x => BC_Seal1Info);

		[ResourceStringData("NctsHeaderContainer.Seal2", Caption = "Seal 2 Number", ShortCaption = "Seal 2")]
		public ZString Seal2
		{
			get => base.BC_Seal2;
			set => base.BC_Seal2 = value;
		}

		public ZPropertyInfo Seal2Info => GetWrappedZPropertyInfo(nameof(Seal2), x => BC_Seal2Info);

		[ResourceStringData("NctsHeaderContainer.BC_Mode", Caption = "Container/Equipment Mode", MediumCaption = "Container Mode", ShortCaption = "Mode")]
		[List(nameof(Lookups) + "." + nameof(NctsDepartureHeaderContainerLookups.CargoIdTypeList))]
		public override ZString BC_Mode
		{
			get => base.BC_Mode;
			set
			{
				var oldValue = base.BC_Mode;
				base.BC_Mode = value;
				if (!IsCopying && oldValue != value)
				{
					Header?.DepartureHeaderContainers.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("NctsHeaderContainer.TotalSealCount", Caption = "Seal Quantity", MediumCaption = "Seal Qty.", ShortCaption = "Seal Qty.")]
		public ZInt TotalSealCount => AdditionalSeals.Count + (Seal1.IsEmpty ? 0 : 1) + (Seal2.IsEmpty ? 0 : 1);

		public ZPropertyInfo TotalSealCountInfo => GetZPropertyInfo(nameof(TotalSealCount));

		public bool IsContainerised => IsContainerisedCore;
		protected virtual bool IsContainerisedCore => BC_Mode == Core.Constants.ContainerModes.Containerised;

		[ChildEditable]
		public CusSealCollection AdditionalSeals
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
				}
				return additionalSeals;
			}
		}
		CusSealCollection additionalSeals;

		Type ICusSealTypeSupporter.CusSealType => CusSealTypeCore;

		protected virtual Type CusSealTypeCore => typeof(CusSeal);

		public IEnumerable<IShortSequenceNumberLine> AdditionalSealsLines => new TypedEnumerable<IShortSequenceNumberLine>(AdditionalSeals);

		public ShortSequenceNumberGenerator AdditionalSealsLineNumberGenerator => additionalSealsLineNumberGenerator ?? (additionalSealsLineNumberGenerator = AdditionalSealsLineNumberGeneratorCore);
		protected virtual ShortSequenceNumberGenerator AdditionalSealsLineNumberGeneratorCore => new ShortSequenceNumberGenerator(() => AdditionalSealsLines);
		ShortSequenceNumberGenerator additionalSealsLineNumberGenerator;

		public override void Delete()
		{
			this.DeleteChildren<CusSeal>(CusSealSchema.BK_ParentID);
			this.DeleteChildren<GenPivot>(GenPivotSchema.XX_Relation2ID);
			base.Delete();
		}

		public new NctsDepartureHeaderContainerValidation Validation => (NctsDepartureHeaderContainerValidation)base.Validation;

		public new NctsDepartureHeaderContainerLookups Lookups => (NctsDepartureHeaderContainerLookups)base.Lookups;

		protected override CusInBondContainerLookups GetNewLookups() => new NctsDepartureHeaderContainerLookups(this);

		protected virtual CusSealCollection GetAdditionalSealsCore() => new CusSealCollection(this);

		/// <exception cref="DeveloperNotificationException"> When requesting <see cref="NctsHeader"/> while not having valid <see cref="BC_ParentTableCode"/>.</exception>
		public NctsHeader Header
		{
			get
			{
				var bC_ParentTableCode = BC_ParentTableCode;
				if (bC_ParentTableCode.IsEmpty)
				{
					// no parent, no object. That is perfectly fine
					return null;
				}
				if (bC_ParentTableCode == CusInBondHeaderSchema.Constants.Prefix)
				{
					return Factory.Load<NctsHeader>(BC_ParentID);
				}

				throw new DeveloperNotificationException(
					$"On {nameof(NctsDepartureHeaderContainer)} expected parent table code to be '{CusInBondHeaderSchema.Constants.Prefix}', but was '{BC_ParentTableCode}'. Can't load {nameof(NctsHeader)} using this prefix");
			}
		}

		public NctsHeader NctsDeparture => Header;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BC_TypeOfService = ContainerTypeOfServiceList.Codes.DepartureContainer;
		}

		protected override bool SupportsCloneCore() => true;

		protected sealed override CusInBondContainerValidation GetNewValidation() => IsPhase5
			? GetNewPhase5Validation()
			: GetNewPhase4Validation();

		protected virtual CusInBondContainerValidation GetNewPhase5Validation() => new NctsDepartureHeaderContainerPhase5Validation(this);

		protected virtual CusInBondContainerValidation GetNewPhase4Validation() => new NctsDepartureHeaderContainerPhase4Validation(this);

		public bool IsPhase5 => Header is NctsHeader header && header.IsPhase5;

		public IReadOnlyList<ZString> AllSeals => AdditionalSeals.Select(x => x.BK_SealNumber).Append(Seal1).Append(Seal2).ToList();

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => BC_SequenceNumber; set => BC_SequenceNumber = value; }

		ZGuid ISequenceNumberLine.FKToHeader => BC_ParentID;

		#region ISynchableContainer members

		public bool IsNonContainerized => BC_ContainerNum == CusInBondContainer.NonContainerizedNumber;

		ZPropertyInfo ISynchableContainer.ContainerNumInfo => BC_ContainerNumInfo;

		ZPropertyInfo ISynchableContainer.Seal1Info => Seal1Info;

		bool ISynchableContainer.Seal2Supported => true;

		ZPropertyInfo ISynchableContainer.Seal2Info => Seal2Info;

		bool ISynchableContainer.Seal3Supported => false;

		ZPropertyInfo ISynchableContainer.Seal3Info => null;

		#endregion

		#region ISynchroniserReadOnlyMembersProvider members

		List<string> synchroniserReadOnlyMembers;

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		#endregion

		#region Type Decider

		[ThreadSafe]
		public new static readonly NctsDepartureHeaderContainerTypeDecider TypeDecider = new NctsDepartureHeaderContainerTypeDecider();

		#endregion

	}
}
