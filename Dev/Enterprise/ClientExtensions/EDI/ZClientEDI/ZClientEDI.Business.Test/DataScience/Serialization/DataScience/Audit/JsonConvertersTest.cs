using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WTG.Serialization.DataScience.Audit.ObjectModel;

namespace WTG.Serialization.DataScience.Audit.Test
{
	class JsonConvertersTest : TestCase
	{
		JsonSerializer JsonSerializer { get; } = AuditMessageJsonSerializerFactory.CreateJsonSerializer();

		public void TestSerializeAuditMessageBasic()
		{
			// Arrange
			var auditMessage = new AuditMessage(
				messageCreatedTime: new DateTimeOffset(new DateTime(2022, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, 1), new TimeSpan()),
				producerDetails: new ProducerDetails(
					producerName: "Data Science Audit Subscriber SHREK",
					producerAssemblyVersion: "x.x.x.x",
					producerHost: "ShrekServer"),
				dataSource: new DataSource(
					dataSchemaVersion: 1,
					database: "Shrek",
					server: "shrek.db.swamp.zone",
					table: "dto.Locations",
					columns: new[]
					{
						new ColumnInfo(columnName: "LO_PK", sqlType: "uniqueidentifier", isNullable: false),
						new ColumnInfo(columnName: "LO_Name", sqlType: "varchar(50)", isNullable: false),
					}),
				changes: new[]
				{
					new AuditRow(
						startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						operation: CdcOperation.Insert,
						transactionEndTimeUtc: new DateTime(2022, 1, 1, 0, 0, 0),
						columnValues: new Dictionary<string, object> {
							["LO_PK"] = "787CB29E-C3ED-4637-B7DE-A3F0DEA49AB3",
							["LO_Name"] = "Swamp",
						}),
				});

			// Act
			var auditMessageString = Serialize(auditMessage);

			// Assert
			AssertJsonEquals(
				@"{
  ""messageFormatVersion"": ""1.0.0"",
  ""messageCreatedTime"": ""2022-01-01T00:00:01.0000000+00:00"",
  ""producerDetails"": {
    ""producerName"": ""Data Science Audit Subscriber SHREK"",
    ""producerAssemblyVersion"": ""x.x.x.x"",
    ""producerHost"": ""ShrekServer""
  },
  ""dataSource"": {
    ""dataSchemaVersion"": 1,
    ""database"": ""Shrek"",
    ""server"": ""shrek.db.swamp.zone"",
    ""table"": ""dto.Locations"",
    ""columns"": [
      {
        ""columnName"": ""LO_PK"",
        ""sqlType"": ""uniqueidentifier"",
        ""isNullable"": false
      },
      {
        ""columnName"": ""LO_Name"",
        ""sqlType"": ""varchar(50)"",
        ""isNullable"": false
      }
    ]
  },
  ""changes"": [
    {
      ""startLsn"": ""0x00000000000000000001"",
      ""sequenceValue"": ""0x00000000000000000001"",
      ""operation"": ""Insert"",
      ""transactionEndTimeUtc"": ""2022-01-01T00:00:00.0000000"",
      ""columnValues"": {
        ""LO_PK"": ""787CB29E-C3ED-4637-B7DE-A3F0DEA49AB3"",
        ""LO_Name"": ""Swamp""
      }
    }
  ]
}",
				auditMessageString);
		}

