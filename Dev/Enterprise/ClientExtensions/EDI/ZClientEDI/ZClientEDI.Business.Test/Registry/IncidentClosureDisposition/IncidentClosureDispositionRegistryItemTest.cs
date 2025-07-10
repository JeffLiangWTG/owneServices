using System.IO;
using System.Text;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(IncidentClosureDispositionRegistryItem))]
	public class IncidentClosureDispositionRegistryItemTest : StronglyTypedRegistryItemTestCase<IncidentClosureDispositionCollection, IncidentClosureDispositionCollection>
	{
		protected override StronglyTypedRegistryItem<IncidentClosureDispositionCollection, IncidentClosureDispositionCollection> GetNewRegistryItem()
		{
			var defaultValue = new IncidentClosureDispositionCollection(true, 3, 4);

			return new IncidentClosureDispositionRegistryItem(
				"IncidentClosureDispositionRegistryItemTest",
				(NoResString)"Category",
				(NoResString)"Caption",
				(NoResString)"Hint",
				RegistryStorageFlags.System,
				null,
				defaultValue);
		}
	}

	[TestedType(typeof(IncidentClosureDispositionRegistryDataType))]
	public class IncidentClosureDispositionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IncidentClosureDispositionRegistryDataType>
	{
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override IncidentClosureDispositionRegistryDataType GetNewDataType()
		{
			return new IncidentClosureDispositionRegistryDataType(new IncidentClosureDispositionCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new IncidentClosureDispositionCollection(true, 3, 4);
			var node1 = list1.Add("AAA", (NoResString)"AAA Desc", null, false);

			var list2 = new IncidentClosureDispositionCollection(true, 3, 4);
			var node2 = list1.Add("BBB", (NoResString)"BBB Desc", null, false);

			var serializer = ZXmlSerializer.New(list1.GetType());

			StringBuilder xml1 = new StringBuilder();
			using (StringWriter stream = new StringWriter(xml1))
			{
				serializer.Serialize(stream, list1);
			}

			StringBuilder xml2 = new StringBuilder();
			using (StringWriter stream = new StringWriter(xml2))
			{
				serializer.Serialize(stream, list2);
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, Encoding.Unicode.GetBytes(xml1.ToString())),
				new ValidSampleAndBinaryValueInDB(list2, Encoding.Unicode.GetBytes(xml2.ToString())),
			};
		}
	}
}
