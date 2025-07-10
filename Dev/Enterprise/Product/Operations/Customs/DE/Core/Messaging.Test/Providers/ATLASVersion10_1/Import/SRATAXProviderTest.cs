using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(SRATAXProvider))]
	sealed class SRATAXProviderTest : InboundDataProviderTestCase<ISRATAX, SRATAXProvider>
	{
		public void TestConstructorThrowsArgumentException() => AssertExceptionThrown<ArgumentException>(() => new SRATAXProvider(null));

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				xmlObject.MetaData.MessageIdentifier = "FB9F073C-27F7-42EF-9109-7EA74D46E93F";
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("FB9F073C27F742EF91097EA74D46E93F"), "Populated");

				xmlObject.MetaData = null;
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "MetaData null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				xmlObject.Header.ReferenceNumber = "987654321";
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("987654321"), "Populated");

				xmlObject.Header = null;
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(default(string)), "Header null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				xmlObject.Header.MRN = "23DE586601055987B7";
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE586601055987B7"), "Populated");

				xmlObject.Header = null;
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "Header null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				xmlObject.Header.LocalReferenceNumber = "LOCALREFERENCENUMBER";
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("LOCALREFERENCENUMBER"), "Populated");

				xmlObject.Header = null;
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)), "Header null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestTaxChangeAssessmentType()
		{
			CombineAssertions(() =>
			{
				xmlObject.Header.TaxChangeAssessmentType = "002";
				NUnit.Framework.Assert.That(dataProvider.TaxChangeAssessmentType, Is.EqualTo("002"), "Populated");

				xmlObject.Header = null;
				NUnit.Framework.Assert.That(dataProvider.TaxChangeAssessmentType, Is.EqualTo(default(string)), "Header null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestTaxAssessmentCreationDate()
		{
			CombineAssertions(() =>
			{
				xmlObject.Header.TaxAssessmentCreationDate = ZDateTime.BrettsBirthday.ToDateTime();
				NUnit.Framework.Assert.That(dataProvider.TaxAssessmentCreationDate, Is.EqualTo(ZDateTime.BrettsBirthday).Using(CustomComparers.TypeComparison), "Populated");

				xmlObject.Header = null;
				NUnit.Framework.Assert.That(dataProvider.TaxAssessmentCreationDate, Is.Null);
			});
		}

		[ExpectNoExceptions]
		public void TestMaturityDate()
		{
			CombineAssertions(() =>
			{
				xmlObject.Header.MaturityDate = ZDateTime.BrettsBirthday.ToDateTime();
				NUnit.Framework.Assert.That(dataProvider.MaturityDate, Is.EqualTo(ZDateTime.BrettsBirthday).Using(CustomComparers.TypeComparison), "Populated");

				xmlObject.Header = null;
				NUnit.Framework.Assert.That(dataProvider.MaturityDate, Is.Null);
			});
		}

		[ExpectNoExceptions]
		public void TestInterchangeRecipientEBS()
		{
			CombineAssertions(() =>
			{
				xmlObject.MetaData.InterchangeRecipient.Identification.SubsidiaryNumber = "0001";
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo("0001"), "Populated");

				xmlObject.MetaData.InterchangeRecipient.Identification = null;
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo(default(string)), "Identification null - should be [null]");

				xmlObject.MetaData.InterchangeRecipient = null;
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo(default(string)), "InterchangeRecipient null - should be [null]");

				xmlObject.MetaData = null;
				NUnit.Framework.Assert.That(dataProvider.InterchangeRecipientEBS, Is.EqualTo(default(string)), "MetaData null - should be [null]");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			xmlObject = new NSTAXK
			{
				MetaData = new NSTAXKMetaData()
				{
					InterchangeRecipient = new NSTAXKMetaDataInterchangeRecipient()
					{
						Identification = new NSTAXKMetaDataInterchangeRecipientIdentification()
					}
				},
				Header = new NSTAXKHeader()
			};
			dataProvider = new SRATAXProvider(xmlObject);
		}
		NSTAXK xmlObject;
		SRATAXProvider dataProvider;

		protected override SRATAXProvider GetProvider() => dataProvider;
	}
}
