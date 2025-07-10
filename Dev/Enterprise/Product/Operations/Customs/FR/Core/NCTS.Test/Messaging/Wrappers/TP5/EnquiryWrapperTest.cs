using System;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class EnquiryWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnquiryWrapper>
	{
		public void TestTC11DeliveryDate()
		{
			AssertEquals("TC11DeliveryDate should be equal to DateTime(2024, 08, 29)", new DateTime(2024, 08, 29), Provider.TC11DeliveryDate);
		}

		public void TestText()
		{
			AssertEquals("Text should be equal to queryinformation", "queryinformation", Provider.Text);
		}

		protected override EnquiryWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.TC11DeliveryDate = new DateTime(2024, 08, 29);
			sendingObject.QueryInformation = "queryinformation";

			return EnquiryWrapper.New(sendingObject);
		}
	}
}
