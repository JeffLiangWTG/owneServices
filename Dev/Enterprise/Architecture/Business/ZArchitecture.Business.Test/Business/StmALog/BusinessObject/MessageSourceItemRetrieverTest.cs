using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class MessageSourceItemRetrieverTest : TestCaseWithFactory
	{
		public void TestGetSourceItemsFromIXmlEventValueObject()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => MessageSourceItemRetriever.GetSourceItems(null, (IXmlEventValueObject)null));
			AssertContainsExactElementsInAnyOrder(Array.Empty<KeyDataPair>(), MessageSourceItemRetriever.GetSourceItems(Factory, (IXmlEventValueObject)null));

			var eventValueObject = new Mock<IXmlEventValueObject> { CallBase = true };

			AssertContainsExactElementsInAnyOrder(Array.Empty<KeyDataPair>(), MessageSourceItemRetriever.GetSourceItems(Factory, eventValueObject.Object));

			var contextValueList = new Mock<IXmlEventValueObjectContextValueList> { CallBase = true };
			contextValueList.Setup(list => list.Values).Returns(new List<KeyValuePair<TypeWithDescription, IZType>>
			{
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("type1", "type1 desc"), new ZString("zumba")),
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("type2", "type2 desc"), new ZInt(1)),
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("type 3", ""), new ZInt(1)),
			});

			eventValueObject.Setup(obj => obj.Context).Returns(contextValueList.Object);

			AssertMultilineASCIIEquals("GetSourceItems",
@"type1 desc|zumba
type2 desc|1
type 3|1",
			ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, eventValueObject.Object)));

			var dataContextValueObject = new Mock<IDataContextDataObject> { CallBase = true };
#if NETFRAMEWORK
			var dataSourceInfo = new List<KeyValuePair>()
			{
				new KeyValuePair("Data Source Something", "WALLAH"),
				new KeyValuePair("Data Source OTHER", "FALLOT"),
			};
#else
			var dataSourceInfo = new List<UniversalDataBuss.Integration.KeyValuePair>()
			{
				new UniversalDataBuss.Integration.KeyValuePair("Data Source Something", "WALLAH"),
				new UniversalDataBuss.Integration.KeyValuePair("Data Source OTHER", "FALLOT"),
			};
#endif
			dataContextValueObject.Setup(o => o.ContextKeyValuePairs).Returns(dataSourceInfo);
			eventValueObject.Setup(m => m.DataContext).Returns(dataContextValueObject.Object);

			AssertMultilineASCIIEquals("GetSourceItems",
@"type1 desc|zumba
type2 desc|1
type 3|1
Data Source Something|WALLAH
Data Source OTHER|FALLOT",
			ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, eventValueObject.Object)));
		}

		public void TestGetSourceItemsFromMessageValueObject()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => MessageSourceItemRetriever.GetSourceItems(null, (MessageValueObject)null));
			AssertContainsExactElementsInAnyOrder(Array.Empty<KeyDataPair>(), MessageSourceItemRetriever.GetSourceItems(Factory, (MessageValueObject)null));

			var interchange = new Mock<IEDIInterchange>();
			interchange.Setup(m => m.EI_From).Returns("geez");
			interchange.Setup(m => m.EI_To).Returns("cargowise");

			var message = new Mock<IEDIMessage>();
			message.SetupAllProperties();
			var textReader = new Mock<TextReader>();
			message.Setup(m => m.Interchange).Returns(interchange.Object);
			message.Setup(m => m.GetEM_MessageTextReader()).Returns(textReader.Object);

			AssertGetSourceItemsFromMessageValueObject(message.Object);
			AssertGetSourceItemsFromMessageValueObject_UniversalEvent(message.Object);
		}

		void AssertGetSourceItemsFromMessageValueObject(IEDIMessage message)
		{
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Events;

			var dataContextDeserializer = new Mock<IXmlDataContextDeserializer>();
			var dataContextValueObject = new Mock<IDataContextDataObject>();
			dataContextDeserializer.Setup(s => s.Parse(It.IsAny<TextReader>(), It.IsAny<IXmlImportLogger>())).Returns(dataContextValueObject.Object);

			using (ObjectFactory.Substitute(nameof(IXmlDataContextDeserializer), dataContextDeserializer.Object))
			{
				AssertMultilineASCIIEquals("GetSourceItems",
@"Sender ID|geez
Recipient ID|cargowise",
				ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, new MessageValueObject(message))));
			}

#if NETFRAMEWORK
			var dataSourceInfo = new List<KeyValuePair>()
			{
				new KeyValuePair("Data Source Something", "WALLAH"),
				new KeyValuePair("Data Source OTHER", "FALLOT"),
			};
#else
			var dataSourceInfo = new List<UniversalDataBuss.Integration.KeyValuePair>()
			{
				new UniversalDataBuss.Integration.KeyValuePair("Data Source Something", "WALLAH"),
				new UniversalDataBuss.Integration.KeyValuePair("Data Source OTHER", "FALLOT"),
			};
