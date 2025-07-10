using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInterchange))]
	class EMCSInterchangeTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<EMCSInterchange>();
			AssertEquals("Application code should be GEX", EDIMessage.ApplicationCodes.GbCustomsEMCS, interchange.EI_ApplicationCode);
		}

		public void TestEMCSCustomsBusinessResponse()
		{
			var interchange = Factory.New<EMCSInterchange>();
			CombineAssertions(() =>
			{
				interchange.EI_BodyText = ResponseFull;
				var customsBusinessResponse = interchange.EMCSCustomsBusinessResponse;
				AssertNotNull(customsBusinessResponse);
				AssertEquals("EHUBTRACKINIDVALUE", customsBusinessResponse.EHubTrackingID);
				AssertEquals("JOBNUMBERVALUE", customsBusinessResponse.JobNumber);
				AssertEquals("E00000902", customsBusinessResponse.ServiceReference);
				AssertEquals("<IE801><!-- Example IE801 message --></IE801>", customsBusinessResponse.BodyXml);

				interchange.EI_BodyText = ResponseFullWithoutJsonEncapsulation;
				customsBusinessResponse = interchange.EMCSCustomsBusinessResponse;
				AssertNotNull(customsBusinessResponse);
				AssertEquals("EHUBTRACKINIDVALUE", customsBusinessResponse.EHubTrackingID);
				AssertEquals("JOBNUMBERVALUE", customsBusinessResponse.JobNumber);
				AssertEquals("E00000902", customsBusinessResponse.ServiceReference);
				AssertEquals("<IE801><!-- Example IE801 message --></IE801>", customsBusinessResponse.BodyXml);

				interchange.EI_BodyText = ZString.Empty;
				customsBusinessResponse = interchange.EMCSCustomsBusinessResponse;
				AssertNotNull(customsBusinessResponse);
				AssertEquals(ZString.Empty, customsBusinessResponse.EHubTrackingID);
				AssertEquals(ZString.Empty, customsBusinessResponse.JobNumber);
				AssertEquals(ZString.Empty, customsBusinessResponse.ServiceReference);
				AssertEquals(ZString.Empty, customsBusinessResponse.BodyXml);
			});
		}

		const string ResponseFull = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <eHubTrackingId>EHUBTRACKINIDVALUE</eHubTrackingId>
    <JobNumber>JOBNUMBERVALUE</JobNumber>
    <ServiceReference>E00000902</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    W3siZW5jb2RlZE1lc3NhZ2UiOiJQRWxGT0RBeFBqd2hMUzBnUlhoaGJYQnNaU0JKUlRnd01TQnRaWE56WVdkbElDMHRQand2U1VVNE1ERSsiLCJtZXNzYWdlVHlwZSI6IklFODAxIiwiY3JlYXRlZE9uIjoiMjAyNC0wMi0wN1QxNDozNjo1Ni4wNTQ1MzZaIn1d
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string ResponseFullWithoutJsonEncapsulation = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <eHubTrackingId>EHUBTRACKINIDVALUE</eHubTrackingId>
    <JobNumber>JOBNUMBERVALUE</JobNumber>
    <ServiceReference>E00000902</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    PElFODAxPjwhLS0gRXhhbXBsZSBJRTgwMSBtZXNzYWdlIC0tPjwvSUU4MDE+
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
	}
}
