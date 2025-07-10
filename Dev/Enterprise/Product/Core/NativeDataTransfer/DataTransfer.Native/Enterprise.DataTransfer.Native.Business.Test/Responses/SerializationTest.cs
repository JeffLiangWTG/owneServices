using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	public class SerializationTest : TestCase
	{
		public void TestCreateResponse()
		{
			var result = serializer.Serialize(response);

			//"<?xml version='1.0' encoding='utf-16'?>";
			//"<Response>";
			//"  <Status>Rejected</Status>";		
			//"  <EntityInfo>
			//"    <Name>Dummy</Name>
			//"  </EntityInfo>
			//"  <Information>";
			//"    <Item>Error - Some Error</Item>";
			//"    <Item>Warning - Some Warning</Item>";
			//"    <Item>Information - Some Info</Item>";
			//"  </Information>";
			//"  <Data>";
			//"    <DataItem>";
			//"      <Hello />";
			//"    </DataItem>";
			//"    <DataItem>";
			//"      <Bye />";
			//"    </DataItem>";
			//"  </Data>";
			//"</Response>";

			//Entity Set Name Section
			AssertEquals("Expect only one EntityInfo Element in Response Element", 1, result.Elements(ns + "EntityInfo").Count());
			var entityInfoElement = result.Elements(ns + "EntityInfo").Single();

			AssertEquals("Expect only one Name Element in EntityInfo Element", 1, entityInfoElement.Elements(ns + "Name").Count());
			var nameElement = entityInfoElement.Elements(ns + "Name").Single();
			AssertEquals("Dummy", nameElement.Value);

			//Status Section
			AssertEquals("Expect only one Status Element in Response Element", 1, result.Elements(ns + "Status").Count());
			var statusElement = result.Descendants(ns + "Status").Single();
			AssertEquals("Rejected", statusElement.Value);

			//Information Section
			AssertEquals("Expect only one Information Element in Response Element", 1, result.Elements(ns + "Information").Count());
			var informationItemElements = result.Descendants(ns + "Item");
			Assert(informationItemElements.Any(e => e.Value.Contains("Error - Some Error")));
			Assert(informationItemElements.Any(e => e.Value.Contains("Warning - Some Warning")));
			Assert(informationItemElements.Any(e => e.Value.Contains("Information - Some Info")));

			//Data Section
			AssertEquals("Expect only one Data Element in Response Element", 1, result.Elements(ns + "Data").Count());
			var dataElements = result.Descendants(ns + "DataItem");
			AssertEquals(2, dataElements.Count());

			var value = result.Descendants("Hello");
			AssertEquals(1, value.Count());

			value = result.Descendants("Bye");
			AssertEquals(1, value.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();

			response = new Response_Universal
			{
				EntityInfo = new EntityInfo { Name = "Dummy" },
				Status = NativeResponseStatus.Rejected,
				Informations = new[]
				{
					"Error - Some Error",
					"Warning - Some Warning",
					"Information - Some Info"
				},
				DataItems = new[]
				{
					new XElement("Hello"),
					new XElement("Bye")
				}
			};

			serializer = new ObjectXmlSerializer<Response_Universal>();
			ns = ReferenceDataXMLForDeSerialize.NameSpace_Universal;
		}

		Response_Universal response;
		ObjectXmlSerializer<Response_Universal> serializer;
		XNamespace ns;
	}
}
