using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTAXLineDutyProvider))]
	sealed class CUSTAXLineDutyProviderTest : InboundDataProviderTestCase<ICUSTAXLineDuty, CUSTAXLineDutyProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTAXLineDutyProvider(null, 0, 0));
		}

		[ExpectNoExceptions]
		public void TestChargeAmount()
		{
			duty.PayableAmountByType = 10.3m;
			duty.PayableAmountByTypeSpecified = true;
			NUnit.Framework.Assert.That(dataProvider.ChargeAmount, Is.EqualTo(10.3m));
		}

		[ExpectNoExceptions]
		public void TestChargeAmountNotSpecified()
		{
			duty.PayableAmountByType = 10.3m;
			duty.PayableAmountByTypeSpecified = false;
			NUnit.Framework.Assert.That(dataProvider.ChargeAmount, Is.EqualTo(decimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestChargeType()
		{
			List<(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType, string)> types =
			[
				(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.A0000, "A0000"),
				(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.Item10200, "10200"),
				(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.Item14000, "14000"),
				(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.D0000, "D0000"),
			];

			duty.TypeSpecified = true;

			foreach (var (dutyType, chargeType) in types)
			{
				duty.Type = dutyType;
				NUnit.Framework.Assert.That(dataProvider.ChargeType, Is.EqualTo(chargeType));
			}

			const int maxLengthOfNationalFeeTypeLength = 5;
			foreach (GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType dutyType in Enum.GetValues(typeof(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType)))
			{
				duty.Type = dutyType;
				NUnit.Framework.Assert.That(dataProvider.ChargeType, Has.Length.EqualTo(maxLengthOfNationalFeeTypeLength));
			}
		}

		[ExpectNoExceptions]
		public void TestChargeTypeNotSpecified()
		{
			duty.Type = GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.A0000;
			duty.TypeSpecified = false;
			NUnit.Framework.Assert.That(dataProvider.ChargeType, Is.EqualTo(string.Empty));
		}

		[ExpectNoExceptions]
		public void TestBaseValue()
		{
			duty.TypeSpecified = true;
			duty.Type = GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.A0000;

			NUnit.Framework.Assert.That(dataProvider.BaseValue, Is.EqualTo(15m));

			duty.Type = GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyType.B0000;

			NUnit.Framework.Assert.That(dataProvider.BaseValue, Is.EqualTo(10m));
		}

		[ExpectNoExceptions]
		public void TestMethodOfCalculation()
		{
			duty.Group = "A";
			NUnit.Framework.Assert.That(dataProvider.MethodOfCalculation, Is.EqualTo("A"));
		}

		[ExpectNoExceptions]
		public void TestMethodOfPayment()
		{
			duty.AdditionalInformation = "X";
			NUnit.Framework.Assert.That(dataProvider.MethodOfPayment, Is.EqualTo("X"));
		}

		[ExpectNoExceptions]
		public void TestDutyRates()
		{
			duty.CustomsDutyRate = new[] { new GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyCustomsDutyRate(), new GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyCustomsDutyRate() };
			NUnit.Framework.Assert.That(dataProvider.DutyRates.Count, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestDutyRates_Empty()
		{
			NUnit.Framework.Assert.That(dataProvider.DutyRates.Count, Is.EqualTo(0));
		}

		protected override CUSTAXLineDutyProvider GetProvider() => dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			duty = new GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty();
			dataProvider = new CUSTAXLineDutyProvider(duty, 10, 15);
		}
		GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty duty;
		CUSTAXLineDutyProvider dataProvider;
	}
}
