using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class ManifestForwardHouseBillTest : EDIFACTMessageProcessorTest
	{
		Enterprise.Messaging.Business.EDIMessage GetAndProcessEDIMessage(bool setForwarderSNPType = false)
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+130828:2051+2678++++++1'
UNG+GOVCBR+CCR+U10207V1+20130828:2051+13+UN+D:11B'
UNH+1+GOVCBR:D:11B:UN'
BGM+714+8036X555+4'
RFF+AFM:10207:CB'
RFF+UCN:UCR555'
DOC+23+:24'
DOC+85+9165XXX4444'
RCS+15'
FTX+ACB+++SOM B 2 B COMMENTS'
TDT+11++1'
UNS+D'
HYN+3'
CNI+1'
STS++0'
MEA+AAX++MTQ:6'
HAN+:::SOME HANDLEING INSTRUCTIONS'
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA'
CTA+AH'
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU'
CTA+IC+:FRED NERCK'
CTA+AH'
COM+61290251100:TE'
NAD+DP+++DELIVERY PARTY NAME+DELIVERY PARTY ADDRESS+ONTARIO+BC+L0J 1C0+CA'
CTA+IC+:CONTACT NAME'
NAD+NI+++ABC CANADA+111 HURONTARIO STREET+TORONTO+ON+M5P 1A2+CA'
CTA+IC+:FRED'
CTA+AH'
COM+1 (905) 555-1247:TE'
NAD+NI+++A SECOND NOTIFY PARTY+NOTIFY ADDRESS+MASCOT+NSW+2020+AU'
NAD+ZZZ+++PLACE OF CONSOLIDATION+POC ADDRESS LINE+MASCOT+NSW+2020+AU'
CTA+AH'
NAD+PK+++FRED WIDGET'
CTA+AH'
LOC+8+0809+3380'
LOC+11+0495+3559'
DOC+714'
NAD+CS+++ABC FREIGHT FORWARDERS+6015 BOTANY ROAD+BANKSMEADOW+NSW+2019+AU'
CTA+AH'
RCS+15'
FTX+AAC+++SOME DG INSTRUCTIONS'
CTA+AH'
EQD+CN+OCLU1231230'
SEQ+4'
SEL+111'
SEQ+4'
SEL+222'
SEQ+4'
EQD+CN+OCLU2342346'
SEQ+4'
SEL+333'
SEQ+4'
CTA+AH'
TDT+1'
SEQ+4'
PAC+100++:::PKG'
SEQ+4'
PCI++SOME MARKS'
GID+1'
FTX+AAA+++PACKAGED STUFF'
TCC+++7326.90.90:SRZ'
SEQ+4'
PAC+10++:::BOX'
SEQ+4'
PCI++MORE MARKS'
GID+1'
FTX+AAA+++BOXES OF DG THINGOES'
TCC+++7326909091:SRZ'
TCC+++1201:SSC'
TCC+++1402:SSC'
UNS+S'
CNT+7:2:TNE'
UNT+69+1'
UNE+1+13'
UNZ+1+2678'";

			#endregion

			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory,
				(setForwarderSNPType ? interchangeString.Replace("RFF+AFM:10207:CB'", "RFF+AFM:10207:FW'") : interchangeString).Replace("\r\n", ""),
				Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var tmpMessage = interchange.ContainedMessages[0];
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid("f0d4ec86-9e4d-4a51-aae1-b840ad6a53ec")).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			var result = interchange.ContainedMessages[0];
			var processor = new ManifestForwardHouseBill(logger);
			processor.ProcessMessage(result);
			return result;
		}

		public void TestSettingSystemDefinedValue()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+130828:2051+2678++++++1'
