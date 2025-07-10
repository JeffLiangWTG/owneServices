using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class VesselInfoUpdaterTest : TestCaseWithFactory
	{
		public void TestUpdateVesselInformationDetails()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_RadioCallSign = "OVYQX";
			vessel.RV_Code = "X P MOLLER";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_RadioCallSign = "ABCDE";
			vessel2.RV_Code = "SHIPNAME";
			var vessel3 = Factory.NewWithValidTestData<RefVessel>();
			vessel3.RV_RadioCallSign = "ZYXWV";
			vessel3.RV_Code = "OTHERSHIPNAME1";
			var vessel4 = Factory.NewWithValidTestData<RefVessel>();
			vessel4.RV_RadioCallSign = "ZYXWV";
			vessel4.RV_Code = "OTHERSHIPNAME2";

			Factory.Save();

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB20170306";
			header.JPH_Voyage = "1234567X";
			header.JPH_CarrierCode = "JEFX";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_LoadingPortSuffix = "2";
			header.JPH_JobReference = "HELLO";
			header.JPH_ETA = new ZDateTime(2018, 3, 9);
			header.JPH_ETD = new ZDateTime(2018, 3, 10);

			// Case 1: all details provided in the message and vessel name on file
			header.JPH_VesselName = vessel.RV_Code;
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(Logger, header, "JEFF", "XVYQX", "12345678", "AUSYD", "1", "SHIPNAME", new ZDateTime(2018, 3, 11), new ZDateTime(2018, 3, 12));
			AssertEquals(@"Information - The Carrier Code of AFR Job 'HELLO' changes from 'JEFX' to 'JEFF'.
Information - The Vessel Name of AFR Job 'HELLO' changes from 'X P MOLLER' to 'SHIPNAME'.
Information - The Voyage Number of AFR Job 'HELLO' changes from '1234567X' to '12345678'.
Information - The Port Of Loading Suffix of AFR Job 'HELLO' changes from '2' to '1'.
Information - The Estimated Time Of Arrival of AFR Job 'HELLO' changes from '09-Mar-18 00:00:00' to '11-Mar-18 00:00:00'.
Information - The Estimated Time Of Departure of AFR Job 'HELLO' changes from '10-Mar-18 00:00:00' to '12-Mar-18 00:00:00'.", Logger.Logs);
			AssertEquals("JEFF", header.JPH_CarrierCode);
			AssertEquals("ABCDE", header.JPH_RadioCallSign);
			AssertEquals("12345678", header.JPH_Voyage);
			AssertEquals("AUSYD", header.JPH_RL_NKLoading);
			AssertEquals("1", header.JPH_LoadingPortSuffix);
			AssertEquals("SHIPNAME", header.JPH_VesselName);

			header.JPH_Voyage = "1234567X";
			header.JPH_CarrierCode = "JEFX";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_LoadingPortSuffix = "2";

			// Case 2: all details provided in the message but vessel name not on file
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_RadioCallSign = ZString.Empty;
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(Logger, header, "JEFX", "XVYQX", "1234567X", "AUSYD", "2", "NEWNAME", ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals(@"Error - Vessel name 'NEWNAME' is not on file.
Information - The Vessel Name of AFR Job 'HELLO' changes from 'X P MOLLER' to 'NEWNAME'.", Logger.Logs);
			AssertEquals("JEFX", header.JPH_CarrierCode);
			AssertEquals("", header.JPH_RadioCallSign);
			AssertEquals("1234567X", header.JPH_Voyage);
			AssertEquals("AUSYD", header.JPH_RL_NKLoading);
			AssertEquals("2", header.JPH_LoadingPortSuffix);
			AssertEquals("NEWNAME", header.JPH_VesselName);

			// Case 3: Vessel Name missing, new call sign uniquely matches in RefVessel
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_RadioCallSign = ZString.Empty;
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(Logger, header, "JEFX", "ABCDE", "1234567X", "AUSYD", "2", "", ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals(@"Information - The Vessel Name of AFR Job 'HELLO' changes from 'X P MOLLER' to 'SHIPNAME'.", Logger.Logs);
			AssertEquals("JEFX", header.JPH_CarrierCode);
			AssertEquals("ABCDE", header.JPH_RadioCallSign);
			AssertEquals("1234567X", header.JPH_Voyage);
			AssertEquals("AUSYD", header.JPH_RL_NKLoading);
			AssertEquals("2", header.JPH_LoadingPortSuffix);
			AssertEquals("SHIPNAME", header.JPH_VesselName);

			// Case 4: Vessel Name missing, new call sign doesn't match in RefVessel
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_RadioCallSign = "ABCDE";
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(Logger, header, "JEFX", "ABCDD", "1234567X", "AUSYD", "2", "", ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals(@"Error - Vessel can not be found via Vessel Call Sign 'ABCDD'.
Information - The Vessel Name of AFR Job 'HELLO' changes from 'X P MOLLER' to 'Vessel not on file ABCDD'.
Information - The JPAFRHeader.JPH_RadioCallSign of AFR Job 'HELLO' changes from 'ABCDE' to 'ABCDD'.", Logger.Logs);
			AssertEquals("JEFX", header.JPH_CarrierCode);
			AssertEquals("ABCDD", header.JPH_RadioCallSign);
			AssertEquals("1234567X", header.JPH_Voyage);
			AssertEquals("AUSYD", header.JPH_RL_NKLoading);
			AssertEquals("2", header.JPH_LoadingPortSuffix);
			AssertEquals("Vessel not on file ABCDD", header.JPH_VesselName);

			// Case 5: Vessel Name missing, new call sign has multiple match in RefVessel
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_RadioCallSign = "ABCDD";
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(Logger, header, "JEFX", "ZYXWV", "1234567X", "AUSYD", "2", "", ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals(@"Information - The Vessel Name of AFR Job 'HELLO' changes from 'X P MOLLER' to 'OTHERSHIPNAME1'.", Logger.Logs);
			AssertEquals("JEFX", header.JPH_CarrierCode);
			AssertEquals("ZYXWV", header.JPH_RadioCallSign);
			AssertEquals("1234567X", header.JPH_Voyage);
			AssertEquals("AUSYD", header.JPH_RL_NKLoading);
			AssertEquals("2", header.JPH_LoadingPortSuffix);
			AssertEquals("OTHERSHIPNAME1", header.JPH_VesselName);

			// Case 6: Vessel Name missing, call sign unchanged
			header.JPH_VesselName = vessel.RV_Code;
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateVesselInformationDetailsForSAS148(Logger, header, "JEFX", "OVYQX", "1234567X", "AUSYD", "2", "", ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals("JEFX", header.JPH_CarrierCode);
			AssertEquals("OVYQX", header.JPH_RadioCallSign);
			AssertEquals("1234567X", header.JPH_Voyage);
			AssertEquals("AUSYD", header.JPH_RL_NKLoading);
			AssertEquals("2", header.JPH_LoadingPortSuffix);
			AssertEquals("X P MOLLER", header.JPH_VesselName);
		}

		public void TestUpdateBillReleaseStatus()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "BLA";
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "BLA-1";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateBillReleaseStatus(Logger, bill, AFRBillCustomsStatusList.Codes.Registered);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, bill.JPB_ReleaseStatus);
			AssertEquals("", Logger.Logs);
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateBillReleaseStatus(Logger, bill, AFRBillCustomsStatusList.Codes.NL1);
			AssertEquals(AFRBillCustomsStatusList.Codes.NL1, bill.JPB_ReleaseStatus);
			AssertEquals("Information - The Bill 'BLA-1' Customs Status of AFR Job 'BLA' changes from 'REG' to 'NL1'.", Logger.Logs);
			Logger.ClearLogs();
			VesselInfoUpdater.UpdateBillReleaseStatus(Logger, bill, " NL2\r ");
			AssertEquals(AFRBillCustomsStatusList.Codes.NL2, bill.JPB_ReleaseStatus);
			AssertEquals("Information - The Bill 'BLA-1' Customs Status of AFR Job 'BLA' changes from 'NL1' to 'NL2'.", Logger.Logs);
		}

		protected TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());
		TestErrorLogger logger;
	}
}
