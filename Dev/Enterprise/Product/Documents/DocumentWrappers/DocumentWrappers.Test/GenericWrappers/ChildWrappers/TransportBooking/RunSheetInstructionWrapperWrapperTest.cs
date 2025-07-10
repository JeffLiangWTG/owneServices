using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RunSheetInstructionWrapper))]
	sealed class RunSheetInstructionWrapperWrapperTest : InstructionWrapperTest
	{
		#region TestReceivedBy

		public void TestReceivedBy()
		{
			var instruction = Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction;
			var runsheetInstruction = Helper.CreateRunSheetInstruction(instruction.Confirmations[0]);
			AssertEquals("", new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBy);

			runsheetInstruction.K1_ReceivedBy = "OnRunsheet";
			AssertEquals("OnRunsheet", new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBy);

			instruction.Confirmations[0].KK_ReceivedBy = "OnConfirmation";
			AssertEquals("OnConfirmation", new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBy);
		}

		#endregion

		#region TestReceivedBySignature

		public void TestReceivedBySignature()
		{
			var instruction = Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction;
			var runsheetInstruction = Helper.CreateRunSheetInstruction(instruction.Confirmations[0]);
			AssertNull("Precondition", new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBySignature);

			runsheetInstruction.K1_ReceivedBySignature = Helper.GetSignature();
			AssertNotNull(new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBySignature);

			runsheetInstruction.K1_ReceivedBySignature = ZBlob.Empty;
			AssertNull("Precondition", new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBySignature);

			instruction.Confirmations[0].KK_ReceivedBySignature = Helper.GetSignature();
			AssertNotNull(new RunSheetInstructionWrapper(runsheetInstruction, instruction.Confirmations[0], Factory).ReceivedBySignature);
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.Instructions[0].Confirmations[0];
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			var emptyWrapper = new RunSheetInstructionWrapper(instruction, confirmation, Factory);
			AssertEquals("DropMode code", CodeAndDescriptionWrapper.Empty.Code, emptyWrapper.DropMode.Code);
			AssertEquals("DropMode description", CodeAndDescriptionWrapper.Empty.Description, emptyWrapper.DropMode.Description);
			AssertEquals("DropMode code and description", CodeAndDescriptionWrapper.Empty.CodeAndDescription, emptyWrapper.DropMode.CodeAndDescription);

			AssertEquals("InstructionType", "", emptyWrapper.InstructionType);
			AssertEquals("Status", "AVL", emptyWrapper.Status);
			AssertEquals("ServiceInstruction", "", emptyWrapper.ServiceInstruction);
			AssertEquals("Equipment", "", emptyWrapper.Equipment);
			AssertEquals("Sequence", 0, emptyWrapper.Sequence);

			AssertEquals("PackageDivotQuantity", 0, emptyWrapper.PackageDivotQuantity);
			AssertEquals("PackageType", "", emptyWrapper.PackageType);
			AssertEquals("PackageID", "", emptyWrapper.PackageID);
			AssertEquals("PackageDivotSequence", 0, emptyWrapper.PackageDivotSequence);
			AssertEquals("PackageDivotID", "", emptyWrapper.PackageDivotID);
			AssertEquals("HasSingleConfirmationForWholePackage", true, emptyWrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("Weight", 0m, emptyWrapper.Weight.Value);
			AssertEquals("Weight UQ", "KG", emptyWrapper.Weight.Unit.Code);
			AssertEquals("Volume", 0m, emptyWrapper.Volume.Value);
			AssertEquals("Volume UQ", "M3", emptyWrapper.Volume.Unit.Code);
			AssertEquals("UNDGsSummary", "", emptyWrapper.UNDGsSummary);
			AssertEquals("ConsignorOrConsigneeAddress", "", emptyWrapper.ConsignorOrConsigneeAddress);
		}

		#endregion

		#region TestConsignorOrConsigneeAddress

		public void TestConsignorOrConsigneeAddress_Consignor()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignor = CreateOrg("consignor", "Honda Motorcycles", "Melbourne", "VIC");
			var consignment = helper.CreateConsignment("LTC001");
			var address = helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, consignor.MainAddress);
			var action = address.Actions[0];

			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			var wrapper = new RunSheetInstructionWrapper(instruction, action, Factory);
			AssertEquals("ConsignorOrConsigneeAddress", "Honda Motorcycles - Melbourne VIC", wrapper.ConsignorOrConsigneeAddress);
		}

		public void TestConsignorOrConsigneeAddress_Consignee()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignee = CreateOrg("consignee", "Wisetech", "Alexandria", "NSW");
			var consignment = helper.CreateConsignment("LTC001");
			var address = helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, consignee.MainAddress);
			var action = address.Actions[0];

			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			var wrapper = new RunSheetInstructionWrapper(instruction, action, Factory);
			AssertEquals("ConsignorOrConsigneeAddress", "Wisetech - Alexandria NSW", wrapper.ConsignorOrConsigneeAddress);
		}

		OrgHeader CreateOrg(ZString code, ZString fullName, ZString city, ZString state)
		{
			var org = Helper.CreateOrganisation(code);
			org.OH_FullName = fullName;

			var address = org.MainAddress;
			address.OA_City = city;
			address.OA_State = state;

			return org;
		}

		#endregion

		#region ExpectedDefaultFormatting

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Address : 
DropMode : 
Registry : (No Default Field Value Available on Registry)
RequiredFrom : 
RequiredTo : 
Transport : 
Volume : 
Weight :
";
			}
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var confirmation = consignment.Instructions[0].Confirmations[0];
			var instruction = Factory.New<DtbConsignmentRunSheetInstruction>();
			return new RunSheetInstructionWrapper(instruction, confirmation, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RunSheetInstructionWrapper(null, null, Factory);
		}

		#endregion
	}
}
