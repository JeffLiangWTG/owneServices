using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CMRAutoUnderbondSenderTest : TestCaseWithFactory
	{
		public void TestSendUnderbondFromDifferentCompany()
		{
			ZString oldABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			try
			{
				ZString newABN = "12345566";
				GlbCompany.GetCurrentCompany(Factory).OrgProxy.PrimaryRegistrationNumber.Number = newABN;
				CusMAWB mAWB = Factory.New<CusMAWB>();
				mAWB.CM_MAWB = "08116326240";
				mAWB.CM_FlightNo = "QF123";
				CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
				mAWB.AllUnderbonds.Load();
				underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
				GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "9999999";
				Factory.Save();
				CMRAutoUnderbondSender.CheckUnderbondsAndSend(mAWB.AllUnderbonds.Cast<CusUnderbond>());
				Assert("Should contain references", underbond1.Messages[0].EM_MessageText.Contains(newABN));
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = oldABN;
			}
		}

		public void TestCheckUnderbondsAndSend()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			CusUnderbond underbond2 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			CusUnderbond underbond3 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond3.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond3);
			EDIMessage[] newMessage = manager.GenerateOriginalMessages(underbond3);
			underbond1.C4_ArrivalDate = new ZDateTime(2010, 1, 1);
			underbond1.C4_FlightNo = "F1";
			underbond1.C4_RL_NKDischargePort = "P1";
			underbond1.C4_IsMoveFromDischarge = false;
			Factory.Save();
			underbond1.Reload();
			underbond1.C4_ArrivalDate = new ZDateTime(2010, 1, 2);
			underbond1.C4_FlightNo = "F2";
			underbond1.C4_RL_NKDischargePort = "P2";
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mAWB.AllUnderbonds.Cast<CusUnderbond>());
			AssertEquals(1, underbond1.Messages.Count);
			var message = underbond1.Messages[0];
			Assert("Message text (" + message.EM_MessageText + ") should contain DTM+132:20100102:102'", message.EM_MessageText.Contains("DTM+132:20100102:102'"));
			Assert("Message text (" + message.EM_MessageText + ") should contain TDT+20+++6+F2::3'", message.EM_MessageText.Contains("TDT+20+++6+F2::3'"));
			Assert("Message text (" + message.EM_MessageText + ") should contain LOC+11+P2::95'", message.EM_MessageText.Contains("LOC+11+P2::95'"));
			AssertEquals(0, underbond2.Messages.Count);
			AssertEquals("Should only have original, not auto generated original", 1, underbond3.Messages.Count);
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mAWB.AllUnderbonds.Cast<CusUnderbond>());
			AssertEquals("Only sent once", 1, underbond1.Messages.Count);
		}

		public void TestAutoUnderbondOnFirstRejection()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08116326240";
			mAWB.CM_FlightNo = "QF123";
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			// User sets underbond to be auto sent later:
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond1.C4_SendersMessageReference = "U00001200";
			Factory.Save();
			// CARST message comes in and Underbond is sent:
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mAWB.AllUnderbonds.Cast<CusUnderbond>());
			AssertEquals("Should still be set to auto underbond for if it gets rejected", CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond1.C4_Status);
			CMRUBMREQEMessage rejection = Factory.New<CMRUBMREQEMessage>();
			// UBM was rejected because a master ACR wasn't submitted:
			rejection.EM_MessageSubType = EDIMessage.Status.Rejected;
			rejection.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQE+C6AF E2C4 1B6:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:UBMREQ'
