using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSREVProvider))]
	sealed class CUSREVProviderTest : InboundDataProviderTestCase<ICUSREV, CUSREVProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSREVProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "MetaData NULL");
				message.MetaData = new FCREVHMetaData
				{
					MessageIdentifier = "MESSAGEIDENTIFIER"
				};
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("MESSAGEIDENTIFIER"), "MetaData not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestCancelledReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CancelledReferenceNumber, Is.EqualTo(ZString.Empty), "Header NULL");
				message.Header = new FCREVHHeader
				{
					CancelledReferenceNumber = "CANCELLEDREFERENCENUMBER"
				};
				NUnit.Framework.Assert.That(dataProvider.CancelledReferenceNumber, Is.EqualTo("CANCELLEDREFERENCENUMBER").Using(CustomComparers.TypeComparison), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(ZString.Empty), "Header NULL");
				message.Header = new FCREVHHeader
				{
					ReferenceNumber = "ATC400533951120184849"
				};
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATC400533951120184849").Using(CustomComparers.TypeComparison), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "Header NULL - should be [null]");
				message.Header = new FCREVHHeader
				{
					MRN = "24DE123050554788M5"
				};
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE123050554788M5"), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestCancelledMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CancelledMRN, Is.EqualTo(default(string)), "Header NULL - should be [null]");
				message.Header = new FCREVHHeader
				{
					CancelledMRN = "CANCELLEDMRN"
				};
				NUnit.Framework.Assert.That(dataProvider.CancelledMRN, Is.EqualTo("CANCELLEDMRN"), "Header not NULL");
			});
		}

		[ExpectNoExceptions]
		public void TestReason()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.Reason, Is.EqualTo(ZString.Empty), "Header NULL");
				message.Header = new FCREVHHeader
				{
					Reason = "Cancellation reason"
				};
				NUnit.Framework.Assert.That(dataProvider.Reason, Is.EqualTo("Cancellation reason").Using(CustomComparers.TypeComparison), "Header not NULL");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new FCREVH();
			dataProvider = new CUSREVProvider(message);
		}
		FCREVH message;
		ICUSREV dataProvider;

		protected override CUSREVProvider GetProvider() => (CUSREVProvider)dataProvider;
	}
}
