using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using CargoWise.Definitions;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	public abstract class DocWhsDocketTest<T, TWrapper> : DocumentWrapperTestCase
			where T : WhsDocket
			where TWrapper : DocWhsDocket
	{
		#region Related Business Objects

		public void TestDocketContainers()
		{
			Docket.Containers.AddNew();
			Docket.Containers.AddNew();
			AssertEquals("Should contain 2 containers", 2, DocketWrapper.DocketContainers.Count);
		}

		#endregion

		#region Properties

		#region Customs Fields

		public virtual void TestIsCustomsTransaction()
		{
			WhsWarehouse warehouse = Factory.New<WhsWarehouse>();
			Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(false, DocketWrapper.IsCustomsTransaction);
			new WhsTestHelperFunctionsEnv(Factory).EnableWarehouseForBond(warehouse, true);
			Docket.WD_DocketSubType = Enterprise.Warehouse.Transactions.CodeLists.OrderType.Codes.Customs;
			AssertEquals(true, DocketWrapper.IsCustomsTransaction);
		}

		public void TestCustomAttrib1()
		{
			Docket.WD_CustomAttrib1 = "CA1";
			AssertEquals("CA1", DocketWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			Docket.WD_CustomAttrib2 = "CA2";
			AssertEquals("CA2", DocketWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			Docket.WD_CustomAttrib3 = "CA3";
			AssertEquals("CA3", DocketWrapper.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			Docket.WD_CustomAttrib4 = "CA4";
			AssertEquals("CA4", DocketWrapper.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			Docket.WD_CustomAttrib5 = "CA1";
			AssertEquals("CA1", DocketWrapper.CustomAttrib5);
		}

		public void TestCustomDate1()
		{
			Docket.WD_CustomDate1 = DateTime.Today;
			AssertEquals(DateTime.Today, DocketWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			Docket.WD_CustomDate2 = DateTime.Today;
			AssertEquals(DateTime.Today, DocketWrapper.CustomDate2);
		}

		public void TestCustomDecimal1()
		{
			Docket.WD_CustomDecimal1 = 10m;
			AssertEquals(10m, DocketWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			Docket.WD_CustomDecimal2 = 10m;
			AssertEquals(10m, DocketWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			Docket.WD_CustomDecimal3 = 10m;
			AssertEquals(10m, DocketWrapper.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			Docket.WD_CustomDecimal4 = 10m;
			AssertEquals(10m, DocketWrapper.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			Docket.WD_CustomDecimal5 = 10m;
			AssertEquals(10m, DocketWrapper.CustomDecimal5);
		}

		public void TestCustomFlag1()
		{
			Docket.WD_CustomFlag1 = true;
			AssertEquals(true, DocketWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			Docket.WD_CustomFlag2 = true;
			AssertEquals(true, DocketWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			Docket.WD_CustomFlag3 = true;
			AssertEquals(true, DocketWrapper.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			Docket.WD_CustomFlag4 = true;
			AssertEquals(true, DocketWrapper.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			Docket.WD_CustomFlag5 = true;
			AssertEquals(true, DocketWrapper.CustomFlag5);
		}

		#endregion

		#region ZDateTime Fields

		public void TestBookingDate()
		{
			var today = ZDateTimeOffset.Today;
			Docket.WD_BookingDate = today;
			AssertEquals(today, DocketWrapper.BookingDate);
		}

		public void TestArrivalDate()
		{
			var today = ZDateTimeOffset.Today;
			Docket.WD_ArrivalDate = today;
			AssertEquals(today, DocketWrapper.ArrivalDate);
		}

		public void TestRequiredDate()
		{
			TestRequiredDateCore();
		}

		protected virtual void TestRequiredDateCore()
		{
			var today = ZDateTimeOffset.Today;
			Docket.WD_RequiredDate = today;
			AssertEquals(today, DocketWrapper.RequiredDate);
		}

		public void TestFinalisedDate()
		{
			var today = ZDateTimeOffset.Today;
			Docket.WD_FinalisedDate = today;
			AssertEquals(today, DocketWrapper.FinalisedDate);
		}

		#endregion

		#region ZString Fields

		public void TestConsigneeName()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			AssertEquals(nameof(wrapper.ConsigneeName), "", wrapper.ConsigneeName);
			TestConsigneeNameCore(docket, wrapper);
		}

		protected virtual void TestConsigneeNameCore(T docket, TWrapper wrapper)
		{
		}

		public void TestCarrierName()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoPK = ZGuid.Empty;
				AssertEquals(ZString.Empty, wrapper.CarrierName);

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_FullName = "TEST NAME";
				job.TransportCoPK = carrier.PK;
				AssertEquals("TEST NAME", wrapper.CarrierName);
			}
			else
			{
				AssertEquals(ZString.Empty, wrapper.CarrierName);
			}
		}

		public void TestDestinationPort()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			AssertEquals(nameof(wrapper.DestinationPort), "", wrapper.DestinationPort);
			TestDestinationPortCore(docket, wrapper);
		}

		protected virtual void TestDestinationPortCore(T docket, TWrapper wrapper)
		{
		}

		public void TestDGContact()
		{
			if (Docket.Client == null)
			{
				OrgHeader orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "OH1";
				Docket.WD_OH_Client = orgHeader.PK;
			}
			OrgContact contact = Docket.Client.Contacts.AddNew();
			contact.OC_ContactName = "A";
			Docket.Client.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			AssertEquals("Contact Name is incorrect", contact.OC_ContactName, DocketWrapper.DGContact);
		}

		public void TestEmergencyNumber()
		{
			if (Docket.Client == null)
			{
				OrgHeader orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "OH1";
				Docket.WD_OH_Client = orgHeader.PK;
			}
			OrgContact contact = Docket.Client.Contacts.AddNew();
			contact.OC_HomePhone = "123";
			Docket.Client.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			Docket.Client.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;
			AssertEquals("COntact Phone is incorrect", contact.OC_HomePhone, DocketWrapper.EmergencyNumber);
		}

		public void TestClientNameAndAddress()
		{
			if (Docket.Client == null)
			{
				OrgHeader orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "OH1";
				Docket.WD_OH_Client = orgHeader.PK;
			}
			Docket.Client.OH_FullName = "HEADER";
			OrgAddress address1 = Docket.Client.MainAddress;
			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			ZString expectedResult = "HEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";
			AssertEquals(expectedResult, DocketWrapper.ClientNameAndAddress);

			Docket.Client.Addresses.AddNew(OrgAddressType.Receivables, false);
			OrgAddress address2 = Docket.Client.Addresses[1];
			address2.OA_Address1 = "Address Line 3";
			address2.OA_Address2 = "Address Line 4";
			address2.OA_City = "CHICAGO";
			address2.OA_PostCode = "3000";

			expectedResult = "HEADER\nADDRESS LINE 3\nADDRESS LINE 4\nCHICAGO 3000\n" + Env.CurrentCompany.Country.Description.ToUpper(CultureInfo.InvariantCulture);
			AssertEquals(expectedResult, DocketWrapper.ClientNameAndAddress);
		}

		public void TestClientAddress()
		{
			if (Docket.Client == null)
			{
				OrgHeader orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "OH1";
				Docket.WD_OH_Client = orgHeader.PK;
			}
			Docket.Client.OH_FullName = "HEADER";
			OrgAddress address1 = Docket.Client.MainAddress;
			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			ZString expectedResult = "ADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";
			AssertEquals(expectedResult, DocketWrapper.ClientAddress);

			Docket.Client.Addresses.AddNew(OrgAddressType.Receivables, false);
			OrgAddress address2 = Docket.Client.Addresses[1];
			address2.OA_Address1 = "Address Line 3";
			address2.OA_Address2 = "Address Line 4";
			address2.OA_City = "CHICAGO";
			address2.OA_PostCode = "3000";

			expectedResult = "ADDRESS LINE 3\nADDRESS LINE 4\nCHICAGO 3000\n" + Env.CurrentCompany.Country.Description.ToUpper(CultureInfo.InvariantCulture);
			AssertEquals(expectedResult, DocketWrapper.ClientAddress);
		}

		public void TestClientNameAndCode()
		{
			if (Docket.Client == null)
			{
				OrgHeader orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "OH1";
				Docket.WD_OH_Client = orgHeader.PK;
			}
			Docket.Client.OH_FullName = "NAME1";
			Docket.Client.OH_Code = "A1";
			AssertEquals("Client Name is incorrect", "NAME1", DocketWrapper.ClientName);
			AssertEquals("Client Code is incorrect", "A1", DocketWrapper.ClientCode);
		}

		#region TestCustomerReference

		public void TestCustomerReference()
		{
			AssertEquals("Customer Reference", "", DocketWrapper.CustomerReference);

			Docket.WD_CustomerReference = "Customer Ref";
			AssertEquals("Customer Reference", "Customer Ref", DocketWrapper.CustomerReference);
		}

		public void TestCustomerReferenceBarcode()
		{
			var barcode = new TextBarcode("Customer Ref");
			Docket.WD_CustomerReference = barcode.TextToEncode;
			AssertEquals("Customer Reference Barcode", barcode.TextAs128sFontString, DocketWrapper.CustomerReferenceBarcode);
		}

		#endregion

		public void TestTransportRefNo()
		{
			AssertEquals("Transport Reference", "", DocketWrapper.TransportReference);
			Docket.WD_TransportReference = "Transport Ref";
			AssertEquals("Transport Reference", "Transport Ref", DocketWrapper.TransportReference);
		}

		public void TestDropOffName()
		{
			AssertEquals("DropOff Name is incorrect", ZString.Empty, DocketWrapper.DropOffName);

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "DropOffName";
			AssertEquals("DropOff Name is incorrect", ZString.Empty, DocketWrapper.DropOffName);

			Docket.DropOffPK = org.PK;
			AssertEquals("DropOff Name is incorrect", "DropOffName", DocketWrapper.DropOffName);
		}

		public void TestSupplierAddress()
		{
			JobDocAddress supplierAddress = Docket.DocAddresses.AddNew(DocAddressType.SupplierDocumentaryAddress);
			supplierAddress.E2_AddressOverride = true;
			supplierAddress.E2_Address1 = "Address Line 1";
			supplierAddress.E2_Address2 = "Address Line 2";
			supplierAddress.E2_City = "SYDNEY";
			supplierAddress.E2_Postcode = "2000";
			supplierAddress.E2_CompanyName = "ABBAHO INDUSTRIES";

			ZString expectedResult = "ABBAHO INDUSTRIES\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000 " + Env.CurrentCompany.Country.Description.ToUpper(CultureInfo.InvariantCulture);
			AssertEquals(expectedResult, DocketWrapper.SupplierAddress);
		}

		public void TestWarehouseName()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}
			Docket.Warehouse.WW_WarehouseName = "WHS1";
			AssertEquals("WHS1", DocketWrapper.WarehouseName);
		}

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "WHS1";

			Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("WarehouseName in English", "WHS1", DocketWrapper.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "WHS1").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库1"));
				AssertEquals("WarehouseName in Chinese", "仓库1", DocketWrapper.WarehouseName);
			}
		}

		public void TestWarehouseNameAndAddress()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}

			Docket.Warehouse.WW_WarehouseName = "Warehouse One";

			var address1 = Factory.NewWithValidTestData<OrgAddress>();

			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			Docket.Warehouse.WW_OA_WarehouseAddress = address1.PK;

			ZString expectedResult = "Warehouse One\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";

			AssertEquals(expectedResult, DocketWrapper.WarehouseNameAndAddress);
		}

		public void TestWarehousePhoneAndFax()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			Docket.Warehouse.WW_OA_WarehouseAddress = address1.PK;
			AssertEquals(ZString.Empty, DocketWrapper.WarehousePhoneAndFax);

			address1.OA_Phone = "0243456789";
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertEquals("Tel: +61 2 4345 6789", DocketWrapper.WarehousePhoneAndFax);
			address1.OA_Fax = "0253456789";
			AssertEquals("Tel: +61 2 4345 6789   Fax: +61 2 5345 6789", DocketWrapper.WarehousePhoneAndFax);
			address1.OA_Phone = ZString.Empty;
			AssertEquals("Fax: +61 2 5345 6789", DocketWrapper.WarehousePhoneAndFax);
		}

		public void TestDocketID()
		{
			Docket.WD_DocketID = "ID123";
			AssertEquals("ID123", DocketWrapper.DocketID);
		}

		public void TestDocketStatus()
		{
			Docket.WD_DocketStatus = "AAA";
			AssertEquals("AAA", DocketWrapper.DocketStatus);
		}

		public void TestDocketType()
		{
			Docket.WD_DocketType = "ABC";
			AssertEquals("ABC", DocketWrapper.DocketType);
		}

		public void TestDocketSubType()
		{
			Docket.WD_DocketSubType = "SUB";
			AssertEquals("SUB", DocketWrapper.DocketSubType);
		}

		public void TestExternalReferenceBarcode()
		{
			Docket.WD_ExternalReference = "EXTREF";
			TextBarcode barcode = new TextBarcode("EXTREF");
			AssertEquals(barcode.TextAs128sFontString, DocketWrapper.ExternalReferenceBarcode);
		}

		public void TestExternalReference()
		{
			Docket.WD_ExternalReference = "EXTREF";
			AssertEquals("EXTREF", DocketWrapper.ExternalReference);
		}

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			AssertEquals("No service level", "", wrapper.ServiceLevel);

			if (SupportsCarrierServiceLevel(docket))
			{
				var job = (IJobWithTransportCompany)docket;
				var transportCo = Factory.New<OrgHeader>();
				var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				serviceLevel.PL_Code = "XXX";
				serviceLevel.PL_CarrierServiceLevelDescription = "XXX Service";

				job.TransportCoPK = transportCo.PK;
				docket.WD_PL_NKCarrierServiceLevel = "XXX";
				AssertEquals("XXX Service", wrapper.ServiceLevel);
			}
		}

		protected virtual bool SupportsCarrierServiceLevel(T docket) => docket is IJobWithTransportCompany;

		#endregion

		public void TestTotalWeightUnit()
		{
			Docket.WD_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(Core.Constants.Weight.Kilograms, DocketWrapper.TotalWeightUnit);
		}

		public void TestTotalPackagesUnit()
		{
			Docket.WD_F3_NKTotalPackType = Core.Constants.PkgUnit.Bag;
			AssertEquals(Core.Constants.PkgUnit.Bag, DocketWrapper.TotalPackagesUnit);
		}

		public void TestTotalCubicUnit()
		{
			Docket.WD_TotalCubicUnit = Core.Constants.Volume.CubicMetres;
			AssertEquals(Core.Constants.Volume.CubicMetres, DocketWrapper.TotalCubicUnit);
		}

		public virtual void TestPickNo()
		{
			AssertEquals("", DocketWrapper.PickNo);
		}

		public void TestReferences()
		{
			AssertEquals("No references", "", DocketWrapper.References);
			WhsDocketReference ref1 = Docket.References.AddNew();
			ref1.WX_Reference = "REFERENCE1";
			ref1.WX_RefType = "RF1";

			WhsDocketReference ref2 = Docket.References.AddNew();
			ref2.WX_Reference = "REFERENCE2";
			ref2.WX_RefType = "RF2";
			AssertEquals("References", "RF1: REFERENCE1\nRF2: REFERENCE2", DocketWrapper.References);
		}

		public void TestReferencesExtended()
		{
			ICodeDescriptionPairListWithDefaultCode referenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;

			ZString expectedResult = "";

			AssertEquals("No references", expectedResult, DocketWrapper.ReferencesExtended);
			Assert("Precondition", referenceTypes.Count > 4);

			int i = 0;
			foreach (ICodeDescription pair in referenceTypes)
			{
				WhsDocketReference @ref = Docket.References.AddNew();
				@ref.WX_Reference = "Reference " + (++i).ToString();
				@ref.WX_RefType = pair.Code;
			}

			expectedResult += referenceTypes[0].Description + ": Reference 1" + System.Environment.NewLine;
			expectedResult += referenceTypes[1].Description + ": Reference 2" + System.Environment.NewLine;
			expectedResult += referenceTypes[2].Description + ": Reference 3" + System.Environment.NewLine;
			expectedResult += referenceTypes[3].Description + ": Reference 4";

			AssertEquals("ReferencesExtended should show only first 4 references", expectedResult, DocketWrapper.ReferencesExtended);
		}

		public void TestBillOfLadingNo()
		{
			AssertEquals("No bill of lading no", "", DocketWrapper.BillOfLadingNo);
			WhsDocketReference ref1 = Docket.References.AddNew();
			ref1.WX_Reference = "REFERENCE1";
			ref1.WX_RefType = "RF1";

			AssertEquals("No bill of lading no", "", DocketWrapper.BillOfLadingNo);

			Docket.WD_BOLNo = "REFERENCE2";
			AssertEquals("Bill Of Lading No", "REFERENCE2", DocketWrapper.BillOfLadingNo);

			WhsDocketReference ref3 = Docket.References.AddNew();
			ref3.WX_Reference = "REFERENCE3";
			ref3.WX_RefType = "HSB";
			AssertEquals("Bill Of Lading No", "REFERENCE2", DocketWrapper.BillOfLadingNo);
		}

		public void TestVehicleNo()
		{
			AssertEquals("No trailer no", "", DocketWrapper.VehicleNo);
			var ref1 = Docket.References.AddNew();
			ref1.WX_Reference = "REFERENCE1";
			ref1.WX_RefType = "RF1";
			AssertEquals("No trailer no", "", DocketWrapper.VehicleNo);

			var ref2 = Docket.References.AddNew();
			ref2.WX_Reference = "REFERENCE2";
			ref2.WX_RefType = "VHN";
			AssertEquals("Trailer No", "REFERENCE2", DocketWrapper.VehicleNo);

			var ref3 = Docket.References.AddNew();
			ref3.WX_Reference = "REFERENCE3";
			ref3.WX_RefType = "VHN";
			AssertEquals("Trailer No", "REFERENCE2", DocketWrapper.VehicleNo);
		}

		public void TestINCOTerm()
		{
			AssertEquals(ZString.Empty, DocketWrapper.INCOTerm);
			Docket.WD_INCO = "PPD";
			AssertEquals("Prepaid", DocketWrapper.INCOTerm);
			Docket.WD_INCO = "FCD";
			AssertEquals(Docket.Lookups.INCOTerms.GetDescriptionFromCode("FCD"), DocketWrapper.INCOTerm);
		}

		public void TestContainerNumberAndTypeLine()
		{
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Container Number And Type Line", ZString.Empty, CreateWhsDocketWrapper(Docket).ContainerNumberAndTypeLine);

			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			RefContainer ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "40FR";
			RefContainer ref4 = Factory.New<RefContainer>();
			ref4.RC_Code = "30FR";

			WhsDocketContainer container1 = Docket.Containers.AddNew();
			container1.WC_ContainerNum = "CONTAINER1";
			container1.WC_RC = ref1.PK;

			Docket.Containers.AddNew().WC_ContainerNum = "CONTAINER2";

			WhsDocketContainer container3 = Docket.Containers.AddNew();
			container3.WC_ContainerNum = "CONTAINER3";
			container3.WC_RC = ref3.PK;

			WhsDocketContainer container4 = Docket.Containers.AddNew();
			container4.WC_ContainerNum = "CONTAINER4";
			container4.WC_RC = ref4.PK;

			Docket.Containers.AddNew().WC_ContainerNum = "CONTAINER5";
			AssertEquals("Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5", DocketWrapper.ContainerNumberAndTypeLine);

			Docket.Containers.AddNew().WC_ContainerNum = "CONTAINER6";
			AssertEquals("Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5 ...", CreateWhsDocketWrapper(Docket).ContainerNumberAndTypeLine);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Container Number And Type Line", "See attached Container Details list.", CreateWhsDocketWrapper(Docket).ContainerNumberAndTypeLine);
		}

		public void TestDocumentName()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Some menu title");
			DocketWrapper.SetTemplateConstants(dictionary);
			AssertEquals("DocumentName", "Some menu title", DocketWrapper.DocumentName);
		}

		public void TestEmailSubjectNumber()
		{
			Docket.WD_ExternalReference = "abc";
			AssertEquals("abc", DocketWrapper.EmailSubjectNumber);
		}

		#endregion

		#region ZDecimal Fields

		public void TestShipperCODAmount()
		{
			AssertEquals(0m, DocketWrapper.ShipperCODAmount);

			Docket.WD_ShipperCODAmount = 12m;
			AssertEquals(0m, DocketWrapper.ShipperCODAmount);

			Docket.WD_INCO = "FCD";
			AssertEquals(12m, DocketWrapper.ShipperCODAmount);
		}

		public void TestTotalCubic()
		{
			Docket.WD_TotalCubic = 0.88M;
			AssertEquals(0.88M, DocketWrapper.TotalCubic);
		}

		public void TestTotalWeight()
		{
			Docket.WD_TotalWeight = 30.5M;
			AssertEquals(30.5M, DocketWrapper.TotalWeight);
		}

		public void TestTotalUnits()
		{
			Docket.WD_TotalUnits = 30m;
			AssertEquals(30m, DocketWrapper.TotalUnits);
		}

		public void TestUnitsSent()
		{
			Docket.WD_UnitsSent = 10m;
			AssertEquals(10m, DocketWrapper.UnitsSent);
		}

		public void TestCubicSent()
		{
			Docket.WD_CubicSent = 10.345m;
			AssertEquals(10.345m, DocketWrapper.CubicSent);
		}

		public void TestWeightSent()
		{
			Docket.WD_WeightSent = 10.23m;
			AssertEquals(10.23m, DocketWrapper.WeightSent);
		}

		#endregion

		#region ZInt Fields

		public void TestTotalNumberOfLabels()
		{
			Docket.WD_PackagesSent = 10;
			AssertEquals(0, DocketWrapper.TotalNumberOfLabels);

			WhsDocketLabelControl docketLabel = new WhsDocketLabelControl(Docket, 10);
			DocketWrapper = CreateWhsDocketWrapper(docketLabel);
			docketLabel.NumberOfLabelsToPrint = 5;
			AssertEquals(5, DocketWrapper.TotalNumberOfLabels);

			docketLabel.NumberOfLabelsToPrint = 10;
			AssertEquals(10, DocketWrapper.TotalNumberOfLabels);
		}

		public void TestPackagesSent()
		{
			Docket.WD_PackagesSent = 5;
			AssertEquals(5, DocketWrapper.PackagesSent);
		}

		#endregion

		#region ZShort Fields

		public void TestTotalPallets()
		{
			ZShort totalPallet = ZShort.Parse("5");
			Docket.WD_TotalPallets = totalPallet;
			AssertEquals(totalPallet, DocketWrapper.TotalPallets);
		}

		public void TestPalletsSent()
		{
			ZShort palletsSent = ZShort.Parse("2");
			Docket.WD_PalletsSent = palletsSent;
			AssertEquals(palletsSent, DocketWrapper.PalletsSent);
		}

		#endregion

		#region ZBool Fields

		public void TestPrintPageWithContainerNumber()
		{
			Docket.Containers.AddNew().WC_ContainerNum = "CON1111";
			Docket.Containers.AddNew().WC_ContainerNum = "CON2222";
			Docket.Containers.AddNew().WC_ContainerNum = "CON3333";
			Docket.Containers.AddNew().WC_ContainerNum = "CON4444";
			Docket.Containers.AddNew().WC_ContainerNum = "CON5555";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)DocketWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)DocketWrapper.PrintPageWithContainerNumber);
			Docket.Containers.AddNew().WC_ContainerNum = "CON6666";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)DocketWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be true", true, (bool)DocketWrapper.PrintPageWithContainerNumber);
		}

		#endregion

		#region Wrapper Fields

		public void TestBranch()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}
			Docket.Warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, DocketWrapper.Branch.Code);
		}

		public void TestClient()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "OH1";
			Docket.WD_OH_Client = client.PK;
			AssertEquals(client.OH_FullName, DocketWrapper.Client.Name);
		}

		public void TestForwarder()
		{
			var forwarder = Factory.New<OrgHeader>();
			Docket.WD_OH_Forwarder = forwarder.PK;
			AssertEquals(forwarder.OH_FullName, DocketWrapper.Forwarder.Name);
		}

		public void TestSupplier()
		{
			var supplier = Factory.New<OrgHeader>();
			Docket.SupplierDocAddress.OrganisationPK = supplier.PK;
			AssertEquals(supplier.OH_FullName, DocketWrapper.Supplier.Name);
		}

		public void TestTransportCo()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				var transportCo = Factory.New<OrgHeader>();
				job.TransportCoPK = transportCo.PK;
				AssertEquals(transportCo.OH_FullName, wrapper.TransportCo.Name);
			}
			else
			{
				AssertNull(wrapper.TransportCo);
			}
		}

		public void TestTransportCo_WithOverridenAddress()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_Address1 = "322 Street";
				job.TransportCoDocAddress.E2_AddressType = "TRA";
				job.TransportCoDocAddress.E2_City = "Here";
				job.TransportCoDocAddress.E2_CompanyName = "COOL COMPANY";
				job.TransportCoDocAddress.E2_Contact = "Ben Til";

				AssertEquals("COOL COMPANY", wrapper.TransportCo.Name);
			}
			else
			{
				AssertNull(wrapper.TransportCo);
			}
		}

		public void TestTransportCoDocAddress()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				var address = GetNewOrgAddress();
				var org = Factory.New<OrgHeader>();
				org.OH_RL_NKClosestPort = "AU";
				org.OH_FullName = "My Organisation Name";
				address.OA_OH = org.PK;

				job.TransportCoDocAddress.OrganisationPK = org.PK;
				job.TransportCoDocAddress.E2_OA_Address = address.PK;

				AssertResults(wrapper.TransportCoAddress, "My Organisation Name", "My Address 1", "My Address 2", "My City", "My Code", "My State", "US");

				address.OA_RL_NKRelatedPortCode = "";
				AssertEquals("US", wrapper.TransportCoAddress.Country.Code);

				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_CompanyName = "Name Override";
				job.TransportCoDocAddress.E2_Address1 = "Address 1 Override";
				job.TransportCoDocAddress.E2_Address2 = "Address 2 Override";
				job.TransportCoDocAddress.E2_City = "City Override";
				job.TransportCoDocAddress.E2_Postcode = "Code";
				job.TransportCoDocAddress.E2_State = "State";
				job.TransportCoDocAddress.E2_RN_NKCountryCode = "US";

				AssertResults(wrapper.TransportCoAddress, "Name Override", "Address 1 Override", "Address 2 Override", "City Override", "Code", "State", "US");
			}
			else
			{
				AssertNull(wrapper.TransportCoAddress);
			}
		}

		public void TestTransportCoDocNameAndAddress()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			if (docket is IJobWithTransportCompany job)
			{
				var address = GetNewOrgAddress();
				var org = Factory.New<OrgHeader>();
				org.OH_RL_NKClosestPort = "AU";
				org.OH_FullName = "My Organisation Name";
				address.OA_OH = org.PK;

				job.TransportCoDocAddress.OrganisationPK = org.PK;
				job.TransportCoDocAddress.E2_OA_Address = address.PK;

				AssertResults(wrapper.TransportCoNameAndAddress, "My Organisation Name", "My Address 1", "My Address 2", "My City", "My Code", "My State", "US");

				address.OA_RL_NKRelatedPortCode = "";
				AssertEquals("US", wrapper.TransportCoAddress.Country.Code);

				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_CompanyName = "Name Override";
				job.TransportCoDocAddress.E2_Address1 = "Address 1 Override";
				job.TransportCoDocAddress.E2_Address2 = "Address 2 Override";
				job.TransportCoDocAddress.E2_City = "City Override";
				job.TransportCoDocAddress.E2_Postcode = "Code";
				job.TransportCoDocAddress.E2_State = "State";
				job.TransportCoDocAddress.E2_RN_NKCountryCode = "US";

				AssertResults(wrapper.TransportCoNameAndAddress, "Name Override", "Address 1 Override", "Address 2 Override", "City Override", "Code", "State", "US");
			}
			else
			{
				AssertNull(wrapper.TransportCoAddress);
			}
		}

		public void TestSupplierDocAddress()
		{
			OrgAddress address = GetNewOrgAddress();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_OH = org.PK;
			address.OA_RN_NKCountryCode = "AU";

			Docket.SupplierDocAddress.OrganisationPK = org.PK;
			Docket.SupplierDocAddress.E2_OA_Address = address.PK;

			AssertResults(DocketWrapper.SupplierDocAddress, "My Organisation Name", "My Address 1", "My Address 2", "My City", "My Code", "My State", "AU");

			Docket.SupplierDocAddress.E2_AddressOverride = true;
			Docket.SupplierDocAddress.E2_CompanyName = "Name Override";
			Docket.SupplierDocAddress.E2_Address1 = "Address 1 Override";
			Docket.SupplierDocAddress.E2_Address2 = "Address 2 Override";
			Docket.SupplierDocAddress.E2_City = "City Override";
			Docket.SupplierDocAddress.E2_Postcode = "Code";
			Docket.SupplierDocAddress.E2_State = "State";
			Docket.SupplierDocAddress.E2_RN_NKCountryCode = "AU";

			AssertResults(DocketWrapper.SupplierDocAddress, "Name Override", "Address 1 Override", "Address 2 Override", "City Override", "Code", "State", "AU");
		}

		public void TestGoodsBillToAddress()
		{
			OrgAddress address = GetNewOrgAddress();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			org.MainAddress.OA_RN_NKCountryCode = "AU";

			address.OA_OH = org.PK;
			Docket.GoodsBillToPK = org.PK;
			Docket.GoodsBillToAddressPK = address.PK;

			AssertEquals("ConsigneeAddress is of type DocDocAddress", typeof(DocDocAddress), DocketWrapper.GoodsBillToAddress.GetType());

			AssertResults(DocketWrapper.GoodsBillToAddress, "My Organisation Name", "My Address 1", "My Address 2", "My City", "My Code", "My State", "US");

			address.OA_RL_NKRelatedPortCode = "";
			AssertEquals("US", DocketWrapper.GoodsBillToAddress.Country.Code);

			Docket.GoodsBillToDocAddress.E2_AddressOverride = true;
			Docket.GoodsBillToDocAddress.E2_CompanyName = "Name Override";
			Docket.GoodsBillToDocAddress.E2_Address1 = "Address 1 Override";
			Docket.GoodsBillToDocAddress.E2_Address2 = "Address 2 Override";
			Docket.GoodsBillToDocAddress.E2_City = "City Override";
			Docket.GoodsBillToDocAddress.E2_Postcode = "Code";
			Docket.GoodsBillToDocAddress.E2_State = "State";
			Docket.GoodsBillToDocAddress.E2_RN_NKCountryCode = "US";

			AssertResults(DocketWrapper.GoodsBillToAddress, "Name Override", "Address 1 Override", "Address 2 Override", "City Override", "Code", "State", "US");
		}

		public void TestConsignee()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			AssertNull(nameof(wrapper.Consignee), wrapper.Consignee);
			TestConsigneeCore(docket, wrapper);
		}

		protected virtual void TestConsigneeCore(T docket, TWrapper wrapper)
		{
		}

		public void TestConsigneeAddress()
		{
			var docket = GetNewDocket();
			var wrapper = CreateWhsDocketWrapper(docket);
			TestConsigneeAddressCore(docket, wrapper);
		}

		protected virtual void TestConsigneeAddressCore(T docket, TWrapper wrapper)
		{
			AssertNull(nameof(wrapper.ConsigneeAddress), wrapper.ConsigneeAddress);
		}

		public void TestWarehouseAddress()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}

			Docket.Warehouse.WW_WarehouseName = "Warehouse One";

			OrgHeader whsOrg = Factory.New<OrgHeader>();
			OrgAddress address = whsOrg.MainAddress;

			whsOrg.OH_RL_NKClosestPort = "AU";
			whsOrg.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";

			address.OA_OH = whsOrg.PK;
			Docket.Warehouse.WW_OA_WarehouseAddress = address.PK;

			AssertEquals("WarehouseAddress is of type DocDocAddress", typeof(DocDocAddress), DocketWrapper.WarehouseAddress.GetType());

			AssertResults(DocketWrapper.WarehouseAddress, "My Organisation Name", "My Address 1", "My Address 2", "My City", "My Code", "My State", "US");
		}

		public void TestWarehouseCompanyLogo()
		{
			AssertNull(DocketWrapper.WarehouseCompanyLogo);

			Bitmap logo1 = new Bitmap(1, 1);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, logo1);

			AssertEquals(logo1.Size, DocketWrapper.WarehouseCompanyLogo.Size);

			WhsWarehouse warehouse = Factory.New<WhsWarehouse>();
			Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(logo1.Size, DocketWrapper.WarehouseCompanyLogo.Size);

			Bitmap logo2 = new Bitmap(2, 2);
			GlbBranch branch = Factory.New<GlbBranch>();
			AssertEquals(logo1.Size, DocketWrapper.WarehouseCompanyLogo.Size);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, logo2);
			AssertEquals(logo1.Size, DocketWrapper.WarehouseCompanyLogo.Size);

			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			AssertEquals(logo2.Size, DocketWrapper.WarehouseCompanyLogo.Size);

			OrgHeader org = Factory.New<OrgHeader>();
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			AssertEquals(logo2.Size, DocketWrapper.WarehouseCompanyLogo.Size);

			Bitmap logo3 = new Bitmap(3, 3);
			MemoryStream memoryStream = new MemoryStream();
			logo3.Save(memoryStream, ImageFormat.Bmp);
			ZBlob blob = new ZBlob(memoryStream.ToArray());
			org.MiscServ.ClientDocumentLogo = blob;
			AssertEquals(logo3.Size, DocketWrapper.WarehouseCompanyLogo.Size);
		}

		public void TestCustomCompanyLogo()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "OH1";
			Docket.WD_OH_Client = client.PK;
			Size defaultSize;

			using (Bitmap bitmap = new Bitmap(2, 2))
			{
				SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bitmap);
				defaultSize = bitmap.Size;
				AssertEquals(defaultSize, DocketWrapper.CustomCompanyLogo.Size);
			}

			using (Bitmap bitmap = new Bitmap(1, 1))
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, ImageFormat.Bmp);
				ZBlob blob = new ZBlob(memoryStream.ToArray());
				client.MiscServ.ClientDocumentLogo = blob;
				AssertEquals(bitmap.Size, DocketWrapper.CustomCompanyLogo.Size);
				AssertNotEquals(defaultSize, DocketWrapper.CustomCompanyLogo.Size);
			}
		}

		#endregion

		#region IDocServicesParent Members

		public void TestConsolNumber()
		{
			AssertEquals("Consol Number", "", ServicesParentWrapper.ConsolNumber);
		}

		public void TestGoodsDescription()
		{
			Docket.WD_GoodsDescription = "WhsDocket Goods Description";
			AssertEquals("Goods Description", "WhsDocket Goods Description", ServicesParentWrapper.GoodsDescription);
		}

		public void TestPackages()
		{
			Docket.WD_PackagesSent = 2;
			AssertEquals("Packages", "2", ServicesParentWrapper.Packages);
		}

		public void TestMasterBillNum()
		{
			AssertEquals("No references", "", ServicesParentWrapper.MasterBillNum);

			var masterBillRef = Docket.References.AddNew();
			masterBillRef.WX_Reference = "Master Bill 123456";
			masterBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			AssertEquals("References", "Master Bill 123456", ServicesParentWrapper.MasterBillNum);
		}

		public void TestMasterBillHeading()
		{
			AssertEquals("MasterBillHeading", "Master Bill", ServicesParentWrapper.MasterBillHeading);
		}

		public void TestHouseBill()
		{
			AssertEquals("No references", "", ServicesParentWrapper.HouseBill);

			var masterBillRef = Docket.References.AddNew();
			masterBillRef.WX_Reference = "House Bill 123456";
			masterBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.HouseBill;
			AssertEquals("References", "House Bill 123456", ServicesParentWrapper.HouseBill);
		}

		public void TestHouseBillHeading()
		{
			AssertEquals("HouseBillHeading", "House Bill", ServicesParentWrapper.HouseBillHeading);
		}

		public void TestContext()
		{
			AssertEquals("Context", "", ServicesParentWrapper.Context);
		}

		public void TestContainerNumbers()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var container1 = Docket.Containers.AddNew();
			var container2 = Docket.Containers.AddNew();

			container1.WC_RC = gP20.PK;
			container1.WC_ContainerNum = "C1";
			container2.WC_RC = gP40.PK;
			container2.WC_ContainerNum = "C2";
			AssertEquals("Many", ServicesParentWrapper.ContainerNumbers);

			container2.WC_RC = gP20.PK;
			AssertEquals("C1, C2", ServicesParentWrapper.ContainerNumbers);
		}

		public void TestTransportInfo()
		{
			AssertEquals("TransportInfo", "", ServicesParentWrapper.TransportInfo);
		}

		public void TestETD()
		{
			Docket.WD_ETD = ZDateTimeOffset.Today;
			AssertEquals("ETD", Docket.WD_ETD.ToZDateTime(), ServicesParentWrapper.ETD);
		}

		public void TestETA()
		{
			Docket.WD_ETA = ZDateTimeOffset.Today;
			AssertEquals("ETA", Docket.WD_ETA.ToZDateTime(), ServicesParentWrapper.ETA);
		}

		public void TestWeight()
		{
			Docket.WD_TotalWeight = 500m;
			AssertEquals("Weight", "500", ServicesParentWrapper.Weight);
		}

		public void TestVolume()
		{
			Docket.WD_TotalCubic = 10m;
			AssertEquals("Volume", "10", ServicesParentWrapper.Volume);
		}

		public void TestWeightUnit()
		{
			Docket.WD_TotalWeightUnit = "KG";
			AssertEquals("WeightUnit", "KG", ServicesParentWrapper.WeightUnit);
		}

		public void TestVolumeUnit()
		{
			Docket.WD_TotalCubicUnit = "M3";
			AssertEquals("VolumeUnit", "M3", ServicesParentWrapper.VolumeUnit);
		}

		public void TestPortOfLoading()
		{
			AssertNull("Port of Loading", ServicesParentWrapper.PortOfLoading);
		}

		public void TestPortOfDischarge()
		{
			AssertNull("Port of Discharge", ServicesParentWrapper.PortOfDischarge);
		}

		public void TestOwnerRefAndOrderRef()
		{
			Docket.WD_ExternalReference = "Docket123";
			AssertEquals("OwnerRefAndOrderRef", "Docket123", ServicesParentWrapper.OwnerRefAndOrderRef);
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			AssertEquals("OwnerRefAndOrderRefHeading", "", ServicesParentWrapper.OwnerRefAndOrderRefHeading);
		}

		#endregion

		#region Implementation

		void AssertResults(DocDocAddress docAddress, ZString companyName, ZString addressLine1, ZString addressLine2, ZString city, ZString postCode, ZString state, ZString countryCode)
		{
			AssertEquals(companyName, docAddress.CompanyName);
			AssertEquals(addressLine1, docAddress.Address1);
			AssertEquals(addressLine2, docAddress.Address2);
			AssertEquals(city, docAddress.City);
			AssertEquals(postCode, docAddress.PostCode);
			AssertEquals(state, docAddress.State);
			AssertEquals(countryCode, docAddress.Country.Code);
		}

		OrgAddress GetNewOrgAddress()
		{
			OrgAddress result = Factory.New<OrgAddress>();

			result.OA_Address1 = "My Address 1";
			result.OA_Address2 = "My Address 2";
			result.OA_City = "My City";
			result.OA_PostCode = "My Code";
			result.OA_State = "My State";
			result.OA_RL_NKRelatedPortCode = "US";
			return result;
		}

		#endregion

		#endregion

		#region Implementation

		protected OrgHeader SetupClientCode(ZString codeType, ZString regNo)
		{
			return SetupClientCode(null, codeType, regNo);
		}

		protected OrgHeader SetupClientCode(OrgHeader org, ZString codeType, ZString regNo)
		{
			if (org == null)
			{
				org = Factory.New<OrgHeader>();
			}

			org.OH_Code = "OH1";
			OrgCusCode code = org.CustomsCodes.AddNew();
			code.OK_CodeType = codeType;
			code.OK_CustomsRegNo = regNo;
			return org;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocketWrapper };
		}

		protected T Docket;
		protected TWrapper DocketWrapper;

		protected override void SetUp()
		{
			Docket = GetNewDocket();
			DocketWrapper = CreateWhsDocketWrapper(Docket);
			ServicesParentWrapper = DocketWrapper;
			AssertNotNull("Wrapper not null", DocketWrapper);
			base.SetUp();
		}

		protected abstract TWrapper CreateWhsDocketWrapper(WhsDocketLabelControl docketLabel);
		protected abstract TWrapper CreateWhsDocketWrapper(T docket);
		protected virtual T GetNewDocket()
		{
			return Factory.NewWithValidTestData<T>();
		}

		protected IDocServicesParent ServicesParentWrapper;

		#endregion
	}
}
