using System;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeDiagnosticsResult))]
	class BarcodeDiagnosticsResultTest : BarcodeParsingTestCase
	{
		public void TestConstructor_WhenMessageIsNull()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new BarcodeDiagnosticsResult(null, null));
				AssertExceptionThrown<ArgumentNullException>(() => new BarcodeDiagnosticsResult(Mock.Of<IBusiness>(), null));
			});
		}

		public void TestConstructor_WhenMessageIsNotNull()
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => new BarcodeDiagnosticsResult(null, "xxx"));
				AssertNoExceptionThrown(() => new BarcodeDiagnosticsResult(Mock.Of<IBusiness>(), "xxx"));
			});
		}

		public void TestProperty()
		{
			var iBusinessMock = new Mock<IBusiness>();
			var barcodeDiagnosticsResult = new BarcodeDiagnosticsResult(iBusinessMock.Object, "test");
			AssertEquals(iBusinessMock.Object, barcodeDiagnosticsResult.MatchedRule);
			AssertEquals("test", barcodeDiagnosticsResult.BarcodeRuleProcessingMessage);
		}
	}
}
