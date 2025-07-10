using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Utils;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business
{
	public class RequestHeaderTest : TransactionedTestCase
	{
		public void TestInitialization()
		{
			var request = new HeaderData_Universal();
			AssertEquals(string.Empty, request.OwnerCode);
			AssertEquals(true, request.EnableCodeMapping);
		}
	}

	public class RequestSerializationTest : TestCase
	{
		public void TestSerialization()
		{
			var response = new Response_Universal();
			response.Status = NativeResponseStatus.Rejected;
			response.Informations =
			new[]
			{
				"Information1",
				"Information2"
			};
			response.DataItems =
			new[]
			{
				new XElement("TestElement"),
				new XElement("TestElement2")
			};
			var serializer = new ObjectXmlSerializer<Response_Universal>();
			var xml = serializer.Serialize(response);
			AssertEquals("Response", xml.Name.LocalName);
			AssertEquals("http://www.cargowise.com/Schemas/Universal", xml.Name.NamespaceName);
			Assert(xml.Elements().Any(e => e.Name.LocalName == "Status"));
			Assert(xml.Elements().Any(e => e.Name.LocalName == "Information"));
			Assert(xml.Descendants().Any(e => e.Name.LocalName == "Item"));
			Assert(xml.Elements().Any(e => e.Name.LocalName == "Data"));
			Assert(xml.Descendants().Any(e => e.Name.LocalName == "DataItem"));
		}
	}
}
