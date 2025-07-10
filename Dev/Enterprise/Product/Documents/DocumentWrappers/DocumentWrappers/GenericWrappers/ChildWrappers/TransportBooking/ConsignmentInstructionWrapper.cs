using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Wraps a Consignment Instruction
	/// Documents group by the Instruction (Sequence), so create multiple for each Confirmation and Package
	/// </summary>
	public class ConsignmentInstructionWrapper : TransportInstructionWrapper
	{
		#region Constructors

		public ConsignmentInstructionWrapper(DtbConsignmentInstruction groupedInstruction, BusinessObjectFactory factory)
			: base(groupedInstruction ?? factory.GetNull<DtbConsignmentInstruction>(), factory)
		{
		}

		public ConsignmentInstructionWrapper(DtbConsignmentInstruction groupedInstruction, DtbConsignmentInstructionPkgDivot perPackageDivot, DtbConsignmentConfirmation perConsignmentConfirmation, BusinessObjectFactory factory)
			: base(groupedInstruction ?? factory.GetNull<DtbConsignmentInstruction>(), perPackageDivot ?? factory.GetNull<DtbConsignmentInstructionPkgDivot>(), perConsignmentConfirmation ?? factory.GetNull<DtbConsignmentConfirmation>(), factory)
		{
		}

		#endregion

		#region GetReceivedBy

		protected override ZString GetReceivedBy()
		{
			return (ConsignmentConfirmationBO) != null ? ConsignmentConfirmationBO.ReceivedBy : ZString.Empty;
		}

		#endregion

		#region GetReceivedBySignature

		protected override Image GetReceivedBySignature()
		{
			return ConsignmentConfirmationBO != null ? new SignatureDrawer(ConsignmentConfirmationBO.ReceivedBySignature).Image : null;
		}

		#endregion

		#region GetTimeIn

		protected override ZDateTime GetTimeIn()
		{
			return RunSheetInstructionBO != null ? RunSheetInstructionBO.K1_TimeIn.ToLocalZDateTime() : ZDateTime.Empty;
		}

		#endregion

		#region GetTimeOut

		protected override ZDateTime GetTimeOut()
		{
			return RunSheetInstructionBO != null ? RunSheetInstructionBO.K1_TimeOut.ToLocalZDateTime() : ZDateTime.Empty;
		}

		#endregion

		#region Implementation

		#region ConsignmentConfirmationBO

		DtbConsignmentConfirmation ConsignmentConfirmationBO
		{
			get { return (DtbConsignmentConfirmation)ActionBO; }
		}

		#endregion

		#region RunSheetInstructionBO

		DtbConsignmentRunSheetInstruction RunSheetInstructionBO
		{
			get { return ConsignmentConfirmationBO != null ? ConsignmentConfirmationBO.RunSheetInstruction : null; }
		}

		#endregion

		#endregion
	}
}
