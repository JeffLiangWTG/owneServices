using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	abstract class FreightWrapperFromDtbTransportTest<TTransport, TFreightWrapperFromDtbTransport> : FreightWrapperTest
			where TTransport : DtbTransport
			where TFreightWrapperFromDtbTransport : FreightWrapperFromDtbTransport<TTransport>
	{
		#region TestGetTransportAddresses

		public void TestGetTransportAddresses()
		{
			var emptyTransport = GetEmptyTransportBizO();
			var emptyTransportWrapper = GetFreightWrapper(emptyTransport);
			AssertEquals(0, emptyTransportWrapper.TransportAddresses.Count);

			var transport = GetEmptyTransportBizO();
			var org = Helper.CreateOrganisation("TEST");
			var address_pickup = Helper.AddAddressToOrganisation(org, "1 Pickup Road", OrgAddressType.PickupAndDelivery);
			var address_delivery = Helper.AddAddressToOrganisation(org, "2 Delivery Road", OrgAddressType.PickupAndDelivery);
			var pickUpInstruction = Helper.CreateInstruction(transport, "", LocalCartageJobOrgTypeList.Codes.CNR, address_pickup);
			pickUpInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp; // we want to set Instruction Type after, to prevent creation of Depot Instruction in Consignments

			Helper.CreateInstruction(transport, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CFS, address_delivery);
			Helper.CreateInstruction(transport, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CFS, address_delivery); // should skip
			var deliveryInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null); // not the last, so add
			using (deliveryInstruction.SuspendOnDocAddressChanged())
			{
				deliveryInstruction.Address.E2_OA_Address = address_pickup.PK; // need to prevent OnDocAddress firing as it will null out the Depot Instruction's Address
			}

			var transportWrapper = GetFreightWrapper(transport);
			var wrappedJobDocAddresses = transportWrapper.TransportAddresses.Cast<AddressWrapper>().Select(a => a.WrappedObject);
			var wrappedOrgAddresses = wrappedJobDocAddresses.Cast<JobDocAddress>().Select(a => a.Address);
			AssertContainsExactElementsInAnyOrder(new[] { address_pickup, address_delivery, address_pickup }, wrappedOrgAddresses);
		}

		#endregion

		#region TestGetJobNumberAndHeading

		public void TestGetJobNumberAndHeading()
		{
			var transport = GetTransportBizO();
			transport.KM_JobID = "ABCD";
			var transportWrapper = GetFreightWrapper(transport);

			AssertEquals("ABCD", transportWrapper.JobNumber);
			AssertEquals(JobNumberHeadingToTest, transportWrapper.JobNumberHeading);
		}

		#region JobNumberHeadingToTest

		protected virtual string JobNumberHeadingToTest
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

			var transport = GetTransportBizO();
			DtbTransportInstruction instruction1 = Helper.CreateInstruction(transport);
			DtbTransportInstruction instruction2 = Helper.CreateInstruction(transport);
			DtbTransportInstruction instruction3 = Helper.CreateInstruction(transport);
			DtbTransportInstructionPkgDivot instruction1_PkgDivot1 = Helper.CreatePackageDivot(instruction1, container1_Instruction1, 1);
			DtbTransportInstructionPkgDivot instruction1_PkgDivot2 = Helper.CreatePackageDivot(instruction1, container2_Instruction1, 1);
			DtbTransportInstructionPkgDivot instruction2_PkgDivot1 = Helper.CreatePackageDivot(instruction2, container3_Instruction2, 1);

			var bookingWrapper = GetFreightWrapper(transport);
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
			var transport = GetTransportBizO();
			var additionalRefNumber = Factory.New<CusEntryNumber>();
			additionalRefNumber.CE_EntryType = "TRF";
			additionalRefNumber.CE_EntryNum = "123";
			additionalRefNumber.CE_ParentID = transport.PK;
			transport.AdditionalReferenceNumbers.Add(additionalRefNumber);

			var transportWrapper = GetFreightWrapper(transport);
			AssertEquals("transportWrapper.CustomsEntries.Count", 1, transportWrapper.CustomsEntries.Count);
			AssertEquals("transportWrapper.CustomsEntries[0].EntryNumber", "123", transportWrapper.CustomsEntries[0].EntryNumber);
			var exceptedCode = (transportWrapper is FreightWrapperFromDtbBooking) ? "TRF" : "Transport Reference Number";

			AssertEquals("transportWrapper.CustomsEntries[0].EntryNumber", exceptedCode, transportWrapper.CustomsEntries[0].EntryType.Code);
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

			var tranport = GetTransportBizO();
			DtbTransportInstruction instruction1 = Helper.CreateInstruction(tranport);
			DtbTransportInstruction instruction2 = Helper.CreateInstruction(tranport);
			DtbTransportInstruction instruction3 = Helper.CreateInstruction(tranport);
			DtbTransportInstructionPkgDivot instruction1_PkgDivot1 = Helper.CreatePackageDivot(instruction1, package1_Instruction1, 1);
			DtbTransportInstructionPkgDivot instruction1_PkgDivot2 = Helper.CreatePackageDivot(instruction1, package2_Instruction1, 1);
			DtbTransportInstructionPkgDivot instruction2_PkgDivot1 = Helper.CreatePackageDivot(instruction2, package3_Instruction2, 1);

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

			var transport = GetTransportBizO();
			DtbTransportInstruction instruction1 = Helper.CreateInstruction(transport);
			DtbTransportInstruction instruction2 = Helper.CreateInstruction(transport);
			DtbTransportInstruction instruction3 = Helper.CreateInstruction(transport);
			Helper.CreatePackageDivot(instruction1, container_Instruction1, 1);
			Helper.CreatePackageDivot(instruction1, package_Instruction1, 1);
			Helper.CreatePackageDivot(instruction2, package_Instruction2, 1);
			helper.CreatePackageDivot(instruction2, package_Instruction2_NoUNDG, 1);

			var bookingWrapper = GetFreightWrapper(transport);
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
			var transport = GetTransportBizO();
			var wrapper = GetFreightWrapper(transport);

			AssertEquals("Non packages, Weight should be 0m.", 0m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("Non packages, Volume should be 0m.", 0m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
		}

		public void TestQtyWeightVolume_DifferentUnits()
		{
			var transport = GetTransportBizO();
			var pic1 = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var pic2 = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var mlt = Helper.CreateInstruction(transport, InstructionTypes.Codes.Multi);
			var dlv1 = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			var dlv2 = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);

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

			var wrapper = GetFreightWrapper(transport);
			AssertEquals("pA(5+6) + pB(3+14) + pC(0)", 28m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("PKG", wrapper.ShipmentOuterPacksQty.Unit.Code);
			AssertEquals("110 + 77.1 + 1500", 1687.1m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("110 + 4.814 + 24", 138.814m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
		}

		public void TestQtyWeightVolume_SameUnit()
		{
			var transport = GetTransportBizO();
			var pic1 = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var pic2 = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			var mlt = Helper.CreateInstruction(transport, InstructionTypes.Codes.Multi);
			var dlv1 = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			var dlv2 = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);

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

			var wrapper = GetFreightWrapper(transport);
			AssertEquals("pA(5+6) + pB(3+14) + pC(0)", 28m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("PKG", wrapper.ShipmentOuterPacksQty.Unit.Code);
			AssertEquals("110 + 170 + 1.5", 281.5m, wrapper.Weight.Value);
			AssertEquals("LB", Constants.Weight.Pounds, wrapper.Weight.Unit.Code);
			AssertEquals("110 + 170 + 24", 304m, wrapper.Volume.Value);
			AssertEquals("CF", Constants.Volume.CubicFeet, wrapper.Volume.Unit.Code);
		}

		#endregion

		#region TestNotes

		public override void TestWrapperNotes()
		{
			var transport = GetTransportBizO();
			var note = transport.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteDataAsText = "transport note.";

			var wrapper = GetFreightWrapper(transport);
			AssertEquals(1, wrapper.Notes.Count);
			AssertEquals("transport note.", wrapper.Notes[PredefinedNoteTypes.Instance.HandlingInstructions.Description].Text);
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
			var transport = GetTransportBizO();
			var wrapper = GetFreightWrapper(transport);

			((IDocTypeCode)wrapper).DocTypeCode = "CAD";
			AssertEquals("CAD", ((IDocTypeCode)wrapper).DocTypeCode);

			((IDocTypeCode)wrapper).DocTypeCode = "CAR";
			AssertEquals("CAR", ((IDocTypeCode)wrapper).DocTypeCode);
		}

		#endregion

		#region Implmentation

		#region GetNewBusinessObjectToWrap

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return GetTransportBizO();
		}

		#endregion

		#region GetSetupWrapperForDefaultFormatting

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return GetFreightWrapper(null);
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

		TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = new TransportCommonTestHelper(Factory)); }
		}

		TransportCommonTestHelper helper;

		#endregion

		protected virtual TTransport GetEmptyTransportBizO()
		{
			return GetTransportBizO();
		}

		protected abstract TTransport GetTransportBizO();
		protected abstract TFreightWrapperFromDtbTransport GetFreightWrapper(TTransport transport);

		#endregion
	}
}
