using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class StarTrackQRCodeTextProviderTest : TestCaseWithFactory
	{
		#region TestStarTrackQRCodeText

		public void TestStarTrackQRCodeText()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var expectedQRCodeText = $"SYDNEY                        2000            123                                    1   0    0    {ZDateTime.Today.ToString("yyyyMMdd")}CNE SYDNEY                                                                      BOX    #1                                                                                            NN                                            ";
			AssertEquals(expectedQRCodeText, provider.StarTrackQRCodeText());
		}

		#endregion

		#region Test Fields

		#region 1. Receiver Suburb / Consignee City	30

		public void TestReceiverSuburb_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_City = value, "Sydney", provider => provider.ReceiverSuburb, "ReceiverSuburb", "SYDNEY".PadRight(30), 30);
		}

		public void TestReceiverSuburb_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_City = value, "Sydney-XYZ01234567890123456789(TooLong)", provider => provider.ReceiverSuburb, "ReceiverSuburb", "SYDNEY-XYZ01234567890123456789", 30);
		}

		public void TestReceiverSuburb_Empty()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_City = value, "", provider => provider.ReceiverSuburb, "ReceiverSuburb", ZString.Empty.PadRight(30), 30);
		}

		#endregion

		#region 2. Receiver Postcode / Consignee Postcode	4

		public void TestReceiverPostCode_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_PostCode = value, "2000", provider => provider.ReceiverPostCode, "ReceiverPostCode", "2000", 4);
		}

		public void TestReceiverPostCode_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_PostCode = value, "0123456789", provider => provider.ReceiverPostCode, "ReceiverPostCode", "0123", 4);
		}

		public void TestReceiverPostCode_Empty()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_PostCode = value, "", provider => provider.ReceiverPostCode, "ReceiverPostCode", "    ", 4);
		}

		#endregion

		#region 3. Consignment Number / Transport Reference	12

		public void TestConsignmentNumber_Normal()
		{
			TestWhsOrderChange((order, value) => order.WD_TransportReference = value, "XYZ", provider => provider.ConsignmentNumber, "ConsignmentNumber", "XYZ".PadRight(12), 12);
		}

		public void TestConsignmentNumber_TruncateWhenToolong()
		{
			TestWhsOrderChange((order, value) => order.WD_TransportReference = value, "123456789012345", provider => provider.ConsignmentNumber, "ConsignmentNumber", "123456789012".PadRight(12), 12);
		}

		public void TestConsignmentNumber_Empty()
		{
			TestWhsOrderChange((order, value) => order.WD_TransportReference = value, "", provider => provider.ConsignmentNumber, "ConsignmentNumber", ZString.Empty.PadRight(12), 12);
		}

		#endregion

		#region 4. Freight Item Number / Package ID	20

		public void TestFreightItemNumber_Normal()
		{
			TestPackageHeaderChange((packageHeader, value) => packageHeader.KPH_PackageID = value, "P0001", provider => provider.FreightItemNumber, "FreightItemNumber", "P0001".PadRight(20), 20);
		}

		public void TestFreightItemNumber_TruncateWhenToolong()
		{
			TestPackageHeaderChange((packageHeader, value) => packageHeader.KPH_PackageID = value, "P01234567890123456789012345", provider => provider.FreightItemNumber, "FreightItemNumber", "P0123456789012345678", 20);
		}

		public void TestFreightItemNumber_Short()   // cannot be empty
		{
			TestPackageHeaderChange((packageHeader, value) => packageHeader.KPH_PackageID = value, "P1", provider => provider.FreightItemNumber, "FreightItemNumber", "P1".PadRight(20), 20);
		}

		#endregion

		#region 5. Product Code / Carrier Service Code	3

		public void TestProduct_Normal()
		{
			TestWhsOrderChange((order, value) => order.WD_PL_NKCarrierServiceLevel = value, "STD", provider => provider.ProductCode, "ProductCode", "STD", 3);
		}

		public void TestProduct_Short()     // cannot be too long
		{
			TestWhsOrderChange((order, value) => order.WD_PL_NKCarrierServiceLevel = value, "BS", provider => provider.ProductCode, "ProductCode", "BS ", 3);
		}

		public void TestProduct_Empty()
		{
			TestWhsOrderChange((order, value) => order.WD_PL_NKCarrierServiceLevel = value, "", provider => provider.ProductCode, "ProductCode", ZString.Empty.PadRight(3), 3);
		}

		#endregion

		#region 6. Payer Account 8

		public void TestPayerAccount()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var expectedText = ZString.Empty.PadRight(8);
			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Payer Account should be 8 chars long.", 8, provider.PayerAccount.Length);
			AssertEquals("Payer Account", expectedText, provider.PayerAccount);
		}

		#endregion

		#region 7. Sender Account / Account Number	8

		public void TestSenderAccount()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();
			var clientPK = data.Client.PK;
			data.Order.TransportCoPK = clientPK;

			var orgCarrierAccount = Factory.New<OrgCarrierAccount>();
			orgCarrierAccount.OAN_OH_Carrier = clientPK;
			orgCarrierAccount.OAN_AccountNumber = "1234567";

			var accountAssociation = Factory.New<OrgWhsClientAccountAssociation>();
			accountAssociation.OWC_WW_Warehouse = data.Warehouse.PK;
			accountAssociation.OWC_OH_Client = clientPK;
			accountAssociation.OWC_OAN_CarrierAccount = orgCarrierAccount.PK;
			Factory.Save();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var expectedText = "1234567 ";
			AssertEquals("Sender Account should be 8 chars long.", 8, provider.SenderAccount.Length);
			AssertEquals("Sender Account", expectedText, provider.SenderAccount);

			orgCarrierAccount.OAN_AccountNumber = "1234567890";
			Factory.Save();
			expectedText = "12345678";
			var provider2 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Sender Account should be 8 chars long.", 8, provider2.SenderAccount.Length);
			AssertEquals("Sender Account", expectedText, provider2.SenderAccount);

			orgCarrierAccount.OAN_AccountNumber = "A";
			Factory.Save();
			expectedText = "A".PadRight(8);
			var provider3 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Sender Account should be 8 chars long.", 8, provider3.SenderAccount.Length);
			AssertEquals("Sender Account", expectedText, provider3.SenderAccount);
		}

		#endregion

		#region 8. Consignment Quantity / Count of Package IDs 4

		public void TestConsignmentQuantity()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Quantity should be 4 chars long.", 4, provider.ConsignmentQuantity.Length);
			AssertEquals("Consignment Quanity", "1   ", provider.ConsignmentQuantity);
		}

		#endregion

		#region 9. Consignment Weight / Total Consignment weight in KGs 5

		public void TestConsignmentWeight()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			data.Package.KP_Weight = 30m;
			data.Package.KP_WeightUQ = Core.Constants.Weight.Kilograms;
			Factory.Save();
			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Weight should be 5 chars long.", 5, provider.ConsignmentWeight.Length);
			AssertEquals("Consignment Weight", "30   ", provider.ConsignmentWeight);

			data.Package.KP_Weight = 3490m;
			data.Package.KP_WeightUQ = Core.Constants.Weight.Grams;
			Factory.Save();
			var provider2 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Weight should be 5 chars long.", 5, provider2.ConsignmentWeight.Length);
			AssertEquals("Consignment Weight", "3    ", provider2.ConsignmentWeight);

			data.Package.KP_Weight = 3590m;
			data.Package.KP_WeightUQ = Core.Constants.Weight.Grams;
			Factory.Save();
			var provider3 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Weight should be 5 chars long.", 5, provider3.ConsignmentWeight.Length);
			AssertEquals("Consignment Weight", "4    ", provider3.ConsignmentWeight);

			data.Package.KP_Weight = 0m;
			data.Package.KP_WeightUQ = "";
			Factory.Save();
			var provider4 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Weight should be 5 chars long.", 5, provider4.ConsignmentWeight.Length);
			AssertEquals("Consignment Weight", "     ", provider4.ConsignmentWeight);
		}

		#endregion

		#region 10. Consignment Cube / Total Consignment volume * 1000  5

		public void TestConsignmentCube()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			data.Package.KP_Volume = 0.1m;
			data.Package.KP_VolumeUQ = Core.Constants.Volume.CubicMetres;
			Factory.Save();
			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Cube should be 5 chars long.", 5, provider.ConsignmentCube.Length);
			AssertEquals("Consignment Cube", "100  ", provider.ConsignmentCube);

			data.Package.KP_Volume = 100m;
			data.Package.KP_VolumeUQ = Core.Constants.Volume.CubicMetres;
			Factory.Save();
			var provider2 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Cube should be 5 chars long.", 5, provider2.ConsignmentCube.Length);
			AssertEquals("Consignment Cube", "*****", provider2.ConsignmentCube);

			data.Package.KP_Volume = 0m;
			data.Package.KP_VolumeUQ = "";
			Factory.Save();
			var provider3 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Consignment Cube should be 5 chars long.", 5, provider3.ConsignmentCube.Length);
			AssertEquals("Consignment Cube", "     ", provider3.ConsignmentCube);
		}

		#endregion

		#region 11. Dispatch Date / Label print date time YYYYMMDD  8

		[TestDate(2018, 7, 6)]
		public void TestDispatchDate()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("DispatchDate should be 8 chars long.", 8, provider.DispatchDate.Length);
			AssertEquals("DispatchDate", ZDateTime.Now.ToString("yyyyMMdd"), provider.DispatchDate);
		}

		#endregion

		#region 12. Receiver Name 1 / Consignee Full Name 40

		public void TestReceiverName1_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.OH_FullName = value, "TEST COMPANY", provider => provider.ReceiverName1, "ReceiverName1", "TEST COMPANY".PadRight(40), 40);
		}

		public void TestReceiverName1_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.OH_FullName = value, "T123456789U123456789V123456789W123456789(Toolong)", provider => provider.ReceiverName1, "ReceiverName1", "T123456789U123456789V123456789W123456789", 40);
		}

		public void TestReceiverName1_Empty()
		{
			TestOrgHeaderChange((consignee, value) => consignee.OH_FullName = value, "", provider => provider.ReceiverName1, "ReceiverName1", ZString.Empty.PadRight(40), 40);
		}

		public void TestReceiverName1_ConsigneeAddressOverriden()
		{
			var data = SetupTestData();

			var consignee = data.Consignee;
			consignee.OH_FullName = "TEST COMPANY";
			data.Order.ConsigneeDocAddress.E2_AddressOverride = true;
			data.Order.ConsigneeDocAddress.CompanyName = "TEST COMPANY OVERRIDE";

			var n = data.Order.ConsigneeDocAddress.Organisation.OH_FullName;
			AssertNotEquals(consignee.OH_FullName, data.Order.ConsigneeDocAddress.Organisation.OH_FullName);
			AssertProviderFieldValue(data, provider => provider.ReceiverName1, "ReceiverName1", "TEST COMPANY OVERRIDE".PadRight(40), 40);
		}

		#endregion

		#region 13. Receiver Name 2 / Consignee Additional Address Info 40

		public void TestReceiverName2_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.UnrestrictedAdditionalAddressInformation = value, "Test Company", provider => provider.ReceiverName2, "ReceiverName2", "Test Company".PadRight(40), 40);
		}

		public void TestReceiverName2_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.UnrestrictedAdditionalAddressInformation = value, "T123456789U123456789V123456789W123456789(Toolong)", provider => provider.ReceiverName2, "ReceiverName2", "T123456789U123456789V123456789W123456789", 40);
		}

		public void TestReceiverName2_Empty()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.UnrestrictedAdditionalAddressInformation = value, "", provider => provider.ReceiverName2, "ReceiverName2", ZString.Empty.PadRight(40), 40);
		}

		#endregion

		#region 14. Unit Type / PackageType 3

		public void TestUnitType_Normal()
		{
			TestPackageChange((package, value) => package.KP_F3_NKPackType = value, "BOX", provider => provider.UnitType, "UnitType", "BOX", 3);
		}

		public void TestUnitType_Short()    // cannot be empty or long
		{
			TestPackageChange((package, value) => package.KP_F3_NKPackType = value, "P", provider => provider.UnitType, "UnitType", "P  ", 3);
		}

		#endregion

		#region 15. Destination Depot / Transport Zone Name 4

		public void TestDestinationDepot()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var transportHelper = new TransportBookingTestHelper(Factory);
			var transportCo = transportHelper.CreateOrganisation("ABCDEFG");
			data.Order.TransportCoPK = transportCo.PK;

			var prov = transportHelper.CreateZoneRateProvider("AU", transportCo);
			var zone = transportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "2500");

			Factory.Save();
			// Need to do this to trigger order.OnConsigneeDocAddressChanged(), which sets the transport zone
			var address = data.Order.ConsigneeAddress;
			data.Order.ConsigneeAddressPK = Guid.Empty;
			data.Order.ConsigneeAddressPK = address.PK;

			AssertEquals("V0", data.FreightWrapper.TransportZone);
			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Destination Depot should be 4 chars long.", 4, provider.DestinationDepot.Length);
			AssertEquals("Destination Depot", "V0  ", provider.DestinationDepot);
		}

		#endregion

		#region 16. Receiver Address L1 / Consignee Address Line 1  40

		public void TestReceiverAddressLine1_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Address1 = value, "Address Line 1", provider => provider.ReceiverAddressLine1, "ReceiverAddressLine1", "ADDRESS LINE 1".PadRight(40), 40);
		}

		public void TestReceiverAddressLine1_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Address1 = value, "Address Line 1-6789012345678901234567890-Toolong", provider => provider.ReceiverAddressLine1, "ReceiverAddressLine1", "ADDRESS LINE 1-6789012345678901234567890", 40);
		}

		public void TestReceiverAddressLine1_Short()    // cannot be empty
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Address1 = value, "A", provider => provider.ReceiverAddressLine1, "ReceiverAddressLine1", "A".PadRight(40), 40);
		}

		#endregion

		#region 17. Receiver Address L2 / Consignee Address Line 2  40

		public void TestReceiverAddressLine2_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Address2 = value, "Address Line 2", provider => provider.ReceiverAddressLine2, "ReceiverAddressLine2", "ADDRESS LINE 2".PadRight(40), 40);
		}

		public void TestReceiverAddressLine2_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Address2 = value, "Address Line 2-6789012345678901234567890-Toolong", provider => provider.ReceiverAddressLine2, "ReceiverAddressLine2", "ADDRESS LINE 2-6789012345678901234567890", 40);
		}

		public void TestReceiverAddressLine2_Empty()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Address2 = value, "", provider => provider.ReceiverAddressLine2, "ReceiverAddressLine2", ZString.Empty.PadRight(40), 40);
		}

		#endregion

		#region 18. Receiver Phone Number / Consignee phone  14

		public void TestReceiverPhoneNumber_Normal()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Phone = value, "0123456789", provider => provider.ReceiverPhoneNumber, "ReceiverPhoneNumber", "0123456789".PadRight(14), 14);
		}

		public void TestReceiverPhoneNumber_TruncateWhenToolong()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Phone = value, "01234567891234-ABC", provider => provider.ReceiverPhoneNumber, "ReceiverPhoneNumber", "01234567891234", 14);
		}

		public void TestReceiverPhoneNumber_Empty()
		{
			TestOrgHeaderChange((consignee, value) => consignee.MainAddress.OA_Phone = value, "", provider => provider.ReceiverPhoneNumber, "ReceiverPhoneNumber", ZString.Empty.PadRight(14), 14);
		}

		#endregion

		#region 19. Dangerous Goods Indicator: if "DG" then "Y" else "N"  1

		public void TestDangerousGoodsIndicator()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Dangerous Goods Indicator should be 1 char long.", 1, provider.DangerousGoodsIndicator.Length);
			AssertEquals("Dangerous Goods Indicator", "N", provider.DangerousGoodsIndicator);

			data.Package.UNDGs.AddNew();
			var provider2 = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Dangerous Goods Indicator should be 1 char long.", 1, provider2.DangerousGoodsIndicator.Length);
			AssertEquals("Dangerous Goods Indicator", "Y", provider2.DangerousGoodsIndicator);
		}

		#endregion

		#region 20. Movement Type Indicator: "N" 1

		public void TestMovementTypeIndicator()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			AssertEquals("Not RANumber", "N", provider.MovementTypeIndicator);
		}

		#endregion

		#region 21. Not Before Date: N/A  12

		public void TestNotBeforeDate()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var expectedText = ZString.Empty.PadRight(12);
			AssertEquals("Not Before Date should be 12 chars long.", 12, provider.NotBeforeDate.Length);
			AssertEquals("Not Before Date", expectedText, provider.NotBeforeDate);
		}

		#endregion

		#region 22. Not After Date: N/A  12

		public void TestNotAfterDate()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var expectedText = ZString.Empty.PadRight(12);
			AssertEquals("Not After Date should be 12 chars long.", 12, provider.NotAfterDate.Length);
			AssertEquals("Not After Date", expectedText, provider.NotAfterDate);
		}

		#endregion

		#region 23. ATL Number: if authority to leave "ATL" else ""  10

		public void TestATLNumber()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var expectedText = ZString.Empty.PadRight(10);
			AssertEquals("ATL Number should be 10 chars long.", 10, provider.ATLNumber.Length);
			AssertEquals("ATL Number", expectedText, provider.ATLNumber);
		}

		#endregion

		#region 24. RA Number:	N/A  10

		public void TestRANumber()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();

			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var expectedText = ZString.Empty.PadRight(10);
			AssertEquals("Not RANumber should be 10 chars long.", 10, provider.RANumber.Length);
			AssertEquals("Not RANumber", expectedText, provider.RANumber);
		}

		#endregion

		#endregion

		#region Implementation

		void TestOrgHeaderChange(Action<OrgHeader, string> setValue, string targetValue, Func<StarTrackQRCodeTextProvider, ZString> getFieldValue, string fieldName, string expectedValue, int expectedLength)
		{
			var data = SetupTestData();

			var consignee = data.Consignee;
			setValue(consignee, targetValue);

			AssertProviderFieldValue(data, getFieldValue, fieldName, expectedValue, expectedLength);
		}

		void TestWhsOrderChange(Action<WhsOrder, string> setValue, string targetValue, Func<StarTrackQRCodeTextProvider, ZString> getFieldValue, string fieldName, string expectedValue, int expectedLength)
		{
			var data = SetupTestData();

			var order = data.Order;
			setValue(order, targetValue);

			AssertProviderFieldValue(data, getFieldValue, fieldName, expectedValue, expectedLength);
		}

		void TestPackageHeaderChange(Action<PkgPackageHeader, string> setValue, string targetValue, Func<StarTrackQRCodeTextProvider, ZString> getFieldValue, string fieldName, string expectedValue, int expectedLength)
		{
			var data = SetupTestData();

			var packageHeader = data.PackageHeader;
			setValue(packageHeader, targetValue);

			AssertProviderFieldValue(data, getFieldValue, fieldName, expectedValue, expectedLength);
		}

		void TestPackageChange(Action<PkgPackage, string> setValue, string targetValue, Func<StarTrackQRCodeTextProvider, ZString> getFieldValue, string fieldName, string expectedValue, int expectedLength)
		{
			var data = SetupTestData();

			var package = data.Package;
			setValue(package, targetValue);

			AssertProviderFieldValue(data, getFieldValue, fieldName, expectedValue, expectedLength);
		}

		StarTrackerTestData SetupTestData()
		{
			var data = new StarTrackerTestData(Factory);
			data.SetupTestData();
			return data;
		}

		void AssertProviderFieldValue(StarTrackerTestData data, Func<StarTrackQRCodeTextProvider, ZString> getFieldValue, string fieldName, string expectedValue, int expectedLength)
		{
			var provider = new StarTrackQRCodeTextProvider(data.PackageHeaderWrapper, data.FreightWrapper, data.Package);
			var fieldValue = getFieldValue(provider);
			AssertEquals($"{fieldName} should be {expectedLength} chars long.", expectedLength, fieldValue.Length);
			AssertEquals($"{fieldName}", expectedValue, fieldValue);
		}

		class StarTrackerTestData
		{
			public StarTrackerTestData(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}
			readonly BusinessObjectFactory Factory;

			public void SetupTestData()
			{
				Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "R1", Data.Part1, 20m);
				order = helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "O1", Data.Part1, 10m);

				consignee = helper.CreateClient("CNE", "CNE SYDNEY");
				consignee.MainAddress.FillWithValidTestData();
				consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				consignee.MainAddress.OA_PostCode = "2000";
				consignee.MainAddress.OA_City = "Sydney";
				Order.ConsigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

				packageJob = PkgPackageJob.LoadOrCreatePackageJob(Order);
				package = packageJob.Packages.AddNew("BOX", "123");
				packageHeader = package.GetPackageHeader();
				packageHeader.CurrentPackageJob = packageJob;

				Factory.Save();
			}

			public OrgHeader Client => Data.Org1;
			public WhsWarehouse Warehouse => Data.Whs1;

			public OrgHeader Consignee => consignee;
			OrgHeader consignee;

			public PkgPackageJob PackageJob => packageJob;
			PkgPackageJob packageJob;

			public PkgPackage Package => package;
			PkgPackage package;

			public PkgPackageHeader PackageHeader => packageHeader;
			PkgPackageHeader packageHeader;

			public WhsOrder Order => order;
			WhsOrder order;

			TestDataSimpleEnvironment Data => data ?? (data = new TestDataSimpleEnvironment(Factory));
			TestDataSimpleEnvironment data;

			WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
			WhsTestHelperFunctions helper;

			public PackageWrapperFromPkgPackageHeader PackageHeaderWrapper => packageHeaderWrapper ?? (packageHeaderWrapper = new PackageWrapperFromPkgPackageHeader(PackageJob, PackageHeader, Factory));
			PackageWrapperFromPkgPackageHeader packageHeaderWrapper;
			public FreightWrapper FreightWrapper => PackageHeaderWrapper.Parent;
		}

		#endregion
	}
}
