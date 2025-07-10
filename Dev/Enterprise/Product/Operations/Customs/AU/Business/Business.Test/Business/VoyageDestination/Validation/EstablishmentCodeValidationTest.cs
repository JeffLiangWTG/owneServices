using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EstablishmentCodeValidationTest : TestCaseWithFactory
	{
		public void TestEstablishmentCodeValidation()
		{
			var mock = new Mock<IZPropertyInfo>();
			foreach (string cusRegNo in validCustomsRegNos)
			{
				mock.Setup(m => m.Value).Returns(new ZString(cusRegNo));
				AssertEquals(true, new EstablishmentCodeValidation(Factory).ValidateEstablishmentCode(mock.Object));
				mock.Verify();
			}

			mock.Setup(m => m.Value).Returns(new ZString("A207H"));
			mock.Setup(m => m.AddWarning(EstablishmentCodeValidation.EstablishmentCodeDoesNotMatchCustomsDefinition + "B"));
			AssertEquals(false, new EstablishmentCodeValidation(Factory).ValidateEstablishmentCode(mock.Object));
			mock.VerifyAll();

			mock.Setup(m => m.Value).Returns(new ZString("FD34N"));
			mock.Setup(m => m.AddMessageError(EstablishmentCodeValidation.InvalidEstablishmentCodeAANNA + "H"));
			AssertEquals(false, new EstablishmentCodeValidation(Factory).ValidateEstablishmentCode(mock.Object));
			mock.VerifyAll();

			mock.Setup(m => m.Value).Returns(new ZString("9515N"));
			mock.Setup(m => m.AddMessageError(EstablishmentCodeValidation.InvalidEstablishmentCodeNNNNA + "C"));
			AssertEquals(false, new EstablishmentCodeValidation(Factory).ValidateEstablishmentCode(mock.Object));
			mock.VerifyAll();

			mock.Setup(m => m.Value).Returns(new ZString("IIIII"));
			mock.Setup(m => m.AddMessageError(EstablishmentCodeValidation.UnrecognisedCodeFormat));
			AssertEquals(false, new EstablishmentCodeValidation(Factory).ValidateEstablishmentCode(mock.Object));
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestANNNACustomsRegNumbers()
		{
			var mock = new Mock<IZPropertyInfo>();
			foreach (string cusRegNo in validANNNACustomsRegNos)
			{
				mock.Setup(m => m.Value).Returns(new ZString(cusRegNo));
				new EstablishmentCodeValidation(Factory).ValidateEstablishmentCode(mock.Object);
				mock.Verify();
			}
		}

		readonly string[] validCustomsRegNos =
		{
			"FB34N",
			"FB48B",
			"FB88A",
			"FA76C",
			"FH36A",
			"DL31B",
			"9516N",
			"8549A",
			"FB34N",
			"FA46A",
			"EG32M",
			"DO27K",
			"8526K"
		};

		readonly string[] validANNNACustomsRegNos =
		{
			"V031P",
			"A007H",
			"S039D",
			"S004P",
			"Q020A",
			"D027K",	// DO27K is valid - this may be an invalid code
			"D026A",
			"S040D",
			"I003C"
		};
	}
}
