using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondAIROUTManagerTest : CMRMessageManagerAbstractTest
	{
		public void TestStatusCalculators()
		{
			AssertEquals("Length", 1, Manager.StatusCalculators.Length);
			AssertEquals("StatusCalculators", typeof(CusUnderbondOutturnStatusCalculator), Manager.StatusCalculators[0].GetType());
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
		}

		public void TestBusinessObject()
		{
			AssertEquals(Underbond, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Outturn Report for: ", Manager.MessageFriendlyName);
		}

		public void TestGetMessages()
		{
			AssertEquals("Messages", Underbond.Messages, Manager.GetMessages(Underbond));
		}

		public void TestGetBuilder()
		{
			Underbond.LinkedObject = CTOMAWB;
			var builders = Manager.GetBuilder(Underbond);
			AssertEquals("Message Builder Type", typeof(AIROUTMessageBuilder), builders[0].GetType());
		}

		public void TestGetAmendmentManager()
		{
			AssertEquals("AmendmentManagerType", typeof(CusUnderbondAIROUTAmendmentGenerator), Manager.GetAmendmentManager(Underbond).GetType());
		}

		public void TestGetBuilderWithNoParent()
		{
			var builders = Manager.GetBuilder(Underbond);
			AssertEquals("MessageBuilderType", typeof(AIROUTMessageBuilder), builders[0].GetType());
		}

		public void TestSendingWithNoOutturns()
		{
			AssertEquals("precondition", 0, Underbond.Outturns.Count);
			var result = Manager.GetNotificationsForSendingAnOriginal();
			var originalNotificationCount = result.Count;
			Assert("notification", result.ContainsError("There must be at least one outturn."));

			Underbond.Outturns.AddNew();
			result = Manager.GetNotificationsForSendingAnOriginal();
			AssertEquals("no notification", originalNotificationCount - 1, result.Count);
		}

		public void TestSplitMessageWhenGreaterThanMaxLines()
		{
			AUCustomsDataRegistry.Instance.MaximumAirOutturnLinesPerMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "081-12345678";

			var hAWB1 = mawb.ChildBills.AddNew();
			hAWB1.CS_HAWB = "111";
			hAWB1.CS_GoodsDescription = "HAWB1";
			hAWB1.CS_PiecesManifested = 3;

			var hAWB2 = mawb.ChildBills.AddNew();
			hAWB2.CS_HAWB = "222";
			hAWB2.CS_GoodsDescription = "HAWB2";
			hAWB2.CS_PiecesManifested = 6;

			var underbond = mawb.Underbonds.AddNew();
			underbond.C4_FlightNo = "QF415";
			underbond.C4_MAWB = mawb.CM_MAWB;

			var outturn = underbond.Outturns.AddNew();
			outturn.Parent = hAWB1;
			outturn.C5_OuterPacks = hAWB1.CS_PiecesManifested;
			outturn.C5_PackagesOutturned = hAWB1.CS_PiecesManifested;
			outturn.C5_GoodsDescription = "Test max lines split - first original";

			var outturn2 = underbond.Outturns.AddNew();
			outturn2.Parent = hAWB2;
			outturn2.C5_OuterPacks = hAWB2.CS_PiecesManifested;
			outturn2.C5_PackagesOutturned = hAWB2.CS_PiecesManifested;
			outturn2.C5_GoodsDescription = "Should be in original message";

			var outturn3 = underbond.Outturns.AddNew();
			outturn3.Parent = hAWB2;
			outturn3.C5_OuterPacks = hAWB2.CS_PiecesManifested;
			outturn3.C5_PackagesOutturned = hAWB2.CS_PiecesManifested;
			outturn3.C5_GoodsDescription = "Should be in original message";

			var outturn4 = underbond.Outturns.AddNew();
			outturn4.Parent = hAWB2;
			outturn4.C5_OuterPacks = hAWB2.CS_PiecesManifested;
			outturn4.C5_PackagesOutturned = hAWB2.CS_PiecesManifested;
			outturn4.C5_GoodsDescription = "Should be in first split message";

			var outturn5 = underbond.Outturns.AddNew();
			outturn5.Parent = hAWB2;
			outturn5.C5_OuterPacks = hAWB2.CS_PiecesManifested;
			outturn5.C5_PackagesOutturned = hAWB2.CS_PiecesManifested;
			outturn5.C5_GoodsDescription = "Should be in first split message";

			var outturn6 = underbond.Outturns.AddNew();
			outturn6.Parent = hAWB2;
			outturn6.C5_OuterPacks = hAWB2.CS_PiecesManifested;
			outturn6.C5_PackagesOutturned = hAWB2.CS_PiecesManifested;
			outturn6.C5_GoodsDescription = "Should be in first split message";

			var outturn7 = underbond.Outturns.AddNew();
			outturn7.Parent = hAWB2;
			outturn7.C5_OuterPacks = hAWB2.CS_PiecesManifested;
			outturn7.C5_PackagesOutturned = hAWB2.CS_PiecesManifested;
			outturn7.C5_GoodsDescription = "Should be in second split message";

			IAirOutturnReportHeaderInformation headerInfo = new AirOutturnStandAloneReportHeader(underbond);

			ReportHeader.DateTimeOfOutturn = new ZDateTime(2012, 2, 22);
			ReportHeader.ResponsiblePartyID = "41065894724";
			ReportHeader.FlightNumber = "QF415";
			ReportHeader.EstablishmentID = "9920A";
			ReportHeader.EstimatedDateOfArrival = new ZDateTime(2012, 2, 22);
			ReportHeader.Lines = headerInfo.Lines;
			AssertEquals("Pre-condition: should be 7 lines", 7, headerInfo.Lines.Length);

			var builders = Manager.GetBuilder(underbond);
			AssertEquals("Should be 3 AirOutMessageBuilders returned", 3, builders.Length);

			var builderCount = 0;
			foreach (var builder in builders)
			{
				builderCount++;
				if (builderCount == 1)
				{
					AssertEquals("First message builder should have SetStatusToPending as false", false, builder.SetStatusToPending);
				}
				else
				{
					AssertEquals("SetStatusToPending should be on for all subsequent split messages", true, builder.SetStatusToPending);
				}

				AssertEquals("SetSplitMessageIdentifier should be true for all split messages", true, builder.SetSplitMessageIdentifier);
			}
		}

		public void TestCanSendOriginalWhenSplitMessageFailed()
		{
			Underbond.LinkedObject = CTOMAWB;
			AssertEquals("CanSendOriginal default", true, Manager.CanSendOriginal);

			Underbond.Logs.AddNew(Events.Cancelled, "Split Outturn message not wholly processed");
			AssertEquals("CanSendOriginal is not allowed when split message has failed", false, Manager.CanSendOriginal);
		}

		public void TestResetToOriginal()
		{
			Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			Underbond.Outturns.AddNew();
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
			Manager.ResetToOriginal();
			AssertEquals("GetStatus()", CMRBaseStatuses.Codes.NotSent, Manager.GetStatus());
			AssertEquals("Message Status", EDIMessage.Status.Discarded, Underbond.Messages[0].EM_Status);
			AssertEquals("Outurned date should be cleared when Outturn is reset to original", ZDate.Empty, Underbond.C4_Outurned);
		}

		protected override void GenerateOriginalMessageForSaving()
		{
			Underbond.LinkedObject = CTOMAWB;
			Manager.GenerateOriginalMessages(Manager.BusinessObject);
		}

		protected override void SetStatus(ZString status)
		{
			Underbond.OutturnStatus.Code = status;
		}

		protected override CMRMessageManager GetManager() => new CusUnderbondAIROUTManager(Underbond);

		CusUnderbond underbond;
		CusUnderbond Underbond => underbond ?? (underbond = Factory.New<CusUnderbond>());

		CTOCusMAWB ctoMAWB;
		CTOCusMAWB CTOMAWB => ctoMAWB ?? (ctoMAWB = Factory.New<CTOCusMAWB>());

		CusUnderbondAIROUTManager manager;
		new CusUnderbondAIROUTManager Manager => manager ?? (manager = (CusUnderbondAIROUTManager)GetManager());

		TestHelperAirOutturnReportHeaderInformation reportHeader;
		TestHelperAirOutturnReportHeaderInformation ReportHeader
		{
			get
			{
				if (reportHeader == null)
				{
					reportHeader = new TestHelperAirOutturnReportHeaderInformation();
					reportHeader.FlightNumber = "QF123";
				}
				return reportHeader;
			}
		}

		sealed class TestHelperAirOutturnReportHeaderInformation : IAirOutturnReportHeaderInformation
		{
			ZDateTime dateTimeOfOutturn;
			public ZDateTime DateTimeOfOutturn
			{
				get => dateTimeOfOutturn;
				set => dateTimeOfOutturn = value;
			}

			ZString flightNumber;
			public ZString FlightNumber
			{
				get => flightNumber;
				set => flightNumber = value;
			}

			ZDateTime estimatedDateOfArrival;
			public ZDateTime EstimatedDateOfArrival
			{
				get => estimatedDateOfArrival;
				set => estimatedDateOfArrival = value;
			}

			IAirOutturnReportLineInformation[] lines = Array.Empty<IAirOutturnReportLineInformation>();
			public IAirOutturnReportLineInformation[] Lines
			{
				get => lines;
				set => lines = value;
			}

			IAirOutturnReportLineInformation[] databaseLines = Array.Empty<IAirOutturnReportLineInformation>();
			public IAirOutturnReportLineInformation[] DatabaseLines
			{
				get => databaseLines;
				set => databaseLines = value;
			}

			ZString responsiblePartyID;
			public ZString ResponsiblePartyID
			{
				get => responsiblePartyID;
				set => responsiblePartyID = value;
			}

			ZString establishmentID;
			public ZString EstablishmentID
			{
				get => establishmentID;
				set => establishmentID = value;
			}
		}
	}
}
