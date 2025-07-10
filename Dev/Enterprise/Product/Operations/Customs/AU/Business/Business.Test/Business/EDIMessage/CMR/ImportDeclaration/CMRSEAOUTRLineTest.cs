using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRSEAOUTRLineTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CMRSEAOUTRMessage message = Factory.New<CMRSEAOUTRMessage>();
			message.EM_MessageText = sEAOUTRMessageWithErrorsText;

			CMRSEAOUTRLine line1 = new CMRSEAOUTRLine(message.CUSRESForTesting.Group4[0]);
			CMRSEAOUTRLine line2 = new CMRSEAOUTRLine(message.CUSRESForTesting.Group4[1]);
			CMRSEAOUTRLine line3 = new CMRSEAOUTRLine(message.CUSRESForTesting.Group4[2]);
			CMRSEAOUTRLine line4 = new CMRSEAOUTRLine(message.CUSRESForTesting.Group4[3]);

			AssertEquals("FTX Text", "THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS", line1.FTXText);
			AssertEquals("FTX Text", "REPORT NOT FOUND FOR CHANGE/DELETE SCO LINE CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H1", line2.FTXText);
			AssertEquals("FTX Text", "NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990055,OBL=LBOBL6994,HBL=H2", line3.FTXText);
			AssertEquals("FTX Text", "NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=,OBL=,HBL=", line4.FTXText);

			AssertEquals("ContainerNumber", "", line1.ContainerNumber);
			AssertEquals("ContainerNumber", "OCLU6990054", line2.ContainerNumber);
			AssertEquals("ContainerNumber", "OCLU6990055", line3.ContainerNumber);
			AssertEquals("ContainerNumber", "", line4.ContainerNumber);

			AssertEquals("MasterBillNumber", "", line1.MasterBillNumber);
			AssertEquals("MasterBillNumber", "LBOBL6993", line2.MasterBillNumber);
			AssertEquals("MasterBillNumber", "LBOBL6994", line3.MasterBillNumber);
			AssertEquals("MasterBillNumber", "", line4.MasterBillNumber);

			AssertEquals("HouseBillNumber", "", line1.HouseBillNumber);
			AssertEquals("HouseBillNumber", "H1", line2.HouseBillNumber);
			AssertEquals("HouseBillNumber", "H2", line3.HouseBillNumber);
			AssertEquals("HouseBillNumber", "", line4.HouseBillNumber);

			AssertEquals("ERC", "ADVICE", line1.ERC);
			AssertEquals("ERC", "ERROR", line2.ERC);
			AssertEquals("ERC", "ERROR", line3.ERC);
			AssertEquals("ERC", "ERROR", line4.ERC);
		}

		readonly string sEAOUTRMessageWithErrorsText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+2646 EGJ1 9AAJ:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEAOUT'
RFF+AFM:4'
RFF+ABO:O00000011/DAT1::005'
DTM+310:20090506130506:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5202:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1978:6:95'
FTX+AAO+++CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054 REPORT NOT FOUND FOR CHANGE/DELETE SCO LINE CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054,OBL=LBOBL6993,HBL=H1'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1978:6:95'
FTX+AAO+++(CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054) NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990055,OBL=LBOBL6994,HBL=H2'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1978:6:95'
FTX+AAO+++(CT=LCL,RDT=06/05/2009,RTM=225600,CNT=OCLU6990054) NUMBER OF PACKAGES IS NOT SUPPLIED CT=LCL,RDT=06/05/2009,RTM=225600,CNT=,OBL=,HBL='
CNT+55:001'
UNT+17+000001'".Replace("\r\n", "");
	}
}
