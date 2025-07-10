using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDtbBooking))]
	sealed class FreightWrapperFromDtbBookingTest : FreightWrapperTest
	{
		#region TestBarcodeTextForFont

		protected override void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
			var booking = (DtbBooking)bizO;
			booking.KM_JobID = "D1";
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^DTB=D1;CAD;|oÊ";
		}

		#endregion

		#region TestBookingInstructions

		public void TestBookingInstructions()
		{
			var booking = (DtbBooking)GetNewBusinessObjectToWrap();

			DtbBookingInstruction instruction_NoPkgDivotsOrConfirmations = booking.Instructions.AddNew();

			DtbBookingInstruction instruction_NoPkgDivots = booking.Instructions.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivots_Confirmation1 = instruction_NoPkgDivots.Confirmations.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivots_Confirmation2 = instruction_NoPkgDivots.Confirmations.AddNew();

			DtbBookingInstruction instruction_NoConfirmations = booking.Instructions.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoConfirmations_PkgDivot1 = instruction_NoConfirmations.PackageDivots.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoConfirmations_PkgDivot2 = instruction_NoConfirmations.PackageDivots.AddNew();

			DtbBookingInstruction instruction_NoPkgDivotConfirmations = booking.Instructions.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivotConfirmations_Confirmation1 = instruction_NoPkgDivotConfirmations.Confirmations.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivotConfirmations_Confirmation2 = instruction_NoPkgDivotConfirmations.Confirmations.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoPkgDivotConfirmations_PkgDivot1 = instruction_NoPkgDivotConfirmations.PackageDivots.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoPkgDivotConfirmations_PkgDivot2 = instruction_NoPkgDivotConfirmations.PackageDivots.AddNew();

			DtbBookingInstruction instruction_NoInstructionConfirmations = booking.Instructions.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoInstructionConfirmations_PkgDivot1 = instruction_NoInstructionConfirmations.PackageDivots.AddNew();
			DtbBookingConfirmation instruction_NoInstructionConfirmations_PkgDivot1_Confirmation1 = instruction_NoInstructionConfirmations_PkgDivot1.Confirmations.AddNew();
			DtbBookingConfirmation instruction_NoInstructionConfirmations_PkgDivot1_Confirmation2 = instruction_NoInstructionConfirmations_PkgDivot1.Confirmations.AddNew();

			// to be able to identify confirmations during assert.
			instruction_NoPkgDivots_Confirmation1.KK_Quantity = 1;
			instruction_NoPkgDivots_Confirmation2.KK_Quantity = 2;
			instruction_NoPkgDivotConfirmations_Confirmation1.KK_Quantity = 3;
			instruction_NoPkgDivotConfirmations_Confirmation2.KK_Quantity = 4;
			instruction_NoInstructionConfirmations_PkgDivot1_Confirmation1.KK_Quantity = 5;
			instruction_NoInstructionConfirmations_PkgDivot1_Confirmation2.KK_Quantity = 6;

			var transportBookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Wrong number of instructions were created, should be 1 + 2 + 2 + 4 + 2", 11, transportBookingWrapper.BookingInstructions.Count);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivotsOrConfirmations, null, null);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivots, null, instruction_NoPkgDivots_Confirmation1);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivots, null, instruction_NoPkgDivots_Confirmation2);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoConfirmations, instruction_NoConfirmations_PkgDivot1, null);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoConfirmations, instruction_NoConfirmations_PkgDivot2, null);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot1, instruction_NoPkgDivotConfirmations_Confirmation1);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot1, instruction_NoPkgDivotConfirmations_Confirmation2);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot2, instruction_NoPkgDivotConfirmations_Confirmation1);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot2, instruction_NoPkgDivotConfirmations_Confirmation2);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoInstructionConfirmations, instruction_NoInstructionConfirmations_PkgDivot1, instruction_NoInstructionConfirmations_PkgDivot1_Confirmation1);
			AssertContainInstructionFor(transportBookingWrapper.BookingInstructions, instruction_NoInstructionConfirmations, instruction_NoInstructionConfirmations_PkgDivot1, instruction_NoInstructionConfirmations_PkgDivot1_Confirmation2);
		}

		void AssertContainInstructionFor(InstructionWrapperCollection allInstructionWrappers, DtbBookingInstruction instruction, DtbBookingInstructionPkgDivot instructionPkgDivot, DtbBookingConfirmation confirmation)
		{
			foreach (BookingInstructionWrapper instructionWrapper in allInstructionWrappers)
			{
				if (instructionWrapper.Sequence == instruction.KN_Sequence &&
					(instructionPkgDivot == null || instructionWrapper.PackageDivotSequence == instruction.PackageDivots.IndexOf(instructionPkgDivot)) &&
					(confirmation == null || instructionWrapper.ConfirmationQuantity == confirmation.KK_Quantity))
				{
					return;
				}
			}
			Fail(string.Format("No Booking Instruction could be found for Instruction Sequence '{0}', Package Divot with index '{1}' and Confirmation with Qty '{2}'",
				instruction.KN_Sequence,
				(instructionPkgDivot != null) ? instruction.PackageDivots.IndexOf(instructionPkgDivot) : 0,
				(confirmation != null) ? confirmation.KK_Quantity : ZInt.Zero));
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_Code = "ABCDEFG";

			var booking = (DtbBooking)GetNewBusinessObjectToWrap();
			booking.Address.OrganisationPK = transportCo.PK;

			var bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertNotNull("Carier", bookingWrapper.Carrier);
			AssertEquals("Carier Org", "ABCDEFG", bookingWrapper.Carrier.CompanyCode);
		}

		#endregion

		#region CustomsEntries

		public void TestCustomsEntries_WithParent()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var entry1 = shipment.CusEntryNumbers.AddNew();
			entry1.CE_EntryNum = "456";
			entry1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entry1.CE_ParentID = shipment.PK;
			entry1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var entry2 = shipment.CusEntryNumbers.AddNew();
			entry2.CE_EntryType = "TST";
			entry2.CE_EntryNum = "123";
			entry2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entry2.CE_ParentID = shipment.PK;
			entry2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var consolidation1 = Helper.CreateConsolidation(shipment);
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking1 = Helper.CreateBooking(consolidation1);
			var bookingReference = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference.CE_EntryType = "TST";
			bookingReference.CE_EntryNum = "123";

			var wrapper1 = new FreightWrapperFromDtbBooking(booking1, Factory);
			AssertEquals(2, wrapper1.CustomsEntries.Count);
			AssertEquals("123", wrapper1.CustomsEntries[0].EntryNumber);
			AssertEquals("456", wrapper1.CustomsEntries[1].EntryNumber);
		}

		#endregion

		#region TestCustomsEntries_WithConsolidation

		public void TestCustomsEntries_WithConsolidation()
		{
			var booking = Helper.CreateBooking();

			var uniqueReferenceOnConsolidation1 = Helper.CreateAddtionalReference(booking.ConsolidationSingleJob, "CIN", "CIN - CON", DateTime.Now);
			var uniqueReferenceOnConsolidation2 = Helper.CreateAddtionalReference(booking.ConsolidationSingleJob, "TST", "Test - CON", DateTime.Now);
			var duplicateReferenceOnConsolidation = Helper.CreateAddtionalReference(booking.ConsolidationSingleJob, "TST", "Test", DateTime.Now);
			var duplicateReferenceOnBooking = Helper.CreateAddtionalReference(booking, "TST", "Test", DateTime.Now);
			var uniqueReferenceOnBooking1 = Helper.CreateAddtionalReference(booking, "TST", "TST", DateTime.Now);
			var uniqueReferenceOnBooking2 = Helper.CreateAddtionalReference(booking, "HSB", "HSB - Booking", DateTime.Now);

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals(5, wrapper.CustomsEntries.Count);
			var customEntries = wrapper.CustomsEntries;
			customEntries.Sort("EntryNumber");
			AssertCustomEntry(customEntries[0], "CIN", "CIN - CON");
			AssertCustomEntry(customEntries[1], "HSB", "HSB - Booking");
			AssertCustomEntry(customEntries[2], "TST", "Test");
			AssertCustomEntry(customEntries[3], "TST", "Test - CON");
			AssertCustomEntry(customEntries[4], "TST", "TST");
		}

		static void AssertCustomEntry(CustomsEntryWrapper customEntry, string expectedEntryType, string expectedEntryNumber)
		{
			AssertEquals(expectedEntryType, customEntry.EntryType.Code);
			AssertEquals(expectedEntryNumber, customEntry.EntryNumber);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			var booking = Helper.CreateBooking();

			var uniqueReferenceOnConsolidation1 = Helper.CreateAddtionalReference(booking.ConsolidationSingleJob, "MAB", "MAB - CON", DateTime.Now);
			var uniqueReferenceOnConsolidation2 = Helper.CreateAddtionalReference(booking.ConsolidationSingleJob, "MAB", "MAB - Test", DateTime.Now);
			var duplicateReferenceOnBooking = Helper.CreateAddtionalReference(booking, "MAB", "MAB - Test", DateTime.Now);
			var uniqueReferenceOnBooking1 = Helper.CreateAddtionalReference(booking, "MAB", "MAB - Booking", DateTime.Now);

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MAB - CON", "MAB - Test", "MAB - Booking" }, wrapper.MasterBill.Split(',').Select(s => s.Trim()).ToArray());
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill_HouseBillOnConsolidation()
		{
			var bookingWithHSBOnConsolidation = Helper.CreateBooking();
			var reference = Helper.CreateAddtionalReference(bookingWithHSBOnConsolidation.ConsolidationSingleJob, "HSB", "HSB - CON", DateTime.Now);
			var wrapper = new FreightWrapperFromDtbBooking(bookingWithHSBOnConsolidation, Factory);
			AssertEquals("HSB - CON", wrapper.HouseBill);
		}

		public void TestHouseBill_HouseBillOnBooking()
		{
			var bookingWithHSBOnBooking = Helper.CreateBooking();
			var reference = Helper.CreateAddtionalReference(bookingWithHSBOnBooking.ConsolidationSingleJob, "HSB", "HSB - Booking", DateTime.Now);
			var wrapper = new FreightWrapperFromDtbBooking(bookingWithHSBOnBooking, Factory);
			AssertEquals("HSB - Booking", wrapper.HouseBill);
		}

		public void TestHouseBill_HouseBillOnConsolidationAndBooking()
		{
			var bookingWithHSBOnConsolidationAndBooking = Helper.CreateBooking();
			var reference1 = Helper.CreateAddtionalReference(bookingWithHSBOnConsolidationAndBooking.ConsolidationSingleJob, "HSB", "HSB - CON", DateTime.Now);
			var reference2 = Helper.CreateAddtionalReference(bookingWithHSBOnConsolidationAndBooking, "HSB", "HSB - Booking", DateTime.Now);

			var wrapper = new FreightWrapperFromDtbBooking(bookingWithHSBOnConsolidationAndBooking, Factory);
			AssertEquals("HSB - Booking", wrapper.HouseBill);
		}

		#endregion

		#region TestOtherReferences

		public void TestOtherReferences()
		{
			var receive = Factory.New<WhsReceive>();

			var entry1 = receive.References.AddNew();
			entry1.WX_RefType = TransportAdditionalReferenceTypes.Codes.TransportReference;
			entry1.WX_Reference = "123";

			var entry2 = receive.References.AddNew();
			entry2.WX_RefType = "MAR";
			entry2.WX_Reference = "456";

			var consolidation1 = Helper.CreateConsolidation(receive);
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking1 = Helper.CreateBooking(consolidation1);
			var bookingReference1 = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference1.CE_EntryType = TransportAdditionalReferenceTypes.Codes.TransportReference;
			bookingReference1.CE_EntryNum = "123";

			var bookingReference2 = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference2.CE_EntryType = TransportAdditionalReferenceTypes.Codes.OrderNumber;
			bookingReference2.CE_EntryNum = "123456";

			var bookingReference3 = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference3.CE_EntryType = "ABC";
			bookingReference3.CE_EntryNum = "123456";

			var bookingReference4 = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference4.CE_EntryType = TransportAdditionalReferenceTypes.Codes.HouseBill;
			bookingReference4.CE_EntryNum = "123456";

			var bookingReference5 = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference5.CE_EntryType = TransportAdditionalReferenceTypes.Codes.MasterBill;
			bookingReference5.CE_EntryNum = "56789";

			var wrapper = new FreightWrapperFromDtbBooking(booking1, Factory);
			AssertEquals("Transport Reference Number: 123\r\nABC: 123456\r\nMAR: 456", wrapper.OtherReferences);
		}

		public void TestOtherReferences_NoParentReferences()
		{
			var receive = Factory.New<WhsReceive>();

			var consolidation1 = Helper.CreateConsolidation(receive);
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking1 = Helper.CreateBooking(consolidation1);
			var bookingReference1 = booking1.AdditionalReferenceNumbers.AddNew();
			bookingReference1.CE_EntryType = TransportAdditionalReferenceTypes.Codes.TransportReference;
			bookingReference1.CE_EntryNum = "123";

			var wrapper = new FreightWrapperFromDtbBooking(booking1, Factory);
			AssertEquals("Transport Reference Number: 123", wrapper.OtherReferences);
		}

		#endregion

		#region TestCartageInfo

		public void TestCartageInfo()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consolidation1 = Helper.CreateConsolidation(shipment);
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var booking1 = Helper.CreateBooking(consolidation1);
			var wrapper1 = new FreightWrapperFromDtbBooking(booking1, Factory);
			AssertEquals(false, wrapper1.CartageInfo.IsExportDocument);
			AssertEquals(true, wrapper1.CartageInfo.IsImportDocument);

			var consolidation2 = Helper.CreateConsolidation(shipment);
			consolidation2.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking2 = Helper.CreateBooking(consolidation2);
			var wrapper2 = new FreightWrapperFromDtbBooking(booking2, Factory);
			AssertEquals(true, wrapper2.CartageInfo.IsExportDocument);
			AssertEquals(false, wrapper2.CartageInfo.IsImportDocument);
		}

		#endregion

		#region TestJobNumberAndHeading

		public void TestJobNumberAndHeading()
		{
			var bookingMovement = (DtbBooking)GetNewBusinessObjectToWrap();
			bookingMovement.KM_JobID = "B00000001";

			var bookingMovementWrapper = new FreightWrapperFromDtbBooking(bookingMovement, Factory);
			AssertEquals("JobNumberHeading", "Transport Booking", bookingMovementWrapper.JobNumberHeading);
			AssertEquals("JobNumber", "B00000001", bookingMovementWrapper.JobNumber);
		}

		#endregion

		#region TestJobHeaderBranchLogo

		public override void TestJobHeaderBranchLogo()
		{
			var bookingMovement = (DtbBooking)GetNewBusinessObjectToWrap();
			bookingMovement.KM_JobID = "B00000001";
			bookingMovement.KM_TransportReference = "TranRef1234";

			var bookingMovementWrapper = new FreightWrapperFromDtbBooking(bookingMovement, Factory);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertNull("No branch logo", bookingMovementWrapper.CompanyLogo);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(1, 1));
			AssertEquals("Branch logo should be Company Logo", new Size(1, 1), bookingMovementWrapper.CompanyLogo.Size);
		}

		#endregion

		#region TestNotes

		public override void TestWrapperNotes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consolidation = Helper.CreateConsolidation(shipment);
			var booking = Helper.CreateBooking(consolidation);

			var note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note1.ST_NoteDataAsText = "dlv note.";

			var note2 = booking.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note2.ST_NoteDataAsText = "booking note.";

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals(2, wrapper.Notes.Count);
			AssertEquals("dlv note.", wrapper.Notes[PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description].Text);
			AssertEquals("booking note.", wrapper.Notes[PredefinedNoteTypes.Instance.HandlingInstructions.Description].Text);
		}

		#region TestHandlingInstructions

		public void TestHandlingInstructions()
		{
			var booking = Helper.CreateBooking();
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with care");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Keep away from water");

			var org1 = Helper.CreateOrganisation("TRANSPORT");
			org1.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Fragile");
			booking.Address.OrganisationPK = org1.PK;

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);

			AssertEquals("Handle with care\r\nKeep away from water\r\nFragile", wrapper.FullHandlingInstructions);
		}

		#endregion

		#region TestHandlingInstructionsWithParentReceive

		public void TestHandlingInstructionsWithParentReceive()
		{
			var receive = Factory.New<WhsReceive>();
			receive.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with care");
			receive.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Keep away from water");

			var consolidation = Helper.CreateConsolidation(receive);
			var booking = Helper.CreateBooking(consolidation);
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Fragile");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Don't set on fire");

			// should not contain organisation notes as they are printed with the instruction's serviceinstructions 
			var org = Helper.CreateOrganisation("CNR");
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Should appear with instruction notes, not handling instructions.");
			var instruction = Helper.CreateInstruction("CNR");
			instruction.Address.OrganisationPK = org.PK;

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Fragile\r\nDon't set on fire\r\nHandle with care\r\nKeep away from water", wrapper.FullHandlingInstructions);
		}

		#endregion

		#region TestHandlingInstructionsWithParentShipment

		public void TestHandlingInstructionsWithParentShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with care");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Keep away from water");

			var consolidation = Helper.CreateConsolidation(shipment);
			var booking = Helper.CreateBooking(consolidation);
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Fragile");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Don't set on fire");

			// should not contain organisation notes as they are printed with the instruction's serviceinstructions 
			var org = Helper.CreateOrganisation("CNR");
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Should appear with instruction notes, not handling instructions.");
			var instruction = Helper.CreateInstruction("CNR");
			instruction.Address.OrganisationPK = org.PK;

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Handle with care\r\nFragile\r\nDon't set on fire\r\nKeep away from water", wrapper.FullHandlingInstructions);
		}

		#endregion

		#region TestHandlingInstructionsWithParentBookingDuplicateNotes

		public void TestHandlingInstructionsWithParentBookingDuplicateNotes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consolidation = Helper.CreateConsolidation(shipment);
			var booking = Helper.CreateBooking(consolidation);
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with care");
			booking.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with care");

			var org1 = Helper.CreateOrganisation("TRANSPORT");
			org1.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with care");
			booking.Address.OrganisationPK = org1.PK;

			var wrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Handle with care", wrapper.FullHandlingInstructions);
		}

		#endregion

		#endregion

		#region TestOrderNumbersWithOwnersReference

		public void TestOrderNumbersWithOwnersReference()
		{
			var booking = Helper.CreateBooking();
			var bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Precondition.", "", bookingWrapper.OrderNumbersWithOwnersReference);

			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.OrderNumber, "Order No");
			bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Order Numbers should be loaded from additional references.", "Order No", bookingWrapper.OrderNumbersWithOwnersReference);

			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.OrderNumber, "123");
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.OrderNumber, "ABC");
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.TransportReference, "NOT Order No");
			bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("Order Numbers should be loaded from additional references.", "Order No, 123, ABC", bookingWrapper.OrderNumbersWithOwnersReference);
		}

		#endregion

		#region TestBusinessObjectToLogAgainst

		public void TestBusinessObjectToLogAgainst()
		{
			var standAloneBooking = Helper.CreateBooking();
			var wrapperForStandAloneBooking = new FreightWrapperFromDtbBooking(standAloneBooking, Factory);
			var shipment = Factory.New<ForwardingShipment>();
			var consolidation = Helper.CreateConsolidation(shipment);
			var bookingWithParent = Helper.CreateBooking(consolidation);
			var wrapperForBookingWithParent = new FreightWrapperFromDtbBooking(bookingWithParent, Factory);

			DtbFormStateService.SetState(Factory, DtbFormState.Booking);
			AssertEquals(standAloneBooking, ((IBODocDataProvider)wrapperForStandAloneBooking).BusinessObjectToLogAgainst);
			AssertEquals(bookingWithParent, ((IBODocDataProvider)wrapperForBookingWithParent).BusinessObjectToLogAgainst);

			DtbFormStateService.SetState(Factory, DtbFormState.Parent);
			AssertEquals(standAloneBooking, ((IBODocDataProvider)wrapperForStandAloneBooking).BusinessObjectToLogAgainst);
			AssertEquals(shipment, ((IBODocDataProvider)wrapperForBookingWithParent).BusinessObjectToLogAgainst);
		}

		#endregion

		#region TestWrapperMappingFull

		public void TestWrapperMappingFull()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00012345";
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var sendingForwarder = GetOrgHeader("SND_FWDER");
			sendingForwarder.MainAddress.OA_Address1 = "SF MAIN ADDRESS";
			var sfAddr2 = sendingForwarder.Addresses.AddNew();
			sfAddr2.OA_Address1 = "SF ADDRESS";
			sfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_SendingForwarderAddress = sfAddr2.PK;

			var receivingForwarder = GetOrgHeader("RCV_FWDER");
			receivingForwarder.MainAddress.OA_Address1 = "RF MAIN ADDRESS";
			var rfAddr2 = receivingForwarder.Addresses.AddNew();
			rfAddr2.OA_Address1 = "RF ADDRESS";
			rfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			var shippingLine = GetOrgHeader("SHIPPINGLINE");
			shippingLine.MainAddress.OA_Address1 = "SL MAIN ADDRESS";
			var slAddr2 = shippingLine.Addresses.AddNew();
			slAddr2.OA_Address1 = "SL ADDRESS";
			slAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = slAddr2.PK;

			consol.JK_OA_CreditorAddress = GetOrgHeader("CONS_CREDIT").MainAddress.PK;
			consol.JK_BookingReference = "BOOK_A_HOOKER";
			consol.JK_PrepaidCollect = "CCX";

			var cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO WHATEVER COMPANY";

			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_ArrivalReference = "ARRIVAL REFERENCE";
			voyageDestination.JB_Berth = "BOOTH A23";
			voyageDestination.JB_JV = voyage.PK;

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2006, 5, 6);
			transport.JW_ATD = new ZDateTime(2006, 5, 7);
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETA = new ZDateTime(2006, 5, 8);
			transport.JW_ATA = new ZDateTime(2006, 5, 9);

			consol.JK_AgentsReference = "AGENT_1";
			consol.JK_MasterBillNum = "MASTERME";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S000234567";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_INCO = Constants.IncoTerms.DeliveredAtFrontier;
			shipment.JS_AdditionalTerms = "Hello AdditionalTerms!";
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_E_DEP = new ZDateTime(2006, 5, 5);
			shipment.JS_RL_NKDestination = "ERXXX";
			shipment.JS_E_ARV = new ZDateTime(2006, 5, 13);

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;

			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2006, 1, 1);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2006, 1, 2);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2006, 1, 3);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2006, 1, 4);
			shipment.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2006, 2, 1);
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2006, 2, 2);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2006, 2, 3);
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2006, 2, 4);

			shipment.DocsAndCartage.JP_CustomAttrib1 = "ONE";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "TWO";
			shipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2001, 1, 1);
			shipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2002, 2, 2);
			shipment.DocsAndCartage.JP_CustomDecimal1 = 1.1m;
			shipment.DocsAndCartage.JP_CustomDecimal2 = 2.22m;
			shipment.DocsAndCartage.JP_CustomFlag1 = true;
			shipment.DocsAndCartage.JP_CustomFlag2 = false;

			shipment.JS_OH_ImportBroker = GetOrgHeader("IMP_BROKER").PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("EXP_BROKER").PK;

			shipment.JS_HouseBillIssueDate = new ZDateTime(2005, 4, 8);
			shipment.JS_GoodsDescription = "JILTED LOVERS";
			shipment.DocsAndCartage.JP_OrderItemsAsString = "OWNERS";
			shipment.JS_MarksAndNumbers = "BROKEN HEARTS";
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Pick it up");

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000026";

			var packLine1 = shipment.OuterPackLines.Count == 0 ? shipment.OuterPackLines.AddNew() : shipment.OuterPackLines[0];
			packLine1.JL_JC = container.PK;
			packLine1.JL_PackageCount = 11;
			packLine1.JL_F3_NKPackType = "PK";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_PackageCount = 22;
			packLine2.JL_F3_NKPackType = "PK";

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container.PK;
			packLine3.JL_PackageCount = 33;
			packLine3.JL_F3_NKPackType = "PK";

			shipment.JS_OuterPacks = 400;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 354;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_ActualWeight = 55.4;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 32.45;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_LoadingMeters = 10.24m;
			shipment.JS_GoodsValue = 665.33m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 898.22m;
			shipment.JS_RX_NKInsuranceCurrency = "EUR";
			shipment.JS_ShippedOnBoard = "SOB";
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2006, 12, 25);
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_NoCopyBills = 5;
			shipment.JS_NoOriginalBills = 6;

			shipment.CustomsEntryNumberType = "COM";
			shipment.CustomsEntryNumber = "123";
			shipment.JS_InspectionTypeCode = "UNK";

			var shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_LegOrder = 2;
			shipmentTransport.JW_RL_NKLoadPort = "ERYYY";
			shipmentTransport.JW_ETD = new ZDateTime(2006, 5, 10);
			shipmentTransport.JW_RL_NKDiscPort = "ERXXX";
			shipmentTransport.JW_ETA = new ZDateTime(2006, 5, 11);

			//var declaration = Factory.New<BaseJobDeclaration>();
			//declaration.JE_JS = shipment.PK;

			//var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			//invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.DeliveredAtFrontier;

			//var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			//var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			//var entry1 = declaration.CustomsEntryHeaders.AddNew();
			//declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			//entry1.EntryNumber = "1ONE1";
			//var entry2 = declaration.CustomsEntryHeaders.AddNew();
			//entry2.EntryNumber = "2TWO2";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;

			var voyage1 = Factory.New<JobVoyage>();
			transport.JW_Vessel = "VESSEL";
			transport.JW_VoyageFlight = "123";
			transport.JW_JX = sailing.PK;
			voyage1.ParentConsol = consol;
			voyage1.JV_RegistrationNo = "111";
			voyage1.JV_IsCargoOnly = true;
			sailing.JX_JB = voyageDestination.PK;
			transport.JW_TerminalCutOff = new ZDateTime(2006, 4, 23);
			transport.JW_DepotCutOff = new ZDateTime(2006, 4, 1);

			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2006, 8, 1);
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2006, 8, 3);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2006, 9, 6);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2006, 9, 9);

			shipment.DocsAndCartage.RequiredDocuments.AddNew();

			shipment.Services.AddNew();
			shipment.Services.AddNew();

			var booking = (DtbBooking)GetNewBusinessObjectToWrap();
			booking.KM_JobID = "B00000001";
			booking.KM_TransportReference = "TranRef1234";

			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.MasterBill, "MASTERME");
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.MasterBill, "OTHERMASTER");
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.HouseBill, "HOUSEME");

			var bookingConsolidation = booking.ConsolidationSingleJob;
			bookingConsolidation.KB_GoodsDescription = "GOODS";
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = shipment.TablePrefix;

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			var package = Helper.CreatePackage("packageA", 400, Constants.PkgUnit.Package);
			package.KP_Weight = 55.4m;
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_Volume = 32.450m;
			package.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var picPackage = Helper.CreatePackageDivot(pic, package, 400);
			var dlvPackage = Helper.CreatePackageDivot(dlv, package, 400);

			#endregion

			var fullWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("fullWrapper.SecondaryHeading", "Shipment", fullWrapper.SecondaryHeading);
			AssertEquals("fullWrapper.SecondaryNumber", "S000234567", fullWrapper.SecondaryNumber);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", Constants.ContainerModes.FCL, fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Constants.ContainerModes.LCL, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.ConsolType.Code", Constants.AgentType.Agent, fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ShipmentType.Code", "IMP", fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.Incoterm.Code", Constants.IncoTerms.DeliveredAtFrontier, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "PPD - Prepaid", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.AdditionalTerms", "Hello AdditionalTerms!", fullWrapper.AdditionalTerms);
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "MSA", fullWrapper.ShipmentStatus.Code);

			AssertEquals("fullWrapper.Consignee.CompanyName", "IMPORTER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "SUPPLIER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.PickupAgent.CompanyName", "PICKUPCARTAGE", fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "CTO WHATEVER COMPANY", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", "DELIVERYCARTAGE", fullWrapper.DeliveryAgent.CompanyName);

			AssertEquals("fullWrapper.ImportAgent.CompanyNameAndAddress", "RCV_FWDER\nRF ADDRESS\nAUSTRALIA", fullWrapper.ImportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ExportAgent.CompanyNameAndAddress", "SND_FWDER\r\nSF ADDRESS\nAUSTRALIA", fullWrapper.ExportAgent.CompanyNameAndAddress);

			AssertEquals("fullWrapper.ImportBroker.CompanyName", "IMP_BROKER", fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", "EXP_BROKER", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.BookingParty.CompanyName", ZString.Empty, fullWrapper.BookingParty.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder.CompanyNameAndAddress", "RCV_FWDER\r\nRF ADDRESS\nAUSTRALIA", fullWrapper.LocalForwarder.CompanyNameAndAddress);

			AssertEquals("fullWrapper.NotifyParty", "NOTIFYME", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.ConsolCreditor", "CONS_CREDIT", fullWrapper.ConsolCreditor.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "IMPORTER", fullWrapper.DeliveryAddress.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "SUPPLIER", fullWrapper.PickupAddress.CompanyName);

			AssertEquals("Port Of Loading UNLOCO", "USLAX", fullWrapper.ShipmentRoutes["First"].Origin.UNLOCO);
			AssertEquals("Port Of Loading ETA", new ZDateTime(2006, 5, 8), fullWrapper.ShipmentRoutes["First"].EstimatedArrival);
			AssertEquals("Port Of Loading ATA", new ZDateTime(2006, 5, 9), fullWrapper.ShipmentRoutes["First"].ActualArrival);
			AssertEquals("Port Of Discharge UNLOCO", "NZAKL", fullWrapper.ShipmentRoutes["First"].Destination.UNLOCO);

			AssertEquals("Origin From Consol UNLOCO", "USLAX", fullWrapper.ConsolRoutes["First"].Origin.UNLOCO);
			AssertEquals("Destination From Consol UNLOCO", "NZAKL", fullWrapper.ConsolRoutes["First"].Destination.UNLOCO);

			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USDNV", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "ERXXX", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 13), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 13), fullWrapper.Destination.ActualDate);

			AssertEquals("fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero", "400 PKG", fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "32.450 M3", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Weight.ValueAndUnitCodeBlankIfZero", "55.4 KG", fullWrapper.Weight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", "32.450 M3", fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero", "354 BOX", fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero);

			AssertEquals("fullWrapper.ShippedOnBoardType.Code", "SOB", fullWrapper.ShippedOnBoardType.Code);
			AssertEquals("fullWrapper.ReleaseType.Code", "NXS", fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.FreightRate.AmountAndCurrencyCode", "123.45 AUD", fullWrapper.FreightRate.AmountAndCurrencyCode);

			ZDateTime dateTimeCreated = shipment.Logs.CreatedDateUtc;
			AssertEquals("fullWrapper.ConsolDateCreated", dateTimeCreated, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.ShipmentDateCreated", dateTimeCreated, fullWrapper.ShipmentDateCreated);

			AssertEquals("fullWrapper.MasterBill", "MASTERME, OTHERMASTER", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "Ocean Bill Of Lading", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBillHeading", "House Bill Of Lading", fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.HouseBill", "HOUSEME", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HBLIssueDate", new ZDateTime(2005, 4, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.GoodsDescription", "GOODS", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.MarksAndNumbers", "BROKEN HEARTS", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.NoCopyBills", 5, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.NoOriginalBills", 6, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2006, 12, 25), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.ExportAgentsReference", "AGENT_1", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "S000234567", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", "S000234567", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.BookingReference", "BOOK_A_HOOKER", fullWrapper.BookingReference);
			AssertEquals("fullWrapper.ConsolPaymentType", "CCX", fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.LCL, fullWrapper.HBLContainerMode);
			AssertEquals("fullWrapper.FullCartageInstructions", "Pick it up", fullWrapper.FullCartageInstructions);
			AssertEquals("fullWrapper.FullHandlingInstructions", "Handle with no care", fullWrapper.FullHandlingInstructions);
			AssertEquals("fullWrapper.CustomsEntryNumber", "COM 123", fullWrapper.CustomsEntryNumber);
			AssertEquals("fullWrapper.InspectionType", "UNK - Unknown - No Security Measures Taken", fullWrapper.InspectionType.ToString());

			AssertEquals("fullWrapper.DeliveryCartageAdvised", new ZDateTime(2006, 1, 1), fullWrapper.DeliveryCartageAdvised);
			AssertEquals("fullWrapper.DeliveryFrom", new ZDateTime(2006, 1, 2), fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2006, 1, 3), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.DeliveryRequiredBy", new ZDateTime(2006, 1, 4), fullWrapper.DeliveryRequiredBy);
			AssertEquals("fullWrapper.PickupCartageAdvised", new ZDateTime(2006, 2, 1), fullWrapper.PickupCartageAdvised);
			AssertEquals("fullWrapper.PickupFrom", new ZDateTime(2006, 2, 2), fullWrapper.PickupFrom);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2006, 2, 3), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.PickupRequiredBy", new ZDateTime(2006, 2, 4), fullWrapper.PickupRequiredBy);
			AssertEquals("fullWrapper.PickupDateOfReceipt", ZDateTime.Empty, fullWrapper.PickupDateOfReceipt);

			AssertEquals("fullWrapper.CustomAttribute1", "ONE", fullWrapper.CustomAttribute1);
			AssertEquals("fullWrapper.CustomAttribute2", "TWO", fullWrapper.CustomAttribute2);
			AssertEquals("fullWrapper.CustomDate1", new ZDateTime(2001, 1, 1), fullWrapper.CustomDate1);
			AssertEquals("fullWrapper.CustomDate2", new ZDateTime(2002, 2, 2), fullWrapper.CustomDate2);
			AssertEquals("fullWrapper.CustomDecimal1", 1.1m, fullWrapper.CustomDecimal1);
			AssertEquals("fullWrapper.CustomDecimal2", 2.22m, fullWrapper.CustomDecimal2);
			AssertEquals("fullWrapper.CustomFlag1", true, fullWrapper.CustomFlag1);
			AssertEquals("fullWrapper.CustomFlag2", false, fullWrapper.CustomFlag2);

			AssertEquals("fullWrapper.CommercialInvoices.Count", 0, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 0, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "665.33 HKD", fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.InsuranceValue.AmountAndCurrencyCode", "898.22 EUR", fullWrapper.InsuranceValue.AmountAndCurrencyCode);

			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.ShipmentRoutes.Count", 2, fullWrapper.ShipmentRoutes.Count);

			AssertEquals("fullWrapper.Orders.Count", 1, fullWrapper.Orders.Count);

			AssertEquals("fullWrapper.FreightJobs.Count", 0, fullWrapper.FreightJobs.Count);

			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);

			AssertEquals("fullWrapper.Services.Count", 2, fullWrapper.Services.Count);

			AssertEquals("fullWrapper.ConsolNumber", "C00012345", fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.ArrivalReference", "ARRIVAL REFERENCE", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "BOOTH A23", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.UnAllocatedWeight", 55.4m, fullWrapper.UnAllocatedWeight);
			AssertEquals("fullWrapper.UnAllocatedVolume", 32.45m, fullWrapper.UnAllocatedVolume);
			AssertEquals("fullWrapper.UnAllocatedPackages", 334, fullWrapper.UnAllocatedPackages);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", "USDNV", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "ERXXX", fullWrapper.DeliveryLocation.UNLOCO);

			AssertEquals("fullWrapper.ReceivingForwarder.CompanyNameAndAddress", "RCV_FWDER\nRF ADDRESS\nAUSTRALIA", fullWrapper.ReceivingForwarder.CompanyNameAndAddress);
			AssertEquals("fullWrapper.SendingForwarder.CompanyNameAndAddress", "SND_FWDER\r\nSF ADDRESS\nAUSTRALIA", fullWrapper.SendingForwarder.CompanyNameAndAddress);

			AssertEquals("fullWrapper.CartageInfo", typeof(CartageInfoWrapperFromShipment), fullWrapper.CartageInfo.GetType());

			AssertOrgWrappersReturnRightTypes(fullWrapper);
		}

		#endregion

		#region TestWrapperNoParent

		public void TestWrapperNoParent()
		{
			#region Setup

			var booking = (DtbBooking)GetNewBusinessObjectToWrap();
			booking.KM_JobID = "B00000001";
			booking.KM_TransportReference = "TranRef1234";

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.Bookings.Add(booking);

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			var package = Helper.CreatePackage("packageA", 400, Constants.PkgUnit.Package);
			package.KP_Weight = 55.4m;
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_Volume = 32.450m;
			package.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var picPackage = Helper.CreatePackageDivot(pic, package, 400);
			var dlvPackage = Helper.CreatePackageDivot(dlv, package, 400);

			#endregion

			var noParentWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("noParentWrapper.SecondaryHeading", "Booking Party Ref.", noParentWrapper.SecondaryHeading);
			AssertEquals("noParentWrapper.SecondaryNumber", "", noParentWrapper.SecondaryNumber);
			AssertEquals("noParentWrapper.Shipment", null, noParentWrapper.FreightShipment);

			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportAdditionalReferenceTypes.Codes.BookingPartyReference, "S00000001");
			AssertEquals("noParentWrapper.SecondaryNumber", "S00000001", noParentWrapper.SecondaryNumber);
		}

		#endregion

		#region TestWrapperHeaderContainsLogo

		public void TestWrapperHeaderContainsLogo()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "B00000001";
			booking.KM_TransportReference = "TranRef1234";

			var relatedJobHeader = new JobHeader.Loader(parent).TryLoadOrCreate();

			var bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);

			Env.Registry.QuotationDocumentLogo = null;
			AssertNull("No header logo", bookingWrapper.Rating.Logo);

			Env.Registry.QuotationDocumentLogo = new Bitmap(1, 1);
			AssertEquals("Header logo should be Quotations Logo", new Size(1, 1), bookingWrapper.Rating.Logo.Size);
		}

		#endregion

		#region TestWrapperHeaderWithParentContainsLogo

		public void TestWrapperHeaderWithParentContainsLogo()
		{
			var dummyBookingParent = Factory.New<ForwardingShipment>();
			var consolidation = Helper.CreateConsolidation(dummyBookingParent);
			var bookingMovement = Helper.CreateBooking(consolidation);

			bookingMovement.KM_JobID = "B00000001";
			bookingMovement.KM_TransportReference = "TranRef1234";

			var bookingMovementWrapper = new FreightWrapperFromDtbBooking(bookingMovement, Factory);
			Env.Registry.QuotationDocumentLogo = new Bitmap(1, 1);
			AssertNull("No header logo", bookingMovementWrapper.Rating);
		}

		#endregion

		#region TestRatingWrapper

		public void TestRatingWrapper()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "B00000001";
			booking.KM_TransportReference = "TranRef1234";

			var relatedJobHeader = new JobHeader.Loader(parent).TryLoadOrCreate();
			var dateJOP = new ZDateTime(2013, 9, 30);
			relatedJobHeader.JH_A_JOP = dateJOP;

			var bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertNotNull(bookingWrapper.Rating);
			AssertEquals(bookingWrapper.Rating.ValidFrom, dateJOP);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			AssertNull("ServiceLevel", Wrapper.ServiceLevel);

			var booking = GetBookingBizO();
			booking.KM_JobID = "ABCD";
			booking.KM_TransportReference = "TranRef1234";
			booking.KM_RS_NKServiceLevel = "DIR";
			var bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("Service Level", "DIR", bookingWrapper.ServiceLevel.Code);
		}

		#endregion

		#region TestCarrierServiceLevel

		public void TestCarrierServiceLevel()
		{
			AssertNull("CarrierServiceLevel", Wrapper.CarrierServiceLevel);

			var booking = GetBookingBizO();
			booking.KM_JobID = "ABCD";
			booking.KM_TransportReference = "TranRef1234";
			booking.KM_PL_NKCarrierServiceLevel = "STD";
			var bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("Carrier Service Level", "STD", bookingWrapper.CarrierServiceLevel.Code);
		}

		#endregion

		#region TestAWBSecurityInspectionStatus

		public void TestAWBSecurityInspectionStatus()
		{
			var booking = Helper.CreateBooking();
			var fullWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			AssertEquals("AWBSecurityInspectionStatus", "", fullWrapper.AWBSecurityInspectionStatus);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();
			shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var consolidation = Helper.CreateConsolidation(shipment);
			booking = Helper.CreateBooking(consolidation);
			fullWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AWBNonApprovedExporterText");
			AssertEquals("AWBSecurityInspectionStatus", "AWBNonApprovedExporterText", fullWrapper.AWBSecurityInspectionStatus);
		}
		#endregion

		#region TestHazardous

		public void TestHazardous()
		{
			var booking = GetBookingBizO();
			AssertEquals("No Hazardous", false, booking.IsAnyPackageHazardous);

			var packageJob = booking.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			container.UNDGs.AddNew();

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			AssertEquals("Assigned Package.", 1, booking.AssignedPackages.Count);
			AssertEquals("Has Hazardous", true, booking.IsAnyPackageHazardous);

			var bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("Has Hazardous", true, bookingWrapper.Hazardous);
		}

		#endregion

		#region TestCarrierAccount

		public void TestCarrierAccount()
		{
			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_Code = "ABCDEFG";

			var booking = Helper.CreateBooking(transportCo);
			var bookingWrapper = GetFreightWrapper(booking);

			AssertNull("No Accounts in Carrier, the CarrierAcount should be null", bookingWrapper.CarrierAccount);

			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_OH_Carrier = transportCo.PK;
			var carrierAccount2 = Factory.New<OrgCarrierAccount>();
			carrierAccount2.OAN_OH_Carrier = transportCo.PK;

			AssertEquals("Precondition:", 2, transportCo.CarrierAccounts.Count);

			booking.KM_OAN_CarrierAccount = carrierAccount.PK;

			bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("Carrier Account should return that used in TransportBooking", carrierAccount, bookingWrapper.CarrierAccount.WrappedObject);
		}

		#endregion

		#region TestGetTransportZone

		public void TestGetTransportZone_WithoutTransportZoneSet()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);
			var query = new ZQuery();
			query.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, booking.Address.OrganisationPK);
			query.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);

			var provider = Factory.LoadTop1<RateTransportProvider>(query);
			AssertNull("Precondition: No domestic sets", provider);

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasTransportZoneSetButInactive()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			prov.TP_IsActive = false;

			var query = new ZQuery();
			query.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, booking.Address.OrganisationPK);
			query.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);
			var provider = Factory.LoadTop1<RateTransportProvider>(query);

			Factory.Save();

			AssertNull("Precondition: No domestic sets", provider);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasTransportZoneSetWithoutZones()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			Factory.Save();

			AssertEquals("Precondition: No Zones", 0, prov.Zones.Count);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasTransportZoneSetButZoneInactive()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = prov.Zones.AddNew();
			zone.TZ_ZoneName = "V0";
			zone.TZ_IsActive = false;

			Factory.Save();

			AssertEquals("Precondition: Zone is inactive", false, zone.TZ_IsActive);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasTransportZoneSetWithoutZoneDefinition()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = prov.Zones.AddNew();
			zone.TZ_ZoneName = "V0";

			Factory.Save();

			AssertEquals("Precondition: Transport Zone has no ZoneItem", 0, zone.Items.Count);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasTransportZoneSetButWithoutParent()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var booking = Helper.CreateBooking(transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			AssertEquals("Precondition: TB has no parent", "", booking.ParentID);

			var bookingWrapper = GetFreightWrapper(booking);

			AssertEquals("TransportZone should be empty", "", bookingWrapper.TransportZone);
		}

		public void TestGetTransportZone_HasTransportZoneSetButConsigneeWithoutPostCode()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			AssertEquals("Precondition: Consignee address has no post code", "", consigneeDocAddress.Postcode);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasTransportZoneSetAndConsigneeButPostCodeNotInRange()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("2000");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsignee_RealAddress()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "V0", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsignee_OverriddenAddress()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", true);

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "V0", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetWithMultiZonesAndConsignee()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1600");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");
			var zone2 = Helper.CreateZoneWithPostCodes("V1", prov, "1500", "2000");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "V1", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectMultiTransportZoneSetsAndConsignee()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("AU", prov, "1000", "1500");

			var prov2 = Helper.CreateZoneRateProvider("US", transportCo);
			var zone2 = Helper.CreateZoneWithPostCodes("US", prov, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "AU", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsigneeButCountryCodeIsEmpty_RealAddress()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", false, false);

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory);
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("CountryCode should be empty", "", mock.Object.Consignee.MainAddress.Country.Code);
			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetAndConsigneeButCountryCodeIsEmpty_OverriddenAddress()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200", true, false);

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);

			var prov = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			AssertEquals("Precondition: Consignee is override", true, consigneeDocAddress.E2_AddressOverride);
			AssertEquals("Precondition: CountryCode is empty", "", consigneeDocAddress.E2_RN_NKCountryCode);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory);
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("CountryCode should be empty", "", mock.Object.Consignee.MainAddress.Country.Code);
			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var pickUpOrg = Helper.CreateOrganisation("PickUp");
			pickUpOrg.MainAddress.OA_City = "TST2";
			pickUpOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, pickUpOrg.Addresses.MainAddress);

			var city = Helper.CreateCityTown("TST", "AU");
			var city2 = Helper.CreateCityTown("TST2", "AU");

			var prov = Helper.CreateZoneRateProvider("AU", transportCo, city);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			var prov2 = Helper.CreateZoneRateProvider("AU", transportCo, city2);
			var zone2 = Helper.CreateZoneWithPostCodes("V1", prov2, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "V1", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee_NoFirstPickUpInstruction()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var dlvOrg = Helper.CreateOrganisation("DLV");
			dlvOrg.MainAddress.OA_City = "TST";
			dlvOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvOrg.Addresses.MainAddress);

			var city = Helper.CreateCityTown("TST", "AU");

			var prov = Helper.CreateZoneRateProvider("AU", transportCo, city);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should be empty", "", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee_FallbackCanMatchOriginHubLocation()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var pickUpOrg = Helper.CreateOrganisation("PickUp");
			pickUpOrg.MainAddress.OA_City = "TST";
			pickUpOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, pickUpOrg.Addresses.MainAddress);

			var city = Helper.CreateCityTown("TST", "AU");

			var prov = Helper.CreateZoneRateProvider("AU", transportCo, city);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			var prov2 = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone2 = Helper.CreateZoneWithPostCodes("V1", prov2, "1000", "1500");

			Factory.Save();

			AssertEquals("Precondition: Can match on Zone Hub Location", true, city.R9_InternationalName == pickUpOrg.MainAddress.City);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "V0", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		public void TestGetTransportZone_HasCorrectTransportZoneSetsWithDifferentZoneHubLocationsAndConsignee_FallbackCannotMatchOriginHubLocation()
		{
			var transportCo = Helper.CreateOrganisation("ABCDEFG");
			var consigneeDocAddress = CreateConsigneeDocAddress("1200");

			var pickUpOrg = Helper.CreateOrganisation("PickUp");
			pickUpOrg.MainAddress.OA_City = "TST2";
			pickUpOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.ConsigneeDocAddress = consigneeDocAddress;

			var booking = CreateBookingWithParent(parent, transportCo);
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, pickUpOrg.Addresses.MainAddress);

			var city = Helper.CreateCityTown("TST", "AU");
			var city2 = Helper.CreateCityTown("TST2", "AU");
			var city3 = Helper.CreateCityTown("TST3", "AU");

			var prov = Helper.CreateZoneRateProvider("AU", transportCo, city);
			var zone = Helper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			var prov2 = Helper.CreateZoneRateProvider("AU", transportCo);
			var zone2 = Helper.CreateZoneWithPostCodes("V1", prov2, "1000", "1500");

			var prov3 = Helper.CreateZoneRateProvider("AU", transportCo, city3);
			var zone3 = Helper.CreateZoneWithPostCodes("V2", prov3, "1000", "1500");

			Factory.Save();

			AssertEquals("Precondition: Cannot match on Zone Hub Location", false, city.R9_InternationalName == pickUpOrg.MainAddress.City);

			var mock = new Mock<FreightWrapperFromDtbBooking>(booking, Factory) { CallBase = true };
			mock.Protected().Setup<OrganisationWrapper>("GetConsignee").Returns(GetConsigneeWrapper(consigneeDocAddress));

			AssertEquals("TransportZone should not empty", "V1", mock.Object.TransportZone);
			mock.VerifyAll();
		}

		DtbBooking CreateBookingWithParent(IDtbBookingParent parent, OrgHeader transportCo)
		{
			var booking = Helper.CreateConsolidation(parent).Bookings.AddNew();
			booking.Address.OrganisationPK = transportCo.PK;

			return booking;
		}

		JobDocAddress CreateConsigneeDocAddress(string postCode, bool isOverride = false, bool hasCountry = true)
		{
			var consigneeDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			if (isOverride)
			{
				consigneeDocAddress.E2_AddressOverride = true;
				consigneeDocAddress.E2_RN_NKCountryCode = hasCountry ? "AU" : "";
				consigneeDocAddress.E2_Postcode = postCode;
			}
			else
			{
				var consignee = Helper.CreateOrganisation("CONSIGNEE");
				var consigneeAddress = Helper.AddAddressToOrganisation(consignee, "Address1", OrgAddressType.Delivery);
				consigneeAddress.OA_PostCode = postCode;
				consigneeAddress.OA_RN_NKCountryCode = hasCountry ? "AU" : "";
				consigneeAddress.OA_City = "ADELAIDE";
				consigneeDocAddress.E2_OA_Address = consigneeAddress.PK;
			}

			return consigneeDocAddress;
		}

		OrganisationWrapper GetConsigneeWrapper(JobDocAddress consigneeDocAddress)
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, consigneeDocAddress, Factory);
		}

		#endregion

		#region TestGetTransportAddresses

		public void TestGetTransportAddresses()
		{
			var emptyBooking = GetEmptyBookingBizO();
			var emptyBookingWrapper = GetFreightWrapper(emptyBooking);
			AssertEquals(0, emptyBookingWrapper.TransportAddresses.Count);

			var booking = GetEmptyBookingBizO();
			var org = Helper.CreateOrganisation("TEST");
			var address_pickup = Helper.AddAddressToOrganisation(org, "1 Pickup Road", OrgAddressType.PickupAndDelivery);
			var address_delivery = Helper.AddAddressToOrganisation(org, "2 Delivery Road", OrgAddressType.PickupAndDelivery);
			var pickUpInstruction = Helper.CreateInstruction(booking, "", LocalCartageJobOrgTypeList.Codes.CNR, address_pickup);
			pickUpInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp; // we want to set Instruction Type after, to prevent creation of Depot Instruction in Consignments

			Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CFS, address_delivery);
			Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CFS, address_delivery); // should skip
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null); // not the last, so add
			using (deliveryInstruction.SuspendOnDocAddressChanged())
			{
				deliveryInstruction.Address.E2_OA_Address = address_pickup.PK; // need to prevent OnDocAddress firing as it will null out the Depot Instruction's Address
			}

			var bookingWrapper = GetFreightWrapper(booking);
			var wrappedJobDocAddresses = bookingWrapper.TransportAddresses.Cast<AddressWrapper>().Select(a => a.WrappedObject);
			var wrappedOrgAddresses = wrappedJobDocAddresses.Cast<JobDocAddress>().Select(a => a.Address);
			AssertContainsExactElementsInAnyOrder(new[] { address_pickup, address_delivery, address_pickup }, wrappedOrgAddresses);
		}

		#endregion

		#region TestGetJobNumberAndHeading

		public void TestGetJobNumberAndHeading()
		{
			var booking = GetBookingBizO();
			booking.KM_JobID = "ABCD";
			var bookingWrapper = GetFreightWrapper(booking);

			AssertEquals("ABCD", bookingWrapper.JobNumber);
			AssertEquals(JobNumberHeadingToTest, bookingWrapper.JobNumberHeading);
		}

		#region JobNumberHeadingToTest

		string JobNumberHeadingToTest
		{
			get
			{
				return "Transport Booking";
			}
		}

		#endregion

		#endregion

		#region TestGetContainers

		public void TestGetContainers()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage container1_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage container2_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage container3_Instruction2 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage container4_NoInstruction = packageJob.Packages.AddNew(Constants.PkgUnit.Container);

			// way to determine which container is which and that qty is used from packages rather that from Divots.
			container1_Instruction1.KP_PackageQty = 1;
			container2_Instruction1.KP_PackageQty = 2;
			container3_Instruction2.KP_PackageQty = 3;
			container4_NoInstruction.KP_PackageQty = 4;

			var booking = GetBookingBizO();
			DtbBookingInstruction instruction1 = Helper.CreateInstruction(booking);
			DtbBookingInstruction instruction2 = Helper.CreateInstruction(booking);
			DtbBookingInstruction instruction3 = Helper.CreateInstruction(booking);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot1 = Helper.CreatePackageDivot(instruction1, container1_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot2 = Helper.CreatePackageDivot(instruction1, container2_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction2_PkgDivot1 = Helper.CreatePackageDivot(instruction2, container3_Instruction2, 1);

			var bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("Wrong number of Containers were created", 3, bookingWrapper.Containers.Count);
			AssertContainsContainerFor(bookingWrapper.Containers, container1_Instruction1);
			AssertContainsContainerFor(bookingWrapper.Containers, container2_Instruction1);
			AssertContainsContainerFor(bookingWrapper.Containers, container3_Instruction2);
		}

		void AssertContainsContainerFor(ContainerWrapperCollection containerWrapperCollection, PkgPackage expectedContainer)
		{
			foreach (ContainerWrapper containerWrapper in containerWrapperCollection)
			{
				if (containerWrapper.ContainerCount == expectedContainer.KP_PackageQty)
				{
					return;
				}
			}
			Fail(string.Format("No Container could be found for container package with Qty '{0}'", expectedContainer.KP_PackageQty));
		}

		#endregion

		#region TestCustomsEntries

		public void TestCustomsEntries()
		{
			var booking = GetBookingBizO();
			var additionalRefNumber = Factory.New<CusEntryNumber>();
			additionalRefNumber.CE_EntryType = "TRF";
			additionalRefNumber.CE_EntryNum = "123";
			additionalRefNumber.CE_ParentID = booking.PK;
			booking.AdditionalReferenceNumbers.Add(additionalRefNumber);

			var bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("bookingWrapper.CustomsEntries.Count", 1, bookingWrapper.CustomsEntries.Count);
			AssertEquals("bookingWrapper.CustomsEntries[0].EntryNumber", "123", bookingWrapper.CustomsEntries[0].EntryNumber);
			var exceptedCode = (bookingWrapper is FreightWrapperFromDtbBooking) ? "TRF" : "Transport Reference Number";

			AssertEquals("bookingWrapper.CustomsEntries[0].EntryNumber", exceptedCode, bookingWrapper.CustomsEntries[0].EntryType.Code);
		}

		#endregion

		#region TestGetPackages

		public void TestGetPackages()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage package1_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Box);
			PkgPackage package2_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage package3_Instruction2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage package4_NoInstruction = packageJob.Packages.AddNew(Constants.PkgUnit.Bag);

			// way to determine which package is which and that qty is used from packages rather that from Divots.
			package1_Instruction1.KP_PackageQty = 1;
			package2_Instruction1.KP_PackageQty = 2;
			package3_Instruction2.KP_PackageQty = 3;
			package4_NoInstruction.KP_PackageQty = 4;

			var tranport = GetBookingBizO();
			DtbBookingInstruction instruction1 = Helper.CreateInstruction(tranport);
			DtbBookingInstruction instruction2 = Helper.CreateInstruction(tranport);
			DtbBookingInstruction instruction3 = Helper.CreateInstruction(tranport);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot1 = Helper.CreatePackageDivot(instruction1, package1_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction1_PkgDivot2 = Helper.CreatePackageDivot(instruction1, package2_Instruction1, 1);
			DtbBookingInstructionPkgDivot instruction2_PkgDivot1 = Helper.CreatePackageDivot(instruction2, package3_Instruction2, 1);

			var bookingWrapper = GetFreightWrapper(tranport);
			AssertEquals("Wrong number of Packages were created", 3, bookingWrapper.Packages.Count);
			AssertContainsPackageFor(bookingWrapper.Packages, package1_Instruction1);
			AssertContainsPackageFor(bookingWrapper.Packages, package2_Instruction1);
			AssertContainsPackageFor(bookingWrapper.Packages, package3_Instruction2);
		}

		void AssertContainsPackageFor(PackageWrapperCollection packageWrapperCollection, PkgPackage expectedPackage)
		{
			foreach (PackageWrapper packageWrapper in packageWrapperCollection)
			{
				if (packageWrapper.Packages.Value == expectedPackage.KP_PackageQty)
				{
					return;
				}
			}
			Fail(string.Format("No Packages could be found for package with Qty '{0}'", expectedPackage.KP_PackageQty));
		}

		#endregion

		#region TestGetUNDGs

		public void TestGetUNDGs()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			PkgPackage container_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage package_Instruction1 = packageJob.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage package_Instruction2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage package_Instruction2_NoUNDG = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage package_NoInstruction = packageJob.Packages.AddNew(Constants.PkgUnit.Bag);

			UNDGDataItem undgDataItem1_Container_Instruction1 = CreateUNDGDataItem(container_Instruction1, "AAAA", "David1");
			UNDGDataItem undgDataItem2_Container_Instruction1 = CreateUNDGDataItem(container_Instruction1, "BBBB", "David2");
			UNDGDataItem undgDataItem_Package_Instruction1 = CreateUNDGDataItem(package_Instruction1, "CCCC", "David3");
			UNDGDataItem undgDataItem_Package_Instruction2 = CreateUNDGDataItem(package_Instruction2, "DDDD", "David4");
			UNDGDataItem undgDataItem_Package_NoInstruction = CreateUNDGDataItem(package_NoInstruction, "EEEE", "David5");

			var booking = GetBookingBizO();
			DtbBookingInstruction instruction1 = Helper.CreateInstruction(booking);
			DtbBookingInstruction instruction2 = Helper.CreateInstruction(booking);
			DtbBookingInstruction instruction3 = Helper.CreateInstruction(booking);
			Helper.CreatePackageDivot(instruction1, container_Instruction1, 1);
			Helper.CreatePackageDivot(instruction1, package_Instruction1, 1);
			Helper.CreatePackageDivot(instruction2, package_Instruction2, 1);
			helper.CreatePackageDivot(instruction2, package_Instruction2_NoUNDG, 1);

			var bookingWrapper = GetFreightWrapper(booking);
			AssertEquals("Wrong number of UNDGs were created", 4, bookingWrapper.UNDGs.Count);
			AssertContainsUNDGFor(bookingWrapper.UNDGs, undgDataItem1_Container_Instruction1);
			AssertContainsUNDGFor(bookingWrapper.UNDGs, undgDataItem2_Container_Instruction1);
			AssertContainsUNDGFor(bookingWrapper.UNDGs, undgDataItem_Package_Instruction1);
			AssertContainsUNDGFor(bookingWrapper.UNDGs, undgDataItem_Package_Instruction2);
		}

		UNDGDataItem CreateUNDGDataItem(PkgPackage package, ZString undgSubstanceCode, ZString contactName)
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_Code = undgSubstanceCode;
			undgSubstance.DG_UNNO = undgSubstanceCode;

			var orgContact = Factory.New<OrgContact>();
			orgContact.OC_ContactName = contactName;

			UNDGDataItem result = package.UNDGs.AddNew();
			result.DI_DG = undgSubstance.PK;
			result.DI_OC_DGContact = orgContact.PK;
			return result;
		}

		void AssertContainsUNDGFor(UNDGSubstanceWrapperCollection allUNDGWrappers, UNDGDataItem undgToFind)
		{
			foreach (UNDGSubstanceWrapper undgWrapper in allUNDGWrappers)
			{
				if (undgToFind.Substance != null && undgWrapper.UNNumber == undgToFind.Substance.DG_Code && undgWrapper.DGContact.FullName == undgToFind.DGContact.OC_ContactName)
				{
					return;
				}
			}
			Fail(string.Format("UNDG with substance code: {0} and contact name: {1} couldn't be found.", undgToFind.Substance?.DG_Code, undgToFind.DGContact.OC_ContactName));
		}

		#endregion

		#region TestQtyWeightVolume

		public void TestQtyWeightVolume_NonPackages()
		{
			var booking = GetBookingBizO();
			var wrapper = GetFreightWrapper(booking);

			AssertEquals("Non packages, Weight should be 0m.", 0m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("Non packages, Volume should be 0m.", 0m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
		}

		public void TestQtyWeightVolume_DifferentUnits()
		{
			var booking = GetBookingBizO();
			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var mlt = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi);
			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			var pA = Helper.CreatePackage("pA", 15, Constants.PkgUnit.Pallet);
			var pB = Helper.CreatePackage("pB", 20, Constants.PkgUnit.Box);
			var pC = Helper.CreatePackage("pC", 1, Constants.PkgUnit.Container);
			pA.KP_Weight = 150;
			pA.KP_WeightUQ = Constants.Weight.Kilograms;
			pA.KP_Volume = 150;
			pA.KP_VolumeUQ = Constants.Volume.CubicMetres;

			pB.KP_Weight = 200;
			pB.KP_WeightUQ = Constants.Weight.Pounds;
			pB.KP_Volume = 200;
			pB.KP_VolumeUQ = Constants.Volume.CubicFeet;

			pC.KP_Weight = 1.5;
			pC.KP_WeightUQ = Constants.Weight.Tonnes;
			pC.KP_Volume = 24;
			pC.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var pic1pA = Helper.CreatePackageDivot(pic1, pA, 5);
			var pic1pB = Helper.CreatePackageDivot(pic1, pB, 2);
			var pic1pC = Helper.CreatePackageDivot(pic1, pC, 1);
			var pic2pA = Helper.CreatePackageDivot(pic2, pA, 6);
			var pic2pB = Helper.CreatePackageDivot(pic2, pB, 12);
			var pic2pC = Helper.CreatePackageDivot(pic2, pC, 1);

			var mlt_pA = Helper.CreatePackageDivot(mlt, pA, 10);
			var mlt_pB = Helper.CreatePackageDivot(mlt, pB, 5);
			var mlt_pC = Helper.CreatePackageDivot(mlt, pC, 1);

			var dlv1pA = Helper.CreatePackageDivot(dlv1, pA, 2);
			var dlv1pB = Helper.CreatePackageDivot(dlv1, pB, 3);
			var dlv1pC = Helper.CreatePackageDivot(dlv1, pC, 1);
			var dlv2pA = Helper.CreatePackageDivot(dlv2, pA, 7);
			var dlv2pB = Helper.CreatePackageDivot(dlv2, pB, 14);
			var dlv2pC = Helper.CreatePackageDivot(dlv2, pC, 1);

			var wrapper = GetFreightWrapper(booking);
			AssertEquals("pA(5+6) + pB(3+14) + pC(0)", 28m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("PKG", wrapper.ShipmentOuterPacksQty.Unit.Code);
			AssertEquals("110 + 77.1 + 1500", 1687.1m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("110 + 4.814 + 24", 138.814m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
		}

		public void TestQtyWeightVolume_SameUnit()
		{
			var booking = GetBookingBizO();
			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var mlt = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi);
			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			var pA = Helper.CreatePackage("pA", 15, Constants.PkgUnit.Pallet);
			var pB = Helper.CreatePackage("pB", 20, Constants.PkgUnit.Box);
			var pC = Helper.CreatePackage("pC", 1, Constants.PkgUnit.Container);
			pA.KP_Weight = 150;
			pA.KP_WeightUQ = Constants.Weight.Pounds;
			pA.KP_Volume = 150;
			pA.KP_VolumeUQ = Constants.Volume.CubicFeet;

			pB.KP_Weight = 200;
			pB.KP_WeightUQ = Constants.Weight.Pounds;
			pB.KP_Volume = 200;
			pB.KP_VolumeUQ = Constants.Volume.CubicFeet;

			pC.KP_Weight = 1.5;
			pC.KP_WeightUQ = Constants.Weight.Pounds;
			pC.KP_Volume = 24;
			pC.KP_VolumeUQ = Constants.Volume.CubicFeet;

			var pic1pA = Helper.CreatePackageDivot(pic1, pA, 5);
			var pic1pB = Helper.CreatePackageDivot(pic1, pB, 2);
			var pic1pC = Helper.CreatePackageDivot(pic1, pC, 1);
			var pic2pA = Helper.CreatePackageDivot(pic2, pA, 6);
			var pic2pB = Helper.CreatePackageDivot(pic2, pB, 12);
			var pic2pC = Helper.CreatePackageDivot(pic2, pC, 1);

			var mlt_pA = Helper.CreatePackageDivot(mlt, pA, 10);
			var mlt_pB = Helper.CreatePackageDivot(mlt, pB, 5);
			var mlt_pC = Helper.CreatePackageDivot(mlt, pC, 1);

			var dlv1pA = Helper.CreatePackageDivot(dlv1, pA, 2);
			var dlv1pB = Helper.CreatePackageDivot(dlv1, pB, 3);
			var dlv1pC = Helper.CreatePackageDivot(dlv1, pC, 1);
			var dlv2pA = Helper.CreatePackageDivot(dlv2, pA, 7);
			var dlv2pB = Helper.CreatePackageDivot(dlv2, pB, 14);
			var dlv2pC = Helper.CreatePackageDivot(dlv2, pC, 1);

			var wrapper = GetFreightWrapper(booking);
			AssertEquals("pA(5+6) + pB(3+14) + pC(0)", 28m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("PKG", wrapper.ShipmentOuterPacksQty.Unit.Code);
			AssertEquals("110 + 170 + 1.5", 281.5m, wrapper.Weight.Value);
			AssertEquals("LB", Constants.Weight.Pounds, wrapper.Weight.Unit.Code);
			AssertEquals("110 + 170 + 24", 304m, wrapper.Volume.Value);
			AssertEquals("CF", Constants.Volume.CubicFeet, wrapper.Volume.Unit.Code);
		}

		#endregion

		#region ExpectedCarrierTypeDescription

		protected override ZString ExpectedCarrierTypeDescription
		{
			get { return "Transport Company"; }
		}

		#endregion

		#region IDocTypeCode Members

		public void TestDocTypeCode()
		{
			var booking = GetBookingBizO();
			var wrapper = GetFreightWrapper(booking);

			((IDocTypeCode)wrapper).DocTypeCode = "CAD";
			AssertEquals("CAD", ((IDocTypeCode)wrapper).DocTypeCode);

			((IDocTypeCode)wrapper).DocTypeCode = "CAR";
			AssertEquals("CAR", ((IDocTypeCode)wrapper).DocTypeCode);
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Transport Booking" },
					{ "SecondaryHeading", "Booking Party Ref." }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
AssuredParty :  is null
BookingParty :  is null
Buyer :  is null
CaratagePickupMode :  is null
CarrierServiceLevel :  is null
ChargeableWeight :  is null
ClaimsPayableBy :  is null
Consignee :  is null
Consignor :  is null
ConsolContainerMode :  is null
ConsolCreditor :  is null
ConsolTransportMode :  is null
ConsolType :  is null
CTOArrival :  is null
DeliveryAddress :  is null
DeliveryAgent :  is null
DepartureCFSTransport :  is null
Destination :  is null
ExportAgent :  is null
ExportBroker :  is null
ExportReceivalAddress :  is null
ExportReceivingCTOAddress :  is null
ExportReceivingDepotAddress :  is null
FreightRate :  is null
GoodsAvailableAt :  is null
GoodsValue :  is null
ImportAgent :  is null
ImportArrivalCTOAddress :  is null
ImportBroker :  is null
IncoTerm :  is null
InsuranceValue :  is null
InsuredBy :  is null
InterestedRoute :  is null
InvoicingJob :  is null
LocalForwarder :  is null
NotifyParty :  is null
OrderTransportMode :  is null
Origin :  is null
PickupAddress :  is null
PickupAgent :  is null
PickupCFSAddress :  is null
Principal :  is null
QueryClaim :  is null
Rating :  is null
ReceivingForwarder :  is null
ReleaseType :  is null
RunSheet :  is null
SalesRep :  is null
SendingForwarder :  is null
ServiceLevel :  is null
ShipmentContainerMode :  is null
ShipmentStatus :  is null
ShipmentTransportMode :  is null
ShipmentType :  is null
ShippedOnBoardType :  is null
StorageTime :  is null
Supplier :  is null
SupplierBuyerLink :  is null
SurveyReportParty :  is null
TranshipmentFreightConsol :  is null
UnpackCFSAddress :  is null
WarehouseJob :  is null";
			}
		}

		DtbBooking GetBookingBizO()
		{
			return Helper.CreateBooking();
		}

		FreightWrapperFromDtbBooking GetFreightWrapper(DtbBooking booking)
		{
			return new FreightWrapperFromDtbBooking(booking, Factory);
		}

		#region SetUp / TearDown

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var booking = Helper.CreateBooking();
			return new FreightWrapperFromDtbBooking(booking, Factory);
		}

		#endregion

		#region GetNewBusinessObjectToWrap

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return GetBookingBizO();
		}

		#endregion

		#region IsCarrierUsed

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Helper

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		#endregion

		DtbBooking GetEmptyBookingBizO()
		{
			return GetBookingBizO();
		}

		#endregion
	}
}
