using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class T2LPOUSCommonResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : XMLResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider>
		where TResponse : XMLResponseMessageProcessor<TResponseProvider, TPrettyMessage>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode
		where TPrettyMessage : IMessagePrettyFormatter
	{
		public void TestProcessRejectedMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>39</td><td>EP01/GoodsShipmentForT2LT2LF</td><td>El elemento GoodsShipmentForT2LT2LF es obligatorio</td><td>GoodsShipmentForT2LT2LF</td></tr>" +
				"</table>";
			AssertT2LPOUSDeclaration(responseMessage, entryStatusCode: OriginalEntryStatus, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		protected void AssertT2LPOUSDeclaration(TestEdiMessage message, string entryStatusCode, string messageSubType = "ACC", string circuit = "", ZDateTime? acceptanceDate = null, string mrn = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string expectedMessageInterpretation = "")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, acceptanceDate: acceptanceDate, movementReferenceNumber: mrn, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.ZG_POUSVersion = 1;

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		protected const string MessageNum = "20221014115635534071";
		protected const string MRNCode = "23ES009999L00026M6";
		protected const string CSVClearance = "379CJX9DEWWYLP4Z";
		protected readonly ZDateTime acceptanceDate = new ZDateTime(2023, 07, 06, 11, 25, 49);

		protected abstract string GetRejectedTestFile();
	}
}
