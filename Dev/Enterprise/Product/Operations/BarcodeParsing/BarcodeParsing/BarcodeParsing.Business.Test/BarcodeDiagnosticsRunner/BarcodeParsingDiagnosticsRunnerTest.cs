using System;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeParsingDiagnosticsRunner))]
	class BarcodeParsingDiagnosticsRunnerTest : BarcodeParsingTestCase
	{
		public void TestRun_WhenConsumerIsNull()
		{
			var runner = new BarcodeParsingDiagnosticsRunner();
			var loadMatchingRulesParameters = new LoadMatchingRulesParameters(
				"dummy module",
				new ZGuid(),
				new ZGuid(),
				new ZGuid(),
				false,
				false);
			AssertExceptionThrown<ArgumentNullException>(() => runner.Run(null, loadMatchingRulesParameters, "barcode", "targetField"));
		}

		public void TestRun_WhenConsumerIsNotTypeOfBarcodeParsingConsumer()
		{
			var runner = new BarcodeParsingDiagnosticsRunner();
			var loadMatchingRulesParameters = new LoadMatchingRulesParameters(
				"dummy module",
				new ZGuid(),
				new ZGuid(),
				new ZGuid(),
				false,
				false);
			var consumerMock = new Mock<IBarcodeParsingConsumer>();
			AssertExceptionThrown<InvalidOperationException>(() => runner.Run(consumerMock.Object, loadMatchingRulesParameters, "barcode", "targetField"));
		}
	}
}