		public void TestSerializeDateTime()
		{
			// Arrange
			var auditMessage = new AuditMessage(
				messageCreatedTime: new DateTimeOffset(new DateTime(2022, 6, 1, 12, 0, 2), new TimeSpan()),
				producerDetails: new ProducerDetails(
					producerName: "Data Science Audit Subscriber SHREK",
					producerAssemblyVersion: "x.x.x.x",
					producerHost: "ShrekServer"),
				dataSource: new DataSource(
					dataSchemaVersion: 1,
					database: "Shrek",
					server: "shrek.db.swamp.zone",
					table: "dto.DiaryEntries",
					columns: new[]
					{
						new ColumnInfo(columnName: "DE_PK", sqlType: "uniqueidentifier", isNullable: false),
						new ColumnInfo(columnName: "DE_Time", sqlType: "datetime2", isNullable: false),
						new ColumnInfo(columnName: "DE_Entry", sqlType: "varchar(max)", isNullable: false),
					}),
				changes: new[]
				{
					new AuditRow(
						startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						operation: CdcOperation.Insert,
						transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
						columnValues: new Dictionary<string, object> {
							["DE_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
							["DE_Time"] = new DateTime(2022, 6, 1, 12, 0, 0),
							["DE_Entry"] = "Saw Donkey"
						}),
				});

			// Act
			var auditMessageString = Serialize(auditMessage);

			// Assert
			AssertJsonEquals(
				@"{
  ""messageFormatVersion"": ""1.0.0"",
  ""messageCreatedTime"": ""2022-06-01T12:00:02.0000000+00:00"",
  ""producerDetails"": {
    ""producerName"": ""Data Science Audit Subscriber SHREK"",
    ""producerAssemblyVersion"": ""x.x.x.x"",
    ""producerHost"": ""ShrekServer""
  },
  ""dataSource"": {
    ""dataSchemaVersion"": 1,
    ""database"": ""Shrek"",
    ""server"": ""shrek.db.swamp.zone"",
    ""table"": ""dto.DiaryEntries"",
    ""columns"": [
      {
        ""columnName"": ""DE_PK"",
        ""sqlType"": ""uniqueidentifier"",
        ""isNullable"": false
      },
      {
        ""columnName"": ""DE_Time"",
        ""sqlType"": ""datetime2"",
        ""isNullable"": false
      },
      {
        ""columnName"": ""DE_Entry"",
        ""sqlType"": ""varchar(max)"",
        ""isNullable"": false
      }
    ]
  },
  ""changes"": [
    {
      ""startLsn"": ""0x00000000000000000001"",
      ""sequenceValue"": ""0x00000000000000000001"",
      ""operation"": ""Insert"",
      ""transactionEndTimeUtc"": ""2022-06-01T12:00:01.0000000"",
      ""columnValues"": {
        ""DE_PK"": ""A077A049-F896-446F-87A4-B55CE728C5E6"",
        ""DE_Time"": ""2022-06-01T12:00:00.0000000"",
        ""DE_Entry"": ""Saw Donkey""
      }
    }
  ]
}",
				auditMessageString);
		}

