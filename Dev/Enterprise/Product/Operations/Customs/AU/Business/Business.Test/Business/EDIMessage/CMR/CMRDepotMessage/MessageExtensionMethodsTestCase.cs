using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class MessageExtensionMethodsTestCase : TestCaseWithFactory
	{
		public void TestAddUnmatchedContainer()
		{
			var shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_UniqueConsignRef = "S00001111";
			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_UniqueConsignRef = "S00001112";
			var container1 = Factory.New<CFSContainer>();
			var container2 = Factory.New<CFSContainer>();
			var message = Factory.New<CMRSEIMessage>();
			var carstRecord = new CARSTRecord()
			{
				Shipment = shipment1,
				Consol = null,
				Container = container1,
				Line = null
			};

			message.AddUnmatchedContainer(carstRecord, message.UnmatchedContainers);
			AssertEquals(1, message.UnmatchedContainers.Count);
			var list = new List<CARSTRecord>();
			Assert(message.UnmatchedContainers.TryGetValue(container1.PK.ToStringKey(), out list));
			AssertEquals(1, list.Count);
			AssertEquals(shipment1.PK, list[0].Shipment.PK);

			carstRecord = new CARSTRecord()
			{
				Shipment = shipment2,
				Consol = null,
				Container = container1,
				Line = null
			};
			message.AddUnmatchedContainer(carstRecord, message.UnmatchedContainers);
			AssertEquals(1, message.UnmatchedContainers.Count);
			list = new List<CARSTRecord>();
			Assert(message.UnmatchedContainers.TryGetValue(container1.PK.ToStringKey(), out list));
			AssertEquals(2, list.Count);
			AssertEquals(shipment1.PK, list[0].Shipment.PK);
			AssertEquals(shipment2.PK, list[1].Shipment.PK);

			carstRecord = new CARSTRecord()
			{
				Shipment = shipment2,
				Consol = null,
				Container = container2,
				Line = null
			};
			message.AddUnmatchedContainer(carstRecord, message.UnmatchedContainers);
			AssertEquals(2, message.UnmatchedContainers.Count);
			list = new List<CARSTRecord>();
			Assert(message.UnmatchedContainers.TryGetValue(container1.PK.ToStringKey(), out list));
			AssertEquals(2, list.Count);
			AssertEquals(shipment1.PK, list[0].Shipment.PK);
			AssertEquals(shipment2.PK, list[1].Shipment.PK);
			Assert(message.UnmatchedContainers.TryGetValue(container2.PK.ToStringKey(), out list));
			AssertEquals(1, list.Count);
			AssertEquals(shipment2.PK, list[0].Shipment.PK);
		}

		public void TestConstructUnmatchedContainersEmail()
		{
			Env.Registry.AUCustoms.SeaCargoSendErrors = Enterprise.Core.Constants.EmailTo.NominatedGroup;
			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Env.Registry.AUCustoms.SeaCargoSendErrorsToGroup = group.PK.ToGuid();

			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			var header = Factory.New<CusOutturnHeader>();
			header.C6_SendersMessageReference = "O00000007";
			header.C6_OutturningPremiseID = "9914N";
			header.C6_LloydsIMO = "1234567";
			header.C6_VoyageNum = "123";
			var line = header.Outturns.AddNew();
			line.C5_HouseBill = "HOUSEBILL";
			line.C5_ContainerNumber = "FAKE4100011";
			line.C5_CargoType = "LCL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Bob";
			vessel.RV_LloydsNumber = "1234567";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_MasterBillNum = "MASTERBILL";
			consol1.JK_UniqueConsignRef = "L00001235";
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			var transport1 = consol1.Transports[0];
			transport1.JW_JX = voyage.Sailings[0].PK;
			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_MasterBillNum = "MASTERBILL";
			consol2.JK_UniqueConsignRef = "L00001234";
			var shipment = consol2.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSEBILL";
			shipment.JS_UniqueConsignRef = "H00001234";
			var transport2 = consol2.Transports[0];
			transport2.JW_JX = voyage.Sailings[0].PK;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100022";
			Factory.Save();

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			var message = Factory.New<CMRSEIMessage>();
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+123++11++++1234567::11'" +
"TDT+1++ROA'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'" +
"DOC+1'PAC+++LCL:67:95'PAC+20++BX:185:95'RFF+MB:MASTERBILL'RFF+BH:HOUSEBILL'RFF+AAQ:FAKE4100011'FTX+AAA+++STUFF TYPE 1'MEA+AAE+G+KG:0000000005000.00'" +
"MEA+AAE+AAL+KG:0000000005000.00'MEA+AAE+ABJ+CM:0000000000005.00'NAD+CN++CONSIGNEE'UNT+43+000001'";
			message.EM_EI = interchange.PK;

			LoggingInformation logger = new LoggingInformation();
			var processor = new SEIMessageProcessor(logger);
			processor.ProcessMessage(message);

			var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("One, or more, Inconsistent Shipment/Consol/Containers, so cannot set container on shipment pack line"); }));
			AssertNotNull("Email generated", email);
			AssertEquals("blah@blah.com", email.CCRecipients[0].Email);

			var expectedBodyContent1 = @"One, or more, Inconsistent Shipment/Consol/Containers, so cannot set container on shipment pack line.";
			var expectedBodyContent2 = @"A Sea Cargo Establishment Information  - (SEI) message has been received";
			var expectedBodyContent3 = @"A status message has been received from Customs with the details shown below.
