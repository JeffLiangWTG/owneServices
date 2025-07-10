using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Internal.Indexing
{
	/// <summary>
	/// Contains the INDEX to all static Registry Items.
	/// An instance of this class can be saved to and loaded from a file.
	/// A reference to an instance of this class can be cached in memory.
	/// </summary>
	class RegistryStaticIndex
	{
		public RegistryStaticIndex(IReadOnlyCollection<RegistryCategory> topLevelCategories)
		{
			TopLevelCategories = topLevelCategories;
		}

		public IReadOnlyCollection<RegistryCategory> TopLevelCategories { get; }

		public void Serialize(XmlWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			Serialize(TopLevelCategories, writer);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		static void Serialize(IReadOnlyCollection<RegistryCategory> categories, XmlWriter writer)
		{
			var setTypes = CreateSetTypesDictionary(categories);

			var xml = new XElement("Index",
					new XElement("SetTypes",
						new XAttribute("Count", setTypes.Count),
						setTypes.OrderBy(set => set.Value).Select(set => new XElement("SetType",
							new XAttribute("Index", set.Value),
							new XAttribute("Type", set.Key.AssemblyQualifiedName ?? set.Key.ToString())))),
					new XElement("Categories", categories.Select(category => GetXmlElement(category, setTypes))));

			xml.WriteTo(writer);
		}

		static Dictionary<Type, int> CreateSetTypesDictionary(IReadOnlyCollection<RegistryCategory> categories)
		{
			var setTypes = new Dictionary<Type, int>();
			var i = 0;
			foreach (var type in GetSetTypesRecursive(categories))
			{
				if (!setTypes.ContainsKey(type))
				{
					setTypes.Add(type, i++);
				}
			}

			return setTypes;
		}

		static IEnumerable<Type> GetSetTypesRecursive(IEnumerable<RegistryCategory> categories)
		{
			foreach (var category in categories)
			{
				foreach (var setType in category.StaticSetPropertyNames.Keys)
				{
					yield return setType;
				}

				foreach (var setType in GetSetTypesRecursive(category.StaticCategories.Values))
				{
					yield return setType;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		static XElement GetXmlElement(RegistryCategory category, IReadOnlyDictionary<Type, int> setTypes)
		{
			var element = new XElement("Category", new XAttribute("Name", CreateKey(category.StaticCategorySegmentRef, setTypes)));

			if (category.StaticSetPropertyNames.Count > 0)
			{
				element.Add(new XElement("Items",
					category.StaticSetPropertyNames.Select(item =>
						new XElement("ItemSet",
							new XAttribute("Type", setTypes[item.Key]),
							item.Value.Select(property => new XElement("Property", property))))));
			}

			foreach (var category1 in category.StaticCategories.Values)
			{
				element.Add(GetXmlElement(category1, setTypes));
			}

			return element;
		}

		static string CreateKey(CategorySegmentRef segment, IReadOnlyDictionary<Type, int> setTypes)
		{
			var typeIndex = setTypes[segment.SetType];
			return $"T{typeIndex}.{segment.PropertyName}{(segment.CategoryIndex == 0 ? "" : $".{segment.CategoryIndex}")}[{segment.SegmentIndex}]";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static RegistryStaticIndex Deserialize(XmlReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}

			if (!(XNode.ReadFrom(reader) is XElement rootElement) || rootElement.Name != "Index")
			{
				throw new FormatException("Expected Index element");
			}

			var setTypesNode = GetSingleElement(rootElement, "SetTypes");
			var categoriesNode = GetSingleElement(rootElement, (NoResString)"Categories");

			var setTypes = new Type[(int)GetRequiredAttribute(setTypesNode, (NoResString)"Count")];
			Parallel.Invoke(setTypesNode.Elements("SetType").Select(set => (Action)(() =>
			{
				setTypes[(int)GetRequiredAttribute(set, (NoResString)"Index")] = Type.GetType((string)GetRequiredAttribute(set, (NoResString)"Type"), true);
			})).ToArray());

			return new RegistryStaticIndex(Load(categoriesNode, setTypes).StaticCategories.Values);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		static RegistryCategory Load(XElement element, Type[] setTypes, CategorySegmentRef text = null)
		{
			var cat = RegistryCategory.NewStatic(text);

			foreach (var subCategory in element.Elements("Category"))
			{
				var name = (string)GetRequiredAttribute(subCategory, (NoResString)"Name");
				text = CreateCategorySegment(name, setTypes);
				cat.StaticCategories.Add(name, Load(subCategory, setTypes, text));
			}

			var itemsNode = element.Element("Items");
			if (itemsNode != null)
			{
				foreach (var set in itemsNode.Elements("ItemSet"))
				{
					cat.StaticSetPropertyNames[setTypes[(int)GetRequiredAttribute(set, (NoResString)"Type")]] = set.Elements("Property").Select(prop => (string)prop).ToList();
				}
			}

			return cat;
		}

		static CategorySegmentRef CreateCategorySegment(string value, Type[] setTypes)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException(nameof(value));
			}

			var start = value.LastIndexOf('[', value.Length - 3);
			if (start < 0 || value[value.Length - 1] != ']')
			{
				throw new FormatException("Bad CategorySegmentRef: " + value);
			}

			var segmentIndex = int.Parse(value.Substring(start + 1, value.Length - start - 2));
			var category = value.Substring(0, start);

			var parts = category.Split('.');
			if (parts.Length < 2 || parts.Length > 3 || parts[0][0] != 'T')
			{
				throw new FormatException("Bad CategorySegmentRef: " + value);
			}

			var type = setTypes[int.Parse(parts[0].Substring(1))];
			return new CategorySegmentRef(type, parts[1], parts.Length == 2 ? 0 : int.Parse(parts[2]), segmentIndex);
		}

		static XAttribute GetRequiredAttribute(XElement element, string attributeName)
		{
			return element.Attribute(attributeName) ?? throw new FormatException($"Required Attribute '{attributeName}' is missing on '{element.Name.LocalName}' element");
		}

		static XElement GetSingleElement(XElement parent, string elementName)
		{
			var elements = parent.Elements(elementName).ToList();
			if (elements.Count != 1)
			{
				throw new FormatException($"Expected one {elementName} element");
			}

			return elements[0];
		}
	}
}
