using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecAcceptanceMessageProcessor))]
sealed class EdecAcceptanceMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => [(JobMessageTypeList.Codes.Import, Events.CustomsCleared), (JobMessageTypeList.Codes.Export, Events.ExportCustomsCleared)];

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.Accepted;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EdecAcceptanceMessageProcessor(logger);

	protected override ZString ExpectedMessageFriendlyName => "Acceptance Message Processor";

	protected override ZString BGMReference => "35253";

	readonly ZString acceptanceDate = "2021-10-14";

	readonly ZString acceptanceTime = "11:31:16";

	readonly ZString entryStatus = SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease;

	readonly ZString declarationNumber = "21CHEI000042027074";

	ZString declarationVersion = "1";

	readonly ZString selectionResultMax = "2";

	readonly ZString accessCode = "b6zCR8Vfz2u5yKeU";

	ZString goodsDeclarationStatusRelease = "1";

	#region Message Response

	protected override ZString ReceivedMessage => $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
  <goodsDeclarationAcceptance>
    <traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
    <traderReference>MM_TEST_2009_eBegleit</traderReference>
    <customsDeclarationNumber>{declarationNumber}</customsDeclarationNumber>
    <customsDeclarationVersion>{declarationVersion}</customsDeclarationVersion>
    <accessCode>{accessCode}</accessCode>
    <acceptanceDate>{acceptanceDate}</acceptanceDate>
    <acceptanceTime>{acceptanceTime}</acceptanceTime>
    <declarant>
      <traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
      <declarantNumber>72</declarantNumber>
    </declarant>
    <initiator>1</initiator>
    <correctionCode>1</correctionCode>
    <valuation>
	  <duty>17.10</duty>
	  <VAT>88.95</VAT>
    </valuation>
    <additionalTax>
	  <type>280</type>
	  <amount>313.20</amount>
    </additionalTax>
    <additionalTax>
	  <type>970</type>
	  <amount>0.00</amount>
    </additionalTax>
    <fee>
	  <type>150</type>
	  <amount>22.50</amount>
    </fee>
    <goodsItem>
      <traderItemID>1</traderItemID>
      <customsItemNumber>1</customsItemNumber>
      <selectionResult>1</selectionResult>
      <valuationRate>38.00</valuationRate>
      <valuationDetail>
	    <duty>11.40</duty>
	    <VAT>59.30</VAT>
      </valuationDetail>
      <additionalTaxDetail>
	    <type>280</type>
	    <amount>104.40</amount>
	    <key>001</key>
      </additionalTaxDetail>
      <additionalTaxDetail>
	    <type>280</type>
	    <amount>104.40</amount>
	    <key>001</key>
      </additionalTaxDetail>
      <additionalTaxDetail>
	    <type>970</type>
	    <amount>0.00</amount>
	    <key>004</key>
      </additionalTaxDetail>
      <feeDetail>
	    <type>150</type>
	    <amount>15.00</amount>
      </feeDetail>
    </goodsItem>
    <goodsItem>
      <traderItemID>2</traderItemID>
      <customsItemNumber>2</customsItemNumber>
      <selectionResult>{selectionResultMax}</selectionResult>
      <valuationRate>38.00</valuationRate>
      <valuationDetail>
	    <duty>5.70</duty>
	    <VAT>29.65</VAT>
      </valuationDetail>
      <additionalTaxDetail>
	    <type>280</type>
	    <amount>104.40</amount>
	    <key>001</key>
      </additionalTaxDetail>
      <additionalTaxDetail>
	    <type>970</type>
	    <amount>0.00</amount>
	    <key>004</key>
      </additionalTaxDetail>
      <feeDetail>
	    <type>150</type>
	    <amount>7.50</amount>
      </feeDetail>
    </goodsItem>
  </goodsDeclarationAcceptance>
  <goodsDeclarationStatus>
    <traderDeclarationNumber>0100011210809998</traderDeclarationNumber>
    <traderReference>MM_TEST_2009_eBegleit</traderReference>
    <customsOfficeNumber>CH001251</customsOfficeNumber>
    <customsDeclarationNumber>21CHEI000042027074</customsDeclarationNumber>
    <customsDeclarationVersion>1</customsDeclarationVersion>
    <statusDate>2021-10-14</statusDate>
    <statusTime>11:31:16</statusTime>
    <status>{entryStatus}</status>
    <materialCheck>0</materialCheck>
    <release>{goodsDeclarationStatusRelease}</release>
  </goodsDeclarationStatus>