One, or more, shipment have been found that match the vessel/voyage and a line house bill, however the
Container mentioned on the inbound status message line is inconsistent with the Consol of the shipment,
i.e. the Container has been registered on another Consol.

Inbound status message header details:
Vessel: Bob (1234567)
Voyage: 123
Origin Premise: 
Destination Premis:";
			var expectedBodyContent4 = @"<th>Master</th><th>House</th><th>Shipment</th><th>Related Consol</th><th>Container</th><th>Container Consol</th></tr></thead><tr><td>MASTERBILL</td><td>HOUSEBILL</td><td>H00001234</td><td>L00001234</td><td>FAKE4100011</td><td>L00001235</td></tr>";
			AssertContains(expectedBodyContent1, email.Body);
			AssertContains(expectedBodyContent2, email.Body);
			AssertContains(expectedBodyContent3.Replace("\r\n", "<br>"), email.Body);
			AssertContains(expectedBodyContent4, email.Body);
		}

		public void TestConstructUnmatchedHouseEmail()
		{
			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Env.Registry.AUCustoms.CargoStatusSendErrorsToGroup = group.PK.ToGuid();
			Env.Registry.AUCustoms.CargoStatusSendErrors = Core.Constants.EmailTo.NominatedGroup;
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "08165437621";
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1615 EB9A 64G5:1+8'
DTM+9:20051026144456899275:ZZZ'
DTM+132:20090120:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9920A::95
NAD+MR+FGE743G::95'
NAD+UD+28078835604::95'
RFF+ABO:QFQF032/26OCT05/081652126630::1'
RFF+MWB:08165437621'
RFF+HWB:HAWB1'
DOC+1'
PAC+0000002'
UNT+17+000001'".Replace("\r\n", "");

			var processor = new CARSTMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);

			var email = Env.OutgoingMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Unidentified consignment status message received."); }));
			AssertNotNull("Email generated", email);
			AssertEquals("blah@blah.com", email.CCRecipients[0].Email);
			AssertContains("A Cargo Status Advice - (CARST) message has been received", email.Body);
			var expectedBodyContent = @"A status message has been received from Customs but a matching house bill could not be found.
An Air Cargo job has been identified for MAWB '08165437621', but no house bill with number 'HAWB1' could be found.
The message has been attached to the master.";
			AssertContains(expectedBodyContent.Replace("\r\n", "<br>"), email.Body);
		}
	}
}
