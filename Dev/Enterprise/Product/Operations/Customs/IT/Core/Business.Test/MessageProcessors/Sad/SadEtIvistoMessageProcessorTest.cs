using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SadEtIvistoMessageProcessorTest : SadCustomsExitAndReleaseMessageProcessorTest<SadIvistoMessageProcessor>
{
	public override void TestIncomingCustomsMessageProcessing()
	{
		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;

		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("20ITQVG1T0008901T0");

		entryHeader.CH_Status = "AWO";

		var sentIdocEdiMessage1 = entryHeader.Messages.AddNew();
		sentIdocEdiMessage1.EM_MessageText = IDocSampleText;
		sentIdocEdiMessage1.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage1.EM_MessageSubType = "ET";
		sentIdocEdiMessage1.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage1.MessageNumberStrategy = new FixedMessageNumberStrategy("824192");

		var sentEdiInterchange1 = Factory.New<EDIInterchange>();
		sentEdiInterchange1.EI_InterchangeNum = "1";
		sentIdocEdiMessage1.EM_Status = EDIInterchange.Status.Sent;
		sentEdiInterchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange1.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentInterchange);
		sentEdiInterchange1.EI_HeaderText = GetSentHeaderText(ValidIdocFileName);
		sentEdiInterchange1.ContainedMessages.Add(sentIdocEdiMessage1);

		Factory.Save();

		(var receivedEdiInterchange, var receivedEdiMessage) = GetReceivedInterchangeAndMessage(IvistoSampleText, receivedCustomsFileName: ValidResponseFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
		processor.ProcessMessage(receivedEdiMessage);
		Factory.Save();

		AssertProcessingResult(entryHeader, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: "AWO", expectedEntryHeaderStatus: "EXI", IvistoSampleText, "824192");

		AssertCusEntryNumbersAfterIvistoOk(entryHeader);
	}

	protected void AssertCusEntryNumbersAfterIvistoOk(CusEntryHeader entryHeader)
	{
		var cusEntryNumber = GetCusEntryNumber(entryHeader);
		AssertNotNull("Check that CusEntryNumber for EntryType 'IVI' exists", cusEntryNumber);

		AssertEntryNumber(cusEntryNumber, EntryType, string.Empty, "CUS", "IT275105", new ZDateTime(2020, 4, 1), "EXC");

		AssertEquals("EntryStatus must be 'EXC'", "EXC", cusEntryNumber.CE_EntryStatus);
		AssertEquals(Enterprise.Core.Constants.RefDocTypes.CustomsCertificate, cusEntryNumber.CE_Category);
		AssertEquals("Country must be Italy", Core.Constants.CountryCodes.Italy, cusEntryNumber.CE_RN_NKCountryCode);
	}

	protected override SadIvistoMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SadIvistoMessageProcessor(logger);

	protected override ZString IdocMessageType => SADConstants.CustomsInterchangeType.IdocR;

	protected override ZString ReceivedInterchangeType => SADConstants.CustomsInterchangeType.Ivisto;

	protected override ZString MessageSubType => SADConstants.MessageSubTypes.ET;

	protected override ZString MessageType => "EXP";

	ZString IvistoSampleText => @"0RON            0RON0401.Q11200005181710028100    01824540346     001 00003    INVIO IN AMBIENTE REALE       
Data:01/04/2020  Ora:12:54:01
TIVISTO  20ITQVG1T0008901T0IT028100IT275105PONTE CHIASSO                      01042020Uscita conclusa                         
";

	ZString IDocSampleText => @"0RON            0RON0401.R11            028100    01824540346     001 00003
TET           8241920014695L 	EU	A	T2	01042020	0			1	IT00777280157	NESTLE' ITALIANA S.P.A.	VIA DEL MULINO, 6	20090	ASSAGO (MI)	IT			CH0	NESTLE' SUISSE S.A.	GALLIKER TRANSPORT AG, LOG.CENTER 3	6246	ALTISHOFEN- SVIZZERA	CH		2	IT01824540346	C.A.D. S.S.D. SRL	P.LE EUROPA 1	43010	BIANCONESE FONTEVIVO	IT		IT	CH	RDE1545P		IT	0	EXW	1	BENEVENTO                          		.		IT				EUR	4276.80		11	3	3		IT275105								89316T-FE																												IT01824540346	S.S.D. C.A.D. SRL	P.LE EUROPA 1	43010	BIANCONESE FONTEVIVO 	IT		01824540346		01824540346	C.A.D. S.S.D. S.R.L.			1	CH004181		1	1	05IT01TR000002820		DAS2				CH004162	1	1	009183				07042020				
?ET1          82419200																																	1	432	NM		CT			Prodotti della panetteria, della pasticceria o della biscotteria, anche con aggiunta di cacao; ostie, capsule vuote dei tipi utilizzati...					1	19059080		0	PR	1956.96	1000	1	0	1641.6																					6	N380	IT	2020	1005955867						N864	IT	2020	1005955867						N271	IT	2020	CMR-59488						Y024	IT	2020	AEOC100212						Y022	IT	2020	AEOF171373						C521	IT	2019	ITACRITP00002-2019-LTK51089														4276.80			000000.00	00.00
";

	protected override ZString ValidResponseContentMessageText => IvistoSampleText;

	protected override ZString EntryType => CusEntryNumberConstants.EntryTypes.Ivisto;

	protected override ZString ValidResponseMRN => "20ITQVG1T0008901T0";

	protected override ZString ValidResponseFileName => "0RON0401.Q11";

	protected override ZString ValidIdocFileName => "0RON0401.R11";
}
