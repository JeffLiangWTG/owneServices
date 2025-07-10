using System;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDTransportLineForSeaTest : IMDTransportLineAbstractTest
	{
		public override void TestPopulate()
		{
			var result = GetPopulatedMessage();
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestGroup21Pivot2()
		{
			var transportLine = new IMDTransportLineForSeaAndAir(package2.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000031' Details For First House Bill", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+200+1'PAC+2+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestGroup21Pivot3()
		{
			var transportLine = new IMDTransportLineForSeaAndAir(package3.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000031' Details For Second House Bill", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+300+1'PAC+3+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT2'PCI+1'RFF+BH:AQT2HBL'"));
		}

		public void TestGroup21Pivot4()
		{
			var transportLine = new IMDTransportLineForSeaAndAir(package4.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000042' Details", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+400+1'PAC+4+3'PCI+1'RFF+AAQ:OCLU10000042'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestTransportModeLiquid()
		{
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			package1.CW_ContainerNoOrEquipmentNo = "";
			AssertMessageContains("LIN+1+I'PAC+++BLK:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'", GetPopulatedMessage());
		}

		public void TestTransportModeBreakBulk()
		{
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			package1.CW_ContainerNoOrEquipmentNo = "";
			AssertMessageContains("LIN+1+I'PAC+++B/B:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'", GetPopulatedMessage());
		}

		public void TestDeclarationContainerMode()
		{
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			var result = GetPopulatedMessage();
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestVisualExaminationIndicator()
		{
			testDec.AddInfo.ZA_VIS_Hidden = true;
			var result = GetPopulatedMessage();
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'GIS+VIS:109:95'PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestMarksAndNumbersForNature10()
		{
			package1.CW_MarksAndNos = "Container Marks and Numbers";

			var line = new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			line.Populate(1, "I");
			var result = GetPopulatedMessage();//line.Group21.ToString(new UNOCCMRCharacterSet()); //
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+28+CONTAINER MARKS AND NUMBERS'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestMarksAndNumbersForNature20()
		{
			invoiceLine.JI_IsPackToBondForLine = true;
			package2.CW_MarksAndNos = "Warehouse Marks and Numbers";
			var transportLine = new IMDTransportLineForSeaAndAir(package2.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000031' Details", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+200+1'PAC+2+3'PCI+21+WAREHOUSE MARKS AND NUMBERS'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestMarksAndNumbersForNature1020()
		{
			invoiceLine.JI_IsPackToBondForLine = true;
			testDec.FilteredInvoiceLines.AddNew();
			testDec.DoMerge();
			package3.CW_MarksAndNos = "Container Marks and Numbers for Container 3";
			var transportLine = new IMDTransportLineForSeaAndAir(package3.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000031' Details", true, result.Contains("LIN+1+I'PAC+++FCL:67:95'PAC+300+1'PAC+3+3'PCI+28+CONTAINER MARKS AND NUMBERS FOR CON:TAINER 3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT2'PCI+1'RFF+BH:AQT2HBL'"));
		}

		public void TestBreakBulkCargo()
		{
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			var transportLine = new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'PAC+++B/B:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestBulkCargo()
		{
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Bulk;
			var transportLine = new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'PAC+++BLK:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestContainerModeEmpty()
		{
			testDec.JE_ContainerMode = ZString.Empty;
			container1.CO_FCL_LCL_AIR = ZString.Empty;
			var transportLine = new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("'OCLU10000020' Details", true, result.Contains("LIN+1+I'PAC+100+1'PAC+1+3'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestConsignmentReferenceNumber()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_PartShipConsignmentReference = "HYE20161114000000326";
			var transportLine = new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.Group28.Group29.ToString(new UNOCCMRCharacterSet());
			Assert("Consignment Reference should have been included in the message", result.Contains("RFF+CNR:HYE20161114000000326"));
			var strippedResult = result.Replace("RFF+CNR:HYE20161114000000326", "");
			AssertEquals("Consignment Reference should only have been included once in the message", false, strippedResult.Contains("RFF+CNR:HYE20161114000000326"));
		}

		protected override IMDTransportLine GetTransportLineToTest
			=> new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());

		protected override string DeclarationTransportMode => Core.Constants.TransportModes.Sea;

		protected override void SetUp()
		{
			base.SetUp();
			AddContainersAndPacksToDeclaration();
		}

		protected override void SetupDeclarationWithMarksAndNumbers()
		{
			package1.CW_MarksAndNos = MarksAndNumbers;
		}

		void AssertMessageContains(string expected, string acceptableResult)
		{
			if (expected.IndexOf(acceptableResult) > -1)
			{
				Assert(true);
			}
			else
			{
				AssertMultilineEquals("Details for Liquid line", expected, acceptableResult, '\'');
			}
		}

		void AddContainersAndPacksToDeclaration()
		{
			container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000020";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			package1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package1.CW_PackQty = 100;
			package1.CW_OuterPacks = 1;
			package1.CW_InBondPackQty = 0;

			container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU10000031";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			package2.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package2.CW_PackQty = 200;
			package2.CW_OuterPacks = 2;
			package2.CW_InBondPackQty = 0;

			package3.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			package3.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_PackQty = 300;
			package3.CW_OuterPacks = 3;
			package3.CW_InBondPackQty = 0;

			container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU10000042";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			package4 = testDec.Packages.AddNew();
			package4.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package4.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			package4.CW_PackQty = 400;
			package4.CW_OuterPacks = 4;
			package4.CW_InBondPackQty = 0;
			package4.PackingGroup.CR_HouseContainerNumber = 4;
		}

		CusContainer container1;
		CusContainer container2;
		CusContainer container3;
		Package package4;
	}
}
