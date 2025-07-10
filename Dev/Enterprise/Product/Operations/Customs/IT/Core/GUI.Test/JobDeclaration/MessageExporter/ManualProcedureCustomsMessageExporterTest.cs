using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ManualProcedureCustomsMessageExporterTest : CustomsMessageExporterTest
{
	public override void TestGetFileContent()
	{
		AssertExceptionThrown<ArgumentNullException>("Parent interchange is required", () => customsMessageExporter.GetFileContentExposed(message));

		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;
		AssertEquals("Empty content", "", customsMessageExporter.GetFileContentExposed(message));

		interchange.EI_HeaderText = "<ITMessage>" +
					"<Staff>BBB</Staff>" +
					"<Node>1234</Node>" +
					"<MessageType>R</MessageType>" +
					"<AccountNumber>11111111111-001</AccountNumber>" +
					"<Header>1111            845A1009.R00            999999    22222222222     001 00006</Header>" +
				"</ITMessage>";

		message.EM_MessageText = "message text";
		AssertEquals("Filled EI_HeaderText and EM_MessageText", "1111            845A1009.R00            999999    22222222222     001 00006\r\nmessage text", customsMessageExporter.GetFileContentExposed(message));
	}

	public override void TestGetFileName()
	{
		AssertExceptionThrown<ArgumentNullException>("Parent interchange is required", () => customsMessageExporter.GetFilenameExposed(message));

		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;
		AssertEquals("Empty EI_HeaderText", "", customsMessageExporter.GetFilenameExposed(message));

		interchange.EI_HeaderText = "<ITMessage>" +
					"<Staff>BBB</Staff>" +
					"<Node>1234</Node>" +
					"<MessageType>R</MessageType>" +
					"<AccountNumber>11111111111-001</AccountNumber>" +
					"<Header>1234            845A1009.R00            000000    11111111111     001 00003</Header>" +
				"</ITMessage>";
		AssertEquals("Valid EI_HeaderText", "845A1009.R00", customsMessageExporter.GetFilenameExposed(message));
	}

	protected override ICustomsMessageExporter GetCustomsMessageExporter() => new ManualProcedureCustomsMessageExporter();

	protected override void SetUp()
	{
		base.SetUp();
		message = Factory.New<ITEDIMessage>();
		customsMessageExporter = new ManualProcedureCustomsMessageExporterForTest();
	}

	ITEDIMessage message;
	ManualProcedureCustomsMessageExporterForTest customsMessageExporter;

	#region ManualProcedureCustomsMessageExporterForTest

	class ManualProcedureCustomsMessageExporterForTest : ManualProcedureCustomsMessageExporter
	{
		public ZString GetFilenameExposed(ITEDIMessage message) => GetFileName(message);
		public ZString GetFileContentExposed(ITEDIMessage message) => GetFileContent(message);
	}

	#endregion
}
