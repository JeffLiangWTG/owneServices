using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(BookingInstructionWrapper))]
	class BookingInstructionWrapperTest : InstructionWrapperTest
	{
		#region TestAddress

		[SetOrgAllowMixedCase(true)]
		public void TestAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "INSTORG";
			org.OH_FullName = "Instruction Organisation";

			var bookingInstruction = GetInstructionBizO();
			var bookingInstructionWrapper = GetInstructionWrapper(bookingInstruction, Factory);
			AssertEquals("", bookingInstructionWrapper.Address.CompanyName);

			bookingInstruction.Address.OrganisationPK = org.PK;
			var bookingInstructionWrapper2 = GetInstructionWrapper(bookingInstruction, Factory);
			AssertEquals("Instruction Organisation", bookingInstructionWrapper2.Address.CompanyName);
		}

		#endregion

		#region Properties

		#region Confirmation Properties

		#region TestEstimatedOrSlot

		public void TestEstimatedOrSlot()
		{
			var instruction = GetInstructionBizO();
			var instructionPkgDivot = Helper.CreatePackageDivot(instruction, 2);
			var confirmation = Helper.CreateConfirmation(instructionPkgDivot, ConfirmationTypes.Codes.PickUp);
			confirmation.KK_Estimated = new ZDateTime(2011, 1, 1, 1, 1, 1);
			confirmation.KK_Actual = new ZDateTime(2011, 2, 2, 2, 2, 2);

			var instructionWrapper = GetInstructionWrapper(instruction, instructionPkgDivot, confirmation, Factory);
			AssertEquals("When Confirmation type is Pickup the estimated date should be shown", new ZDateTime(2011, 1, 1, 1, 1, 1), instructionWrapper.EstimatedOrSlot);
		}

		#endregion

		#region TestConfirmationID

		public void TestConfirmationID()
		{
			PkgPackageJob packageJob;
			var instruction = CreateInstructionBizO(InstructionTypes.Codes.PickUp, out packageJob);
			var divot1 = GetPackageDivot(instruction, packageJob, 5, "PLT", 4);
			var divot2 = GetPackageDivot(instruction, packageJob, 10, "CTN", 8);

			var confirmationForSinglePackage = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp, 3);
			var confirmationForAllPackages = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			var singlePackageConfirmationWrapper = GetInstructionWrapper(instruction, divot1, confirmationForSinglePackage, Factory);
			AssertEquals("When confirmation is package specific the Confirmation ID should show quantity from confirmation.", "3x PLT", singlePackageConfirmationWrapper.ConfirmationID);

			var allPackagesConfirmationWrapper = GetInstructionWrapper(instruction, divot1, confirmationForAllPackages, Factory);
			AssertEquals("When confirmation is for all packages the Confirmation ID should show quantity from divot.", "4x PLT", allPackagesConfirmationWrapper.ConfirmationID);
		}

		DtbBookingInstruction CreateInstructionBizO(string instructionType, out PkgPackageJob packageJob)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			packageJob = consolidation.PackageJob;
			return Helper.CreateInstruction(booking, instructionType);
		}

		protected DtbBookingInstructionPkgDivot GetPackageDivot(DtbBookingInstruction instruction, PkgPackageJob packageJob, int packageQty, string packgeType, int packageDivotQty)
		{
			var package = PackingHelper.CreatePackage(packageJob, packageQty, packgeType);
			return Helper.CreatePackageDivot(instruction, package, packageDivotQty);
		}

		#endregion

		#region TestRequiredFromAndRequiredTo

		[TestDate(2013, 07, 16)]
		public void TestRequiredFromAndRequiredTo()
		{
			var instructionWrapperWithoutBizO = GetInstructionWrapper(null, Factory);
			AssertEquals(LabelValuePairWrapper.Empty, instructionWrapperWithoutBizO.RequiredTo);
			AssertEquals(LabelValuePairWrapper.Empty, instructionWrapperWithoutBizO.RequiredFrom);

			var requiredTo = ZDateTime.Now;
			var requiredFrom = requiredTo.AddDays(-1);

			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Customs";
			dateAndReference.AllowRequiredFromDate = true;
			dateAndReference.AllowRequiredFromLabel = (NoResString)"RequiredFromLabel";
			dateAndReference.AllowRequiredToDate = true;
			dateAndReference.AllowRequiredToLabel = (NoResString)"RequiredToLabel";

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var consoldiation = Helper.CreateConsolidation();
			var booking = consoldiation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			confirmation.KK_ConfirmationType = "CUS";
			confirmation.KK_RequiredTo = requiredTo;
			confirmation.KK_RequiredFrom = requiredFrom;

			var instructionWrapperWithBizO = GetInstructionWrapper(instruction, null, confirmation, Factory);
			AssertEquals("RequiredToLabel: " + requiredTo.ToLongTimeString(), instructionWrapperWithBizO.RequiredTo.LabelAndValue);
			AssertEquals("RequiredFromLabel: " + requiredFrom.ToLongTimeString(), instructionWrapperWithBizO.RequiredFrom.LabelAndValue);
		}

		#endregion

		#region TestServiceInstructionNotes

		public void TestServiceInstructionNotes()
		{
			PkgPackageJob packageJob;
			var instruction = CreateInstructionBizO(InstructionTypes.Codes.PickUp, out packageJob);
			instruction.KN_ServiceInstruction = "SERVICE NOTE";
			var org = Helper.CreateOrganisation("ORG");
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling Instruction.");
			instruction.Address.OrganisationPK = org.PK;

			var wrapper = GetInstructionWrapper(instruction, Factory);
			AssertEquals("SERVICE NOTE\r\nHandling Instruction.", wrapper.ServiceInstruction);
		}

		#endregion

		#endregion

		#region TestWrapperMappingFull

		public void TestWrapperMappingFull()
		{
			var booking = GetNewBooking();

			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 5;
			package.KP_F3_NKPackType = Constants.PkgUnit.Box;
			var packageID = "";
			package.KP_PackageID = packageID;
			package.KP_DimensionUQ = "M";
			package.KP_Length = 3;
			package.KP_Width = 4;
			package.KP_Height = 2;
			package.KP_WeightUQ = "KG";
			package.KP_Weight = 1000;
			package.KP_VolumeUQ = "M3";
			package.KP_Volume = 24;

			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "XYZ";
			equipment.RQ_EquipmentType = "DS1";
			equipment.RQ_Description = "Twenty foot cube";

			var picOrganization = Helper.CreateOrganisation("PCORG");
			var bookingInstruction = Helper.CreateInstruction(booking); // KN_Sequence == 1
			bookingInstruction.Address.OrganisationPK = picOrganization.PK;
			bookingInstruction.KN_DropMode = "2D2";
			bookingInstruction.KN_InstructionType = "PIC";
			bookingInstruction.KN_RQ_Equipment = equipment.PK;
			bookingInstruction.KN_ServiceInstruction = "NODE123";
			bookingInstruction.KN_Status = "CLS";

			var instructionPkgDivot = Helper.CreatePackageDivot(bookingInstruction, package, 3);
			booking.PackageJob.Packages.Add(package);

			var bookingConfirmation = Helper.CreateConfirmation(instructionPkgDivot, ConfirmationTypes.Codes.PickUp);
			bookingConfirmation.KK_Quantity = 2;
			bookingConfirmation.KK_RequiredFrom = new ZDateTime(2011, 1, 1, 1, 1, 1);
			bookingConfirmation.KK_RequiredTo = new ZDateTime(2011, 2, 2, 2, 2, 2);
			bookingConfirmation.KK_ReferenceNum = "CONFREF123";
			bookingConfirmation.KK_ConfirmationType = "PIC";

			bookingInstruction.KN_Status = InstructionTypes.Codes.PickUp;
			Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery); // For Consignment
			SetupValuesForTestWrapperMapping(bookingInstruction, bookingConfirmation);

			Factory.Save();

			bookingConfirmation.KK_IsEmptyContainer = true;

			var bookingInstructionWrapper = GetInstructionWrapper(bookingInstruction, instructionPkgDivot, bookingConfirmation, Factory);
			AssertEquals("DropMode", "2D2", bookingInstructionWrapper.DropMode.Code);
			AssertEquals("DropMode description", "2D2", bookingInstructionWrapper.DropMode.Description);
			AssertEquals("DropMode code and description", "2D2", bookingInstructionWrapper.DropMode.CodeAndDescription);
			AssertEquals("InstructionType", "Pickup", bookingInstructionWrapper.InstructionType);
			AssertEquals("ServiceInstruction", "NODE123", bookingInstructionWrapper.ServiceInstruction);
			AssertEquals("Equipment", "Twenty foot cube", bookingInstructionWrapper.Equipment);
			AssertEquals("Sequence", 1, bookingInstructionWrapper.Sequence);
			AssertEquals("Weight", "1000.0 KG", bookingInstructionWrapper.Weight.ValueAndUnitCode);
			AssertEquals("Volume", "24.000 M3", bookingInstructionWrapper.Volume.ValueAndUnitCode);

			AssertEquals("PackageDivotQuantity", 3, bookingInstructionWrapper.PackageDivotQuantity);
			AssertEquals("PackageType", Constants.PkgUnit.Box, bookingInstructionWrapper.PackageType);
			AssertEquals("PackageID", packageID, bookingInstructionWrapper.PackageID);
			AssertEquals("PackageDimensions", "3.0 x 4.0 x 2.0 M", bookingInstructionWrapper.PackageDimensions);
			AssertEquals("PackageDivotSequence", 0, bookingInstructionWrapper.PackageDivotSequence);
			AssertEquals("PackageDivotID", "3x BOX", bookingInstructionWrapper.PackageDivotID.Trim());
			AssertEquals("HasSingleConfirmationForWholePackage", false, bookingInstructionWrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("ConfirmationQuantity", 2, bookingInstructionWrapper.ConfirmationQuantity);
			AssertEquals("ReceivedBy", "Bob", bookingInstructionWrapper.ReceivedBy);
			AssertNotNull("ReceivedBySignature", bookingInstructionWrapper.ReceivedBySignature);
			AssertEquals("RequiredFrom.Label", "Test From", bookingInstructionWrapper.RequiredFrom.Label);
			AssertEquals("RequiredFrom.Value", new ZDateTime(2011, 1, 1, 1, 1, 1).ToLongTimeString(), bookingInstructionWrapper.RequiredFrom.Value);
			AssertEquals("RequiredTo.Lavel", "Test To", bookingInstructionWrapper.RequiredTo.Label);
			AssertEquals("RequiredTo.Value", new ZDateTime(2011, 2, 2, 2, 2, 2).ToLongTimeString(), bookingInstructionWrapper.RequiredTo.Value);
			AssertEquals("ConfirmationReferenceNum", "CONFREF123", bookingInstructionWrapper.ConfirmationReferenceNum);
			AssertEquals("ConfirmationDescription", "PIC (MT)", bookingInstructionWrapper.ConfirmationDescription);
			AssertEquals("ConfirmationID", "2x BOX", bookingInstructionWrapper.ConfirmationID);
			AssertEquals("TimeIn", new ZDateTime(2011, 1, 1, 2, 1, 1), bookingInstructionWrapper.TimeIn);
			AssertEquals("TimeOut", new ZDateTime(2011, 1, 1, 2, 1, 1), bookingInstructionWrapper.TimeOut);

			var container = helper.CreatePackageContainer("Container00001");
			var booking2 = GetNewBooking();
			booking2.PackageJob.Packages.Add(container);

			var picOrganization2 = Helper.CreateOrganisation("PCORG2");
			var bookingInstruction2 = Helper.CreateInstruction(booking2);
			bookingInstruction2.Address.OrganisationPK = picOrganization2.PK;
			bookingInstruction2.KN_DropMode = "2D2";
			bookingInstruction2.KN_InstructionType = "PIC";
			bookingInstruction2.KN_RQ_Equipment = equipment.PK;
			bookingInstruction2.KN_ServiceInstruction = "NODE345";
			bookingInstruction2.KN_Status = "CLS";

			var existingDivot = bookingInstruction2.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().SingleOrDefault(d => d.KD_KP_Package == container.PK);
			var instructionPkgDivot2 = existingDivot ?? Helper.CreatePackageDivot(bookingInstruction2, container, 1);
			var bookingConfirmation2 = Helper.CreateConfirmation(instructionPkgDivot2, ConfirmationTypes.Codes.PickUp);
			bookingConfirmation2.KK_Quantity = 2;
			bookingConfirmation2.KK_RequiredFrom = new ZDateTime(2011, 1, 1, 1, 1, 1);
			bookingConfirmation2.KK_RequiredTo = new ZDateTime(2011, 2, 2, 2, 2, 2);
			bookingConfirmation2.KK_ReferenceNum = "CONFREF345";
			bookingConfirmation2.KK_ConfirmationType = "PIC";

			SetupValuesForTestWrapperMapping(bookingInstruction2, bookingConfirmation2);
			Factory.Save();

			var bookingInstructionWrapper2 = GetInstructionWrapper(bookingInstruction2, instructionPkgDivot2, bookingConfirmation2, Factory);
			AssertEquals("PackageDimensions", "20GP - Twenty foot general purpose", bookingInstructionWrapper2.PackageDimensions);
		}

		void SetupValuesForTestWrapperMapping(DtbBookingInstruction instruction, DtbBookingConfirmation confirmation)
		{
			DateAndReferenceCollection registryItem = TransportRegistry.Instance.DateAndReference.Value;
			var dateAndReference = registryItem.FindConfirmation(confirmation.KK_ConfirmationType, "", instruction.KN_InstructionType, confirmation.OrganisationType, instruction.ContainerMode);
			dateAndReference.AllowRequiredFromLabel = (NoResString)"Test From";
			dateAndReference.AllowRequiredToLabel = (NoResString)"Test To";
			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			confirmation.KK_Actual = new ZDateTime(2011, 1, 1, 2, 1, 1);
			confirmation.KK_ReceivedBy = "Bob";
			confirmation.KK_ReceivedBySignature = new TransportBookingConsignmentTestHelper(Factory).GetSignature();
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = (BookingInstructionWrapper)GetNewDocumentWrapper();
			AssertEquals("DropMode code", CodeAndDescriptionWrapper.Empty.Code, emptyWrapper.DropMode.Code);
			AssertEquals("DropMode description", CodeAndDescriptionWrapper.Empty.Description, emptyWrapper.DropMode.Description);
			AssertEquals("DropMode code and description", CodeAndDescriptionWrapper.Empty.Code, emptyWrapper.DropMode.CodeAndDescription);
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
			AssertEquals("HasSingleConfirmationForWholePackage", false, emptyWrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("Weight", 0m, emptyWrapper.Weight.Value);
			AssertEquals("Weight UQ", "", emptyWrapper.Weight.Unit.Code);
			AssertEquals("Volume", 0m, emptyWrapper.Volume.Value);
			AssertEquals("Volume UQ", "", emptyWrapper.Volume.Unit.Code);
			AssertEquals("UNDGsSummary", "", emptyWrapper.UNDGsSummary);

			AssertEquals("ConfirmationQuantity", 0, emptyWrapper.ConfirmationQuantity);
			AssertEquals("RequiredFrom", LabelValuePairWrapper.Empty, emptyWrapper.RequiredFrom);
			AssertEquals("RequiredTo", LabelValuePairWrapper.Empty, emptyWrapper.RequiredTo);
			AssertEquals("ReceivedBy", "", emptyWrapper.ReceivedBy);
			AssertNull("ReceivedBySignature", emptyWrapper.ReceivedBySignature);
			AssertEquals("ConfirmationReferenceNum", "", emptyWrapper.ConfirmationReferenceNum);
			AssertEquals("ConfirmationDescription", "", emptyWrapper.ConfirmationDescription);
			AssertEquals("ConfirmationID", "", emptyWrapper.ConfirmationID);
			AssertEquals("TimeIn", ZDateTime.Empty, emptyWrapper.TimeIn);
			AssertEquals("TimeOut", ZDateTime.Empty, emptyWrapper.TimeOut);
		}

		#endregion

		#region TestDropModes

		public void TestValidateDropMode()
		{
			var container = Helper.CreatePackageContainer("Container00001");
			var booking = GetNewBooking();
			booking.PackageJob.Packages.Add(container);

			var picOrganization = Helper.CreateOrganisation("PCORG2");
			var bookingInstruction = Helper.CreateInstruction(booking);
			bookingInstruction.Address.OrganisationPK = picOrganization.PK;

			foreach (var wrapper in DefaultDropModes)
			{
				bookingInstruction.KN_DropMode = wrapper.Code;

				var wrapperToTest = GetInstructionWrapper(bookingInstruction, Factory);
				AssertEquals("DropMode code", wrapper.Code, wrapperToTest.DropMode.Code);
				AssertEquals("DropMode description", wrapper.Description, wrapperToTest.DropMode.Description);
				AssertEquals("DropMode code and description", wrapper.CodeAndDescription, wrapperToTest.DropMode.CodeAndDescription);
			}

			bookingInstruction.KN_DropMode = "RAN";
			var randomCodeWrapper = GetInstructionWrapper(bookingInstruction, Factory);
			AssertEquals("Incorrect DropMode - DropMode code", "RAN", randomCodeWrapper.DropMode.Code);
			AssertEquals("Incorrect DropMode - DropMode description", "RAN", randomCodeWrapper.DropMode.Description);
			AssertEquals("Incorrect DropMode - DropMode code and description", "RAN", randomCodeWrapper.DropMode.CodeAndDescription);

			bookingInstruction.KN_DropMode = "";
			var blankCodeWrapper = GetInstructionWrapper(bookingInstruction, Factory);
			AssertEquals("Incorrect DropMode - DropMode code", CodeAndDescriptionWrapper.Empty.Code, blankCodeWrapper.DropMode.Code);
			AssertEquals("Incorrect DropMode - DropMode description", CodeAndDescriptionWrapper.Empty.Description, blankCodeWrapper.DropMode.Description);
			AssertEquals("Incorrect DropMode - DropMode code and description", CodeAndDescriptionWrapper.Empty.Code, blankCodeWrapper.DropMode.CodeAndDescription);
		}

		#endregion

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

		#region GetInstructionBizO

		DtbBookingInstruction GetInstructionBizO()
		{
			return Helper.CreateInstruction();
		}

		#endregion

		#region GetNewBooking

		DtbBooking GetNewBooking()
		{
			return Helper.CreateBooking();
		}

		#endregion

		#region GetInstructionWrapper

		BookingInstructionWrapper GetInstructionWrapper(DtbBookingInstruction instruction, BusinessObjectFactory factory)
		{
			return new BookingInstructionWrapper(instruction, Factory);
		}

		BookingInstructionWrapper GetInstructionWrapper(DtbBookingInstruction instruction, DtbBookingInstructionPkgDivot divot, DtbBookingConfirmation confirmation, BusinessObjectFactory factory)
		{
			return new BookingInstructionWrapper(instruction, divot, confirmation, Factory);
		}

		#endregion

		#region GetSetupWrapperForDefaultFormatting

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var booking = GetNewBooking();
			var instruction = GetInstructionBizO();
			booking.Instructions.Add(instruction);
			return GetInstructionWrapper(instruction, Factory);
		}

		#endregion

		#region GetNewDocumentWrapper

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return GetInstructionWrapper(null, Factory);
		}

		#endregion

		#region Helper

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		#endregion

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}
}
