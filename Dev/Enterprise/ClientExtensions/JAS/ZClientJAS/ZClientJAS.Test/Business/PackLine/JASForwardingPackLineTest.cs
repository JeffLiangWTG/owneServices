using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class JASForwardingPackLineTest : ForwardingPackLineTest
	{
		public void TestRightTypeIsReturnedFromParentBizOs()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AssertEquals(typeof(JASForwardingPackLine), shipment.OuterPackLines.AddNew().GetType());
			AssertEquals(typeof(JASForwardingPackLine), shipment.InnerPackLines.AddNew().GetType());
		}

		public void TestShouldSubClassFromForwardingPackLine()
		{
			Assert("Should sub-class from ForwardingPackLine", typeof(JASForwardingPackLine).IsSubclassOf(typeof(ForwardingPackLine)));
		}

		public void TestLinePriceCurrency()
		{
			JASForwardingPackLine packLine = Factory.New<JASForwardingPackLine>();
			packLine.JL_CustomAttrib1 = "AUD";
			AssertEquals("AUD", packLine.LinePriceCurrency);
			packLine.LinePriceCurrency = "HKD";
			AssertEquals("HKD", packLine.JL_CustomAttrib1);
		}

		public void TestSetDefaultValues()
		{
			JASForwardingPackLine packLine = Factory.New<JASForwardingPackLine>();
			AssertEquals("Should be set as the current company's currency", Core.Constants.CurrencyCodes.Australia, packLine.LinePriceCurrency);
		}
	}
}