</goodsDeclarationsResponse>";

	ZString MessageResponseWithoutAdditionalTax => $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
  <goodsDeclarationAcceptance>
    <traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
    <traderReference>MM_TEST_2009_eBegleit</traderReference>
    <customsDeclarationNumber>{declarationNumber}</customsDeclarationNumber>
    <customsDeclarationVersion>{declarationVersion}</customsDeclarationVersion>
    <accessCode>{accessCode}</accessCode>
    <acceptanceDate>{acceptanceDate}</acceptanceDate>
    <acceptanceTime>{acceptanceTime}</acceptanceTime>
    <declarant>
      <traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
      <declarantNumber>72</declarantNumber>
    </declarant>
    <initiator>1</initiator>
    <correctionCode>1</correctionCode>
    <valuation>
	  <duty>17.10</duty>
	  <VAT>88.95</VAT>
    </valuation>
    <fee>
	  <type>150</type>
	  <amount>22.50</amount>
    </fee>
    <goodsItem>
      <traderItemID>1</traderItemID>
      <customsItemNumber>1</customsItemNumber>
      <selectionResult>1</selectionResult>
      <valuationRate>38.00</valuationRate>
      <valuationDetail>
	    <duty>11.40</duty>
	    <VAT>59.30</VAT>
      </valuationDetail>
      <feeDetail>
	    <type>150</type>
	    <amount>15.00</amount>
      </feeDetail>
    </goodsItem>
    <goodsItem>
      <traderItemID>2</traderItemID>
      <customsItemNumber>2</customsItemNumber>
      <selectionResult>{selectionResultMax}</selectionResult>
      <valuationRate>38.00</valuationRate>
      <valuationDetail>
	    <duty>5.70</duty>
	    <VAT>29.65</VAT>
      </valuationDetail>
      <feeDetail>
	    <type>150</type>
	    <amount>7.50</amount>
      </feeDetail>
    </goodsItem>
  </goodsDeclarationAcceptance>
  <goodsDeclarationStatus>
    <traderDeclarationNumber>0100011210809998</traderDeclarationNumber>
    <traderReference>MM_TEST_2009_eBegleit</traderReference>
    <customsOfficeNumber>CH001251</customsOfficeNumber>
    <customsDeclarationNumber>21CHEI000042027074</customsDeclarationNumber>
    <customsDeclarationVersion>1</customsDeclarationVersion>
    <statusDate>2021-10-14</statusDate>
    <statusTime>11:31:16</statusTime>
    <status>{entryStatus}</status>
    <materialCheck>0</materialCheck>
    <release>{goodsDeclarationStatusRelease}</release>
  </goodsDeclarationStatus>
