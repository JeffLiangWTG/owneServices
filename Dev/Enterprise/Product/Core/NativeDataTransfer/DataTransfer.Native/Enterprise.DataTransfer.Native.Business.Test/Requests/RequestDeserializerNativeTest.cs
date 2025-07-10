using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestDeserializerNativeTest : TestCase
	{
		readonly XNamespace ns = ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native;

		public void TestDeserializeDataContext()
		{
			var xml =
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:nv='http://www.cargowise.com/Schemas/Native/2011/11'>
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>4</TriggerCount>
					<TriggerDate>2014-12-15T08:31:00+11:00</TriggerDate>
					<TriggerDescription>adfsd</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version='2.0'>
				</Rate>
			</Body>
		</Native>";

			var element = XElement.Parse(xml);

			var deserializer = new RequestDeserializer_Versioned_Native();
			var request = deserializer.Deserialize(element);

			AssertNotNull(request);
			AssertNotNull(request.Settings);
			AssertNotNull(request.Settings.DataContext);
			AssertEquals("EDICUS", request.Settings.OwnerCode);
			Assert(request.Settings.EnableCodeMapping);
			AssertEquals("EVT", request.Settings.DataContext.ActionPurpose.Code);
			AssertEquals(4, request.Settings.DataContext.TriggerCount);
			AssertEquals("EDI", request.Settings.DataContext.EnterpriseID);
			AssertEquals("EDT", request.Settings.DataContext.EventType.Code);
		}

		public void TestDeserializeDataContext_HeaderData_WeirdAndFalse()
		{
			var xml =
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>FaLsE</EnableCodeMapping>
				<nv:DataContext xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:nv='http://www.cargowise.com/Schemas/Native/2011/11'>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version='2.0'>
				</Rate>
			</Body>
		</Native>";

			var element = XElement.Parse(xml);

			var deserializer = new RequestDeserializer_Versioned_Native();
			var request = deserializer.Deserialize(element);

			AssertNotNull(request);
			AssertNotNull(request.Settings);
			AssertNotNull(request.Settings.DataContext);
			AssertEquals("EDICUS", request.Settings.OwnerCode);
			Assert(!request.Settings.EnableCodeMapping);
		}

		public void TestDeserializeDataContext_HeaderData_WeirdAndTrue()
		{
			var xml =
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>TrUe</EnableCodeMapping>
				<nv:DataContext xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:nv='http://www.cargowise.com/Schemas/Native/2011/11'>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version='2.0'>
				</Rate>
			</Body>
		</Native>";

			var element = XElement.Parse(xml);

			var deserializer = new RequestDeserializer_Versioned_Native();
			var request = deserializer.Deserialize(element);

			AssertNotNull(request);
			AssertNotNull(request.Settings);
			AssertNotNull(request.Settings.DataContext);
			AssertEquals("EDICUS", request.Settings.OwnerCode);
			Assert(request.Settings.EnableCodeMapping);
		}

		public void TestHeaderExceptionShouldBeNative()
		{
			var xml =
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping></EnableCodeMappin>
				<nv:DataContext xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:nv='http://www.cargowise.com/Schemas/Native/2011/11'>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version='2.0'>
				</Rate>
			</Body>
		</Native>";

			var xmlSessionTracker = new DummyXmlImportLogger();
			var handler = new NativeXmlRequestHandler(NativeDataTypeList.Codes.Organization, xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				request.SetMessageTextSource(stream);
				AssertExceptionThrown<NativeXMLUserVisibleException>(() => handler.Process(request));
			}
		}

		public void TestDeserializeDataContext_HeaderData_InvalidBooleanValue()
		{
					var xml =
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>TRUE</EnableCodeMapping>
				<nv:DataContext xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:nv='http://www.cargowise.com/Schemas/Native/2011/11'>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version='2.0'>
				</Rate>
			</Body>
		</Native>";

			var element = XElement.Parse(xml);

			var deserializer = new RequestDeserializer_Versioned_Native();
			var request = deserializer.Deserialize(element);

			AssertNotNull(request);
			AssertNotNull(request.Settings);
			AssertNotNull(request.Settings.DataContext);
			AssertEquals("EDICUS", request.Settings.OwnerCode);
			Assert(request.Settings.EnableCodeMapping);

			var xml2 =
	@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>AAA</EnableCodeMapping>
				<nv:DataContext xmlns='http://www.cargowise.com/Schemas/Universal/2011/11' xmlns:nv='http://www.cargowise.com/Schemas/Native/2011/11'>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version='2.0'>
				</Rate>
			</Body>
		</Native>";

			var element2 = XElement.Parse(xml2);

			var deserializer2 = new RequestDeserializer_Versioned_Native();

			AssertExceptionThrown<InvalidOperationException>(() => deserializer.Deserialize(element2));
		}

		public void TestDeserialize_Verbose()
		{
			var entity = new XElement(ns + "Order",
									new XElement(ns + "JobOrderHeader"));
			var entity2 = new XElement(ns + "Order2",
									new XElement(ns + "JobOrderHeader"));
			var xml = new XElement(ns + "Native",
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
			var xml = new XElement(ns + "Native", new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);
			AssertNull(request.Settings);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserializer_BodyInFrontOfHeader()
		{
			var xml = new XElement(ns + "Native",
				new XElement(ns + "Header"),
				new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);

			AssertNotNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithHeader()
		{
			var xml = new XElement(ns + "Native",
				new XElement(ns + "Header"),
				new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);

			AssertNotNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithoutBody()
		{
			var xml = new XElement(ns + "Native");
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException),
				() =>
				{
					var request = deserializer.Deserialize(xml);
					request.EntitySets.Count();
				});
		}

		public void TestDeserialize_OtherElementAfterBody()
		{
			var xml = new XElement(ns + "Native",
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
			var xml = new XElement(ns + "Native",
				new XComment("Test"),
				new XElement(ns + "Body"));
			var request = deserializer.Deserialize(xml);
			AssertNull(request.Settings);
			AssertNotNull(request.EntitySets);
			AssertEquals(0, request.EntitySets.Count());
		}

		public void TestDeserialize_WithoutNative()
		{
			var xml = new XElement(ns + "Order");
			AssertExceptionThrown(typeof(XmlException),
				() =>
				{
					deserializer.Deserialize(xml);
				});
		}

		public void TestDeserialize_InvalidXML()
		{
			var xml =
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native'><Body></Native></Body>".Replace("'", "\"");
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
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native'><Body></Native>".Replace("'", "\"");
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
				@"<Native xmlns='http://www.cargowise.com/Schemas/Native'><Body/></Native>".Replace("'", "\"");
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
			deserializer = new RequestDeserializer_Unversioned_Native();
		}
		RequestDeserializer deserializer;
	}
}
