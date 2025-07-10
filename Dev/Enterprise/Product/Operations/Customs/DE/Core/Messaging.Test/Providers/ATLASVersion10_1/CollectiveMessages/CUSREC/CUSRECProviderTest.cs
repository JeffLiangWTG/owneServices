using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(NotificationTypeList))]

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSRECProvider))]
	sealed class CUSRECProviderTest : InboundDataProviderTestCase<ICUSREC, CUSRECProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSRECProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "MetaData NULL");

				message.MetaData = new GCRECFMetaData { MessageIdentifier = "CUSREC58750000000375302250219160050" };
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSREC58750000000375302250219160050"), "MetaData not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo(ZString.Empty), "Header NULL");

				message.Header = new GCRECFHeader { ReferencedMessageIdentifier = "DE899978300000000812" };
				NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("DE899978300000000812").Using(CustomComparers.TypeComparison), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "Header NULL - should be [null]");

				message.Header = new GCRECFHeader { MRN = "24DE1234567890" };
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE1234567890"), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(default(string)), "Header NULL");

				message.Header = new GCRECFHeader { ReferenceNumber = "ATB150000620520195875" };
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000620520195875"), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(ZString.Empty), "Header NULL");

				message.Header = new GCRECFHeader { LRN = "LOCALREFERENCENUMBER" };
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("LOCALREFERENCENUMBER").Using(CustomComparers.TypeComparison), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestRegistrationDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.RegistrationDate, Is.EqualTo(ZDate.Empty), "Header NULL");

				message.Header = new GCRECFHeader { RegistrationDateSpecified = true, RegistrationDate = new DateTime(2020, 5, 18) };
				NUnit.Framework.Assert.That(dataProvider.RegistrationDate, Is.EqualTo(new ZDate(2020, 5, 18)), "Registration Date Specified");
				message.Header.RegistrationDateSpecified = false;
				NUnit.Framework.Assert.That(dataProvider.RegistrationDate, Is.EqualTo(ZDate.Empty), "Registration Date Not Specified");
			});
		}

		public void TestNotificationSeverity()
		{
			message.Notification = new GCRECFNotification[]
			{
				new GCRECFNotification()
				{
					Severity = NotificationTypeList.Codes.Information,
				},
				new GCRECFNotification()
				{
					Severity = NotificationTypeList.Codes.Error,
				},
			};
			AssertContainsExactElementsInAnyOrder(new[] { NotificationTypeList.Codes.Information, NotificationTypeList.Codes.Error }, dataProvider.NotificationSeverity);
		}

		[ExpectNoExceptions]
		public void TestNotificationSeverity_Empty()
		{
			NUnit.Framework.Assert.That(dataProvider.NotificationSeverity.Any(), Is.EqualTo(false), "Collection doesn't return null but is empty");
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			message.Body = new GCRECFGoodsItem[]
			{
				new GCRECFGoodsItem(),
				new GCRECFGoodsItem()
			};
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection count");
				NUnit.Framework.Assert.That(typeof(ICUSRECGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is ICUSCANGoodsItem");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsItems_Empty()
		{
			NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(0), "Empty Collection");
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new GCRECF();
			dataProvider = new CUSRECProvider(message);
		}
		GCRECF message;
		ICUSREC dataProvider;

		protected override CUSRECProvider GetProvider() => (CUSRECProvider)dataProvider;
	}
}
