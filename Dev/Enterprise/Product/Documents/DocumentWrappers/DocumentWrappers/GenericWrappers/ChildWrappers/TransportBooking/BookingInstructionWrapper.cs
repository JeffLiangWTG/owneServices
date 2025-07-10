using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Wraps a Booking Instruction
	/// Documents group by the Instruction (Sequence), so create multiple for each Confirmation and Package
	/// </summary>
	public sealed class BookingInstructionWrapper : InstructionWrapper
	{
		#region Constructors

		public BookingInstructionWrapper(DtbBookingInstruction groupedInstruction, BusinessObjectFactory factory)
			: base(groupedInstruction ?? factory.GetNull<DtbBookingInstruction>(), null, factory)
		{
		}

		public BookingInstructionWrapper(DtbBookingInstruction groupedInstruction, DtbBookingInstructionPkgDivot perPackageDivot, DtbBookingConfirmation perBookingConfirmation, BusinessObjectFactory factory)
			: base(groupedInstruction ?? factory.GetNull<DtbBookingInstruction>(), perBookingConfirmation ?? factory.GetNull<DtbBookingConfirmation>(), factory)
		{
			PackageDivot = perPackageDivot ?? factory.GetNull<DtbBookingInstructionPkgDivot>();
		}

		readonly DtbBookingInstructionPkgDivot PackageDivot;

		#endregion

		#region Instruction Properties

		#region GetTimeIn

		protected override ZDateTime GetTimeIn()
		{
			return DtbBookingConfirmationBO != null ? DtbBookingConfirmationBO.KK_Actual : ZDateTime.Empty;
		}

		#endregion

		#region GetTimeOut

		protected override ZDateTime GetTimeOut()
		{
			return DtbBookingConfirmationBO != null ? DtbBookingConfirmationBO.KK_Actual : ZDateTime.Empty;
		}

		#endregion

		#region Address

		protected override AddressWrapper GetAddress()
		{
			return new AddressWrapper(BookingInstructionBO.Address, Factory);
		}

		#endregion

		#region DropMode

		protected override CodeAndDescriptionWrapper GetDropMode()
		{
			return new CodeAndDescriptionWrapper(BookingInstructionBO.KN_DropMode, BookingInstructionBO.Lookups.DropModes, Factory);
		}

		#endregion

		#region InstructionType

		protected override ZString GetInstructionType()
		{
			return BookingInstructionBO.Lookups.InstructionTypes.GetDescriptionFromCode(BookingInstructionBO.KN_InstructionType);
		}

		#endregion

		#region Status

		protected override ZString GetStatus()
		{
			return BookingInstructionBO.KN_Status;
		}

		#endregion

		#region ServiceInstruction

		protected override ZString GetServiceInstruction()
		{
			return ServiceInstructionHelper.GetHandlingInstructionsWithResultAppended(BookingInstructionBO.Address.Organisation, BookingInstructionBO.KN_ServiceInstruction);
		}

		#endregion

		#region Equipment

		protected override ZString GetEquipment()
		{
			return BookingInstructionBO.Equipment != null ? BookingInstructionBO.Equipment.RQ_DescriptionMultilingual : ZString.Empty;
		}

		#endregion

		#region Sequence

		protected override ZInt GetSequence()
		{
			return BookingInstructionBO.KN_Sequence;
		}

		#endregion

		#endregion

		#region Confirmation Properties

		#region RequiredFrom

		protected override LabelValuePairWrapper GetRequiredFromLabelValuePairCore()
		{
			return new LabelValuePairWrapper(DtbBookingConfirmationBO.RequiredFromLabel, DtbBookingConfirmationBO.KK_RequiredFrom, Factory);
		}

		#endregion

		#region RequiredTo

		protected override LabelValuePairWrapper GetRequiredToLabelValuePairCore()
		{
			return new LabelValuePairWrapper(DtbBookingConfirmationBO.RequiredToLabel, DtbBookingConfirmationBO.KK_RequiredTo, Factory);
		}

		#endregion

		DtbBookingConfirmation DtbBookingConfirmationBO
		{
			get { return (DtbBookingConfirmation)ActionBO; }
		}

		#endregion

		#region Package Properties / Per Package Divot

		#region GetPackageDivotQuantity

		protected override ZInt GetPackageDivotQuantity()
		{
			return PackageDivot != null ? PackageDivot.KD_Quantity : ZInt.Zero;
		}

		#endregion

		#region GetPackageType

		protected override ZString GetPackageType()
		{
			ZString result = ZString.Empty;
			if (PackageDivot != null)
			{
				PkgPackage package = PackageDivot.Package;
				if (package != null)
				{
					if (package.IsContainer && package.Container != null && package.Container.ContainerType != null)
					{
						result = package.Container.ContainerType.RC_Code;
					}
					else
					{
						result = package.KP_F3_NKPackType;
					}
				}
			}
			return result;
		}

		#endregion

		#region GetPackageID

		protected override ZString GetPackageID()
		{
			return (PackageDivot != null) ? PackageDivot.Package.KP_PackageID : ZString.Empty;
		}

		#endregion

		#region GetPackageDivotID

		protected override ZString GetPackageDivotID()
		{
			return (PackageDivot != null)
				? ZString.Format((NoResString)"{0}x {1} {2}", PackageDivot.KD_Quantity, PackageType, PackageID)
				: PackageID;
		}

		#endregion

		#region GetPackageDimensions

		protected override ZString GetPackageDimensions()
		{
			var result = ZString.Empty;

			if (PackageDivot != null)
			{
				var package = PackageDivot.Package;
				if (package.IsContainer)
				{
					var containerType = PackageDivot.Package.Container.ContainerType;
					result = ZString.Format("{0} - {1}", containerType.RC_Code, containerType.RC_DescriptionMultilingual);
				}
				else
				{
					result = package.ToStringDimensionSummary();
				}
			}

			return result;
		}

		#endregion

		#region GetHasSingleConfirmationForWholePackage()

		protected override ZBool GetHasSingleConfirmationForWholePackage()
		{
			return
				PackageDivot != null &&
				ActionBO != null &&
				PackageDivot.Confirmations.Count == 1 &&
				PackageDivot.KD_Quantity == ActionBO.ActionQuantity;
		}

		#endregion

		#region GetPackageDivotSequence

		// This property used for GroupBy on template.
		protected override ZInt GetPackageDivotSequence()
		{
			ZInt result = 0;
			if (PackageDivot != null)
			{
				result = BookingInstructionBO.PackageDivots.IndexOf(PackageDivot);
			}
			return result;
		}

		#endregion

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			return (PackageDivot != null)
				? new WeightWrapper(PackageDivot.Package.KP_Weight, PackageDivot.Package.KP_WeightUQ, 1, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory)
				: WeightWrapper.Empty;
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			return (PackageDivot != null)
				? new VolumeWrapper(PackageDivot.Package.KP_Volume, PackageDivot.Package.KP_VolumeUQ, new CodeDescriptionPairList(OLookUpEditType.Volume), Factory)
				: VolumeWrapper.Empty;
		}

		#endregion

		#region GetUNDGsSummary

		protected override ZString GetUNDGsSummary()
		{
			return ZString.Empty; // not implemented
		}

		#endregion

		#endregion

		#region Implementation

		DtbBookingInstruction BookingInstructionBO
		{
			get { return (DtbBookingInstruction)WrappedBO; }
		}

		#endregion
	}
}
