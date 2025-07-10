using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDTransportLineForAirTest : IMDTransportLineAbstractTest
	{
		public override void TestPopulate()
		{
			var result = GetPopulatedMessage();
			AssertEquals("Details For House Bill One", true, result.Contains("LIN+1+I'PAC+100+1'PAC+1+3'PCI+1'RFF+MWB:AQT1'PCI+1'RFF+HWB:AQT1HBL'PCI+1'RFF+CNR:PG1'"));
			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++AIR:67:95'"));
		}

		public void TestGropu21Pivot2()
		{
			var transportLine = new IMDTransportLineForSeaAndAir(package2.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Details For House Bill Two", true, result.Contains("LIN+1+I'PAC+200+1'PAC+2+3'PCI+1'RFF+MWB:AQT2'PCI+1'RFF+HWB:AQT2HBL'"));
			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++AIR:67:95'"));
		}

		public void TestGropu21Pivot3()
		{
			var transportLine = new IMDTransportLineForSeaAndAir(package3.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Details For House Bill Three", true, result.Contains("LIN+1+I'PAC+2+1'PAC+2+2'PCI+1'RFF+HWB:AQT3HBL'"));
			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++AIR:67:95'"));
		}

		public void TestMarksAndNumbersForNature10()
		{
			package1.CW_MarksAndNos = "Container Marks and Numbers";
			var result = GetPopulatedMessage();
			AssertEquals("Details For House Bill One", true, result.Contains("LIN+1+I'PAC+100+1'PAC+1+3'PCI+28+CONTAINER MARKS AND NUMBERS'PCI+1'RFF+MWB:AQT1'PCI+1'RFF+HWB:AQT1HBL'"));
		}

		public void TestMarksAndNumbersForNature20()
		{
			invoiceLine.JI_IsPackToBondForLine = true;
			package2.CW_MarksAndNos = "Warehouse Marks and Numbers";
			var transportLine = new IMDTransportLineForSeaAndAir(package2.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			transportLine.Populate(1, LineAction.Insert);
			var result = transportLine.group21.ToString(new UNOCCMRCharacterSet());
			AssertEquals("Details For House Bill Two", true, result.Contains("LIN+1+I'PAC+200+1'PAC+2+3'PCI+21+WAREHOUSE MARKS AND NUMBERS'PCI+1'RFF+MWB:AQT2'PCI+1'RFF+HWB:AQT2HBL'"));
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
			AssertEquals("Details For House Bill Three", true, result.Contains("LIN+1+I'PAC+2+1'PAC+2+2'PCI+28+CONTAINER MARKS AND NUMBERS FOR CON:TAINER 3'PCI+1'RFF+HWB:AQT3HBL'"));
		}

		protected override IMDTransportLine GetTransportLineToTest
		{
			get
			{
				package1.PackingGroup.Bill.CU_fPartShipConsignmentReference = "PG1";
				return new IMDTransportLineForSeaAndAir(package1.PackingGroup, entryHeader, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());
			}
		}

		protected override string DeclarationTransportMode => Core.Constants.TransportModes.Air;

		protected override void SetupDeclarationWithMarksAndNumbers()
		{
			package1.CW_MarksAndNos = MarksAndNumbers;
		}
	}
}
