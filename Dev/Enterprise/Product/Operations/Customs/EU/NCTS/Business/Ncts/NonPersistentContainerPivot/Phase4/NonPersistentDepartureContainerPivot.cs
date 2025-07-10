using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentDepartureContainerPivot : AutoNonPersistentDepartureContainerPivot
	{
		public NonPersistentDepartureContainerPivot(NctsCommonCargoDesc line)
			: base(line.Factory)
		{
			this.line = line;
		}

		public new class Schema : AutoNonPersistentDepartureContainerPivot.Schema
		{
			public const string ContainerSelected = nameof(NonPersistentDepartureContainerPivot.ContainerSelected);
			public const string ContainerType = nameof(NonPersistentDepartureContainerPivot.ContainerType);
			public const string ContainerMode = nameof(NonPersistentDepartureContainerPivot.ContainerMode);
		}

		public NctsDepartureHeaderContainer Container
		{
			get => container == null || container.IsDeleted ? null : container;
			set => container = value;
		}
		NctsDepartureHeaderContainer container;

		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerNumber", Caption = "Container", ShortCaption = "Cont.")]
		[ReadOnly(true)]
		public override ZString ContainerNumber
		{
			get => container?.BC_ContainerNum ?? ZString.Empty;
			set
			{
				if (container != null && container.BC_ContainerNum != value)
				{
					CheckMaximumLength(ContainerNumberInfo, value);
					container.BC_ContainerNum = value;
					ContainerNumberInfo.RefreshBinding();
				}
			}
		}

		[List(nameof(Containers))]
		[ReadOnly(true)]
		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerType", Caption = "Type")]
		public ZGuid ContainerType
		{
			get => container?.BC_RC ?? ZGuid.Empty;
			set
			{
				if (container != null && container.BC_RC != value)
				{
					container.BC_RC = value;
					ContainerTypeInfo.RefreshBinding();
				}
			}
		}

		public RefContainer RefContainer => container?.Container;

		public ZPropertyInfo ContainerTypeInfo => GetZPropertyInfo(Schema.ContainerType);

		public RefContainerCollection Containers => Container.Lookups.Containers;

		[MaxLength(CusInBondContainer.Schema.BC_ModeMaxLength)]
		[ReadOnly(true)]
		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerMode", Caption = "Mode")]
		public ZString ContainerMode
		{
			get => container?.BC_Mode ?? ZString.Empty;
			set
			{
				if (container != null && container.BC_Mode != value)
				{
					CheckMaximumLength(ContainerModeInfo, value);
					container.BC_Mode = value;
					ContainerModeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ContainerModeInfo => GetZPropertyInfo(Schema.ContainerMode);

		[ResourceStringData("EU.NCTS.Business.NonPersistentContainerPivot.ContainerSelected", Caption = "Selected", ShortCaption = "Sel.")]
		[ReadOnlyMember(nameof(IsPhase5ArrivalNotNew))]
		public virtual ZBool ContainerSelected
		{
			get => HasPivot;
			set
			{
				if (Container != null && HasPivot != value)
				{
					if (value)
					{
						AddContainerSelectedPivot();
					}
					else
					{
						DeleteContainerSelectedPivot();
					}

					Container.MarkAsNeedingValidation();
					ContainerSelectedInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ContainerSelectedInfo => GetZPropertyInfo(Schema.ContainerSelected);

		ZBool HasPivot => ContainerSelectedPivot != null;

		protected bool IsPhase5ArrivalNotNew => line.Header != null && (line.Header.IsPhase5 && line.Header.IsArrivalMovement && line.BY_UnloadedState != NctsUnloadedStateList.Codes.NEW);

		void AddContainerSelectedPivot()
		{
			var pivot = ContainerSelectedPivot;
			if (pivot == null)
			{
				var genPivot = Factory.New<GenPivot>();
				genPivot.XX_RelationType = Core.Constants.GenPivotTypes.CusNctsContainer;
				genPivot.XX_Relation1ID = line.PK;
				genPivot.XX_Relation2ID = container.PK;
				genPivot.XX_Relation1TableCode = line.TablePrefix;
				genPivot.XX_Relation2TableCode = container.TablePrefix;
			}
		}

		void DeleteContainerSelectedPivot()
		{
			ContainerSelectedPivot?.Delete();
		}

		internal GenPivot ContainerSelectedPivot
		{
			get
			{
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.CusNctsContainer);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, line.PK);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, container.PK);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, line.TablePrefix);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, container.TablePrefix);
				return line.Factory.LoadTop1<GenPivot>(pivotQuery);
			}
		}

		readonly NctsCommonCargoDesc line;
	}
}
