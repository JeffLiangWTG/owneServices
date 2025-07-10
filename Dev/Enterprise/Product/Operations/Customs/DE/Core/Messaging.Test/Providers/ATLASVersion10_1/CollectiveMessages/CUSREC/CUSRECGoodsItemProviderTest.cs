using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSRECGoodsItemProvider))]
	sealed class CUSRECGoodsItemProviderTest : InboundDataProviderTestCase<ICUSRECGoodsItem, CUSRECGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSRECGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "12";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("12").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNotificationSeverity()
		{
			goodsItem.Notification = new GCRECFGoodsItemNotification()
			{
				Severity = NotificationTypeList.Codes.Information,
			};
			NUnit.Framework.Assert.That(dataProvider.NotificationSeverity, Is.EqualTo(NotificationTypeList.Codes.Information).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNotificationSeverity_NotificationNull()
		{
			NUnit.Framework.Assert.That(dataProvider.NotificationSeverity, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestNotificationCode()
		{
			goodsItem.Notification = new GCRECFGoodsItemNotification()
			{
				Code = MonthlyClosingCUSRECMessageProcessor.ATLASNotificationCodes_819,
			};
			NUnit.Framework.Assert.That(dataProvider.NotificationCode, Is.EqualTo(MonthlyClosingCUSRECMessageProcessor.ATLASNotificationCodes_819).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNotificationCode_NotificationNull()
		{
			NUnit.Framework.Assert.That(dataProvider.NotificationCode, Is.EqualTo(ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new GCRECFGoodsItem();
			dataProvider = new CUSRECGoodsItemProvider(goodsItem);
		}
		GCRECFGoodsItem goodsItem;
		ICUSRECGoodsItem dataProvider;

		protected override CUSRECGoodsItemProvider GetProvider() => (CUSRECGoodsItemProvider)dataProvider;
	}
}
