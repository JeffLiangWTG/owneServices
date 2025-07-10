using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusMAWBBaseAbstractTest : Customs.Business.Testing.CusMAWBTest
	{
		public void TestCM_fUseAltPartShipModel()
		{
			Assert(!MAWB.CM_fUseAltPartShipModel);
			MAWB.CM_fUseAltPartShipModel = true;
			Assert(MAWB.CM_fUseAltPartShipModel);
		}

		public void TestDeferredScheduledMessagesEvent()
		{
			var logs = MAWB.GetLogs().GetAllLogs();
			AssertEquals("Pre-condition: logs count", 0, logs.Count);
			AssertEquals("HasDeferredScheduledMessageLog for AIRCR", false, MAWB.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoReportLogReference));
			AssertEquals("HasDeferredScheduledMessageLog for AIROUT", false, MAWB.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoOutturnLogReference));

			MAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			AssertEquals("1 DSM event should generated for AIRCR", 1, logs.Where(x => x.SL_Reference == CusMAWBBase.AirCargoReportLogReference && !x.SL_IsCancelled).Count());
			AssertEquals("HasDeferredScheduledMessageLog for AIRCR", true, MAWB.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoReportLogReference));
			AssertEquals("HasDeferredScheduledMessageLog for all type", true, MAWB.HasDeferredScheduledMessageLog());

			MAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage);
			AssertEquals("1 DSM event should generated for AIROUT", 1, logs.Where(x => x.SL_Reference == CusMAWBBase.AirCargoOutturnLogReference && !x.SL_IsCancelled).Count());
			AssertEquals("HasDeferredScheduledMessageLog for AIROUT", true, MAWB.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoOutturnLogReference));
			AssertEquals("HasDeferredScheduledMessageLog for all type", true, MAWB.HasDeferredScheduledMessageLog());

			MAWB.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoReportLogReference);
			AssertEquals("DSM event for AIRCR should be cancelled", 1, logs.Where(x => x.SL_Reference == CusMAWBBase.AirCargoReportLogReference && x.SL_IsCancelled).Count());
			AssertEquals("HasDeferredScheduledMessageLog for AIRCR", false, MAWB.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoReportLogReference));
			AssertEquals("HasDeferredScheduledMessageLog for all type", true, MAWB.HasDeferredScheduledMessageLog());

			MAWB.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoOutturnLogReference);
			AssertEquals("DSM event for AIROUT should be cancelled", 1, logs.Where(x => x.SL_Reference == CusMAWBBase.AirCargoOutturnLogReference && x.SL_IsCancelled).Count());
			AssertEquals("HasDeferredScheduledMessageLog for AIROUT", false, MAWB.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoOutturnLogReference));
			AssertEquals("HasDeferredScheduledMessageLog for all type", false, MAWB.HasDeferredScheduledMessageLog());

			MAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			MAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage);
			AssertEquals("HasDeferredScheduledMessageLog for all type", true, MAWB.HasDeferredScheduledMessageLog());

			MAWB.CancelDeferredScheduledMessageLogs();
			AssertEquals("HasDeferredScheduledMessageLog for all type", false, MAWB.HasDeferredScheduledMessageLog());
		}

		[TestDate(2012, 8, 2, 06, 15, 00)] // Thursday
		public void TestDeferredScheduledMessages()
		{
			var cbrLoco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUCBR");
			DateTime GetLocationDateTime() => cbrLoco.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()); // portCBR.LocationDateTime is cached so it will return the same time over and over
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 16, GetLocationDateTime().Hour);
			AssertEquals("Pre-condition - lateTimeframe", 4, AUCustomsDataRegistry.Instance.AirMandatoryLatestCargoReportingTimeframe.Value);

			MAWB.CM_RL_NKDischargePort = "AUCBR";

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 2, 22, 30, 0);
			AssertEquals("Immediate when Scheduled is within the 4 hour late window", ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 2, 23, 30, 0);
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 4 hour late window and CBR Time is in the White Window (6AM-7PM)", new ZDateTime(2012, 8, 2, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 2, 09, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 19, GetLocationDateTime().Hour);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (7PM-6AM)", new ZDateTime(2012, 8, 2, 19, 15, 0), MAWB.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 2, 19, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 5, GetLocationDateTime().Hour);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 3, 23, 30, 0);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (7PM-6AM)", new ZDateTime(2012, 8, 3, 05, 15, 0), MAWB.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 2, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 6, GetLocationDateTime().Hour);
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 4 hour late window and CBR Time is in the White Window (6AM-7PM)", new ZDateTime(2012, 8, 3, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 3, 09, 30, 0);
			AssertEquals("Immediate when Scheduled Time is within the 4 hour late window and CBR Time is in the White Window (6AM-7PM)", ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 3, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", DayOfWeek.Saturday, GetLocationDateTime().DayOfWeek);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 4, 09, 30, 0);
			AssertEquals("Immediate when within the 4 hour late window", ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 4, 23, 30, 0);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (SAT/SUN)", new ZDateTime(2012, 8, 4, 06, 15, 0), MAWB.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 4, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", DayOfWeek.Sunday, GetLocationDateTime().DayOfWeek);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 5, 09, 30, 0);
			AssertEquals("Immediate when within the 4 hour late window", ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 5, 23, 30, 0);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (SAT/SUN)", new ZDateTime(2012, 8, 5, 06, 15, 0), MAWB.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 5, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", DayOfWeek.Monday, GetLocationDateTime().DayOfWeek);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 6, 09, 30, 0);
			AssertEquals("Immediate when within the 4 hour late window", ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 6, 23, 30, 0);
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 4 hour late window and CBR Time is in the White Window (6AM-7PM MON-FRI)", new ZDateTime(2012, 8, 6, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
		}

		[TestDate(2012, 08, 09, 01, 09, 09)]
		public void TestDeferredScheduledMessagesDateTimeOnWeekday()
		{
			AssertEquals("Pre-condition-the test date attribute also returns the same value for UTC time, so this test date value should return 11:09:09AM for CBR",
						new ZDateTime(2012, 08, 09, 11, 09, 09), Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUCBR").LocationDateTime);
			MAWB.CM_RL_NKDischargePort = "AUPER"; //CBR is 2 hours later then PER

			// if current time + 4 hours > arrival then return empty (empty means run immediately)
			// if current time + 48 hours > arrival and current is weekend or out of hours then schedule
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 08, 09, 13, 08, 09);
			AssertEquals(ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 19, 13, 30, 0);
			AssertEquals(new ZDateTime(2012, 8, 17, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 10, 21, 30, 0);
			AssertEquals(new ZDateTime(2012, 8, 9, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 11, 6, 30, 0);
			AssertEquals(new ZDateTime(2012, 8, 9, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 11, 16, 30, 0);
			AssertEquals("Earliest 7PM within 48 hours of arrival", new ZDateTime(2012, 8, 9, 19, 00, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 11, 17, 30, 0);
			AssertEquals("no more than 48 hours before arrival", new ZDateTime(2012, 8, 9, 19, 30, 0), MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 10, 11, 30, 0);
			AssertEquals(new ZDateTime(2012, 8, 9, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 9, 23, 30, 0);
			AssertEquals(new ZDateTime(2012, 8, 9, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 9, 15, 30, 0);
			AssertEquals(ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 9, 21, 0, 0);
			AssertEquals(new ZDateTime(2012, 8, 9, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 9, 20, 59, 0);
			AssertEquals(ZDateTime.Empty, MAWB.DeferredScheduledMessagesDateTime);
		}

		[TestDate(2012, 08, 09, 09, 09, 09)]
		public void TestDeferredScheduledMessagesDateTimeOutOfHours()
		{
			AssertEquals("Pre-condition-the test date attribute also returns the same value for UTC time, so this test date value should return 11:09:09AM for CBR",
						new ZDateTime(2012, 08, 09, 19, 09, 09), Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUCBR").LocationDateTime);
			MAWB.CM_RL_NKDischargePort = "AUPER"; //CBR is 2 hours later then PER

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 19, 13, 30, 0);
			AssertEquals("Arrival - 48 is Next Friday at 13:30 moved to 7PM.", new ZDateTime(2012, 8, 17, 19, 0, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 10, 21, 30, 0);
			AssertEquals("Within 48 hours and is Friday at 7:09pm, send now.", new ZDateTime(2012, 8, 9, 19, 09, 09), MAWB.DeferredScheduledMessagesDateTime);
		}

		[TestDate(2012, 08, 11, 01, 09, 09)]
		public void TestDeferredScheduledMessagesDateonWeekEnd()
		{
			AssertEquals("Pre-condition-the test date attribute also returns the same value for UTC time, so this test date value should return 11:09:09AM for CBR",
						new ZDateTime(2012, 08, 11, 11, 09, 09), Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUCBR").LocationDateTime);
			MAWB.CM_RL_NKDischargePort = "AUPER"; //CBR is 2 hours later then PER

			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 21, 13, 30, 0);
			AssertEquals("48 hours before arrival is a Sunday. Send anytime.", new ZDateTime(2012, 8, 19, 15, 30, 0), MAWB.DeferredScheduledMessagesDateTime);
			MAWB.CM_ArrivalDate = new ZDateTime(2012, 8, 12, 21, 30, 0);
			AssertEquals("Within 48 hours of arrival and is a Saturday. Send anytime.", new ZDateTime(2012, 08, 11, 11, 09, 09), MAWB.DeferredScheduledMessagesDateTime);
		}

		[TestDate(2013, 7, 9, 8, 0, 0)] // this is UTC, in Canberra its Tuesday (2013, 7, 9, 18, 0, 0)
		public void TestDeferredScheduledMessageLogChangesOnSaving()
		{
			var logs = new LogsForNominatedEvent(MAWB.GetLogs(), Events.DeferredScheduledMessage);
			logs.AddNew(CusMAWBBase.AirCargoOutturnLogReference);
			logs.AddNew(CusMAWBBase.AirCargoReportLogReference);
			MAWB.CM_RL_NKDischargePort = "AUCBR";
			Factory.Save();
			AssertEquals("Precondition", 2, logs.Count);
			AssertEquals("Precondition", new ZDateTime(2013, 7, 9, 8, 0, 0), logs[0].SL_EventTime);
			AssertEquals("Precondition", new ZDateTime(2013, 7, 9, 8, 0, 0), logs[1].SL_EventTime);

			MAWB.CM_ArrivalDate = new ZDateTime(2013, 7, 11, 18, 0, 0);
			Factory.Save();
			AssertEquals("First 7PM within 48 hours of ArrivalDate Canberra Time", new ZDateTime(2013, 7, 9, 19, 0, 0), logs[0].SL_EventTime);
			AssertEquals("First 7PM within 48 hours of ArrivalDate Canberra Time", new ZDateTime(2013, 7, 9, 19, 0, 0), logs[1].SL_EventTime);

			MAWB.CM_RL_NKDischargePort = "AUPER";
			Factory.Save();
			AssertEquals("48 hours before ArrivalDate Canberra Time", new ZDateTime(2013, 7, 9, 20, 0, 0), logs[0].SL_EventTime);
			AssertEquals("48 hours before ArrivalDate Canberra Time", new ZDateTime(2013, 7, 9, 20, 0, 0), logs[1].SL_EventTime);
		}

		public void TestDefaultValues()
		{
			AssertEquals("HouseMessageIsSent should be set to false as DB has it true", false, MAWB.CM_HouseMessageIsSent);
			AssertEquals("Discharge port is set to home port", GlbBranch.CurrentBranch.GB_RL_NKHomePort, MAWB.CM_RL_NKDischargePort);
			AssertEquals(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, MAWB.CM_ApplicationCode);
		}

		public void TestTypeDecider()
		{
			AssertEquals("TypeDeciderType", typeof(CusMAWBBaseTypeDecider), CusMAWBBase.TypeDecider.GetType());
		}

		public void TestMAWBStripping()
		{
			MAWB.CM_MAWB = "01232141234";
			AssertEquals("non-stripped", "01232141234", MAWB.CM_MAWB);
			MAWB.CM_MAWB = "081-0000 0000";
			AssertEquals("stripped", "08100000000", MAWB.CM_MAWB);
			MAWB.CM_MAWB = "081-0000 0000-987";
			AssertEquals("stripped and truncated", "081000000009", MAWB.CM_MAWB);
		}

		public void TestIsForAir()
		{
			MAWB.CM_MAWB = "0819329234";
			ICusUnderbondUnionCollectionParent decider = MAWB;
			AssertNotNull(decider);
			AssertEquals(true, decider.IsForAirCargo);
		}

		public void TestPartShips()
		{
			AssertNotNull(MAWB.PartShips);
		}

		public void TestPartShipsAreRegisteredEditable()
		{
			AssertEquals("IsRegisteredEditableChildObject", true, MAWB.IsRegisteredEditableChildObject(MAWB.PartShips));
		}

		public void TestShortDescription()
		{
			AssertEquals(ZString.Empty, MAWB.ShortDescription);
			MAWB.CM_MAWB = "01232141234";
			AssertEquals("MAWB: 01232141234", MAWB.ShortDescription);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var partShip1 = MAWB.PartShips.AddNew();
			var partShip2 = MAWB.PartShips.AddNew();

			MAWB.AllUnderbonds.Load();

			AssertEquals("BusinessObjectWithRelatedLogs should contain both PartShipment & Underbonds", 2, MAWB.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains("PartShip1 should be in BusinessObjectWithRelatedLogs", partShip1, MAWB.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("PartShip2 should be in BusinessObjectWithRelatedLogs", partShip2, MAWB.BusinessObjectsWithRelatedEvents);
			TestBusinessObjectsWithRelatedEventsCore(MAWB);
		}

		public void TestReportWhenLoadAsAWrongType()
		{
			var gbMawb = Factory.New<CusMAWB>();
			gbMawb.CM_MAWB = "61898391193";
			gbMawb.CM_ApplicationCode = "CUK";
			Factory.Save();

			var query = new ZQuery(CusMAWBSchema.CM_MAWB, "61898391193");
			new BusinessObjectFactory().LoadTop1<CusMAWB>(query);
			AssertEquals("Attempting to load a non-AU CusMAWB as a AU CusMAWB, CM_ApplicationCode: CUK", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestErrorWhenMawbAlreadyCreatedOnSaving()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;

				var consol = factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_UniqueConsignRef = "CON4TST";
				consol.JK_MasterBillNum = "08111111111";
				factory.Save();

				var mawb1 = factory.New<CusMAWB>();
				mawb1.CM_MAWB = "08111111111";
				mawb1.CM_JK = consol.PK;

				CusMAWB mawb2 = null;

				factory.Saving += (BusinessObjectFactory f) =>
				{
					var factory2 = new BusinessObjectFactory();
					factory2.RefreshEnabled = false;

					mawb2 = factory2.New<CusMAWB>();
					mawb2.CM_MAWB = "08111111112";
					mawb2.CM_JK = consol.PK;
					AssertNoExceptionThrown(() => factory2.Save());
				};

				var odysseyException = AssertExceptionThrown<ZArchitecture.Environment.OdysseyException>(() => factory.Save());

				mawb2.Reload();
				var expectedErrorMessage = $@"A Master Bill for this Consol has already been created (On saving).
This Mawb PK: {mawb1.PK}
MAWB: '08111111111'
Consol: 'CON4TST' ({mawb1.Consol.PK})
Created: '{mawb1.CM_SystemCreateTimeUtc.ToString("u")}' by '{mawb1.CM_SystemCreateUser}'
Other Mawb PK: {mawb2.PK}
MAWB: '08111111112'
Consol: 'CON4TST' ({mawb2.Consol.PK})
Created: '{mawb2.CM_SystemCreateTimeUtc.ToString("u")}' by '{mawb2.CM_SystemCreateUser}'";
				AssertMultilineASCIIEquals(expectedErrorMessage, odysseyException.Message);
			}
		}

		protected virtual void TestBusinessObjectsWithRelatedEventsCore(CusMAWBBase mAWB)
		{
		}

		public abstract void TestIsStandAlone();

		public abstract void TestAllUnderbonds();

		CusMAWBBase mawb;
		CusMAWBBase MAWB => mawb ?? (mawb = (CusMAWBBase)GetNewBusinessObject());

		protected override int GetMAWBCount(string mAWBNo) => new CusMAWBBase.Loader(Factory).FindMatchingMAWBs(mAWBNo).Length;

		protected override BusinessObject GetFirstMatchingMAWB(string mAWBNo) => new CusMAWBBase.Loader(Factory).FindFirstMatchingMAWB(mAWBNo);

		protected override BusinessObject[] GetMAWBsByConsol(ForwardingConsol consol) => new CusMAWBBase.Loader(Factory).FindMatchingMAWBs(consol.PK, false);

		protected override BusinessObject GetFirstMatchingCTOMAWB(string mAWBNo) => new CusMAWBBase.Loader(Factory).FindFirstMatchingCTOMAWB(mAWBNo);

		protected override BusinessObject[] GetMatchingCTOMAWBs(ZString flightNumber, ZDateTime arrivalDate)
			=> new CusMAWBBase.Loader(Factory).FindMatchingCTOMAWBs(flightNumber, arrivalDate);

		protected override BusinessObject[] GetMatchingForwarderMAWBs(ZString mAWBNo, ZString flightNumber, ZDateTime arrivalDate)
			=> new CusMAWBBase.Loader(Factory).FindMatchingForwarderMAWBs(mAWBNo, flightNumber, arrivalDate);

		protected override ZDBOnlyQuery GetMAWBDBOnlyQuery(ZString flightNumber, ZDateTime arrivalDate)
			=> new CusMAWBBase.Loader(Factory).GetMAWBQuery(flightNumber, arrivalDate);
	}
}
