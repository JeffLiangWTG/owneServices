using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTRAProvider))]
	sealed class CUSTRAProviderTest : InboundDataProviderTestCase<ICUSTRA, CUSTRAProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTRAProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "No MetaData");
				message.MetaData = new GCTRAGMetaData()
				{
					MessageIdentifier = "CUSTRA58750000000375302250219160050"
				};
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSTRA58750000000375302250219160050"), "Value");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(ZString.Empty), "No Header");
			message.Header = new GCTRAGHeader()
			{
				ReferenceNumber = "ATB150000620520195875"
			};
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000620520195875").Using(CustomComparers.TypeComparison), "Value");
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "No Header - should be [null]");

			message.Header = new GCTRAGHeader { MRN = "23DE12345678901234" };

			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE12345678901234"));
		}

		[ExpectNoExceptions]
		public void TestForwardedDate()
		{
			NUnit.Framework.Assert.That(dataProvider.ForwardedDate, Is.EqualTo(ZDate.Empty), "No Header");
			message.Header = new GCTRAGHeader()
			{
				ForwardedDateSpecified = true,
				ForwardedDate = new DateTime(2020, 10, 07)
			};
			NUnit.Framework.Assert.That(dataProvider.ForwardedDate, Is.EqualTo(new ZDate(2020, 10, 07)), "Is Specified");
			message.Header.ForwardedDateSpecified = false;
			NUnit.Framework.Assert.That(dataProvider.ForwardedDate, Is.EqualTo(ZDate.Empty), "Not Specified");
		}

		[ExpectNoExceptions]
		public void TestReason()
		{
			NUnit.Framework.Assert.That(dataProvider.Reason, Is.EqualTo(ZString.Empty), "No Header");
			message.Header = new GCTRAGHeader()
			{
				Reason = "Reason 1"
			};
			NUnit.Framework.Assert.That(dataProvider.Reason, Is.EqualTo("Reason 1").Using(CustomComparers.TypeComparison), "Value");
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new GCTRAG();
			dataProvider = new CUSTRAProvider(message);
		}
		GCTRAG message;
		ICUSTRA dataProvider;

		protected override CUSTRAProvider GetProvider() => (CUSTRAProvider)dataProvider;
	}
}
