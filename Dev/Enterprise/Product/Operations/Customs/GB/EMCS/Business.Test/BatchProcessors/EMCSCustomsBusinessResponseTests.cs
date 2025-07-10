using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	class EMCSCustomsBusinessResponseTests : TestCase
	{
		public void TestResponseWhenNoBody()
		{
			var response = new EMCSCustomsBusinessResponse(ResponseWithNoBody);
			CombineAssertions(() =>
			{
				AssertEquals(ResponseWithNoBody, response.Xml);
				AssertEquals("ServiceReference", "E00000902", response.ServiceReference);
				AssertEquals("BodyXml should be empty", ZString.Empty, response.BodyXml);
			});
		}

		public void TestResponseWhenNoHeader()
		{
			CombineAssertions(() =>
			{
				var response = new EMCSCustomsBusinessResponse(ResponseWithNoHeaderAndMessages);
				AssertEquals(ResponseWithNoHeaderAndMessages, response.Xml);
				AssertEquals("ServiceReference", ZString.Empty, response.ServiceReference);
				AssertEquals("BodyXml", ResponseBodyDecoded, response.BodyXml);

				response = new EMCSCustomsBusinessResponse(ResponseWithNoHeaderAndNoMessages);
				AssertEquals(ResponseWithNoHeaderAndNoMessages, response.Xml);
				AssertEquals("ServiceReference", ZString.Empty, response.ServiceReference);
				AssertEquals("BodyXml should be empty", ZString.Empty, response.BodyXml);
			});
		}

		public void TestFullResponse()
		{
			CombineAssertions(() =>
			{
				var response = new EMCSCustomsBusinessResponse(ResponseFull);
				AssertEquals(ResponseFull, response.Xml);
				AssertEquals("ServiceReference", "E00000900", response.ServiceReference);
				AssertEquals("BodyXml", ResponseBodyDecoded, response.BodyXml);

				response = new EMCSCustomsBusinessResponse(ResponseFullWithoutJsonEncapsulation);
				AssertEquals(ResponseFullWithoutJsonEncapsulation, response.Xml);
				AssertEquals("ServiceReference", "E00000900", response.ServiceReference);
				AssertEquals("BodyXml", ResponseBodyDecoded, response.BodyXml);
			});
		}

		public void TestResponseHeader()
		{
			var response = new EMCSCustomsBusinessResponse(ResponseWithHeaders);
			CombineAssertions(() =>
			{
				AssertEquals(ResponseWithHeaders, response.Xml);
				AssertEquals("eHubTrackingId", "EHUBTRACKINIDVALUE", response.EHubTrackingID);
				AssertEquals("JobNumber", "JOBNUMBERVALUE", response.JobNumber);
				AssertEquals("ServiceReference", "SERVICEREFERENCEVALUE", response.ServiceReference);
			});
		}

		const string ResponseWithNoBody = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <ServiceReference>E00000902</ServiceReference>
  </s0:ResponseHeader>
</s0:GBCustomsBusinessResponse>";
		const string ResponseWithNoHeaderAndMessages = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    W3siZW5jb2RlZE1lc3NhZ2UiOiJQRWxGT0RBeFBqd2hMUzBnUlhoaGJYQnNaU0JKUlRnd01TQnRaWE56WVdkbElDMHRQand2U1VVNE1ERSsiLCJtZXNzYWdlVHlwZSI6IklFODAxIiwiY3JlYXRlZE9uIjoiMjAyNC0wMi0wN1QxNDozNjo1Ni4wNTQ1MzZaIn1d
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string ResponseWithNoHeaderAndNoMessages = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string ResponseFull = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <ServiceReference>E00000900</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    W3siZW5jb2RlZE1lc3NhZ2UiOiJQRWxGT0RBeFBqd2hMUzBnUlhoaGJYQnNaU0JKUlRnd01TQnRaWE56WVdkbElDMHRQand2U1VVNE1ERSsiLCJtZXNzYWdlVHlwZSI6IklFODAxIiwiY3JlYXRlZE9uIjoiMjAyNC0wMi0wN1QxNDozNjo1Ni4wNTQ1MzZaIn1d
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string ResponseFullWithoutJsonEncapsulation = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <ServiceReference>E00000900</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    PElFODAxPjwhLS0gRXhhbXBsZSBJRTgwMSBtZXNzYWdlIC0tPjwvSUU4MDE+
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string ResponseBodyDecoded = "<IE801><!-- Example IE801 message --></IE801>";
		const string ResponseWithHeaders = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <eHubTrackingId>EHUBTRACKINIDVALUE</eHubTrackingId>
    <JobNumber>JOBNUMBERVALUE</JobNumber>
    <ServiceReference>SERVICEREFERENCEVALUE</ServiceReference>
  </s0:ResponseHeader>
</s0:GBCustomsBusinessResponse>";
	}
}
