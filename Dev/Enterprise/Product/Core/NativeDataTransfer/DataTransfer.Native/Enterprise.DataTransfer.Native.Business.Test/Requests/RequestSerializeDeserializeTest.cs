using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestSerializeDeserializeTest : TestCase
	{
		public void TestSerialize_Deserialize_EmptyRequest()
		{
			var input = new Request();
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(stream, input);
				var thang = Encoding.UTF8.GetString(stream.ToArray());
				var output = deserializer.Deserialize(stream);
				AssertNotNull("output.Settings - we don't REALLY want it, but as long as it only contains empty values that's fine.", output.Settings);
				AssertEquals("output.Settings.DataContext", null, output.Settings.DataContext);
				AssertEquals("output.Settings.EnableCodeMapping", true, output.Settings.EnableCodeMapping);
				AssertEquals("output.Settings.OwnerCode", string.Empty, output.Settings.OwnerCode);
				AssertEquals("output.Settings.MessageNumberCollection.Any()", false, output.Settings.MessageNumberCollection.Any());
				AssertEquals("Body item count", 0, output.EntitySets.Count());
			}
		}

		public void TestSerialize_Deserialize_NormalRequest()
		{
			var input = new Request
							{
								Settings = new HeaderData_Versioned_Native(),
								EntitySets = new[] { new XElement("Test1"), new XElement("Test2") }
							};
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(stream, input);
				var thang = Encoding.UTF8.GetString(stream.ToArray());
				var output = deserializer.Deserialize(stream);
				AssertEquals("Header.EnableCodeMapping", input.Settings.EnableCodeMapping, output.Settings.EnableCodeMapping);
				AssertEquals("Header.OwnerCode", input.Settings.OwnerCode, output.Settings.OwnerCode);
				AssertEquals("Body item count", 2, output.EntitySets.Count());
			}
		}

		public void TestSerialize_Deserialize_Header_With_MessageNumberCollection()
		{
			var input = new Request
			{
				Settings = new HeaderData_Versioned_Native() { MessageNumberCollection = new List<MessageNumberWrapper>() { MessageNumberWrapper.New(new MessageNumber() { Type = MessageNumberType.TrackingID, Value = new ZString("TestTrackingID") } ),
																															MessageNumberWrapper.New(new MessageNumber() { Type = MessageNumberType.InterchangeNumber, Value = new ZString("TestInterchangeNumber") } ),
																															MessageNumberWrapper.New(new MessageNumber() { Type = MessageNumberType.MessageNumber, Value = new ZString("TestMessageNumber") } ) }
				},
				EntitySets = new[] { new XElement("Test1"), new XElement("Test2")  }
			};
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(stream, input);
				var thang = Encoding.UTF8.GetString(stream.ToArray());
				var output = deserializer.Deserialize(stream);
				AssertEquals("Header.EnableCodeMapping", input.Settings.EnableCodeMapping, output.Settings.EnableCodeMapping);
				AssertEquals("Header.OwnerCode", input.Settings.OwnerCode, output.Settings.OwnerCode);
				AssertEquals("Header.MessageNumberCollection.MessageNumberType.TrackingID", input.Settings.MessageNumberCollection.Find(x => x.Type == "TrackingID").Value, output.Settings.MessageNumberCollection.Find(x => x.Type == "TrackingID").Value);
				AssertEquals("Header.MessageNumberCollection.MessageNumberType.InterchangeNumber", input.Settings.MessageNumberCollection.Find(x => x.Type == "InterchangeNumber").Value, output.Settings.MessageNumberCollection.Find(x => x.Type == "InterchangeNumber").Value);
				AssertEquals("Header.MessageNumberCollection.MessageNumberType.MessageNumber", input.Settings.MessageNumberCollection.Find(x => x.Type == "MessageNumber").Value, output.Settings.MessageNumberCollection.Find(x => x.Type == "MessageNumber").Value);
				AssertEquals("Body item count", 2, output.EntitySets.Count());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			serializer = new RequestSerializer();
			deserializer = new RequestDeserializer_Versioned_Native();
		}
		RequestSerializer serializer;
		RequestDeserializer deserializer;
	}
}
