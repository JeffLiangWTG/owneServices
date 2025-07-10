using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	public abstract class ElementProcessor
	{
		protected ElementProcessor(string nameSpace, string version)
		{
			this.Namespace = Argument.NotNull(nameSpace, "string nameSpace");
			this.Version = Argument.NotNull(version, "string version");
		}

		protected ElementProcessor() : this(DefaultNamespace, DefaultVersion) { }

		readonly Dictionary<PropertyInfo, ElementPlacementManager> placingCache = new Dictionary<PropertyInfo, ElementPlacementManager>();
		internal ElementPlacementManager GetCachedPlacementManager(Element element)
		{
			var propertyInfo = element.PropertyInfo;
			if (propertyInfo != null)
			{
				ElementPlacementManager result;
				if (!placingCache.TryGetValue(propertyInfo, out result))
				{
					placingCache[propertyInfo] = result = new ElementPlacementManager(element);
				}

				return result;
			}
			else
			{
				return new ElementPlacementManager(element);
			}
		}

		readonly Dictionary<PropertyInfo, int> maxLengthCache = new Dictionary<PropertyInfo, int>();
		internal int GetMaxLengthCached(PropertyInfo propertyInfo)
		{
			int result;
			if (!maxLengthCache.TryGetValue(propertyInfo, out result))
			{
				maxLengthCache[propertyInfo] = result = propertyInfo.GetMaxLength();
			}

			return result;
		}

		internal string Namespace
		{
			get;
			private set;
		}

		internal string Version
		{
			get;
			private set;
		}

		public void SetNamespaceAndVersion(string nameSpace, string version)
		{
			this.Namespace = string.IsNullOrWhiteSpace(nameSpace) ? DefaultNamespace : nameSpace;
			this.Version = string.IsNullOrWhiteSpace(version) ? DefaultVersion : version;
		}

		static string DefaultNamespace
		{
			get { return SchemaVersionManager.Current.Namespace; }
		}

		static string DefaultVersion
		{
			get { return SchemaVersionManager.Current.Version; }
		}

		internal bool ShouldFlattenToAttributes(Type type, out string basePropertyName)
		{
			basePropertyName = null;
			var flattenedAttribute = type.GetAttribute<FlattenedIntoAttributesAttribute>();
			if (flattenedAttribute == null)
			{
				basePropertyName = null;
				return false;
			}
			else
			{
				basePropertyName = flattenedAttribute.BaseElementPropertyName;
				return Namespace == UniversalXmlInfo.Namespace_2012_11 || flattenedAttribute.FlattenEvenWithOldNamespace;
			}
		}
	}
}

