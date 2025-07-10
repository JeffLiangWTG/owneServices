using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(ARMessage))]
	class ARMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.NewWithValidTestData<ARMessageForTest>();
			AssertEquals(EDIInterchange.ApplicationCodes.ARCustoms, message.ApplicationCode);
		}

		public void TestMessageStreamFormatterType()
		{
			var message = Factory.NewWithValidTestData<ARMessageForTest>();
			AssertType<EDIMessageStreamFormatterForXml>(message.MessageStreamFormatterExposed);
		}
	}

	class ARMessageForTest : ARMessage
	{
		public ARMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString ApplicationCode => base.EM_ApplicationCode;

		public IStreamFormatter MessageStreamFormatterExposed => base.MessageStreamFormatter;
	}
}