</goodsDeclarationsResponse>";

	ZString MessageResponseWithoutAccessCode => $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
  <goodsDeclarationAcceptance>
    <traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
    <traderReference>MM_TEST_2009_eBegleit</traderReference>
    <customsDeclarationNumber>{declarationNumber}</customsDeclarationNumber>
    <customsDeclarationVersion>{declarationVersion}</customsDeclarationVersion>
    <acceptanceDate>{acceptanceDate}</acceptanceDate>
    <acceptanceTime>{acceptanceTime}</acceptanceTime>
    <declarant>
      <traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
      <declarantNumber>72</declarantNumber>
    </declarant>
    <initiator>1</initiator>
    <correctionCode>1</correctionCode>
    <valuation>
      <duty>230.40</duty>
      <VAT>5022.70</VAT>
    </valuation>
    <goodsItem>
      <traderItemID>1</traderItemID>
      <customsItemNumber>1</customsItemNumber>
      <selectionResult>1</selectionResult>
      <valuationRate>4.80</valuationRate>
      <valuationDetail>
        <duty>115.20</duty>
        <VAT>2511.35</VAT>
      </valuationDetail>
    </goodsItem>
    <goodsItem>
      <traderItemID>2</traderItemID>
      <customsItemNumber>2</customsItemNumber>
      <selectionResult>{selectionResultMax}</selectionResult>
      <valuationRate>4.80</valuationRate>
      <valuationDetail>
        <duty>115.20</duty>
        <VAT>2511.35</VAT>
      </valuationDetail>
    </goodsItem>
  </goodsDeclarationAcceptance>
  <goodsDeclarationStatus>
    <traderDeclarationNumber>0100011210809998</traderDeclarationNumber>
    <traderReference>MM_TEST_2009_eBegleit</traderReference>
    <customsOfficeNumber>CH001251</customsOfficeNumber>
    <customsDeclarationNumber>21CHEI000042027074</customsDeclarationNumber>
    <customsDeclarationVersion>1</customsDeclarationVersion>
    <statusDate>2021-10-14</statusDate>
    <statusTime>11:31:16</statusTime>
    <status>{entryStatus}</status>
    <materialCheck>0</materialCheck>
    <release>1</release>
  </goodsDeclarationStatus>
