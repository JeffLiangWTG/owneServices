using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class AllFieldsAreSentTest : TestCaseWithFactory
	{
		public void TestAllFieldsAreExportedInNativeXML()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var dummy = Factory.New<DummyBusinessObject>();

			Factory.Save();

			var properties = new List<string>();
			foreach (var propertyInfo in typeof(AutoDummyBizo).GetProperties())
			{
				if (typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.Name.StartsWith(dummy.TablePrefix + "_") && !propertyInfo.Name.EndsWith("AddInfo"))
				{
					var property = propertyInfo.Name.Split('_');
					properties.Add(property[property.Length - 1]);
				}
			}

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			using (var dataStream = xmlSerializer.SerializeToStream(dummy))
			using (var reader = new StreamReader(dataStream))
			{
				string actualMessage = reader.ReadToEnd();
				foreach (var property in properties)
				{
					AssertContains("<" + property + ">", actualMessage);
				}
			}
		}
	}
}
