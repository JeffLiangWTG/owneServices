using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsEdiMessagePrettierTests : TestCaseWithFactory
	{
		public const string TestFilePath = "Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.EdiMessage.Prettier.TestFiles.TestMessage.xml";
		const string ExpectedHtmlPath = "Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.EdiMessage.Prettier.TestFiles.ExpectedHtml.html";

		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("null message", () => new NctsEdiMessagePrettier<INCTSPrettierData>(message: null, dataProvider));
			AssertExceptionThrown<ArgumentNullException>("null dataProvider", () => new NctsEdiMessagePrettier<INCTSPrettierData>(message, dataProvider: null));
			AssertNoExceptionThrown("All ok", () => new NctsEdiMessagePrettier<INCTSPrettierData>(message, dataProvider));
		});

		public void TestMakeOutboundPrettyForInterpretation()
		{
			AssertContains("<H3>NCTS Message (Phase 5)</H3>", prettier.MakeOutboundPrettyForInterpretation(header));
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			message.EM_MessageText = "Not a NCTS 5 Message";
			prettier = new NctsEdiMessagePrettier<INCTSPrettierData>(message, dataProvider);
			AssertEquals(message.HumanReadableMessage, prettier.MakeOutboundPrettyForInterpretation(header));
		}

		public void TestMakeOutboundPrettyForPhase5Interpretation()
		{
			var retriever = new EmbeddedResourceRetriever();
			var expectedHtml = retriever.GetString(ExpectedHtmlPath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);
			AssertEquals(expectedHtml, prettier.MakeOutboundPrettyForInterpretation(header));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var retriever = new EmbeddedResourceRetriever();
			var testMessageContent = retriever.GetString(TestFilePath);

			message = Factory.New<EDIMessage>();
			message.EM_MessageText = testMessageContent;
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			dataProvider = new EdiMessageParsingXMLPrettyDataProvider(message);
			prettier = new NctsEdiMessagePrettier<INCTSPrettierData>(message, dataProvider);
		}

		NctsHeader header;
		EDIMessage message;
		EdiMessageParsingXMLPrettyDataProvider dataProvider;
		NctsEdiMessagePrettier<INCTSPrettierData> prettier;
	}
}
