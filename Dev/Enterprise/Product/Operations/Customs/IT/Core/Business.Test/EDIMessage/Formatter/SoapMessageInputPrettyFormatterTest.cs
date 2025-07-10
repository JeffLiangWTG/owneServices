using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SoapMessageInputPrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When soapNamespace is null",
				() => new SoapMessageInputPrettyFormatter(Factory, soapNamespace: null));

			AssertExceptionThrown<ArgumentException>(
				"When soapNamespace is empty",
				() => new SoapMessageInputPrettyFormatter(Factory, soapNamespace: null));
		});
	}
}
