using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	public class DeserializationTest : TestCase
	{
		public void TestParseResponse()
		{
			var element = new XElement(ns + "Response",
				new XElement(ns + "EntityInfo",
					new XElement(ns + "Name", "Dummy")),
				new XElement(ns + "Status", "Rejected"),
				new XElement(ns + "Information",
					new XElement(ns + "Item", "Error"),
					new XElement(ns + "Item", "Warning"),
					new XElement(ns + "Item", "Information")),
				new XElement(ns + "Data",
					new XElement(ns + "DataItem", new XElement("Error")),
					new XElement(ns + "DataItem", new XElement("Warning")),
					new XElement(ns + "DataItem", new XElement("Information"))
				)
			);
			response = serialize.Deserialize(element);

			AssertNotNull(response.EntityInfo);
			AssertEquals("Dummy", response.EntityInfo.Name);

			AssertEquals(NativeResponseStatus.Rejected, response.Status);

			AssertEquals(3, response.Informations.Length);
			Assert(response.Informations.Any(i => i == "Error"));
			Assert(response.Informations.Any(i => i == "Warning"));
			Assert(response.Informations.Any(i => i == "Information"));

			AssertEquals(3, response.DataItems.Length);
			Assert(response.DataItems.Any(i => i.Name == "Error"));
			Assert(response.DataItems.Any(i => i.Name == "Warning"));
			Assert(response.DataItems.Any(i => i.Name == "Information"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			serialize = new ObjectXmlSerializer<Response_Universal>();
			ns = ReferenceDataXMLForDeSerialize.NameSpace_Universal;
		}
		Response_Universal response;
		ObjectXmlSerializer<Response_Universal> serialize;
		XNamespace ns;
	}
}
