using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(UniversalEventMessage))]
	public sealed class UniversalEventMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniversalEvent()
		{
			var message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0011</Value></Context>");
			AssertEquals(2, message.StatusCodes.Count());

			message.EM_MessageText = ZString.Empty;
			AssertEquals(0, message.StatusCodes.Count());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDetailsProperties()
		{
			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			message.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\EDIMessageDetails.xml");

			var expectedText = @"Send:
UNH+20190523000001+GOVCBR:D:13A:UN:IID
BGM+929+10207001002026+9
DTM+132:201905231729:203
MOA+134:10000:CAD
RFF+ABO:B00175557
RFF+CN:2026HSE0523196
GOR++5
LOC+23+0453+:::SIF
NAD+IM+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA
CTA+IC+:PGA Contact
COM+cedomir.bekic@gmail.com:EM
COM+15198177045:TE
COM+15198177666:FX
NAD+CB+10207++CANADIAN CUSTOMS BROKER+2701 LOMBARDY CRES WHSE+LA SALLE+ON+N9H2L7+CA
CTA+IC+:Craig Seelig
COM+craig.seelig@wisetechglobal.com:EM
COM+12155551234:TE
COM+13125551616:FX
UNS+D
SEQ+1
NAD+VN+58-123456789++ACE TEST IMPORTER 1+123 MADISON AVE+NEW YORK+NY+10016+US
CTA+IC
COM+email@test.com:EM
COM+12155551212:TE
NAD+UC+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA
CTA+IC+:PGA Contact
COM+cedomir.bekic@gmail.com:EM
COM+15198177045:TE
COM+15198177666:FX
LOC+35+US+NY
LOC+277+AU
DTM+757:20190522:102
DOC+380+INV052319AA
SEQ+1
DTM+3:20190522:102
MOA+39:10000:CAD
MEA+AAE+AAB+KGM:1500
LIN+1
LOC+27+US+NY
GID+1
IMD++8+:::SACKS AND BAGS
IMD++57+:::123456
MEA+AAE+AAB+KGM:1500.000
MOA+66:10000:CAD
TCC+++3923219090:HS
NAD+MF+++ACE TEST IMPORTER 1+123 MADISON AVE+NEW YORK+NY+10016+US
CTA+IC
COM+email@test.com:EM
COM+12155551212:TE
HYN+3
UNS+S
UNT+52+20190523000001

Results:
UNB+UNOC:3+INETCECPT+YUSAIRXPN+190523:2028+12668
UNG+GOVCBR+IIDT+U10207V1+20190523:2028+4283+UN+D:13A
UNH+1+GOVCBR:D:13A:UN+SWI210
BGM+961+10207001002026
DTM+9:201905232028:203
RFF+AGO:B00175557
STS++2:::S002
ERC+ZZZ
ERP+:MAND SEG MISSING+PAC:0
UNS+D
HYN+3
UNS+D
UNT+11+1
UNE+1+4283
UNZ+1+12668

";
			AssertEquals(expectedText, message.RawMessage);
			var expectedHtml = File.ReadAllText(
				BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CAGOVCBR13AMessageInterpretation.html"
			);
			expectedHtml = Regex.Replace(expectedHtml, @"[\r\n\t]+", string.Empty);
			var actualHtml = Regex.Replace(message.RawMessageInterpretation, @"[\r\n\t]+", string.Empty);

			AssertMultilineASCIIEquals(expectedHtml, actualHtml);
		}

		public void TestEM_MessageSubType()
		{
			var message = Factory.New<UniversalEventMessage>();
			AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			AssertEquals("EM_MessageSubType", UniversalEventMessageTypes.Codes.IIDResponses, message.EM_MessageSubType);
		}

		readonly string mmaMessageText = @"<s0:UniversalEvent><s0:Event><s0:DataContext><s0:DataTargetCollection><s0:DataTarget><s0:Type>CAIntegratedImportDeclaration</s0:Type><s0:Key>10207000019090</s0:Key></s0:DataTarget></s0:DataTargetCollection><s0:RecipientRoleCollection><s0:RecipientRole><s0:Code>CD4</s0:Code><s0:Description>CA Customs IID/D4 Status Notice</s0:Description></s0:RecipientRole></s0:RecipientRoleCollection></s0:DataContext><s0:EventTime>2018-03-19T11:00:00</s0:EventTime><s0:EventType>MAA</s0:EventType><s0:EventReference>IID Accepted</s0:EventReference><s0:ContextCollection><s0:Context><s0:Type>InterchangeNumber</s0:Type><s0:Value>10484</s0:Value></s0:Context><s0:Context><s0:Type>MessageNumber</s0:Type><s0:Value>1</s0:Value></s0:Context><s0:Context><s0:Type>IsTest</s0:Type><s0:Value>Y</s0:Value></s0:Context><s0:Context><s0:Type>OrganizationReference</s0:Type><s0:Value>B00172152</s0:Value></s0:Context></s0:ContextCollection></s0:Event></s0:UniversalEvent>";

		public void TestEventType()
		{
			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			message.EM_MessageText = mmaMessageText;
			AssertEquals("EventyType", AutoEvents.MessageAcceptedCode, message.EventType);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (UniversalEventMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<UniversalEventMessage>();
		}

		public void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestDocumentSupporter()
		{
			var universalEventMessage = (UniversalEventMessage)GetNewBusinessObject();
			AssertType<UniversalEventMessageDocumentSupporter>(((IDocumentSupportable)universalEventMessage).DocumentSupporter);
		}

		public void TestReferenceNumber()
		{
			string sendersReferenceNodeXml = "<Context><Type>OrganizationReference</Type><Value>HBL - S00048811</Value></Context>";
			string relatedDocumentNodeXml = @"<Context>
				<Type>RelatedDocument</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>DocumentType</Type>
						<Value>RD0-1000</Value>
					</SubContext>
					<SubContext>
						<Type>DocumentNumber</Type>
						<Value>10207555324587</Value>
					</SubContext>
				</SubContextCollection>
			</Context>";

			var message = Factory.New<UniversalEventMessage>();
			AssertEquals("ReferenceNumber", ZString.Empty, message.ReferenceNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty);
			AssertEquals("ReferenceNumber", ZString.Empty, message.ReferenceNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, "123456789", sendersReferenceNodeXml + relatedDocumentNodeXml);
			AssertEquals("ReferenceNumber", "HBL - S00048811", message.ReferenceNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, "123456789", relatedDocumentNodeXml);
			AssertEquals("ReferenceNumber", "123456789", message.ReferenceNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, "", relatedDocumentNodeXml);
			AssertEquals("ReferenceNumber", "10207555324587/RD0-1000", message.ReferenceNumber);
		}

		public void TestRNSProcessingDate()
		{
			var message = Factory.New<UniversalEventMessage>();
			AssertEquals("RNSProcessingDate", ZDateTime.Empty, message.RNSProcessingDate);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty);
			AssertEquals("RNSProcessingDate", ZDateTime.Empty, message.RNSProcessingDate);
			message = CreateNewUniversalEventMessage(Factory, new ZDateTime(2016, 6, 3));
			AssertEquals("RNSProcessingDate", new ZDateTime(2016, 6, 3), message.RNSProcessingDate);
		}

		public void TestStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "0001", "Matched.", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "0002", "Not Matched.", startDate, endDate);
			Factory.Save();

			var message = Factory.New<UniversalEventMessage>();
			AssertEquals("StatusDescription", ZString.Empty, message.StatusDescription);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty);
			AssertEquals("StatusDescription", ZString.Empty, message.StatusDescription);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0001</Value></Context>");
			AssertEquals("StatusDescription", "0001 - Matched.", message.StatusDescription);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0001</Value></Context>");
			AssertEquals("StatusDescription", "0002 - Not Matched.", message.StatusDescription);
		}

		public void TestInterchangeAndMessageNumber()
		{
			var message = Factory.New<UniversalEventMessage>();
			AssertEquals("InterchangeNumber", 0, message.EDIFACTInterchangeNumber);
			AssertEquals("MessageNumber", 0, message.EDIFACTMessageNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty);
			AssertEquals("InterchangeNumber", 0, message.EDIFACTInterchangeNumber);
			AssertEquals("MessageNumber", 0, message.EDIFACTMessageNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, interchangeNumber: "7599", messageNumber: "2");
			AssertEquals("InterchangeNumber", 7599, message.EDIFACTInterchangeNumber);
			AssertEquals("MessageNumber", 2, message.EDIFACTMessageNumber);
			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, interchangeNumber: "A", messageNumber: "A");
			AssertEquals("InterchangeNumber", 0, message.EDIFACTInterchangeNumber);
			AssertEquals("MessageNumber", 0, message.EDIFACTMessageNumber);
		}

		public void TestStatuses()
		{
			var statuses = new ZString[] { "8000", "0001" };
			var message = CreateNewUniversalEventMessage(Factory, ZDateTime.Now, status: CreateStatusContext(statuses[0]));
			AssertArrayEqualsByElements("StatusCodes", new[] { statuses[0] }, message.StatusCodes.ToArray());
			AssertEquals("StatusCodesCombined", statuses[0], message.StatusCodesCombined);

			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Now, status: CreateStatusContext(statuses));
			AssertArrayEqualsByElements("StatusCodes", statuses, message.StatusCodes.ToArray());
			AssertEquals("StatusCodesCombined", statuses[0] + "|" + statuses[1], message.StatusCodesCombined);
		}

		public void TestGOVCBR()
		{
			var message = CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			AssertNull("GOVCBR", message.GOVCBR);

			string rawMessageXml = @"<Context>
			<Type>RawMessage</Type>
			<Value>UNB+UNOC:3+INETCECPW+YUSAIRXPN+220621:1118+6588+++A+++1'UNG+GOVCBR+CCR+U10207V1+220621:1118+733+UN+D:13A'UNH+1+GOVCBR:D:13A:UN'BGM+23:::RA0-1000+10207003502172:1:1+11'DTM+9:202206211118:203'RFF+AGO:B00224682'GOR++5'STS++2:::0001'UNS+D'HYN+3'UNS+S'UNT+10+1'UNE+1+733'UNZ+1+6588'</Value>
		</Context>";

			message = CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, "123456789", rawMessageXml, messageNumber: "1");
			AssertNotNull("GOVCBR", message.GOVCBR);
		}

		public static UniversalEventMessage CreateNewUniversalEventMessage(BusinessObjectFactory factory, ZDateTime eventTime, string dataTargetKey = "", string contextNodeXml = "",
			string interchangeNumber = "0", string messageNumber = "0", string status = "")
		{
			var message = factory.New<UniversalEventMessage>();
			message.EM_MessageText = ZString.Format(@"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type></Type>
					<Key>{0}</Key>
				</DataTarget>
			</DataTargetCollection>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code></Code>
					<Description></Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<EventTime>{1}</EventTime>
		<EventType></EventType>
		<ContextCollection>
			<Context>
				<Type>InterchangeNumber</Type>
				<Value>{3}</Value>
			</Context>
			<Context>
				<Type>MessageNumber</Type>
				<Value>{4}</Value>
			</Context>
			{2}
            {5}
		</ContextCollection>
	</Event>
</UniversalEvent>", dataTargetKey, eventTime.ToISO8601String(), contextNodeXml, interchangeNumber, messageNumber, status);

			return message;
		}

		public static void LinkToBusinessObject(UniversalEventMessage message, BusinessObject bo)
		{
			var stmAlog = bo.Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Table = bo.TableName;
				stmAlog.SL_Parent = bo.PK;
			}
			var genPivot = bo.Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
		}

		public static ZString CreateStatusContext(params ZString[] statuses)
		{
			return ZString.Join(statuses
				.Select(s => ZString.Format(@"
					<Context>
						<Type>Status</Type>
						<Value>{0}</Value>
					</Context>", s))
				.ToArray());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		EDIMessage message;

		#endregion
	}
}
