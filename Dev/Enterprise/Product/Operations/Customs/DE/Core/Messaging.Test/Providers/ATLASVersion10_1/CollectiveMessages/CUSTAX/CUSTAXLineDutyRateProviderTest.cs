using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTAXLineDutyRateProvider))]
	sealed class CUSTAXLineDutyRateProviderTest : InboundDataProviderTestCase<ICUSTAXLineDutyRate, CUSTAXLineDutyRateProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTAXLineDutyRateProvider(null));
		}

		[ExpectNoExceptions]
		public void TestCriteriaType()
		{
			dutyRate.CriteriaType = "X";
			NUnit.Framework.Assert.That(dataProvider.CriteriaType, Is.EqualTo("X"));
		}

		[ExpectNoExceptions]
		public void TestAssessmentScale()
		{
			dutyRate.AssessmentScale = "09";
			NUnit.Framework.Assert.That(dataProvider.AssessmentScale, Is.EqualTo("09"));
		}

		[ExpectNoExceptions]
		public void TestRate()
		{
			dutyRate.Rate = 10.9m;
			dutyRate.RateSpecified = true;
			NUnit.Framework.Assert.That(dataProvider.Rate, Is.EqualTo(10.9m));
		}

		[ExpectNoExceptions]
		public void TestRate_NotSpecified()
		{
			dutyRate.Rate = 10.9m;
			dutyRate.RateSpecified = false;
			NUnit.Framework.Assert.That(dataProvider.Rate, Is.EqualTo(0m));
		}

		protected override CUSTAXLineDutyRateProvider GetProvider() => dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			dutyRate = new GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyCustomsDutyRate();
			dataProvider = new CUSTAXLineDutyRateProvider(dutyRate);
		}
		GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyCustomsDutyRate dutyRate;
		CUSTAXLineDutyRateProvider dataProvider;
	}
}
