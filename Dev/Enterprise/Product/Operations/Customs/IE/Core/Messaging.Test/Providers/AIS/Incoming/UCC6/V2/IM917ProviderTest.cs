using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM917ProviderTest : TestCaseWithFactory
	{
		public void TestErrors()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Collection Contains", 2, provider.Errors.Count);
				AssertEquals("Is INegativeAcknowledgementError", true, typeof(INegativeAcknowledgementError).IsAssignableFrom(provider.Errors.First().GetType()));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mock = new Mock<IIM917XmlObject>();
			mock.Setup(o => o.XmlNegativeAcknowledgements).Returns(new[] { new Mock<IXmlNegativeAcknowledgement>().Object, new Mock<IXmlNegativeAcknowledgement>().Object });
			provider = new IM917Provider(mock.Object);
		}
		IM917Provider provider;
	}
}
