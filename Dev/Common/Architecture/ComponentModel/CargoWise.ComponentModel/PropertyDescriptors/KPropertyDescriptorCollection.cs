using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	public static class KPropertyDescriptorCollectionExtensions
	{
		public static KPropertyDescriptorCollection AllPropertiesSafe(this KPropertyDescriptorCollection collection)
		{
			return collection != null ? collection.AllProperties : null;
		}
	}

	/// <summary>
	/// Base class for customised PropertyDescriptorCollection objects.
	/// NOTE: foreaching over a variable of type KPropertyDescriptorCollection may behave differently to foreaching over a variable
	///       of type PropertyDescriptorCollection.
	/// </summary>
	public class KPropertyDescriptorCollection : PropertyDescriptorCollection
	{
		protected internal KPropertyDescriptorCollection(PropertyDescriptor[] properties)
			: base(properties)
		{
		}

		#region Factory Methods

		protected internal KPropertyDescriptorCollection(Type componentType, bool includePrivate)
			: base(Array.Empty<PropertyDescriptor>())
		{
			Argument.NotNull(componentType, nameof(componentType));
			ComponentType = componentType;
			IncludePrivate = includePrivate;
		}

		public static KPropertyDescriptorCollection FromType(Type componentType)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return FromType(componentType, false);
		}

		public static KPropertyDescriptorCollection FromType(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return factory.FromType(componentType, includePrivate);
		}

		static readonly PropertyDescriptorCollectionFactory<KPropertyDescriptorCollection> factory = new PropertyDescriptorCollectionFactory<KPropertyDescriptorCollection>(delegate(Type componentType)
		{
			return new KPropertyDescriptorCollection(componentType, false);
		});

		public static KPropertyDescriptorCollection ConvertIfRequired(PropertyDescriptorCollection collection)
		{
			Argument.NotNull(collection, nameof(collection)); // Suggested By ReviewBot 
			var result = collection as KPropertyDescriptorCollection ?? new KPropertyDescriptorCollection((PropertyDescriptor[])new ArrayList(collection).ToArray(typeof(PropertyDescriptor)));
			return result;
		}

		#endregion

		#region ComponentType / IncludePrivate / IncludingFlattened

		/// <summary>
		/// Get the Type of the component we are getting properties from.
		/// </summary>
		public Type ComponentType { get; }

		/// <summary>
		/// Get whether we're going to include private properties.
		/// </summary>
		protected bool IncludePrivate { get; private set; }

		/// <summary>
		/// Get a collection that can be enumerated over that includes flattened properties (properties with '+' in their property name).
		/// </summary>
		public PropertyDescriptorCollection IncludingFlattened
		{
			get
			{
				return this;
			}
		}

		#endregion

		#region Find

		public sealed override PropertyDescriptor Find(string name, bool ignoreCase)
		{
			var result = LastFoundPropertyDescriptor;
			if (result == null || !string.Equals(result.Name, name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				if (name != null)
				{
					for (int i = 0; i < 10; i++)
					{
						try
						{
							result = FindCore(name, ignoreCase);
							if (result == null || string.Equals(result.Name, name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
							{
								break;
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							if (i == 9)
							{
								throw;
							}
						}
					}
				}
				LastFoundPropertyDescriptor = result;
			}
			return result;
		}
		PropertyDescriptor LastFoundPropertyDescriptor;

		protected virtual PropertyDescriptor FindCore(string name, bool ignoreCase)
		{
			Argument.NotNull(name, nameof(name)); // Suggested By ReviewBot 
			var dictionary = ignoreCase ? FastPropertyCacheIgnoreCase : FastPropertyCache;
			var result = dictionary[name] as PropertyDescriptor;
			if (result == null || !string.Equals(result.Name, name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				result = base.Find(name, ignoreCase);
				lock (dictionary)
				{
					dictionary[name] = result;
				}
			}
			return result;
		}

		readonly HybridDictionary FastPropertyCache = new HybridDictionary(false);
		readonly HybridDictionary FastPropertyCacheIgnoreCase = new HybridDictionary(true);

		#endregion

		#region Contract Invariants

		#endregion

		#region AllProperties / PropertiesWithNoGenericIncompletePropertyTypes

		/// <summary>
		/// Get PropertyDescriptor objects for properties.
		/// </summary>
		public virtual KPropertyDescriptorCollection AllProperties
		{
			get
			{
				if (allProperties == null && !IncludePrivate)
				{
					if (ComponentType == null)
					{
						throw new InvalidOperationException("ComponentType must be non-null if IncludePrivate is false.");
					}

					var newAllProperties = NewEmptyPropertyDescriptorCollection(ComponentType, true); // store in a local variable first for ThreadSafety. See TestIndexerThreadSafety.
					newAllProperties.PopulatePropertyDescriptors();

					if (newAllProperties.GetType() != GetType())
					{
						throw new InvalidOperationException(
								"The return value of NewPrivatePropertiesCollection " +
								"was not of the same type as this (" + GetType().Name + ")");
					}
					allProperties = newAllProperties;
				}
				return (IncludePrivate) ? this : allProperties;
			}
		}
		KPropertyDescriptorCollection allProperties;

		public PropertyDescriptor[] PropertiesWithNoGenericParametersOnPropertyTypes
		{
			get
			{
				if (propertiesWithNoGenericIncompletePropertyTypes == null)
				{
					var list = new List<PropertyDescriptor>();

					foreach (PropertyDescriptor property in this)
					{
						if (property.PropertyType != null && !property.PropertyType.ContainsGenericParameters)
						{
							list.Add(property);
						}
					}
					propertiesWithNoGenericIncompletePropertyTypes = list.ToArray();
				}
				return propertiesWithNoGenericIncompletePropertyTypes;
			}
		}
		PropertyDescriptor[] propertiesWithNoGenericIncompletePropertyTypes;

		#endregion

		#region GetPropertyDescriptorCollectionForComponentType / NewEmptyPropertyDescriptorCollection

		protected KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentType(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			var result = GetPropertyDescriptorCollectionForComponentTypeCore(componentType, includePrivate);
#if DEBUG
			string coreMethodName = "GetPropertyDescriptorCollectionForComponentTypeCore";
			if (result == null || !GetType().IsInstanceOfType(result)) // MS RiSE haven't put an ensure for type == null being false in this case.
			{
				throw new InvalidOperationException("You must override " + coreMethodName + " on " + GetType().FullName);
			}
			if (result.IncludePrivate != IncludePrivate)
			{
				throw new InvalidOperationException("IncludePrivate on the collection returned from " + coreMethodName + " is not consistent with the 'includePrivate' parameter");
			}
#endif
			return result;
		}

		protected virtual KPropertyDescriptorCollection GetPropertyDescriptorCollectionForComponentTypeCore(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return FromType(componentType, includePrivate);
		}

		/// <summary>
		/// Create a new empty KPropertyDescriptorCollection object.
		/// </summary>
		public KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollection(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			var result = NewEmptyPropertyDescriptorCollectionCore(componentType, includePrivate);
#if DEBUG
			var coreMethodName = "NewEmptyPropertyDescriptorCollectionCore";
			if (result.Count != 0)
			{
				throw new InvalidOperationException("You must return an empty collection from " + coreMethodName);
			}
			if (result.GetType() != GetType())
			{
				throw new InvalidOperationException("You must override " + coreMethodName + " and return a collection of type " + GetType().FullName);
			}
			if (includePrivate != result.IncludePrivate)
			{
				throw new InvalidOperationException("IncludePrivate on the collection returned from " + coreMethodName + " is not consistent with the 'includePrivate' parameter");
			}
#endif
			return result;
		}

		/// <summary>
		/// Create a new empty KPropertyDescriptorCollection object.
		/// </summary>
		protected virtual KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
		{
			Argument.NotNull(componentType, nameof(componentType));
			return new KPropertyDescriptorCollection(componentType, includePrivate);
		}

		#endregion

		#region Making Enumeration Deterministic

		public new PropertyDescriptor this[int index]
		{
			get
			{
				if (!(0 <= index && index < Count))
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				return EnumerableProperties[index];
			}
		}

		public new int Count
		{
			get { return EnumerableProperties.Length; }
		}

		public new int Add(PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property));
			if (ShouldEnumerateOver(property))
			{
				enumerableProperties = null;
			}
			return base.Add(property);
		}

		public new void Remove(PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property));
			if (ShouldEnumerateOver(property))
			{
				enumerableProperties = null;
			}
			base.Remove(property);
			RemoveFromCache(property);
		}

		public new void RemoveAt(int index)
		{
			if (!(0 <= index && index < Count))
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			var current = this[index];
			if (ShouldEnumerateOver(current))
			{
				enumerableProperties = null;
			}
			base.RemoveAt(index);
			RemoveFromCache(current);
		}

		void RemoveFromCache(PropertyDescriptor property)
		{
			if (property != null)
			{
				var name = property.Name;
				RemoveFromDictionary(FastPropertyCacheIgnoreCase, name);
				RemoveFromDictionary(FastPropertyCache, name);
				LastFoundPropertyDescriptor = null;
				var cachedFoundProperties = GetCachedFoundProperties();
				cachedFoundProperties?.Remove(name);
			}
		}

		IDictionary GetCachedFoundProperties()
		{
			//NOTE: If this reflection fails then we don't need to do this code anymore
			var cachedFoundProperties = CachedFoundPropertiesField?.GetValue(this);
#if NETFRAMEWORK
			if (cachedFoundProperties is HybridDictionary hybridDictionary)
			{
				return hybridDictionary;
			}
#else
			if (cachedFoundProperties is Hashtable hashtable)
			{
				return hashtable;
			}
#endif
			return null;
		}

		internal static FieldInfo CachedFoundPropertiesField => cachedFoundPropertiesField ??=
			typeof(PropertyDescriptorCollection)
#if NETFRAMEWORK
				.GetField("cachedFoundProperties",
#else
				.GetField("_cachedFoundProperties",
#endif
				BindingFlags.NonPublic | BindingFlags.Instance);
		[ThreadStatic]
		static FieldInfo cachedFoundPropertiesField;

		void RemoveFromDictionary(HybridDictionary dictionary, string key)
		{
			lock (dictionary)
			{
				dictionary.Remove(key);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1084:DoNotUseVirtualNew", Justification = "Baseline")]
		public new virtual IEnumerator GetEnumerator()
		{
			return EnumerableProperties.GetEnumerator();
		}

		protected virtual bool ShouldEnumerateOver(PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property));
			return true;
		}

		PropertyDescriptor[] EnumerableProperties
		{
			get
			{
				if (enumerableProperties == null)
				{
					List<PropertyDescriptor> newEnumerableProperties = new List<PropertyDescriptor>();
					foreach (PropertyDescriptor property in (PropertyDescriptorCollection)this)
					{
						if (property != null && ShouldEnumerateOver(property))
						{
							newEnumerableProperties.Add(property);
						}
					}
					enumerableProperties = newEnumerableProperties.ToArray();
				}
				return enumerableProperties;
			}
		}
		PropertyDescriptor[] enumerableProperties;

#endregion

		#region PopulatePropertyDescriptors / NewPropertyDescriptor

		public void AddIfNotExists(PropertyDescriptor property)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 

			if (base.Find(property.Name, false) == null)
			{
				Add(property);
			}
		}

		readonly object lockObject = new object();

		/// <summary>
		/// Get a new PropertyDescriptorCollection that will be cached and returned from
		/// GetProperties().
		/// </summary>
		protected internal void PopulatePropertyDescriptors()
		{
			if (!propertyDescriptorsPopulated)
			{
				lock (lockObject)
				{
					if (!propertyDescriptorsPopulated)
					{
						PopulatePropertyDescriptorsCore();
						propertyDescriptorsPopulated = true;
					}
				}
			}
		}

		protected virtual void PopulatePropertyDescriptorsCore()
		{
			if (ComponentType != null)
			{
				var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
				if (IncludePrivate)
				{
					bindingFlags |= BindingFlags.NonPublic;
				}

				Type lastType = null;
				IncludeOnlyNamedPropertiesForPropertyDescriptorReflectionAttribute attribute = null;
				foreach (var propertyInfo in ComponentType.GetProperties(bindingFlags))
				{
					if (propertyInfo != null && propertyInfo.GetIndexParameters().Length == 0)
					{
						var thisType = propertyInfo.DeclaringType;
						if (thisType != lastType)
						{
							lastType = thisType;
							attribute = (IncludeOnlyNamedPropertiesForPropertyDescriptorReflectionAttribute)Attribute.GetCustomAttribute(thisType, typeof(IncludeOnlyNamedPropertiesForPropertyDescriptorReflectionAttribute), false);
						}
						if (attribute == null || attribute.IncludedPropertyNames != null && attribute.IncludedPropertyNames.Contains(propertyInfo.Name))
						{
							var reflectProperty = NewReflectPropertyDescriptor(propertyInfo);
							if (reflectProperty != null)
							{
								var propertyDescriptor = NewPropertyDescriptor(reflectProperty);
								Add(propertyDescriptor);
							}
						}
					}
				}
			}
		}

		protected virtual ReflectPropertyDescriptor NewReflectPropertyDescriptor(PropertyInfo info)
		{
			Argument.NotNull(info, nameof(info));
			if (ComponentType == null)
			{
				return null;
			}

			var propertyInfo = info;
			var methodInfo = info.GetSetMethod(true);
			while (propertyInfo != null && !propertyInfo.CanRead && methodInfo != null && methodInfo.IsVirtual)
			{
				if (propertyInfo.DeclaringType != null && propertyInfo.DeclaringType.BaseType != null)
				{
					propertyInfo = propertyInfo.DeclaringType.BaseType.GetProperty(info.Name, info.PropertyType, Array.Empty<Type>());

					if (propertyInfo != null)
					{
						methodInfo = propertyInfo.GetSetMethod(true);
					}
				}
				else
				{
					propertyInfo = null;
				}
			}
			return propertyInfo != null ? new KReflectPropertyDescriptor(ComponentType, propertyInfo) : null;
		}

		/// <summary>
		/// Create a new PropertyDescriptor based on a reflected PropertyDescriptor (one returned
		/// from TypeDescriptor.GetProperties(Type)[]). This is called from the default
		/// implementation of NewPropertyDescriptorCollection.
		/// </summary>
		protected internal virtual PropertyDescriptor NewPropertyDescriptor(PropertyDescriptor reflectedProperty)
		{
			var result = reflectedProperty as KPropertyDescriptor;
			if (result == null && reflectedProperty != null)
			{
				result = new KPropertyDescriptor(this, reflectedProperty);
			}
			return result;
		}

		bool propertyDescriptorsPopulated;

		#endregion
	}
}
