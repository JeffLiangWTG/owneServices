using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMesageDecoderPermitsTest : TestCaseWithFactory
	{
		public void TestProcessDOC()
		{
			testEXDOCMesageDecoderPermits.Process();
			Assert("Import License is empty", testEXDOCMesageDecoderPermits.importLicenseNumber.IsEmpty);
			EXDOCMessageUtilities.PopulateDOC(group1.DOC.InstantiateAChildAndAddItToChildrenCollection(), DocumentMessageNameCodedList.ImportLicence, "DONGISGAY");
			testEXDOCMesageDecoderPermits.Process();
			AssertEquals("Import License", "DONGISGAY", testEXDOCMesageDecoderPermits.importLicenseNumber);
		}

		public void TestProcessDTM()
		{
			testEXDOCMesageDecoderPermits.Process();
			Assert("Import License Date is empty", testEXDOCMesageDecoderPermits.importLicenseDate.IsEmpty);
			EXDOCMessageUtilities.PopulateDTM(group1.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodQualifierList.DocumentMessageDateTime, "20061212", DateTimePeriodFormatQualifierList.Ccyymmdd);
			testEXDOCMesageDecoderPermits.Process();
			AssertEquals("Import License Date", new ZDateTime(2006, 12, 12), testEXDOCMesageDecoderPermits.importLicenseDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			group1 = new SegmentGroup1();
			testEXDOCMesageDecoderPermits = new EXDOCMesageDecoderPermit(group1);
		}

		SegmentGroup1 group1;
		EXDOCMesageDecoderPermit testEXDOCMesageDecoderPermits;
	}
}
