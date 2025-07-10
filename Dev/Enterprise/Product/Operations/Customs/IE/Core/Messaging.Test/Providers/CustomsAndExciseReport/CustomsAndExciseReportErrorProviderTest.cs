using System;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing;

[TestedType(typeof(CustomsAndExciseReportErrorProvider))]
public class CustomsAndExciseReportErrorProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CustomsAndExciseReportErrorProvider(null));
	}

	public void TestErrorCode()
	{
		var provider = new CustomsAndExciseReportErrorProvider(new MessageAcknowledgement { ErrorReference = new ErrorReference { ErrorCode = "111008" } });
		AssertEquals("111008", provider.ErrorCode);
	}
}
