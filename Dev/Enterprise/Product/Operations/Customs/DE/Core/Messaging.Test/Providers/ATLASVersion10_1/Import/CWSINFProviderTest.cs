using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CWSINFProvider))]
	sealed class CWSINFProviderTest : InboundDataProviderTestCase<ICWSINF, CWSINFProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CWSINFProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			xmlObject.MetaData = new LCWSIEMetaData
			{
				MessageIdentifier = "0624532020"
			};
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0624532020"));
		}

		[ExpectNoExceptions]
		public void TestCurrentProcedure()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CurrentProcedure, Is.EqualTo(string.Empty), "Header is null");

				var header = xmlObject.Header = new LCWSIEHeader();
				NUnit.Framework.Assert.That(dataProvider.CurrentProcedure, Is.EqualTo(string.Empty), "Header.CustomsAuthorisation is null");

				header.CustomsAuthorisation = new LCWSIEHeaderCustomsAuthorisation()
				{
					CurrentProcedure = "ABC123456"
				};
				NUnit.Framework.Assert.That(dataProvider.CurrentProcedure, Is.EqualTo("ABC123456"), "Header.CustomsAuthorisation isn't null");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			xmlObject = new LCWSIE();
			dataProvider = new CWSINFProvider(xmlObject);
		}
		LCWSIE xmlObject;
		CWSINFProvider dataProvider;

		protected override CWSINFProvider GetProvider() => dataProvider;
	}
}
