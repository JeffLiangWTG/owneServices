using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ConsignmentInstructionWrapper))]
	sealed class ConsignmentInstructionWrapperTest : TransportInstructionWrapperTest
	{
		protected override void SetupValuesForTestWrapperMapping(DtbTransportInstruction instruction, DtbTransportConfirmation confirmation)
		{
			base.SetupValuesForTestWrapperMapping(instruction, confirmation);

			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = Helper.CreateRunSheetInstruction(runSheet, new[] { (DtbConsignmentConfirmation)confirmation });
			runSheetInstruction.K1_IsAcceptedByDriver = true;
			runSheetInstruction.K1_TimeIn = new ZDateTimeOffset(new ZDateTime(2011, 1, 1, 2, 1, 1), DateTimeKind.Local);
			runSheetInstruction.K1_TimeOut = new ZDateTimeOffset(new ZDateTime(2011, 1, 1, 3, 1, 1), DateTimeKind.Local);
		}

		#region TestWrapperMappingFullCore

		protected override void TestWrapperMappingFullCore(TransportInstructionWrapper wrapper)
		{
			AssertEquals("Attaching to RunSheet resets Status", "AVL", wrapper.Status);
			AssertEquals("TimeIn", new ZDateTime(2011, 1, 1, 2, 1, 1), wrapper.TimeIn);
			AssertEquals("TimeOut", new ZDateTime(2011, 1, 1, 3, 1, 1), wrapper.TimeOut);
		}

		#endregion

		#region TestRequiredFromAndRequiredTo

		protected override void TestRequiredFromAndRequiredToCore()
		{
			base.TestRequiredFromAndRequiredToCore();

			var instruction = GetInstructionBizO();
			var instructionWrapperWithBizO = GetInstructionWrapper(instruction, Factory);
			AssertEquals(LabelValuePairWrapper.Empty, instructionWrapperWithBizO.RequiredTo);
			AssertEquals(LabelValuePairWrapper.Empty, instructionWrapperWithBizO.RequiredFrom);
			AssertEquals("TimeIn", ZDateTime.Empty, instructionWrapperWithBizO.TimeIn);
			AssertEquals("TimeOut", ZDateTime.Empty, instructionWrapperWithBizO.TimeOut);
		}

		#endregion

		#region TestConfirmationID

		protected override DtbTransportInstruction CreateInstructionBizO(string instructionType, out PkgPackageJob packageJob)
		{
			var consignment = Helper.CreateBookingConsignment();
			packageJob = consignment.PackageJob;
			return Helper.CreateInstruction(consignment, instructionType);
		}

		#endregion

		#region TestReceivedBy

		public void TestReceivedBy()
		{
			var instruction = Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction;
			AssertEquals("", GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBy);

			var runsheetInstruction = Helper.CreateRunSheetInstruction(instruction.Confirmations[0]);
			runsheetInstruction.K1_ReceivedBy = "OnRunsheet";
			AssertEquals("OnRunsheet", GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBy);

			instruction.Confirmations[0].KK_ReceivedBy = "OnConfirmation";
			AssertEquals("OnConfirmation", GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBy);
		}

		#endregion

		#region TestReceivedBySignature

		public void TestReceivedBySignature()
		{
			var instruction = Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction;
			AssertNull("Precondition", GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBySignature);

			var runsheetInstruction = Helper.CreateRunSheetInstruction(instruction.Confirmations[0]);
			runsheetInstruction.K1_ReceivedBySignature = Helper.GetSignature();
			AssertNotNull(GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBySignature);

			runsheetInstruction.K1_ReceivedBySignature = ZBlob.Empty;
			AssertNull("Precondition", GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBySignature);

			instruction.Confirmations[0].KK_ReceivedBySignature = Helper.GetSignature();
			AssertNotNull(GetInstructionWrapper(instruction, null, instruction.Confirmations[0], Factory).ReceivedBySignature);
		}

		#endregion

		#region Implementation

		#region GetInstructionBizO

		protected override DtbTransportInstruction GetInstructionBizO()
		{
			return Factory.New<DtbConsignmentInstruction>();
		}

		#endregion

		#region GetInstructionWrapper

		protected override TransportInstructionWrapper GetInstructionWrapper(DtbTransportInstruction instruction, BusinessObjectFactory factory)
		{
			return new ConsignmentInstructionWrapper((DtbConsignmentInstruction)instruction, Factory);
		}

		protected override TransportInstructionWrapper GetInstructionWrapper(DtbTransportInstruction instruction, DtbTransportInstructionPkgDivot divot, DtbTransportConfirmation confirmation, BusinessObjectFactory factory)
		{
			return new ConsignmentInstructionWrapper((DtbConsignmentInstruction)instruction, (DtbConsignmentInstructionPkgDivot)divot, (DtbConsignmentConfirmation)confirmation, Factory);
		}

		#endregion

		#region GetNewTransport

		protected override DtbTransport GetNewTransport()
		{
			return Helper.CreateBookingConsignment();
		}

		#endregion

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
