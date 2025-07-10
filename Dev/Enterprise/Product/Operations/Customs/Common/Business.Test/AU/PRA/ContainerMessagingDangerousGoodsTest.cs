using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.Testing
{
	class DangerousGoodsTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetErrors()
		{
			var goods = new ContainerMessagingDangerousGoods();
			var expected = @"    IMDG Class
    UNDG Number
    Hazard Technical Name
    Emergency Contact Name
    Emergency Contact Phone Number
";
			NUnit.Framework.Assert.That(goods.GetErrors(), Is.EqualTo(expected));
		}

		[ExpectNoExceptions]
		public void TestDangerousGoods()
		{
			var goods = new ContainerMessagingDangerousGoods();
			NUnit.Framework.Assert.That(goods, Is.Not.EqualTo(default(ContainerMessagingDangerousGoods)));
			goods.IMDGClass = "AAA";
			goods.IMDGCodePage = "AAA";
			goods.IMDGCodeVersion = "AAA";
			goods.UNDGNumber = "AAA";
			goods.FlashpointTemperatureInCelcius = "AAA";
			goods.PackingGroup = "AAA";
			goods.TechnicalName = "AAA";
			goods.ContactName = "AAA";
			goods.ContactPhoneNumber = "AAA";
			goods.ContactEmailAddress = "AAA";
			goods.ContactFaxNumber = "AAA";
			goods.Weight = 1.2m;
		}
	}
}
