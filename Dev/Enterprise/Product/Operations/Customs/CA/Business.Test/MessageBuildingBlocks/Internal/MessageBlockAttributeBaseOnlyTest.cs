using System;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks.Testing
{
	sealed class MessageBlockAttributeBaseOnlyTest : TestCase
	{
		[ExpectExceptionMessage(typeof(ArgumentOutOfRangeException), "Position must be greater than 0\r\nParameter name: position")]
		public void TestInvalidPositionParameter()
		{
			var mock = new Mock<MessageBlockAttribute>(MockBehavior.Loose, 0, 1);
			MessageBlockAttribute attribute = mock.Object;
		}

		[ExpectExceptionMessage(typeof(ArgumentOutOfRangeException), "Length must be greater than 0\r\nParameter name: length")]
		public void TestInvalidLengthParameter()
		{
			var mock = new Mock<MessageBlockAttribute>(1, 0);
			MessageBlockAttribute attribute = mock.Object;
		}

		public void TestSerialise()
		{
			IZType value = new ZString("HELLO WORLD");
			var mock = new Mock<MessageBlockAttribute>(7, 4);
			mock.Protected().Setup<string>("SerialiseCore", ItExpr.IsAny<IZType>(), ItExpr.IsAny<bool>()).Returns("WORLD");
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ mock.Object.Serialise(value); });
			AssertNoExceptionThrown(delegate
			{ mock.Object.Serialise(value, true); });
			mock.VerifyAll();

			mock.Reset();
			mock.Protected().Setup<string>("SerialiseCore", ItExpr.IsAny<IZType>(), ItExpr.IsAny<bool>()).Returns("WO");
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ mock.Object.Serialise(value); });
			AssertNoExceptionThrown(delegate
			{ mock.Object.Serialise(value, true); });
			mock.VerifyAll();

			mock.Reset();
			mock.Protected().Setup<string>("SerialiseCore", ItExpr.IsAny<IZType>(), ItExpr.IsAny<bool>()).Returns("WORL");
			AssertNoExceptionThrown(delegate
			{ mock.Object.Serialise(value); });
			AssertNoExceptionThrown(delegate
			{ mock.Object.Serialise(value, true); });
			mock.VerifyAll();
		}

		public void TestDeSerialise()
		{
			var mock = new Mock<MessageBlockAttribute>(7, 4);
			mock.Protected().Setup<IZType>("DeSerialiseCore", ItExpr.IsAny<string>()).Returns(new ZString("WORL"));
			AssertEquals("WORL", mock.Object.DeSerialise("HELLO WORLD"));
		}

		public void TestProperties()
		{
			var mock = new Mock<MessageBlockAttribute>(10, 20);
			AssertEquals("Position", 10, mock.Object.Position);
			AssertEquals("Position", 9, mock.Object.Offset);
			AssertEquals("Length", 20, mock.Object.Length);
		}
	}
}
