using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	class UniversalEventMessageInterpretationGeneratorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDetailsProperties()
		{
			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			message.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\EDIMessageDetails.xml");
			var generator = new UniversalEventMessageInterpretationGenerator(Factory, message);

			var expectedSent = @"UNH+20190523000001+GOVCBR:D:13A:UN:IID'BGM+929+10207001002026+9'DTM+132:201905231729:203'MOA+134:10000:CAD'RFF+ABO:B00175557'RFF+CN:2026HSE0523196'GOR++5'LOC+23+0453+:::SIF'NAD+IM+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA'CTA+IC+:PGA Contact'COM+cedomir.bekic@gmail.com:EM'COM+15198177045:TE'COM+15198177666:FX'NAD+CB+10207++CANADIAN CUSTOMS BROKER+2701 LOMBARDY CRES WHSE+LA SALLE+ON+N9H2L7+CA'CTA+IC+:Craig Seelig'COM+craig.seelig@wisetechglobal.com:EM'COM+12155551234:TE'COM+13125551616:FX'UNS+D'SEQ+1'NAD+VN+58-123456789++ACE TEST IMPORTER 1+123 MADISON AVE+NEW YORK+NY+10016+US'CTA+IC'COM+email@test.com:EM'COM+12155551212:TE'NAD+UC+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA'CTA+IC+:PGA Contact'COM+cedomir.bekic@gmail.com:EM'COM+15198177045:TE'COM+15198177666:FX'LOC+35+US+NY'LOC+277+AU'DTM+757:20190522:102'DOC+380+INV052319AA'SEQ+1'DTM+3:20190522:102'MOA+39:10000:CAD'MEA+AAE+AAB+KGM:1500'LIN+1'LOC+27+US+NY'GID+1'IMD++8+:::SACKS AND BAGS'IMD++57+:::123456'MEA+AAE+AAB+KGM:1500.000'MOA+66:10000:CAD'TCC+++3923219090:HS'NAD+MF+++ACE TEST IMPORTER 1+123 MADISON AVE+NEW YORK+NY+10016+US'CTA+IC'COM+email@test.com:EM'COM+12155551212:TE'HYN+3'UNS+S'UNT+52+20190523000001'";
			AssertEquals(expectedSent, generator.SentRawMessage);

			var expectedResponse = @"
					UNB+UNOC:3+INETCECPT+YUSAIRXPN+190523:2028+12668'
					UNG+GOVCBR+IIDT+U10207V1+20190523:2028+4283+UN+D:13A'
					UNH+1+GOVCBR:D:13A:UN+SWI210'
					BGM+961+10207001002026'
					DTM+9:201905232028:203'
					RFF+AGO:B00175557'
					STS++2:::S002'
					ERC+ZZZ'
					ERP+:MAND SEG MISSING+PAC:0'
					UNS+D'
					HYN+3'
					UNS+D'
					UNT+11+1'
					UNE+1+4283'
					UNZ+1+12668'";
			AssertEquals(expectedResponse, generator.ResponseRawMessage);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "1", "Invoice/P.O. Number From RR70: FIELD IS MANDATORY", startDate, endDate);
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code, "FR", "FACTURE/NUMERO DE BON DE COMMANDE: ZONE EST OBLIGATOIRE");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "S001", "Positive Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "S002", "Negative Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var message = CreateD4NoticeMessageAndSetUpTestData(Factory, File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\D4NoticeMessage.xml"));
			var generator = new UniversalEventMessageInterpretationGenerator(Factory, message);

			CombineAssertions(() =>
			{
				AssertEquals("EventType", "Customs Manifest Status", generator.EventType);
				AssertEquals("DocumentType", "test 123 of TotalResponsesCount", generator.DocumentType);
				AssertEquals("ProcessingDate", new ZDateTime(2016, 5, 27, 14, 25, 0), generator.ProcessingDate);
				AssertEquals("SendersReference", "SECONDARY BUSINESS ID", generator.SendersReference);
				AssertEquals("ReferenceNumber", "12345000000012", generator.ReferenceNumber);
				AssertEquals("RelatedDocument Count", 3, generator.RelatedDocuments.Count);
				AssertEquals("First Related Document Number", "99999000070328", generator.RelatedDocuments[0].DocumentNumber);
				AssertEquals("First Related Document Type", "RD0-1000", generator.RelatedDocuments[0].DocumentType);
				AssertEquals("Second Related Document Number", "99999000070329", generator.RelatedDocuments[1].DocumentNumber);
				AssertEquals("Second Related Document Type", "RD0-1001", generator.RelatedDocuments[1].DocumentType);
				AssertEquals("Third Related Document Number", "99999000070330", generator.RelatedDocuments[2].DocumentNumber);
				AssertEquals("Third Related Document Type", "RD0-1002", generator.RelatedDocuments[2].DocumentType);

				var pgaDetails = generator.PGADetails.ToList();
				var pgaDetail = pgaDetails[0];
				AssertEquals("PGA", "CFIA", pgaDetail.PGA);
				AssertEquals("Name", "Canadian Food Inspection Agency", pgaDetail.Name);
				AssertEquals("Port", "InspectionPort", pgaDetail.Port);
				AssertEquals("WH", "InspectionWarehouse", pgaDetail.WH);
				AssertEquals("Site", "InspectionLocationOther", pgaDetail.Site);
				AssertEquals("contact", "<table border=\"0\"></table>", pgaDetail.Contact);

				var statuses = generator.Statuses.ToList();
				var status1 = statuses[0];
				AssertEquals("Code", "S001", status1.Code);
				AssertEquals("Description", "Positive Functional Acknowledgement.", status1.Description);
				var status2 = statuses[1];
				AssertEquals("Code", "S002", status2.Code);
				AssertEquals("Description", "Negative Functional Acknowledgement.", status2.Description);

				AssertEquals("NoticeRecipientType", "Freight Forwarder", generator.NoticeRecipientType);
				AssertEquals("NoticeRecipientReferenceNumber", "7000", generator.NoticeRecipientReferenceNumber);

				var closeMessageHouseBillses = generator.CloseMessageHouseBillses.ToList();
				var closeMessageHouseBills = closeMessageHouseBillses[0];
				AssertEquals("DocumentID", "99999000070329", closeMessageHouseBills.DocumentID);
				AssertEquals("DocumentType", "RD0-1001", closeMessageHouseBills.DocumentType);

				var requestingPGAs = generator.RequestingPGAs.ToList();
				var requestingPGA = requestingPGAs[0];
				AssertEquals("PGA", "CFIA", requestingPGA.PGA);
				AssertEquals("Name", "Canadian Food Inspection Agency", requestingPGA.Name);
				AssertEquals("CommentsSpecialInstructions", "RequestingReviewComments\r\nRequestingSpecialInstructions", requestingPGA.CommentsSpecialInstructions);
				AssertEquals("Errors", "RequestingErrorDescription", requestingPGA.Errors);

				var errorDetails = generator.ErrorDetails.ToList();
				var errorDetail = errorDetails[0];
				AssertEquals("Code", "1", errorDetail.Code);
				AssertEquals("Description", "Invoice/P.O. Number From RR70: FIELD IS MANDATORY", errorDetail.EnglishDescription);
				AssertEquals("French Description", "FACTURE/NUMERO DE BON DE COMMANDE: ZONE EST OBLIGATOIRE", errorDetail.FrenchDescription);
				AssertEquals("Text", "CRR-123", errorDetail.Text);
				AssertEquals("Location", "CRR-456", errorDetail.Location);

				AssertEquals("ContainerString", "CTN-123, CTN-456", generator.ContainerString);
			});
		}

		public void TestGetInterpretatedHTML()
		{
			var message = CreateD4NoticeMessageAndSetUpTestData(Factory, testText);
			var generator = new UniversalEventMessageInterpretationGenerator(Factory, message);
			var html = generator.GetInterpretatedHTML();
			var expectedHtml = "<table border=\"0\"><tr><td><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><thead><tr class=\"tableheadings\"><th>DATE AND REFERENCES</th></tr></thead><tr><td><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Event Type</td><td>Customs Manifest Status</td></tr><tr><td>Processing Date</td><td>27-May-16 14:25:00</td></tr><tr><td>Senders Reference</td><td>&nbsp;</td></tr><tr><td>Reference Number</td><td>12345000000012</td></tr><tr><td>Related Document</td><td>99999000070328</td></tr><tr><td>Related Document Type</td><td>RD0-1000</td></tr><tr><td>Related Document</td><td>99999000070329</td></tr><tr><td>Related Document Type</td><td>RD0-1001</td></tr><tr><td>Related Document</td><td>99999000070330</td></tr><tr><td>Related Document Type</td><td>RD0-1002</td></tr></table></td></tr></table></td></tr><tr><td><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Raw Message</th></tr></thead><tr><td>UNH+2+GOVCBR:D:13A:UN+ECRD40'<br>BGM+23:::ME1-1212+803629102014:1:1+11'<br>DTM+9:201412152359:203'<br>RFF+AGO:SECONDARYBUSINESSID'<br>RFF+ACE:8XXX2XXXCCN1XXXXXXXXX::LA0-1000'<br>GOR++5'<br>STS++2:::0001'<br>UNS+D'<br>HYN+3'<br>UNS+S'<br>UNT+11+2'<br></td></tr></table></td></tr></table>";
			AssertEquals(expectedHtml, html);
		}

		const string testText = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAeManifestStatusNotice</Type>
					<Key>12345000000012</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2016-05-27T14:25:00</EventTime>
		<EventType>CMS</EventType>
		<ContextCollection>
			<Context>
				<Type>MessageNumber</Type>
				<Value>2</Value>
			</Context>
			<Context>
				<Type>RelatedDocument</Type>
				<Value>1234567</Value>
				<SubContextCollection>
					<SubContext>
						<Type>DocumentType</Type>
						<Value>RD0-1000</Value>
					</SubContext>
					<SubContext>
						<Type>DocumentNumber</Type>
						<Value>99999000070328</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>RelatedDocument</Type>
				<Value>2345678</Value>
				<SubContextCollection>
					<SubContext>
						<Type>DocumentType</Type>
						<Value>RD0-1001</Value>
					</SubContext>
					<SubContext>
						<Type>DocumentNumber</Type>
						<Value>99999000070329</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>RelatedDocument</Type>
				<Value>3456789</Value>
				<SubContextCollection>
					<SubContext>
						<Type>DocumentType</Type>
						<Value>RD0-1002</Value>
					</SubContext>
					<SubContext>
						<Type>DocumentNumber</Type>
						<Value>99999000070330</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>RawMessage</Type>
				<Value>UNB+UNOC:3+INETCECPT+YUSAIRXPN+151122:0557+10091'UNG+GOVCBR+CCR+U10207V1+20151122:0557+56+UN+D:13A'UNH+1+GOVCBR:D:13A:UN+ECRD40'BGM+23:::MXXX1XXXX+10207000503268:1:1+11'DTM+9:201511230931:203'RFF+AGO:SECONDARY BUSINESS ID'RFF+ACE:8010CCN1::LA0-1000'RFF+ACE:8010CCN2::LA0-1001'GOR++1'GOR++5'LOC+22+495+1234'GOR++5'LOC+87+497+5678'GOR++12'STS++2:::0001'STS++2:::0003'NAD+FW+8036'DOC+916+8036HBL1::::XXX-XXX1'DOC+916+8036HBL2::::XXX-XXX2'RCS+15+1::12'FTX+SIN+++SPECIAL INSTRUCTIONS 1x'FTX+SIN+++SPECIAL INSTRUCTIONS 2x'TDT+3'EQD+CN+ABCU1234567'SEQ+4'EQD+CN+ABCU1234568'SEQ+4'UNS+D'HYN+3'UNS+S'UNT+29+1'UNH+2+GOVCBR:D:13A:UN+ECRD40'BGM+23:::ME1-1212+803629102014:1:1+11'DTM+9:201412152359:203'RFF+AGO:SECONDARYBUSINESSID'RFF+ACE:8XXX2XXXCCN1XXXXXXXXX::LA0-1000'GOR++5'STS++2:::0001'UNS+D'HYN+3'UNS+S'UNT+11+2'UNE+2+56'UNZ+1+10091'</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		protected virtual UniversalEventMessage CreateD4NoticeMessageAndSetUpTestData(BusinessObjectFactory factory, string messageText)
		{
			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message.EM_MessageText = messageText;
			return message;
		}
	}
}
