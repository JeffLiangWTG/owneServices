using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class eManifestStatusNoticeEventParentFinderTest : TestCaseWithFactory
	{
		public void TestCusEntryHeaderByNumberQuery()
		{
			var eventXmlText = GetEventXmlText("12345000000011");
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader.CH_BGMReference = "12345000000011";
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusEntryHeader;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "12345000000011", relatedObj.CH_BGMReference);
		}

		public void TestDeclarationsByCCNQuery()
		{
			var eventXmlText = GetEventXmlText("ccn88881111");
			var declaration = Factory.New<JobDeclaration>();
			var ccn = declaration.CargoControlNumbers.AddNew();
			ccn.CA_CCNInfoNumber = "ccn88881111";
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as JobDeclaration;
			AssertNotNull("Not Null", relatedObj);
			Assert("Number", relatedObj.CargoControlNumbers.Any(o => o.CA_CCNInfoNumber.Replace(" ", "") == "ccn88881111"));
		}

		public void TestShipmentsByCCNQuery()
		{
			var eventXmlText = GetEventXmlText("ccn88882222");
			CreateForwardingShipment("ccn88882222");
			CreateCFSShipment("ccn88882222");
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as ForwardingShipment;
			AssertNotNull("Not Null", relatedObj);
			Assert("Number", relatedObj.Numbers.Cast<CusEntryNumber>().Any(o => o.CE_EntryNum.Replace(" ", "") == "ccn88882222"));
		}

		public void TestCusCAeMHHouseByCCNQuery()
		{
			var eventXmlText = GetEventXmlText("ccn88883333");
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = "ccn88883333";
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusCAeMHHouse;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "ccn88883333", relatedObj.BW_HouseCCN);
		}

		public void TestCusCAeMHHouseBySendersRef()
		{
			var eventXmlText = GetEventXmlText("ccn88883333", sendersRef: "HBL-S12345678");
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = "XXXXXXXX";
			house.BW_MessageReference = "S12345678";
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusCAeMHHouse;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "S12345678", relatedObj.BW_MessageReference);
		}

		public void TestCusCAeMHMasterByCCNQuery()
		{
			var eventXmlText = GetEventXmlText("ccn88884444");
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "ccn88884444";
			master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "8884444";
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(2, logParents.Length);
			var relatedObj = logParents[0] as CusCAeMHMaster;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "ccn88884444", relatedObj.BP_PrimaryCCN);
			relatedObj = logParents[1] as CusCAeMHMaster;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "8884444", relatedObj.BP_PrimaryCCN);
		}

		public void TestCusCAeMHMasterBySendersRef()
		{
			var eventXmlText = GetEventXmlText("ccn88883333", sendersRef: "CLS-C12345678");
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "XXXXXXXX";
			master.BP_MessageReference = "C12345678";
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusCAeMHMaster;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "C12345678", relatedObj.BP_MessageReference);
		}

		public void TestForwardingConsolByCCNQuery()
		{
			var eventXmlText = GetEventXmlText("ccn88884444");
			CreateForwardingConsol("ccn88884444");
			CreateForwardingConsol("8884444");
			CreateLoadList("8884444");
			Factory.Save();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(2, logParents.Length);
			var relatedObj = logParents[0] as ForwardingConsol;
			AssertNotNull("Not Null", relatedObj);
			Assert("Number", relatedObj.Numbers.Cast<CusEntryNumber>().Any(o => o.CE_EntryNum.Replace(" ", "") == "ccn88884444"));
			relatedObj = logParents[1] as ForwardingConsol;
			AssertNotNull("Not Null", relatedObj);
			Assert("Number", relatedObj.Numbers.Cast<CusEntryNumber>().Any(o => o.CE_EntryNum.Replace(" ", "") == "8884444"));
		}

		public void TestNoticeRecipientType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader.CH_BGMReference = "ccn88884444";
			var ccn = declaration.CargoControlNumbers.AddNew();
			ccn.CA_CCNInfoNumber = "ccn88884444";
			CreateForwardingShipment("ccn88884444");
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "ccn88884444";
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = "ccn88884444";
			CreateForwardingConsol("ccn88884444");
			Factory.Save();
			var eventXmlText = GetEventXmlText("ccn88884444");
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(6, logParents.Length);
			Assert("CusEntryHeader", logParents.Any(o => o is CusEntryHeader));
			Assert("JobDeclaration", logParents.Any(o => o is JobDeclaration));
			Assert("CusCAeMHHouse", logParents.Any(o => o is CusCAeMHHouse));
			Assert("CusCAeMHMaster", logParents.Any(o => o is CusCAeMHMaster));
			Assert("ForwardingShipment", logParents.Any(o => o is ForwardingShipment));
			Assert("ForwardingConsol", logParents.Any(o => o is ForwardingConsol));

			eventXmlText = GetEventXmlText("ccn88884444", noticeRecipientType: "Warehouse Operator");
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(2, logParents.Length);
			Assert("ForwardingShipment", logParents.Any(o => o is ForwardingShipment));
			Assert("ForwardingConsol", logParents.Any(o => o is ForwardingConsol));
		}

		public void TestThrowDuplicateMessageException()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message.EM_ApplicationReference = "23-Nov-15 09:3100000000";
			Factory.Save();

			var eventXmlText = GetEventXmlText("ccn88884444");
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			AssertExceptionThrown<DuplicateMessageException>(() => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestSendAcknowledgementReport()
		{
			var newGroup = Factory.New<GlbGroup>();
			newGroup.GG_Code = "NG1";
			var newStaff = newGroup.Staff.AddNew();
			newStaff.GS_Code = "NS1";
			newStaff.GS_LoginName = "NS1";
			newStaff.GS_EmailAddress = "ns1@cargowise.com";
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, newGroup.PK.ToGuid());
			var eventXmlText = GetEventXmlText("xxx");
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			finder.GetLogParentsForEvent(xmlEvent);
			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == AutoEvents.CustomsManifestStatus.Description);
			AssertNotNull("Email Not Null", email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns1@cargowise.com", email.CCRecipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Remove(email);
			eventXmlText = GetEventXmlText("xxx", "CustomsDeclaration");
			xmlEvent = eventDeserializer.Parse(eventXmlText);
			finder.GetLogParentsForEvent(xmlEvent);
			email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == AutoEvents.CustomsManifestStatus.Description);
			AssertNull("Email Null", email);
		}

		protected override void SetUp()
		{
			base.SetUp();
			eventDeserializer = new XmlEventDeserializer();
			finder = new eManifestStatusNoticeEventParentFinder(Factory, new eManifestStatusNoticeDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
		}
		eManifestStatusNoticeEventParentFinder finder;
		XmlEventDeserializer eventDeserializer;

		void CreateForwardingShipment(ZString key)
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ccn = shipment.Numbers.AddNew();
			ccn.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			ccn.CE_EntryNum = key;
		}

		void CreateCFSShipment(ZString key)
		{
			var cfsShipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();
			var ccn = cfsShipment.Numbers.AddNew();
			ccn.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			ccn.CE_EntryNum = key;
		}

		void CreateForwardingConsol(ZString key)
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var ccn = forwardingConsol.Numbers.AddNew();
			ccn.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			ccn.CE_EntryNum = key;
		}

		void CreateLoadList(ZString key)
		{
			var cfsLoadList = (CommonConsol)Factory.New<CFS.ICFSLoadListConsol>();
			var ccn = cfsLoadList.Numbers.AddNew();
			ccn.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			ccn.CE_EntryNum = key;
		}

		ZString GetEventXmlText(ZString key, string dataTargetType = "CAeManifestStatusNotice", string sendersRef = "XXXX", string noticeRecipientType = "XXXX")
		{
			return ZString.Format(@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>{0}</Type>
					<Key>{1}</Key>
				</DataTarget>
			</DataTargetCollection>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>CD4</Code>
					<Description>CA Customs IID/D4 Status Notice</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
        <EventTime>2015-11-23T09:31:00</EventTime>
		<EventType>MAA</EventType>
		<EventReference></EventReference>
		<ContextCollection>
			<Context>
				<Type>IsTest</Type>
				<Value>Y</Value>
			</Context>
			<Context>
				<Type>OrganizationReference</Type>
				<Value>{2}</Value>
			</Context>
			<Context>
				<Type>NoticeRecipientType</Type>
				<Value>{3}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
", dataTargetType, key, sendersRef, noticeRecipientType);
		}
	}
}
