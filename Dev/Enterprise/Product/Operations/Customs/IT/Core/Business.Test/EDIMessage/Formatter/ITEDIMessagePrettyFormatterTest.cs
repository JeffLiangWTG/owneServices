using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ITEDIMessagePrettyFormatterForTest_ThrowsNotImplementedException(null));
		AssertNoExceptionThrown(() => new ITEDIMessagePrettyFormatterForTest_ThrowsNotImplementedException(Factory));
	}

	public void TestGetFormattedText_WhenEmptyOriginalText()
	{
		var formatter = new ITEDIMessagePrettyFormatterForTest_ThrowsNotImplementedException(Factory);
		AssertEquals("<h2>Message is empty</h2>", formatter.GetFormattedText(""));
	}

	public void TestGetFormattedText_WhenCustomsMessageProcessorException()
	{
		var formatter = new ITEDIMessagePrettyFormatterForTest_ThrowsCustomsMessageProcessorException(Factory);
		AssertEquals("<h2>CustomsMessageProcessorException</h2>", formatter.GetFormattedText("SOME TEXT"));
	}

	public void TestGetFormattedText_WhenNotCustomsMessageProcessorException()
	{
		const string expectedMessage = "<h2>The message formatter was unable to parse received message.</h2><br /><h2>Please use \"Message Text\" tab page to inspect the raw message content.</h2>";

		var formatter = new ITEDIMessagePrettyFormatterForTest_ThrowsNotImplementedException(Factory);
		AssertEquals(expectedMessage, formatter.GetFormattedText("SOME TEXT"));
	}
}

sealed class ITEDIMessagePrettyFormatterForTest_ThrowsNotImplementedException : ITEDIMessagePrettyFormatter
{
	public ITEDIMessagePrettyFormatterForTest_ThrowsNotImplementedException(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		throw new NotImplementedException();
	}
}

sealed class ITEDIMessagePrettyFormatterForTest_ThrowsCustomsMessageProcessorException : ITEDIMessagePrettyFormatter
{
	public ITEDIMessagePrettyFormatterForTest_ThrowsCustomsMessageProcessorException(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString GetFormattedTextCore(ZString originalText)
	{
		throw new CustomsMessageProcessorException("CustomsMessageProcessorException");
	}
}
