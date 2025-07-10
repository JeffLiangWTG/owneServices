using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WarehouseDocketWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestWrapperMappingsEmpty

		protected override void TestWrapperMappingsEmpty_TransportCoAddress(WarehouseJobGenericWrapper emptyWrapper)
		{
			AssertEquals("TransportCoAddress", AddressWrapper.Empty(Factory).Address, emptyWrapper.TransportCoAddress.Address);
		}

		#endregion

		#region  TestPalletizedInventory

		protected override void TestPalletizedInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			WhsDocket docket;
			WhsInventoryView inventory;
			TestPalletizedInventory_Setup(data, out docket, out inventory);
			TestPalletizedInventory_ForSelectedInventoryCore(docket, inventory);

			docket.InventoryToPrintPalletLabelFor = null;
			var wrapperWithNoInventorySelected = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("When no inventory selected for printing, no wrappers for palletised inventory should be created.", 0, wrapperWithNoInventorySelected.PalletizedInventory.Count);
		}

		protected abstract void TestPalletizedInventory_Setup(TestDataSimpleEnvironment data, out WhsDocket docket, out WhsInventoryView inventory);

		protected void TestPalletizedInventory_ForSelectedInventoryCore(WhsDocket docket, WhsInventoryView inventory)
		{
			docket.InventoryToPrintPalletLabelFor = inventory;
			AssertEquals("Precondition - PalletID should not be empty.", false, inventory.WI_PalletID.IsEmpty);
			AssertEquals("Precondition - TotalUnits should not be 0.", true, inventory.InDocketLine.WE_StockOnHand != 0m);

			var oldPalletID = inventory.InDocketLine.WE_PalletID;
			inventory.InDocketLine.WE_PalletID = "";

			var docketWrapper_WithoutPalletID = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("When selected Inventory has no Pallet ID, then no label should be printed for it.", 0, docketWrapper_WithoutPalletID.PalletizedInventory.Count);
			inventory.InDocketLine.WE_PalletID = oldPalletID; // restore actual palletID

			var docketWrapper_WithPalletID = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("When selected Inventory has a Pallet ID, then a label should be printed for it.", 1, docketWrapper_WithPalletID.PalletizedInventory.Count);

			inventory.InDocketLine.WE_StockOnHand = 0m;

			var docketWrapper_WithZeroTotalUnits = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("When selected Inventory has a Pallet ID, but no Existing stock, then no label should be printed for it.", 0, docketWrapper_WithZeroTotalUnits.PalletizedInventory.Count);
		}

		#endregion

		#region TestArrivalDate

		protected override void TestArrivalDateCore()
		{
			var now = ZDateTime.Today;
			var docket = GetNewDocket();
			docket.WD_ArrivalDate = ZDateTimeOffset.Today;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(now, docketWrapper.ArrivalDate);
		}

		#endregion

		#region TestBookingDate

		protected override void TestBookingDateCore()
		{
			var now = ZDateTime.Today;
			var docket = GetNewDocket();
			docket.WD_BookingDate = ZDateTimeOffset.Today;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(now, docketWrapper.BookingDate);
		}

		#endregion

		#region TestContainers

		protected override void TestContainersCore()
		{
			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";

			var docket = GetNewDocket();
			var container1 = docket.Containers.AddNew();
			container1.WC_ContainerNum = "CONTAINER1";
			container1.WC_RC = ref1.PK;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(1, docketWrapper.Containers.Count);
		}
		#endregion

		#region TestJobNumber

		protected override void TestJobNumberCore()
		{
			var docket = GetNewDocket();
			docket.WD_DocketID = "Docket12345";

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("Docket12345", docketWrapper.JobNumber);
		}

		#endregion

		#region TestJobNumberHeading

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("Job Number", WarehouseDocketWrapper.JobNumberHeading);
		}

		#endregion

		#region TestPrimaryBarcodeText

		protected override void TestPrimaryBarcodeTextCore()
		{
			var docket = GetNewDocket();
			var barcode = new TextBarcode("TextForBarcode");
			docket.WD_ExternalReference = barcode.TextToEncode;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(barcode.TextAs128sFontString, docketWrapper.PrimaryBarcodeText);
		}

		#endregion

		#region TestCustomerReferenceBarcode

		protected override void TestCustomerReferenceBarcodeCore()
		{
			var docket = GetNewDocket();
			var barcode = new TextBarcode("TextForBarcode");
			docket.WD_CustomerReference = barcode.TextToEncode;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(barcode.TextAs128sFontString, docketWrapper.CustomerReferenceBarcode);
		}

		#endregion

		#region TestWarehouse

		protected override void TestWarehouseCore()
		{
			var whs = Helper.CreateWarehouse("Warehouse ABC");
			var docket = GetNewDocket();
			docket.WD_WW_Whs = whs.PK;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("Warehouse ABC", docketWrapper.Warehouse.Name);
		}

		#endregion

		#region TestTransportCompany

		protected override void TestTransportCompanyCore()
		{
			var docket = GetNewDocket();
			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				var transportCo = Factory.New<OrgHeader>();
				transportCo.OH_Code = "Carrier123";
				job.TransportCoPK = transportCo.PK;
				AssertEquals("Carrier123", wrapper.TransportCompany.CompanyCode);
			}
			else
			{
				AssertEquals("", wrapper.TransportCompany.CompanyCode);
			}
		}

		#endregion

		#region TestTransportCoAddress

		protected override void TestTransportCoAddressCore()
		{
			var docket = GetNewDocket();
			var wrapper = GetNewWarehouseJobGenericWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoDocAddress.E2_OA_Address = AddressPKWithDetailsFilled();
				AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", wrapper.TransportCoAddress.CompanyNameAndAddress);
			}
			else
			{
				AssertEquals("", wrapper.TransportCoAddress.CompanyNameAndAddress);
			}
		}

		ZGuid AddressPKWithDetailsFilled()
		{
			var org = Helper.CreateClient("Org1");
			var address = org.MainAddress;
			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "AU";
			return address.PK;
		}

		#endregion

		#region TestCarrierServiceLevel

		protected override void TestCarrierServiceLevelCore()
		{
			var docket = GetNewDocket();
			docket.WD_PL_NKCarrierServiceLevel = "GEN";

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("GEN", docketWrapper.CarrierServiceLevel.Code);
		}

		#endregion

		#region TestDropMode

		protected override void TestDropModeCore()
		{
			var docket = GetNewDocket();
			docket.WD_DropMode = "HSL";

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("DropMode", "HSL", docketWrapper.DropMode.Code);
		}

		#endregion

		#region TestVendorID

		protected override void TestVendorIDCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			AssertEquals("", docketWrapper.VendorID);

			var vendorIDRef = docket.References.AddNew();
			vendorIDRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.VendorIDCode;
			vendorIDRef.WX_Reference = "VendorID1234";

			AssertEquals("VendorID1234", docketWrapper.VendorID);
		}

		#endregion

		#region Test MasterBill

		protected override void TestMasterBillCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("No references", "", docketWrapper.MasterBill);

			var masterBillRef = docket.References.AddNew();
			masterBillRef.WX_Reference = "Master Bill 123456";
			masterBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;

			AssertEquals("References", "Master Bill 123456", docketWrapper.MasterBill);
		}

		#endregion

		#region Test MasterBillHeading

		protected override void TestMasterBillHeadingCore()
		{
			AssertEquals("Master Bill Heading", "Master Bill", WarehouseDocketWrapper.MasterBillHeading);
		}

		#endregion

		#region Test HouseBill

		protected override void TestHouseBillCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("No references", "", docketWrapper.HouseBill);

			var masterBillRef = docket.References.AddNew();
			masterBillRef.WX_Reference = "House Bill 123456";
			masterBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.HouseBill;

			AssertEquals("References", "House Bill 123456", docketWrapper.HouseBill);
		}

		#endregion

		#region Test HouseBillHeading

		protected override void TestHouseBillHeadingCore()
		{
			AssertEquals("House Bill Heading", "House Bill", WarehouseDocketWrapper.HouseBillHeading);
		}

		#endregion

		#region TestOtherReferences

		protected override void TestOtherReferencesCore()
		{
			var referenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			var expectedResult = "";
			AssertEquals("No references", expectedResult, docketWrapper.OtherReferences);
			Assert("Precondition", referenceTypes.Count > 4);

			var masterBillCode = "";
			var masterBillRef = "";
			var houseBillCode = "";
			var houseBillRef = "";
			int i = 0;
			foreach (ICodeDescription pair in referenceTypes)
			{
				var @ref = docket.References.AddNew();

				@ref.WX_RefType = pair.Code;
				if (pair.Code == WarehouseAdditionalReferenceTypes.Codes.MasterBill)
				{
					masterBillCode = referenceTypes.GetDescriptionFromCode(pair.Code);
					masterBillRef = "Master Bill 12345";
					@ref.WX_Reference = masterBillRef;
				}
				else if (pair.Code == WarehouseAdditionalReferenceTypes.Codes.HouseBill)
				{
					houseBillCode = referenceTypes.GetDescriptionFromCode(pair.Code);
					houseBillRef = "House Bill 98765";
					@ref.WX_Reference = houseBillRef;
				}
				else
				{
					@ref.WX_Reference = "Reference " + (++i).ToString();
					if (i < 4)
					{
						expectedResult += referenceTypes.GetDescriptionFromCode(pair.Code) + ": Reference " + i.ToString() + System.Environment.NewLine;
					}
					else if (i == 4)
					{
						expectedResult += referenceTypes.GetDescriptionFromCode(pair.Code) + ": Reference " + i.ToString();
					}
				}
			}

			AssertNotContains("Other Reference should NOT contain MasterBillCode", masterBillCode, docketWrapper.OtherReferences);
			AssertNotContains("Other Reference should NOT contain MasterBillReference", masterBillRef, docketWrapper.OtherReferences);
			AssertNotContains("Other Reference should NOT contain HouseBillCode", houseBillCode, docketWrapper.OtherReferences);
			AssertNotContains("Other Reference should NOT contain HouseBillReference", houseBillRef, docketWrapper.OtherReferences);

			AssertEquals("OtherReferences should show only first 4 references", expectedResult, docketWrapper.OtherReferences);
		}

		#endregion

		#region TestReferences

		protected override void TestReferencesCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			AssertEquals("No references", "", docketWrapper.References);
			var ref1 = docket.References.AddNew();
			ref1.WX_Reference = "REFERENCE1";
			ref1.WX_RefType = "RF1";

			var ref2 = docket.References.AddNew();
			ref2.WX_Reference = "REFERENCE2";
			ref2.WX_RefType = "RF2";
			AssertEquals("References", "RF1: REFERENCE1" + System.Environment.NewLine + "RF2: REFERENCE2", docketWrapper.References);
		}

		#endregion

		#region TestReferencesExtended

		protected override void TestReferencesExtendedCore()
		{
			var referenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			var expectedResult = "";
			AssertEquals("No references", expectedResult, docketWrapper.ReferencesExtended);
			Assert("Precondition", referenceTypes.Count > 4);

			int i = 0;
			foreach (ICodeDescription pair in referenceTypes)
			{
				var @ref = docket.References.AddNew();
				@ref.WX_Reference = "Reference " + (++i).ToString();
				@ref.WX_RefType = pair.Code;
			}

			expectedResult += referenceTypes[0].Description + ": Reference 1" + System.Environment.NewLine;
			expectedResult += referenceTypes[1].Description + ": Reference 2" + System.Environment.NewLine;
			expectedResult += referenceTypes[2].Description + ": Reference 3" + System.Environment.NewLine;
			expectedResult += referenceTypes[3].Description + ": Reference 4";

			AssertEquals("ReferencesExtended should show only first 4 references", expectedResult, docketWrapper.ReferencesExtended);
		}

		#endregion

		#region  TestStatus

		protected override void TestStatusCore()
		{
			AssertEquals("Status", WarehouseDocketWrapper.Status.Label);
			AssertEquals("New Entry (Unsaved)", WarehouseDocketWrapper.Status.Value);
		}

		#endregion

		#region TestSecondaryHeading

		protected override void TestSecondaryHeadingCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			docket.WD_DocketSubType = ZString.Empty;
			AssertEquals("Secondary Heading Details", "Details", docketWrapper.SecondaryHeading);

			docket.WD_DocketSubType = "ASS";
			AssertEquals("Secondary Heading Details", "Work Order Assembly Details", docketWrapper.SecondaryHeading);

			docket.WD_DocketSubType = "DIS";
			AssertEquals("Secondary Heading Details", "Work Order Disassembly Details", docketWrapper.SecondaryHeading);

			docket.WD_DocketSubType = "BAK";
			AssertEquals("Secondary Heading Details", "BACK ORDER Details", docketWrapper.SecondaryHeading);

			docket.WD_DocketSubType = "CUS";
			AssertEquals("Secondary Heading Details", "CUSTOMS RELEASE Details", docketWrapper.SecondaryHeading);

			docket.WD_DocketSubType = "ORD";
			AssertEquals("Secondary Heading Details", "ORDER Details", docketWrapper.SecondaryHeading);

			docket.WD_DocketSubType = "REP";
			AssertEquals("Secondary Heading Details", "REPEAT ORDER Details", docketWrapper.SecondaryHeading);
		}

		#endregion

		#region TestServiceLevel

		protected override void TestServiceLevelCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			docket.WD_RS_NKServiceLevel = "";
			AssertEquals("", docketWrapper.ServiceLevel.Code);

			docket.WD_RS_NKServiceLevel = "TST";
			AssertEquals("TST", docketWrapper.ServiceLevel.Code);
		}

		#endregion

		#region TestServices

		protected override void TestServicesCore()
		{
			var docket = GetNewDocket();
			var jobService = docket.Services.AddNew();
			jobService.ES_References = "Docket123";

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(1, docketWrapper.Services.Count);
			AssertEquals("Docket123", docketWrapper.Services[0].ReferenceNumber);
		}

		#endregion

		#region TestSubTypeDesc

		protected override void TestSubTypeDescCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals(docket.SubTypeDesc, docketWrapper.SubTypeDesc);
		}

		#endregion

		#region TestWarehouseCompanyLogo

		protected override void TestWarehouseCompanyLogoCore()
		{
			var docket = GetNewDocket();
			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);

			AssertNull(docketWrapper.WarehouseCompanyLogo);

			var logo1 = new Bitmap(1, 1);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, logo1);

			AssertEquals(logo1.Size, docketWrapper.WarehouseCompanyLogo.Size);

			var warehouse = Factory.New<WhsWarehouse>();
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(logo1.Size, docketWrapper.WarehouseCompanyLogo.Size);

			var logo2 = new Bitmap(2, 2);
			var branch = Factory.New<GlbBranch>();
			AssertEquals(logo1.Size, docketWrapper.WarehouseCompanyLogo.Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, logo2);
			AssertEquals(logo1.Size, docketWrapper.WarehouseCompanyLogo.Size);

			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			AssertEquals(logo2.Size, docketWrapper.WarehouseCompanyLogo.Size);

			var org = Factory.New<OrgHeader>();
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			AssertEquals(logo2.Size, docketWrapper.WarehouseCompanyLogo.Size);

			var logo3 = new Bitmap(3, 3);
			var memoryStream = new MemoryStream();
			logo3.Save(memoryStream, ImageFormat.Bmp);
			var blob = new ZBlob(memoryStream.ToArray());
			org.MiscServ.ClientDocumentLogo = blob;
			AssertEquals(logo3.Size, docketWrapper.WarehouseCompanyLogo.Size);
		}

		#endregion

		#region TestCustomCompanyLogo

		public void TestCustomCompanyLogo_FindWarehouseCompanyBranchLogo()
		{
			var docket = GetNewDocket();
			var wrapperWithNewLogo = (WarehouseDocketWrapper)GetNewWarehouseJobGenericWrapper(docket);
			AssertNull(wrapperWithNewLogo.CustomCompanyLogo);

			var logo1 = new Bitmap(1, 1);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, logo1);
			AssertEquals(logo1.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);

			var warehouse = Factory.New<WhsWarehouse>();
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(logo1.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);

			var logo2 = new Bitmap(2, 2);
			var branch = Factory.New<GlbBranch>();
			AssertEquals(logo1.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, logo2);
			AssertEquals(logo1.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);

			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			AssertEquals(logo2.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);

			var org = Factory.New<OrgHeader>();
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			AssertEquals(logo2.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);

			var logo3 = new Bitmap(3, 3);
			var memoryStream = new MemoryStream();
			logo3.Save(memoryStream, ImageFormat.Bmp);
			var blob = new ZBlob(memoryStream.ToArray());
			org.MiscServ.ClientDocumentLogo = blob;
			docket.WD_OH_Client = org.PK;
			AssertEquals(logo3.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);
		}

		public void TestCustomCompanyLogo()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "OH1";

			var docket = GetNewDocket();
			docket.WD_OH_Client = client.PK;
			Size defaultSize;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			using (var bitmap = new Bitmap(2, 2))
			{
				SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bitmap);
				defaultSize = bitmap.Size;
				AssertEquals(defaultSize, docketWrapper.CustomCompanyLogo.Size);
			}

			using (var bitmap = new Bitmap(1, 1))
			using (var memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, ImageFormat.Bmp);
				var blob = new ZBlob(memoryStream.ToArray());
				client.MiscServ.ClientDocumentLogo = blob;
				var wrapperWithNewLogo = (WarehouseDocketWrapper)GetNewWarehouseJobGenericWrapper(docket);
				AssertEquals(bitmap.Size, wrapperWithNewLogo.CustomCompanyLogo.Size);
				AssertNotEquals(defaultSize, wrapperWithNewLogo.CustomCompanyLogo.Size);
			}
		}

		#endregion

		#region TestWarehouseNameAndAddress

		protected override void TestWarehouseNameAndAddressCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			var docket = GetNewDocket();
			docket.WD_WW_Whs = warehouse.PK;
			var client = Factory.New<OrgHeader>();
			docket.WD_OH_Client = client.PK;
			client.OH_FullName = "HEADER";

			warehouse.WW_WarehouseName = "Warehouse One";

			var address1 = Factory.NewWithValidTestData<OrgAddress>();

			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			warehouse.WW_OA_WarehouseAddress = address1.PK;

			var expectedResult = "Warehouse One\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";

			AssertEquals(expectedResult, (ZString)GetNewWarehouseJobGenericWrapper(docket).WarehouseNameAndAddress);
		}

		#endregion

		#region TestWarehousePhoneAndFax

		protected override void TestWarehousePhoneAndFaxCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			var docket = GetNewDocket();
			docket.WD_WW_Whs = warehouse.PK;

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			warehouse.WW_OA_WarehouseAddress = address1.PK;
			var wrapper = GetNewWarehouseJobGenericWrapper(docket);

			AssertEquals(ZString.Empty, wrapper.WarehousePhoneAndFax);

			address1.OA_Phone = "0243456789";
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertEquals("Tel: +61 2 4345 6789", wrapper.WarehousePhoneAndFax);
			address1.OA_Fax = "0253456789";
			AssertEquals("Tel: +61 2 4345 6789   Fax: +61 2 5345 6789", wrapper.WarehousePhoneAndFax);
			address1.OA_Phone = ZString.Empty;
			AssertEquals("Fax: +61 2 5345 6789", wrapper.WarehousePhoneAndFax);
		}

		#endregion

		#region TestDocketStatus

		protected override void TestDocketStatusCore()
		{
			var docket = GetNewDocket();
			docket.WD_DocketStatus = "AAA";
			AssertEquals("AAA", GetNewWarehouseJobGenericWrapper(docket).DocketStatus);
		}

		#endregion

		#region TestDocketType

		protected override void TestDocketTypeCore()
		{
			var docket = GetNewDocket();
			docket.WD_DocketType = "ABC";
			AssertEquals("ABC", GetNewWarehouseJobGenericWrapper(docket).DocketType);
		}

		#endregion

		#region TestDocketSubType

		protected override void TestDocketSubTypeCore()
		{
			var docket = GetNewDocket();
			docket.WD_DocketSubType = "SUB";
			AssertEquals("SUB", GetNewWarehouseJobGenericWrapper(docket).DocketSubType);
		}

		#endregion

		#region TestTotalCubic

		protected override void TestTotalCubicCore()
		{
			var docket = GetNewDocket();
			docket.WD_TotalCubic = 0.88M;
			AssertEquals(0.88M, GetNewWarehouseJobGenericWrapper(docket).TotalCubic);
		}

		#endregion

		#region TestTotalWeight

		protected override void TestTotalWeightCore()
		{
			var docket = GetNewDocket();
			docket.WD_TotalWeight = 30.5M;
			AssertEquals(30.5M, GetNewWarehouseJobGenericWrapper(docket).TotalWeight);
		}

		#endregion

		#region TestTotalUnits

		protected override void TestTotalUnitsCore()
		{
			var docket = GetNewDocket();
			docket.WD_TotalUnits = 30m;
			AssertEquals(30m, GetNewWarehouseJobGenericWrapper(docket).TotalUnits);
		}

		#endregion

		#region TestTotalPallets

		protected override void TestTotalPalletsCore()
		{
			var docket = GetNewDocket();
			ZShort totalPallet = ZShort.Parse("5");
			docket.WD_TotalPallets = totalPallet;
			AssertEquals(totalPallet, GetNewWarehouseJobGenericWrapper(docket).TotalPallets);
		}

		#endregion

		#region TestBranch

		protected override void TestBranchCore()
		{
			var docket = GetNewDocket();
			docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			docket.Warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, GetNewWarehouseJobGenericWrapper(docket).Branch.Code);
		}

		#endregion

		#region TestSupplier

		protected override void TestSupplierCore()
		{
			var docket = GetNewDocket();
			var supplier = Factory.New<OrgHeader>();
			docket.SupplierDocAddress.OrganisationPK = supplier.PK;
			AssertEquals(supplier.OH_FullName, GetNewWarehouseJobGenericWrapper(docket).Supplier.CompanyName);
		}

		#endregion

		#region TestForwarder

		protected override void TestForwarderCore()
		{
			var docket = GetNewDocket();
			var forwarder = Factory.New<OrgHeader>();
			docket.WD_OH_Forwarder = forwarder.PK;
			AssertEquals(forwarder.OH_FullName, GetNewWarehouseJobGenericWrapper(docket).Forwarder.CompanyName);
		}

		#endregion

		#region TestClient

		protected override void TestClientCore()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "TestClient";

			var docket = GetNewDocket();
			docket.WD_OH_Client = client.PK;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("TestClient", docketWrapper.Client.CompanyCode);
		}

		#endregion

		#region TestJobClient

		protected override void TestJobClientCore()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "JobClient";

			var docket = GetNewDocket();
			docket.WD_OH_Client = client.PK;

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("JobClient", docketWrapper.Client.CompanyCode);
		}

		#endregion

		#region TestUNDGs

		public void TestUNDGs()
		{
			TestUNDGsCore();
		}

		protected virtual void TestUNDGsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1.PK, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2.PK, 1);

			var warehouseDocketWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Should have no UNDG", 0, warehouseDocketWrapper.UNDGs.Count);

			var line1Undg1 = orderLine1.Product.Parent.UNDGs.AddNew();
			var line1Undg2 = orderLine1.Product.Parent.UNDGs.AddNew();
			warehouseDocketWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertContainsExactElementsInAnyOrder("Should have undgs from line 1", new[] { line1Undg1, line1Undg2 }, warehouseDocketWrapper.UNDGs.Cast<UNDGSubstanceWrapper>().Select(w => w.WrappedObject));

			var line2Undg1 = orderLine1.Product.Parent.UNDGs.AddNew();
			var line2Undg2 = orderLine1.Product.Parent.UNDGs.AddNew();
			warehouseDocketWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertContainsExactElementsInAnyOrder("Should have undgs from line 1 and line 2", new[] { line1Undg1, line1Undg2, line2Undg1, line2Undg2 }, warehouseDocketWrapper.UNDGs.Cast<UNDGSubstanceWrapper>().Select(w => w.WrappedObject));
		}

		#endregion

		#region TestVehicleNumberCore

		public void TestVehicleNumber()
		{
			TestVehicleNumberCore();
		}

		public virtual void TestVehicleNumberCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var warehouseDocketWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Should have no vehicle number", "", warehouseDocketWrapper.VehicleNumber);

			order.VehicleNo = "007";
			warehouseDocketWrapper = new WarehouseOrderWrapper(order, Factory);
			AssertEquals("Should have the vehicle number", "007", warehouseDocketWrapper.VehicleNumber);
		}

		#endregion

		#region TestWhoFinalisedCore

		public void TestWhoFinalisedCore()
		{
			var testStaff = Factory.New<GlbStaff>();
			testStaff.GS_Code = "ABC";
			testStaff.GS_LoginName = "testStaff1";
			testStaff.GS_FullName = "testStaff1";

			Factory.Save();

			var docket = GetNewDocket();
			var wrapper = GetNewWarehouseJobGenericWrapper(docket);

			AssertEquals(string.Format("Created By: E"), wrapper.WhoCreated.LabelAndValue);
			AssertEquals(LabelValuePairWrapper.Empty, wrapper.WhoFinalised);

			using (Env.SetTemporaryUserContext(testStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				docket.Logs.AddNew(AutoEvents.ItemDocumentJobFinalised, "TestFinalise", ZDateTimeOffset.Now);
			}

			AssertEquals(string.Format("Created By: E"), wrapper.WhoCreated.LabelAndValue);
			AssertEquals(string.Format("Finalized By: ABC"), wrapper.WhoFinalised.LabelAndValue);
		}

		public void TestWhoFinalisedCore_EventDoesntExistOnFinalisedDocket()
		{
			var testStaff = Factory.New<GlbStaff>();
			testStaff.GS_Code = "ABC";
			testStaff.GS_LoginName = "testStaff1";
			testStaff.GS_FullName = "testStaff1";

			Factory.Save();

			var docket = GetNewDocket();
			var wrapper = GetNewWarehouseJobGenericWrapper(docket);

			AssertEquals(string.Format("Created By: E"), wrapper.WhoCreated.LabelAndValue);
			AssertEquals(LabelValuePairWrapper.Empty, wrapper.WhoFinalised);

			using (Env.SetTemporaryUserContext(testStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			}

			AssertEquals(string.Format("Created By: E"), wrapper.WhoCreated.LabelAndValue);
			AssertEquals(string.Format("Finalized By: E"), wrapper.WhoFinalised.LabelAndValue);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
			((WhsDocket)bizO).WD_WW_Whs = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return GetNewDocketInSpecifiedFactory(factory);
		}

		protected abstract WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory);

		#endregion

		#region Implementation

		WarehouseDocketWrapper WarehouseDocketWrapper => wrapper ?? (wrapper = (WarehouseDocketWrapper)Wrapper);
		WarehouseDocketWrapper wrapper;

		protected WhsDocket Docket => docket ?? (docket = GetNewDocket());
		WhsDocket docket;

		protected abstract WhsDocket GetNewDocket();

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return GetNewDocket();
		}

		#endregion
	}
}
