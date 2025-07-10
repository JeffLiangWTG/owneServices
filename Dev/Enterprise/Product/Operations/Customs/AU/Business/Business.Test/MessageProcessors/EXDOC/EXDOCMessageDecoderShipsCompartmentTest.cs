using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMessageDecoderShipsCompartmentTest : TestCaseWithFactory
	{
		public void TestProcessIMD()
		{
			Assert("Ships Compartments is empty", testEXDOCMessageDecoderShipsCompartment.compartmentNumbers.IsEmpty);
			Assert("Inspection Port is empty", testEXDOCMessageDecoderShipsCompartment.InspectionPort.IsEmpty);
			EXDOCMessageUtilities.PopulateIMD(group8.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitGrainsAndPlantsHeaderMessageBuilder.Compartment, "12345, 67890");
			EXDOCMessageUtilities.PopulateIMD(group8.IMD.InstantiateAChildAndAddItToChildrenCollection(), RequestForPermitGrainsAndPlantsHeaderMessageBuilder.InspectionPortCode, "Tsukumi");
			testEXDOCMessageDecoderShipsCompartment.Process();
			AssertEquals("Ships Compartments", "12345, 67890", testEXDOCMessageDecoderShipsCompartment.compartmentNumbers);
			AssertEquals("Inspection Port", "JPTMI", testEXDOCMessageDecoderShipsCompartment.InspectionPort);
		}

		public void TestProcessDTM()
		{
			Assert("Inspection Date is empty", testEXDOCMessageDecoderShipsCompartment.inspectionDate.IsEmpty);
			EXDOCMessageUtilities.PopulateDTM(group8.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.InspectionDate, "20061212", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMessageDecoderShipsCompartment.Process();
			AssertEquals("Inspection Date", new ZDateTime(2006, 12, 12), testEXDOCMessageDecoderShipsCompartment.inspectionDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			group8 = new SegmentGroup8();
			testEXDOCMessageDecoderShipsCompartment = new EXDOCMessageDecoderShipsCompartment(group8, Factory);
		}

		SegmentGroup8 group8;
		EXDOCMessageDecoderShipsCompartment testEXDOCMessageDecoderShipsCompartment;
	}
}