		public void TestSerializeNonLsnHexByteArray()
		{
			// Arrange
			byte[] donkeyByteArray = Encoding.ASCII.GetBytes("Donkey");
			var auditMessage = new AuditMessage(
				messageCreatedTime: new DateTimeOffset(new DateTime(2022, 6, 1, 12, 0, 2), new TimeSpan()),
				producerDetails: new ProducerDetails(
					producerName: "Data Science Audit Subscriber SHREK",
					producerAssemblyVersion: "x.x.x.x",
					producerHost: "ShrekServer"),
				dataSource: new DataSource(
					dataSchemaVersion: 1,
					database: "Shrek",
					server: "shrek.db.swamp.zone",
					table: "dto.DiaryEntries",
					columns: new[]
					{
						new ColumnInfo(columnName: "DE_PK", sqlType: "uniqueidentifier", isNullable: false),
						new ColumnInfo(columnName: "DE_Time", sqlType: "datetime2", isNullable: false),
						new ColumnInfo(columnName: "DE_Entry", sqlType: "varchar(max)", isNullable: false),
						new ColumnInfo(columnName: "DE_Attachment", sqlType: "varbinary(max)", isNullable: true),
					}),
				changes: new[]
				{
					new AuditRow(
						startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						operation: CdcOperation.Insert,
						transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
						columnValues: new Dictionary<string, object> {
							["DE_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
							["DE_Time"] = new DateTime(2022, 6, 1, 12, 0, 0),
							["DE_Entry"] = "Saw Donkey",
							["DE_Attachment"] = donkeyByteArray
						}),
				});
			string donkeyBase64 = Convert.ToBase64String(donkeyByteArray);

			// Act
			var auditMessageString = Serialize(auditMessage);

			// Assert
			AssertJsonEquals(
				$@"{{
  ""messageFormatVersion"": ""1.0.0"",
  ""messageCreatedTime"": ""2022-06-01T12:00:02.0000000+00:00"",
  ""producerDetails"": {{
    ""producerName"": ""Data Science Audit Subscriber SHREK"",
    ""producerAssemblyVersion"": ""x.x.x.x"",
    ""producerHost"": ""ShrekServer""
  }},
  ""dataSource"": {{
    ""dataSchemaVersion"": 1,
    ""database"": ""Shrek"",
    ""server"": ""shrek.db.swamp.zone"",
    ""table"": ""dto.DiaryEntries"",
    ""columns"": [
      {{
        ""columnName"": ""DE_PK"",
        ""sqlType"": ""uniqueidentifier"",
        ""isNullable"": false
      }},
      {{
        ""columnName"": ""DE_Time"",
        ""sqlType"": ""datetime2"",
        ""isNullable"": false
      }},
      {{
        ""columnName"": ""DE_Entry"",
        ""sqlType"": ""varchar(max)"",
        ""isNullable"": false
      }},
      {{
        ""columnName"": ""DE_Attachment"",
        ""sqlType"": ""varbinary(max)"",
        ""isNullable"": true
      }}
    ]
  }},
  ""changes"": [
    {{
      ""startLsn"": ""0x00000000000000000001"",
      ""sequenceValue"": ""0x00000000000000000001"",
      ""operation"": ""Insert"",
      ""transactionEndTimeUtc"": ""2022-06-01T12:00:01.0000000"",
      ""columnValues"": {{
        ""DE_PK"": ""A077A049-F896-446F-87A4-B55CE728C5E6"",
        ""DE_Time"": ""2022-06-01T12:00:00.0000000"",
        ""DE_Entry"": ""Saw Donkey"",
        ""DE_Attachment"": ""{donkeyBase64}""
      }}
    }}
  ]
}}",
				auditMessageString);
		}

		public void TestSerialisePartialAuditMessages()
		{
			var messageId = Guid.Parse("ebf7631f-a404-4eac-ae9c-e15402b42717");
			var partialAuditMessages = new []
			{
				new PartialAuditMessage(messageId, 42, 69, "{\"key\": \"hello, "),
				new PartialAuditMessage(messageId, 43, 69, "world!\"},"),
			};

			AssertJsonEquals(
				@"
				{
					""multipartTrackingInfo"": {
						""messageId"": ""ebf7631f-a404-4eac-ae9c-e15402b42717"",
						""partIndex"": 42,
						""partsCount"": 69
					},
					""payloadString"": ""{\""key\"": \""hello, ""
				}
				",
				Serialize(partialAuditMessages[0]));

			AssertJsonEquals(
				@"
				{
					""multipartTrackingInfo"": {
						""messageId"": ""ebf7631f-a404-4eac-ae9c-e15402b42717"",
						""partIndex"": 43,
						""partsCount"": 69
					},
					""payloadString"": ""world!\""},""
				}
				",
				Serialize(partialAuditMessages[1]));
		}

		public void TestSerialisePartialAuditMessagesFromAuditMessage()
		{
			// Arrange
			var auditMessage = new AuditMessage(
				messageCreatedTime: new DateTimeOffset(new DateTime(2022, 6, 1, 12, 0, 2), new TimeSpan()),
				producerDetails: new ProducerDetails(
					producerName: "Data Science Audit Subscriber SHREK",
					producerAssemblyVersion: "x.x.x.x",
					producerHost: "ShrekServer"),
				dataSource: new DataSource(
					dataSchemaVersion: 1,
					database: "Shrek",
					server: "shrek.db.swamp.zone",
					table: "dto.DiaryEntries",
					columns: new[]
					{
						new ColumnInfo(columnName: "DE_PK", sqlType: "uniqueidentifier", isNullable: false),
						new ColumnInfo(columnName: "DE_Time", sqlType: "datetime2", isNullable: false),
						new ColumnInfo(columnName: "DE_Entry", sqlType: "varchar(max)", isNullable: false),
						new ColumnInfo(columnName: "DE_Attachment", sqlType: "varbinary(max)", isNullable: true),
					}),
				changes: new[]
				{
					new AuditRow(
						startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
						operation: CdcOperation.Insert,
						transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
						columnValues: new Dictionary<string, object> {
							["DE_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
							["DE_Time"] = new DateTime(2022, 6, 1, 12, 0, 0),
							["DE_Entry"] = "Saw Donkey",
							["DE_Attachment"] = Encoding.ASCII.GetBytes("Donkey")
						}),
				});

			var messageId = Guid.Parse("ac0fd33a-6fd3-418f-b06e-414f29423dc0");
			var auditMessageParts = PartialAuditMessage.SplitFrom(auditMessage, maxMessageSize: 1000, kafkaMessageOverhead: 50, messageSerialiser: JsonSerializer, messageId: messageId).Select(Serialize).ToArray();

			// Act
			var auditMessageString = new StringBuilder();
			for (int partIndex = 0, partsCount = auditMessageParts.Length; partIndex < partsCount; ++partIndex)
			{
				var partialMessageJO = JsonConvert.DeserializeObject<JObject>(auditMessageParts[partIndex]);
				var trackingInfoJO = partialMessageJO.Value<JObject>("multipartTrackingInfo");
				AssertEquals(partIndex, trackingInfoJO.Value<int>("partIndex"));
				AssertEquals(partsCount, trackingInfoJO.Value<int>("partsCount"));
				AssertEquals("ac0fd33a-6fd3-418f-b06e-414f29423dc0", trackingInfoJO.Value<string>("messageId"));

				var payloadString = partialMessageJO.Value<string>("payloadString");
				AssertNotNullOrEmpty(payloadString);
				auditMessageString.Append(payloadString);
			}

			// Assert
			AssertGreaterThan(auditMessageParts.Length, 1);
			AssertJsonEquals(
				@"{
  ""messageFormatVersion"": ""1.0.0"",
  ""messageCreatedTime"": ""2022-06-01T12:00:02.0000000+00:00"",
  ""producerDetails"": {
    ""producerName"": ""Data Science Audit Subscriber SHREK"",
    ""producerAssemblyVersion"": ""x.x.x.x"",
    ""producerHost"": ""ShrekServer""
  },
  ""dataSource"": {
    ""dataSchemaVersion"": 1,
    ""database"": ""Shrek"",
    ""server"": ""shrek.db.swamp.zone"",
    ""table"": ""dto.DiaryEntries"",
    ""columns"": [
      {
        ""columnName"": ""DE_PK"",
        ""sqlType"": ""uniqueidentifier"",
        ""isNullable"": false
      },
      {
        ""columnName"": ""DE_Time"",
        ""sqlType"": ""datetime2"",
        ""isNullable"": false
      },
      {
        ""columnName"": ""DE_Entry"",
        ""sqlType"": ""varchar(max)"",
        ""isNullable"": false
      },
      {
        ""columnName"": ""DE_Attachment"",
        ""sqlType"": ""varbinary(max)"",
        ""isNullable"": true
      }
    ]
  },
  ""changes"": [
    {
      ""startLsn"": ""0x00000000000000000001"",
      ""sequenceValue"": ""0x00000000000000000001"",
      ""operation"": ""Insert"",
      ""transactionEndTimeUtc"": ""2022-06-01T12:00:01.0000000"",
      ""columnValues"": {
        ""DE_PK"": ""A077A049-F896-446F-87A4-B55CE728C5E6"",
        ""DE_Time"": ""2022-06-01T12:00:00.0000000"",
        ""DE_Entry"": ""Saw Donkey"",
        ""DE_Attachment"": ""RG9ua2V5""
      }
    }
  ]
}",
				auditMessageString.ToString());
		}

		public void TestDeserializeNoLowLevelDateParsing()
		{
			// Arrange
			var date = new DateTime(2022, 1, 1);
			var str = Serialize(date);
			var expectedStr = @"""2022-01-01T00:00:00.0000000""";

			AssertEquals(expectedStr, str);
			AssertEquals(date, Deserialize<DateTime>(str));

			// Act
			str = Serialize((object)date);

			// Assert
			AssertEquals(expectedStr, str);
			AssertNotEquals(date, Deserialize<object>(str));
		}

		string Serialize<T>(T obj)
		{
			using (var stringWriter = new StringWriter())
			using (var jsonWriter = new JsonTextWriter(stringWriter))
			{
				JsonSerializer.Serialize(jsonWriter, obj);
				return stringWriter.ToString();
			}
		}

		T Deserialize<T>(string str)
		{
			using (var stringReader = new StringReader(str))
			using (var jsonTextReader = new JsonTextReader(stringReader))
			{
				return JsonSerializer.Deserialize<T>(jsonTextReader);
			}
		}

		void AssertJsonEquals(string expectedJson, string actualJson)
		{
			AssertEquals(TranscribeJson(expectedJson), actualJson);
		}

		static string TranscribeJson(string jsonStr)
		{
			var buffer = new StringBuilder(jsonStr.Length);
			{
				using var textWriter = new StringWriter(buffer);
				using var jsonWriter = new JsonTextWriter(textWriter);

				using var textReader = new StringReader(jsonStr);
				using var jsonReader = new JsonTextReader(textReader);
				jsonReader.DateParseHandling = DateParseHandling.None;
				jsonReader.FloatParseHandling = FloatParseHandling.Double;

				while (jsonReader.Read())
				{
					switch (jsonReader.TokenType)
					{
						case JsonToken.StartObject:
							jsonWriter.WriteStartObject();
							break;

						case JsonToken.StartArray:
							jsonWriter.WriteStartArray();
							break;

						case JsonToken.EndObject:
							jsonWriter.WriteEndObject();
							break;

						case JsonToken.EndArray:
							jsonWriter.WriteEndArray();
							break;

						case JsonToken.PropertyName:
							jsonWriter.WritePropertyName((string)jsonReader.Value);
							break;

						case JsonToken.Integer:
						case JsonToken.Float:
						case JsonToken.String:
						case JsonToken.Boolean:
						case JsonToken.Null:
						case JsonToken.Undefined:
							jsonWriter.WriteValue(jsonReader.Value);
							break;

						default:
							Fail($"Unexpected JSON token type {jsonReader.TokenType}");
							break;
					}
				}
			}

			return buffer.ToString();
		}
	}
}