RFF+AFM:9'
RFF+ABO:U00001200/CMT1::001'
DTM+310:20060919053358:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5201:6:95'
FTX+AAO+++THIS TRANSACTION WAS REJECTED'
ERP+1'
ERC+ERROR:80:95'
ERC+CG0900:6:95'
FTX+AAO+++UCL NOT FOUND (Aln Cd=QF,Flt No=123,Flt Dt=05/09/2006,MawB=08116326240,HawB=) Aln Cd=QF,Flt No=123,Flt Dt=05/09/2006,MawB=08116326240,HawB='
ERP+1'
ERC+ERROR:80:95'
ERC+CG1131:6:95'
FTX+AAO+++UB MOVEMENT NOT PERMITTED MATCHING MAWB ACR NOT FOUND MawB=08116326240,HB=,CrgType=AIR'
ERP+1'
ERC+ERROR:80:95'
ERC+CG1016:6:95'
FTX+AAO+++MESSAGE REJECTED - NO LINES WERE ACCEPTED'
CNT+55:003'
UNT+25+000001'".Replace("\r\n", "");
			rejection.SetEM_LinkedObject();
			Factory.Save();
			AssertEquals(2, underbond1.Messages.Count);
			AssertEquals(CMRBaseStatuses.Codes.OriginalRejected, underbond1.UnderbondStatus.Code);
			AssertEquals("Should still be set to auto underbond because it was rejected and we want to resend", CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond1.C4_Status);
			// Second CARST comes in:
			var factory2 = new BusinessObjectFactory();
			var mawbInFactory2 = factory2.Load<CusMAWB>(mAWB.PK);
			var underbond1InFactory2 = factory2.Load<CusUnderbond>(underbond1.PK);
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mawbInFactory2.AllUnderbonds.Cast<CusUnderbond>());
			factory2.Save();
			AssertEquals(3, underbond1InFactory2.Messages.Count);
			Assert(underbond1InFactory2.C4_Status.IsEmpty);
			// Another CARST:
			var factory3 = new BusinessObjectFactory();
			var mawbInFactory3 = factory2.Load<CusMAWB>(mAWB.PK);
			var underbond1InFactory3 = factory2.Load<CusUnderbond>(underbond1.PK);
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mawbInFactory3.AllUnderbonds.Cast<CusUnderbond>());
			AssertEquals(3, underbond1InFactory3.Messages.Count);
		}

		public void TestAutoUnderbondOnFirstAcceptance()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08116326240";
			mAWB.CM_FlightNo = "QF123";
			CusUnderbond underbond1 = (CusUnderbond)((ICusUnderbondDependentCollectionParent)mAWB).Underbonds.AddNew();
			mAWB.AllUnderbonds.Load();
			// User sets underbond to be auto sent later:
			underbond1.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond1.C4_SendersMessageReference = "U00001200";
			Factory.Save();
			// CARST message comes in and Underbond is sent:
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mAWB.AllUnderbonds.Cast<CusUnderbond>());
			AssertEquals("Should still be set to auto underbond for if it gets rejected", CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond1.C4_Status);
			CMRUBMREQEMessage acceptance = Factory.New<CMRUBMREQEMessage>();
			// UBM was Accepted:
			acceptance.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQE+32C5 EB39 CEA6:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:UBMREQ'
RFF+AFM:9'
RFF+ABO:U00001200/CMT1::001'
DTM+310:20060322031536:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			acceptance.SetEM_LinkedObject();
			Factory.Save();
			AssertEquals(2, underbond1.Messages.Count);
			AssertEquals(CMRBaseStatuses.Codes.OriginalAccepted, underbond1.UnderbondStatus.Code);
			Assert("Should NOT be set to auto underbond because original was accepted", underbond1.C4_Status.IsEmpty);
			// Second CARST comes in:
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(mAWB.AllUnderbonds.Cast<CusUnderbond>());
			AssertEquals("No more messages should have been sent", 2, underbond1.Messages.Count);
		}

		public void TestAutoSeaCargoUnderbondOnFirstAcceptance()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;

			// User sets underbond to be auto sent later:
			underbond.C4_Status = CMRUnderbondStatuses.Codes.UnderbondSendingDelayed;
			underbond.C4_SendersMessageReference = "U00001200";
			Factory.Save();

			// CARST message comes in and Underbond should be sent:
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(container.Underbonds.Cast<CusUnderbond>());
			AssertEquals("Should still be set to auto underbond, until successfully processed, incase it gets rejected", CMRUnderbondStatuses.Codes.UnderbondSendingDelayed, underbond.C4_Status);
			CMRUBMREQEMessage acceptance = Factory.New<CMRUBMREQEMessage>();
			// UBM was Accepted:
			acceptance.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQE+32C5 EB39 CEA6:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:UBMREQ'
RFF+AFM:9'
RFF+ABO:U00001200/CMT1::001'
DTM+310:20060322031536:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			acceptance.SetEM_LinkedObject();
			Factory.Save();
			AssertEquals(2, underbond.Messages.Count);
			AssertEquals(CMRBaseStatuses.Codes.OriginalAccepted, underbond.UnderbondStatus.Code);
			Assert("Should NOT be set to auto underbond because original was accepted", underbond.C4_Status.IsEmpty);
			// Second CARST comes in:
			CMRAutoUnderbondSender.CheckUnderbondsAndSend(container.Underbonds.Cast<CusUnderbond>());
			AssertEquals("No more messages should have been sent", 2, underbond.Messages.Count);
		}
	}
}
