using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LinehaulManifestInstructionWrapper))]
	sealed class LinehaulManifestInstructionWrapperTest : InstructionWrapperTest
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var manifest = Factory.New<DtbLinehaulManifest>();
			var package = Helper.CreatePackage(ZString.Empty, 0, ZString.Empty);
			manifest.Packages.Add(package);
			var instruction = new LinehaulManifestDeliveryInstructionStrategy(manifest, Factory);
			var emptyWrapper = new LinehaulManifestInstructionWrapper(package, instruction, Factory);
			AssertEquals("DropMode code", CodeAndDescriptionWrapper.Empty.Code, emptyWrapper.DropMode.Code);
			AssertEquals("DropMode description", CodeAndDescriptionWrapper.Empty.Description, emptyWrapper.DropMode.Description);
			AssertEquals("DropMode code and description", CodeAndDescriptionWrapper.Empty.CodeAndDescription, emptyWrapper.DropMode.CodeAndDescription);

			AssertEquals("InstructionType", InstructionTypes.Descriptions.Delivery, emptyWrapper.InstructionType);
			AssertEquals("Status", ZString.Empty, emptyWrapper.Status);
			AssertEquals("ServiceInstruction", ZString.Empty, emptyWrapper.ServiceInstruction);
			AssertEquals("Equipment", ZString.Empty, emptyWrapper.Equipment);

			AssertEquals("PackageID", ZString.Empty, emptyWrapper.PackageID);
			AssertEquals("PackageDivotSequence", 0, emptyWrapper.PackageDivotSequence);
			AssertEquals("PackageDivotID", ZString.Empty, emptyWrapper.PackageDivotID);
			AssertEquals("HasSingleConfirmationForWholePackage", true, emptyWrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("Weight", 0m, emptyWrapper.Weight.Value);
			AssertEquals("Weight UQ", "KG", emptyWrapper.Weight.Unit.Code);
			AssertEquals("Volume", 0m, emptyWrapper.Volume.Value);
			AssertEquals("Volume UQ", "M3", emptyWrapper.Volume.Unit.Code);
			AssertEquals("UNDGsSummary", ZString.Empty, emptyWrapper.UNDGsSummary);
			AssertEquals("ConsignorOrConsigneeAddress", ZString.Empty, emptyWrapper.ConsignorOrConsigneeAddress);
		}

		#endregion

		#region TestWrapperMappingFull

		public void TestWrapperMappingFull()
		{
			var pickupDepot = Factory.New<OrgHeader>();
			pickupDepot.OH_Code = "DEP1";
			var deliveryDepot = Factory.New<OrgHeader>();
			deliveryDepot.OH_Code = "DEP2";
			var manifest = Factory.New<DtbLinehaulManifest>();
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = Helper.CreatePackage("PACKAGE123", 1, "BOX");
			package.KP_Weight = 1.2;
			package.KP_WeightUQ = "KG";
			package.KP_Volume = 3.4;
			package.KP_VolumeUQ = "M3";
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			var undgSubs = Factory.New<UNDGSubstance>();
			undgSubs.DG_UNNO = "9911";
			undgSubs.DG_Class = "1";
			undgSubs.DG_PSN = "SHIP NAME 1";
			undgSubs.DG_PG = "I";
			undgSubs.DG_MP = "X";

			var undg = package.UNDGs.AddNew();
			undg.DI_DG = undgSubs.PK;

			manifest.Packages.Add(package);
			manifest.LHM_OA_OriginDepot = pickupDepot.MainAddress.PK;
			manifest.LHM_OA_DestinationDepot = deliveryDepot.MainAddress.PK;

			var instruction = new LinehaulManifestPickupInstructionStrategy(manifest, Factory);

			Factory.Save();

			var bookingInstructionWrapper = new LinehaulManifestInstructionWrapper(package, instruction, Factory);
			AssertEquals("InstructionType", "Pickup", bookingInstructionWrapper.InstructionType);
			AssertEquals("Sequence", 1, bookingInstructionWrapper.Sequence);
			AssertEquals("PackageDivotQuantity", 1, bookingInstructionWrapper.PackageDivotQuantity);
			AssertEquals("PackageType", Core.Constants.PkgUnit.Box, bookingInstructionWrapper.PackageType);
			AssertEquals("PackageID", "PACKAGE123", bookingInstructionWrapper.PackageID);
			AssertEquals("PackageDimensions", "0.0 x 0.0 x 0.0 M", bookingInstructionWrapper.PackageDimensions);
			AssertEquals("PackageDivotSequence", 0, bookingInstructionWrapper.PackageDivotSequence);
			AssertEquals("PackageDivotID", "BOX", bookingInstructionWrapper.PackageDivotID);
			AssertEquals("HasSingleConfirmationForWholePackage", true, bookingInstructionWrapper.HasSingleConfirmationForWholePackage);

			AssertEquals("Weight", 1.2m, bookingInstructionWrapper.Weight.Value);
			AssertEquals("Weight UQ", "KG", bookingInstructionWrapper.Weight.Unit.Code);
			AssertEquals("Volume", 3.4m, bookingInstructionWrapper.Volume.Value);
			AssertEquals("Volume UQ", "M3", bookingInstructionWrapper.Volume.Unit.Code);
			AssertEquals("UNDGsSummary", "UN9911, SHIP NAME 1, class 1, PG I, MARINE POLLUTANT, in 1x BOX PACKAGE123", bookingInstructionWrapper.UNDGsSummary);
		}

		#endregion

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
Volume : 10.000 M3
Weight : 10.000 KG
";
			}
		}

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
			var package = Helper.CreatePackage(consignment, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment.PickupInstruction, package, 1);
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.Packages.Add(package);
			var instruction = new LinehaulManifestPickupInstructionStrategy(manifest, Factory);
			return new LinehaulManifestInstructionWrapper(package, instruction, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var package = Helper.CreatePackage(consignment, 10m, 10m);
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.Packages.Add(package);
			var instruction = new LinehaulManifestPickupInstructionStrategy(manifest, Factory);
			return new LinehaulManifestInstructionWrapper(package, instruction, Factory);
		}

		#endregion
	}
}
