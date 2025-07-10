using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsWorkOrderLine))]
	sealed class DocWhsWorkOrderLineTest : DocWhsDocketLineTest<WhsWorkOrder, WhsWorkOrderLine, DocWhsWorkOrderLine>
	{
		public void TestIsBOMTopLevelProduct()
		{
			AssertEquals("BOM top level product", "Y", DocBikeLine.IsBOMTopLevelProduct);
			AssertEquals("Not BOM top level product", "N", DocBikeWheelLine.IsBOMTopLevelProduct);
		}

		public void TestBomLevel()
		{
			AssertEquals("BOM level first level", 0, DocBikeLine.BomLevel);
			AssertEquals("BOM level second level", 1, DocBikeWheelLine.BomLevel);
		}

		public void TestIsTopLevelOrOddIndex()
		{
			AssertEquals("Is Top Level Or Odd Index first level", "Y", DocBikeLine.IsTopLevelOrOddIndex);
			AssertEquals("Is Top Level Or Odd Index second level", "N", DocBikeWheelLine.IsTopLevelOrOddIndex);
		}

		public void TestIsTopLevelOrEvenIndex()
		{
			AssertEquals("Is Top Level Or Even Index first level", "Y", DocBikeLine.IsTopLevelOrEvenIndex);
			AssertEquals("Is Top Level Or Even Index second level", "Y", DocBikeWheelLine.IsTopLevelOrEvenIndex);
		}

		public void TestBomIndentation()
		{
			AssertEquals("Bom Indentation first level", "", DocBikeLine.BomIndentation);
			AssertEquals("Bom Indentation second level", "         ", DocBikeWheelLine.BomIndentation);
		}

		public void TestDocket()
		{
			AssertEquals(typeof(DocWhsWorkOrder), DocketLineWrapper.Docket.GetType());
		}

		public void TestAttributes()
		{
			CombineAssertions(() =>
				{
					AssertEquals("DocWrapper Attributes shoule be empty", ZString.Empty, DocketLineWrapper.Attributes);

					OrgHeader orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_Code = "OH1";
					DocketLine.Docket.WD_OH_Client = orgHeader.PK;
					DocketLine.WE_PartAttrib1 = ZString.Empty;
					AssertEquals("DocWrapper Attributes shoule be empty", ZString.Empty, DocketLineWrapper.Attributes);

					DocketLine.WE_PartAttrib1 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
					AssertEquals("DocWrapper Attributes is correct", "Attribute 1: TEST", DocketLineWrapper.Attributes);

					DocketLine.WE_SerialNumber = "SERT";
					AssertEquals("DocWrapper Attributes is correct", "Attribute 1: TEST,   Tracked Serial Number: SERT", DocketLineWrapper.Attributes);

					DocketLine.WE_PartAttrib1 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
					AssertEquals("DocWrapper Attributes is correct", "LABEL: TEST,   Tracked Serial Number: SERT", DocketLineWrapper.Attributes);

					DocketLine.WE_PartAttrib2 = "TEST2";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL2";
					AssertEquals("DocWrapper Attributes is correct", "LABEL: TEST,   LABEL2: TEST2,   Tracked Serial Number: SERT", DocketLineWrapper.Attributes);

					DocketLine.WE_PartAttrib3 = "TEST3";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL3";
					AssertEquals("DocWrapper Attributes is correct", "LABEL: TEST,   LABEL2: TEST2,   LABEL3: TEST3,   Tracked Serial Number: SERT", DocketLineWrapper.Attributes);

					DocketLine.WE_SerialNumber = ZString.Empty;
					DocketLine.WE_PartAttrib1 = ZString.Empty;
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
					AssertEquals("DocWrapper Attributes is correct", "LABEL2: TEST2,   LABEL3: TEST3", DocketLineWrapper.Attributes);

					DocketLine.WE_PartAttrib2 = ZString.Empty;
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
					AssertEquals("DocWrapper Attributes is correct", "LABEL3: TEST3", DocketLineWrapper.Attributes);
				});
		}

		#region Implementation

		protected override DocWhsWorkOrderLine CreateDocketLineWrapper(WhsWorkOrderLine docketLine)
		{
			return DocWhsWorkOrderLine.New(docketLine, Factory);
		}

		TestDataForBOM Data
		{
			get
			{
				if (data == null)
				{
					data = new TestDataForBOM(Factory);
					data.CreateBOMProductsInInventory();
				}
				return data;
			}
		}
		TestDataForBOM data;

		DocWhsWorkOrderLine DocBikeWheelLine
		{
			get
			{
				if (docBikeWheelLine == null)
				{
					docBikeWheelLine = DocWhsWorkOrderLine.New(Data.BOM.Lines.BikeWheel(workOrder), Factory);
					docBikeWheelLine.Index = 2;
				}
				return docBikeWheelLine;
			}
		}
		DocWhsWorkOrderLine docBikeWheelLine;

		DocWhsWorkOrderLine DocBikeLine
		{
			get
			{
				if (docBikeLine == null)
				{
					docBikeLine = DocWhsWorkOrderLine.New(Helper.CreateWhsWorkOrderLine(WorkOrder, data.BOM.Bike, 2m), Factory);
					docBikeLine.Index = 1;
				}
				return docBikeLine;
			}
		}
		DocWhsWorkOrderLine docBikeLine;

		WhsWorkOrder WorkOrder
		{
			get { return workOrder ?? (workOrder = Helper.CreateWhsWorkOrder(Data.Org1, Data.Whs1)); }
		}
		WhsWorkOrder workOrder;

		#endregion
	}
}
