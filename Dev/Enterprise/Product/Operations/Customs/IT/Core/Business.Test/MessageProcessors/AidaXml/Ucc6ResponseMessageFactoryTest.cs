using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6ResponseMessageFactoryTest : TestCase
{
	public void TestGetResponseMessage_ThrowsExceptionIfResponseMessageTextIsEmpty()
	{
		AssertExceptionThrown<ArgumentException>(() => Ucc6ResponseMessageFactory.GetResponseMessage(responseMessageText: "", "ABC"));
	}

	public void TestGetResponseMessage_ThrowsExceptionIfDeclarationTypeIsEmpty()
	{
		AssertExceptionThrown<ArgumentException>(() => Ucc6ResponseMessageFactory.GetResponseMessage("dummy text", declarationType: ""));
	}

	public void TestGetImportResponseMessage()
	{
		var messageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponseWithClearance.xml");
		var wrapper = Ucc6ResponseMessageFactory.GetResponseMessage(messageText, "IMP");
		AssertNotNull("Wrapper", wrapper);
		AssertType<Ucc6ImportResponseMessage>("Wrapper type", wrapper);
	}

	public void TestGetExportResponseMessage()
	{
		var messageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponseWithClearance.xml");
		var wrapper = Ucc6ResponseMessageFactory.GetResponseMessage(messageText, "EXP");
		AssertNotNull("Wrapper", wrapper);
		AssertType<Ucc6ExportResponseMessage>("Wrapper type", wrapper);
	}

	public void TestGetNctsResponseMessage()
	{
		var messageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ncts_PositiveResponseWithClearance.xml");
		var wrapper = Ucc6ResponseMessageFactory.GetResponseMessage(messageText, "TRA");
		AssertNotNull("Wrapper", wrapper);
		AssertType<NctsResponseMessage>("Wrapper type", wrapper);
	}

	public void TestGetResponseMessage_ThrowsExceptionIfUnsupportedApplicationReference()
	{
		AssertExceptionThrown<CustomsMessageProcessorException>(() => Ucc6ResponseMessageFactory.GetResponseMessage("dummy text", "ABC"));
	}
}
