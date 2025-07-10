using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Business.Xml.Serializers
{
	public class EntitySetXmlSerializer
	{
		public EntitySetXmlSerializer()
		{
			entityGenerator = new EntityElementGenerator();
			ns = ReferenceDataXMLForSerialize.Instance.NameSpace;
		}
		readonly EntityElementGenerator entityGenerator;
		readonly XNamespace ns;

		public XElement Serialize(IEntity rootEntity)
		{
			var entitySetElement = new XElement(rootEntity.Definition.EntitySetDefinition.Name);
			var elements = GenerateEntityElements(rootEntity);
			var entitySet = rootEntity.ToString();

			if (elements.ContainsKey(entitySet))
			{
				entitySetElement.Add(elements[entitySet]);
			}

			GenerateAssociation(rootEntity, elements);

			entitySetElement.SetDefaultNameSpace(ns);
			entitySetElement.Add(new XAttribute(NativeXsd.Tag.Version, NativeXmlInfo.Version_2011_11));
			return entitySetElement;
		}

		IDictionary<string, XElement> GenerateEntityElements(IEntity root)
		{
			var elements = new Dictionary<string, XElement>();
			root.DepthFirstTraversal(null,
				(entity, relative) =>
				{
					var element = entityGenerator.Generate(entity);
					elements[entity.ToString()] = element;
				});
			return elements;
		}

		#region Append Related Element

		static void GenerateAssociation(IEntity root, IDictionary<string, XElement> elements)
		{
			var parentCollectionsWithChildrenAlreadyAdded = new Dictionary<XElement, HashSet<XElement>>();

			foreach (var entity in root.Relatives())
			{
				var entitySet = elements[entity.ToString()];

				foreach (var child in entity.OneToOne())
				{
					AppendOneToOneElement(entitySet, elements[child.ToString()], child.Definition);
				}

				foreach (var child in entity.OneToMany())
				{
					AppendOneToManyElement(entitySet, elements[child.ToString()], child.Definition, parentCollectionsWithChildrenAlreadyAdded);
				}

				foreach (var belongsTo in entity.BelongsTo().Where(x => !entitySet.Elements().Select(e => e.Name).Contains(x.EntityName)))
				{
					AppendBelongsToElement(entitySet, elements[belongsTo.ToString()], belongsTo.Definition);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		static void AppendOneToManyElement(XContainer parentElement, XElement childElement, IEntityDefinition childDefinition, Dictionary<XElement, HashSet<XElement>> parentCollectionsWithChildrenAlreadyAdded)
		{
			var collectionName = childElement.Name + "Collection";
			var nodeList = parentElement.Elements(collectionName);

			var parentCollectionElement = nodeList.FirstOrDefault();
			if (parentCollectionElement == null)
			{
				parentCollectionElement = new XElement(collectionName);
				AppendTableNameAttribute(parentCollectionElement, childDefinition);
				parentElement.Add(parentCollectionElement);
			}

			AppendChildElement(parentCollectionElement, childElement, parentCollectionsWithChildrenAlreadyAdded);
		}

		static void AppendChildElement(XElement parentCollectionElement, XElement child, Dictionary<XElement, HashSet<XElement>> parentCollectionsWithChildrenAlreadyAdded)
		{
			if (!parentCollectionsWithChildrenAlreadyAdded.TryGetValue(parentCollectionElement, out HashSet<XElement> childrenAlreadyAdded))
			{
				childrenAlreadyAdded = new HashSet<XElement>();
				parentCollectionsWithChildrenAlreadyAdded[parentCollectionElement] = childrenAlreadyAdded;
			}

			if (!childrenAlreadyAdded.Contains(child))
			{
				parentCollectionElement.Add(child);
				childrenAlreadyAdded.Add(child);
			}
		}

		static void AppendOneToOneElement(XContainer parentElement, XElement childElement, IEntityDefinition childDefinition)
		{
			AppendTableNameAttribute(childElement, childDefinition);
			parentElement.Add(childElement);
		}

		static void AppendBelongsToElement(XContainer entityElement, XElement belongsToElement, IEntityDefinition belongsToDefinition)
		{
			AppendTableNameAttribute(belongsToElement, belongsToDefinition);
			entityElement.Add(belongsToElement);
		}

		static void AppendTableNameAttribute(XElement element, IEntityDefinition definition)
		{
			if (definition.EntityName == definition.TableName)
			{
				return;
			}

			if (element.Attributes(TagName.TableName).Any())
			{
				return;
			}

			element.Add(new XAttribute(TagName.TableName, definition.TableName));
		}

		#endregion
	}
}