</goodsDeclarationsResponse>";

	ZString RejectedMessageResponse => $@"<goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
        <goodsDeclarationRejection>
          <rejectionDate>2021-10-14</rejectionDate>
            <rejectionTime>11:36:24</rejectionTime>
            <errors>
            <ruleErrors>
              <traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
                <traderReference>MM_TEST_2009_eBegleit</traderReference>
                <declarant>
                <traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
                  <declarantNumber>72</declarantNumber>
                  </declarant>
                <error>
                <ruleName>R109c</ruleName>
                  <checkType>Version Check</checkType>
                  <reference>goodsDeclaration</reference>
                  <referencedElements>
                  <referencedElement>/goodsDeclarations/goodsDeclaration/correctionCode</referencedElement>
                    </referencedElements>
                  <descriptions>
                  <description language = ""de"" > ungültige Version der übermittelten Einfuhrzollanmeldung</description>
                    <description language = ""fr"" > Version de la déclaration en douane importation transmise non valable</description>
                    <description language = ""it"" > La versione della dichiarazione doganale d''importazione trasmessa non è valida</description>
                    </descriptions>
                  </error>
                </ruleErrors>
              </errors>
            </goodsDeclarationRejection>
          </goodsDeclarationsResponse>";

	#endregion

	public void TestResponseAcceptance_Success_WithoutAccessCode() => CombineAssertions(() =>
	{
		foreach (var messageType in MessageTypes)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, MessageResponseWithoutAccessCode, BGMReference);
			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
			factory.Save();

			ProcessMessage(ediMessage);
			factory.Save();

			var movementReferenceNumber = factory.Load<CusEntryNumber>(GetEntryNumberFilter(entryHeader.PK, CusEntryNumberTypes.Standard.MovementReferenceNumber)).FirstOrDefault();
			var accessCode = factory.Load<CusEntryNumber>(GetEntryNumberFilter(entryHeader.PK, CusEntryNumberTypes.Switzerland.AccessCode)).FirstOrDefault();
			var logEntry = entryHeader.Logs.MostRecentLogByEventTime(messageType.ExpectedEvent);
			ZDateTime expectedDateForDuty;
			ZDateTime.TryParseExact($"{acceptanceDate} {acceptanceTime}", out expectedDateForDuty, "yyyy-MM-dd HH:mm:ss");

			AssertEquals("CEI_DateForDuty", expectedDateForDuty, entryHeader.EntryInstruction.CEI_DateForDuty);
			AssertCusEntryHeader(entryHeader, PassarDeclarationPhaseList.Codes.Declaration);
			AssertCusEntryNumber(movementReferenceNumber, $"{declarationNumber}.{declarationVersion}");
			Assert("Logged MRN update", Logger.ContainsLogEntry($"Movement Reference Number changed from '' to '{declarationNumber}.{declarationVersion}' by the EDI Message '{ediMessage.EM_MessageNum}'."));
			AssertEquals("CE_EntryStatus", selectionResultMax, movementReferenceNumber.CE_EntryStatus);
			AssertNull(accessCode);
			AssertLog(messageType, logEntry);
		}
	});

	public void TestResponseAcceptance_Success_WithoutAdditionalTax()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();

				var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, MessageResponseWithoutAdditionalTax, BGMReference);
				entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;

				var entryLine1 = entryHeader.AllEntryLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				factory.Save();

				AssertNoExceptionThrown("No Exception is thrown when there are no Additional Tax", () => ProcessMessage(ediMessage));
			}
		});
	}

	public void TestResponseAcceptance_Success_WithAccessCode() => CombineAssertions(() =>
	{
		foreach (var messageType in MessageTypes)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, ReceivedMessage, BGMReference);
			var declaration = entryHeader.Declaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;

			var extraCharge = entryHeader.Charges.AddNew();
			extraCharge.C1_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
			extraCharge.C1_ChargeType = "XXX";

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var extraFee1 = entryLine1.Fees.AddNew();
			extraFee1.CF_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
			extraFee1.CF_ChargeType = "XX1";

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var extraFee2 = entryLine2.Fees.AddNew();
			extraFee2.CF_Source = CusEntryHeaderChargesSourceCodeList.Codes.CUS;
			extraFee2.CF_ChargeType = "XX2";

			factory.Save();

			ProcessMessage(ediMessage);
			factory.Save();

			var movementReferenceNumber = factory.Load<CusEntryNumber>(GetEntryNumberFilter(entryHeader.PK, CusEntryNumberTypes.Standard.MovementReferenceNumber)).FirstOrDefault();
			var accessCode = factory.Load<CusEntryNumber>(GetEntryNumberFilter(entryHeader.PK, CusEntryNumberTypes.Switzerland.AccessCode)).FirstOrDefault();
			var logEntry = entryHeader.Logs.MostRecentLogByEventTime(messageType.ExpectedEvent);

			AssertEquals("CEI_DateForDuty", $"{acceptanceDate} {acceptanceTime}", entryHeader.EntryInstruction.CEI_DateForDuty.ToString("yyyy-MM-dd HH:mm:ss"));
			AssertCusEntryHeader(entryHeader, PassarDeclarationPhaseList.Codes.Declaration);
			AssertCusEntryNumber(movementReferenceNumber, $"{declarationNumber}.{declarationVersion}");
			Assert("Logged MRN update", Logger.ContainsLogEntry($"Movement Reference Number changed from '' to '{declarationNumber}.{declarationVersion}' by the EDI Message '{ediMessage.EM_MessageNum}'."));
			AssertEquals("CE_EntryStatus", selectionResultMax, movementReferenceNumber.CE_EntryStatus);
			AssertCusEntryNumber(accessCode, this.accessCode);
			AssertCusEntryHeaderCharges(entryHeader);
			AssertCusEntryLineFees(entryHeader);
			AssertLog(messageType, logEntry);
		}
	});

	public void TestCorrectHighestLineNumberAfterAcceptance()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();

				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType.MessageType;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.CustomsEntryInstructions.AddNew();

				var invoice = declaration.Invoices.AddNew();

				var line1 = invoice.InvoiceLines.AddNew();
				line1.JI_Tariff = "12345678";
				line1.JI_Description = "description 1";
				var line2 = invoice.InvoiceLines.AddNew();
				line2.JI_Tariff = "87654321";
				line2.JI_Description = "description 2";
				var line3 = invoice.InvoiceLines.AddNew();
				line3.JI_Tariff = "97541357";
				line3.JI_Description = "description 3";

				declaration.DoMerge();

				var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
				entryHeader.CH_BGMReference = BGMReference;
				AssertEquals("HighestLineNumber has no number assigned to itself.", (ZShort)0, entryHeader.CH_HighestLineNumber);

				var message = MessageProcessorTestHelper.CreateEDIMessage(factory, applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: ReceivedMessage);
				ProcessMessage(message);

				AssertEquals("HighestLineNumber has the highest Entry Line Number.", (ZShort)3, entryHeader.CH_HighestLineNumber);
			}
		});
	}

	public void TestSameHighestLineNumberAfterRejectedMessage()
	{
		foreach (var messageType in MessageTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType.MessageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_Tariff = "12345678";
			line1.JI_Description = "description 1";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_Tariff = "87654321";
			line2.JI_Description = "description 2";
			var line3 = invoice.InvoiceLines.AddNew();
			line3.JI_Tariff = "97541357";
			line3.JI_Description = "description 3";

			declaration.DoMerge();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
				entryHeader.CH_BGMReference = BGMReference;
				AssertEquals("HighestLineNumber has no number assigned to itself.", (ZShort)0, entryHeader.CH_HighestLineNumber);

				var message = MessageProcessorTestHelper.CreateEDIMessage(Factory, applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: RejectedMessageResponse);

				Processor.ProcessMessage(message);
				Factory.Save();

				AssertEquals("HighestLineNumber has no number assigned to itself.", (ZShort)0, entryHeader.CH_HighestLineNumber);
			});
		}
	}

	public void TestRemoveDeletionPendingEntryLinesAfterAcceptance()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType.MessageType;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.CustomsEntryInstructions.AddNew();

				var invoice = declaration.Invoices.AddNew();

				var line1 = invoice.InvoiceLines.AddNew();
				line1.JI_Tariff = "12345678";
				line1.JI_Description = "description 1";
				var line2 = invoice.InvoiceLines.AddNew();
				line2.JI_Tariff = "87654321";
				line2.JI_Description = "description 2";
				var line3 = invoice.InvoiceLines.AddNew();
				line3.JI_Tariff = "97541357";
				line3.JI_Description = "description 3";

				declaration.DoMerge();

				var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
				entryHeader.CH_BGMReference = BGMReference;

				var entryLine = line2.CusEntryLine;
				entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
				entryHeader.PendingDeletionEntryLines.Rebuild();

				AssertEquals("Entry Line Number 1 corresponds to the merged value.", (ZShort)1, entryHeader.AllEntryLines[0].CL_LineNumber);
				AssertEquals("Entry Line Number 2 corresponds to the merged value.", (ZShort)2, entryHeader.AllEntryLines[1].CL_LineNumber);
				AssertEquals("Entry Line Number 3 corresponds to the merged value.", (ZShort)3, entryHeader.AllEntryLines[2].CL_LineNumber);

				AssertEquals("There should be three Entry Lines present.", 3, entryHeader.AllEntryLines.Count);

				var message = MessageProcessorTestHelper.CreateEDIMessage(factory, applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: ReceivedMessage);
				ProcessMessage(message);

				entryHeader.DeletedEntryLines.Rebuild();
				AssertEquals("One Entry Line should be removed.", true, entryLine.IsDeleted);
				AssertEquals("Entry Line Number 1 corresponds to the merged value.", (ZShort)1, entryHeader.AllEntryLines[0].CL_LineNumber);
				AssertEquals("Entry Line Number 3 corresponds to the merged value.", (ZShort)3, entryHeader.AllEntryLines[1].CL_LineNumber);
			}
		});
	}

	public void TestPresentDeletionPendingEntryLinesAfterRejected()
	{
		foreach (var messageType in MessageTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType.MessageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_Tariff = "12345678";
			line1.JI_Description = "description 1";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_Tariff = "87654321";
			line2.JI_Description = "description 2";
			var line3 = invoice.InvoiceLines.AddNew();
			line3.JI_Tariff = "97541357";
			line3.JI_Description = "description 3";

			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
			entryHeader.CH_BGMReference = BGMReference;

			var entryLine = entryHeader.AllEntryLines[1];
			entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			entryHeader.PendingDeletionEntryLines.Rebuild();

			CombineAssertions(() =>
			{
				AssertEquals("Entry Line Number 1 corresponds to the merged value.", (ZShort)1, entryHeader.AllEntryLines[0].CL_LineNumber);
				AssertEquals("Entry Line Number 2 corresponds to the merged value.", (ZShort)2, entryHeader.AllEntryLines[1].CL_LineNumber);
				AssertEquals("Entry Line Number 3 corresponds to the merged value.", (ZShort)3, entryHeader.AllEntryLines[2].CL_LineNumber);

				AssertEquals("There should be three Entry Lines present.", 3, entryHeader.AllEntryLines.Count);

				var message = MessageProcessorTestHelper.CreateEDIMessage(Factory, applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: RejectedMessageResponse);

				Processor.ProcessMessage(message);
				Factory.Save();

				AssertEquals("There should be three Entry Lines present.", 3, entryHeader.AllEntryLines.Count);
				AssertEquals("Entry Line Number 1 corresponds to the merged value.", (ZShort)1, entryHeader.AllEntryLines[0].CL_LineNumber);
				AssertEquals("Entry Line Number 2 corresponds to the merged value.", (ZShort)2, entryHeader.AllEntryLines[1].CL_LineNumber);
				AssertEquals("Entry Line Number 3 corresponds to the merged value.", (ZShort)3, entryHeader.AllEntryLines[2].CL_LineNumber);
			});
		}
	}

	public void TestRemoveNewEntryLineAfterAcceptedAndRejectedMessage()
	{
		foreach (var messageType in MessageTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType.MessageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_Tariff = "12345678";
			line1.JI_Description = "description 1";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_Tariff = "87654321";
			line2.JI_Description = "description 2";

			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
			entryHeader.CH_BGMReference = BGMReference;
			AssertEquals((ZShort)0, entryHeader.CH_HighestLineNumber);

			var acceptanceMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: ReceivedMessage);

			ProcessMessage(acceptanceMessage);
			Factory.Save();

			var line3 = invoice.InvoiceLines.AddNew();
			line3.JI_Tariff = "97541357";
			line3.JI_Description = "description 3";

			declaration.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("There should be three Entry Lines present.", 3, entryHeader.AllEntryLines.Count);
				AssertEquals("New Enrty Line with the number three should be present.", true, ((AllCusEntryLineCollection<CusEntryLine>)entryHeader.AllEntryLines).Find(obj => obj.CL_LineNumber == 3).Any());

				var rejectedMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: ReceivedMessage);

				ProcessMessage(rejectedMessage);
				Factory.Save();

				line3.Delete();
				declaration.DoMerge();

				AssertEquals("One Entry Line should be removed.", 2, entryHeader.AllEntryLines.Count);
				AssertEquals("The new Entry Line with the Number three should be removed.", false, ((AllCusEntryLineCollection<CusEntryLine>)entryHeader.AllEntryLines).Find(obj => obj.CL_LineNumber == 3).Any());
			});
		}
	}

	public void TestDeclarationResponseOverride()
	{
		foreach (var (messageType, expectedEventType) in MessageTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
			entryHeader.CH_BGMReference = BGMReference;

			declarationVersion = "2";
			var ediMessageV2 = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType, MessageSubType, messageText: ReceivedMessage);
			declarationVersion = "1";
			var ediMessageV1 = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType, MessageSubType, messageText: ReceivedMessage);
			declarationVersion = "3";
			var ediMessageV3 = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType, MessageSubType, messageText: ReceivedMessage);

			Factory.Save();

			CombineAssertions(() =>
			{
				ProcessMessage(ediMessageV2);
				Factory.Save();
				AssertEquals("Message V2 processed", "21CHEI000042027074.2", entryHeader.EntryNumber);
				AssertEquals("Latest Declaration Version", 2, entryHeader.GetLatestDeclarationVersion());

				var cleardEvents = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(expectedEventType);
				AssertEquals("CLR Event Time", new ZDateTime(2021, 10, 14, 11, 31, 16), cleardEvents.First().SL_EventTime);
				AssertEquals("CLR Event Reference", "|CDN=21CHEI000042027074.2|STU=203", cleardEvents.First().SL_Reference);
				AssertEquals("A new CLR Event added", 1, cleardEvents.Count());

				ProcessMessage(ediMessageV1);
				Factory.Save();
				AssertEquals("Message V1 processed, MRN not updated ", "21CHEI000042027074.2", entryHeader.EntryNumber);
				AssertEquals("Latest Declaration Version", 2, entryHeader.GetLatestDeclarationVersion());

				cleardEvents = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(expectedEventType);
				AssertEquals("CLR Event Time", new ZDateTime(2021, 10, 14, 11, 31, 16), cleardEvents.First().SL_EventTime);
				AssertEquals("CLR Event Reference", "|CDN=21CHEI000042027074.1|STU=203", cleardEvents.First().SL_Reference);
				AssertEquals("A new CLR Event added", 2, cleardEvents.Count());

				ProcessMessage(ediMessageV3);
				Factory.Save();
				AssertEquals("Message V3 processed", "21CHEI000042027074.3", entryHeader.EntryNumber);
				AssertEquals("Latest Declaration Version", 3, entryHeader.GetLatestDeclarationVersion());

				cleardEvents = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(expectedEventType);
				AssertEquals("CLR Event V3 Time", new ZDateTime(2021, 10, 14, 11, 31, 16), cleardEvents.First().SL_EventTime);
				AssertEquals("CLR Event V3 Reference", "|CDN=21CHEI000042027074.3|STU=203", cleardEvents.First().SL_Reference);
				AssertEquals("A new CLR Event added", 3, cleardEvents.Count());
			});

			entryHeader.CH_BGMReference = "11111";
		}
	}

	public void TestEntryReleaseDateAfterAcceptance()
	{
		CombineAssertions(() =>
		{
			goodsDeclarationStatusRelease = "0";
			AssertNotEquals("CH_EntryReleaseDate", $"{acceptanceDate} {acceptanceTime}", GetEntryReleaseDateAfterAcceptance(ReceivedMessage).ToString("yyyy-MM-dd HH:mm:ss"));
			goodsDeclarationStatusRelease = "1";
			AssertEquals("CH_EntryReleaseDate", $"{acceptanceDate} {acceptanceTime}", GetEntryReleaseDateAfterAcceptance(ReceivedMessage).ToString("yyyy-MM-dd HH:mm:ss"));
		});

		ZDateTime GetEntryReleaseDateAfterAcceptance(string response)
		{
			var factory = new BusinessObjectFactory();
			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, JobMessageTypeList.Codes.Import, MessageSubType, response, BGMReference);

			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
			factory.Save();

			ProcessMessage(ediMessage);
			factory.Save();

			return entryHeader.CH_EntryReleaseDate;
		}
	}

	void AssertCusEntryHeader(CusEntryHeader entryHeader, string expectedPhaseStatus)
	{
		AssertEquals("CH_Status", CHLogicalStatusList.Codes.Accepted, entryHeader.CH_Status);
		AssertEquals("CH_PhaseStatus", expectedPhaseStatus, entryHeader.CH_PhaseStatus);
		AssertEquals("CH_EntryStatus", entryStatus, entryHeader.CH_EntryStatus);
		AssertEquals("CH_EntryReleaseDate", $"{acceptanceDate} {acceptanceTime}", entryHeader.CH_EntryReleaseDate.ToString("yyyy-MM-dd HH:mm:ss"));
	}

	void AssertCusEntryNumber(CusEntryNumber entryNum, ZString entryNumber)
	{
		AssertNotNull(entryNum);
		AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, entryNum.CE_Category);
		AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Switzerland, entryNum.CE_RN_NKCountryCode);
		AssertEquals("CE_EntryNum", entryNumber, entryNum.CE_EntryNum);
		AssertEquals("CE_IssueDate", $"{acceptanceDate} {acceptanceTime}", entryNum.CE_IssueDate.ToString("yyyy-MM-dd HH:mm:ss"));
	}

	void AssertLog((string MessageType, Event ExpectedEvent) messageType, StmALog logEntry)
	{
		AssertNotNull(logEntry);
		AssertEquals("Event", messageType.ExpectedEvent, logEntry.Event);
		AssertEquals("Event", $"|CDN={declarationNumber}.{declarationVersion}|STU={entryStatus}", logEntry.SL_Reference);
	}

	void AssertCusEntryHeaderCharges(CusEntryHeader entryHeader)
	{
		AssertEquals("Header Duty 17.10", 17.10M, entryHeader.Duty);
		AssertEquals("Header VAT 88.95", 88.95M, entryHeader.VAT);
		AssertEquals("Header AdditionalTax 280 Spirits 313.20", 313.20M, entryHeader.Charges.GetAmount(AdditionalTaxesTypes.Spirits));
		AssertEquals("Header AdditionalTax 970 PrepaidDisposal 0.0", 0.0M, entryHeader.Charges.GetAmount(AdditionalTaxesTypes.PrepaidDisposal));
		AssertEquals("Header Fee 22.50", 22.50M, entryHeader.Charges.GetAmount(AdditionalTaxesTypes.OtherTaxesOrFees));

		var extraCharge = entryHeader.ConfirmedCharges.Any(c => c.C1_ChargeType == "XXX");
		AssertEquals("extra charge XXX should no longer exist", false, extraCharge);
	}

	void AssertCusEntryLineFees(CusEntryHeader entryHeader)
	{
		var entryLine1 = entryHeader.AllEntryLines[0];
		AssertEquals("Line 1 Duty 11.40", 11.40M, entryLine1.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
		AssertEquals("Line 1 VAT 59.30", 59.30M, entryLine1.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.VAT));
		AssertEquals("Line 1 AdditionalTax 280 Spirits SUM 104.40 + 104.40 = 208.80", 208.80M, entryLine1.Fees.GetAmount(AdditionalTaxesTypes.Spirits));
		AssertEquals("Line 1 AdditionalTax 970 PrepaidDisposal 0.0", 0.0M, entryLine1.Fees.GetAmount(AdditionalTaxesTypes.PrepaidDisposal));
		AssertEquals("Line 1 Fee 15.00", 15.00M, entryLine1.Fees.GetAmount(AdditionalTaxesTypes.OtherTaxesOrFees));

		var extraFee1 = entryLine1.ConfirmedFees.Cast<CusEntryLineFee>().Any(f => f.CF_ChargeType == "XX1");
		AssertEquals("extra fee XX1 on line 1 should no longer exist", false, extraFee1);

		var entryLine2 = entryHeader.AllEntryLines[1];
		AssertEquals("Line 2 Duty 5.70", 5.70M, entryLine2.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
		AssertEquals("Line 2 VAT 59.30", 29.65M, entryLine2.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.VAT));
		AssertEquals("Line 2 AdditionalTax 280 Spirits 104.40", 104.40M, entryLine2.Fees.GetAmount(AdditionalTaxesTypes.Spirits));
		AssertEquals("Line 2 AdditionalTax 970 PrepaidDisposal 0.0", 0.0M, entryLine2.Fees.GetAmount(AdditionalTaxesTypes.PrepaidDisposal));
		AssertEquals("Line 2 Fee 7.50", 7.50M, entryLine2.Fees.GetAmount(AdditionalTaxesTypes.OtherTaxesOrFees));

		var extraFee2 = entryLine2.ConfirmedFees.Cast<CusEntryLineFee>().Any(f => f.CF_ChargeType == "XX2");
		AssertEquals("extra fee XX2 on line 2 should no longer exist", false, extraFee1);
	}

	ZQuery GetEntryNumberFilter(ZGuid parentID, ZString entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);
		query.AddToFilter(CusEntryNumSchema.CE_ParentID, parentID);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		return query;
	}
}
