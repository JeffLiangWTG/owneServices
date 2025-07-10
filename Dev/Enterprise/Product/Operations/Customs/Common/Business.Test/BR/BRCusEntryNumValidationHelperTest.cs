using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.BR.Testing
{
	class BRCusEntryNumValidationHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDUENums()
		{
			var validDUEs = new string[4] { "69BR5689541960", "69BR5689541910", "69BR5689541901", "69BR5689541944" };
			var invalidFormatDUEs = new string[4] { "6BR5689541850", "69B5689541850", "69BR514", "69BR51974968848" };

			foreach (var number in validDUEs)
			{
				NUnit.Framework.Assert.That(BRCusEntryNumValidationHelper.CheckValidDUEEntryNumberFormat(number), Is.EqualTo(true), "Code " + number + " should have a valid format.");
			}

			foreach (var number in invalidFormatDUEs)
			{
				NUnit.Framework.Assert.That(BRCusEntryNumValidationHelper.CheckValidDUEEntryNumberFormat(number), Is.EqualTo(false), "Code " + number + " should be invalid due to incorrect formatting.");
			}

			var invalidChecksumDUE = "69BR5689541975";
			NUnit.Framework.Assert.That(BRCusEntryNumValidationHelper.CheckValidDUEEntryNumberCheckDigit(invalidChecksumDUE, out _), Is.EqualTo(false), "Code " + invalidChecksumDUE + " should be invalid due to incorrect checksum.");
		}

		[ExpectNoExceptions]
		public void TestMasterUCRNums()
		{
			var validMasterUCRs = new string[2] { "1BR01831941200000000000000000062021", "1BR64571045200000000000000000280023" };
			var invalidMasterUCRs = new string[2] { "12313131313132132132111111111111111", "1BRR1111111255555555555555555555555" };

			foreach (var number in validMasterUCRs)
			{
				NUnit.Framework.Assert.That(BRCusEntryNumValidationHelper.CheckValidMasterUCREntryNumberFormat(number), Is.EqualTo(true), "Code " + number + " should have a valid format.");
			}

			foreach (var number in invalidMasterUCRs)
			{
				NUnit.Framework.Assert.That(BRCusEntryNumValidationHelper.CheckValidMasterUCREntryNumberFormat(number), Is.EqualTo(false), "Code " + number + " should be invalid due to incorrect formatting.");
			}
		}
	}
}
