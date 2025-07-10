using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	public class EntitySetDefinitionBuilder
	{
		readonly XElement definitionData;

		public EntitySetDefinitionBuilder(XElement definitionData)
		{
			this.definitionData = definitionData;
		}

		public EntitySetDefinitionBuilder(byte[] definitionData)
		{
			using (var stream = new MemoryStream(definitionData))
			using (var reader = XmlReader.Create(stream))
			{
				this.definitionData = XElement.Load(reader);
			}
		}

		public EntitySetDefinition GetEntitySetDefinition()
		{
			try
			{
				var name = definitionData.Attribute(TagName.Name).Value;
				var compactAttribute = definitionData.Attribute(TagName.IsCompact);
				bool isCompact = false;
				string noImportReason = null;
				bool useBatching = false;

				if (compactAttribute != null)
				{
					bool.TryParse(compactAttribute.Value, out isCompact);
				}

				var noImportReasonAttribute = definitionData.Attribute(TagName.NoImportReason);
				if (noImportReasonAttribute != null)
				{
					noImportReason = noImportReasonAttribute.Value;
				}

				var useBatchingAttribute = definitionData.Attribute(TagName.UseBatching);
				if (useBatchingAttribute != null)
				{
					bool.TryParse(useBatchingAttribute.Value, out useBatching);
				}

				var codeMappings = ParseDefinitionElement(definitionData);
				var codeMappingCollection = new CodeMappingCollection(codeMappings);

				var entitySetDefinition = new EntitySetDefinition(name, noImportReason, codeMappingCollection, isCompact, useBatching);
				var entityInfos = new EntityInfoLoader(definitionData, entitySetDefinition).GetEntityInfos();
				var associationInfos = new AssociationInfoLoader(definitionData).GetAssociationInfos().ToList();

				var rootName = GetRootName(definitionData);
				var rootEntityInfo = entityInfos.FindByName(rootName);
				var rootEntityDefinition = new EntityDefinition(rootEntityInfo);

				new RootAssociationLinker(entityInfos, associationInfos).LinkRootAssociations(rootEntityDefinition);
				new ExternalEntityLinker(entitySetDefinition, associationInfos).LinkExternalEntities(rootEntityDefinition);

				var entities = new EntityDefinitionCollection(rootEntityDefinition.Relatives<IEntityDefinition>().OfType<EntityDefinition>());
				entitySetDefinition.SetEntities(entities, rootEntityDefinition);

				return entitySetDefinition;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error creating entity definition", definitionData.ToString(), ex);
				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "column name")]
		public IEnumerable<CodeMapping> ParseDefinitionElement(XElement definitionElement)
		{
			var mappings = new List<CodeMapping>();
			var mappingElements = definitionElement.Element("EDICodeMappings");

			if (mappingElements != null)
			{
				foreach (var mappingElement in mappingElements.Elements("Mapping"))
				{
					var mapping = new CodeMapping
					{
						TableName = mappingElement.Attribute("Table").Value,
						PropertyName = mappingElement.Attribute("PropertyName").Value
					};

					if (mappingElement.Attribute(TagName.Relationship) != null)
					{
						mapping.Relationship = mappingElement.Attribute(TagName.Relationship).Value;
					}

					if (mappingElement.Attribute(TagName.RelationshipResolver) != null)
					{
						mapping.RelationshipResolver = mappingElement.Attribute(TagName.RelationshipResolver).Value;
					}

					mappings.Add(mapping);
				}
			}

			return mappings;
		}

		class RootAssociationLinker
		{
			internal RootAssociationLinker(IEnumerable<EntityDefinition> definitions, IEnumerable<AssociationInfo> associations)
			{
				this.definitions = definitions;
				this.associations = associations;
				this.parentsWalked = new HashSet<string>();
			}

			readonly IEnumerable<EntityDefinition> definitions;
			readonly IEnumerable<AssociationInfo> associations;

			internal void LinkRootAssociations(EntityDefinition rootEntityDefinition)
			{
				LinkParentAssociations(rootEntityDefinition, rootDefinition: true);
				LinkRelativeAssociations(rootEntityDefinition);
			}

			readonly HashSet<string> parentsWalked;

			void LinkParentAssociations(EntityDefinition definition, bool rootDefinition = false)
			{
				if (!parentsWalked.Contains(definition.EntityName) && (rootDefinition || definition.RequiresAdditionOfActionEqualsMerge))
				{
					if (!rootDefinition)
					{
						parentsWalked.Add(definition.EntityName);
					}
					var parentAssociations = definition.FindParentAssociations(associations).Where(a => !a.IsMainAssociationFor(definition) && a.ParentName != null);
					foreach (var parentAssociation in parentAssociations)
					{
						//Create Parent Definition
						var parent = definitions.FindByName(parentAssociation.ParentName);
						var parentDefinition = new EntityDefinition(parent);
						//External Entity will be handled in LinkRelativeAssociation
						if (!parentDefinition.IsExternal)
						{
							var association = AssociationDefinition.New(parentDefinition, definition, parentAssociation);
							definition.AssociationCollection.ParentAssociations.Add(association);

							parentDefinition.AssociationCollection.MainAssociation = association;
							parentDefinition.AssociationCollection.ChildAssociations.Add(association);
							LinkRelativeAssociations(parentDefinition);
						}
					}
				}
			}

			void LinkRelativeAssociations(EntityDefinition definition)
			{
				var externalBasicDefinitions = definitions.Where(d => d.IsExternal);
				foreach (var externalBasicDefinition in externalBasicDefinitions)
				{
					var externalDefinition = new EntityDefinition(externalBasicDefinition);
					var basicAssociation = associations.FindAssociation(externalDefinition, definition);
					AddExternalAssociation(definition, externalDefinition, basicAssociation);
				}

				var childAssociations = definition.FindChildAssociations(associations).Where(a => !a.IsMainAssociationFor(definition) && a.LinkChild);
				foreach (var childAssociation in childAssociations)
				{
					//Create Child Definition
					var child = definitions.FindByName(childAssociation.ChildName);
					var childDefinition = new EntityDefinition(child);

					//Create Child Association
					var association = AssociationDefinition.New(definition, childDefinition, childAssociation);
					definition.AssociationCollection.ChildAssociations.Add(association);

					// Link association to child definition
					childDefinition.Parent = definition;
					childDefinition.AssociationCollection.MainAssociation = association;
					childDefinition.AssociationCollection.ParentAssociations.Add(association);
					LinkRelativeAssociations(childDefinition);
				}

				LinkParentAssociations(definition);
			}

			void AddExternalAssociation(EntityDefinition definition, EntityDefinition externalDefinition, AssociationInfo basicAssociation)
			{
				if (basicAssociation != null)
				{
					var association = AssociationDefinition.New(externalDefinition, definition, basicAssociation);
					definition.AssociationCollection.ParentAssociations.Add(association);
					externalDefinition.AssociationCollection.MainAssociation = association;
					externalDefinition.AssociationCollection.ChildAssociations.Add(association);
					AddAdditionalExternalChildren(externalDefinition);
					AddAdditionalExternalParents(externalDefinition);
				}
			}

			void AddAdditionalAssociation(EntityDefinition childDefinition, EntityDefinition parentDefinition, AssociationInfo basicAssociation, bool isChildToParentAssociation)
			{
				if (basicAssociation != null)
				{
					var association = AssociationDefinition.New(parentDefinition, childDefinition, basicAssociation);
					childDefinition.Parent = parentDefinition;
					childDefinition.AssociationCollection.ParentAssociations.Add(association);
					parentDefinition.AssociationCollection.ChildAssociations.Add(association);

					if (isChildToParentAssociation)
					{
						childDefinition.AssociationCollection.MainAssociation = association;
					}
					else
					{
						parentDefinition.AssociationCollection.MainAssociation = association;
					}
				}
			}

			void AddAdditionalExternalChildren(EntityDefinition externalDefinition)
			{
				foreach (var association in associations.Where(x => x.IsExternalChild && x.ParentName == externalDefinition.EntityName))
				{
					var childDefinition = definitions.FirstOrDefault(x => !x.IsExternal && x.EntityName == association.ChildName);
					if (childDefinition != null)
					{
						AddAdditionalAssociation(childDefinition, externalDefinition, association, isChildToParentAssociation: true);
						AddAdditionalExternalChildren(childDefinition);
						AddAdditionalExternalParents(childDefinition);
					}
				}
			}

			void AddAdditionalExternalParents(EntityDefinition externalDefinition)
			{
				foreach (var association in associations.Where(x => x.IsExternalParent && x.ChildName == externalDefinition.EntityName))
				{
					var parentDefinition = definitions.FirstOrDefault(x => x.IsExternal && x.EntityName == association.ParentName);
					if (parentDefinition != null)
					{
						AddAdditionalAssociation(externalDefinition, parentDefinition, association, isChildToParentAssociation: false);
						AddAdditionalExternalChildren(parentDefinition);
						AddAdditionalExternalParents(parentDefinition);
					}
				}
			}
		}

		class ExternalEntityLinker
		{
			internal ExternalEntityLinker(EntitySetDefinition entitySetDefinition, IEnumerable<AssociationInfo> associations)
			{
				this.entitySetDefinition = entitySetDefinition;
				this.associations = associations;
			}
			readonly EntitySetDefinition entitySetDefinition;
			readonly IEnumerable<AssociationInfo> associations;

			internal void LinkExternalEntities(EntityDefinition rootEntityDefinition)
			{
				var internalEntities = rootEntityDefinition.Relatives<IEntityDefinition>().Where(e => !(e.IsExternal)).OfType<EntityDefinition>();
				foreach (var internalEntity in internalEntities)
				{
					CreateExternalEntitiesForEntity(internalEntity);
				}
			}

			void CreateExternalEntitiesForEntity(EntityDefinition entityDefinition)
			{
				var table = entityDefinition.Table;
				var foreignKeys = table.Columns.ForeignKeys.Where(k => !entityDefinition.PropertyIsExcluded(k.Name));
				foreach (var foreignKey in foreignKeys)
				{
					CreateExternalEntityForForeignKey(foreignKey, entityDefinition);
				}
			}

			void CreateExternalEntityForForeignKey(ForeignKey foreignKey, EntityDefinition entityDefinition)
			{
				EntityDefinition externalEntityDefinition;
				var existingExternalAssociation = entityDefinition.AssociationCollection.FirstOrDefault(association => association.ForeignKeys.Any(x => x.Equals(foreignKey)));
				if (existingExternalAssociation == null)
				{
					if (foreignKey.ReferenceTable == null)
					{
						return;
					}
					// 1. Create External Entity
					externalEntityDefinition = BuildExternalEntity(foreignKey, entityDefinition);
					// 2. Create External Association
					var externalAssociation = BuildExternalAssociation(foreignKey, externalEntityDefinition, entityDefinition);
					// 3. Link External Association for both internal Entity and external Entity
					entityDefinition.AssociationCollection.ParentAssociations.Add(externalAssociation);
					externalEntityDefinition.AssociationCollection.MainAssociation = externalAssociation;
					externalEntityDefinition.AssociationCollection.ChildAssociations.Add(externalAssociation);
				}
				else
				{
					externalEntityDefinition = (EntityDefinition)existingExternalAssociation.To;
				}

				AddParentExternalEntitiesForForeignKeysMakingUpTheCandidateKey(externalEntityDefinition);
			}

			EntityDefinition BuildExternalEntity(ForeignKey foreignKey, EntityDefinition parentEntityDefinition)
			{
				var externalEntityName = GenerateExternalEntityName(parentEntityDefinition, foreignKey);
				var refTable = foreignKey.ReferenceTable;
				return new EntityDefinition(externalEntityName, refTable.Name, string.Empty, entitySetDefinition, true);
			}

			void AddParentExternalEntitiesForForeignKeysMakingUpTheCandidateKey(EntityDefinition externalEntityDefinition)
			{
				var table = externalEntityDefinition.Table;
				var candidateKeys = table.CandidateKeyConstraints;
				foreach (var candidateKey in candidateKeys)
				{
					foreach (var column in candidateKey.Columns)
					{
						var foreignKeyColumnMakingUpCandidateKey = table.Columns.ForeignKeys.FirstOrDefault(fk => fk.Name == column.Name);

						if (foreignKeyColumnMakingUpCandidateKey != null)
						{
							CreateExternalEntityForForeignKey(foreignKeyColumnMakingUpCandidateKey, externalEntityDefinition);
						}
					}
				}
			}

			AssociationDefinition BuildExternalAssociation(ForeignKey foreignKey, EntityDefinition externalEntity, EntityDefinition internalEntity)
			{
				var association =
					associations.FirstOrDefault(a => a.ParentName == externalEntity.EntityName && a.ChildName == internalEntity.EntityName) ??
					new AssociationInfo
					{
						Cardinality = "*",
						ParentKeys = new List<AssociationKeyInfo>
						{
							new AssociationKeyInfo() { ParentKey = foreignKey.Name }
						}
					};
				return AssociationDefinition.New(externalEntity, internalEntity, association);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
			static string GenerateExternalEntityName(IEntityDefinition definition, ForeignKey foreignKey)
			{
				var externalEntityName = foreignKey.HumanName;
				if (definition.PropertyDefinitions.Find(externalEntityName) != null)
				{
					externalEntityName += "External";
				}
				return externalEntityName;
			}
		}

		static string GetRootName(XElement definitionData)
		{
			var rootAttributes = definitionData.Attributes(TagName.Root);
			if (rootAttributes.Count() != 1)
			{
				throw new NativeXMLUserVisibleException("Entity Set Definition should have " + TagName.Root);
			}

			var rootAttribute = rootAttributes.First();
			return rootAttribute.Value;
		}
	}
}
