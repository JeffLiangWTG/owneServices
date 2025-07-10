using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class TotalAmountsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTotalAllDutyAndTaxes()
		{
			var testTotalAmount = new TotalAmounts();
			testTotalAmount.TotalCustomsDuty = 1m;
			testTotalAmount.TotalSIMAAssessment = 2m;
			testTotalAmount.TotalExciseTax = 3m;
			testTotalAmount.TotalGST = 4m;
			NUnit.Framework.Assert.That(testTotalAmount.TotalAllDutyAndTaxes, NUnit.Framework.Is.EqualTo(1m + 2m + 3m + 4m).Using(CustomComparers.TypeComparison));
		}
	}
}
