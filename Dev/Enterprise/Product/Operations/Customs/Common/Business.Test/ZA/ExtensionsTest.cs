using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.ZA.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRoundUsingCustomsValueRule()
		{
			NUnit.Framework.Assert.That(new ZDecimal(0.8m).RoundUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundBetweenZeroAndOneWillBeOne");
			NUnit.Framework.Assert.That(new ZDecimal(0.3m).RoundUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundBetweenZeroAndOneWillBeOne");
			NUnit.Framework.Assert.That(new ZDecimal(1.1m).RoundUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "ValueLessThanCriticalValueWillRoundDown");
			NUnit.Framework.Assert.That(new ZDecimal(1.5001m).RoundUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "ValueLessThanCriticalValueWillRoundDown");
			NUnit.Framework.Assert.That(new ZDecimal(1.5m).RoundUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "ValueGreaterThanOrEqualToCriticalValueWillRoundUp");
			NUnit.Framework.Assert.That(new ZDecimal(1.51m).RoundUsingCustomsValueRule(), Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "ValueGreaterThanOrEqualToCriticalValueWillRoundUp");
			NUnit.Framework.Assert.That(new ZDecimal(0.8m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule");
			NUnit.Framework.Assert.That(new ZDecimal(0.3m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule - this should actually round to Zero if valid to");
			NUnit.Framework.Assert.That(new ZDecimal(1.8m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule");
			NUnit.Framework.Assert.That(new ZDecimal(1.5m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule");
			NUnit.Framework.Assert.That(new ZDecimal(1.3m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule");
			NUnit.Framework.Assert.That(new ZDecimal(0.5m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule - this should actually round to Zero if valid to");
			NUnit.Framework.Assert.That(new ZDecimal(0.51m).RoundDownIncludingToZeroUsingCustomsValueRule(), Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "RoundDownIncludingToZeroUsingCustomsValueRule");
		}
	}
}
