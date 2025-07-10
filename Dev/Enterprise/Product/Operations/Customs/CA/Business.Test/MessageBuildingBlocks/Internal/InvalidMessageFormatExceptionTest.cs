using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks.Testing
{
	sealed class InvalidMessageFormatExceptionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestContstructor()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			ArgumentException argumentException = new ArgumentException();
			new InvalidMessageFormatException("Dummy Test Message");
			new InvalidMessageFormatException("Dummy Test Message", argumentException);
			new InvalidMessageFormatException("Dummy Test Message", argumentException, entry);
		}

		public void TestProperties()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			ArgumentException argumentException = new ArgumentException();
			InvalidMessageFormatException exception = new InvalidMessageFormatException("Dummy Test Message", argumentException, entry);
			AssertEquals("Message", "Dummy Test Message", exception.Message);
			AssertEquals("InnerException", argumentException, exception.InnerException);
			AssertEquals("Originator", entry, exception.Originator);
		}
	}
}
