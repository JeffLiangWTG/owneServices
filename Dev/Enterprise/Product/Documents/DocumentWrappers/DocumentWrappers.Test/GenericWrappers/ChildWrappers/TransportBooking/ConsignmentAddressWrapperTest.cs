using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ConsignmentAddressWrapper))]
	sealed class ConsignmentAddressWrapperTest : InstructionWrapperTest
	{
		#region TestReceivedBy_ReceivedBySignature

		public void TestReceivedBy_ReceivedBySignature()
		{
			var consignmentHelper = new TransportConsignmentTestHelper(Factory);
			var consignor = Helper.CreateOrganisation("consignor");
			var consignment = consignmentHelper.CreateConsignment("LTC001");
			var address = consignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, consignor.MainAddress);
			var action = address.Actions[0];
			action.LTA_SignedBy = "MAX";
			action.LTA_SignedBySignature = Helper.GetSignature();

			var wrapper = new ConsignmentAddressWrapper(address, action, Factory);
			AssertEquals("ReceivedBy", "MAX", wrapper.ReceivedBy);
			AssertNotNull("ReceivedBySignature", wrapper.ReceivedBySignature);
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment("LTC001");
			var address = helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			var action = address.Actions[0];
			var emptyWrapper = new ConsignmentAddressWrapper(address, action, Factory);

			AssertEquals("DropMode code", CodeAndDescriptionWrapper.Empty.Code, emptyWrapper.DropMode.Code);
			AssertEquals("DropMode description", CodeAndDescriptionWrapper.Empty.Description, emptyWrapper.DropMode.Description);
			AssertEquals("DropMode code and description", CodeAndDescriptionWrapper.Empty.CodeAndDescription, emptyWrapper.DropMode.CodeAndDescription);
			AssertEquals("InstructionType", "", emptyWrapper.InstructionType);
			AssertEquals("Status", "", emptyWrapper.Status);
			AssertEquals("ServiceInstruction", "", emptyWrapper.ServiceInstruction);
			AssertEquals("Equipment", "", emptyWrapper.Equipment);
			AssertEquals("Sequence", 0, emptyWrapper.Sequence);

			AssertEquals("PackageDivotQuantity", 0, emptyWrapper.PackageDivotQuantity);
			AssertEquals("PackageType", "", emptyWrapper.PackageType);
			AssertEquals("PackageID", "", emptyWrapper.PackageID);
			AssertEquals("PackageDivotSequence", 0, emptyWrapper.PackageDivotSequence);
			AssertEquals("PackageDivotID", "", emptyWrapper.PackageDivotID);
			AssertEquals("HasSingleConfirmationForWholePackage", false, emptyWrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("Weight", 0m, emptyWrapper.Weight.Value);
			AssertEquals("Weight UQ", "", emptyWrapper.Weight.Unit.Code);
			AssertEquals("Volume", 0m, emptyWrapper.Volume.Value);
			AssertEquals("Volume UQ", "", emptyWrapper.Volume.Unit.Code);
			AssertEquals("UNDGsSummary", "", emptyWrapper.UNDGsSummary);
			AssertEquals("ConsignorOrConsigneeAddress", "", emptyWrapper.ConsignorOrConsigneeAddress);
			AssertEquals("ReceivedBy", "", emptyWrapper.ReceivedBy);
			AssertEquals("ReceivedBySignature", null, emptyWrapper.ReceivedBySignature);
		}

		#endregion

		#region TestWrapperMappingFull

		public void TestWrapperMappingFull()
		{
			var consignmentHelper = new TransportConsignmentTestHelper(Factory);
			var consignor = Helper.CreateOrganisation("consignor");
			var consignment = consignmentHelper.CreateConsignment("LTC001");
			var address = consignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, consignor.MainAddress);
			var action = address.Actions[0];
			action.LTA_SignedBy = "MAX";
			action.LTA_SignedBySignature = Helper.GetSignature();

			var wrapper = new ConsignmentAddressWrapper(address, action, Factory);
			AssertEquals("DropMode code", CodeAndDescriptionWrapper.Empty.Code, wrapper.DropMode.Code);
			AssertEquals("DropMode description", CodeAndDescriptionWrapper.Empty.Description, wrapper.DropMode.Description);
			AssertEquals("DropMode code and description", CodeAndDescriptionWrapper.Empty.CodeAndDescription, wrapper.DropMode.CodeAndDescription);
			AssertEquals("InstructionType", "", wrapper.InstructionType);
			AssertEquals("Status", "", wrapper.Status);
			AssertEquals("ServiceInstruction", "", wrapper.ServiceInstruction);
			AssertEquals("Equipment", "", wrapper.Equipment);
			AssertEquals("Sequence", 0, wrapper.Sequence);

			AssertEquals("PackageDivotQuantity", 0, wrapper.PackageDivotQuantity);
			AssertEquals("PackageType", "", wrapper.PackageType);
			AssertEquals("PackageID", "", wrapper.PackageID);
			AssertEquals("PackageDivotSequence", 0, wrapper.PackageDivotSequence);
			AssertEquals("PackageDivotID", "", wrapper.PackageDivotID);
			AssertEquals("HasSingleConfirmationForWholePackage", false, wrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("Weight", 0m, wrapper.Weight.Value);
			AssertEquals("Weight UQ", "", wrapper.Weight.Unit.Code);
			AssertEquals("Volume", 0m, wrapper.Volume.Value);
			AssertEquals("Volume UQ", "", wrapper.Volume.Unit.Code);
			AssertEquals("UNDGsSummary", "", wrapper.UNDGsSummary);
			AssertEquals("ConsignorOrConsigneeAddress", "", wrapper.ConsignorOrConsigneeAddress);
			AssertEquals("ReceivedBy", "MAX", wrapper.ReceivedBy);
			AssertNotNull("ReceivedBySignature", wrapper.ReceivedBySignature);
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
Transport :  is null
Volume : 
Weight :
";
			}
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var address = helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			var action = address.Actions[0];
			return new ConsignmentAddressWrapper(address, action, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment("LTC001");
			var address = helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			var action = address.Actions[0];
			return new ConsignmentAddressWrapper(address, action, Factory);
		}

		#endregion
	}
}
