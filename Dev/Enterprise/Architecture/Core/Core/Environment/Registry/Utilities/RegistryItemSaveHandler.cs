using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.OverrideLevels;

namespace Enterprise.ZArchitecture.Environment.Registry
{
	public interface IRegistryItemSaveHandler
	{
		LoadedRegistryItems LoadItems(IEnumerable<IRegistryItem> allRegistryItems, Stream stream);
		void SaveItems(IEnumerable<IRegistryItem> itemsToSaveToStream, IOverrideLevel levelToSaveFor, Stream stream);
	}

	public class RegistryItemSaveHandler : IRegistryItemSaveHandler
	{
		#region Loading

		public LoadedRegistryItems LoadItems(IEnumerable<IRegistryItem> allRegistryItems, Stream stream)
		{
			var allItems = allRegistryItems.ToDictionary(item => item.Name);

			var root = XElement.Load(stream);
			var items = root.Element(ItemCollectionName)
				?? throw new XmlException("The file was not correctly formatted for this parser");

			var duplicateItems = from node in items.Elements(ItemNodeName)
								 group node by GetValue(node, ItemNameNodeName) into g
								 where g.Count() > 1
								 select g.Key;
			if (duplicateItems.Any())
			{
				throw new RegistryDuplicateException(string.Join(",", duplicateItems));
			}

			var loadedValues = items.Elements(ItemNodeName)
				.Select(node => LoadItem(allItems, node));

			return new LoadedRegistryItems(loadedValues);
		}

		static LoadedRegistryItemValue LoadItem(IDictionary<string, IRegistryItem> items, XElement itemNode)
		{
			var name = GetValue(itemNode, ItemNameNodeName);
			var caption = GetValue(itemNode, ItemCaptionNodeName);
			var base64Data = GetValue(itemNode, ItemDataNodeName);

			IRegistryItem relatedRegistryItem = null;
			object deserializedValue = null;

			var wasSuccessfullyLoaded = (name != null && caption != null && items.TryGetValue(name, out relatedRegistryItem) && TryDeserialiseValue(relatedRegistryItem.DataType, base64Data, out deserializedValue));

			return new LoadedRegistryItemValue(name, caption, relatedRegistryItem, deserializedValue, wasSuccessfullyLoaded);
		}

		static bool TryDeserialiseValue(IRegistryDataType dataType, string base64Data, out object value)
		{
			if (dataType == null || base64Data == null)
			{
				value = null;
				return false;
			}

			try
			{
				value = dataType.Deserialise(Convert.FromBase64String(base64Data));
				return true;
			}
			catch (FormatException)
			{
				value = null;
				return false;
			}
		}

		static string GetValue(XElement root, string name)
		{
			var node = root.Element(name);
			return node == null ? null : node.Value;
		}
		#endregion

		#region Saving

		public void SaveItems(IEnumerable<IRegistryItem> itemsToSaveToStream, IOverrideLevel levelToSaveFor, Stream stream)
		{
			using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
			{
				writer.WriteStartDocument();

				writer.WriteStartElement(RootNodeName);
				writer.WriteStartElement(ItemCollectionName);

				foreach (var item in itemsToSaveToStream)
				{
					writer.WriteStartElement(ItemNodeName);

					writer.WriteElementString(ItemNameNodeName, item.Name);
					writer.WriteElementString(ItemCaptionNodeName, item.Caption);
					writer.WriteElementString(ItemDataNodeName, AsBase64String(item, levelToSaveFor));

					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndElement();

				writer.WriteEndDocument();
			}
		}

		static string AsBase64String(IRegistryItem item, IOverrideLevel levelToSaveFor)
		{
			var dataType = item.DataType;
			var objectValue = levelToSaveFor.GetValueOf(item);
			var binaryValue = dataType.Serialise(objectValue);

			return Convert.ToBase64String(binaryValue);
		}

		#endregion

		#region Xml Tag Names
		#region SuppressResourceStringsCheckRegion

		internal const string RootNodeName = "RegistryItemOverrides";

		internal const string ItemCollectionName = "Items";
		internal const string ItemNodeName = "Item";

		internal const string ItemNameNodeName = "Name";
		internal const string ItemCaptionNodeName = "Caption";
		internal const string ItemDataNodeName = "Data";

		#endregion
		#endregion
	}
}
