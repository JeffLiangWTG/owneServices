using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(SRAREVProvider))]
	sealed class SRAREVProviderTest : InboundDataProviderTestCase<ISRAREV, SRAREVProvider>
	{
		public void TestConstructorThrowsArgumentException() => AssertExceptionThrown<ArgumentException>(() => new SRAREVProvider(null));

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "Not Populated - should be [null]");

				xmlObject.MetaData.MessageIdentifier = "1EA08BA1-FafD-3EF9-9239-e87a78D79dc2";
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("1EA08BA1FafD3EF99239e87a78D79dc2"), "Populated and removed '-'");
			});
		}

		[ExpectNoExceptions]
		public void TestCancelledReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CancelledReferenceNumber, Is.EqualTo(default(string)), "Not Populated - should be [null]");

				xmlObject.Header.CancelledReferenceNumber = "DE000001";
				NUnit.Framework.Assert.That(dataProvider.CancelledReferenceNumber, Is.EqualTo("DE000001"), "Populated");
			});
		}

		[ExpectNoExceptions]
		public void TestCancelledMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CancelledMRN, Is.EqualTo(default(string)), "Not Populated - should be [null]");

				xmlObject.Header.CancelledMRN = "DE000001";
				NUnit.Framework.Assert.That(dataProvider.CancelledMRN, Is.EqualTo("DE000001"), "Populated");
			});
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientEBS()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo(default(string)), "No InterchangeRecipient - should be [null]");

				xmlObject.MetaData.InterchangeRecipient = new NSREVCMetaDataInterchangeRecipient();
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo(default(string)), "No Identification - should be [null]");

				xmlObject.MetaData.InterchangeRecipient.Identification = new NSREVCMetaDataInterchangeRecipientIdentification
				{
					SubsidiaryNumber = "0001"
				};
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo("0001"));
			});
		}

		protected override SRAREVProvider GetProvider() => dataProvider;

		protected override void SetUp()
		{
			base.SetUp();
			xmlObject = new NSREVC
			{
				MetaData = new NSREVCMetaData(),
				Header = new NSREVCHeader()
			};
			dataProvider = new SRAREVProvider(xmlObject);
		}
		NSREVC xmlObject;
		SRAREVProvider dataProvider;
	}
}
