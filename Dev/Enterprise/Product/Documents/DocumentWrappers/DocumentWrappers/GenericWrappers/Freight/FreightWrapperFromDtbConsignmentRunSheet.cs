using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money;
using Enterprise.TransportConsignment.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class FreightWrapperFromDtbConsignmentRunSheet : FreightWrapper
	{
		public FreightWrapperFromDtbConsignmentRunSheet(DtbConsignmentRunSheet runSheet, BusinessObjectFactory factory)
			: base(runSheet ?? factory.GetNull<DtbConsignmentRunSheet>(), factory)
		{
			Argument.NotNull(factory, "factory");
		}

		#region RelatedEntities

		#region GetRunSheet

		protected override RunSheetWrapper GetRunSheet()
		{
			return new RunSheetWrapperFromDtbConsignmentRunSheet(RunSheetBO, Factory);
		}

		#endregion

		#region GetBookingInstructions

		protected override InstructionWrapperCollection GetBookingInstructions()
		{
			var runSheetInstructions = new InstructionWrapperCollection(Factory);
			foreach (var instruction in RunSheetBO.RunSheetInstructions)
			{
				AddWrappersFromInstructionConfirmations(runSheetInstructions, instruction);
			}
			return runSheetInstructions;
		}

		void AddWrappersFromInstructionConfirmations(InstructionWrapperCollection runSheetInstructions, DtbConsignmentRunSheetInstruction runSheetInstruction)
		{
			foreach (var confirmation in runSheetInstruction.ConsignmentActions)
			{
				if (confirmation.PackageDivot != null)
				{
					throw new NotSupportedException("Consignment Confirmations always reference all Instruction Packages (ie. Confirmation.Parent is the Instruction)");
				}

				var instructionWrapper = new RunSheetInstructionWrapper(runSheetInstruction, confirmation, Factory);
				runSheetInstructions.Add(instructionWrapper);
			}
		}

		#endregion

		#endregion

		#region Properties

		#region GetJobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("FreightWrapperFromDtbConsignmentRunSheet|RunSheet", "Run Sheet");
		}

		#endregion

		#region GetJobNumber

		protected override ZString GetJobNumber()
		{
			return RunSheetBO.KG_RunSheetNumber;
		}

		#endregion

		#endregion

		#region RunSheet

		DtbConsignmentRunSheet RunSheetBO
		{
			get { return (DtbConsignmentRunSheet)WrappedBO; }
		}

		#endregion

		#region Costs

		protected override CostWrapperCollection GetCosts()
		{
			return new CostWrapperCollection(new JobConsolCostCollection(Factory, RunSheetBO), Factory);
		}

		#endregion
	}
}
