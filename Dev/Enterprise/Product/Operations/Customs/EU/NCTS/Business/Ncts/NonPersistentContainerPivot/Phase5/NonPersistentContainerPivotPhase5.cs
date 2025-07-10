using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentContainerPivotPhase5 : AutoNonPersistentContainerPivotPhase5
	{
		public NonPersistentContainerPivotPhase5(NctsPackage package, NctsCusInBondContainer container)
			: base(package.Factory)
		{
			this.package = package;
			this.Container = Argument.NotNull(container, nameof(container));
		}

		public new class Schema : AutoNonPersistentContainerPivotPhase5.Schema
		{
			public const string ContainerType = nameof(NonPersistentContainerPivotPhase5.ContainerType);
			public const string ContainerMode = nameof(NonPersistentContainerPivotPhase5.ContainerMode);
		}

		public readonly NctsCusInBondContainer Container;

		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerNumber", Caption = "Container", ShortCaption = "Cont.")]
		public override ZString ContainerNumber => Container.BC_ContainerNum;

		public override ZPropertyInfo ContainerNumberInfo => GetWrappedZPropertyInfo(Schema.ContainerNumber, (x) => Container.BC_ContainerNumInfo);

		[List(nameof(Containers))]
		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerType", Caption = "Type")]
		public ZGuid ContainerType => Container.BC_RC;

		public RefContainer RefContainer => Container.Container;

		public ZPropertyInfo ContainerTypeInfo => GetWrappedZPropertyInfo(Schema.ContainerType, (x) => Container.BC_RCInfo);

		public RefContainerCollection Containers => Container.Lookups.Containers;

		[MaxLength(CusInBondContainer.Schema.BC_ModeMaxLength)]
		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerMode", Caption = "Mode")]
		public ZString ContainerMode
		{
			get => Container.BC_Mode;
		}

		public ZPropertyInfo ContainerModeInfo => GetWrappedZPropertyInfo(Schema.ContainerMode, (x) => Container.BC_ModeInfo);

		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerSelected", Caption = "Selected", ShortCaption = "Sel.")]
		[ReadOnlyMember(nameof(ContainerSelected_ReadOnly))]
		public override ZBool ContainerSelected
		{
			get => HasPivot;
			set
			{
				if (HasPivot != value)
				{
					var pivot = package.ToggleLinkageWithContainer(Container, value);
					Container.MarkAsNeedingValidation();
					ContainerSelectedInfo.RefreshBinding((ZBool)!value);
				}
			}
		}

		ZBool HasPivot => ContainerSelectedPivot != null;

		protected virtual bool ContainerSelected_ReadOnly
		{
			get
			{
				var readOnly = ContainerSelectedPivot is GenPivot pivot && pivot.XX_Relation2IDInfo.ReadOnly;
				if (!readOnly)
				{
					var goodsItem = package.Parent;

					if (goodsItem.Header is NctsHeader header && header.IsPhase5Arrival)
					{
						if (package.B5_TypeOfDifference != NctsUnloadedStateList.Codes.NEW
							&& !EditableUnloadedStates.Contains(Container.BC_UnloadedState)
							&& !EditableUnloadedStates.Contains(goodsItem.BY_UnloadedState))
						{
							readOnly = true;
						}
					}
				}
				return readOnly;
			}
		}

		static readonly ImmutableHashSet<string> EditableUnloadedStates = ImmutableHashSet.Create(
			NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF);

		public NctsCusInBondContainerPackageGenPivot ContainerSelectedPivot
		{
			get { return Container != null ? package.ContainersPivot.GetRelatedPivot(Container) : null; }
		}

		protected readonly NctsPackage package;
	}
}
