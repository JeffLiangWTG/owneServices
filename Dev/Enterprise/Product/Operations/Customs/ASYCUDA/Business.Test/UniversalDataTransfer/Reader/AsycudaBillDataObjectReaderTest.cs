using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingAsycudaBillsData()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.SouthAfrica, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var zaAirLocalPort2 = help.GetAirLocalPort2(zaAirLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = zaAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = zaAirLocalPort2.RL_Code };
			var goodsValue = 25m;
			var goodsValueCurrency = new Currency() { Code = "AUD" };
			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			bill.GoodsValue = goodsValue;
			bill.GoodsValueCurrency = goodsValueCurrency;
			bill.OuterPacks = 10;
			bill.OuterPacksPackageType = new PackageType() { Code = "BOT", Description = "Bottles" };
			bill.AddInfoCollection.Add(new AddInfo() { Key = AsycudaBill.Schema.ABL_MarksAndNumbers, Value = "MARKS FOR EXPORTING" });
			bill.AddInfoCollection.Add(new AddInfo() { Key = AsycudaBill.Schema.ABL_UCRNumber, Value = "FakeUCR" });
			Factory.SaveForTesting();
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("billBO.ABL_RL_NKOrigin", zaAirLocalPort1.RL_Code, billBO.ABL_RL_NKOrigin);
			AssertEquals("billBO.ABL_RL_NKFinalDestination", zaAirLocalPort2.RL_Code, billBO.ABL_RL_NKFinalDestination);
			AssertEquals("billBO.ABL_GrossWeight", 300m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GoodsDescription", "Goods Desc", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_Volume", 3m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_CarrierReference", "Carrier Reference", billBO.ABL_CarrierReference);
			AssertEquals("billBO.ABL_BolType", "STD", billBO.ABL_BolType);  // change to correct matching
			AssertEquals("billBO.ABL_PrepaidCollect", "PRE", billBO.ABL_PrepaidCollect);
			AssertEquals("billBO.ABL_ManifestQty", 10, billBO.ABL_ManifestQty);
			AssertEquals("billBO.ABL_ManifestUQ", "BOT", billBO.ABL_ManifestUQ);
			AssertEquals("billBO.ABL_MarksAndNumbers", "MARKS FOR EXPORTING", billBO.ABL_MarksAndNumbers);
			AssertEquals("billBO.ABL_UCRNumber", "FakeUCR", billBO.ABL_UCRNumber);
			AssertEquals("billBO.ABL_GoodsValue", goodsValue, billBO.ABL_GoodsValue);
			AssertEquals("billBO.ABL_RX_NKGoodsValueCurrency", goodsValueCurrency.Code, billBO.ABL_RX_NKGoodsValueCurrency);
		}

		public void TestImportingAsycudaBillsCharges()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.SouthAfrica, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var zaAirLocalPort2 = help.GetAirLocalPort2(zaAirLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = zaAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = zaAirLocalPort2.RL_Code };
			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			bill.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[]
				{
					new UniversalCustoms.CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.ExWorks },
						Amount = 100m,
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia }
					},
					new UniversalCustoms.CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasFreight },
						Amount = 200m,
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Bahamas }
					},
					new UniversalCustoms.CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasInsurance },
						Amount = 300m,
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Cambodia }
					},
					new UniversalCustoms.CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OtherCharges },
						Amount = 400m,
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Denmark }
					},
					new UniversalCustoms.CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Discount },
						Amount = 500m,
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.EastTimor }
					},
					new UniversalCustoms.CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = Constants.CustomsChargeType.CustomsChargeCode },
						Amount = 600m,
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.FalklandIslands }
					},
				})
			};
			Factory.SaveForTesting();
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("billBO.ABL_FreightValue", 100m, billBO.ABL_FreightValue);
			AssertEquals("billBO.ABL_RX_NKFreightValueCurrency", Core.Constants.CurrencyCodes.Australia, billBO.ABL_RX_NKFreightValueCurrency);
			AssertEquals("billBO.ABL_TransportValue", 200m, billBO.ABL_TransportValue);
			AssertEquals("billBO.ABL_RX_NKTransportValueCurrency", Core.Constants.CurrencyCodes.Bahamas, billBO.ABL_RX_NKTransportValueCurrency);
			AssertEquals("billBO.ABL_InsuranceValue", 300m, billBO.ABL_InsuranceValue);
			AssertEquals("billBO.ABL_RX_NKInsuranceValueCurrency", Core.Constants.CurrencyCodes.Cambodia, billBO.ABL_RX_NKInsuranceValueCurrency);
			AssertEquals("billBO.OtherChargesValue", 400m, billBO.OtherChargesValue);
			AssertEquals("billBO.OtherChargesValueCurrency", Core.Constants.CurrencyCodes.Denmark, billBO.OtherChargesValueCurrency);
			AssertEquals("billBO.DiscountValue", 500m, billBO.DiscountValue);
			AssertEquals("billBO.DiscountValueCurrency", Core.Constants.CurrencyCodes.EastTimor, billBO.DiscountValueCurrency);
			AssertEquals("billBO.ABL_CustomsValue", 600m, billBO.ABL_CustomsValue);
			AssertEquals("billBO.ABL_RX_NKCustomsValueCurrency", Core.Constants.CurrencyCodes.FalklandIslands, billBO.ABL_RX_NKCustomsValueCurrency);
		}

		public void TestUpdateAsycudaBillsData()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.SouthAfrica, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var zaAirLocalPort2 = help.GetAirLocalPort2(zaAirLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = zaAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = zaAirLocalPort2.RL_Code };
			var oldBill = header.Bills.AddNew();
			oldBill.ABL_BillNumber = "BIL00001";
			var billPK = oldBill.PK;
			Factory.SaveForTesting();

			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			Factory.SaveForTesting();
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("bill is updated.", billPK, billBO.PK);
			AssertEquals("billBO.ABL_RL_NKOrigin", zaAirLocalPort1.RL_Code, billBO.ABL_RL_NKOrigin);
			AssertEquals("billBO.ABL_RL_NKFinalDestination", zaAirLocalPort2.RL_Code, billBO.ABL_RL_NKFinalDestination);
			AssertEquals("billBO.ABL_GrossWeight", 300m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GoodsDescription", "Goods Desc", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_Volume", 3m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_CarrierReference", "Carrier Reference", billBO.ABL_CarrierReference);
			AssertEquals("billBO.ABL_BolType", "STD", billBO.ABL_BolType); // change to correct matching
			AssertEquals("billBO.ABL_PrepaidCollect", "PRE", billBO.ABL_PrepaidCollect);
		}

		public void TestDoNotUpdateBOLBill()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.SouthAfrica, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var zaAirLocalPort2 = help.GetAirLocalPort2(zaAirLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = zaAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = zaAirLocalPort2.RL_Code };
			header.AMA_MasterBill = "BIL00001";
			AssertEquals(0, header.Bills.Count);
			Factory.SaveForTesting();
			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			Factory.SaveForTesting();
			var billReaderBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billReaderBO);
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var reloadedHeader = factory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("BIL00001", reloadedHeader.AMA_MasterBill);
			AssertEquals(1, reloadedHeader.Bills.Count);
			var billBO = reloadedHeader.Bills[0];
			AssertEquals("billBO.ABL_BillNumber.", "BIL00001", billBO.ABL_BillNumber);
			AssertEquals("billBO.ABL_RL_NKOrigin", zaAirLocalPort1.RL_Code, billBO.ABL_RL_NKOrigin);
			AssertEquals("billBO.ABL_RL_NKFinalDestination", zaAirLocalPort2.RL_Code, billBO.ABL_RL_NKFinalDestination);
			AssertEquals("billBO.ABL_GrossWeight", 300m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GoodsDescription", "Goods Desc", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_Volume", 3m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_CarrierReference", "Carrier Reference", billBO.ABL_CarrierReference);
			AssertEquals("billBO.ABL_BolType", "STD", billBO.ABL_BolType);
			AssertEquals("billBO.ABL_PrepaidCollect", "PRE", billBO.ABL_PrepaidCollect);
		}

		public void TestImportOrganizations()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var fjAirLocalPort1 = help.GetAirLocalPort1(Core.Constants.CountryCodes.Eritrea);
			var fjAirLocalPort2 = help.GetAirLocalPort2(fjAirLocalPort1.PK);
			var portOfLoading = new UNLOCO() { Code = fjAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = fjAirLocalPort2.RL_Code };
			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "CCC";
			orgHeader.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "SYD";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "AU";
			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
				CompanyName = "Consignee",
				Address1 = "Consignee Address1",
				City = "SYD",
				State = "NSW",
				Postcode = "1000",
				Phone = "12345678",
				Country = new Country() { Code = "AU" }
			};

			var shipperAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				CompanyName = "Shipper",
				Address1 = "Shipper Address1",
				Address2 = "Shipper Address2",
				City = "SSS",
				State = "NSW",
				Postcode = "2000",
				Phone = "222222",
				Country = new Country() { Code = "AU" }
			};

			var notifyPartyAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.NotifyParty),
				CompanyName = "NotifyParty",
				Address1 = "NotifyParty Address1",
				City = "NNN",
				State = "NSW",
				Postcode = "3000",
				Phone = "33333333",
				Country = new Country() { Code = "AU" }
			};

			bill.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			bill.OrganizationAddressCollection.AddSafe(consigneeAddress);
			bill.OrganizationAddressCollection.AddSafe(shipperAddress);
			bill.OrganizationAddressCollection.AddSafe(notifyPartyAddress);
			Factory.SaveForTesting();
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("billBO.ABL_OA_Consignee", orgAddress.PK, billBO.ABL_OA_Consignee);
			AssertEquals("billBO.ABL_ConsigneeName", orgAddress.Header.OH_FullName, billBO.ABL_ConsigneeName);
			AssertEquals("billBO.ABL_ConsigneeStreet1", orgAddress.OA_Address1, billBO.ABL_ConsigneeStreet1);
			AssertEquals("billBO.ABL_ConsigneeStreet2", orgAddress.OA_Address2, billBO.ABL_ConsigneeStreet2);
			AssertEquals("billBO.ABL_ConsigneeCity", orgAddress.OA_City, billBO.ABL_ConsigneeCity);
			AssertEquals("billBO.ABL_ConsigneeState", orgAddress.OA_State, billBO.ABL_ConsigneeState);
			AssertEquals("billBO.ABL_ConsigneePostcode", orgAddress.OA_PostCode, billBO.ABL_ConsigneePostcode);
			AssertEquals("billBO.ABL_ConsigneePhone", orgAddress.PhoneNumber.FormattedForBinding, billBO.ABL_ConsigneePhone);
			AssertEquals("billBO.ABL_RN_NKConsigneeCountry", orgAddress.OA_RN_NKCountryCode, billBO.ABL_RN_NKConsigneeCountry);

			AssertEquals("billBO.ABL_OA_Shipper", ZGuid.Empty, billBO.ABL_OA_Shipper);
			AssertEquals("billBO.ABL_ShipperName", "Shipper", billBO.ABL_ShipperName);
			AssertEquals("billBO.ABL_ShipperStreet1", "Shipper Address1", billBO.ABL_ShipperStreet1);
			AssertEquals("billBO.ABL_ShipperStreet2", "Shipper Address2", billBO.ABL_ShipperStreet2);
			AssertEquals("billBO.ABL_ShipperCity", "SSS", billBO.ABL_ShipperCity);
			AssertEquals("billBO.ABL_ShipperState", "NSW", billBO.ABL_ShipperState);
			AssertEquals("billBO.ABL_ShipperPostcode", "2000", billBO.ABL_ShipperPostcode);
			AssertEquals("billBO.ABL_RN_NKShipperCountry", "AU", billBO.ABL_RN_NKShipperCountry);
			AssertEquals("billBO.ABL_ShipperPhone", "222222", billBO.ABL_ShipperPhone);

			AssertEquals("billBO.ABL_OA_Shipper", ZGuid.Empty, billBO.ABL_OA_NotifyParty);
			AssertEquals("billBO.ABL_ShipperName", "NotifyParty", billBO.ABL_NotifyPartyName);
			AssertEquals("billBO.ABL_ShipperStreet1", "NotifyParty Address1", billBO.ABL_NotifyPartyStreet1);
			AssertEquals("billBO.ABL_ShipperStreet2", "", billBO.ABL_NotifyPartyStreet2);
			AssertEquals("billBO.ABL_ShipperCity", "NNN", billBO.ABL_NotifyPartyCity);
			AssertEquals("billBO.ABL_ShipperState", "NSW", billBO.ABL_NotifyPartyState);
			AssertEquals("billBO.ABL_ShipperPostcode", "3000", billBO.ABL_NotifyPartyPostcode);
			AssertEquals("billBO.ABL_ShipperPostcode", "33333333", billBO.ABL_NotifyPartyPhone);
			AssertEquals("billBO.ABL_RN_NKShipperCountry", "AU", billBO.ABL_RN_NKNotifyPartyCountry);
		}

		public void TestCustomsJobNumberImported()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			var portOfLoading = new UNLOCO() { Code = "AUSYD" };
			var portOfDischarge = new UNLOCO() { Code = "SGSIN" };
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 10m, "Goods Descrption", 1m, "Carrier Reference", "STD", "PRE");
			var dataSourceMock = new Mock<IDataSourceDataObject>();
			dataSourceMock.Setup(m => m.Key).Returns("B0000996");
			dataSourceMock.Setup(m => m.Type).Returns(nameof(DataContextType.CustomsDeclaration));
			var dataContextMock = new Mock<IDataContextDataObject>();
			dataContextMock.Setup(m => m.DataSourceCollection).Returns(new List<IDataSourceDataObject> { dataSourceMock.Object });
			bill.DataContext = dataContextMock.Object;
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("B0000996", billBO.CustomsJobNumber);
			dataSourceMock.VerifyAll();
		}

		public void TestMatchingReferenceImported()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			var portOfLoading = new UNLOCO() { Code = "AUSYD" };
			var portOfDischarge = new UNLOCO() { Code = "SGSIN" };
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 10m, "Goods Descrption", 1m, "Carrier Reference", "STD", "PRE");
			bill.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New(AddInfoConstants.Bill.MatchingReference, "TESTMATCHREF") });

			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("TESTMATCHREF", billBO.MatchingReference);
		}

		public void TestMatchingReferenceUpdatesBillNumber()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN00001";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "OLDBIL00002";
			bill.MatchingReference = "TESTMATCHREF";

			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var portOfLoading = new UNLOCO() { Code = "AUSYD" };
			var portOfDischarge = new UNLOCO() { Code = "SGSIN" };
			var billShipment = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 10m, "Goods Descrption", 1m, "Carrier Reference", "STD", "PRE");
			billShipment.SetAddInfoCollection(() => new List<AddInfo>() { AddInfo.New(AddInfoConstants.Bill.MatchingReference, "TESTMATCHREF") });

			var billBO = new AsycudaBillDataObjectReader(billShipment, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("TESTMATCHREF", billBO.MatchingReference);
			AssertEquals("BIL00001", billBO.ABL_BillNumber);
		}

		public void TestNotifyPartyAddressOverride()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = help.SetupBill("BIL00001",
				new UNLOCO() { Code = "AUSYD" },
				new UNLOCO() { Code = "SGSIN" },
				300m,
				"Goods Desc",
				3m,
				"Carrier Reference",
				"STD",
				"PRE");

			var notifyPartyAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.NotifyParty),
				CompanyName = "NOTIFYPARTY",
				Address1 = "NOTIFY ADDRESS1",
				Address2 = "NOTIFY ADDRESS2",
				City = "MELBOURNE",
				State = "VIC",
				Postcode = "3000",
				Phone = "33333333",
				Country = new Country() { Code = Core.Constants.CountryCodes.Australia },
				AddressOverride = true
			};

			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			bill.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			bill.OrganizationAddressCollection.AddSafe(notifyPartyAddress);
			Factory.SaveForTesting();
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals(ZGuid.Empty, billBO.ABL_OA_NotifyParty);
			AssertEquals("NOTIFYPARTY", billBO.ABL_NotifyPartyName);
			AssertEquals("NOTIFY ADDRESS1", billBO.ABL_NotifyPartyStreet1);
			AssertEquals("NOTIFY ADDRESS2", billBO.ABL_NotifyPartyStreet2);
			AssertEquals("MELBOURNE", billBO.ABL_NotifyPartyCity);
			AssertEquals("VIC", billBO.ABL_NotifyPartyState);
			AssertEquals("3000", billBO.ABL_NotifyPartyPostcode);
			AssertEquals("33333333", billBO.ABL_NotifyPartyPhone);
			AssertEquals(Core.Constants.CountryCodes.Australia, billBO.ABL_RN_NKNotifyPartyCountry);
		}

		public void TestConsigneeAddressOverride()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = help.SetupBill("BIL00001",
				new UNLOCO() { Code = "AUSYD" },
				new UNLOCO() { Code = "SGSIN" },
				300m,
				"Goods Desc",
				3m,
				"Carrier Reference",
				"STD",
				"PRE");

			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress),
				CompanyName = "CONSIGNEE",
				Address1 = "CONSIGNEE ADDRESS1",
				Address2 = "CONSIGNEE ADDRESS2",
				City = "SYDNEY",
				State = "NSW",
				Postcode = "1000",
				Phone = "12345678",
				Country = new Country() { Code = Core.Constants.CountryCodes.Australia },
				AddressOverride = true
			};

			bill.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			bill.OrganizationAddressCollection.AddSafe(consigneeAddress);
			Factory.SaveForTesting();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals(ZGuid.Empty, billBO.ABL_OA_Consignee);
			AssertEquals("CONSIGNEE", billBO.ABL_ConsigneeName);
			AssertEquals("CONSIGNEE ADDRESS1", billBO.ABL_ConsigneeStreet1);
			AssertEquals("CONSIGNEE ADDRESS2", billBO.ABL_ConsigneeStreet2);
			AssertEquals("SYDNEY", billBO.ABL_ConsigneeCity);
			AssertEquals("NSW", billBO.ABL_ConsigneeState);
			AssertEquals("1000", billBO.ABL_ConsigneePostcode);
			AssertEquals("12345678", billBO.ABL_ConsigneePhone);
			AssertEquals(Core.Constants.CountryCodes.Australia, billBO.ABL_RN_NKConsigneeCountry);
		}

		public void TestShipperAddressOverride()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = help.SetupBill("BIL00001",
				new UNLOCO() { Code = "AUSYD" },
				new UNLOCO() { Code = "SGSIN" },
				300m,
				"Goods Desc",
				3m,
				"Carrier Reference",
				"STD",
				"PRE");

			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				CompanyName = "SHIPPER",
				Address1 = "SHIPPER ADDRESS1",
				Address2 = "SHIPPER ADDRESS2",
				City = "SINGAPORE",
				State = "RAFFLES",
				Postcode = "408600",
				Phone = "12345678",
				Country = new Country() { Code = Core.Constants.CountryCodes.Singapore },
				AddressOverride = true
			};

			bill.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			bill.OrganizationAddressCollection.AddSafe(consigneeAddress);
			Factory.SaveForTesting();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals(ZGuid.Empty, billBO.ABL_OA_Shipper);
			AssertEquals("SHIPPER", billBO.ABL_ShipperName);
			AssertEquals("SHIPPER ADDRESS1", billBO.ABL_ShipperStreet1);
			AssertEquals("SHIPPER ADDRESS2", billBO.ABL_ShipperStreet2);
			AssertEquals("SINGAPORE", billBO.ABL_ShipperCity);
			AssertEquals("RAFFLES", billBO.ABL_ShipperState);
			AssertEquals("408600", billBO.ABL_ShipperPostcode);
			AssertEquals(Core.Constants.CountryCodes.Singapore, billBO.ABL_RN_NKShipperCountry);
			AssertEquals("12345678", billBO.ABL_ShipperPhone);
		}

		public void TestForwarderAddressImported()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = help.SetupBill("BIL00001",
				new UNLOCO() { Code = "AUSYD" },
				new UNLOCO() { Code = "SGSIN" },
				300m,
				"Goods Desc",
				3m,
				"Carrier Reference",
				"STD",
				"PRE");

			var forwarderAddress = new OrganizationAddress()
			{
				AddressType = nameof(DocAddressType.Forwarder),
				CompanyName = "FORWARDER",
				Address1 = "FORWARDER ADDRESS1",
				Address2 = "FORWARDER ADDRESS2",
				City = "SINGAPORE",
				State = "RAFFLES",
				Postcode = "408600",
				Phone = "12345678",
				AddressShortCode = "FORWARD",
				Country = new Country() { Code = Core.Constants.CountryCodes.Singapore },
				AddressOverride = true
			};
			var loggerForSetup = new TestErrorLogger();
			var addressBO = new OrganisationDataObjectReader(forwarderAddress, logger, Factory).GetMatchedOrNewForTesting();
			bill.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			bill.OrganizationAddressCollection.AddSafe(forwarderAddress);

			AssertNotNull("Precondition: addressBOForSetup", addressBO);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBO.IsInDatabase);

			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var billBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertNotNullOrEmpty(billBO.ABL_OA_Forwarder.ToString());
			AssertEquals(forwarderAddress.CompanyName, addressBO.CompanyName);
			AssertEquals(forwarderAddress.Address1, addressBO.OA_Address1);
			AssertEquals(forwarderAddress.Address2, addressBO.OA_Address2);
			AssertEquals(forwarderAddress.City, addressBO.OA_City);
			AssertEquals(forwarderAddress.State, addressBO.OA_State);
			AssertEquals(forwarderAddress.Postcode, addressBO.OA_PostCode);
			AssertEquals(Core.Constants.CountryCodes.Singapore, addressBO.OA_RN_NKCountryCode);
			AssertEquals(forwarderAddress.Phone, addressBO.OA_Phone);
		}

		public void TestImportingCustomizedFields()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.SouthAfrica, Factory.BOFactory);
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var zaAirLocalPort2 = help.GetAirLocalPort2(zaAirLocalPort1.PK);
			var portOfLoading = new UNLOCO { Code = zaAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO { Code = zaAirLocalPort2.RL_Code };
			header.AMA_MasterBill = "BIL00001";
			AssertEquals(0, header.Bills.Count);

			Factory.SaveForTesting();

			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			bill.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				CustomizedField.New("CustomField1", (ZString)"Hello World"),
				CustomizedField.New("CustomField2", ZDateTime.BrettsBirthday),
				CustomizedField.New("CustomField3", (ZDecimal)200.5),
				CustomizedField.New("CustomField4", (ZBool)true)
			});

			var billReaderBO = new AsycudaBillDataObjectReader(bill, Logger, Factory, header, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(billReaderBO);

			AssertEquals("Count of customs fields", 4, billReaderBO.GetUserDefinedValues().Count());
			AssertEquals("CustomField1", "Hello World", billReaderBO.GetUserDefinedValue<ZString>("CustomField1"));
			AssertEquals("CustomField2", ZDateTime.BrettsBirthday, billReaderBO.GetUserDefinedValue<ZDateTime>("CustomField2"));
			AssertEquals("CustomField3", (ZDecimal)200.5, billReaderBO.GetUserDefinedValue<ZDecimal>("CustomField3"));
			AssertEquals("CustomField4", true, billReaderBO.GetUserDefinedValue<ZBool>("CustomField4"));
		}
	}
}
