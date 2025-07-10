using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class MessageProviderTest : TestCaseWithFactory
	{
		[TestDate(2022, 5, 15, 16, 45, 35)]
		public void TestPreparationDateAndTime()
		{
			IHeader provider = new MessageProviderTestClass();
			AssertEquals("PreparationDateAndTime", new DateTime(2022, 5, 15, 16, 45, 35), provider.PreparationDateAndTime);
		}

		class MessageProviderTestClass : MessageProvider
		{
		}
	}
}
