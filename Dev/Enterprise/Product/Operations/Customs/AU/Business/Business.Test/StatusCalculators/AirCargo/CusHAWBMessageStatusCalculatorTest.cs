using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBMessageStatusCalculator))]
	sealed class CusHAWBMessageStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			var calculator = GetNewBusinessObject() as CusHAWBMessageStatusCalculator;
			AssertEquals("StatusInfo.Name", "CS_MsgStatus", calculator.StatusInfo.Name);
		}

		public void TestParentForCargoReportingEvents()
		{
			var house = Factory.New<CusHAWB>();
			var shipment = Factory.New<ForwardingShipment>();
			house.CS_JS = shipment.PK;
			var calculator = new CusHAWBMessageStatusCalculator(house);
			AssertEquals("correct parent for events", shipment, ((ICMRCargoReportEventsLogger)calculator).ParentForCargoReportingEvents);
		}

		public void TestInterestedMessageTypes()
		{
			var calculator = GetNewBusinessObject() as CusHAWBMessageStatusCalculator;
			AssertEquals("InterestedMessageTypes.Length", 1, calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.AIRCR, calculator.InterestedMessageTypes[0]);
		}

		public void TestStatusLogIsPrefixed()
		{
			var calculator = GetNewBusinessObject() as CusHAWBMessageStatusCalculator;
			AssertEquals("CusHAWB Message Status - ", calculator.StatusChangedEventLogPrefix);
		}

		public void TestStatusLog()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			Factory.Save();
			var message = Factory.New<CMRAIRCRMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageNum = "1";
			message.EM_MessageText = @"UNH+11+CUSCAR:D:99B:UN'
BGM+933:::AIRCR+A00265415/CMT1:1+9'
RFF+PQ:PO'
RFF+AWB:H3345A'
RFF+HWB:H3345A9'
RFF+MWB:08155553245'
NAD+CN++MICAELA ALCAINO::4 RAVENNA ST  STRATHFIELD NSW 2135 :AU'
NAD+CZ++EFORCITY::12340 DENHOLM DRIVE  EL MONTE CA 97:153 US'
NAD+VW+41065894724::95'
TDT+20+100++6+QF::3'
LOC+8+AUBNE::6'
LOC+76+GBLHR::6'
LOC+12+AUBNE::6'
LOC+91+GBLHR::6'
DTM+178:20140618:102'
CNI+1'
RFF+UCN:A00265415'
MOA+44:12.70:AUD'
GIS+SAC:109:95'
GID+1'
PAC+1'
FTX+AAA+++MOBILE PHONE/ IPOD ACCESSORIES'
MEA+AAE+G+KG:0.33'
UNT+24+11'".Replace("\r\n", "");
			message.EM_LinkUniqueID = hAWB.PK;
			message.EM_LinkTable = "CusHAWB";
			hAWB.Messages.Add(message);
			Factory.Save();
			var logs = hAWB.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChange.Code));
			AssertEquals(0, logs.Length);

			var message2 = Factory.New<CMRAIRCRMessage>();
			message2.EM_MessageText = @"UNH+000005+CUSRES:D:99B:UN'
BGM+961:::AIRCRR+1BD4 9CCB IE0E:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:AIRCR'
RFF+AFM:9'
RFF+ABO:A00265415/CMT1::001'
DTM+310:20140618003756:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000005'".Replace("\r\n", "");
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageSubType = "CLR";
			message2.EM_Status = EDIMessage.Status.Received;
			hAWB.Messages.Add(message2);
			Factory.Save();
			logs = hAWB.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChange.Code));
			AssertEquals(1, logs.Length);
			AssertEquals("CusHAWB Message Status - ACO", logs[0].SL_Reference);
			AssertEquals("Status Change", logs[0].SL_EventDescription);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusHAWBMessageStatusCalculator(Factory.New<CusHAWB>());
	}
}
