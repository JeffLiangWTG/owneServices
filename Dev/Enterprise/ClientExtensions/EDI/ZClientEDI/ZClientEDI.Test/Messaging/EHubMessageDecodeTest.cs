using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.Messaging.Testing
{
	class EHubMessageDecodeTest : TestCaseWithFactory
	{
		public void TestUnpackMessage()
		{
			Xsd.CustomerServiceRequest rawRequest = new Xsd.CustomerServiceRequest();
			var image = new Bitmap(1, 1);
			image.SetPixel(0, 0, Color.White);
			var stream = new MemoryStream();
			image.Save(stream, System.Drawing.Imaging.ImageFormat.Tiff);
			byte[] imageArray = stream.ToArray();
			Xsd.CustomerServiceRequestAttachment attachment1 = rawRequest.Attachments.AddNew();
			attachment1.FileName = "Image";
			attachment1.Data = imageArray;
			string encodedtext1 = SystemMessage.Pack(ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest)), rawRequest, "request.tmp");
			string request = string.Concat("<CustomerServiceRequest compressed=\"1\" xmlns=\"http://www.cargowise.com/Schemas/System\">", encodedtext1, "</CustomerServiceRequest>");
			string xmlData1 = EHubMessageDecoder.UnpackMessage(request);
			AssertContains("This data has been removed", xmlData1);
			Xsd.CustomerServiceResponse rawResponse = new Xsd.CustomerServiceResponse();
			Xsd.CustomerServiceResponseAttachment attachment2 = rawResponse.Attachments.AddNew();
			attachment2.FileName = "Image";
			attachment2.Data = imageArray;
			string encodedtext2 = SystemMessage.Pack(ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse)), rawResponse, "response.tmp");
			string response = string.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?><SystemInterchange xmlns=\"http://www.cargowise.com/Schemas/System\"><Header><SenderID>EDIAUSSYD</SenderID><RecipientID>GEOGSGPRO</RecipientID></Header><Body><CustomerServiceResponse compressed=\"1\">", encodedtext2, "</CustomerServiceResponse></Body></SystemInterchange>");
			string xmlData2 = EHubMessageDecoder.UnpackMessage(response);
			AssertContains("This data has been removed", xmlData2);
			string xmlData3 = EHubMessageDecoder.UnpackMessage("");
			AssertEquals("This message cannot be decoded or is empty. Customer service message with content is expected.", xmlData3);
		}

		public void TestGetCustomerServiceMessage()
		{
			EHubMessageDecoder decoder = new EHubMessageDecoder();
			Xsd.CustomerServiceRequest rawRequest = new Xsd.CustomerServiceRequest();
			rawRequest.IncidentSummary = "This is a summary of the request.";
			rawRequest.IncidentDetails = @"This request contains details.

Of course. This is fake data.";
			string encodedtext = SystemMessage.Pack(ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest)), rawRequest, "request.tmp");
			string request = string.Concat("<CustomerServiceRequest compressed=\"1\" xmlns=\"http://www.cargowise.com/Schemas/System\">", encodedtext, "</CustomerServiceRequest>");
			string message = EHubMessageDecoder.GetCustomerServiceMessage(request);
			AssertEquals(encodedtext, message);
			Xsd.CustomerServiceResponse rawResponse = new Xsd.CustomerServiceResponse();
			rawResponse.IncidentSummary = "This is a summary of the reponse.";
			rawResponse.IncidentDetails = @"This response contains details.

Of course. This is fake data.";
			string encodedtext2 = SystemMessage.Pack(ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse)), rawResponse, "response.tmp");
			string response = string.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?><SystemInterchange xmlns=\"http://www.cargowise.com/Schemas/System\"><Header><SenderID>EDIAUSSYD</SenderID><RecipientID>GEOGSGPRO</RecipientID></Header><Body><CustomerServiceResponse compressed=\"1\">", encodedtext2, "</CustomerServiceResponse></Body></SystemInterchange>");
			string message2 = EHubMessageDecoder.GetCustomerServiceMessage(response);
			AssertEquals(encodedtext2, message2);
		}
	}
}
