using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;
using static NUnit.Framework.XmlAssertions;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	public abstract class TopLevelDataObjectTestCase<T> : DataObjectTestCase<T> where T : ITopLevelDataObject, new()
	{
		public void TestObjectWritesMessageNumbersFineThroughXmlWriter()
		{
			foreach (var ns in new[] { UniversalXmlInfo.Namespace_2011_11, UniversalXmlInfo.Namespace_2012_11, })
			{
				using (SchemaVersionManager.SetNamespaceForTesting(ns))
				{
					var record = new T();
					record.SetMessageNumber(MessageNumberType.TrackingID, "dummy-trk-num");
					record.SetMessageNumber(MessageNumberType.InterchangeNumber, "dummy-int-num");
					record.SetMessageNumber(MessageNumberType.MessageNumber, "dummy-msg-num");
					record.SetMessageNumber(MessageNumberType.External, "dummy-ext-num");

					using (var stream = (SubStreamableStream)new MemoryStream())
					{
						var xmlWriter = ObjectFactory.Get<IXmlWriter>();
						xmlWriter.WriteXML(record, stream);

						using (var reader = new StreamReader(stream))
						{
							var content = reader.ReadToEnd();
							var result = XElement.Parse(content);
							var messageNumberCollection = result.Elements().FirstOrDefault()?.Element(XName.Get(nameof(TopLevelDataObject.MessageNumberCollection), ns));

							AssertNotNull(messageNumberCollection);
							AssertEquals(4, messageNumberCollection.Elements().Count());

							AssertNotNull(messageNumberCollection.Elements().FirstOrDefault(e => e.Name.LocalName == nameof(MessageNumber) && e.Attribute(nameof(MessageNumber.Type)).Value == nameof(MessageNumberType.TrackingID) && e.Value == "dummy-trk-num"));
							AssertNotNull(messageNumberCollection.Elements().FirstOrDefault(e => e.Name.LocalName == nameof(MessageNumber) && e.Attribute(nameof(MessageNumber.Type)).Value == nameof(MessageNumberType.InterchangeNumber) && e.Value == "dummy-int-num"));
							AssertNotNull(messageNumberCollection.Elements().FirstOrDefault(e => e.Name.LocalName == nameof(MessageNumber) && e.Attribute(nameof(MessageNumber.Type)).Value == nameof(MessageNumberType.MessageNumber) && e.Value == "dummy-msg-num"));
							AssertNotNull(messageNumberCollection.Elements().FirstOrDefault(e => e.Name.LocalName == nameof(MessageNumber) && e.Attribute(nameof(MessageNumber.Type)).Value == nameof(MessageNumberType.External) && e.Value == "dummy-ext-num"));
						}
					}
				}
			}
		}

		public void TestMultipleSetMessageNumberWithMessageNumberTypeProducesMultipleInstances()
		{
			var record = new T();
			record.SetMessageNumber(MessageNumberType.MessageNumber, "1");
			record.SetMessageNumber(MessageNumberType.MessageNumber, "2");
			record.SetMessageNumber(MessageNumberType.MessageNumber, "3");

			AssertEquals(3, record.MessageNumberCollection.Count());
		}

		public void TestMultipleSetMessageNumbersWithNonMessageNumberTypeProducesSingleInstance()
		{
			var record = new T();
			record.SetMessageNumber(MessageNumberType.TrackingID, "1");
			record.SetMessageNumber(MessageNumberType.TrackingID, "2");
			record.SetMessageNumber(MessageNumberType.InterchangeNumber, "1");
			record.SetMessageNumber(MessageNumberType.InterchangeNumber, "2");
			record.SetMessageNumber(MessageNumberType.External, "1");
			record.SetMessageNumber(MessageNumberType.External, "2");

			AssertEquals(3, record.MessageNumberCollection.Count());
		}

		public void TestObjectWritesTimestampThroughXmlWriter()
		{
			var dataContext1 = (IDataContextDataObject)new Universal._2011_11.DataContext();
			var dataContext2 = (IDataContextDataObject)new Universal._2012_11.DataContext();
			var dataContextPairs = new[]
			{
				(UniversalXmlInfo.Namespace_2011_11, dataContext1),
				(UniversalXmlInfo.Namespace_2012_11, dataContext2)
			};

			foreach (var (ns, dataContext) in dataContextPairs)
			{
				using (SchemaVersionManager.SetNamespaceForTesting(ns))
				{
					var record = new T();
					record.DataContext = dataContext;
					record.DataContext.Timestamp = 1095379198;

					using (var stream = (SubStreamableStream)new MemoryStream())
					{
						var xmlWriter = ObjectFactory.Get<IXmlWriter>();
						xmlWriter.WriteXML(record, stream);

						using (var reader = new StreamReader(stream))
						{
							var content = reader.ReadToEnd();
							AssertIsXml(content).HavingExactlyOneChildNode(typeof(T).Name + "/DataContext/Timestamp", n => n.WithValue("1095379198"));
						}
					}
				}
			}
		}
	}
}
