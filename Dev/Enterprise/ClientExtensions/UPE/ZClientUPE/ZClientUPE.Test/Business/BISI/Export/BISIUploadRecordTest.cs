using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class BISIUploadRecordTest : TransactionedTestCase
	{
		public void TestShipmentChargeLines()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var proxy = new TradeNetIShipmentDataProxyForTest("DOK", DateTime.UtcNow, declaration);
			var chargesData = new ShipmentChargeData[3];
			chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Disbursement, 105m, Core.Constants.CurrencyCodes.Australia);
			chargesData[1] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 159m, Core.Constants.CurrencyCodes.Australia);
			chargesData[2] = new ShipmentChargeData(ShipmentChargeTypeCode.ExtendedAreaSurcharge, 205m, Core.Constants.CurrencyCodes.Australia);
			proxy.ChargesData = chargesData;
			var record = new BISIUploadRecord();
			record.SetShipmentDetailsLine(proxy);
			AssertEquals(3, record._500000Lines.Count);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			using (SGCustomsDataRegistry.Instance.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var regValue = new DecimalEffectiveDate { EffectiveDate = ZDateTime.Today.AddDays(-1), NewValue = 400 };
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateTaxOrFee("DEM", 400m, "SG", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6), "Low Value Threshold");
				factory.Save();
				var template = factory.New<ProcessTaskTemplate>();
				template.P0_Name = "Snakey";
				template.P0_ProcessType = "BRK";
				var column1 = template.GenCustomColumnDefinitions.AddNew();
				column1.XC_Name = "Code 1";
				column1.XC_Type = AddOnColumnDataType.Codes.String;
				var column2 = template.GenCustomColumnDefinitions.AddNew();
				column2.XC_Name = "Charge 1";
				column2.XC_Type = AddOnColumnDataType.Codes.Decimal;
				var customPropertiesCollection = new UserDefinedPropertyCollection(declaration);
				customPropertiesCollection.Add(new ProcessTaskTemplateMatches(template));
				foreach (var column in customPropertiesCollection)
				{
					if (column.Info.Type == typeof(ZString))
					{
						column.TrySetValue(declaration, new ZString("100"));
					}
					else
					{
						column.TrySetValue(declaration, new ZDecimal(10));
					}
				}

				proxy = new TradeNetIShipmentDataProxyForTest("DOK", DateTime.UtcNow, declaration);
				proxy.ChargesData = new ShipmentChargeData[3];
				record = new BISIUploadRecord();
				record.SetShipmentDetailsLine(proxy);
				AssertEquals(4, record._500000Lines.Count);
				template = factory.New<ProcessTaskTemplate>();
				template.P0_Name = "Snakey1";
				template.P0_ProcessType = "GMB";
				column1 = template.GenCustomColumnDefinitions.AddNew();
				column1.XC_Name = "Code 1";
				column1.XC_Type = AddOnColumnDataType.Codes.String;
				column2 = template.GenCustomColumnDefinitions.AddNew();
				column2.XC_Name = "Charge 1";
				column2.XC_Type = AddOnColumnDataType.Codes.Decimal;
				var manifestHeader = factory.New<AsycudaManifestHeader>();
				manifestHeader.FillWithValidTestData();
				var bill = manifestHeader.Bills.AddNew();
				customPropertiesCollection = new UserDefinedPropertyCollection(bill);
				customPropertiesCollection.Add(new ProcessTaskTemplateMatches(template));
				foreach (var column in customPropertiesCollection)
				{
					if (column.Info.Type == typeof(ZString))
					{
						column.TrySetValue(bill, new ZString("100"));
					}
					else
					{
						column.TrySetValue(bill, new ZDecimal(10));
					}
				}

				factory.Save();
				var newBill = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
				var proxy1 = new AsycudaIShipmentDataProxyForTest(newBill);
				proxy1.ChargesData = new ShipmentChargeData[3];
				record = new BISIUploadRecord();
				record.SetShipmentDetailsLine(proxy1);
				AssertEquals(4, record._500000Lines.Count);
			}
		}

		public void TestRecord()
		{
			Record.SetShipmentDetailsLine(ShipmentData);
			AssertEquals(6, Record.LineCount);
			Record.AddShipmentStatusLine(CreateTestShipmentStatusData1());
			Record.AddShipmentStatusLine(CreateTestShipmentStatusData2());
			AssertEquals(2, Record._200000Lines.Count);
			AssertEquals(8, Record.LineCount);
		}

		public void TestRecord_CommodityLines()
		{
			Record.SetShipmentDetailsLine(ShipmentData);
			AssertEquals(2, Record._400000Lines.Count);
			AssertEquals(6, Record.LineCount);
		}

		public void TestRecord_OnlyChargeLines()
		{
			Record.AddShipmentStatusLine(CreateTestShipmentStatusData1());
			AssertEquals(1, Record.LineCount);
		}

		public void TestHasShipmentDetailLine()
		{
			AssertEquals("Should not have shipment detail line yet", false, Record.HasShipmentDetailLine());
			Record.SetShipmentDetailsLine(ShipmentData);
			AssertEquals("Should have shipment detail line after setting it", true, Record.HasShipmentDetailLine());
		}

		public void TestShipmentCountryDetailLines()
		{
			var recipientData = new List<ShipmentReceiptData>
			{
				new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, "001"),
				new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, "010-1984987")
			};

			ShipmentData.ReceiptsData = recipientData;
			Record.SetShipmentDetailsLine(ShipmentData);
			AssertEquals(2, Record._300000Lines.Count);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			InitialPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "GBLON";
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = InitialPort;
			base.TearDown();
		}

		BISIUploadRecord Record
		{
			get
			{
				if (fRecord == null)
				{
					fRecord = new BISIUploadRecord();
				}

				return fRecord;
			}
		}

		ShipmentDataForTest ShipmentData
		{
			get
			{
				if (fShipmentData == null)
				{
					fShipmentData = new ShipmentDataForTest();
					PopulateShipmentDataWithTestValues();
				}

				return fShipmentData;
			}
		}

		ShipmentStatusDataForTest CreateTestShipmentStatusData1()
		{
			ShipmentStatusDataForTest result = new ShipmentStatusDataForTest();
			result.ShipmentRef = ShipmentData.ShipmentRef;
			result.ImportDate = ShipmentData.ImportDate;
			result.ShipmentStatus = "BS";
			result.HoldReasonCode = "REST";
			result.InspectIndicator = false;
			result.AddressCorrectionIndicator = true;
			result.ImportReleaseDate = new ZDateTime(2005, 1, 3);
			result.CustomsRefNo = "X1234567890";
			result.BrokerCode = "BRK19238";
			result.Remarks = "This is the remarks for this exception";
			result.ExceptionStatusCode = "LL";
			result.ExceptionResolutionCode = "JJ";
			return result;
		}

		ShipmentStatusDataForTest CreateTestShipmentStatusData2()
		{
			ShipmentStatusDataForTest result = new ShipmentStatusDataForTest();
			result.ShipmentRef = ShipmentData.ShipmentRef;
			result.ImportDate = ShipmentData.ImportDate;
			result.ShipmentStatus = "AA";
			result.HoldReasonCode = "BB";
			result.InspectIndicator = true;
			result.AddressCorrectionIndicator = false;
			result.ImportReleaseDate = new ZDateTime(2005, 3, 1);
			result.CustomsRefNo = "JJANS1019";
			result.BrokerCode = "LM";
			result.Remarks = "Remarks2";
			result.ExceptionStatusCode = "oo";
			result.ExceptionResolutionCode = "**";
			return result;
		}

		void PopulateShipmentDataWithTestValues()
		{
			ShipmentData.ShipmentRef = "S0015678";
			ShipmentData.ImportDate = new ZDateTime(2004, 10, 12);
			ShipmentData.ImporterAccountNumber = "IMPACC101";
			ShipmentData.DutyType = "AB";
			ShipmentData.MasterBillNumber = "08190837222";
			ShipmentData.CustomsValue = 34.78m;
			ShipmentData.StatisticalValue = 98.21m;
			ShipmentData.DVCCurrencyCode = "SGD";
			ShipmentData.CustomsExchangeRate = 0.709827001m;
			ShipmentData.BISICustomsEntryStatus = "SS";
			ShipmentData.EntryType = "DB";
			ShipmentData.CustomsEntryNumber = "L123459809";
			ShipmentData.CustomsEntryDate = new ZDateTime(2004, 10, 13);
			ShipmentData.CustomsOfficeNumber = "O14";
			ShipmentData.VATNumber = "098364537";
			ShipmentData.ImporterVATDefermentNumber = "13456780";
			ShipmentData.SplitDutyDefermentNumber = "90982123";
			ShipmentData.ReceiptsData = Array.Empty<ShipmentReceiptData>();
			PopulateCommoditiesWithTestValues();
			PopulateShipmentChargesWithTestValues();
		}

		void PopulateCommoditiesWithTestValues()
		{
			var commoditiesData = new CommodityDetailData[2];
			commoditiesData[0] = new CommodityDetailData("Goods1", "Tariff1", Core.Constants.CountryCodes.Australia, 100.40m);
			commoditiesData[1] = new CommodityDetailData("Goods2", "Tariff2", Core.Constants.CountryCodes.NewZealand, 200.75m);
			ShipmentData.CommoditiesData = commoditiesData;
		}

		void PopulateShipmentChargesWithTestValues()
		{
			var chargesData = new ShipmentChargeData[3];
			chargesData[0] = new ShipmentChargeData(ShipmentChargeTypeCode.Disbursement, 105m, Core.Constants.CurrencyCodes.Australia);
			chargesData[1] = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 159m, Core.Constants.CurrencyCodes.Australia);
			chargesData[2] = new ShipmentChargeData(ShipmentChargeTypeCode.ExtendedAreaSurcharge, 205m, Core.Constants.CurrencyCodes.Australia);
			ShipmentData.ChargesData = chargesData;
		}

		BISIUploadRecord fRecord;
		ShipmentDataForTest fShipmentData;
		string InitialPort;
		#endregion
	}
}
