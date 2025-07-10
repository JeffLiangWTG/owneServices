using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class AFRHeaderDataEventContextReaderTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "OB123456";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_RL_NKDischarge = "JPTKY";
			header.JPH_Voyage = "E001";

			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "HB323432";
			bill.JPB_RL_NKOrigin = "AUMEL";
			bill.JPB_RL_NKFinalDestination = "JPYNT";

			var manager = header.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MBOLNumber - OB123456
MBOLOriginUNLOCO - AUSYD
MBOLDestinationUNLOCO - JPTKY
VoyageNumber - E001
HBOLNumber - HB323432
HBOLOriginUNLOCO - AUMEL
HBOLDestinationUNLOCO - JPYNT
".Trim(), eventContextValues);
		}
	}
}
