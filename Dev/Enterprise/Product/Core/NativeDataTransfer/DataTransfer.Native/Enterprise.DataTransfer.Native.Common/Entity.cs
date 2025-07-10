using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Common
{
	public class Entity : IEntity, IEquatable<Entity>
	{
		public event EventHandler InternalPKChanged = delegate { };

		public Entity(IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			this.definition = definition;
			properties = new Dictionary<string, Property>();
			children = new EntityCollection();
			parentCollection = new EntityCollection();
			this.sessionServices = sessionServices;
		}
		readonly IEntityDefinition definition;
		readonly Dictionary<string, Property> properties;
		readonly EntityCollection children;
		readonly EntityCollection parentCollection;
		readonly AncillaryImportServices sessionServices;

		public string EntityName
		{
			get { return definition.EntityName; }
		}

		public IEntityDefinition Definition { get { return definition; } }

		public EntityAction Action { get; set; }

		#region Related IEntity

		public EntityCollection ParentCollection
		{
			get { return parentCollection; }
		}

		public EntityCollection ChildrenCollection
		{
			get { return children; }
		}

		#region IGraphNode

		public IEntity Self
		{
			get { return this; }
		}

		public IEntity Parent
		{
			get { return parent; }
			set
			{
				parent = value;
				ParentCollection.Add(parent);
			}
		}
		IEntity parent;

		public IEnumerable<IEntity> Parents
		{
			get { return ParentCollection; }
		}

		public IEnumerable<IEntity> Children
		{
			get { return ChildrenCollection; }
		}

		Guid IGraphNode.ID
		{
			get { return iGraphNodeID; }
		}

		readonly Guid iGraphNodeID = Guid.NewGuid();

		#region ITreeNode Members
		/// <summary>
		/// Deprecated Methods!! Should not be used.
		/// Handle Variance for Generic Type
		/// </summary>
		IGraphNode IGraphNode.Self
		{
			get { return this.Self; }
		}

		IGraphNode IGraphNode.Parent
		{
			get { return this.Parent; }
		}

		IEnumerable<IGraphNode> IGraphNode.Parents
		{
			get { return Parents.OfType<IGraphNode>(); }
		}

		IEnumerable<IGraphNode> IGraphNode.Children
		{
			get { return Children.OfType<IGraphNode>(); }
		}

		#endregion

		#endregion

		#endregion

		#region ID & PropertyDefinitions

		#region Properties

		public object this[string index]
		{
			get
			{
				CheckPropertyIsDefined(index);
				if (properties.ContainsKey(index))
				{
					return properties[index].Value;
				}
				throw new PropertyNotExistException(index);
			}

			set
			{
				CheckPropertyIsDefined(index);

				if (!properties.ContainsKey(index))
				{
					properties.Add(index, new Property(Definition.PropertyDefinitions[index]));
				}
				properties[index].Value = value;
			}
		}

		public IEnumerable<Property> Properties
		{
			get { return properties.Values; }
		}

		public bool HasProperty(string propertyName)
		{
			return properties.ContainsKey(propertyName);
		}

		public int PropertyCount => properties.Count;

		internal void CheckPropertyIsDefined(string propertyName)
		{
			if (!Definition.PropertyDefinitions.HasDefinition(propertyName))
			{
				throw new PropertyNotDefinedException(propertyName);
			}
		}

		readonly static Regex newLineRegex = new Regex(@"([\r\n]+ |[\r\n]+| [\r\n]+)", RegexOptions.Compiled);
		readonly static Regex anyNewLineCharRegex = new Regex(@"((\r\n)|\r|\n)", RegexOptions.Compiled);

		string StripDuplicateWhiteSpace(string text)
		{
			return newLineRegex.Replace(text.Trim(), " ");
		}

		public (bool Success, string Error) AddProperty(Property property)
		{
			bool success = false;
			var error = string.Empty;

			CheckPropertyIsDefined(property.Name);

			if (property.Value != DBNull.Value && property.Value is string originalValue)
			{
				if (property.Definition.StripCRLF)
				{
					property.Value = StripDuplicateWhiteSpace(originalValue);

					if (!originalValue.Equals((string)property.Value))
					{
						sessionServices.Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Property: \"{0}\" of Entity: \"{1}\" was trimmed of white space and stripped of CRLF characters", property.Name, EntityName));
					}
				}
				else
				{
					property.Value = anyNewLineCharRegex.Replace(originalValue, "\r\n");
				}
			}

			var validationResult = property.Validate();
			if (validationResult.Success)
			{
				if (!properties.ContainsKey(property.Name))
				{
					properties.Add(property.Name, property);
					success = true;
				}
				else
				{
					error = $"Duplicate {property} added to Entity: {GetDictionaryContents()}";
				}
			}
			else
			{
				error = $"{Definition.FullName}.{property.Name} validation failed: {validationResult.Error}";
			}

			return (success, error);
		}

		internal Property FindProperty(string name)
		{
			CheckPropertyIsDefined(name);
			return properties[name];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public string GetDictionaryContents()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format("{0} ({1}):\n", Definition.EntityName, Definition.TableName));

			foreach (var property in properties)
			{
				stringBuilder.Append(string.Format("Key: <{0}>, Value: [{1}]\n", property.Key, property.Value));
			}

			return stringBuilder.ToString();
		}

		#endregion

		#region InternalPK

		public Guid InternalPK
		{
			get
			{
				return internalPK;
			}
			set
			{
				var oldValue = internalPK;
				if (oldValue != value)
				{
					internalPK = value;

					lock (sessionServices.EntitiesReferencingPK)
					{
						if (sessionServices.EntitiesReferencingPK.TryGetValue(oldValue, out var entitiesWithOldPK))
						{
							if (oldValue == Guid.Empty) // This is a special case.
							{
								entitiesWithOldPK = null;
							}
							else if (internalPK != Guid.Empty)
							{
								sessionServices.EntitiesReferencingPK.Remove(oldValue);
							}
							else
							{
								var indexOfThisEntity = entitiesWithOldPK.FindIndex(entity => object.ReferenceEquals(this, entity));
								entitiesWithOldPK.RemoveAt(indexOfThisEntity);
							}
						}

						if (internalPK != Guid.Empty)
						{
							if (!sessionServices.EntitiesReferencingPK.TryGetValue(internalPK, out var entitiesWithNewPK))
							{
								entitiesWithNewPK = new EntityCollection();
								sessionServices.EntitiesReferencingPK.Add(internalPK, entitiesWithNewPK);
							}
							if (entitiesWithOldPK != null)
							{
								entitiesWithNewPK.AddRange(entitiesWithOldPK);
								entitiesWithOldPK.ForEach(entity => ((Entity)entity).internalPK = internalPK);
							}
							else
							{
								entitiesWithNewPK.Add(this);
							}
						}

						InternalPKChanged(this, EventArgs.Empty);
					}
				}
			}
		}
		Guid internalPK;

		#endregion // InternalPK

		#endregion

		#region Notes

		public IEnumerable<UnmatchOrgRecord> Notes
		{
			get { return notes ?? Enumerable.Empty<UnmatchOrgRecord>(); }
		}

		public void AddNote(UnmatchOrgRecord note)
		{
			notes = notes ?? new List<UnmatchOrgRecord>();
			notes.Add(note);
		}
		List<UnmatchOrgRecord> notes;

		#endregion

		#region Equals & ToString

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format("{0} ({1}):", Definition.EntityName, Definition.TableName));
			stringBuilder.Append("[");
			foreach (var property in Properties)
			{
				stringBuilder.Append(property);
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public bool Equals(Entity other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			if (!Equals(other.definition, definition))
			{
				return false;
			}

			if (InternalPK == Guid.Empty || other.InternalPK == Guid.Empty)
			{
				if (Properties.Count() != other.Properties.Count())
				{
					return false;
				}

				foreach (var property in Properties)
				{
					if (!other.HasProperty(property.Name))
					{
						return false;
					}

					if (!other[property.Name].Equals(property.Value))
					{
						return false;
					}
				}
				return true;
			}
			return other.InternalPK.Equals(InternalPK);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(Entity))
			{
				return false;
			}

			return Equals((Entity)obj);
		}

		public override int GetHashCode()
		{
			unchecked { return (InternalPK.GetHashCode() * 397) ^ (definition != null ? definition.GetHashCode() : 0); }
		}

		#endregion

		string IEntityInfo.TableName
		{
			get { return definition.TableName; }
		}

		Type IEntityInfo.Type
		{
			get { return null; }
		}

		/// <summary>
		/// Set this to true to tell the engine not to bother trying to find an existing row (from natural keys) if the interceptor cannot find one.
		/// </summary>
		public bool DisableNativeEnginesOwnNaturalKeyMatch { get; set; }
	}

	public static class EntityRelationExtension
	{
		public static string GetPropertyOrBlankString(this IEntity entity, string propertyName)
		{
			return entity != null && entity.HasProperty(propertyName) ? entity[propertyName].ToString() : "";
		}

		public static bool HasOne(this IEntity self, IEntityDefinition childDef)
		{
			var selfDef = self.Definition;
			return selfDef.AssociationCollection[childDef, selfDef].Cardinality == Cardinality.OneToOne;
		}

		public static bool HasMany(this IEntity self, IEntityDefinition childDef)
		{
			var selfDef = self.Definition;
			return selfDef.AssociationCollection[childDef, selfDef].Cardinality == Cardinality.OneToMany;
		}

		public static IEnumerable<IEntity> BelongsTo(this IEntity self)
		{
			return self.Parents.Where(e => e != self.Parent);
		}

		public static IEnumerable<IEntity> OneToOne(this IEntity self)
		{
			return self.Children.Where(e => self.HasOne(e.Definition));
		}

		public static IEnumerable<IEntity> OneToMany(this IEntity self)
		{
			return self.Children.Where(e => self.HasMany(e.Definition));
		}

		public static bool HasRelatedEntity(this IEntity self, string entityName)
		{
			var hasEntityInParents = self.Parents.Any(entity => entity.EntityName == entityName);
			var hasEntityInChild = self.Children.Any(entity => entity.EntityName == entityName);
			return hasEntityInParents || hasEntityInChild;
		}

		public static IEntity GetParentEntity(this IEntity self, string entityName)
		{
			return self.Parents.FirstOrDefault(entity => entity.EntityName == entityName);
		}
	}
}