#endif
			dataContextValueObject.Setup(o => o.ContextKeyValuePairs).Returns(dataSourceInfo);

			using (ObjectFactory.Substitute(nameof(IXmlDataContextDeserializer), dataContextDeserializer.Object))
			{
				AssertMultilineASCIIEquals("GetSourceItems",
@"Data Source Something|WALLAH
Data Source OTHER|FALLOT
Sender ID|geez
Recipient ID|cargowise",
				ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, new MessageValueObject(message))));
			}
		}

		void AssertGetSourceItemsFromMessageValueObject_UniversalEvent(IEDIMessage message)
		{
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var eventDeserializer = new Mock<IXmlEventDeserializer>();
			var eventValueObject = new Mock<IXmlEventValueObject>();
			eventDeserializer.Setup(m => m.Parse(It.IsAny<TextReader>(), It.IsAny<IXmlImportLogger>()))
				.Returns(eventValueObject.Object);

			using (ObjectFactory.Substitute(nameof(IXmlEventDeserializer), eventDeserializer.Object))
			{
				AssertMultilineASCIIEquals("GetSourceItems_UniversalEvent",
@"Sender ID|geez
Recipient ID|cargowise",
				ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, new MessageValueObject(message))));
			}

			var contextValueList = new Mock<IXmlEventValueObjectContextValueList>();
			contextValueList.Setup(m => m.Values).Returns(new List<KeyValuePair<TypeWithDescription, IZType>>
			{
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("type1", "type1 desc"), new ZString("zumba")),
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("type2", "type2 desc"), new ZInt(1)),
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("type 3", ""), new ZInt(1)),
			});

			eventValueObject.Setup(m => m.Context).Returns(contextValueList.Object);

			using (ObjectFactory.Substitute(nameof(IXmlEventDeserializer), eventDeserializer.Object))
			{
				AssertMultilineASCIIEquals("GetSourceItems_UniversalEvent",
@"type1 desc|zumba
type2 desc|1
type 3|1
Sender ID|geez
Recipient ID|cargowise",
				ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, new MessageValueObject(message))));
			}

			var dataContextValueObject = new Mock<IDataContextDataObject>();
#if NETFRAMEWORK
			var dataSourceInfo = new List<KeyValuePair>()
			{
				new KeyValuePair("Data Source Something", "WALLAH"),
				new KeyValuePair("Data Source OTHER", "FALLOT"),
			};
#else
			var dataSourceInfo = new List<UniversalDataBuss.Integration.KeyValuePair>()
			{
				new UniversalDataBuss.Integration.KeyValuePair("Data Source Something", "WALLAH"),
				new UniversalDataBuss.Integration.KeyValuePair("Data Source OTHER", "FALLOT"),
			};
