using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Wraps a Run Sheet Instruction, Consignment Instruction or Booking Instruction
	/// Documents group by the Instruction (Sequence), so create multiple Instruction Wrappers for each Confirmation/Package
	/// </summary>
	public abstract class InstructionWrapper : GenericWrapper
	{
		protected InstructionWrapper(BusinessObject groupedInstruction, IConsignmentAction perAction, BusinessObjectFactory factory)
			: base(groupedInstruction, factory)
		{
			ActionBO = perAction;
		}

		protected InstructionWrapper(PkgPackage package, BusinessObjectFactory factory)
			: base(package, factory)
		{
		}

		internal readonly IConsignmentAction ActionBO;

		#region Instruction Properties

		#region Address

		public AddressWrapper Address
		{
			get { return address ?? (address = GetAddress()); }
		}
		AddressWrapper address;
		protected abstract AddressWrapper GetAddress();

		#endregion

		#region DropMode

		public CodeAndDescriptionWrapper DropMode
		{
			get { return GetDropMode(); }
		}
		protected abstract CodeAndDescriptionWrapper GetDropMode();

		#endregion

		#region InstructionType

		public ZString InstructionType
		{
			get { return GetInstructionType(); }
		}
		protected abstract ZString GetInstructionType();

		#endregion

		#region Status

		public ZString Status
		{
			get { return GetStatus(); }
		}
		protected abstract ZString GetStatus();

		#endregion

		#region ServiceInstruction

		public ZString ServiceInstruction
		{
			get { return GetServiceInstruction(); }
		}
		protected abstract ZString GetServiceInstruction();

		#endregion

		#region Equipment

		public ZString Equipment
		{
			get { return GetEquipment(); }
		}
		protected abstract ZString GetEquipment();

		#endregion

		#region Sequence

		public ZInt Sequence
		{
			get { return GetSequence(); }
		}
		protected abstract ZInt GetSequence();

		#endregion

		#region TimeIn

		public ZDateTime TimeIn
		{
			get { return GetTimeIn(); }
		}
		protected abstract ZDateTime GetTimeIn();

		#endregion

		#region TimeOut

		public ZDateTime TimeOut
		{
			get { return GetTimeOut(); }
		}
		protected abstract ZDateTime GetTimeOut();

		#endregion

		#endregion

		#region Confirmation Properties

		#region ConfirmationQuantity

		public ZInt ConfirmationQuantity
		{
			get { return (ActionBO != null) ? ActionBO.ActionQuantity : ZInt.Zero; }
		}

		#endregion

		#region EstimatedOrSlot

		public ZDateTime EstimatedOrSlot
		{
			get
			{
				var result = ZDateTime.Empty;
				if (ActionBO != null)
				{
					result = ActionBO.Estimated;
				}
				return result;
			}
		}

		#endregion

		#region RequiredFrom

		public LabelValuePairWrapper RequiredFrom
		{
			get
			{
				if (ActionBO != null)
				{
					requiredFrom = GetRequiredFromLabelValuePairCore();
				}
				else if (requiredFrom == null)
				{
					requiredFrom = LabelValuePairWrapper.Empty;
				}
				return requiredFrom;
			}
		}

		protected virtual LabelValuePairWrapper GetRequiredFromLabelValuePairCore()
		{
			return new LabelValuePairWrapper(Res.GetString("261c2cc3-0432-453e-bd86-fef1e66e0e9f", "Required From"), ActionBO.RequiredFrom, Factory);
		}

		LabelValuePairWrapper requiredFrom;

		#endregion

		#region RequiredTo

		public LabelValuePairWrapper RequiredTo
		{
			get
			{
				if (ActionBO != null)
				{
					requiredTo = GetRequiredToLabelValuePairCore();
				}
				else if (requiredTo == null)
				{
					requiredTo = LabelValuePairWrapper.Empty;
				}
				return requiredTo;
			}
		}

		protected virtual LabelValuePairWrapper GetRequiredToLabelValuePairCore()
		{
			return new LabelValuePairWrapper(Res.GetString("1c5959b0-3f4e-4493-a661-bbcb1cd0ae81", "Required To"), ActionBO.RequiredTo, Factory);
		}

		LabelValuePairWrapper requiredTo;

		#endregion

		#region ConfirmationReferenceNum

		public ZString ConfirmationReferenceNum
		{
			get { return (ActionBO != null) ? ActionBO.ReferenceNumber : ZString.Empty; }
		}

		#endregion

		#region ConfirmationType

		public ZString ConfirmationType
		{
			get { return (ActionBO) != null ? ActionBO.ActionType : ZString.Empty; }
		}

		#endregion

		#region ConfirmationDescription

		public ZString ConfirmationDescription
		{
			get { return (ActionBO) != null ? ZString.Format("{0} {1}", ActionBO.ActionType, EmptyContainerText) : ZString.Empty; }
		}

		#endregion

		#region ConfirmationID

		public ZString ConfirmationID
		{
			get
			{
				ZString result = "";
				if (ActionBO != null)
				{
					var quantity = (ActionBO.PackageDivot == null) ? PackageDivotQuantity : ConfirmationQuantity;
					result = string.Format((NoResString)"{0}x {1}", quantity, PackageType);
				}
				return result;
			}
		}

		#endregion

		#region ReceivedBy

		public ZString ReceivedBy
		{
			get { return GetReceivedBy(); }
		}

		protected virtual ZString GetReceivedBy()
		{
			return ActionBO != null ? ActionBO.ReceivedBy : ZString.Empty;
		}

		#endregion

		#region ReceivedBySignature

		public Image ReceivedBySignature
		{
			get { return GetReceivedBySignature(); }
		}

		protected virtual Image GetReceivedBySignature()
		{
			return ActionBO != null ? new SignatureDrawer(ActionBO.ReceivedBySignature).Image : null;
		}

		#endregion

		#endregion

		#region RunSheet Properties

		#region ConsignorOrConsigneeAddress

		public ZString ConsignorOrConsigneeAddress
		{
			get { return ConsignorOrConsigneeAddressCore; }
		}

		/// <summary>
		/// The other Address for the consignment.
		/// If this is a Pickup, then where is the consignment going?
		/// If this is a Delivery, then where did it come from?
		/// </summary>
		protected virtual ZString ConsignorOrConsigneeAddressCore
		{
			get
			{
				var result = ZString.Empty;

				if (ActionBO != null)
				{
					var consignoree = ActionBO.IsPickUp ? Transport?.Consignee : Transport?.Consignor;
					if (consignoree != null)
					{
						result = GetOrganizationAddress(consignoree);
					}
					else if (!ActionBO.ConsignorOrConsigneeAddress.IsEmpty)
					{
						result = ActionBO.ConsignorOrConsigneeAddress;
					}
				}

				return result;
			}
		}

		protected static string GetOrganizationAddress(OrganisationWrapper wrapper)
		{
			var cityAndState = wrapper.MainAddress.City + " " + wrapper.MainAddress.State;
			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(wrapper.CompanyName);
			builder.AppendIfNotEmpty(cityAndState.Trim());
			{
				return builder.ToStringWithDelimiterBetweenAppends(" - ");
			}
		}

		#endregion

		#region Transport

		public FreightWrapper Transport
		{
			get { return TransportCore; }
		}

		protected virtual FreightWrapper TransportCore
		{
			get { return ActionBO?.ConsignmentAddress?.Booking != null ? FreightWrapper.New(ActionBO.ConsignmentAddress.Booking, Factory)[0] : null; }
		}

		#endregion

		#endregion

		#region Package Properties

		#region PackageDivotQuantity

		public ZInt PackageDivotQuantity
		{
			get { return GetPackageDivotQuantity(); }
		}
		protected abstract ZInt GetPackageDivotQuantity();

		#endregion

		#region PackageType

		public ZString PackageType
		{
			get { return GetPackageType(); }
		}
		protected abstract ZString GetPackageType();

		#endregion

		#region Weight

		public WeightWrapper Weight
		{
			get { return GetWeight(); }
		}
		protected abstract WeightWrapper GetWeight();

		#endregion

		#region Volume

		public VolumeWrapper Volume
		{
			get { return GetVolume(); }
		}
		protected abstract VolumeWrapper GetVolume();

		#endregion

		#region PackageID

		public ZString PackageID
		{
			get { return GetPackageID(); }
		}
		protected abstract ZString GetPackageID();

		#endregion

		#region PackageDivotSequence

		// This property used for GroupBy on template.
		public ZInt PackageDivotSequence
		{
			get { return GetPackageDivotSequence(); }
		}
		protected abstract ZInt GetPackageDivotSequence();

		#endregion

		#region PackageDivotID

		public ZString PackageDivotID
		{
			get { return GetPackageDivotID(); }
		}
		protected abstract ZString GetPackageDivotID();

		#endregion

		#region PackageDimensions

		public ZString PackageDimensions
		{
			get { return GetPackageDimensions(); }
		}
		protected abstract ZString GetPackageDimensions();

		#endregion

		#region HasSingleConfirmationForWholePackage

		public ZBool HasSingleConfirmationForWholePackage
		{
			get { return GetHasSingleConfirmationForWholePackage(); }
		}
		protected abstract ZBool GetHasSingleConfirmationForWholePackage();

		#endregion

		#region UNDGsSummary

		public ZString UNDGsSummary
		{
			get { return GetUNDGsSummary(); }
		}
		protected abstract ZString GetUNDGsSummary();

		#endregion

		#endregion

		#region Implementation

		#region EmptyContainerText

		protected ZString EmptyContainerText
		{
			get { return ActionBO != null && ActionBO.IsEmptyContainer ? Res.GetString("InstructionWrapper|Empty", "(MT)") : ""; }
		}

		#endregion

		#endregion
	}
}
