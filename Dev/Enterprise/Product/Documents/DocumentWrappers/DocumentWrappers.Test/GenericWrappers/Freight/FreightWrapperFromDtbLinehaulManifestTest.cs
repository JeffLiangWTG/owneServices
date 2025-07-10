using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDtbLinehaulManifest))]
	sealed class FreightWrapperFromDtbLinehaulManifestTest : FreightWrapperTest
	{
		#region TestConsignorAndConsigneeAddresses

		public void TestConsignorAndConsigneeAddresses()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var package = Helper.CreatePackage(consignment, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment.PickupInstruction, package, 1);

			var pickupCity = "Sydney";
			var pickupAddress = SetupOrgHeaderAndAddress(pickupCity, "DEP1");

			var deliveryCity = "Melbourne";
			var deliveryAddress = SetupOrgHeaderAndAddress(deliveryCity, "DEP2");

			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.Packages.Add(package);

			manifest.LHM_OA_OriginDepot = pickupAddress.PK;
			manifest.LHM_OA_DestinationDepot = deliveryAddress.PK;

			var wrapper = new FreightWrapperFromDtbLinehaulManifest(manifest, Factory);
			AssertEquals(pickupCity.ToUpper(), wrapper.Consignor.Addresses[0].City);
			AssertEquals(deliveryCity.ToUpper(), wrapper.Consignee.Addresses[0].City);
		}

		OrgAddress SetupOrgHeaderAndAddress(string cityName, string code)
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_City = cityName;
			var depotOrg = Factory.New<OrgHeader>();
			depotOrg.OH_Code = code;
			orgAddress.OA_OH = depotOrg.PK;
			return orgAddress;
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "RS12345" },
					{ "JobNumberBarcodeText", "^DLM=RS12345;;|" },
					{ "JobNumberBarcodeTextForFont", "È^DLM=RS12345;;|¿Ê" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈRS123454Ê" },
					{ "JobNumberHeading", "Linehaul Manifest" }
				};
			}
		}

		protected override bool IsCarrierUsed
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<DtbLinehaulManifest>();
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Consignee : ERITREA
Consignor : ERITREA
RunSheet : (No Default Field Value Available on RunSheetFromLinehaulManifest)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromDtbLinehaulManifest(GetLinehaulManifest(), Factory);
		}

		DtbLinehaulManifest GetLinehaulManifest()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment.PickupInstruction, package1, 1);
			var package2 = Helper.CreatePackage(consignment, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment.DeliveryInstruction, package2, 1);

			var pickupDepot = Factory.New<OrgHeader>();
			pickupDepot.OH_Code = "DEP1";
			var deliveryDepot = Factory.New<OrgHeader>();
			deliveryDepot.OH_Code = "DEP2";
			var manifest = Factory.New<DtbLinehaulManifest>();

			manifest.Packages.Add(package1);
			manifest.Packages.Add(package2);

			manifest.LHM_OA_OriginDepot = pickupDepot.MainAddress.PK;
			manifest.LHM_OA_DestinationDepot = deliveryDepot.MainAddress.PK;
			var truck = Helper.CreateVehicle("VHCL");
			manifest.LHM_ManifestID = "RS12345";
			//manifest.LHM_RQ_PrimaryEquipment = truck.PK;
			manifest.LHM_GS_NKDriver1 = Helper.CreateDriver("Bob", "Bob").GS_Code;

			return manifest;
		}

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
