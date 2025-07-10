using System.Collections.Generic;
using System.Text;
using System.Xml;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(ResolutionAndClosureBehaviourRegistryItem))]
	public class ResolutionAndClosureBehaviourRegistryItemTest : StronglyTypedRegistryItemTestCase<ResolutionAndClosureBehaviourCollection, ResolutionAndClosureBehaviourCollection>
	{
		public void TestMaxDepthSerialization()
		{
			for (int depth = 2; depth <= 8; ++depth)
			{
				var defaultValue = new ResolutionAndClosureBehaviourCollection(true, 3, depth);
				var dataType = new ResolutionAndClosureBehaviourRegistryDataType(defaultValue);
				AssertEquals(depth.ToString(), depth, dataType.Deserialise(dataType.Serialise(defaultValue)).MaxDepth);
			}
		}

		public void TestDescriptionSerialization()
		{
			for (int depth = 2; depth <= 8; ++depth)
			{
				var allDescriptions = new List<MultilingualString>(depth - 1);
				for (int i = 0; i < depth - 1; ++i)
				{
					allDescriptions.Add((NoResString)i.ToString());
				}

				var codeLists = new List<CodeDescriptionPairList>(depth);
				for (int i = 0; i < depth; ++i)
				{
					var codeList = new CodeDescriptionPairList();
					codeList.AddPair("Code " + i, "Desc " + i);
					codeLists.Add(codeList);
				}

				var defaultValue = new ResolutionAndClosureBehaviourCollection(true, 3, depth, allDescriptions.ToArray(), codeLists.ToArray());
				defaultValue.AddSystemChildren(null);
				defaultValue.AddSystemChildren(defaultValue.Add("TY1", (NoResString)"TY1 Desc"));
				var dataType = new ResolutionAndClosureBehaviourRegistryDataType(defaultValue);
				var deserialized = dataType.Deserialise(dataType.Serialise(defaultValue));
				for (int i = 0; i < deserialized.Count; ++i)
				{
					var node = deserialized[i];
					if (node.IsSystemAll)
					{
						AssertEquals(allDescriptions[deserialized.GetDepth(node) - 1], node.Description);
					}
					else
					{
						AssertEquals("TY1", node.Code);
						AssertEquals("TY1 Desc", node.Description);
					}
					AssertContainsExactElementsInAnyOrder(codeLists[deserialized.GetDepth(node) - 1], node.CodeList);
				}
			}
		}

		protected override StronglyTypedRegistryItem<ResolutionAndClosureBehaviourCollection, ResolutionAndClosureBehaviourCollection> GetNewRegistryItem()
		{
			var defaultValue = new ResolutionAndClosureBehaviourCollection(true, 3, 8);

			return new ResolutionAndClosureBehaviourRegistryItem(
				"ResolutionAndClosureBehaviourRegistryItemTest",
				(NoResString)"Category",
				(NoResString)"Caption",
				(NoResString)"Hint",
				RegistryStorageFlags.System,
				null,
				defaultValue);
		}
	}

	[TestedType(typeof(ResolutionAndClosureBehaviourRegistryDataType))]
	public class ResolutionAndClosureBehaviourRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ResolutionAndClosureBehaviourRegistryDataType>
	{
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ResolutionAndClosureBehaviourRegistryDataType GetNewDataType()
		{
			return new ResolutionAndClosureBehaviourRegistryDataType(new ResolutionAndClosureBehaviourCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new ResolutionAndClosureBehaviourCollection(true, 3, 5);
			var list2 = new ResolutionAndClosureBehaviourCollection(true, 3, 4);
			var node1 = list2.Add("AAA", (NoResString)"AAA Desc");
			list2.Add("BBB", (NoResString)"BBB Desc", node1);

			var serializer = ZXmlSerializer.New(list1.GetType());
			var xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = false;
			xmlWriterSettings.NewLineChars = string.Empty;

			var xml1 = new StringBuilder();
			using (var stream = XmlWriter.Create(xml1, xmlWriterSettings))
			{
				serializer.Serialize(stream, list1);
			}

			var xml2 = new StringBuilder();
			using (var stream = XmlWriter.Create(xml2, xmlWriterSettings))
			{
				serializer.Serialize(stream, list2);
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, Encoding.Unicode.GetBytes(xml1.ToString())),
				new ValidSampleAndBinaryValueInDB(list2, Encoding.Unicode.GetBytes(xml2.ToString()))
			};
		}
	}
}
