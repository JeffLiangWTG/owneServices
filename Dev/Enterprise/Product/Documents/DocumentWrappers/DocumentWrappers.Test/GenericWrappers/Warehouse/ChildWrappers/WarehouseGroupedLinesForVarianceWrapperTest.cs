using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseGroupedLinesForVarianceWrapper))]
	sealed class WarehouseGroupedLinesForVarianceWrapperTest : GenericWrapperTest
	{
		#region TestProduct

		public void TestProduct()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			AssertEquals(string.Empty, wrapper.ProductCode);
			AssertEquals(string.Empty, wrapper.ProductDescription);

			docketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			docketLine.SupplierPart.OP_PartNum = "ABC";
			docketLine.SupplierPart.OP_Desc = "DEF";

			AssertEquals("ProductCode is correct.", "ABC", wrapper.ProductCode);
			AssertEquals("ProductDescription is correct.", "DEF", wrapper.ProductDescription);
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			AssertEquals(string.Empty, wrapper.PartAttribute1);

			docketLine.WE_PartAttrib1 = "TEST";
			AssertEquals("PartAttribute1 is correct.", "TEST", wrapper.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2WithLabel

		public void TestPartAttribute2WithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			Factory.Save();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(receiveLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("PartAttribute2WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute2WithLabel);

			receive.WD_OH_Client = data.Org1.PK;
			receiveLine.WE_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute2WithLabel);

			receiveLine.WE_PartAttrib2 = "TEST";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", wrapper.PartAttribute2WithLabel);

			receiveLine.WE_PartAttrib2 = "TEST";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("PartAttribute2WithLabel is incorrect", "LABEL: TEST", wrapper.PartAttribute2WithLabel);
		}

		#endregion

		#region TestPartAttribute3WithLabel

		public void TestPartAttribute3WithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			Factory.Save();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(receiveLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("PartAttribute3WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute3WithLabel);

			receive.WD_OH_Client = data.Org1.PK;
			receiveLine.WE_PartAttrib3 = ZString.Empty;
			AssertEquals("PartAttribute3WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute3WithLabel);

			receiveLine.WE_PartAttrib3 = "TEST";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", wrapper.PartAttribute3WithLabel);

			receiveLine.WE_PartAttrib3 = "TEST";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("PartAttribute3WithLabel is incorrect", "LABEL: TEST", wrapper.PartAttribute3WithLabel);
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m);
			Factory.Save();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(receiveLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("TrackedSerialWithLabel should be empty", ZString.Empty, wrapper.TrackedSerialWithLabel);

			receive.WD_OH_Client = data.Org1.PK;
			receiveLine.WE_SerialNumber = ZString.Empty;
			AssertEquals("TrackedSerialWithLabel should be empty", ZString.Empty, wrapper.TrackedSerialWithLabel);

			receiveLine.WE_SerialNumber = "TEST";
			AssertEquals("TrackedSerialWithLabel is correct", "Tracked Serial Number: TEST", wrapper.TrackedSerialWithLabel);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var testDate = ZDate.Today;
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			docketLine.WE_PackingDate = testDate;
			AssertEquals("PackingDateWithLabel should be correct",
				"Packing Date: " + docketLine.WE_PackingDate.ToShortDateString(),
				wrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var testDate = ZDate.Today;
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			docketLine.WE_ExpiryDate = testDate;
			AssertEquals("ExpiryDateWithLabel should be correct",
				"Expiry Date: " + docketLine.WE_ExpiryDate.ToShortDateString(),
				wrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestExpectedReceiptQuantity

		public void TestExpectedReceiptQuantity()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			docketLine.WE_ClientOrderedUnits = 10m;
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);
			AssertEquals("ExpectedReceiptQuantity is correct", 10m, wrapper.TotalExpectedReceiptQuantity);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			docketLine.WE_TransactionQuantity = 10m;
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			AssertEquals("Units is correct", 10m, wrapper.TotalUnits);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			docketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			docketLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("UnitsUQ is correct", "TST", wrapper.UnitsUQ);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			docketLine.WE_CurrentInventoryStatus = "TST";
			AssertEquals("Status property is correct", "TST", wrapper.Status);
		}

		#endregion

		#region TestVariance

		public void TestVariance()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			docketLine.WE_ClientOrderedUnits = 15;
			docketLine.WE_TransactionQuantity = 10;
			var wrapper1 = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);
			AssertEquals("Variance should be -5", -5m, wrapper1.TotalVariance);

			docketLine.WE_ClientOrderedUnits = 5;
			docketLine.WE_TransactionQuantity = 10;
			var wrapper2 = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);
			AssertEquals("Variance should be 5", 5m, wrapper2.TotalVariance);

			docketLine.WE_ClientOrderedUnits = 10;
			docketLine.WE_TransactionQuantity = 10;
			var wrapper3 = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);
			AssertEquals("Variance should be 0", 0m, wrapper3.TotalVariance);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper1 = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);
			AssertEquals("Weight should be 0m", 0m, wrapper1.TotalWeight);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_Weight = 10m;
			docketLine.WE_OP = product.PK;
			docketLine.WE_TransactionQuantity = 5m;

			var wrapper2 = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);
			AssertEquals("Weight should be 50m", 50m, wrapper2.TotalWeight);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			AssertEquals("Weight should be empty", ZString.Empty, wrapper.WeightUQ);

			docketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			docketLine.SupplierPart.OP_WeightUQ = "KG";
			AssertEquals("Weight should be KG", "KG", wrapper.WeightUQ);
		}

		#endregion

		#region TestCustomsData

		public void TestCustomsData()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			docketLine.CustomsData.WB_EntryKey = "ABC";
			docketLine.CustomsData.WB_EntryLineNo = 1;
			docketLine.CustomsData.WB_CustomsQty = 10m;
			docketLine.CustomsData.WB_CustomsUnitOfQty = "UNT";
			docketLine.CustomsData.WB_RN_NKCountryOfOrigin = "AU";
			docketLine.CustomsData.WB_ValueForDuty = 20m;
			docketLine.CustomsData.WB_TILV = 40m;
			docketLine.CustomsData.WB_AddInfo = "TEST";

			var today = ZDate.Today;
			docketLine.CustomsData.WB_EntryDate = today;

			AssertEquals("CustomsEntryKey", "ABC / 1", wrapper.CustomsEntryKey);
			AssertEquals("CustomsEntryDate", today, wrapper.CustomsEntryDate);
			AssertEquals("CustomsQuantity", 10m, wrapper.CustomsQuantity);
			AssertEquals("CustomsQuantityUQ", "UNT", wrapper.CustomsQuantityUQ);
			AssertEquals("CustomsCtryOfOrigin", "AU", wrapper.CustomsCtryOfOrigin);
			AssertEquals("CustomsVFD", 20m, wrapper.CustomsVFD);
			AssertEquals("CustomsTILV", 40m, wrapper.CustomsTILV);
			AssertEquals("CustomsAddInfo", "TEST", wrapper.CustomsAddInfo);
		}

		public void TestCustomsData_NoCustomsData()
		{
			var docketLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine, Factory);

			AssertEquals("CustomsEntryKey", string.Empty, wrapper.CustomsEntryKey);
			AssertEquals("CustomsEntryDate", ZDateTime.Empty, wrapper.CustomsEntryDate);
			AssertEquals("CustomsQuantity", 0m, wrapper.CustomsQuantity);
			AssertEquals("CustomsQuantityUQ", string.Empty, wrapper.CustomsQuantityUQ);
			AssertEquals("CustomsCtryOfOrigin", string.Empty, wrapper.CustomsCtryOfOrigin);
			AssertEquals("CustomsVFD", 0m, wrapper.CustomsVFD);
			AssertEquals("CustomsTILV", 0m, wrapper.CustomsTILV);
			AssertEquals("CustomsAddInfo", string.Empty, wrapper.CustomsAddInfo);
		}

		#endregion

		#region TestIncrementTotalValues

		public void TestIncrementTotalValues()
		{
			var docketLine1 = Factory.New<WhsReceiveLine>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_Weight = 10m;
			docketLine1.WE_OP = product.PK;
			docketLine1.WE_ClientOrderedUnits = 10m;
			docketLine1.WE_TransactionQuantity = 5m;

			var wrapper = new WarehouseGroupedLinesForVarianceWrapper(docketLine1, Factory);
			AssertEquals("TotalWeight", 50m, wrapper.TotalWeight);
			AssertEquals("TotalUnits", 5m, wrapper.TotalUnits);
			AssertEquals("TotalExpectedReceiptQuantity", 10m, wrapper.TotalExpectedReceiptQuantity);
			AssertEquals("TotalVariance", -5m, wrapper.TotalVariance);

			var docketLine2 = Factory.New<WhsReceiveLine>();
			docketLine2.WE_OP = product.PK;
			docketLine2.WE_ClientOrderedUnits = 20m;
			docketLine2.WE_TransactionQuantity = 10m;

			wrapper.IncrementTotalValues(docketLine2);
			AssertEquals("TotalWeight", 150m, wrapper.TotalWeight);
			AssertEquals("TotalUnits", 15m, wrapper.TotalUnits);
			AssertEquals("TotalExpectedReceiptQuantity", 30m, wrapper.TotalExpectedReceiptQuantity);
			AssertEquals("TotalVariance", -15m, wrapper.TotalVariance);
		}

		#endregion

		#region Base Tests

		protected override ZString ExpectedDefaultFormatting => "Registry : (No Default Field Value Available on Registry)";

		protected override string ExpectedFieldMap => @"
