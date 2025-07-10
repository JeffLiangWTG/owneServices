using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class CustomsMessageExporterFactoryTest : TestCaseWithFactory
{
	public void TestGetMessageExporter()
	{
		AssertExceptionThrown<ArgumentException>("messageSendingMode is required", () => CustomsMessageExporterFactory.GetMessageExporter(ZString.Empty, null));
		AssertExceptionThrown<ArgumentNullException>("sendingResult is required", () => CustomsMessageExporterFactory.GetMessageExporter(CustomsMessageSendingModeList.Codes.AutomaticProcedure, null));

		var message = Factory.New<ITEDIMessage>();

		AssertType<NoActionCustomsMessageExporter>("MessageSendingResult has error", CustomsMessageExporterFactory.GetMessageExporter("XXX", message));
		AssertType<NoActionCustomsMessageExporter>("MessageSendingResult has success, but invalid sending mode", CustomsMessageExporterFactory.GetMessageExporter("XXX", message));
		AssertType<FallbackProcedureCustomsMessageExporter>("Fallback Procedure", CustomsMessageExporterFactory.GetMessageExporter(CustomsMessageSendingModeList.Codes.FallbackProcedure, message));
		AssertType<ManualProcedureCustomsMessageExporter>("Manual Procedure", CustomsMessageExporterFactory.GetMessageExporter(CustomsMessageSendingModeList.Codes.ManualProcedure, message));
	}
}
