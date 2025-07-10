using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Ccsuk.ServiceTask;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public class CuscarInboundParserTests : CcsukNonChiefResponseBaseMessageProcessorTest
	{
		[TestDate(2015, 8, 22)]
		public void TestFrcConsolThenFriHouseInOneBatchResultsInCorrectMawbDescription()
		{
			BadgeCodeSetting badgeDan;
			CredentialsSetting credentialCla;
			MakeCredentials(out badgeDan, out credentialCla);
			var receivedFactory = new BusinessObjectFactory();
			var mawbFrc = "UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRC3CMU3JQF8Q0+CUKFFW98000DAN:IATA+141119:0735+NRC3CMU3JQF8Q0'UNH+NRC3CMU3JQF8Q0+CUSCAR:1:912:UN'BGM+:::FRC+61884763696'GIS+T:121'TDT+20+306++++SQ:172:3++178:141119:101'LOC+11:LHR:145:3::HCS:129:ZZZ+84:PVG:145:3+85:LHR:145:3'NAD+CB+DAN'GID+0'FTX+AAA+++CONSOL'QTY+118:16'MEA+WT++KGM:125'UNT+11+NRC3CMU3JQF8Q0'UNZ+1+NRC3CMU3JQF8Q0'";
			var houseFri = "UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRI5CMU3JQHV20+CUKFFW98000DAN:IATA+141119:0735+NRI5CMU3JQHV20'UNH+NRI5CMU3JQHV20+CUSCAR:1:912:UN'BGM+:::FRI+61884763696+97:1411190735:201++HWB:14110673'GIS+T:121'TDT+20+306++++SQ:172:3++178:141119:101'LOC+11:LHR:145:3::HCS:129:ZZZ+84:PVG:145:3+85:LHR:145:3'NAD+CB+DAN'GID+0'FTX+AAA+++COMPUTER PARTS'QTY+118:16'MEA+WT++KGM:125'UNT+11+NRI5CMU3JQHV20'UNZ+1+NRI5CMU3JQHV20'";
			EDIInterchange.CreateNewInterchangeFromString(receivedFactory, mawbFrc, ApplicationCodeList.Codes.GbCcsuk);
			EDIInterchange.CreateNewInterchangeFromString(receivedFactory, houseFri, ApplicationCodeList.Codes.GbCcsuk);
			receivedFactory.Save();
			RunTask(null, null);
			var newFactory = new BusinessObjectFactory();
			var mawbsInserted = newFactory.Load<CusMAWB>(new ZQuery());
			AssertEquals("We should have inserted only one mawb. If two, it could be we could not find the first mawb when looking for it while processing the second message.  This could be a date limit issue, factory cache issue....",
							1, mawbsInserted.Length);
			var mawbInserted = mawbsInserted[0];
			var hawbInserted = mawbInserted.ChildBills[0];
			AssertEquals("We process the hawb and mawb messages in same batch.  When we processed the hawb the mawb was in the factory but not yet in the database. Thus we should not assume that the mawb is created as a dummy/placeholder mawb as part of the hawb message processing, and therefore we should not update the mawb's desc with the hawbe's desc.  We keep the mawb's desc from the mawb message.",
							"CONSOL", mawbInserted.DescriptionOfGoods);
			AssertEquals("COMPUTER PARTS", hawbInserted.DescriptionOfGoods);
		}

		public void TestFRCRenominationsToOurOwnAgentResultsInCorrectNewAgentProfile()
		{
			var badgeDan = new BadgeCodeSetting();
			badgeDan.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeDan.BadgeCode = "DAN";
			var credentialDan = new CredentialsSetting();
			credentialDan.BadgeCode = "DAN";
			credentialDan.Company = "DAN";
			credentialDan.PIMA = "CUKFFW98000DAN";

			var badgeCla = new BadgeCodeSetting();
			badgeCla.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeCla.BadgeCode = "CLA";
			var credentialCla = new CredentialsSetting();
			credentialCla.Company = "CLA";
			credentialCla.BadgeCode = "CLA";
			credentialCla.PIMA = "CUKFFW98000CLA";

			var badges = new BadgeCodeSettingCollection();
			badges.Add(badgeDan);
			badges.Add(badgeCla);
			var creds = new CredentialsSettingCollection();
			creds.Add(credentialCla);
			creds.Add(credentialDan);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);

			// Nominated to agent DAN. Renominated to CLA.  Check that PIMA ends up as that of CLA to allow them to send messages.
			var interchangeOne = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRC5CJMSK0ZXJ0+CUKFFW98000DAN:IATA+140806:1739+NRC5CJMSK0ZXJ0'UNH+NRC5CJMSK0ZXJ0+CUSCAR:1:912:UN'BGM+:::FRC+20579482126+++HWB:09364604+50:1408061836:201'GIS+S2Y'GIS+T:121'TDT+20+277++++NH:172:3++178:140806:101'LOC+11:LHR:145:3::ANS:129:ZZZ+84:OSA:145:3+85:LHR:145:3'NAD+CB+DAN'GID+0'FTX+AAA+++COVER PALTE'QTY+118:1'QTY+48:1'MEA+WT++KGM:162'UNT+13+NRC5CJMSK0ZXJ0'UNZ+1+NRC5CJMSK0ZXJ0'", "CUK");
			Factory.Save();
			RunTask(null, null);
			var house = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "09364604"));
			AssertEquals("DAN", house.AgentBadge);
			AssertEquals("CUKFFW98000DAN", house.Profile);
			var interchangeTwo = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRC4CJNHDMI9G0+CUKFFW98000DAN:IATA+140807:0839+NRC4CJNHDMI9G0'UNH+NRC4CJNHDMI9G0+CUSCAR:1:912:UN'BGM+:::FRC+20579482126+++HWB:09364604+50:1408061836:201'GIS+S2Y'GIS+T:121'TDT+20+277++++NH:172:3++178:140806:101'LOC+11:LHR:145:3::ANS:129:ZZZ+84:OSA:145:3+85:LHR:145:3'NAD+CB+CLA'GID+0'FTX+AAA+++COVER PALTE'QTY+118:1'QTY+48:1'MEA+WT++KGM:162'UNT+13+NRC4CJNHDMI9G0'UNZ+1+NRC4CJNHDMI9G0'", "CUK");
			Factory.Save();
			RunTask(null, null);
			house.Reload();
			AssertEquals("CLA", house.AgentBadge);
			AssertEquals("CUKFFW98000DAN", house.Profile);
			var interchangeThree = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRC4CJNHDMIZO0+CUKFFW98000CLA:IATA+140807:0839+NRC4CJNHDMIZO0'UNH+NRC4CJNHDMIZO0+CUSCAR:1:912:UN'BGM+:::FRC+20579482126+++HWB:09364604+50:1408061836:201'GIS+S2Y'GIS+T:121'TDT+20+277++++NH:172:3++178:140806:101'LOC+11:LHR:145:3::ANS:129:ZZZ+84:OSA:145:3+85:LHR:145:3'NAD+CB+CLA'GID+0'FTX+AAA+++COVER PALTE'QTY+118:1'QTY+48:1'MEA+WT++KGM:162'UNT+13+NRC4CJNHDMIZO0'UNZ+1+NRC4CJNHDMIZO0'", "CUK");
			Factory.Save();
			RunTask(null, null);
			house.Reload();
			AssertEquals("CLA", house.AgentBadge);
			AssertEquals("CUKFFW98000CLA", house.Profile);
		}

		public void TestFRCToOurOwnAgentFromOUrOwnShedDoesNotChangeProfile()
		{
			BadgeCodeSetting badgeDan;
			CredentialsSetting credentialCla;
			MakeCredentials(out badgeDan, out credentialCla);

			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = credentialCla.PIMA;
			mawb.AgentBadge = badgeDan.BadgeCode;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "09364604";

			// Nominated to agent DAN, at our own shed LHRCLA.  Shed checks in pieces, auto-FRC to own agent DAN, but this should not change the job's profile.
			var interchangeOne = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRC5CJMSK0ZXJ0+CUKFFW98000DAN:IATA+140806:1739+NRC5CJMSK0ZXJ0'UNH+NRC5CJMSK0ZXJ0+CUSCAR:1:912:UN'BGM+:::FRC+20579482126+++HWB:09364604+50:1408061836:201'GIS+S2Y'GIS+T:121'TDT+20+277++++NH:172:3++178:140806:101'LOC+11:LHR:145:3::CLA:129:ZZZ+84:OSA:145:3+85:LHR:145:3'NAD+CB+DAN'GID+0'FTX+AAA+++COVER PALTE'QTY+118:1'QTY+48:1'MEA+WT++KGM:162'UNT+13+NRC5CJMSK0ZXJ0'UNZ+1+NRC5CJMSK0ZXJ0'", "CUK");
			Factory.Save();
			RunTask(null, null);
			var house = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "09364604"));
			AssertEquals("DAN", house.AgentBadge);
			AssertEquals("LHRCLA", house.CS_WarehouseLocation);
			AssertEquals("PIMA unchanged as this is own own shed", credentialCla.PIMA, house.Profile);
		}

		public static void MakeCredentials(out BadgeCodeSetting badgeDan, out CredentialsSetting credentialCla, string pimaForCredentialCla = "CUKAIR98LHRCLA")
		{
			badgeDan = new BadgeCodeSetting();
			badgeDan.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeDan.BadgeCode = "DAN";
			var credentialDan = new CredentialsSetting();
			credentialDan.BadgeCode = "DAN";
			credentialDan.Company = "DAN";
			credentialDan.PIMA = "CUKFFW98000DAN";

			var shedCla = new BadgeCodeSetting();
			shedCla.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			shedCla.BadgeCode = "CLA";
			credentialCla = new CredentialsSetting();
			credentialCla.Company = "CLA";
			credentialCla.BadgeCode = "CLA";
			credentialCla.PIMA = pimaForCredentialCla;

			var badges = new BadgeCodeSettingCollection();
			badges.Add(badgeDan);
			badges.Add(shedCla);
			var creds = new CredentialsSettingCollection();
			creds.Add(credentialCla);
			creds.Add(credentialDan);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);
		}

		public void TestCuscarFrxDeleteAtConsolidationMasterLevel()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRCWE";
			mawb.CM_MAWB = "15022013002";
			var hawb1 = mawb.ChildBills.AddNew();
			Factory.Save();
			var frx = "UNH+JRX1C36GJVD7D0+CUSCAR:1:912:UN'BGM+:::FRX+15022013002+++HWB:M'TDT+20'LOC+11:LHR:145:3::CWE:129:ZZZ'UNT+5+JRX1C36GJVD7D0'";
			var message = CreateMockMessageForTest(frx, mawb);
			RunTask(mawb, message);
			AssertEquals(PresenceOnNetworkList.Codes.NotOnCommDbDeleted, mawb.PresenceOnNetworkStatus);
			AssertEquals(0, hawb1.Messages.Count);
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals(mawb.MasterLevelHouseHelper.PK, message.EM_LinkUniqueID);
		}

		public void TestProcessableMessageOrder()
		{
			// When two message arrive very closely together and are picked up for processing in one batch, make sure the old message is processed first.
			// It's possible that you get an FRI before an FSN but the message numbers would sort them the other way around.
			var friDate = ZDateTime.Now.AddDays(-1); // Put the date in the message text, don't use [TestDate], because that will set EM_SystemCreateTimeUtc to be equal all round.
			var fri = CreateMockMessageForTest("UNH+222222222222+CUSCAR:1:912:UN'BGM+:::FRI+12514010194+97:" + friDate.ToString("yyMMddHHmm") + ":201'GIS+T:121'TDT+20+176++++BA:172:3++178:" + friDate.ToString("yyMMdd") + ":101'LOC+11:LHR:145:3::BAC:129:ZZZ+84:NYC:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++CONSOLIDATION'QTY+118:10'MEA+WT++KGM:100'UNT+11+222222222222'", null);
			fri.EM_MessageNum = "222222222222";
			Factory.Save();
			System.Threading.Thread.Sleep(4); // 3.3333ms is the resolution of EM_SystemCreateTimeUtc (datetime)
			var fsn = CreateMockMessageForTest("UNH+111111111111+CIMFSN:0:0:Z1:IATA+12514010194'FTX+CIM+++FSN:LHRBAC:125-14010194:CSN/CB/10/" + friDate.ToString("ddMMMHHmm") + "//OK TFR SHD CAX'UNT+3+111111111111'", null);
			fsn.EM_MessageNum = "111111111111";
			Factory.Save();
			RunTask(null, null);
			var awbCreatedAndUpdated = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("Awb was inserted and updated OK.  If messages were retrieved in the wrong order (or either message was not processed properly, perhaps due to date limitations in CusMAWB.Loader), we'll load and process the FSN first, so we'll get an insert but no customs action code update", "CB", awbCreatedAndUpdated.CustomsActionCode);
		}

		public void TestSetRightBranchFromPima()
		{
			BadgeCodeSetting badgeHeathrow;
			CredentialsSetting credentialHeathrow;
			BadgeCodeSetting badgeGatwickPrimary;
			CredentialsSetting credentialGatwickPrimary;
			BadgeCodeSetting badgeGatwickIrrelevant;
			CredentialsSetting credentialGatwickIrrelevant;
			GlbCompany company;
			GlbBranch heathrowBranch;
			GlbBranch gatwickBranch;
			GlbBranch crappyBranchForServiceTask;
			CreateThreeBranches(out company, out heathrowBranch, out gatwickBranch, out crappyBranchForServiceTask, Factory);

			var rawMessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace("\t", "").Replace(System.Environment.NewLine, "").Replace(EDIMessage.MessageNumberPlaceHolder, "DANIEL");
			using (DisposableEnvironment.ForBranch(crappyBranchForServiceTask.PK.ToGuid()))
			{
				CreateCredentials(out badgeHeathrow, out credentialHeathrow, out badgeGatwickPrimary, out credentialGatwickPrimary, out badgeGatwickIrrelevant, out credentialGatwickIrrelevant, company, heathrowBranch, gatwickBranch, Factory);

				var interchangeTextSendToHeathrowBranch = "UNB+UNOA:2+CUKCTM9800120Z:IATA+" + heathrowPima + "/:IATA+110519:1458+833856++833856'"
									+ rawMessageText
									+ "UNZ+1+833856'";
				var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeTextSendToHeathrowBranch, "CUK");
				Factory.Save();
				RunProcessors();
				var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "80112345678"));
				AssertEquals("MAWB made from FRI addressed to Heathrow PIMA is created under Heathrow branch EVEN IF the Heathrow branch has no primary badge defined", heathrowBranch.PK, mawb.CM_GB);
				AssertEquals(heathrowPima, mawb.Profile);

				var interchangeTextSendToGatwickBranch = "UNB+UNOA:2+CUKCTM9800120Z:IATA+" + gatwickPimaPrimary + "/:IATA+110519:1458+833856++833856'"
									+ rawMessageText.Replace("80112345678", "11122222222")
									+ "UNZ+1+833856'";
				interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeTextSendToGatwickBranch, "CUK");
				Factory.Save();
				RunProcessors();
				mawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "11122222222"));
				AssertEquals("MAWB made from FRI addressed to Gatwick PIMA is created under Gatwick branch", gatwickBranch.PK, mawb.CM_GB);
				AssertEquals(gatwickPimaPrimary, mawb.Profile);
			}
		}

		public void TestFCSSplit_Basic()
		{
			var originalFcsText = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			RunFcsTestAndAssertSimple(originalFcsText, basic, CusPartShipSchema.CG_CM_LinkToPartMaster);
		}

		public void TestFCSSplit_Basic_NoGISSegment()
		{
			var originalFcsText = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			RunFcsTestAndAssertSimple(originalFcsText, basic, CusPartShipSchema.CG_CM_LinkToPartMaster);
		}

		public void TestFCSSplit_Basic_WithDeletedHouse()
		{
			var originalFcsText = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var deletedHouse = basic.ChildBills[0];
			deletedHouse.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			RunFcsTestAndAssertSimple(originalFcsText, basic, CusPartShipSchema.CG_CM_LinkToPartMaster);
		}

		public void TestFCSAssignsNprWhenStatus1()
		{
			var originalFcsText = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.NumberOfPiecesExpected = 60;
			basic.NumberOfPiecesReceived = 60;
			RunFcsTestAndAssertSimple(originalFcsText, basic, CusPartShipSchema.CG_CM_LinkToPartMaster, true);
		}

		public void TestFCSSplit_House()
		{
			var originalFcsText = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678+++HWB:87654321'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			RunFcsTestAndAssertSimple(originalFcsText, mawb.ChildBills[0], CusPartShipSchema.CG_CS);
		}

		public void TestFCSSplit_BasicButMessageBelongedToDifferentBranch()
		{
			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "DAN";
			ukCompany.GC_RN_NKCountryCode = "GB";
			var goodBranchThatWillOwnJobAndPrinter = ukCompany.Branches.AddNew();
			goodBranchThatWillOwnJobAndPrinter.GB_Code = "DJC";
			goodBranchThatWillOwnJobAndPrinter.GB_RL_NKHomePort = "GBLON";
			var irrelevantBranchThatWillOwnMessagebutNotJob = ukCompany.Branches.AddNew();
			irrelevantBranchThatWillOwnMessagebutNotJob.GB_Code = "XXX";
			irrelevantBranchThatWillOwnMessagebutNotJob.GB_RL_NKHomePort = "GBLON";
			Factory.Save();
			var originalFcsText = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			using (DisposableEnvironment.ForBranch(goodBranchThatWillOwnJobAndPrinter.PK.ToGuid()))
			{
				var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
				var message = CreateMockMessageForTest(originalFcsText, basic);
				message.EM_GB = irrelevantBranchThatWillOwnMessagebutNotJob.PK;
				basic.CM_GB = goodBranchThatWillOwnJobAndPrinter.PK;
				message = RunTask(basic, message);
				message.Reload();
				MakeAssertionsForFcsTest(basic, CusPartShipSchema.CG_CM_LinkToPartMaster, message);
				AssertEquals(goodBranchThatWillOwnJobAndPrinter.PK, message.EM_GB);
			}
		}

		public void TestFCSSplit_HouseWithTwoMasters()
		{
			var originalFcsText = "UNH+JCS1BKBAYBG9W0+CUSCAR:1:912:UN'BGM+:::FCS+09062011001+++HWB:42345678'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:4'MEA+WT++KGM:4'GID+02'QTY+118:6'MEA+WT++KGM:6'UNT+12+JCS1BKBAYBG9W0'";
			var mawb1 = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb1.CM_MAWB = "10062011006";
			var mawb2 = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb2.CM_MAWB = "09062011001";
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "42345678";
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = "42345678";
			AssertEquals("Precondition 1", hawb1.CS_HAWB, hawb2.CS_HAWB);
			AssertNotEquals("Precondition 2", hawb1.MAWB.CM_MAWB, hawb2.MAWB.CM_MAWB);
			var message = RunFcsTest(originalFcsText, hawb1);
			hawb1 = new BusinessObjectFactory().Load<CusHAWB>(hawb1.PK);
			hawb2 = new BusinessObjectFactory().Load<CusHAWB>(hawb2.PK);
			AssertEquals("The split should only affect hawb2 and mawb2", false, hawb1.HasSplits);
			AssertEquals("The split should only affect hawb2 and mawb2", true, hawb2.HasSplits);
		}

		public void TestFCSSplit_IncreaseAndDecraseNumberOfSplitsAndDeleteAllSplits()
		{
			var firstFcsToMakeThreeSplits = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678+++HWB:87654321'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'UNT+14+MSGREF'";
			var secondFcsToMakeFourSplits = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678+++HWB:87654321'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:4'MEA+WT++KGM:80'GID+04'QTY+118:1'MEA+WT++KGM:20'UNT+15+MSGREF'";
			var thirdFcsToMakeBackToThreeSplits = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678+++HWB:87654321'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:2'MEA+WT++KGM:33'GID+02'QTY+118:3'MEA+WT++KGM:80'GID+03'QTY+118:5'MEA+WT++KGM:100'GID+04'QTY+118:0'UNT+15+MSGREF'";
			var fourthFcsToDeleteAllSplits = "UNH+MSGREF+CUSCAR:2:912:UN:109505'BGM+:::FCS+80112345678+++HWB:87654321'GIS+P:121'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'GID+01'QTY+118:10'MEA+WT++KGM:33'GID+02'QTY+118:0'MEA+WT++KGM:80'GID+03'QTY+118:0'MEA+WT++KGM:0'UNT+14+MSGREF'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];

			RunFcsTest(firstFcsToMakeThreeSplits, hawb);
			AssertSplitDistributionForHouse(hawb.PK, "1: Split into three", 2, 3, 5);

			RunFcsTest(secondFcsToMakeFourSplits, hawb);
			AssertSplitDistributionForHouse(hawb.PK, "2: increase to four", 2, 3, 4, 1);

			RunFcsTest(thirdFcsToMakeBackToThreeSplits, hawb);
			AssertSplitDistributionForHouse(hawb.PK, "3: reduce to 3", 2, 3, 5);

			RunFcsTest(fourthFcsToDeleteAllSplits, hawb);
			AssertEquals("4: delete - reduce to 1 and then remove means no splits remain", 0, hawb.Splits.Count);
		}

		void AssertSplitDistributionForHouse(ZGuid hawbPK, string explanation, params int[] pieces)
		{
			CombineAssertions(() =>
			{
				var newFactoryBecauseOtherwiseSplitCollectionLoadsTheLastLoadOfSplitsToo = new BusinessObjectFactory();
				var hawb = newFactoryBecauseOtherwiseSplitCollectionLoadsTheLastLoadOfSplitsToo.Load<CusHAWB>(hawbPK);
				var splits = hawb.Splits.Cast<SplitConsignment>().OrderBy(sr => sr.SplitReference).ToArray();
				AssertEquals("The awb should have this many splits", pieces.Length, splits.Length);
				for (int i = 0; i < pieces.Length; i++)
				{
					var assertionMessage = $"{explanation}. The {i + 1}th split, with split reference {splits[i].SplitReference}, should have this number of pieces.";
					AssertEquals(assertionMessage, pieces[i], splits[i].NumberOfPiecesExpected);
				}
			});
		}

		void RunFcsTestAndAssertSimple(string originalFcsText, ICcsukCusAwb awb, SchemaGuidColumn schemaColumnForLookupOfSplitsParent, bool expectSplitsToHaveStatus1 = false)
		{
			var message = RunFcsTest(originalFcsText, awb);
			message.Reload();
			MakeAssertionsForFcsTest(awb, schemaColumnForLookupOfSplitsParent, message, expectSplitsToHaveStatus1);
		}

		void MakeAssertionsForFcsTest(ICcsukCusAwb awb, SchemaGuidColumn schemaColumnForLookupOfSplitsParent, EDIMessage message, bool expectSplitsToHaveStatus1 = false)
		{
			CombineAssertions(() =>
			{
				AssertContains("MessageInterpretation", "<th>Split Reference</th><th>Pieces</th><th>Weight</th></tr></thead><tr><td>01</td><td>2</td><td>33KG</td></tr><tr><td>02</td><td>3</td><td>80KG</td></tr><tr><td>03</td><td>5</td><td>100KG</td></tr></table>",
					message.EM_MessageInterpretation);
				AssertContains("Community Request MessageInterpretation", "split using community request", message.EM_MessageInterpretation);
				var splits = Factory.Load<biz.CusPartShip>(new ZQuery(schemaColumnForLookupOfSplitsParent, awb.PK));
				AssertEquals("Number of Splits", 3, splits.Length);
				AssertCusPartShipDetails("01", 2, 2, 33, Core.Constants.Weight.Kilograms);
				AssertCusPartShipDetails("02", 3, 3, 80, Core.Constants.Weight.Kilograms);
				AssertCusPartShipDetails("03", 5, 5, 100, Core.Constants.Weight.Kilograms);
				message.Interchange.Reload();
				AssertEquals("Status", EDIInterchange.Status.Received, message.Interchange.EI_Status);

				void AssertCusPartShipDetails(ZString messageReference, ZShort piecesManifested, ZShort piecesLanded, ZDecimal grossWeight, string grossWeightUQ)
				{
					var split = splits.Single(x => x.CG_MessageReference == messageReference);
					AssertEquals(piecesManifested, split.CG_PiecesManifested);
					if (expectSplitsToHaveStatus1)
					{
						AssertEquals(piecesLanded, split.CG_PiecesLanded);
					}
					AssertEquals(grossWeight, split.CG_GrossWeight);
					AssertEquals(grossWeightUQ, split.CG_GrossWeightUQ);
				}
			});
		}

		EDIMessage RunFcsTest(string originalFcsText, ICcsukCusAwb awb)
		{
			var message = CreateMockMessageForTest(originalFcsText, awb);
			return RunTask(awb, message);
		}

		EDIMessage CreateMockMessageForTest(string originalMessageText, ICcsukCusAwb awb)
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = originalMessageText;
			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_From = "POOP";
			inboundInterchange.EI_To = "FART";
			inboundInterchange.EI_BodyText = "X";
			message.EM_EI = inboundInterchange.PK;
			if (awb != null)
			{
				awb.Factory.Save();
			}
			return message;
		}

		EDIMessage RunTask(ICcsukCusAwb awb, EDIMessage message)
		{
			RunProcessors();
			if (awb != null)
			{
				((BusinessObject)awb).Reload();
			}
			return message;
		}

		public void TestFRXMaster()
		{
			var originalFrxText = "UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+80112345678'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'UNT+5+MSGREF'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRBAC";
			mawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = originalFrxText;
			message.EM_MessageNum = "69";
			mawb.Factory.Save();

			RunProcessors();
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);

			AssertEquals(PresenceOnNetworkList.Codes.NotOnCommDbDeleted, mawb.PresenceOnNetworkStatus);
			AssertEquals("Mawb still active, this field is not touched", true, mawb.CM_IsActive);
			AssertEquals("Mawb now has inbound FRX message on it", 1, mawb.Messages.Count);
			AssertEquals("Mawb msg EM_MessageSubType", "FRX", mawb.Messages[0].EM_MessageSubType);
			AssertEquals("Mawb msg EM_MessageType", "CAR", mawb.Messages[0].EM_MessageType);
			AssertEquals("Mawb msg EM_Status", "RCV", mawb.Messages[0].EM_Status);
			AssertContains("Inbound FRX #69, found consignment 801-12345678, about to delete it", log[0]);
			AssertContains("Mawb msg EM_MessageInterpretation", "Record 801-12345678 deleted using community request", mawb.Messages[0].EM_MessageInterpretation);

			AssertEquals("Although we had to flip out the real segment IDs for fake one in order to parse, due to us not having a real Enterprise.Edifact.D91 assembly and needing to use D00A, the message text should not have change.  It should not have its fake segment names in still",
							originalFrxText, message.EM_MessageText);
			AssertNotNull(mawb.Logs.MostRecentLogByEventTime(Events.SetToInactive));

			AssertEquals("CCS-UK Basic Air Waybill 801-12345678 deleted using community data, message #69", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK Basic Air Waybill 801-12345678</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestFRXNotificationsGoToRecipientForSpecificBranch()
		{
			SetupAndAssertNotificationForBranch(
				"UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+80112345678'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'UNT+5+MSGREF'",
				GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFrx,
				Factory, GetDunstableBranchPkForTest(Factory));
		}

		static internal void SetupAndAssertNotificationForBranch(string receivedMessageText, IRegistryItem registryNotificationItem, BusinessObjectFactory factory, GlbBranch branchForTest,
			bool createMawb = true, bool createConsol = false, bool createNotificationGroup = true, string awbReference = "801-12345678", string createInterchangeForMessageToo = "")
		{
			var branchGuidForTest = branchForTest.PK.ToGuid();
			if (branchForTest == GlbBranch.CurrentBranch)
			{
				branchGuidForTest = Guid.Empty;
			}

			using (DisposableEnvironment.ForBranch(branchGuidForTest))
			{
				string notificationEmailAddress = null;
				if (createNotificationGroup)
				{
					notificationEmailAddress = SetUpNotificationGroup(registryNotificationItem, branchGuidForTest, factory);
				}

				if (createConsol)
				{
					var consol = factory.New<ForwardingConsol>();
					consol.JK_TransportMode = "AIR";
					consol.JK_MasterBillNum = awbReference;
					consol.JK_RL_NKLoadPort = "GBLHR";
					factory.Save();
				}
				if (createMawb)
				{
					var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, factory);
					mawb.CM_MAWB = awbReference;
					mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRBAC";
					mawb.CM_GB = branchGuidForTest;
					mawb.Factory.Save();
				}

				var mockMessage = factory.NewMoq<EDIMessage>();
				var message = mockMessage.Object;
				message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageText = receivedMessageText;
				message.EM_MessageNum = "69";

				if (!string.IsNullOrEmpty(createInterchangeForMessageToo))
				{
					var interchange = EDIInterchange.CreateNewInterchangeFromString(factory, createInterchangeForMessageToo + "UNZ'", "CUK");
					interchange.EI_ApplicationCode = "CUK";
					interchange.EI_ReceiveTransmit = "RCV";
					interchange.EI_InterchangeNum = new Random().Next(1000000).ToString();
					message.EM_EI = interchange.PK;
					interchange.ContainedMessages.Add(message);
				}

				factory.Save();
				new CcsukNonChiefResponseBaseMessageProcessor(new TestServiceLogger()).ExecuteBatch();

				if (createNotificationGroup)
				{
					AssertNotNull(notificationEmailAddress);
					AssertContains(notificationEmailAddress, Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
				}
			}
		}

		static public GlbBranch GetDunstableBranchPkForTest(BusinessObjectFactory factory)
		{
			var company = factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			company.GC_Code = "ZZZ";
			var dunstableBranch = company.Branches.AddNew();
			dunstableBranch.GB_Code = "ZZZ";
			dunstableBranch.GB_RL_NKHomePort = "GBDTE";
			factory.Save();
			return dunstableBranch;
		}

		static public ZString SetUpNotificationGroup(IRegistryItem notificationRegoItem, Guid branchGuid, BusinessObjectFactory factory)
		{
			var notificationEmailAddress = notificationRegoItem.Name.Replace("CustomsResponseNotificationsToGroup", "") + "@zzz.com";
			var group = factory.New<GlbGroup>();
			group.GG_Code = "Test";
			group.GG_Desc = "TestGroup";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZZZ";
			staff.GS_LoginName = "ZZZ";
			staff.GS_EmailAddress = notificationEmailAddress;
			notificationRegoItem.SetValue(Guid.Empty, branchGuid, Guid.Empty, group.PK.ToGuid());
			factory.Save();
			return notificationEmailAddress;
		}

		public void TestFRXHouseLastHouseInConsol()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var house = mawb.ChildBills[0];
			house.CS_WarehouseLocation = "LHRBAC";
			house.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+80112345678+++HWB:87654321'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'UNT+5+MSGREF'";
			message.EM_MessageNum = "69";
			mawb.Factory.Save();

			RunProcessors();
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			house = new BusinessObjectFactory().Load<CusHAWB>(house.PK);

			AssertEquals(PresenceOnNetworkList.Codes.NotOnCommDbDeleted, house.PresenceOnNetworkStatus);
			AssertEquals("Mawb also deleted because the last house has been deleted", PresenceOnNetworkList.Codes.NotOnCommDbDeleted, mawb.PresenceOnNetworkStatus);
			AssertEquals("Mawb still active, do not touch this field", true, mawb.CM_IsActive);
			AssertEquals("Hawb now has inbound FRX message on it", 1, house.Messages.Count);
			AssertEquals("Mawb has no message on it", 0, mawb.Messages.Count);
			AssertEquals("Hawb message EM_MessageSubType", "FRX", house.Messages[0].EM_MessageSubType);
			AssertEquals("Hawb message EM_MessageType", "CAR", house.Messages[0].EM_MessageType);
			AssertEquals("Hawb message EM_Status", "RCV", house.Messages[0].EM_Status);
			AssertContains("Hawb message EM_Interpretation", "<h3>Record 801-12345678-87654321 deleted using community request</h3>", house.Messages[0].EM_MessageInterpretation);
			AssertContains("Inbound FRX #69, found consignment 801-12345678-87654321, about to delete it", log[0]);
			AssertNotNull(house.Logs.MostRecentLogByEventTime(Events.SetToInactive));

			AssertEquals("CCS-UK Master Air Waybill 801-12345678 deleted using community data, message #69", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK Master Air Waybill 801-12345678</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertEquals("CCS-UK House Bill 801-12345678-87654321 deleted using community data, message #69", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[1].Subject);
			AssertContains(">CCS-UK House Bill 801-12345678-87654321</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[1].Body);
		}

		public void TestFRXHouseStillOtherHousesInConsol()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var house1 = mawb.ChildBills[0];
			house1.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			house1.CS_WarehouseLocation = "LHRBAC";
			var house2 = mawb.ChildBills.AddNew();
			house2.CS_HAWB = "66554433";
			house2.CS_WarehouseLocation = "LHRBAC";
			house2.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+80112345678+++HWB:87654321'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'UNT+5+MSGREF'";
			mawb.Factory.Save();

			RunProcessors();
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			house1 = new BusinessObjectFactory().Load<CusHAWB>(house1.PK);

			AssertEquals(PresenceOnNetworkList.Codes.NotOnCommDbDeleted, house1.PresenceOnNetworkStatus);
			AssertEquals("Mawb not deleted because the still more houses left", PresenceOnNetworkList.Codes.OnCommDb, mawb.PresenceOnNetworkStatus);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, house2.PresenceOnNetworkStatus);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestFRXNoMatchingLocalRecord()
		{
			var emailAddress = SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukErrors, Guid.Empty, Factory);
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+11122222222'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'UNT+5+MSGREF'";
			message.EM_MessageNum = "69";
			var receivedInterchange = Factory.New<EDIInterchange>();
			receivedInterchange.EI_ReceiveTransmit = "RCV";
			receivedInterchange.EI_RetryCount = 9;
			receivedInterchange.EI_From = "FROM";
			receivedInterchange.EI_To = "TO";
			message.EM_EI = receivedInterchange.PK;
			Factory.Save();

			RunProcessors();
			message.Reload();
			receivedInterchange.Reload();

			AssertEquals("Message.EM_Status not yet changed from queue", "QUE", message.EM_Status);
			AssertEquals("receivedInterchange's retry count increased", 10, receivedInterchange.EI_RetryCount);
			AssertEquals("Held Until is bumped", new ZDateTime(1986, 3, 12, 4, 28, 0), message.EM_HeldUntilDate);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			receivedInterchange.EI_RetryCount = 10;
			message.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 4, 27, 0); // rewind to simulate real time moving forward
			Factory.Save();
			RunProcessors();
			message.Reload();
			AssertEquals("Message.EM_Status now failed", "FAL", message.EM_Status);
			AssertContains("Inbound FRX message #69, could not find consignment, unable to delete. Requeued. Message text starts: UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+11122222222'", log[0]);
			AssertContains("Inbound FRX message #69, could not find consignment, unable to delete. FAILS. Message text starts: UNH+MSGREF+CUSCAR:2:912:UN:109504'BGM+:::FRX+11122222222'", log[1]);
			AssertEquals("Could not find CCSUK job using inbound FRX data - 69", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(emailAddress, Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		public void TestBasicWithSplitsAssignsPiecesReceivedToSplits()
		{
			GBCustomsDataRegistry.Instance.CcsukAllocationOfNPR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:50'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split1.NumberOfPiecesExpected = 30;
			split2.NumberOfPiecesExpected = 20;
			split2.NumberOfPiecesReceived = 1;
			basic.CM_MAWB = "34315432167";
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic, false);
			AssertEquals((ZShort)50, basic.NumberOfPiecesReceived);
			AssertEquals((ZShort)50, basic.NumberOfPiecesExpected);
			split1.Reload();
			split2.Reload();
			AssertEquals((ZShort)30, split1.NumberOfPiecesReceived);
			AssertEquals((ZShort)20, split2.NumberOfPiecesReceived);
		}

		public void TestBasicWithSplitsAssignsPiecesReceivedToSplits_Status1NotBeingSet()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split1.NumberOfPiecesExpected = 30;
			split2.NumberOfPiecesExpected = 20;
			split1.NumberOfPiecesReceived = 3;
			split2.NumberOfPiecesReceived = 2;
			basic.CM_MAWB = "34315432167";
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic, false);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);
			AssertEquals((ZShort)50, basic.NumberOfPiecesExpected);
			AssertEquals((ZShort)3, split1.NumberOfPiecesReceived);  // unchanged
			AssertEquals((ZShort)2, split2.NumberOfPiecesReceived);  // unchanged
		}

		public void TestFRCSplitBasic()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167+++ACD::69+50:1304161501:201'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Weight = 3000;
			RunExistingSplitFrcTest<CusMAWB, SplitBasic>(frc, basic);
			AssertEquals(new ZDateTime(2013, 04, 16, 15, 01, 0), new BusinessObjectFactory().Load<SplitBasic>(basic.Splits["69"].PK).Status1Date);
		}

		public void TestFRCSplitBasicNewlyNominatedToAgent()
		{
			// NB no split or even basic yet exists.
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167+++ACD::69'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var splitInserted = RunFrcForBrandNewSplitTest<SplitBasic>(frc);
			AssertEquals("343-15432167", splitInserted.Basic.ReferenceNumber);
			AssertEquals("MACHINERY", splitInserted.DescriptionOfGoods);
		}

		public void TestFRCSplitHouseNewlyNominatedToAgentWhenNothingAlreadyExists()
		{
			// NB no split or even house yet exists.
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167+++HWB:DANIEL01:69+50:1304161501:201'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var splitInserted = RunFrcForBrandNewSplitTest<SplitHouse>(frc);
			AssertEquals("343-15432167-DANIEL01", splitInserted.HAWB.ReferenceNumber);
			AssertEquals("MACHINERY", splitInserted.DescriptionOfGoods);
			AssertEquals(new ZDateTime(2013, 04, 16, 15, 01, 0), splitInserted.Status1Date);
		}

		public void TestFRCSplitHouseNewlyNominatedToAgentWhenWholeAwbAlreadyExistsWithoutSplits()
		{
			// Agent was once nominated for whole awb, but then was unnominated. Record still exists locally, nom'd to another agent.  Shed then splits and nominates one of the splits to the first agent. FRC arrives for split, whole awb already exists. Ensure we add & populate a split on the parent AWB, rather than updating the parent with the message's details.
			var consol = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			consol.CM_MAWB = "34315432167";
			consol.NumberOfPiecesExpected = 100;
			consol.NumberOfPiecesReceived = 100;
			var house = consol.ChildBills[0];
			house.CS_HAWB = "DANIEL01";
			house.CS_PiecesLanded = 100;
			house.CS_PiecesManifested = 100;
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167+++HWB:DANIEL01:69'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var splitInserted = RunFrcForBrandNewSplitTest<SplitHouse>(frc);
			house.Reload();
			AssertEquals((ZShort)100, house.CS_PiecesLanded);
			AssertEquals((ZShort)100, house.CS_PiecesManifested);
			AssertEquals((ZShort)50, splitInserted.NumberOfPiecesExpected);
			AssertEquals((ZShort)51, splitInserted.NumberOfPiecesReceived);
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertContains("<h3>Split 69", message.EM_MessageInterpretation);
		}

		public void TestFRCBasicWithSplitMessage()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167+++ACD::69+50:1304161501:201'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Status2Granted = false;

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_ApplicationReference = "000001";
			message.EM_MessageNum = "69";
			basic.Factory.Save();

			RunProcessors();
			message = new BusinessObjectFactory().Load<EDIMessage>(message.PK);

			AssertContains("Inbound FRC message #69, cannot add split to MAWB 343-15432167 which has child bills", log[0]);
			AssertEquals("ERR", message.EM_Status);
		}

		ZGuid MakeInterchangeJustForPima(string recipientPIma)
		{
			var interchangeJustForPima = Factory.New<EDIInterchange>();
			interchangeJustForPima.EI_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			interchangeJustForPima.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			interchangeJustForPima.EI_From = "CUKSYS98COMMDB";
			interchangeJustForPima.EI_To = recipientPIma;
			return interchangeJustForPima.PK;
		}

		T RunFrcForBrandNewSplitTest<T>(string frc) where T : SplitConsignment
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_ApplicationReference = "000001";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");
			Factory.Save();

			RunProcessors();
			var splitInserted = Factory.LoadTop1<T>(new ZQuery());
			AssertEquals(50, (ZInt)splitInserted.NumberOfPiecesExpected);
			AssertEquals(51, (ZInt)splitInserted.NumberOfPiecesReceived);
			AssertEquals("DAN", splitInserted.AgentBadge);
			mockMessage.VerifyAll();
			return splitInserted;
		}

		ICcsukCusAwb RunExistingSplitFrcTest<TAwb, TSplit>(string frc, TAwb parentAwb)
			where TAwb : BusinessObject, ICcsukCusAwb
			where TSplit : SplitConsignment
		{
			parentAwb.NumberOfPiecesReceived = 1;
			parentAwb.NumberOfPiecesExpected = 2;
			var splitAffectedByThisFrcMessage = (TSplit)parentAwb.Splits.AddNew();
			var splitNotTouchedByMessage = (TSplit)parentAwb.Splits.AddNew();

			splitAffectedByThisFrcMessage.SplitReference = "69";
			splitAffectedByThisFrcMessage.Weight = 71;
			splitAffectedByThisFrcMessage.NumberOfPiecesReceived = 72;
			splitAffectedByThisFrcMessage.NumberOfPiecesExpected = 73;
			splitNotTouchedByMessage.SplitReference = "74";
			splitNotTouchedByMessage.NumberOfPiecesReceived = 100;
			splitNotTouchedByMessage.NumberOfPiecesExpected = 1000;
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_ApplicationReference = "000001";
			parentAwb.Factory.Save();

			RunProcessors();
			var newFactory = new BusinessObjectFactory();
			parentAwb = newFactory.Load<TAwb>(parentAwb.PK);
			splitAffectedByThisFrcMessage = newFactory.Load<TSplit>(splitAffectedByThisFrcMessage.PK);
			splitNotTouchedByMessage = newFactory.Load<TSplit>(splitNotTouchedByMessage.PK);
			AssertEquals((ZShort)51, splitAffectedByThisFrcMessage.NumberOfPiecesReceived);
			AssertEquals((ZShort)50, splitAffectedByThisFrcMessage.NumberOfPiecesExpected);
			AssertEquals(3000m, parentAwb.Weight);
			AssertEquals(500m, splitAffectedByThisFrcMessage.Weight);
			AssertEquals("KG", parentAwb.WeightCode);
			AssertEquals("Parent is unchanged", "LXA", parentAwb.AgentBadge);
			AssertEquals("Split has new agent", "DAN", splitAffectedByThisFrcMessage.AgentBadge);  // NB: one day we may need to lock/delete/something the job if we're no longer the nominated agent. Do nothing yet. Note also that we're not allowed to respond to an FSR-with-update (FAU) and update the local job because we're no longer the nominated agent (Navinder's rule - seems strange now).
			AssertEquals("Other Split unchanged", "LXA", splitNotTouchedByMessage.AgentBadge);
			AssertContains(@"<h3>Record LGWBAC " + splitAffectedByThisFrcMessage.ReferenceNumber + " updated using community data</h3>", parentAwb.Messages[0].EM_MessageInterpretation);
			AssertContains(@"<th>Field</th><th>Old Value</th><th>New Value</th></tr></thead><tr style='background-color: Yellow'><td>NPX</td><td>73</td><td>50</td></tr><tr style='background-color: Yellow'><td>NPR</td><td>72</td><td>51</td></tr><tr style='background-color: Gold'><td>Weight</td><td>71.000</td><td>500</td></tr><tr style='background-color: Gold'><td>Weight Unit</td><td>&nbsp;</td><td>KG</td></tr><tr style='background-color: Yellow'><td>Nominated Agent</td><td>LXA</td><td>DAN</td></tr>".Replace("'", "\""),
								parentAwb.Messages[0].EM_MessageInterpretation);
			AssertEquals(splitAffectedByThisFrcMessage.ReferenceNumber + " (50 pieces) updated using community data", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(splitAffectedByThisFrcMessage.ReferenceNumber + " (50 pieces)</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, parentAwb.PresenceOnNetworkStatus);
			return parentAwb;
		}

		public void TestFRCSplitHouse()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167+++HWB:87654321:69'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";

			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.CM_MAWB = "34315432167";
			var hawb = mawb.ChildBills[0];
			hawb.CS_WarehouseLocation = "LGWBAC";
			hawb.CS_Weight = 3000;
			RunExistingSplitFrcTest<CusHAWB, SplitHouse>(frc, hawb);
		}

		public void TestFrcReNominatingToUs()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false); // LXA
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'GIS+S2Y'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+LXA'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Status2Granted = false;
			basic.AgentBadge = "DJC";
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic, false);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);
			var interpretation = basic.Messages[0].EM_MessageInterpretation;
			AssertNotContains("no longer belongs to any of your badges", interpretation);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, basic.PresenceOnNetworkStatus);
		}

		public void TestFrcReNominatingAwayFromUs()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false); // LXA
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'GIS+S2Y'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DJC'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Status2Granted = false;
			basic.AgentBadge = "LXA";
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic, false);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);
			var interpretation = basic.Messages[0].EM_MessageInterpretation;
			AssertContains("no longer belongs to any of your badges", interpretation);
			AssertEquals(PresenceOnNetworkList.Codes.ArchivedOnCcsuk, basic.PresenceOnNetworkStatus);
		}

		public void TestFRCBasic1()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'GIS+S2Y'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Status2Granted = false;
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);
			AssertContains("<td>Status 2 Granted</td><td>N</td><td>Y</td>", basic.Messages[0].EM_MessageInterpretation);
		}

		public void TestFRCBasicNotificationsGoToRecipientForSpecificBranch()
		{
			SetupAndAssertNotificationForBranch(
				@"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+80112345678'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'",
				GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFrc,
				Factory, GetDunstableBranchPkForTest(Factory));
		}

		public void TestFRCBasic2()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'GIS+S2N'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Status2Granted = true;
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);
			AssertContains("<td>Status 2 Granted</td><td>Y</td><td>N</td>", basic.Messages[0].EM_MessageInterpretation);
		}

		public void TestFRCBasic3()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.Status2Granted = false;
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);
			AssertNotContains("St2 not mentioned in message, to not in interpretation", "<td>Status 2 Granted</td>", basic.Messages[0].EM_MessageInterpretation);
		}

		#region Tests to check behaviour of CUSCAR (FRI, FRC) processing when a the basic already has exactly one DELETED hawb - both for re-cycle and create new
		public void TestFRCAddHouseToWithOnceDeletedHouse_ReactivateOriginalHouse()
		{
			var cusCarText = friExampleArrivedHouse.Replace("FRI", "FRC");
			var (mawbReloaded, originalHawb) = RunCusCarProcessingForBasicWithOnceDeletedHouse(cusCarText, "87654321");
			AssertEquals("MAWB has one bill", 1, mawbReloaded.ChildBills.Count);
			var firstUndeletedHawb = mawbReloaded.ChildBills.OfType<CusHAWB>().First(h => h.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			AssertEquals("Existing HAWB is re-used", originalHawb.PK, firstUndeletedHawb.PK);
			AssertEquals("Existing HAWB is present", PresenceOnNetworkList.Codes.OnCommDb, firstUndeletedHawb.PresenceOnNetworkStatus);
			AssertEquals("MAWB is no longer a basic", false, mawbReloaded.IsBasic);
		}

		public void TestFRIAddHouseToWithOnceDeletedHouse_ReactivateOriginalHouse()
		{
			var (mawbReloaded, originalHawb) = RunCusCarProcessingForBasicWithOnceDeletedHouse(friExampleArrivedHouse, "87654321");
			AssertEquals("MAWB has one bill", 1, mawbReloaded.ChildBills.Count);
			var firstUndeletedHawb = mawbReloaded.ChildBills.OfType<CusHAWB>().First(h => h.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			AssertEquals("Existing HAWB is re-used", originalHawb.PK, firstUndeletedHawb.PK);
			AssertEquals("Existing HAWB is present", PresenceOnNetworkList.Codes.OnCommDb, firstUndeletedHawb.PresenceOnNetworkStatus);
			AssertEquals("MAWB is no longer a basic", false, mawbReloaded.IsBasic);
		}

		public void TestFRCAddHouseToWithOnceDeletedHouse_NewBillNumber()
		{
			var cusCarText = friExampleArrivedHouse.Replace("FRI", "FRC");
			var (mawbReloaded, originalHawb) = RunCusCarProcessingForBasicWithOnceDeletedHouse(cusCarText, "12345678");
			AssertEquals("MAWB has two bills", 2, mawbReloaded.ChildBills.Count);
			var firstUndeletedHawb = mawbReloaded.ChildBills.OfType<CusHAWB>().First(h => h.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			var firstDeleteHawb = mawbReloaded.ChildBills.OfType<CusHAWB>().First(h => h.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			AssertNotEquals("Existing HAWB is NOT re-used", originalHawb.PK, firstUndeletedHawb.PK);
			AssertEquals("Existing HAWB is NOT re-used and is the deleted one", originalHawb.PK, firstDeleteHawb.PK);
			AssertEquals("New HAWB is present", PresenceOnNetworkList.Codes.OnCommDb, firstUndeletedHawb.PresenceOnNetworkStatus);
			AssertEquals("MAWB is no longer a basic", false, mawbReloaded.IsBasic);
		}

		public void TestFRIAddHouseToBasicWithOnceDeletedHouse_NewBillNumber()
		{
			var (mawbReloaded, originalHawb) = RunCusCarProcessingForBasicWithOnceDeletedHouse(friExampleArrivedHouse, "12345678");
			AssertEquals("MAWB has two bills", 2, mawbReloaded.ChildBills.Count);
			var firstUndeletedHawb = mawbReloaded.ChildBills.OfType<CusHAWB>().First(h => h.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			var firstDeleteHawb = mawbReloaded.ChildBills.OfType<CusHAWB>().First(h => h.PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			AssertNotEquals("Existing HAWB is NOT re-used", originalHawb.PK, firstUndeletedHawb.PK);
			AssertEquals("Existing HAWB is NOT re-used and is the deleted one", originalHawb.PK, firstDeleteHawb.PK);
			AssertEquals("New HAWB is present", PresenceOnNetworkList.Codes.OnCommDb, firstUndeletedHawb.PresenceOnNetworkStatus);
			AssertEquals("MAWB is no longer a basic", false, mawbReloaded.IsBasic);
		}

		(CusMAWB, CusHAWB) RunCusCarProcessingForBasicWithOnceDeletedHouse(string messageText, string orginalDeletedHawbNumber)
		{
			var master = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			master.CargoTerminalOperatorAirportAndShed = "MANKLM";
			var hawb = master.ChildBills[0];
			hawb.CS_HAWB = orginalDeletedHawbNumber;
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			AssertEquals("Pre-Req - mawb is a basic", true, master.IsBasic);
			var masterReloaded = RunFrcTestAndAssertionsOnCusMAWB(messageText.Replace(System.Environment.NewLine, ""), master, false);
			return (masterReloaded, hawb);
		}
		#endregion

		public void TestFRCBasic4_FrcReceivedInsteadOfFriToMakeNewRecord()
		{
			// Scenario:  MAWB already exists as shed 1. FRC message arrives to advise of new record at shed 2. Ensure we make a second record and don't amend the first.
			var basicAlreadyExists = Factory.New<CusMAWB>();
			basicAlreadyExists.CM_MAWB = "34315432167";
			basicAlreadyExists.NumberOfPiecesExpected = 99;
			basicAlreadyExists.CargoTerminalOperator = "DAN";
			basicAlreadyExists.CargoTerminalOperatorAirport = "LGW";
			Factory.Save();
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";
			RunFrcTestAndAssertionsOnCusMAWB(frc, basicAlreadyExists, false);
			basicAlreadyExists = new BusinessObjectFactory().Load<CusMAWB>(basicAlreadyExists.PK);
			AssertEquals(0, basicAlreadyExists.Messages.Count);
			AssertEquals("DAN", basicAlreadyExists.CargoTerminalOperator);
			AssertEquals((ZShort)99, basicAlreadyExists.NumberOfPiecesExpected);
			var query = new ZQuery(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, basicAlreadyExists.PK);
			query.AddToFilter(CusMAWBSchema.CM_MAWB, "34315432167");
			var newBasic = new BusinessObjectFactory().LoadTop1<CusMAWB>(query);
			AssertNotNull(newBasic);
			AssertEquals(1, newBasic.Messages.Count);
			AssertEquals((ZShort)50, newBasic.NumberOfPiecesExpected);
			AssertEquals("BAC", newBasic.CargoTerminalOperator);
		}

		public void TestFRIGetsLatestMawbForSameSelector()
		{
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "34315432167";
			mawb1.CargoTerminalOperator = "BAC";
			mawb1.CargoTerminalOperatorAirport = "LGW";
			mawb1.CM_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-6);
			Factory.Save();

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "34315432167";
			mawb2.CargoTerminalOperator = "BAC";
			mawb2.CargoTerminalOperatorAirport = "LGW";
			Factory.Save();

			var fri = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRI+34315432167'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = fri;
			message.EM_ApplicationReference = "000001";
			mawb1.Factory.Save();

			RunTask(null,null);

			message = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			var selectedHawbPk = message.EM_LinkUniqueID;
			var selectedHawb = new BusinessObjectFactory().Load<CusHAWB>(selectedHawbPk);
			var selectedMawb = new BusinessObjectFactory().Load<CusMAWB>(selectedHawb.CS_CM);

			AssertEquals("Second mawb should selected for message", selectedMawb.PK, mawb2.PK);

			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestFRIBasicV3WithCommunityHandlingCodesIncludingRemoveOldAndAddNew()
		{
			// Shed record already exists with some codes (AAA & BBB).
			// Check FRI's data (AAA & CCC) completely replaces those and that we show which have been deleted.

			var fri = @"UNH+MSGREF+CUSCAR:3:912:UN:109503'BGM+:::FRI+34315432167'GIS+AAA:131'GIS+CCC:131'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+13+MSGREF'";

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "AAA";
			basic.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "BBB";
			var basicInNewFactory = RunFrcTestAndAssertionsOnCusMAWB(fri, basic, false);
			AssertEquals(2, basicInNewFactory.CommunityHandlingCodes.Count);
			AssertEquals("CCC", basicInNewFactory.CommunityHandlingCodes[1].Data.C4_CommunityHandlingCode);
			AssertContains("<tr><td>Community Handling Code</td><td>AAA</td></tr>", basicInNewFactory.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<tr style=\"background-color: FireBrick\"><td>Community Handling Code</td><td>BBB (deleted)</td></tr>", basicInNewFactory.Messages.LastIncomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td>Community Handling Code</td><td>CCC</td></tr>", basicInNewFactory.Messages.LastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestFRCBasicV3WithCommunityHandlingCodes()
		{
			var frc = @"UNH+MSGREF+CUSCAR:3:912:UN:109503'BGM+:::FRC+34315432167'GIS+DDD:131'GIS+AAA:131'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+13+MSGREF'";

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			PrepareAndRunChcsAndAssert_Whole<CusMAWB>(frc, basic, basic);
		}

		public void TestFRCHouseWithCommunityHandlingCodes()
		{
			var frc = @"UNH+MSGREF+CUSCAR:3:912:UN:109503'BGM+:::FRC+34315432167+++HWB:DANIEL01+50:1304161440:201'GIS+DDD:131'GIS+AAA:131'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+13+MSGREF'";

			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.CM_MAWB = "34315432167";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "DANIEL01";
			PrepareAndRunChcsAndAssert_Whole<CusHAWB>(frc, mawb, hawb);
			AssertEquals(new ZDateTime(2013, 04, 16, 14, 40, 0), hawb.Status1Date);
			mawb = Factory.Load<CusMAWB>(mawb.PK);
			AssertEquals(0, mawb.CommunityHandlingCodes.Count);
		}

		public void TestFRCSplitHouseWithCommunityHandlingCodes()
		{
			var frc = @"UNH+MSGREF+CUSCAR:3:912:UN:109503'BGM+:::FRC+34315432167+++HWB:DANIEL01:02'GIS+DDD:131'GIS+EEE:131'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+13+MSGREF'";
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.CM_MAWB = "34315432167";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "DANIEL01";
			PrepareAndRunChcsAndAssert_Split<CusHAWB>(frc, mawb, hawb);
		}

		public void TestFRCSplitBasicWithCommunityHandlingCodes()
		{
			var frc = @"UNH+MSGREF+CUSCAR:3:912:UN:109503'BGM+:::FRC+34315432167+++ACD::02'GIS+DDD:131'GIS+EEE:131'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+13+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			PrepareAndRunChcsAndAssert_Split<CusMAWB>(frc, basic, basic);
		}

		void PrepareAndRunChcsAndAssert_Whole<T>(string frc, CusMAWB mawbBasicParent, ICcsukCusAwb awb) where T : BusinessObject, ICcsukCusAwb
		{
			awb.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "AAA";
			awb.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "BBB";
			awb.CommunityHandlingCodes.AddNew().Data.C4_CommunityHandlingCode = "CCC";

			RunFrcTestAndAssertionsOnCusMAWB(frc, mawbBasicParent, false);
			awb = new BusinessObjectFactory().Load<T>(awb.PK);
			AssertEquals(2, awb.CommunityHandlingCodes.Count);
			AssertEquals("AAA", awb.CommunityHandlingCodes[0].Data.C4_CommunityHandlingCode);
			AssertEquals("DDD", awb.CommunityHandlingCodes[1].Data.C4_CommunityHandlingCode);
			AssertContains("<tr><td>Community Handling Code</td><td>AAA</td><td>AAA</td></tr><tr style='background-color: FireBrick'><td>Community Handling Code</td><td>BBB</td><td>&nbsp;</td></tr><tr style='background-color: FireBrick'><td>Community Handling Code</td><td>CCC</td><td>&nbsp;</td></tr><tr style='background-color: YellowGreen'><td>Community Handling Code</td><td>&nbsp;</td><td>DDD</td></tr>", awb.Messages[0].EM_MessageInterpretation.Replace("\"", "'"));
		}

		void PrepareAndRunChcsAndAssert_Split<T>(string frc, CusMAWB mawbBasicParent, ICcsukCusAwb awb) where T : BusinessObject, ICcsukCusAwb
		{
			var split1 = awb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2 = awb.Splits.AddNew();
			split2.SplitReference = "02";
			var split3 = awb.Splits.AddNew();
			split3.SplitReference = "03";
			var chc1 = awb.CommunityHandlingCodes.AddNew().Data;
			chc1.C4_CommunityHandlingCode = "AAA";
			chc1.C4_SplitReferenceToWhichThisPertains = "01";
			var chc2 = awb.CommunityHandlingCodes.AddNew().Data;
			chc2.C4_CommunityHandlingCode = "BBB";
			chc2.C4_SplitReferenceToWhichThisPertains = "02";
			var chc3 = awb.CommunityHandlingCodes.AddNew().Data;
			chc3.C4_CommunityHandlingCode = "CCC";
			chc3.C4_SplitReferenceToWhichThisPertains = "03";

			RunFrcTestAndAssertionsOnCusMAWB(frc, mawbBasicParent, false);

			awb = new BusinessObjectFactory().Load<T>(awb.PK);
			awb.CommunityHandlingCodes.Sort(CusAddInfoSchema.B7_AddInfoData.Name, System.ComponentModel.ListSortDirection.Ascending);

			AssertEquals("Four CHCs - AAA still on 01, BBB becomes DDD and EEE on 02, and CCC still on 03", 4, awb.CommunityHandlingCodes.Count);
			AssertEquals("AAA", awb.CommunityHandlingCodes[0].Data.C4_CommunityHandlingCode);
			AssertEquals("01", awb.CommunityHandlingCodes[0].Data.C4_SplitReferenceToWhichThisPertains);
			AssertEquals("CCC", awb.CommunityHandlingCodes[1].Data.C4_CommunityHandlingCode);
			AssertEquals("03", awb.CommunityHandlingCodes[1].Data.C4_SplitReferenceToWhichThisPertains);
			AssertEquals("DDD", awb.CommunityHandlingCodes[2].Data.C4_CommunityHandlingCode);
			AssertEquals("02", awb.CommunityHandlingCodes[2].Data.C4_SplitReferenceToWhichThisPertains);
			AssertEquals("EEE", awb.CommunityHandlingCodes[3].Data.C4_CommunityHandlingCode);
			AssertEquals("02", awb.CommunityHandlingCodes[3].Data.C4_SplitReferenceToWhichThisPertains);
			AssertContains("<tr style='background-color: FireBrick'><td>Community Handling Code</td><td>BBB</td><td>&nbsp;</td></tr><tr style='background-color: YellowGreen'><td>Community Handling Code</td><td>&nbsp;</td><td>DDD</td></tr><tr style='background-color: YellowGreen'><td>Community Handling Code</td><td>&nbsp;</td><td>EEE</td>", awb.Messages[0].EM_MessageInterpretation.Replace("\"", "'"));
		}

		public void TestFRCBasicDoesNotClobberCompleteStatus()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167++++50:8603120401:201'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:68'QTY+48:68'MEA+WT++KGM:500'UNT+11+MSGREF'";
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			basic.PresenceOnNetworkStatus = "YES";
			basic.SetCustomsActionCode("CW", ZDateTime.BrettsBirthday);
			basic.AgentBadge = "DAN";
			AssertEquals(false, ((ICcsukCusAwb)basic).IsCompleteOnCcsuk);
			basic.CM_MAWB = "34315432167";
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic, false);
			AssertEquals((ZShort)68, basic.NumberOfPiecesReceived);
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 1, 0), basic.Status1Date);
			AssertEquals("CW", basic.CustomsActionCode);
			AssertEquals(true, ((ICcsukCusAwb)basic).IsCompleteOnCcsuk);
			AssertEquals(ZDateTime.BrettsBirthday, basic.CustomsActionDate);
		}

		CusMAWB RunFrcTestAndAssertionsOnCusMAWB(string frc, CusMAWB basic, bool alsoMakeExtendedAssertions = true)
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_ApplicationReference = "000001";
			basic.Factory.Save();

			RunProcessors();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			if (alsoMakeExtendedAssertions)
			{
				MakeFrcBasicAssertions(basic);
			}

			return basic;
		}

		void MakeFrcBasicAssertions(CusMAWB basic)
		{
			AssertEquals((ZShort)50, basic.NumberOfPiecesExpected);
			AssertEquals(500m, basic.Weight);
			AssertEquals("KG", basic.WeightCode);
			AssertEquals("LGW", basic.AirportOfArrival);
			AssertEquals("BAC", basic.CargoTerminalOperator);
			AssertEquals("DAN", basic.AgentBadge);
			AssertContains(@"<h3>Record LGWBAC 343-15432167 updated using community data</h3>", basic.Messages[0].EM_MessageInterpretation);
			AssertContains(@"<th>Field</th><th>Old Value</th><th>New Value</th></tr></thead><tr style='background-color: Yellow'><td>Responsible Party ID</td><td>LXA</td><td>DAN</td></tr><tr style='background-color: Yellow'><td>First Arrival Port</td><td>MAN</td><td>LGW</td></tr><tr><td>Cargo Terminal Operator</td><td>BAC</td><td>BAC</td></tr><tr style='background-color: Gold'><td>Goods Description</td><td>COLUMBIAN FLOUR</td><td>MACHINERY</td></tr><tr style='background-color: Yellow'><td>Flight No</td><td>BA112</td><td>&nbsp;</td></tr><tr style='background-color: Yellow'><td>Weight</td><td>3000.000</td><td>500</td></tr><tr><td>Weight UQ</td><td>KG</td><td>KG</td></tr><tr style='background-color: Gold'><td>Pieces Manifested</td><td>15</td><td>50</td></tr><tr style='background-color: Yellow'><td>Pieces Landed</td><td>68</td><td>51</td></tr>".Replace("'", "\""),
								basic.Messages[0].EM_MessageInterpretation);
			AssertEquals("CCS-UK Basic Air Waybill 343-15432167 updated using community data", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK Basic Air Waybill 343-15432167</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestFrcForConsignmentWhichAlreadyHasStatus1DoesntShowAChangeInDate()
		{
			var frc = @"UNH+JRC1BJC5983540+CUSCAR:1:912:UN'BGM+:::FRC+09062011006++++50:1508211543:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:110509:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:XAT:145:3+85:MAN:145:3'NAD+CB+WIS'GID+0'FTX+AAA+++TO CAR'QTY+118:10'QTY+48:10'MEA+WT++KGM:111'UNT+13+JRC1BJC5983540'";

			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "09062011006";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "CAX";
			basic.Profile = "CUKFFW98000LXA";
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			basic.Status1Date = new ZDateTime(2015, 08, 21, 15, 43, 00); // Yesterday
			basic.ShipmentDescriptionCode = "T";
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_MessageNum = "69";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000LXA");
			Factory.Save();

			RunProcessors();
			var basicReloaded = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "09062011006"));

			AssertEquals(new ZDateTime(2015, 08, 21, 15, 43, 00), basicReloaded.Status1Date);
			AssertContains("Status 1 date shows as unchanged", "<tr><td>Status 1 Date</td><td>21-Aug-15 15:43:00</td><td>21-Aug-15 15:43:00</td></tr>", message.EM_MessageInterpretation);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestFrcForConsignmentWhichAlreadyHasStatus1ButIsNowLosingItShowsEdit()
		{
			// Local hob had NPX=NPR=10, but message advises NPX-->11, so loses St1
			var frc = @"UNH+JRC1BJC5983540+CUSCAR:1:912:UN'BGM+:::FRC+09062011006'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:110509:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:XAT:145:3+85:MAN:145:3'NAD+CB+WIS'GID+0'FTX+AAA+++TO CAR'QTY+118:11'QTY+48:10'MEA+WT++KGM:111'UNT+13+JRC1BJC5983540'";

			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "09062011006";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "CAX";
			basic.Profile = "CUKFFW98000LXA";
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			basic.Status1Date = new ZDateTime(2015, 08, 21, 15, 43, 00); // Yesterday
			basic.ShipmentDescriptionCode = "T";
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_MessageNum = "69";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000LXA");
			Factory.Save();

			RunProcessors();
			var basicReloaded = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "09062011006"));

			AssertEquals(ZDateTime.Empty, basicReloaded.Status1Date);
			AssertContains("NPX showing as changed", "<td>Pieces Manifested</td><td>10</td><td>11</td></tr><tr><td>Pieces Landed</td><td>10</td><td>10</td></tr>", message.EM_MessageInterpretation);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestFrcForConsignmentWhichLacksStatus1AndGainsItShowsEdit()
		{
			var frc = @"UNH+JRC1BJC5983540+CUSCAR:1:912:UN'BGM+:::FRC+09062011006++++50:1508211543:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:110509:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:XAT:145:3+85:MAN:145:3'NAD+CB+WIS'GID+0'FTX+AAA+++TO CAR'QTY+118:10'QTY+48:10'MEA+WT++KGM:111'UNT+13+JRC1BJC5983540'";

			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "09062011006";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.CargoTerminalOperator = "CAX";
			basic.Profile = "CUKFFW98000LXA";
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 0;
			basic.ShipmentDescriptionCode = "T";
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_MessageNum = "69";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000LXA");
			Factory.Save();

			RunProcessors();
			var basicReloaded = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "09062011006"));

			AssertEquals(new ZDateTime(2015, 08, 21, 15, 43, 00), basicReloaded.Status1Date);
			AssertContains("<td>Pieces Manifested</td><td>10</td><td>10</td></tr>", message.EM_MessageInterpretation);
			AssertContains("<td>Pieces Landed</td><td>0</td><td>10</td></tr>", message.EM_MessageInterpretation);
			AssertContains("<td>Status 1 Date</td><td>&nbsp;</td><td>21-Aug-15 15:43:00</td></tr>", message.EM_MessageInterpretation);
		}

		public void TestFRCBasicThatHasSplitsButMessagePertainsToWholeJob()
		{
			var frc = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'TDT+20'LOC+11:LGW:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+11+MSGREF'";

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LGWBAC";
			var splitBasicNotTouchedByMessage = basic.Splits.AddNew();
			splitBasicNotTouchedByMessage.SplitReference = "74";
			splitBasicNotTouchedByMessage.NumberOfPiecesReceived = 68;
			splitBasicNotTouchedByMessage.NumberOfPiecesExpected = 15;
			basic = RunFrcTestAndAssertionsOnCusMAWB(frc, basic);
			AssertEquals((ZShort)51, basic.NumberOfPiecesReceived);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNull("C1 NOT produced automatically from FSN because FRC is for whole, but splits exist", printJob);
		}

		public void TestFRCBasic_ReceiveAllPiecesButDontPrintC1BecauseNoCAC()
		{
			var basic = FRCBasic_PrintC1_Runner(false, 50);
			AssertEquals((ZShort)0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNull("C1 NOT produced automatically from FSN because CAC is not released", printJob);
		}

		public void TestFRCBasic_ReceiveAllPiecesAndPrintC1()
		{
			var basic = FRCBasic_PrintC1_Runner(true, 50);
			AssertEquals((ZShort)50, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			var query = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "C1 Removal Authority 343-15432167");
			var printJobOriginal = Factory.LoadTop1<StmPrintJob>(query);
			query.AddToFilter(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "REPRINT");
			var printJobReprint = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull("C1 produced automatically from FSN", printJobOriginal);
			AssertNotNull("C1 reprint produced automatically from FSN", printJobReprint);
		}

		[TestDate(1986, 3, 12, 23, 59, 00)]
		public void TestFRCBasic_ReceiveAllPiecesAfterSomeAlreadyReleased()
		{
			var basic = FRCBasic_PrintC1_Runner(true, 50, 30);
			AssertEquals("Total of 50 (that's an extra 20)  now released by this FRC after 30 manually released (not 30+50=80, but 30+(50-30)=50)", (ZShort)50, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			var logger = NumberOfPiecesReleasedHelper.ReleaseDatesAndCountsFormatted(basic, NumberOfPiecesReleasedHelper.AgentC1Event);
			AssertContains("Initially 30 pieces released", "30 piece(s)", logger);
			AssertContains("Then upon receipt of the FRC we auto release the next 20, NOT the whole 50", "20 piece(s)", logger);
		}

		public void TestFRCBasic_ReceiveSomePiecesButDoNotPrintC1()
		{
			var basic = FRCBasic_PrintC1_Runner(true, 49);
			AssertEquals((ZShort)0, basic.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNull("C1  NOT produced automatically from FSN because not all pieces received", printJob);
		}

		ICcsukCusAwb FRCBasic_PrintC1_Runner(bool alsoMakeInboundFsnFromUnderbondRequest, ZShort newNprInMessage, int setThesePartialPiecesAlreadyReleasedBeforeProcessingFrc = 0)
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());

			var frcText = @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+34315432167'TDT+20'LOC+11:LHR:145:3::KLM:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:" + newNprInMessage + "'MEA+WT++KGM:500'UNT+11+MSGREF'";

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.CM_MAWB = "34315432167";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRKLM";
			basic.NumberOfPiecesReceived = 0;
			basic.NumberOfPiecesExpected = 50;
			if (alsoMakeInboundFsnFromUnderbondRequest)
			{
				basic.SetCustomsActionCode("CW", ZDateTime.BrettsBirthday);
				var inboundFsnText = @"UNH+MSGREF+CIMFSN:0:0:IA+07412345675'FTX+CIM+++FSN:LHRKLM:343-15432167:CSN/CW/50/12SEP1200/000123/YOU ROCK'UNT+3+MSGREF'";
				var mockFsnMessage = Factory.NewMoq<EDIMessage>();
				var fsnMessage = mockFsnMessage.Object;
				fsnMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
				fsnMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				fsnMessage.EM_Status = EDIMessage.Status.Received;
				fsnMessage.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code;
				fsnMessage.EM_MessageSubType = CcsukTransmissionMessageFunction.CIM.CUKFSR.FSN.Subcode;
				fsnMessage.EM_MessageText = inboundFsnText;
				basic.Messages.Add(fsnMessage);
				var iar = basic.IARs.AddNew();
				iar.C4_SendersMessageReference = "U000123";
			}
			var mockFrcMessage = Factory.NewMoq<EDIMessage>();
			var frcMessage = mockFrcMessage.Object;
			frcMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			frcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			frcMessage.EM_MessageText = frcText;

			if (setThesePartialPiecesAlreadyReleasedBeforeProcessingFrc > 0)
			{
				basic.ReleaseThisNumberOfPieces(setThesePartialPiecesAlreadyReleasedBeforeProcessingFrc, NumberOfPiecesReleasedHelper.AgentC1Event);
			}

			basic.Factory.Save();

			RunProcessors();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);

			AssertEquals(newNprInMessage, basic.NumberOfPiecesReceived);
			AssertEquals((ZShort)50, basic.NumberOfPiecesExpected);
			return basic;
		}

		[TestDate(2013, 03, 12, 04, 27, 00)]
		public void TestFRC_ConsignmentNotAlreadyExists()
		{
			var frc = @"UNH+JRC1BJC5983540+CUSCAR:1:912:UN'BGM+:::FRC+09062011006++++50:1105091543:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:110509:101'LOC+11:LHR:145:3::CAX:129:ZZZ+84:XAT:145:3+85:MAN:145:3'NAD+CB+WIS'GID+0'FTX+AAA+++TO CAR'QTY+118:10'QTY+48:11'MEA+WT++KGM:111'UNT+13+JRC1BJC5983540'";

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_MessageNum = "69";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");
			Factory.Save();

			RunProcessors();
			var mawb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "09062011006"));

			AssertEquals((ZShort)11, mawb.NumberOfPiecesReceived);
			AssertEquals((ZShort)10, mawb.NumberOfPiecesExpected);
			AssertEquals(111m, mawb.Weight);
			AssertEquals("KG", mawb.WeightCode);
			AssertEquals("LHR", mawb.AirportOfArrival);
			AssertEquals("MAN", mawb.AirportOfDestination);
			AssertEquals("FRANT", mawb.AirportOfOrigin);
			// Also check that the worker house gets the right airports set too, so that it shows up in the All AWBs grid nicely
			AssertEquals("LHR", mawb.MasterLevelHouseHelper.AirportOfArrival);
			AssertEquals("MAN", mawb.MasterLevelHouseHelper.AirportOfDestination);
			AssertEquals("FRANT", mawb.MasterLevelHouseHelper.AirportOfOrigin);
			AssertEquals("CAX", mawb.CargoTerminalOperator);
			AssertEquals("FRC", mawb.Messages[0].EM_MessageSubType);
			AssertContains(@"<h3>Record inserted using community data</h3>", mawb.Messages[0].EM_MessageInterpretation);
			AssertContains(@"<th>Field</th><th>Value</th></tr></thead><tr><td>Master bill number</td><td>09062011006</td></tr><tr><td>Responsible Party ID</td><td>WIS</td></tr><tr><td>First Arrival Port</td><td>LHR</td></tr><tr><td>Discharge Port</td><td>MAN</td></tr><tr><td>Load Port</td><td>FRANT</td></tr><tr><td>Arrival Date</td><td>09-May-11 00:00:00</td></tr><tr><td>Cargo Terminal Operator</td><td>CAX</td></tr><tr><td>Goods Description</td><td>TO CAR</td></tr><tr><td>Flight No</td><td>BA</td></tr><tr><td>Weight</td><td>111</td></tr><tr><td>Weight UQ</td><td>KG</td></tr><tr><td>Pieces Manifested</td><td>10</td></tr><tr><td>Pieces Landed</td><td>11</td></tr><tr><td>Shipment Description Code</td><td>T</td></tr><tr><td>Status 2 Granted</td><td>Y</td></tr><tr><td>Status 1 Date</td><td>09-May-11 15:43:00</td>", mawb.Messages[0].EM_MessageInterpretation);

			AssertEquals("CCS-UK Basic Air Waybill 090-62011006 created using community data", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK Basic Air Waybill 090-62011006</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("Inbound FRC message #69, consignment LHRCAX62011006 not found, inserting new record instead", log[0]);
			AssertContains("Created new master 09062011006 using inbound data from community because an insertable message was received and no mawb was found", log[1]);
			AssertEquals("Status 1 date is set as per message even if incongruent with piece counts", new ZDateTime(2011, 05, 09, 15, 43, 0), mawb.Status1Date);
		}

		public void TestFRC_ConsignmentNotAlreadyExistsNotificationsGoesToRecipientForDefaultBranch()
		{
			SetupAndAssertNotificationForBranch(
				@"UNH+JRC1BJC5983540+CUSCAR:1:912:UN'BGM+:::FRC+80112345678++++50:1105091543:201'GIS+S2Y'GIS+T:121'TDT+20+++++BA:172:3++178:110509:101'LOC+11:LHR:145:3::BAC:129:ZZZ+84:XAT:145:3+85:LHR:145:3'NAD+CB+WIS'GID+0'FTX+AAA+++TO CAR'QTY+118:10'QTY+48:11'MEA+WT++KGM:111'UNT+13+JRC1BJC5983540'",
				GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFrc,
				Factory, GetDunstableBranchPkForTest(Factory));
		}

		[TestDate(2013, 03, 12, 04, 27, 00)]
		public void TestFRCSplitNothingAlreadyExists()
		{
			var frc = @"UNH+NRC3C629QRRHF0+CUSCAR:1:912:UN'BGM+:::FRC+17600701223+++ACD::02+50:1305201633:201'GIS+S2Y'GIS+T:121'TDT+20+0039++++EK:172:3++178:130520:101'LOC+11:BHX:145:3::SLS:129:ZZZ+84:DAC:145:3+85:BHX:145:3'NAD+CB+CAR'GID+02'FTX+AAA+++VEGETABLES'QTY+118:47'QTY+48:46'MEA+WT++KGM:250.8'UNT+13+NRC3C629QRRHF0'";

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_MessageNum = "69";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");
			Factory.Save();

			RunProcessors();
			var basic = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "17600701223"));
			var split = basic.Splits[0];
			AssertEquals("02", split.SplitReference);
			AssertEquals((ZShort)46, split.NumberOfPiecesReceived);
			AssertEquals((ZShort)47, split.NumberOfPiecesExpected);
			AssertEquals(250.8m, split.Weight);
			AssertContains(@"<h3>Split 02", basic.Messages[0].EM_MessageInterpretation);
			AssertEquals((ZShort)46, basic.NumberOfPiecesReceived);
			AssertEquals((ZShort)47, basic.NumberOfPiecesExpected);
		}

		[TestDate(2013, 03, 12, 04, 27, 00)]
		public void TestFRCSplitAddsToExistingSplitBasic()
		{
			// Basic exists with one split, FRC adds a further split (e.g. if it was nominated to someone else and then re-nominated to us).
			// Check that we add the new split and update the parent's NoP to be the sum of its children's.

			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "17600701223";
			basic.Profile = "CUKFFW98000AAA";
			basic.MasterLevelHouseHelper.CS_WarehouseLocation = "BHXSLS";
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 9;
			basic.Weight = 100;
			var split01 = basic.Splits.AddNew();
			split01.SplitReference = "01";
			split01.NumberOfPiecesExpected = 10;
			split01.NumberOfPiecesReceived = 9;
			split01.Weight = 100;

			var frc = @"UNH+NRC3C629QRRHF0+CUSCAR:1:912:UN'BGM+:::FRC+17600701223+++ACD::02+50:1305201633:201'GIS+S2Y'GIS+T:121'TDT+20+0039++++EK:172:3++178:130520:101'LOC+11:BHX:145:3::SLS:129:ZZZ+84:DAC:145:3+85:BHX:145:3'NAD+CB+CAR'GID+02'FTX+AAA+++VEGETABLES'QTY+118:47'QTY+48:46'MEA+WT++KGM:250.8'UNT+13+NRC3C629QRRHF0'";

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = frc;
			message.EM_MessageNum = "69";
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");
			Factory.Save();

			RunProcessors();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			var split02 = basic.Splits["02"];
			AssertEquals((ZShort)46, split02.NumberOfPiecesReceived);
			AssertEquals((ZShort)47, split02.NumberOfPiecesExpected);
			AssertEquals(250.8m, split02.Weight);
			AssertContains(@"<h3>Split 02", basic.Messages[0].EM_MessageInterpretation);
			AssertEquals((ZShort)55, basic.NumberOfPiecesReceived); // sum of splits' details
			AssertEquals((ZShort)57, basic.NumberOfPiecesExpected);  // sum of splits' details
		}

		public void TestFRINewHouseCreatedOnNewMawbDoesNotUpdateExistingHouses()
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");

			var existingMawb = Factory.New<CusMAWB>();
			existingMawb.CM_MAWB = "99999999999";
			existingMawb.CM_ArrivalDate = ZDateTime.Now.AddMonths(-6);
			var existingHawb = existingMawb.ChildBills.AddNew();
			existingHawb.CS_HAWB = "87654321";
			existingHawb.CS_GoodsDescription = "Existing House";
			Factory.Save();

			RunProcessors();

			var mawb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "99999999999"));
			AssertNotNull(mawb);
			AssertNotNull(mawb.ChildBills);
			AssertEquals(1, mawb.ChildBills.Count);
			AssertEquals("EXISTING HOUSE", mawb.ChildBills[0].CS_GoodsDescription);
			mawb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "80112345678"));
			AssertNotNull(mawb);
			AssertNotNull(mawb.ChildBills);
			AssertEquals(1, mawb.ChildBills.Count);
			AssertEquals("COLUMBIAN FLOUR", mawb.ChildBills[0].CS_GoodsDescription);
		}

		public void TestInsertSecondBasicFromFriInDifferentShed_NoConsol()
		{
			var firstBasicInAnotherShed = Factory.New<CusMAWB>();
			firstBasicInAnotherShed.CM_MAWB = "80112345678";  // same as in message
			firstBasicInAnotherShed.CargoTerminalOperator = "LSM";  // different to message
			firstBasicInAnotherShed.CargoTerminalOperatorAirport = "MAN";
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();
			var mawbs = new BusinessObjectFactory().Load<CusMAWB>(new ZQuery());
			var mawbThatAlreadyExisted = (from CusMAWB m in mawbs where m.CargoTerminalOperator == "LSM" select m).First();
			var mawbThatWeJustCreated = (from CusMAWB m in mawbs where m.CargoTerminalOperator == "KLM" select m).First();
			AssertNotEquals("Should have two wholly different mawbs", mawbThatAlreadyExisted.PK, mawbThatWeJustCreated.PK);
		}

		public void TestInsertSecondBasicFromFriInDifferentShed_AttachedToConsol()
		{
			var firstBasicInAnotherShed = Factory.New<CusMAWB>();
			firstBasicInAnotherShed.CM_MAWB = "irrelevant";  // we seek consol by JK_MasterBillNum and then CusMAWB from foreign key
			firstBasicInAnotherShed.CargoTerminalOperator = "LSM";  // different to message
			firstBasicInAnotherShed.CargoTerminalOperatorAirport = "MAN";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "80112345678";
			firstBasicInAnotherShed.CM_JK = consol.PK;
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();
			var mawbs = new BusinessObjectFactory().Load<CusMAWB>(new ZQuery());
			var mawbThatAlreadyExisted = (from CusMAWB m in mawbs where m.CargoTerminalOperator == "LSM" select m).First();
			var mawbThatWeJustCreated = (from CusMAWB m in mawbs where m.CargoTerminalOperator == "KLM" select m).First();
			AssertNotEquals("Should have two wholly different mawbs", mawbThatAlreadyExisted.PK, mawbThatWeJustCreated.PK);
			AssertEquals("Both Mawbs are hanging from consol", consol.PK, mawbThatAlreadyExisted.CM_JK);
			AssertEquals("Both Mawbs are hanging from consol", consol.PK, mawbThatWeJustCreated.CM_JK);
		}

		public void TestFRIMasterNothingAlreadyExists()
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "");
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");
			Factory.Save();

			RunProcessors();
			var mawb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery());
			AssertNotNull(mawb);
			AssertEquals((ZShort)68, mawb.NumberOfPiecesReceived);
			AssertEquals((ZShort)15, mawb.NumberOfPiecesExpected);
			AssertEquals(3000m, mawb.Weight);
			AssertEquals("KG", mawb.WeightCode);
			AssertEquals("MAN", mawb.AirportOfArrival);
			AssertEquals("KLM", mawb.CargoTerminalOperator);
			AssertEquals("80112345678", mawb.CM_MAWB);
			AssertEquals("T", mawb.ShipmentDescriptionCode);
			AssertEquals("BA112", mawb.CM_FlightNo);
			AssertEquals(new ZDate(1987, 12, 11), mawb.CM_ArrivalDate);
			AssertEquals("80112345678", mawb.CM_MAWB);
			AssertEquals("LXA", mawb.AgentBadge);
			AssertEquals("COLUMBIAN FLOUR", mawb.DescriptionOfGoods);
			message.Reload();
			AssertEquals("FRI", message.EM_MessageSubType);
			AssertEquals("CAR", message.EM_MessageType);
			AssertContains("Record inserted using community data", message.EM_MessageInterpretation);
			AssertContains("<td>Master bill number</td><td>80112345678</td>", message.EM_MessageInterpretation);
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("CCS-UK Basic Air Waybill 801-12345678 created using community data", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains(">CCS-UK Basic Air Waybill 801-12345678</a>", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertEquals(ZDateTime.Empty, mawb.Status1Date);
		}

		public void TestFRIHouseNothingAlreadyExists()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZ9";
			var staffInGroup = group.Staff.AddNew();
			staffInGroup.GS_EmailAddress = "foo@bar.com";
			staffInGroup.GS_Code = "DAN";
			staffInGroup.GS_LoginName = "DAN";
			Factory.Save();
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");
			Factory.Save();

			RunProcessors();
			var mawb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery());
			var hawb = mawb.ChildBills[0];
			foreach (var awb in new ICcsukCusAwb[] { hawb, mawb })
			{
				AssertNotNull(awb);
				AssertEquals(68, (int)awb.NumberOfPiecesReceived);
				AssertEquals(15, (int)awb.NumberOfPiecesExpected);
				AssertEquals(3000m, awb.Weight);
				AssertEquals("KG", awb.WeightCode);
				AssertEquals("KLM", awb.CargoTerminalOperator);
				AssertEquals("801-12345678", awb.MasterBill);
				AssertEquals("T", awb.ShipmentDescriptionCode);
				AssertEquals("LXA", awb.AgentBadge);
				AssertEquals("COLUMBIAN FLOUR", awb.DescriptionOfGoods);
				AssertEquals("MAN", awb.AirportOfArrival);
				AssertEquals("USLAX", awb.AirportOfOrigin);
				AssertEquals("LHR", awb.AirportOfDestination);
				AssertEquals("CUKFFW98000AAA", awb.Profile);
			}
			AssertEquals("87654321", hawb.CS_HAWB);
			AssertEquals("BA112", mawb.CM_FlightNo);

			AssertEquals(new ZDateTime(1987, 12, 11), mawb.CM_ArrivalDate);
			message.Reload();
			AssertEquals("FRI", message.EM_MessageSubType);
			AssertEquals("CAR", message.EM_MessageType);
			AssertContains("Record inserted using community data</h3><h5>MAWB MANKLM 80112345678 created", message.EM_MessageInterpretation);
			AssertContains("<td>HAWB</td><td>87654321</td>", message.EM_MessageInterpretation);
			AssertEquals("RCV", message.EM_Status);
			AssertContains("Created new master 80112345678", log[0]);
			AssertContains("Created new house bill 87654321 on master 80112345678", log[1]);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);
			AssertEquals("foo@bar.com", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0]);
			AssertEquals("CCS-UK House Bill 801-12345678-87654321 created using community data", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestFRIHouseInterestingPorts()
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000AAA");

			var locLineOld = "LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'";
			var locLineNew = "LOC+11:STN:145:3::KLM:129:ZZZ+84:XAT:145:3+85:MAN:145:3'";
			AssertContains("PreReq", locLineOld, friExampleArrivedHouse);
			var interestingFriInbound = friExampleArrivedHouse.Replace(System.Environment.NewLine, "").Replace(locLineOld, locLineNew);
			message.EM_MessageText = interestingFriInbound;
			Factory.Save();

			RunProcessors();
			var mawb = new BusinessObjectFactory().LoadTop1<CusMAWB>(new ZQuery());
			var hawb = mawb.ChildBills[0];
			AssertNotNull(mawb);
			AssertNotNull(hawb);
			AssertEquals(68, (int)hawb.CS_PiecesLanded);
			AssertEquals(15, (int)hawb.CS_PiecesManifested);
			AssertEquals(3000m, hawb.CS_Weight);
			AssertEquals("KG", hawb.CS_WeightUQ);
			AssertEquals("KLM", hawb.CargoTerminalOperator);
			AssertEquals("80112345678", mawb.CM_MAWB);
			AssertEquals("T", hawb.ShipmentDescriptionCode);
			AssertEquals("T", mawb.ShipmentDescriptionCode);
			AssertEquals("87654321", hawb.CS_HAWB);
			AssertEquals("LXA", hawb.AgentBadge);
			AssertEquals("COLUMBIAN FLOUR", hawb.CS_GoodsDescription);
			AssertEquals("STN", hawb.AirportOfArrival);
			AssertEquals("FRANT", hawb.AirportOfOrigin);
			AssertEquals("MAN", hawb.AirportOfDestination);
			message.Reload();
			AssertEquals("FRI", message.EM_MessageSubType);
			AssertEquals("CAR", message.EM_MessageType);
			AssertContains("Record inserted using community data</h3><h5>MAWB STNKLM 80112345678 created", message.EM_MessageInterpretation);
			AssertContains("<td>HAWB</td><td>87654321</td>", message.EM_MessageInterpretation);
			AssertEquals("RCV", message.EM_Status);
			AssertContains("Created new master 80112345678", log[0]);
			AssertContains("Created new house bill 87654321 on master 80112345678", log[1]);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);
		}

		public void TestFRIHouseMawbAlreadyExistsButNoConsol()
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "80112345678";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "MANKLM";
			mawb.CM_ArrivalDate = ZDateTime.Now.AddMonths(-6);
			Factory.Save();

			RunProcessors();
			mawb.Reload();
			mawb.ChildBills.Load();
			var hawb = mawb.ChildBills[0];
			AssertNotNull(hawb);
			AssertEquals("80112345678", mawb.CM_MAWB);
			AssertEquals("87654321", hawb.CS_HAWB);
			message.Reload();
			AssertContains("Record inserted using community data</h3><table", message.EM_MessageInterpretation);
			AssertContains("<td>HAWB</td><td>87654321</td>", message.EM_MessageInterpretation);
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("No new mawb created", 1, Factory.Load<CusMAWB>(new ZQuery()).Length);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);
			AssertEquals("Setting hawb's presence to yes must force mawb's to be yes too.  Cannot have orphaned hawb.", PresenceOnNetworkList.Codes.OnCommDb, mawb.PresenceOnNetworkStatus);
		}

		public void TestFRIHousePimaNotUpdatedIfHawbAlreadyExists()
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			message.EM_EI = MakeInterchangeJustForPima("CUKFFW98000ZZZ");
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "80112345678";
			mawb.Profile = "CUKFFW98000AAA";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = "MANKLM";
			mawb.CM_ArrivalDate = ZDateTime.Now.AddMonths(-6);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			Factory.Save();

			RunProcessors();
			hawb.Reload();
			AssertEquals("CUKFFW98000AAA", hawb.Profile);
		}

		void SetupForLockedMutexTest(out ForwardingConsol consol, out EDIMessage message, string messageText)
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00069";
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBXXX";
			consol.JK_MasterBillNum = "80112345678";
			var mockMessage = Factory.NewMoq<EDIMessage>();
			message = mockMessage.Object;
			message.EM_MessageNum = "123";
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = messageText;
			message.Logs.AddNew(Events.ExceptionRaised, "Exception [ fake exception 1 to force processor to fail");
			message.Logs.AddNew(Events.ExceptionRaised, "Exception [ fake exception 2 to force processor to fail");
			message.Logs.AddNew(Events.ExceptionRaised, "Exception [ fake exception 3 to force processor to fail");
			message.Logs.AddNew(Events.ExceptionRaised, "Exception [ fake exception 4 to force processor to fail");
			message.Logs.AddNew(Events.ExceptionRaised, "Exception [ fake exception 5 to force processor to fail");
		}

		public void TestFRIMasterLockedMutex()
		{
			ForwardingConsol consol;
			EDIMessage message;
			SetupForLockedMutexTest(out consol, out message, CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, ""));
			Factory.Save();

			using (var mutex = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedKingdom))
			{
				mutex.Lock();
				RunProcessors();
			}
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("", ErrorReporter.LastMessageReported);
			var mawbCreated = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "80112345678"));
			AssertNotNull("Mawb was created even if consol was locked", mawbCreated);
			AssertEquals("No consol link", ZGuid.Empty, mawbCreated.CM_JK);
			AssertEquals("Created at right location", "MANKLM", mawbCreated.MasterLevelHouseHelper.CS_WarehouseLocation);
			AssertContains($"{BrandingFactory.Instance.ProductName} could not lock the consol (C00069) for this mawb", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestFRIHouseLockedMutex()
		{
			ForwardingConsol consol;
			EDIMessage message;
			SetupForLockedMutexTest(out consol, out message, CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "").Replace("HWB:M", "HWB:DANIEL01"));
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = "GBLHR";
			shipment.JS_HouseBill = "DANIEL01";
			shipment.JS_UniqueConsignRef = "S000001";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			Factory.Save();

			using (var mutex = CusHAWB.CreateMutexForShipment(shipment.PK, Core.Constants.CountryCodes.UnitedKingdom))
			{
				mutex.Lock();
				Factory.Save();
				RunProcessors();
			}
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("", ErrorReporter.LastMessageReported);
			var hawbCreated = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "DANIEL01"));
			AssertNotNull("Hawb was created even if shipment was locked", hawbCreated);
			AssertEquals("No shipment link", ZGuid.Empty, hawbCreated.CS_JS);
			AssertEquals("Created at right location", "MANKLM", hawbCreated.CS_WarehouseLocation);
			AssertContains($"{BrandingFactory.Instance.ProductName} could not lock the shipment (S000001) for this hawb", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestFRIMasterConsolNotExistsMawbExistsMultipleLocations()
		{
			RunTestFRyMasterConsolNotExistsMawbExistsMultipleLocations("FRI");
		}

		public void TestFRCMasterConsolNotExistsMawbExistsMultipleLocations()
		{
			RunTestFRyMasterConsolNotExistsMawbExistsMultipleLocations("FRC");
		}

		void RunTestFRyMasterConsolNotExistsMawbExistsMultipleLocations(string messageCode)  // where FRy = FRI or FRC
		{
			var cusMawbShedOne = Factory.New<CusMAWB>();
			cusMawbShedOne.CM_MAWB = "80112345678";
			cusMawbShedOne.MasterLevelHouseHelper.CS_WarehouseLocation = "MANBAC";
			var cusMawbShedTwo = Factory.New<CusMAWB>();
			cusMawbShedTwo.CM_MAWB = "80112345678";
			cusMawbShedTwo.MasterLevelHouseHelper.CS_WarehouseLocation = "MANKLM";
			var cusMawbShedThree = Factory.New<CusMAWB>();
			cusMawbShedThree.CM_MAWB = "80112345678";
			cusMawbShedThree.MasterLevelHouseHelper.CS_WarehouseLocation = "MANSLS";
			Factory.Save();
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "").Replace("FRI", messageCode);
			Factory.Save();

			RunProcessors();
			var newFactory = new BusinessObjectFactory();
			cusMawbShedOne = newFactory.Load<CusMAWB>(cusMawbShedOne.PK);
			cusMawbShedTwo = newFactory.Load<CusMAWB>(cusMawbShedTwo.PK);
			cusMawbShedThree = newFactory.Load<CusMAWB>(cusMawbShedThree.PK);
			AssertEquals("", cusMawbShedOne.DescriptionOfGoods); // mawb is not touched
			AssertEquals("COLUMBIAN FLOUR", cusMawbShedTwo.DescriptionOfGoods); // mawb is updated
			AssertEquals("", cusMawbShedThree.DescriptionOfGoods); // mawb is not touched
			AssertEquals(0, cusMawbShedOne.Messages.Count); // mawb is not touched
			AssertEquals(1, cusMawbShedTwo.Messages.Count); // mawb is not touched
			AssertEquals(0, cusMawbShedThree.Messages.Count); // mawb is not touched
			AssertEquals("No new mawb record created, existing one is used", 3, Factory.Load<CusMAWB>(new ZQuery()).Length);
		}

		public void TestFRIMasterConsolExistsMawbExists()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBXXX";
			consol.JK_MasterBillNum = "80112345678";
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "80112345678";
			cusMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "MANKLM";
			cusMawb.CM_JK = consol.PK;
			Factory.Save();
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();
			cusMawb.MasterLevelHouseHelper.Reload();
			AssertEquals("COLUMBIAN FLOUR", cusMawb.DescriptionOfGoods); // mawb is updated
			AssertEquals("No new mawb record created, existing one is used", 1, Factory.Load<CusMAWB>(new ZQuery()).Length);
			var mutex = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("Lock not applied", false, mutex.HasLock);
		}

		public void TestFRIMasterConsolExistsNoMawb()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBXXX";
			consol.JK_MasterBillNum = "80112345678";
			Factory.Save();
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var cusMawb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("COLUMBIAN FLOUR", cusMawb.DescriptionOfGoods); // mawb is created
			var mutex = CusMAWB.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("Lock released", false, mutex.HasLock);
		}

		public void TestFRIMasterConsolExistsMawbExistsButMarkedDeleted()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBXXX";
			consol.JK_MasterBillNum = "80112345678";
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "80112345678";
			cusMawb.CM_JK = consol.PK;
			cusMawb.MasterLevelHouseHelper.CS_WarehouseLocation = "MANKLM";
			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			Factory.Save();
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = CUSCAR9122GeneratorTests.friExampleArrivedMaster.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();
			cusMawb.Reload();
			AssertEquals("mawb should be forced to present", PresenceOnNetworkList.Codes.OnCommDb, cusMawb.PresenceOnNetworkStatus);
			AssertEquals("No additional mawb should be inserted", 1, Factory.Load<CusMAWB>(new ZQuery()).Length);
		}

		void RunFRIHouseHawbAndMawbExistButNoShipmentTest(string airportAndShedOfExisting)
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "80112345678";
			mawb.MasterLevelHouseHelper.CS_WarehouseLocation = airportAndShedOfExisting;
			mawb.CM_ArrivalDate = ZDateTime.Now.AddMonths(-6);
			existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest = mawb.ChildBills.AddNew();
			existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest.CS_HAWB = "87654321";
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();
			existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest.Reload();
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
		}

		public void TestFRIHouseHawbAndMawbExistButNoShipment_MatchingShed()
		{
			RunFRIHouseHawbAndMawbExistButNoShipmentTest("MANKLM");
			AssertEquals("Existing house is updated with inbound data", "COLUMBIAN FLOUR", existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest.CS_GoodsDescription);
			AssertEquals("No new hawb created", 1, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false)).Length);
		}

		public void TestFRIHouseHawbAndMawbExistButNoShipment_NoMatchingShed()
		{
			RunFRIHouseHawbAndMawbExistButNoShipmentTest("POOPIE");
			var q = new ZQuery(CusHAWBSchema.CS_HAWB, "87654321");
			q.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest.PK);
			var newlyInsertedHawb = Factory.LoadTop1<CusHAWB>(q);
			AssertEquals("Existing house NOT updated, mismatch on shed", "", existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest.CS_GoodsDescription);
			AssertEquals("New house is for columbian flour", "COLUMBIAN FLOUR", newlyInsertedHawb.CS_GoodsDescription);
		}

		CusHAWB existingHouseForRunFRIHouseHawbAndMawbExistButNoShipmentTest;

		public void TestFRIHouseShipmentExistsButNoHawb()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-6);
			shipment.JS_HouseBill = "87654321";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = "GBXXX";
			Factory.Save();

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawb = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals("COLUMBIAN FLOUR", hawb.CS_GoodsDescription);
			var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertEquals("80112345678", mawb.CM_MAWB);
			AssertEquals("Insert HAWB and make MAWB updates MAWB too", "LXA", mawb.AgentBadge);
			AssertEquals("Not linked to a shipment because the shipment had no consol", ZGuid.Empty, hawb.CS_JS);
			message.Reload();
			AssertEquals("RCV", message.EM_Status);
			var mutex = CusHAWB.CreateMutexForShipment(shipment.PK, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("Lock has been released", false, mutex.HasLock);
		}

		public void TestFRIHouseNoCushawbButTwoShipmentsWithSameHawbNumberOnlyOneOnRelevantConsol()
		{
			var consolGood = Factory.New<ForwardingConsol>();
			var consolBad = Factory.New<ForwardingConsol>();
			var shipmentBad = Factory.New<ForwardingShipment>();
			Factory.Save();
			var shipmentGood = Factory.New<ForwardingShipment>();
			consolGood.Shipments.Add(shipmentGood);
			consolBad.Shipments.Add(shipmentBad);
			shipmentGood.JS_HouseBill = "87654321";
			shipmentBad.JS_HouseBill = "87654321";
			shipmentGood.JS_RL_NKDestination = "GBLHR";
			shipmentBad.JS_RL_NKDestination = "GBLHR";
			shipmentGood.JS_TransportMode = "AIR";
			shipmentBad.JS_TransportMode = "AIR";
			consolGood.JK_TransportMode = "AIR";
			consolBad.JK_TransportMode = "AIR";
			consolGood.JK_RL_NKDischargePort = "GBLHR";
			consolBad.JK_RL_NKDischargePort = "GBLHR";

			// Good and bad jobs are identical so far, now they differ...
			consolGood.JK_MasterBillNum = "80112345678";
			consolBad.JK_MasterBillNum = "00000000000";
			Factory.Save();
			Assert("Pre-req: good shipment needs a higher reference than bad shipment, so that we select top one order by reference and get the higher-referenced shipment, we know our query is good and is excluding the lower-referenced (bad) shipment",
				shipmentGood.JS_UniqueConsignRef > shipmentBad.JS_UniqueConsignRef);

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals(1, hawbs.Length);
			AssertEquals("CusHAWB created against good shipment, even though both shipments could have been candidates.  Their consols distinguish them.", shipmentGood.PK, hawbs[0].CS_JS);
		}

		public void TestFRIHouseNoCushawbButTwoShipmentsWithSameHawbNumberAlsoSameConsol()
		{
			var shipmentBad = Factory.New<ForwardingShipment>();
			var shipmentGood = Factory.New<ForwardingShipment>();
			SetupHouseNoCushawbButTwoShipmentsWithSameHawbNumberAlsoSameConsol(shipmentGood, shipmentBad);

			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals(1, hawbs.Length);
			AssertEquals("CusHAWB created against good shipment, even though both shipments could have been candidates.  Their consols distinguish them.", shipmentGood.PK, hawbs[0].CS_JS);
			AssertNotContains("too many matching shipments were found.  A new Hawb record has been created but no shipment has been linked", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestFRIHouseNoCushawbButTwoShipmentsWithSameHawbNumberAlsoSameConsolWithTooManyMatchingShipments()
		{
			var shipmentBad = Factory.New<ForwardingShipment>();
			var shipmentGood = Factory.New<ForwardingShipment>();
			SetupHouseNoCushawbButTwoShipmentsWithSameHawbNumberAlsoSameConsol(shipmentGood, shipmentBad, false);

			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals(1, hawbs.Length);
			AssertContains("When processing an incoming record from CCSUK, too many matching shipments were found.  A new Hawb record has been created but no shipment has been linked to it for this reason", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			AssertContains("too many matching shipments were found.  A new Hawb record has been created but no shipment has been linked", Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertEquals("CusHAWB created but too many matching shipments were found. No shipment has been linked", Guid.Empty, hawbs[0].CS_JS);
		}

		void SetupHouseNoCushawbButTwoShipmentsWithSameHawbNumberAlsoSameConsol(ForwardingShipment shipmentGood, ForwardingShipment shipmentBad, bool makeShipmentsDifferent = true)
		{
			var consolGood = Factory.New<ForwardingConsol>();
			var consolBad = Factory.New<ForwardingConsol>();
			Factory.Save();
			consolGood.Shipments.Add(shipmentGood);
			consolBad.Shipments.Add(shipmentBad);
			shipmentGood.JS_HouseBill = "87654321";
			shipmentBad.JS_HouseBill = "87654321";
			shipmentGood.JS_RL_NKDestination = "GBLHR";
			shipmentBad.JS_RL_NKDestination = "GBLHR";
			shipmentGood.JS_RL_NKOrigin = "USLAX";
			shipmentBad.JS_RL_NKOrigin = "USLAX";
			shipmentGood.JS_TransportMode = "AIR";
			shipmentBad.JS_TransportMode = "AIR";
			consolGood.JK_TransportMode = "AIR";
			consolBad.JK_TransportMode = "AIR";
			consolGood.JK_RL_NKDischargePort = "GBLHR";
			consolBad.JK_RL_NKDischargePort = "GBLHR";
			consolGood.JK_MasterBillNum = "80112345678";
			consolBad.JK_MasterBillNum = "80112345678";
			consolGood.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolBad.JK_AgentType = Core.Constants.AgentType.CoLoad;

			if (makeShipmentsDifferent)
			{
				// Good and bad jobs are identical so far, now they differ...
				consolBad.JK_MasterBillIssueDate = ZDateTime.Now.AddMonths(-13);
				consolGood.JK_MasterBillIssueDate = ZDateTime.Now.AddMonths(-6);
				Factory.Save();
				Assert("Pre-req: good shipment needs a higher reference than bad shipment, so that we select top one order by reference and get the higher-referenced shipment, we know our query is good and is excluding the lower-referenced (bad) shipment",
					shipmentGood.JS_UniqueConsignRef > shipmentBad.JS_UniqueConsignRef);
			}
		}

		public void TestFRIHouseNoCushawbButTwoShipmentsWithSameHawbNumberOnlyOneFromRightOrigin()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipmentBad = Factory.New<ForwardingShipment>();
			Factory.Save();
			var shipmentGood = Factory.New<ForwardingShipment>();
			shipmentGood.JS_HouseBill = "87654321";
			shipmentBad.JS_HouseBill = "87654321";
			shipmentGood.JS_RL_NKDestination = "GBLHR";
			shipmentBad.JS_RL_NKDestination = "GBLHR";
			shipmentGood.JS_RL_NKOrigin = "USLAX";
			shipmentBad.JS_RL_NKOrigin = "USATL"; // NB
			shipmentGood.JS_TransportMode = "AIR";
			shipmentBad.JS_TransportMode = "AIR";
			consol.Shipments.Add(shipmentBad);
			consol.Shipments.Add(shipmentGood);
			consol.JK_MasterBillNum = "80112345678";

			Factory.Save();
			Assert("Pre-req: good shipment needs a higher reference than bad shipment, so that we select top one order by reference and get the higher-referenced shipment, we know our query is good and is excluding the lower-referenced (bad) shipment",
				shipmentGood.JS_UniqueConsignRef > shipmentBad.JS_UniqueConsignRef);

			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals(1, hawbs.Length);
			AssertEquals("CusHAWB created against good shipment, even though both shipments could have been candidates.  Their origins distinguish them.", shipmentGood.PK, hawbs[0].CS_JS);
		}

		public void TestFRIHouseNoCushawbButTwoShipmentsWithSameHawbNumberOnlyOneInConsolidation()
		{
			var consolGood = Factory.New<ForwardingConsol>();
			var shipmentBad = Factory.New<ForwardingShipment>();
			Factory.Save();
			var shipmentGood = Factory.New<ForwardingShipment>();
			consolGood.Shipments.Add(shipmentGood);
			shipmentGood.JS_HouseBill = "87654321";
			shipmentBad.JS_HouseBill = "87654321";
			shipmentGood.JS_RL_NKDestination = "GBLHR";
			shipmentBad.JS_RL_NKDestination = "GBLHR";
			shipmentGood.JS_TransportMode = "AIR";
			shipmentBad.JS_TransportMode = "AIR";
			consolGood.JK_TransportMode = "AIR";
			consolGood.JK_RL_NKDischargePort = "GBLHR";

			// Good and bad jobs are identical so far, now they differ...
			consolGood.JK_MasterBillNum = "80112345678";
			Factory.Save();
			Assert("Pre-req: good shipment needs a higher reference than bad shipment, so that we select top one order by reference and get the higher-referenced shipment, we know our query is good and is excluding the lower-referenced (bad) shipment",
				shipmentGood.JS_UniqueConsignRef > shipmentBad.JS_UniqueConsignRef);

			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals(1, hawbs.Length);
			AssertEquals("CusHAWB created against good shipment, even though both shipments could have been candidates.  Their consols distinguish them.", shipmentGood.PK, hawbs[0].CS_JS);
		}

		public void TestFRIHouseNoCushawbButTwoShipmentsWithSameHawbNumberButOneNotBoundForGB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "80112345678";
			var shipmentBad = Factory.New<ForwardingShipment>();
			Factory.Save();
			var shipmentGood = Factory.New<ForwardingShipment>();
			shipmentGood.JS_HouseBill = "87654321";
			shipmentBad.JS_HouseBill = "87654321";
			shipmentGood.JS_RL_NKDestination = "GBLHR";
			shipmentBad.JS_RL_NKDestination = "FRPAR";  // NB
			shipmentGood.JS_TransportMode = "AIR";
			shipmentBad.JS_TransportMode = "AIR";
			consol.Shipments.Add(shipmentBad);
			consol.Shipments.Add(shipmentGood);

			// Good and bad jobs are identical so far, now they differ...
			Factory.Save();
			Assert("Pre-req: good shipment needs a higher reference than bad shipment, so that we select top one order by reference and get the higher-referenced shipment, we know our query is good and is excluding the lower-referenced (bad) shipment",
				shipmentGood.JS_UniqueConsignRef > shipmentBad.JS_UniqueConsignRef);

			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = friExampleArrivedHouse.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false));
			AssertEquals(1, hawbs.Length);
			AssertEquals("CusHAWB created against good shipment, bad shipment is Froggie.", shipmentGood.PK, hawbs[0].CS_JS);
		}

		public void TestFrcHawbForForwardingLinkedInterShededHouse()
		{
			RunTestFrcHawbForForwardingLinkedInterShededHouse(friExampleArrivedHouse.Replace("FRI", "FRC"));
		}

		public void TestFriHawbForForwardingLinkedInterShededHouse()
		{
			RunTestFrcHawbForForwardingLinkedInterShededHouse(friExampleArrivedHouse);
		}

		void RunTestFrcHawbForForwardingLinkedInterShededHouse(string messageText)
		{
			// Consol, shipment, mawb, hawb.  Hawb has status 3 (ISR).
			// Then receive an FRI/FRC for the hawb at the new shed. Should create a new hawb on the shipment, not just blindly update them first.
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLON";
			consol.JK_MasterBillNum = "80112345678";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_HouseBill = "87654321";
			shipment.JS_RL_NKDestination = "GBLHR";
			shipment.JS_TransportMode = "AIR";
			consol.Shipments.Add(shipment);
			var mawbAtShedOne = Factory.New<CusMAWB>();
			mawbAtShedOne.CM_MAWB = consol.JK_MasterBillNum;
			mawbAtShedOne.MasterLevelHouseHelper.CS_WarehouseLocation = "LHRBAC";
			var hawbAtShedOne = mawbAtShedOne.ChildBills.AddNew();
			hawbAtShedOne.CS_HAWB = shipment.JS_HouseBill;
			hawbAtShedOne.CS_JS = shipment.PK;
			hawbAtShedOne.SetCustomsActionCode("CB", ZDateTime.Now);
			Factory.Save();
			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = messageText.Replace(System.Environment.NewLine, "");
			Factory.Save();

			RunProcessors();

			var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "87654321"));
			var mawbs = Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "80112345678"));

			AssertEquals(2, hawbs.Length);
			AssertEquals(2, mawbs.Length);
			AssertEquals("CusHAWB created against shipment.", shipment.PK, hawbs[1].CS_JS);
			AssertEquals("CusMAWB created against consol.", consol.PK, mawbs[1].CM_JK);
			AssertEquals("New shed", "MANKLM", hawbs[1].CS_WarehouseLocation);
			AssertEquals("New shed", "MANKLM", mawbs[1].MasterLevelHouseHelper.CS_WarehouseLocation);
		}

		[TestDate(2013, 3, 12, 4, 27, 0)]
		public void TestProcessSeveralMessagesInOneGo()
		{
			// Simulates receiving an FRI and FRX at the same time and processing both in one cycle of the task; hopefully the FRX finds the FRI's job.
			// Real data, real scenario
			var fiveInterchanges = new ZString[]
				{
					"UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRI2C55YEHKWV0+CUKFFW98000DBC:IATA+130421:0808+NRI2C55YEHKWV0'UNH+NRI2C55YEHKWV0+CUSCAR:1:912:UN'BGM+:::FRI+03747579490+97:1304210908:201'GIS+T:121'TDT+20+730++++US:172:3++178:130421:101'LOC+11:LHR:145:3::UBS:129:ZZZ+84:CLT:145:3+85:LHR:145:3'NAD+CB+DBC'GID+0'FTX+AAA+++CONSOL'QTY+118:2'MEA+WT++KGM:105'UNT+11+NRI2C55YEHKWV0'UNZ+1+NRI2C55YEHKWV0'",  // insert mawb
					"UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRI2C55YEHP0L0+CUKFFW98000DBC:IATA+130421:0808+NRI2C55YEHP0L0'UNH+NRI2C55YEHP0L0+CUSCAR:1:912:UN'BGM+:::FRI+03747579490+97:1304210908:201++HWB:41312420'GIS+S2Y'GIS+T:121'TDT+20+730++++US:172:3++178:130421:101'LOC+11:LHR:145:3::UBS:129:ZZZ+84:CLT:145:3+85:LHR:145:3'NAD+CB+DBC'GID+0'FTX+AAA+++FABIC'QTY+118:2'MEA+WT++KGM:105'UNT+12+NRI2C55YEHP0L0'UNZ+1+NRI2C55YEHP0L0'",  // insert hawb
					"UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRX0C55YHXRH10+CUKFFW98000DBC:IATA+130421:0812+NRX0C55YHXRH10'UNH+NRX0C55YHXRH10+CUSCAR:1:912:UN'BGM+:::FRX+03747579490+++HWB:41312420'TDT+20'LOC+11:LHR:145:3::UBS:129:ZZZ'UNT+5+NRX0C55YHXRH10'UNZ+1+NRX0C55YHXRH10'",	// delete hawb (should delete mawb too)
					"UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRI2C55YHXVIW0+CUKFFW98000DBC:IATA+130421:0812+NRI2C55YHXVIW0'UNH+NRI2C55YHXVIW0+CUSCAR:1:912:UN'BGM+:::FRI+03747579490+97:1304210912:201'GIS+T:121'TDT+20+730++++US:172:3++178:130421:101'LOC+11:LHR:145:3::UBS:129:ZZZ+84:CLT:145:3+85:LHR:145:3'NAD+CB+DBC'GID+0'FTX+AAA+++CONSOL'QTY+118:2'MEA+WT++KGM:105'UNT+11+NRI2C55YHXVIW0'UNZ+1+NRI2C55YHXVIW0'", // re-insert mawb
					"UNB+UNOA:2+CUKSYS98COMMDB:IATA:NRI2C55YHY00C0+CUKFFW98000DBC:IATA+130421:0812+NRI2C55YHY00C0'UNH+NRI2C55YHY00C0+CUSCAR:1:912:UN'BGM+:::FRI+03747579490+97:1304210912:201++HWB:31242001'GIS+S2Y'GIS+T:121'TDT+20+730++++US:172:3++178:130421:101'LOC+11:LHR:145:3::UBS:129:ZZZ+84:CLT:145:3+85:LHR:145:3'NAD+CB+DBC'GID+0'FTX+AAA+++FABIC'QTY+118:2'MEA+WT++KGM:105'UNT+12+NRI2C55YHY00C0'UNZ+1+NRI2C55YHY00C0'"  // insert new hawb
				};
			foreach (var interchange in fiveInterchanges)
			{
				EDIInterchange.CreateNewInterchangeFromString(Factory, interchange, "CUK");
				Factory.Save();
			}
			RunProcessors();

			var newFactory = new BusinessObjectFactory();
			var mawbs = newFactory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "03747579490"));
			AssertEquals(1, mawbs.Length);
			AssertEquals(2, mawbs[0].ChildBills.Count);
			mawbs[0].ChildBills.Sort(CusHAWBSchema.CS_HAWB.Name);
			AssertEquals("31242001", mawbs[0].ChildBills[0].CS_HAWB);
			AssertEquals("41312420", mawbs[0].ChildBills[1].CS_HAWB);
			AssertEquals("YES", mawbs[0].ChildBills[0].PresenceOnNetworkStatus);
			AssertEquals("DEL", mawbs[0].ChildBills[1].PresenceOnNetworkStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_EmailAddress = "daniel@wisetechglobal.com";
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			var staffGroup = Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup);
			staffGroup.Staff.Add(currentUserInCurrentFactory);
			staffGroup.Staff.Add(anotherStaff);
			branchEnvironment = DisposableEnvironment.ForBranch(Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}

		internal const string gatwickPimaPrimary = "CUKAIR98LHRGGG";
		internal const string gatwickPimaIrrelevant = "CUKAIR98LHRYYY";
		internal const string heathrowPima = "CUKAIR98LHRHHH";

		internal static void CreateCredentials(out BadgeCodeSetting badgeHeathrow, out CredentialsSetting credentialHeathrow, out BadgeCodeSetting badgeGatwickPrimary,
			out CredentialsSetting credentialGatwickPrimary, out BadgeCodeSetting badgeGatwickIrrelevant, out CredentialsSetting credentialGatwickIrrelevant,
			GlbCompany company, GlbBranch heathrowBranch, GlbBranch gatwickBranch, BusinessObjectFactory factory)
		{
			badgeHeathrow = new BadgeCodeSetting();
			badgeHeathrow.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeHeathrow.BadgeCode = "DAN";
			credentialHeathrow = new CredentialsSetting();
			credentialHeathrow.BadgeCode = "DAN";
			credentialHeathrow.Company = "DAN";
			credentialHeathrow.PIMA = heathrowPima;

			badgeGatwickPrimary = new BadgeCodeSetting();
			badgeGatwickPrimary.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeGatwickPrimary.BadgeCode = "CLA";
			badgeGatwickPrimary.IsPrimaryBadgeForBranch = true;
			credentialGatwickPrimary = new CredentialsSetting();
			credentialGatwickPrimary.Company = "CLA";
			credentialGatwickPrimary.BadgeCode = "CLA";
			credentialGatwickPrimary.PIMA = gatwickPimaPrimary;

			badgeGatwickIrrelevant = new BadgeCodeSetting();
			badgeGatwickIrrelevant.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeGatwickIrrelevant.BadgeCode = "LSM";
			credentialGatwickIrrelevant = new CredentialsSetting();
			credentialGatwickIrrelevant.Company = "LSM";
			credentialGatwickIrrelevant.BadgeCode = "LSM";
			credentialGatwickIrrelevant.PIMA = gatwickPimaIrrelevant;

			var heathrowBadges = new BadgeCodeSettingCollection();
			heathrowBadges.Add(badgeHeathrow);
			var gatwickBadges = new BadgeCodeSettingCollection();
			gatwickBadges.Add(badgeGatwickPrimary);
			gatwickBadges.Add(badgeGatwickIrrelevant);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, heathrowBranch.PK.ToGuid(), Guid.Empty, heathrowBadges);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, gatwickBranch.PK.ToGuid(), Guid.Empty, gatwickBadges);
			factory.Save();
			var allCredentials = new CredentialsSettingCollection();
			allCredentials.Add(credentialHeathrow);
			allCredentials.Add(credentialGatwickPrimary);
			allCredentials.Add(credentialGatwickIrrelevant);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, allCredentials);
			factory.Save();
		}

		internal static void CreateThreeBranches(out GlbCompany company, out GlbBranch heathrowBranch, out GlbBranch gatwickBranch, out GlbBranch crappyBranchForServiceTask, BusinessObjectFactory factory)
		{
			company = factory.New<GlbCompany>();
			company.GC_Code = "XXX";
			company.GC_RN_NKCountryCode = "GB";
			heathrowBranch = company.Branches.AddNew();
			heathrowBranch.GB_Code = "LHR";
			heathrowBranch.GB_BranchName = "London Heathrow";
			heathrowBranch.GB_RL_NKHomePort = "GBLHR";
			gatwickBranch = company.Branches.AddNew();
			gatwickBranch.GB_Code = "LGW";
			gatwickBranch.GB_BranchName = "London Gatwick";
			gatwickBranch.GB_RL_NKHomePort = "GBLGW";
			crappyBranchForServiceTask = company.Branches.AddNew();
			crappyBranchForServiceTask.GB_Code = "MAN";  // The branch as which the service tasks run
			crappyBranchForServiceTask.GB_BranchName = "Crappy Branch";
			crappyBranchForServiceTask.GB_RL_NKHomePort = "GBMAN";
			factory.Save();
		}

		readonly string friExampleArrivedHouse = @"
UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:2:912:UN:109502+<<SYSCAR>>'
BGM+:::FRI+80112345678+++HWB:87654321'
GIS+S2Y'
GIS+T:121'
TDT+20+112++++BA:172:3++178:871211:101'
LOC+11:MAN:145:3::KLM:129:ZZZ+84:LAX:145:3+85:LHR:145:3'
NAD+CB+LXA'
GID+0'
FTX+AAA+++COLUMBIAN FLOUR'
QTY+118:15'
QTY+48:68'
MEA+WT++KGM:3000'
UNT+13+<<MSGNO PLACEHOLDER>>'";
	}
}
