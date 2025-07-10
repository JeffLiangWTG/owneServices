using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService.Testing
{
	sealed class ResponseConverterTest : TestCase
	{
		public void TestConvert_RejectedMessage()
		{
			XNamespace cw = ReferenceDataXMLForDeSerialize.NameSpace_Universal;
			var response =
				new XElement(cw + "Response",
					new XElement(cw + "Status", "Rejected"),
					new XElement(cw + "Information",
						new XElement(cw + "Item", "Test Message")));

			var result = new ResponseConverter().Convert(response);
			AssertEquals("Should have error when message is rejected", true, result.HasError);
			AssertEquals(response.ToString(), result.ErrorMessage);
		}

		public void TestResponseConverterConvertingFailedClientMessage()
		{
			string failingMessage = @"

<Response>
  <Status>Accepted</Status>
  <Information>
    <Item>Information - JobOrderHeader - 1 inserts, 0 updates, 0 deletes</Item>
  </Information>
</Response>

".Trim();

			var response = XElement.Parse(failingMessage);

			var result = new ResponseConverter().Convert(response);
			CombineAssertions(delegate
			{
				AssertEquals("result.HasError", true, result.HasError);
				AssertEquals("result.ErrorMessage", ResponseConverter.ErrorMessageMissingStatusTag, result.ErrorMessage);
				AssertEquals("result.MessageID", null, result.MessageID);
				AssertMultilineASCIIEquals("result.ResponseMessage", failingMessage, result.ResponseMessage);
			});

			string passingMessage = @"

<Response xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Status>Accepted</Status>
  <Information>
    <Item>Information - JobOrderHeader - 1 inserts, 0 updates, 0 deletes</Item>
  </Information>
</Response>

".Trim();

			response = XElement.Parse(passingMessage);

			result = new ResponseConverter().Convert(response);
			CombineAssertions(delegate
			{
				AssertEquals("result.HasError", false, result.HasError);
				AssertEquals("result.ErrorMessage", null, result.ErrorMessage);
				AssertEquals("result.MessageID", null, result.MessageID);
				AssertMultilineASCIIEquals("result.ResponseMessage", passingMessage, result.ResponseMessage);
			});
		}
	}
}