#endif
			dataContextValueObject.Setup(m => m.ContextKeyValuePairs).Returns(dataSourceInfo);

			eventValueObject.Setup(m => m.DataContext).Returns(dataContextValueObject.Object);

			using (ObjectFactory.Substitute(nameof(IXmlEventDeserializer), eventDeserializer.Object))
			{
				AssertMultilineASCIIEquals("GetSourceItems_UniversalEvent",
@"type1 desc|zumba
type2 desc|1
type 3|1
Data Source Something|WALLAH
Data Source OTHER|FALLOT
Sender ID|geez
Recipient ID|cargowise",
				ConvertToString(MessageSourceItemRetriever.GetSourceItems(Factory, new MessageValueObject(message))));
			}
		}

		public void TestAddPairIfNotEmpty()
		{
			ZString stringForTest = new ZString("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum");
			ZString nonPrintable = new ZString("\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a");
			KeyDataPair testKeyDataPair = new KeyDataPair();
			int maxKey = testKeyDataPair.KeyInfo.MaxLength;
			List<ZString> errors = new List<ZString>();
			var tests = new List<KeyPairForTest>() {
		new KeyPairForTest(new ZString("Test null key"), null, new ZString("value 1"), new ZString("|value 1")),
		new KeyPairForTest(new ZString("Test null value"), new ZString("key 1"), null, new ZString("")),
		new KeyPairForTest(new ZString("Test null key and value"), null, null, new ZString("")),

				new KeyPairForTest(new ZString("Test ZString.Empty key"), ZString.Empty, new ZString("value 1"), new ZString("|value 1")),
				new KeyPairForTest(new ZString("Test ZString.Empty value"), new ZString("key 1"), ZString.Empty, new ZString("")),
				new KeyPairForTest(new ZString("Test ZString.Empty key and value"), ZString.Empty, ZString.Empty, new ZString("")),

				new KeyPairForTest(new ZString("Test empty key"), new ZString(""), new ZString("value 1"), new ZString("|value 1")),
				new KeyPairForTest(new ZString("Test empty value"), new ZString("key 1"), new ZString(""), new ZString("")),
				new KeyPairForTest(new ZString("Test empty key and value"), new ZString(""), new ZString(""), new ZString("")),

				new KeyPairForTest(new ZString("Test key with length < N"), stringForTest.Substring(0, maxKey / 2) , new ZString("value 1"), ZString.Format("{0}|value 1", stringForTest.Substring(0, maxKey / 2))),
		new KeyPairForTest(new ZString("Test key with length = N"), stringForTest.Substring(0,maxKey), new ZString("value 2"), ZString.Format("{0}|value 2", stringForTest.Substring(0,maxKey))),
		new KeyPairForTest(new ZString("Test key with length = N+1"), stringForTest.Substring(0,maxKey + 1), new ZString("value 3"), ZString.Format("{0}|value 3", stringForTest.Substring(0,maxKey))),
		new KeyPairForTest(new ZString("Test key with length > N"), stringForTest, new ZString("value 4"), ZString.Format("{0}|value 4", stringForTest.Substring(0,maxKey))),
		new KeyPairForTest(new ZString("Test key with length > N AND non-printable"), ZString.Format("{0}{1}", stringForTest.Substring(0, maxKey / 2), nonPrintable), new ZString("value 5"),  ZString.Format("{0}{1}|value 5", stringForTest.Substring(0, maxKey / 2), nonPrintable.Substring(0,maxKey - maxKey / 2))),
	  };

			foreach (var test in tests)
			{
				test.Results = new KeyDataPairCollection(Factory);
				try
				{
					MessageSourceItemRetriever.AddPairIfNotEmpty(test.Results, test.Key, test.Value);
				}
				catch (Exception ex) { errors.Add(ex.Message); }
			}

			CombineAssertions(() =>
			{
				foreach (KeyPairForTest test in tests)
				{
					AssertMultilineASCIIEquals(test.TestDescription, test.ExpectedResult, ConvertToString(test.Results));
				}
				AssertMultilineASCIIEquals("Tests MUST NOT return any errors", "", string.Join("\n", errors.ToArray()));
			});
		}

		#region TestGetSourceItems_UniversalEvent_DataExceedsMaximumLength

		public void TestGetSourceItems_UniversalEvent_DataExceedsMaximumLength()
		{
			var interchangeMoq = new Mock<IEDIInterchange>();
			interchangeMoq.Object.EI_From = "geez";
			interchangeMoq.Object.EI_To = "cargowise";

			var messageMoq = new Mock<IEDIMessage> { CallBase = true };
			messageMoq.Setup(m => m.Interchange).Returns(interchangeMoq.Object);
			var textReader = new Mock<TextReader>() { CallBase = true };
			messageMoq.Setup(m => m.GetEM_MessageTextReader()).Returns(textReader.Object);

			messageMoq.Object.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var eventDeserializerMoq = new Mock<IXmlEventDeserializer>();
			var eventValueObjectMoq = new Mock<IXmlEventValueObject>();
			eventDeserializerMoq.Setup(m => m.Parse(It.IsAny<TextReader>(), It.IsAny<IXmlImportLogger>()))
				.Returns(eventValueObjectMoq.Object);

			var randomStringExceedingDataLength = "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";
			var dataContextValueObjectMoq = new Mock<IDataContextDataObject>();
#if NETFRAMEWORK
			var dataSourceInfo = new List<KeyValuePair>()
			{
				new KeyValuePair("Data Source Something", randomStringExceedingDataLength),
			};
#else
			var dataSourceInfo = new List<UniversalDataBuss.Integration.KeyValuePair>()
			{
				new UniversalDataBuss.Integration.KeyValuePair("Data Source Something", randomStringExceedingDataLength),
			};
#endif
			dataContextValueObjectMoq.Setup(m => m.ContextKeyValuePairs).Returns(dataSourceInfo);

			eventValueObjectMoq.Object.DataContext = dataContextValueObjectMoq.Object;

			using (ObjectFactory.Substitute(nameof(IXmlEventDeserializer), eventDeserializerMoq.Object))
			{
				AssertNoExceptionThrown(() => MessageSourceItemRetriever.GetSourceItems(Factory, new MessageValueObject(messageMoq.Object)));
			}
		}

		#endregion

		string ConvertToString(KeyDataPairCollection pairCollection)
		{
			return pairCollection != null ? string.Join(System.Environment.NewLine, pairCollection.Cast<KeyDataPair>().Select(o => string.Concat(o.Key, "|", o.Data))) :
				string.Empty;
		}

		class KeyPairForTest
		{
			public string TestDescription { get; set; }
			public string Key { get; set; }
			public string Value { get; set; }
			public string ExpectedResult { get; set; }
			public KeyDataPairCollection Results { get; set; }
			public KeyPairForTest(string testDescription, string key, string value, string expectedResult)
			{
				TestDescription = testDescription;
				Key = key;
				Value = value;
				ExpectedResult = expectedResult;
			}
		}
	}
}
