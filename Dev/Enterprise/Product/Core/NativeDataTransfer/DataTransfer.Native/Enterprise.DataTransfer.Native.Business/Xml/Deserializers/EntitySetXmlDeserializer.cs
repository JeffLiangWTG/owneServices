using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.EntityBuilders;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers
{
	public class EntitySetXmlDeserializer
	{
		// Dependency will be injected by Spring.NET
		#region Dependency

		public IDefinitionFinder DefinitionFinder;
		public bool FoundAtLeastOneAction { get; private set; }

		#endregion

		public EntitySetXmlDeserializer()
		{
			DefinitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
		}

		public EntitySet Deserialize(XElement element, AncillaryImportServices sessionServices)
		{
#if DEBUG
			// When a Native XML message is imported manually (through the UI) or automatically (via EDI Message)
			// If it is a known exception type, only show the exception message in the log. Otherwise including stacktrace and send error report.
			// We need some code to define UnhandledException as the unknown exception.
			// We could not use any unknown exception in the tests because if we know the unknown exception, it should be a known exception.
			if (element.Name.LocalName == "UnhandledException")
			{
				throw new NativeXMLCoreException(element.Value);
			}
#endif
			element.RemoveNameSpace();
			var entitySetName = element.Name.LocalName;
			var entitySet = new EntitySet(entitySetName)
			{
				Definition = DefinitionFinder.FindByEntitySetName(entitySetName)
			};
			var rootDefinition = entitySet.Definition.Root;

			AssertOnlyOneChildElement(element);

			var entityName = rootDefinition.EntityName;
			var childElement = element.Elements(entityName).SingleOrDefault() ?? throw new NativeXMLUserVisibleException(FormattableString.Invariant($"Missing mandatory element {entityName}"));

			entitySet.Root = ParseEntitySetElement(childElement, rootDefinition, sessionServices);

			return entitySet;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		protected void AssertOnlyOneChildElement(XElement element)
		{
			var count = element.Elements().Count();

			if (count != 1)
			{
				var exceptionMessage = string.Format(
					CultureInfo.InvariantCulture,
					"XML cannot be processed as it does not adhere to the native XML format. There should only be one element within the '{0}' section but {1} elements were found.",
					element.Name.LocalName,
					count);

				throw new NativeXMLUserVisibleException(exceptionMessage);
			}
		}

		public Entity ParseEntitySetElement(XElement element, IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			FoundAtLeastOneAction = false;
			var visited = new List<IEntityDefinition>();
			return ParseEntitySetElement(element, definition, visited, sessionServices);
		}

		/// <summary>
		/// Recursive method, traverse entity set element
		/// </summary>
		/// <param name="element"></param>
		/// <param name="self"></param>
		/// <param name="visited"></param>
		/// <param name="sessionServices"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public Entity ParseEntitySetElement(XElement element, IEntityDefinition self, List<IEntityDefinition> visited, AncillaryImportServices sessionServices)
		{
			var builder = new XmlEntityBuilder(element, self, sessionServices);
			Entity selfEntity;
			try
			{
				selfEntity = EntityBuilder.Construct(builder);
			}
			finally
			{
				FoundAtLeastOneAction |= builder.Action != EntityAction.EMPTY;
			}

			foreach (var child in self.HasOne())
			{
				if (visited.Contains(child))
				{
					continue;
				}

				var childName = child.EntityName;
				var childElements = element.Elements(childName);
				foreach (var childElement in childElements)
				{
					var childEntity = ParseEntitySetElement(childElement, child, visited, sessionServices);
					childEntity.Parent = selfEntity;
					selfEntity.ChildrenCollection.Add(childEntity);
				}
			}

			foreach (var child in self.HasMany())
			{
				if (visited.Contains(child))
				{
					continue;
				}

				var childCollectionName = child.EntityName + "Collection";
				var childElements = element.Elements(childCollectionName).Elements();
				foreach (var childElement in childElements)
				{
					var childEntity = ParseEntitySetElement(childElement, child, visited, sessionServices);
					childEntity.Parent = selfEntity;
					selfEntity.ChildrenCollection.Add(childEntity);
				}
			}

			foreach (var parent in self.Parents)
			{
				if (visited.Contains(parent))
				{
					continue;
				}

				var parentName = parent.EntityName;
				var parentElements = element.Elements(parentName);
				foreach (var parentElement in parentElements)
				{
					var belongsTo = ParseEntitySetElement(parentElement, parent, visited, sessionServices);
					selfEntity.ParentCollection.Add(belongsTo);
					belongsTo.ChildrenCollection.Add(selfEntity);
				}
			}
			ParseMissingMappedEntity(element, self, visited, selfEntity, sessionServices);
			return selfEntity;
		}

		void ParseMissingMappedEntity(XElement element, IEntityDefinition self, List<IEntityDefinition> visited, Entity selfEntity, AncillaryImportServices sessionServices)
		{
			if (element.Name.LocalName == "CusClassPartPivot" && !element.Elements("OrgHeader").Any())
			{
				var orgPartRelations = element.Elements("OrgPartRelation").Take(2).ToArray();
				if (orgPartRelations.Length == 1)
				{
					var orgHeaders = orgPartRelations[0].Elements("OrgHeader").Take(2).ToArray();
					if (orgHeaders.Length == 1)
					{
						var orgHeaderElement = orgHeaders[0];
						var parent = self.Parents.FirstOrDefault(p => p.EntityName == "OrgHeader");
						var belongsTo = ParseEntitySetElement(orgHeaderElement, parent, visited, sessionServices);
						selfEntity.ParentCollection.Add(belongsTo);
						belongsTo.ChildrenCollection.Add(selfEntity);
					}
				}
			}
		}
	}
}
