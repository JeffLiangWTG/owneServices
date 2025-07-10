using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestDeserializerReferenceDataTest : TestCase
	{
		readonly XNamespace ns = ReferenceDataXMLForDeSerialize.NameSpace_Universal;

		public void TestDeserialize_Verbose()
		{
			var entity = new XElement(ns + "Order",
									new XElement(ns + "JobOrderHeader"));
			var entity2 = new XElement(ns + "Order2",
									new XElement(ns + "JobOrderHeader"));
			var xml = new XElement(ns + "ReferenceData",
				new XElement(ns + "Header",
					new XElement(ns + "OwnerCode", "TestOrg"),
					new XElement(ns + "EnableCodeMapping", true)),
				new XElement(ns + "Body", entity, entity2));
			var request = deserializer.Deserialize(xml);

			AssertNotNull(request.Settings);
			AssertEquals("TestOrg", request.Settings.OwnerCode);
			AssertEquals(true, request.Settings.EnableCodeMapping);

			AssertNotNull(request.EntitySets);
			AssertEquals(2, request.EntitySets.Count());
			AssertMultilineASCIIEquals(entity.ToString().Replace("  ", ""), request.EntitySets.First().ToString().Replace("  ", ""));
		}

		public void TestDeserialize_EmptyBody()
		{
			var xml = new XElement(ns + "ReferenceData", new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);
			AssertNull(request.Settings);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserializer_BodyInFrontOfHeader()
		{
			var xml = new XElement(ns + "ReferenceData",
				new XElement(ns + "Header"),
				new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);

			AssertNotNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithHeader()
		{
			var xml = new XElement(ns + "ReferenceData",
				new XElement(ns + "Header"),
				new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);

			AssertNotNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithoutBody()
		{
			var xml = new XElement(ns + "ReferenceData");
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				() =>
				{
					var request = deserializer.Deserialize(xml);
					request.EntitySets.Count();
				});
		}

		public void TestDeserialize_OtherElementAfterBody()
		{
			var xml = new XElement(ns + "ReferenceData",
				new XElement(ns + "Body"),
				new XComment("Test"),
				new XElement(ns + "Test"));
			var request = deserializer.Deserialize(xml);
			AssertNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithCommentOnTop()
		{
			var xml = new XElement(ns + "ReferenceData",
				new XComment("Test"),
				new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);
			AssertNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithoutReferenceData()
		{
			var xml = new XElement(ns + "Airline");
			AssertExceptionThrown(typeof(XmlException),
				() =>
				{
					deserializer.Deserialize(xml);
				});
		}

		public void TestDeserialize_InvalidXML()
		{
			var xml =
				@"<ReferenceData xmlns='http://www.cargowise.com/Schemas/Universal'><Body></ReferenceData></Body>".Replace("'", "\"");
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream, Encoding.UTF8);
				writer.Write(xml);
				writer.Flush();

				AssertExceptionThrown(typeof(XmlException),
					() =>
					{
						var request = deserializer.Deserialize(stream);
						request.EntitySets.Count();
					});

				writer.Dispose();
			}
		}

		public void TestDeserialize_InvalidXML2()
		{
			var xml =
				@"<ReferenceData xmlns='http://www.cargowise.com/Schemas/Universal'><Body></ReferenceData>".Replace("'", "\"");
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream, Encoding.UTF8);
				writer.Write(xml);
				writer.Flush();

				AssertExceptionThrown(typeof(XmlException),
					() =>
					{
						var request = deserializer.Deserialize(stream);
						request.EntitySets.Count();
					});

				writer.Dispose();
			}
		}

		public void TestDeserialize_EmptyBody2()
		{
			var xml =
				@"<ReferenceData xmlns='http://www.cargowise.com/Schemas/Universal'><Body/></ReferenceData>".Replace("'", "\"");
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream, Encoding.UTF8);
				writer.Write(xml);
				writer.Flush();

				var request = deserializer.Deserialize(stream);
				AssertNull(request.Settings);
				AssertNotNull(request.EntitySets);
				AssertEquals(0, request.EntitySets.Count());

				writer.Dispose();
			}
		}
		protected override void SetUp()
		{
			base.SetUp();
			deserializer = new RequestDeserializer_Universal();
		}
		RequestDeserializer deserializer;
	}
}