WarehouseGroupedLinesForVariance
======================================================================
Name                                    Type
----------------------------------------------------------------------
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
ExpiryDateWithLabel                     String
PackingDateWithLabel                    String
PartAttribute1                          String
PartAttribute2WithLabel                 String
PartAttribute3WithLabel                 String
ProductCode                             String
ProductDescription                      String
Status                                  String
TotalExpectedReceiptQuantity            Decimal
TotalUnits                              Decimal
TotalVariance                           Decimal
TotalWeight                             Decimal
TrackedSerialWithLabel                  String
UnitsUQ                                 String
WeightUQ                                String
";

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = new WarehouseGroupedLinesForVarianceWrapper(null, Factory);
			AssertEquals("CustomsAddInfo", ZString.Empty, emptyWrapper.CustomsAddInfo);
			AssertEquals("CustomsCtryOfOrigin", ZString.Empty, emptyWrapper.CustomsCtryOfOrigin);
			AssertEquals("CustomsEntryDate", ZDateTime.Empty, emptyWrapper.CustomsEntryDate);
			AssertEquals("CustomsEntryKey", ZString.Empty, emptyWrapper.CustomsEntryKey);
			AssertEquals("CustomsQuantity", 0m, emptyWrapper.CustomsQuantity);
			AssertEquals("CustomsQuantityUQ", ZString.Empty, emptyWrapper.CustomsQuantityUQ);
			AssertEquals("CustomsTILV", 0m, emptyWrapper.CustomsTILV);
			AssertEquals("CustomsVFD", 0m, emptyWrapper.CustomsVFD);
			AssertEquals("ExpiryDateWithLabel", ZString.Empty, emptyWrapper.ExpiryDateWithLabel);
			AssertEquals("ExpectedReceiptQuantity", 0m, emptyWrapper.TotalExpectedReceiptQuantity);
			AssertEquals("PackingDateWithLabel", ZString.Empty, emptyWrapper.PackingDateWithLabel);
			AssertEquals("PartAttribute1", ZString.Empty, emptyWrapper.PartAttribute1);
			AssertEquals("PartAttribute2WithLabel", ZString.Empty, emptyWrapper.PartAttribute2WithLabel);
			AssertEquals("PartAttribute3WithLabel", ZString.Empty, emptyWrapper.PartAttribute3WithLabel);
			AssertEquals("ProductCode", ZString.Empty, emptyWrapper.ProductCode);
			AssertEquals("ProductDescription", ZString.Empty, emptyWrapper.ProductDescription);
			AssertEquals("Status", "PND", emptyWrapper.Status);
			AssertEquals("Units", 0m, emptyWrapper.TotalUnits);
			AssertEquals("UnitsUQ", "UNT", emptyWrapper.UnitsUQ);
			AssertEquals("Variance", 0m, emptyWrapper.TotalVariance);
			AssertEquals("Weight", 0m, emptyWrapper.TotalWeight);
			AssertEquals("WeightUQ", ZString.Empty, emptyWrapper.WeightUQ);
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new WarehouseGroupedLinesForVarianceWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WarehouseGroupedLinesForVarianceWrapper(null, Factory);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
