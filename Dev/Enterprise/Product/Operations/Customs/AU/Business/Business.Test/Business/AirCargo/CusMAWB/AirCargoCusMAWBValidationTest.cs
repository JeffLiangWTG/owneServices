using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AirCargoCusMAWBValidationTest : CusMAWBValidationAbstractTest
	{
		public void TestDuplicateMAWBValidation()
		{
			CusMAWB mAWB1 = (CusMAWB)GetNewMAWB();
			mAWB1.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB1.CM_MAWB = "~2100000000";
			mAWB1.CM_MasterHouseBill = "1111";
			AssertEquals("There are no MAWBs with the combination", false, mAWB1.CM_MAWBInfo.HasErrors());
			AssertEquals("There are no MAWBs with the combination", false, mAWB1.CM_MasterHouseBillInfo.HasErrors());

			CusMAWB mAWB2 = (CusMAWB)GetNewMAWB();
			mAWB2.CM_MAWB = "~2100000000";
			mAWB2.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MAWBInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = "1111";
			AssertEquals("There is one MAWB with the number", true, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There is one MAWB with the number", true, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = "2222";
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			CusMAWB mAWB3 = (CusMAWB)GetNewMAWB();
			mAWB3.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB3.CM_MAWB = "~2111111111";

			CusMAWB mAWB4 = (CusMAWB)GetNewMAWB();
			mAWB4.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB4.CM_MAWB = "~2111111111";
			AssertEquals("There is one MAWB with the number", true, mAWB4.CM_MAWBInfo.HasErrors());

			mAWB4.CM_MasterHouseBill = "2222";
			AssertEquals("There is one MAWB with the number", false, mAWB4.CM_MasterHouseBillInfo.HasErrors());

			Customs.Business.CusMAWB nzMAWB = (Customs.Business.CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();
			nzMAWB.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;

			nzMAWB.CM_MAWB = "~2100000000";
			nzMAWB.CM_MasterHouseBill = "2222";
			mAWB2.Validation.ValidateCM_MAWB();
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MAWBInfo.HasErrors());
		}

		public void TestDuplicateMawbValidationForNotActive()
		{
			var mAWB1 = (CusMAWB)GetNewMAWB();
			mAWB1.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB1.CM_MAWB = "~2100000000";
			mAWB1.CM_MasterHouseBill = "1111";
			AssertEquals("There are no MAWBs with the combination", false, mAWB1.CM_MAWBInfo.HasErrors());
			AssertEquals("There are no MAWBs with the combination", false, mAWB1.CM_MasterHouseBillInfo.HasErrors());

			var mAWB2 = (CusMAWB)GetNewMAWB();
			mAWB2.CM_MAWB = "~2100000000";
			mAWB2.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MAWBInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = "1111";
			AssertEquals("There is one MAWB with the number", true, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There is one MAWB with the number", true, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = "2222";
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			var mAWB3 = (CusMAWB)GetNewMAWB();
			mAWB3.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB3.CM_MAWB = "~2111111111";

			var mAWB4 = (CusMAWB)GetNewMAWB();
			mAWB4.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB4.CM_MAWB = "~2111111111";
			AssertEquals("There is one MAWB with the number", true, mAWB4.CM_MAWBInfo.HasErrors());
			mAWB3.CM_IsActive = false;
			mAWB4.CM_MAWB = "~2111111111";
			AssertEquals("There is now no other active MAWBs with the number", false, mAWB4.CM_MAWBInfo.HasErrors());

			mAWB2.CM_IsActive = false;
			mAWB4.CM_MasterHouseBill = "2222";
			AssertEquals("There is no other active MAWB with the number", false, mAWB4.CM_MasterHouseBillInfo.HasErrors());

			Customs.Business.CusMAWB nzMAWB = (Customs.Business.CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();
			nzMAWB.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;

			nzMAWB.CM_MAWB = "~2100000000";
			nzMAWB.CM_MasterHouseBill = "2222";
			mAWB2.Validation.ValidateCM_MAWB();
			AssertEquals("There is one MAWB with the number", false, mAWB2.CM_MAWBInfo.HasErrors());
		}

		public void TestDuplicateMawbValidationForHVLV()
		{
			var mAWB1 = (CusMAWB)GetNewMAWB();
			mAWB1.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mAWB1.CM_MAWB = "~2100000000";
			mAWB1.CM_MasterHouseBill = "1111";
			var hawb1 = mAWB1.ChildBills.AddNew();
			hawb1.CS_IsHVLV = true;
			AssertEquals("There are no MAWBs with the combination", false, mAWB1.CM_MAWBInfo.HasErrors());
			AssertEquals("There are no MAWBs with the combination", false, mAWB1.CM_MasterHouseBillInfo.HasErrors());

			var mAWB2 = (CusMAWB)GetNewMAWB();
			mAWB2.CM_MAWB = "~2100000000";
			mAWB2.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var hawb2 = mAWB2.ChildBills.AddNew();
			hawb2.CS_IsHVLV = true;
			AssertEquals("There are no MAWBs with the combination", false, mAWB2.CM_MAWBInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = "1111";
			AssertEquals("There is one MAWB with the combination", true, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There is one MAWB with the combination", true, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = "2222";
			AssertEquals("There are no MAWBs with the combination", false, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There are no MAWBs with the combination", false, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			mAWB1.CM_MasterHouseBill = ZString.Empty;
			AssertEquals("There are no MAWBs with the combination", false, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("There are no MAWBs with the combination", false, mAWB2.CM_MasterHouseBillInfo.HasErrors());

			mAWB2.CM_MasterHouseBill = ZString.Empty;
			AssertEquals("Does not validate for HVLV empty MasterHouseBill", false, mAWB2.CM_MAWBInfo.HasErrors());
			AssertEquals("Does not validate for HVLV empty MasterHouseBill", false, mAWB2.CM_MasterHouseBillInfo.HasErrors());
		}

		public void TestWarnConsolMasterBeingDifferent()
		{
			CusHAWB houseBill = new ZTestHelper(Factory).CreateTestHouseBill();
			CusMAWB masterBill = houseBill.MAWB;
			ForwardingConsol consol = masterBill.Consol;

			masterBill.CM_FlightNo = consol.JK_VoyageFlightForLastImportTransport;//same
			Assert("AirCargo data is synchronised", !masterBill.CM_FlightNoInfo.HasWarnings());

			masterBill.CM_FlightNo = "CC12";
			Assert("AirCargo data is different from Freight Job Data", masterBill.CM_FlightNoInfo.HasWarnings());

			masterBill.CM_FlightNo = "";
			masterBill.SynchroniseData();
			Assert("AirCargo data is synchronised", !masterBill.CM_FlightNoInfo.HasWarnings());
		}

		public void TestWarnIfArrivalDatesFromConsolDifferentFromAirCargoData()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_ETA = DateTime.Today;
			MAWB.CM_JK = consol.PK;

			MAWB.CM_ArrivalDate = DateTime.Today;
			Assert("Two jobs' date are the same", !MAWB.CM_ArrivalDateInfo.HasWarnings());

			MAWB.CM_ArrivalDate = DateTime.Today.AddDays(-1);
			Assert("Two jobs' date are different", MAWB.CM_ArrivalDateInfo.HasWarnings());
		}

		public void TestAddMessageForInvalidMAWB()
		{
			MAWB.CM_MAWB = "08162424242";
			IEnumerable<INotification> messageErrors = MAWB.CM_MAWBInfo.GetMessageErrors();
			StringBuilder messageError = new StringBuilder();
			foreach (INotification one in messageErrors)
			{
				messageError.Append(one.Message);
			}
			AssertEquals(messageError.ToString(), true, MAWB.CM_MAWBInfo.HasMessageErrors());

			MAWB.CM_MAWB = "08162424246";
			messageErrors = MAWB.CM_MAWBInfo.GetMessageErrors();
			messageError = new StringBuilder();
			foreach (INotification one in messageErrors)
			{
				messageError.Append(one.Message);
			}
			AssertEquals(messageError.ToString(), false, MAWB.CM_MAWBInfo.HasMessageErrors());
		}

		public void TestRunMAWBValidation()
		{
			CusHAWB houseBill = MAWB.ChildBills.AddNew();
			MAWB.RunMAWBValidations();
			Assert("Master Bill has message errors", MAWB.HasMessageErrors);
			Assert("HouseBill validation hasn't run", houseBill.HasMessageErrors);
		}

		public void TestPortOfLoadingBeingOverseas()
		{
			MAWB.CM_FlightNo = "QF1";
			MAWB.CM_RL_NKLoadPort = "AUSYD";
			Assert("The Loading port should be an overseas one", MAWB.CM_RL_NKLoadPortInfo.HasMessageErrors());

			MAWB.CM_RL_NKLoadPort = "NZAKL";
			Assert("The Loading port should be an overseas one", !MAWB.CM_RL_NKLoadPortInfo.HasMessageErrors());
		}

		public void TestEmptyMAWBError()
		{
			MAWB.CM_MAWB = "";
			Assert("Empty master bill number is an error", MAWB.CM_MAWBInfo.HasErrors());
		}

		public void TestMAWBUnique()
		{
			MAWB.CM_MAWB = "081-4568 7112";
			Factory.Save();

			CusMAWB duplicateMAWB = (CusMAWB)GetNewMAWB();
			duplicateMAWB.CM_MAWB = "081-4568 7112";
			AssertEquals("Duplicate MAWB Number is an error", true, duplicateMAWB.CM_MAWBInfo.HasErrors());
		}

		protected override Customs.Business.CusMAWB GetNewMAWB() => Factory.New<CusMAWB>();

		CusMAWB mawb;
		protected CusMAWB MAWB => mawb ?? (mawb = (CusMAWB)GetNewMAWB());
	}
}
