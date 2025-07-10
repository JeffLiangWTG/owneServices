using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CANValidationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidCANs()
		{
			foreach (var validCAN in validCANs)
			{
				TestValidCAN(validCAN);
			}
		}

		[ExpectNoExceptions]
		public void TestInvalidCANs()
		{
			foreach (var invalidCAN in invalidCANs)
			{
				TestInvalidCAN(invalidCAN);
			}
		}

		#region Implementation

		[ExpectNoExceptions]
		protected void TestValidCAN(ZString cAN)
		{
			NUnit.Framework.Assert.That(new CANValidation().GetInvalidReason(cAN).IsEmpty, cAN + " should be valid");
		}

		[ExpectNoExceptions]
		protected void TestInvalidCAN(ZString cAN)
		{
			NUnit.Framework.Assert.That(!new CANValidation().GetInvalidReason(cAN).IsEmpty, cAN + " should be invalid");
		}

		protected ZString[] validCANs = new ZString[] { "AAAAJH4EF", "AAAAHGFCR", "AAAAHGWKN", "AAAAE3J6E", "AAAAHGE9F", "AAAAEA7RH", "AAAAEA7RH", "AAAAEA7MY", "AAAAEA9HH", "AAAAEA9PF", "aaaajh4ef" };
		protected ZString[] invalidCANs = new ZString[] { "123", "123456789", "AAAAAAAA", "AAA AAAAA" };

		#endregion
	}
}
