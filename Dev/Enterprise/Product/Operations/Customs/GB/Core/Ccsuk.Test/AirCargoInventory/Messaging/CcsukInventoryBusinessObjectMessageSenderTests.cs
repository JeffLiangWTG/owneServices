using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public class CcsukInventoryBusinessObjectMessageSenderTests : TestCaseWithFactory
	{
		// NB the EDIFACT content of the cuscar message is tested in CUSCAR9122GeneratorTests.cs

		public void TestMakeCusdec()
		{
			MakeCcsukBadgeAndCredential();
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var sender = new CcsukInventoryICusAwbMessageSender();
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesLanded = 0;
			hawb.CS_PiecesManifested = 15;
			hawb.CS_RL_NKDischargePort = "MAN";
			var iar = hawb.IARs.AddNew();
			iar.LicenseRestrictionInd = "N";
			Factory.Save();
			var function = new CcsukTransmissionMessageFunction.CUSDEC.IAR();
			function.CusUnderbond = iar;
			sender.Send(hawb, new SendsMessagesToCustomsShutterUpperer(), function);
			AssertEquals(1, hawb.Messages.Count);
			var msg = hawb.Messages[0];
			AssertEquals("CDC", msg.EM_MessageType);
			AssertEquals("IAR", msg.EM_MessageSubType);
			AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, msg.EM_ApplicationReference);
			AssertEquals("CUKFFW98000LXA", msg.EM_MessageOwner);
			AssertContains("+CUSDEC:", msg.EM_MessageText);
			AssertEquals("CUK", msg.EM_ApplicationCode);
		}

		public void TestMakeCuscar_FRI()
		{
			MakeCcsukBadgeAndCredential();
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var sender = new CcsukInventoryICusAwbMessageSender();
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesLanded = 0;
			hawb.CS_PiecesManifested = 1;
			foreach (ICcsukCusAwb awb in new ICcsukCusAwb[] { mawb, hawb })
			{
				sender.Send(awb, new SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CUSCAR.FRI());
				AssertEquals(1, awb.Messages.Count);
				var msg = awb.Messages[0];
				AssertEquals("CAR", msg.EM_MessageType);
				AssertEquals("FRI", msg.EM_MessageSubType);
				AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, msg.EM_ApplicationReference);
				AssertEquals("CUKFFW98000LXA", msg.EM_MessageOwner);
				AssertContains("+CUSCAR:", msg.EM_MessageText);
				AssertEquals("CUK", msg.EM_ApplicationCode);
			}
		}

		public void TestMakeCuscar_FCS()
		{
			MakeCcsukBadgeAndCredential();
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var sender = new CcsukInventoryICusAwbMessageSender();
			var splits = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory);
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesLanded = 0;
			hawb.CS_PiecesManifested = 15;
			foreach (ICcsukCusAwb awb in new ICcsukCusAwb[] { mawb, hawb })
			{
				sender.Send(awb, new SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CUSCAR.FCS(splits.SplitLines));
				AssertEquals(1, awb.Messages.Count);
				var msg = awb.Messages[0];
				AssertEquals("CAR", msg.EM_MessageType);
				AssertEquals("FCS", msg.EM_MessageSubType);
				AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, msg.EM_ApplicationReference);
				AssertEquals("CUKFFW98000LXA", msg.EM_MessageOwner);
				AssertContains("+CUSCAR:", msg.EM_MessageText);
				AssertEquals("CUK", msg.EM_ApplicationCode);
			}
		}

		public void TestMakeCuscar_FRX()
		{
			MakeCcsukBadgeAndCredential();
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var sender = new CcsukInventoryICusAwbMessageSender();
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesLanded = 0;
			hawb.CS_PiecesManifested = 15;
			foreach (ICcsukCusAwb awb in new ICcsukCusAwb[] { mawb, hawb })
			{
				sender.Send(awb, new SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CUSCAR.FRX());
				AssertEquals(1, awb.Messages.Count);
				var msg = awb.Messages[0];
				AssertEquals("CAR", msg.EM_MessageType);
				AssertEquals("FRX", msg.EM_MessageSubType);
				AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, msg.EM_ApplicationReference);
				AssertEquals("CUKFFW98000LXA", msg.EM_MessageOwner);
				AssertContains("+CUSCAR:", msg.EM_MessageText);
				AssertEquals("CUK", msg.EM_ApplicationCode);
			}
		}

		public void TestMakeCuscar_FRC_FullShed()
		{
			MakeCcsukBadgeAndCredential("SHD", "CUKAIR98LHRSHD");
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.Profile = "CUKAIR98LHRSHD";
			var hawb = mawb.ChildBills[0];
			hawb.Profile = "CUKAIR98LHRSHD";
			hawb.CS_PiecesManifested = 1;
			FrcRunner(new ICcsukCusAwb[] { mawb, hawb });
		}

		public void TestMakeCuscar_FRC_AgentNotInFallback_Whole()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			mawb.Profile = "CUKAIR98LHRLXA";
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesLanded = 0;
			hawb.Profile = "CUKAIR98LHRLXA";
			FrcRunner(new ICcsukCusAwb[] { mawb, hawb }, "valid profile (PIMA)");
		}

		public void TestMakeCuscar_FRC_FallbackAgent_Split()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.Profile = "CUKAIR98LHRLXA";
			var splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "01";
			splitBasic.NumberOfPiecesExpected = 1;
			FrcRunner(new ICcsukCusAwb[] { splitBasic });
		}

		public void TestMakeCuscar_FRC_Agent_Split()
		{
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.Profile = "CUKFFW98000LXA";
			var splitBasic = basic.Splits.AddNew();
			splitBasic.SplitReference = "01";
			FrcRunner(new ICcsukCusAwb[] { splitBasic }, CUSCARGeneratorFRC.FRcNotAllowedForSimpleAgents);
		}

		public void TestMakeCuscar_FRC_Agent()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			mawb.Profile = "CUKFFW98000LXA";
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesLanded = 0;
			hawb.Profile = "CUKFFW98000LXA";
			hawb.CS_PiecesManifested = 15;
			FrcRunner(new ICcsukCusAwb[] { mawb, hawb }, CUSCARGeneratorFRC.FrcNotAllowedForSimpleAgentsOrAirlinesNotInFallback);
		}

		public void TestMakeCuscar_FRC_FallbackAgentInFallback()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.Profile = "CUKAIR98LHRLXA";
			var hawb = mawb.ChildBills[0];
			hawb.Profile = "CUKAIR98LHRLXA";
			mawb.NumberOfPiecesExpected = 1;
			hawb.CS_PiecesManifested = 1;
			FrcRunner(new ICcsukCusAwb[] { mawb, hawb });
		}

		public void Test_SendToCommunity_FRC_Validation_OnShedProfile()
		{
			MakeCcsukBadgeAndCredential("SHD", "CUKAIR98LHRSHD", true);

			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.Profile = "CUKAIR98LHRSHD";
			mawb.CM_FlightNo = ""; 

			var hawb = mawb.ChildBills[0];
			hawb.Profile = "CUKAIR98LHRSHD";
			hawb.CS_PiecesManifested = 1;

			Factory.Save();

			var expectedError =
								"It is not permitted to send this message using your ETSF profile when the bill lacks a flight number. This sending will be aborted.\r\n" +
								"Supply a flight number and re-try the sending, or if you do not have the flight number you may consider using your agent profile to create a pre-arrival record. \r\n" +
								"In doing the latter, it should be remembered that agent pre-arrival records are expunged (archived) if not arrived within 4 days.";

			FrcRunner(new ICcsukCusAwb[] { mawb }, expectedError);
		}

		void FrcRunner(ICcsukCusAwb[] awbsToTest, string expectedErrorMessage = null)
		{
			var sender = new CcsukInventoryICusAwbMessageSender();
			foreach (ICcsukCusAwb awb in awbsToTest)
			{
				var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
				sender.Send(awb, shutUp, new CcsukTransmissionMessageFunction.CUSCAR.FRC());
				if (expectedErrorMessage == null)
				{
					AssertEquals(1, awb.Messages.Count);
					var msg = awb.Messages[0];
					AssertEquals("CAR", msg.EM_MessageType);
					AssertEquals("FRC", msg.EM_MessageSubType);
					AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, msg.EM_ApplicationReference);
					AssertEquals(awb.Profile, msg.EM_MessageOwner);
					AssertContains("+CUSCAR:", msg.EM_MessageText);
					AssertEquals("CUK", msg.EM_ApplicationCode);
				}
				else
				{
					AssertEquals(0, awb.Messages.Count);
					AssertContains(expectedErrorMessage, shutUp.InvalidOperationText + shutUp.LastErrorsAsString);
				}
			}
		}

		public static void MakeCcsukBadgeAndCredential(string badge = "LXA", string fullPima = "CUKFFW98000LXA", bool appendToExistingCredsAndBadges = false)
		{
			var badges = appendToExistingCredsAndBadges ? GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty) : new BadgeCodeSettingCollection();
			var ccsukBadge = new BadgeCodeSetting();
			ccsukBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			ccsukBadge.BadgeCode = badge;
			badges.Add(ccsukBadge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var ccsukCred = new CredentialsSetting();
			ccsukCred.BadgeCode = ccsukBadge.BadgeCode;
			ccsukCred.Company = ccsukBadge.BadgeCode;
			ccsukCred.Printer = fullPima;
			CredentialsSettingCollection allCreds = appendToExistingCredsAndBadges ? GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) : new CredentialsSettingCollection();
			allCreds.Add(ccsukCred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
		}
	}
}
