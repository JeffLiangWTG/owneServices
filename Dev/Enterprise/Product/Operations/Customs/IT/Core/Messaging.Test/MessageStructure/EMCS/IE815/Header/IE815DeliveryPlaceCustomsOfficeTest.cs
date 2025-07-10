using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815DeliveryPlaceCustomsOfficeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815DeliveryPlaceCustomsOffice(null));
			AssertNoExceptionThrown(() => new IE815DeliveryPlaceCustomsOffice(new Mock<IOffice>().Object));
		});
	}
}
