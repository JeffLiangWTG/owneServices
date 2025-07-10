using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class FallbackProcedureCustomsMessageExporterTest : CustomsMessageExporterTest
{
	public override void TestGetFileContent()
	{
		AssertEquals("Empty EM_MessageText", ZString.Empty, customsMessageExporter.GetFileContentExposed(message));

		message.EM_MessageText = "message text";
		AssertEquals("Filled EM_MessageText", "message text", customsMessageExporter.GetFileContentExposed(message));
	}

	public override void TestGetFileName()
	{
		AssertEquals("Hardcoded filename", "pratiche.001", customsMessageExporter.GetFilenameExposed(message));
	}

	protected override ICustomsMessageExporter GetCustomsMessageExporter() => new FallbackProcedureCustomsMessageExporter();

	protected override void SetUp()
	{
		base.SetUp();
		message = Factory.New<ITEDIMessage>();
		customsMessageExporter = new FallbackProcedureCustomsMessageExporterForTest();
	}

	ITEDIMessage message;
	FallbackProcedureCustomsMessageExporterForTest customsMessageExporter;

	#region FallbackProcedureCustomsMessageExporterForTest

	class FallbackProcedureCustomsMessageExporterForTest : FallbackProcedureCustomsMessageExporter
	{
		public ZString GetFilenameExposed(ITEDIMessage message) => GetFileName(message);
		public ZString GetFileContentExposed(ITEDIMessage message) => GetFileContent(message);
	}

	#endregion
}
