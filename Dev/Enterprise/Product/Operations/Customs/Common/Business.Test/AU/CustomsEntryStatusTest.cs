using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.Testing
{
	class CustomsEntryStatusTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestMostImportantStatus()
		{
			NUnit.Framework.Assert.That(CustomsEntryStatus.MostImportantStatusForEdifice(CustomsEntryStatus.AmberLine, CustomsEntryStatus.RedLine), Is.EqualTo(CustomsEntryStatus.RedLine), "MostImportantStatus");
			NUnit.Framework.Assert.That(CustomsEntryStatus.MostImportantStatusForEdifice(CustomsEntryStatus.AmberLine, CustomsEntryStatus.RedLine, CustomsEntryStatus.ClearPay), Is.EqualTo(CustomsEntryStatus.ClearPay), "MostImportantStatus");
			NUnit.Framework.Assert.That(CustomsEntryStatus.MostImportantStatusForEdifice(CustomsEntryStatus.AwaitingPay, CustomsEntryStatus.AmberLine, CustomsEntryStatus.RedLine, CustomsEntryStatus.ClearPay), Is.EqualTo(CustomsEntryStatus.AwaitingPay), "MostImportantStatus");
		}
	}
}
