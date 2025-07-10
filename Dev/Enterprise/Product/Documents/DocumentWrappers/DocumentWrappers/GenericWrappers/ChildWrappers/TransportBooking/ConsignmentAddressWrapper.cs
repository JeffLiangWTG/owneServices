using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ConsignmentAddressWrapper : InstructionWrapper
	{
		#region Constructors

		public ConsignmentAddressWrapper(DtbConsignmentAddress groupedInstruction, IConsignmentAction perAction, BusinessObjectFactory factory)
			: base(groupedInstruction ?? factory.GetNull<DtbConsignmentAddress>(), perAction ?? factory.GetNull<DtbConsignmentAction>(), factory)
		{
		}

		#endregion

		protected override ZString GetReceivedBy()
		{
			return ConsignmentActionBO != null ? ConsignmentActionBO.ReceivedBy : ZString.Empty;
		}

		protected override Image GetReceivedBySignature()
		{
			return ConsignmentActionBO != null ? new SignatureDrawer(ConsignmentActionBO.ReceivedBySignature).Image : null;
		}

		protected override AddressWrapper GetAddress()
		{
			return new AddressWrapper(ConsignmentAddressBO.Address, Factory);
		}

		protected override CodeAndDescriptionWrapper GetDropMode()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override ZString GetEquipment()
		{
			return ZString.Empty;
		}

		protected override ZBool GetHasSingleConfirmationForWholePackage()
		{
			return ZBool.False;
		}

		protected override ZString GetInstructionType()
		{
			return ZString.Empty;
		}

		protected override ZString GetPackageDimensions()
		{
			return ZString.Empty;
		}

		protected override ZString GetPackageDivotID()
		{
			return ZString.Empty;
		}

		protected override ZInt GetPackageDivotQuantity()
		{
			return ZInt.Zero;
		}

		protected override ZInt GetPackageDivotSequence()
		{
			return ZInt.Zero;
		}

		protected override ZString GetPackageID()
		{
			return ZString.Empty;
		}

		protected override ZString GetPackageType()
		{
			return ZString.Empty;
		}

		protected override ZInt GetSequence()
		{
			return ZInt.Zero;
		}

		protected override ZString GetServiceInstruction()
		{
			return ZString.Empty;
		}

		protected override ZString GetStatus()
		{
			return ZString.Empty;
		}

		protected override ZDateTime GetTimeIn()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetTimeOut()
		{
			return ZDateTime.Empty;
		}

		protected override ZString GetUNDGsSummary()
		{
			return ZString.Empty;
		}

		protected override VolumeWrapper GetVolume()
		{
			return VolumeWrapper.Empty;
		}

		protected override WeightWrapper GetWeight()
		{
			return WeightWrapper.Empty;
		}

		#region Implementation

		DtbConsignmentAddress ConsignmentAddressBO
		{
			get { return (DtbConsignmentAddress)WrappedBO; }
		}

		IConsignmentAction ConsignmentActionBO
		{
			get { return ActionBO; }
		}

		#endregion
	}
}