UNG+GOVCBR+CCR+U10207V1+20130828:2051+13+UN+D:11B'
UNH+1+GOVCBR:D:11B:UN'
BGM+714+8036X555+4'
RFF+AFM:10207:CB'
RFF+UCN:UCR555'
DOC+23+:24'
DOC+85+9165XXX4444'
RCS+15'
FTX+ACB+++SOM B 2 B COMMENTS'
TDT+11++1'
UNS+D'
HYN+3'
CNI+1'
STS++0'
MEA+AAX++MTQ:6'
HAN+:::SOME HANDLEING INSTRUCTIONS'
NAD+CN+++VANCOUVER IMPORT/ EXPORT COMPANY+99 MAIN ST+VANCOUVER+BC+V6B3G2+CA'
CTA+AH'
NAD+CZ+++TREETOYS PTY LTD+105 WOMBAT DRIVE+KATOOMBA+NSW+2780+AU'
CTA+IC+:FRED NERCK'
CTA+AH'
COM+61290251100:TE'
NAD+DP+++DELIVERY PARTY NAME+DELIVERY PARTY ADDRESS+ONTARIO+BC+L0J 1C0+CA'
CTA+IC+:CONTACT NAME'
NAD+NI+++ABC CANADA+111 HURONTARIO STREET+TORONTO+ON+M5P 1A2+CA'
CTA+IC+:FRED'
CTA+AH'
COM+1 (905) 555-1247:TE'
NAD+NI+++A SECOND NOTIFY PARTY+NOTIFY ADDRESS+MASCOT+NSW+2020+AU'
NAD+ZZZ+++PLACE OF CONSOLIDATION+POC ADDRESS LINE+MASCOT+NSW+2020+AU'
CTA+AH'
NAD+PK+++FRED WIDGET'
CTA+AH'
LOC+8+0809+3380'
LOC+11+0495+3559'
DOC+714'
NAD+CS+++ABC FREIGHT FORWARDERS+6015 BOTANY ROAD+BANKSMEADOW+NSW+2019+AU'
CTA+AH'
RCS+15'
FTX+AAC+++SOME DG INSTRUCTIONS'
CTA+AH'
EQD+CN+OCLU1231230'
SEQ+4'
SEL+111'
SEQ+4'
SEL+222'
SEQ+4'
EQD+CN+OCLU2342346'
SEQ+4'
SEL+333'
SEQ+4'
CTA+AH'
TDT+1'
SEQ+4'
PAC+100++:::PKG'
SEQ+4'
PCI++SOME MARKS'
GID+1'
FTX+AAA+++PACKAGED STUFF'
TCC+++7326.90.90:SRZ'
SEQ+4'
PAC+10++:::BOX'
SEQ+4'
PCI++MORE MARKS'
GID+1'
FTX+AAA+++BOXES OF DG THINGOES'
TCC+++7326909091:SRZ'
TCC+++1201:SSC'
TCC+++1402:SSC'
UNS+S'
CNT+7:2:TNE'
UNT+69+1'
UNE+1+13'
UNZ+1+2678'";

			#endregion

			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString.Replace("\r\n", ""),
			Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var tmpMessage = interchange.ContainedMessages[0];
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid("f0d4ec86-9e4d-4a51-aae1-b840ad6a53ec")).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			var result = interchange.ContainedMessages[0];
			var processor = new ManifestForwardHouseBill(logger);
			processor.ProcessMessage(result);

			var message = Factory.Load<ACIHouseBillMessage>(result.PK);
			var primaryCCN = message.GetSystemDefinedValue<ZString>(ACIHouseBillMessage.Schema.PrimaryCCN);
			var snpType = message.GetSystemDefinedValue<ZString>(Business.EDIMessage.Schema.SNPType);
			var cbsaIffuce = message.GetSystemDefinedValue<ZString>(Business.EDIMessage.Schema.CBSAOffice);
			var sublocation = message.GetSystemDefinedValue<ZString>(Business.EDIMessage.Schema.SubLocation);

			AssertEquals("PrimaryCCN", "9165XXX4444", primaryCCN);
			AssertEquals("SNPType", "CB", snpType);
			AssertEquals("CBSAOffice", "0809", cbsaIffuce);
			AssertEquals("SubLocation", "3380", sublocation);
			AssertEquals("PrimaryCCN", "9165XXX4444", message.PrimaryCCN);
			AssertEquals("SNPType", "CB", message.SNPType);
			AssertEquals("CBSAOffice", "0809", message.CBSAOffice);
			AssertEquals("SubLocation", "3380", message.SubLocation);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInterpretation()
		{
			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals("EM_MessageType", MessageTypeList.Codes.ACIHouseBill, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageTypeList.Codes.ManifestForwardHouse, ediMessage.EM_MessageSubType);
			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\ManifestForwardMessageInterpretation.html");
			expectedInterpretation = expectedInterpretation.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");
			AssertMultilineASCIIEquals("Interpretation", expectedInterpretation, ediMessage.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
			Assert(ediMessage.EM_LinkUniqueID.IsEmpty);
			AssertEquals("8036X555", ediMessage.EM_ApplicationReference);
		}

		JobDeclaration AddDeclaration(ZString tranNo, ZString ccn)
		{
			var result = Factory.New<JobDeclaration>();
			result.IsCancelled = false;
			result.TransactionNumber.AccountSecurityCode = "12345";
			result.TransactionNumber.SequentialNumber = tranNo;
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var number = result.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = ccn;
			return result;
		}

		ForwardingShipment AddShipment(ZString shipmentNo, ZString ccn)
		{
			var result = Factory.New<ForwardingShipment>();
			result.IsCancelled = false;
			result.JS_UniqueConsignRef = shipmentNo;
			var number = result.Numbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = ccn;
			return result;
		}

		CFSShipment AddCFSShipment(ZString shipmentNo, ZString ccn)
		{
			var result = Factory.New<CFSShipment>();
			result.IsCancelled = false;
			result.JS_UniqueConsignRef = shipmentNo;
			var number = result.Numbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = ccn;
			return result;
		}

		public void TestSingleDeclarationMatch()
		{
			var declaration1 = AddDeclaration("00000001", "8036X555");
			var declaration2 = AddDeclaration("00000002", "8036X556");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals(declaration1.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease).PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("Matching declaration found for this CCN (8036X555), with transaction number: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">12345000000012</a>"));
		}

		public void TestMultipleDeclarationMatches()
		{
			var declaration1 = AddDeclaration("00000001", "8036X555");
			var declaration2 = AddDeclaration("00000002", "8036X556");
			var declaration3 = AddDeclaration("00000003", "8036X557");
			declaration3.CargoControlNumbers.AddNew().CY_CargoControlNumber = "8036X555";
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals(declaration1.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease).PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("Multiple matching declarations found for this CCN (8036X555), with transaction numbers: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">12345000000012</a>"));
		}

		public void TestSingleShipmentMatch()
		{
			var shipment1 = AddShipment("S10000001", "8036X555");
			var shipment2 = AddShipment("S10000002", "8036X556");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals(shipment1.PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("No matching declaration could be found for this Customs Broker Manifest Forward message with CCN (8036X555). Matching shipment found for this CCN (8036X555), with shipment number: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000001</a>"));
		}

		public void TestCFSShipmentMatch()
		{
			var shipment1 = AddCFSShipment("H10000001", "8036X555");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals(shipment1.PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("No matching declaration could be found for this Customs Broker Manifest Forward message with CCN (8036X555). Matching shipment found for this CCN (8036X555), with shipment number: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">H10000001</a>"));
			Assert(ediMessage.EM_MessageInterpretation.Contains("ControllerID=ShipmentReceival&BusinessEntityPK=" + shipment1.PK));
		}

		public void TestMultipleShipmentMatches()
		{
			var shipment1 = AddShipment("S10000001", "8036X555");
			var shipment2 = AddShipment("S10000002", "8036X555");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals(shipment1.PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("No matching declaration could be found for this Customs Broker Manifest Forward message with CCN (8036X555). Multiple matching shipments found for this CCN (8036X555), with shipment numbers: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000001</a>"));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000002</a>"));
		}

		public void TestNoMatchWithFWSNP()
		{
			var ediMessage = GetAndProcessEDIMessage(setForwarderSNPType: true);
			Assert(ediMessage.EM_LinkUniqueID.IsEmpty);
			Assert(ediMessage.EM_MessageInterpretation.Contains("No matching declaration or shipment could be found for this Non-Customs Broker Manifest Forward message with CCN (8036X555)"));
		}

		public void TestSingleShipmentMatchWithFWSNP()
		{
			var shipment1 = AddShipment("S10000001", "8036X555");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage(setForwarderSNPType: true);
			AssertEquals(shipment1.PK, ediMessage.EM_LinkUniqueID);
			Assert(!ediMessage.EM_MessageInterpretation.Contains("No matching declaration could be found for this Customs Broker Manifest Forward message"));
			Assert(ediMessage.EM_MessageInterpretation.Contains("Matching shipment found for this CCN (8036X555), with shipment number: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000001</a>"));
		}

		public void TestMultipleShipmentMatchesWithFWSNP()
		{
			var shipment1 = AddShipment("S10000001", "8036X555");
			var shipment2 = AddShipment("S10000002", "8036X555");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage(setForwarderSNPType: true);
			AssertEquals(shipment1.PK, ediMessage.EM_LinkUniqueID);
			Assert(!ediMessage.EM_MessageInterpretation.Contains("No matching declaration could be found for this Customs Broker Manifest Forward message"));
			Assert(ediMessage.EM_MessageInterpretation.Contains("Multiple matching shipments found for this CCN (8036X555), with shipment numbers: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000001</a>"));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000002</a>"));
		}

		public void TestDeclarationAndShipmentMatches()
		{
			var shipment1 = AddShipment("S10000001", "8036X555");
			var declaration1 = AddDeclaration("00000001", "8036X555");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage();
			AssertEquals(declaration1.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease).PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("Matching declaration found for this CCN (8036X555), with transaction number: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">12345000000012</a>"));
		}

		public void TestDeclarationAndShipmentMatchesWithFWSNP()
		{
			var shipment1 = AddShipment("S10000001", "8036X555");
			var declaration1 = AddDeclaration("00000001", "8036X555");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage(setForwarderSNPType: true);
			AssertEquals(shipment1.PK, ediMessage.EM_LinkUniqueID);
			Assert(ediMessage.EM_MessageInterpretation.Contains("Matching shipment found for this CCN (8036X555), with shipment number: "));
			Assert(ediMessage.EM_MessageInterpretation.Contains(">S10000001</a>"));
		}

		public void TestSendNotification()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "STAFF1";
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var mailGroup1 = Factory.New<GlbGroup>();
			mailGroup1.GG_Code = "GG1";
			mailGroup1.Staff.Add(staff1);
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "STAFF2";
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var mailGroup2 = Factory.New<GlbGroup>();
			mailGroup2.GG_Code = "GG2";
			mailGroup2.Staff.Add(staff2);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "CA";
			company.GC_Code = "CA1";
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.OrgProxy.OH_Code = "CA1OH";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "CB1";
			Factory.Save();

			CACustomsDataRegistry.Instance.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, mailGroup1.PK.ToGuid());
			CACustomsDataRegistry.Instance.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, mailGroup2.PK.ToGuid());
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "10207");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "10207");
			var ediMessage = GetAndProcessEDIMessage();
			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Manifest Forward Message for House Bill CCN 8036X555");
			AssertNotNull("Manifest Forward Message Notification should be created", email);
			AssertEquals("Email.CCRecipients", 2, email.CCRecipients.Count);
			Assert("Email.CCRecipients", email.CCRecipients.Contains(staff1.GS_EmailAddress));
			Assert("Email.CCRecipients", email.CCRecipients.Contains(staff2.GS_EmailAddress));
		}

		public void TestSetAssociatedBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "CA";
			company.GC_Code = "CA1";
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.OrgProxy.OH_Code = "CA1OH";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "CB1";
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			branch.OrgProxy.OH_Code = "CB1OH";
			branch.OrgProxy.CustomsCodes.AddNew("CCC", "10207", "CA");
			Factory.Save();

			var ediMessage = GetAndProcessEDIMessage(setForwarderSNPType: true);
			Assert(ediMessage.EM_LinkUniqueID.IsEmpty);
			AssertEquals("Branch changed", branch.PK, ediMessage.EM_GB);
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
}
