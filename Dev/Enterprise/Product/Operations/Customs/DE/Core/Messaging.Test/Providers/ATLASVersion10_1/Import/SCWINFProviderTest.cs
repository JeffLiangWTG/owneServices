using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(SCWINFProvider))]
	sealed class SCWINFProviderTest : InboundDataProviderTestCase<ISCWINF, SCWINFProvider>
	{
		[ExpectNoExceptions]
		public void TestMRN() => CombineAssertions(() =>
		{
			message.Header = null;
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "When no Header is set - should be [null]");

			message.Header = new LSCWIFHeader
			{
				MRN = "24DE12345678901234",
			};
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE12345678901234"), "When Header/MRN is set");
		});

		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWINFProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "MetaData is null - should be [null]");
				message.MetaData = new LSCWIFMetaData
				{
					MessageIdentifier = "MESSAGEIDENTIFIER"
				};
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("MESSAGEIDENTIFIER"), "MetaData isn't null");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(default(string)), "Header is null - should be [null]");
				message.Header = new LSCWIFHeader
				{
					ReferenceNumber = "ATC400533951120184849"
				};
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATC400533951120184849"), "Header isn't null");
			});
		}

		[ExpectNoExceptions]
		public void TestCurrentProcedure()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CurrentProcedure, Is.EqualTo(default(string)), "Header is null - should be [null]");

				var header = message.Header = new LSCWIFHeader();
				NUnit.Framework.Assert.That(dataProvider.CurrentProcedure, Is.EqualTo(default(string)), "Header.CustomsAuthorisation is null - should be [null]");

				header.CustomsAuthorisation = new LSCWIFHeaderCustomsAuthorisation
				{
					CurrentProcedure = "ABC123456"
				};
				NUnit.Framework.Assert.That(dataProvider.CurrentProcedure, Is.EqualTo("ABC123456"), "Header.CustomsAuthorisation isn't null");
			});
		}

		protected override SCWINFProvider GetProvider() => (SCWINFProvider)dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			message = new LSCWIF();
			dataProvider = new SCWINFProvider(message);
		}

		LSCWIF message;
		ISCWINF dataProvider;
	}
}
