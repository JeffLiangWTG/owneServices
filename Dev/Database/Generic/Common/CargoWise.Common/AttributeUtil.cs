using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;

namespace CargoWise.Common
{
	/// <summary>
	/// Contains utility methods for working with attributes.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util")]
	public static class AttributeUtil
	{
		/// <summary>
		/// Finds attributes from a property. The attributes can be applied to the property multiple times
		/// for this method. The attributes are ordered by most specific component type, to most base component
		/// type the property belongs to.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags")]
		public static IEnumerable<Attribute> FindAttributes(
			Type componentType, string propertyName, Type propertyType, Type attributeType, BindingFlags flags)
		{
			List<Attribute> attributesOnProperty = new List<Attribute>();
			foreach (Attribute attr in FindAttributesOnPropertyOnly(componentType, propertyName, attributeType, flags))
			{
				yield return attr;
				attributesOnProperty.Add(attr);
			}
			foreach (Attribute attr in FindAttributes(propertyType, attributeType))
			{
				Attribute found = FindAttributeMatchInList(attributesOnProperty, attr);
				if (found == null)
				{
					yield return attr;
				}
			}
		}

		public static IEnumerable<Attribute> FindAttributes(Type componentType, Type attributeType)
		{
			if (componentType == null)
			{
				throw new ArgumentException("prop cannot be null");
			}

			if (attributeType == null)
			{
				throw new ArgumentException("attrType cannot be null");
			}

			// iterate through the attributes on the component type and all it's bases up the hierarchy
			Type type = componentType;

			List<Attribute> list = new List<Attribute>();
			foreach (Attribute attr in type.GetCustomAttributes(attributeType, false))
			{
				list.Add(attr);
				yield return attr;
			}
			while (type != null)
			{
				foreach (Attribute attr in type.GetCustomAttributes(attributeType, false))
				{
					Attribute found = FindAttributeMatchInList(list, attr);
					if (found == null)
					{
						yield return attr;
					}
				}
				type = type.BaseType;
			}
		}

		/// <summary>
		/// Finds the specified attribute on type declaration.
		/// </summary>
		/// <typeparam name="T">Type of attribute</typeparam>
		/// <param name="obj">The object to use.</param>
		/// <returns>An instance of attribute or <c>null</c> if attribute is not specified</returns>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is null</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static T GetAttribute<T>(object obj) where T : Attribute
		{
			Argument.NotNull(obj, nameof(obj));
			return GetAttribute<T>(obj.GetType());
		}

		/// <summary>
		/// Finds the specified attribute on member declaration.<br/>
		/// This method does not return subclasses.
		/// </summary>
		/// <typeparam name="T">Type of attribute</typeparam>
		/// <param name="memberInfo">The member to use.</param>
		/// <returns>An instance of attribute or <c>null</c> if attribute is not specified</returns>
		/// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is null</exception>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
		{
			Argument.NotNull(memberInfo, "info");
			var customAttributes = Attribute.GetCustomAttributes(memberInfo);
			if (customAttributes != null)
			{
				foreach (Attribute attribute in customAttributes)
				{
					if (attribute != null && attribute.GetType() == typeof(T))
					{
						return (T)attribute;
					}
				}
			}

			return null;
		}

		#region FindAttributesOnPropertyOnly

		/// <summary>
		/// Find attributes of a property
		/// </summary>
		/// <param name="property">PropertyDescriptor to inspect</param>
		/// <param name="attrType">Attribute type</param>
		/// <param name="flags">Attribute's binding flags</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "attr")]
		public static IEnumerable<Attribute> FindAttributesOnPropertyOnly(
						PropertyDescriptor property, Type attrType, BindingFlags flags)
		{
			Argument.NotNull(property, nameof(property)); // Suggested By ReviewBot 
			Argument.NotNull(property.ComponentType, nameof(property.ComponentType));
			Argument.NotNull(property.Name, nameof(property.Name));
			Argument.NotNull(attrType, nameof(attrType));
			return FindAttributesOnPropertyOnly(property.ComponentType, property.Name, attrType, flags);
		}

		static IEnumerable<Attribute> FindAttributesOnPropertyOnly(
				Type componentType, string propertyName, Type attrType, BindingFlags flags)
		{
			Argument.NotNull(componentType, nameof(componentType));
			Argument.NotNull(propertyName, nameof(propertyName));
			Argument.NotNull(attrType, nameof(attrType));
			AttributesOnPropertyOnlyCacheKey key = new AttributesOnPropertyOnlyCacheKey(componentType, propertyName, attrType, flags);
			Attribute[] result = attributesOnPropertyOnlyCache[key];
			if (result == null)
			{
				result = new List<Attribute>(FindAttributesOnPropertyOnly_NoCache(componentType, propertyName, attrType, flags)).ToArray();
				attributesOnPropertyOnlyCache.Add(key, result);
			}
			return result;
		}

		static IEnumerable<Attribute> FindAttributesOnPropertyOnly_NoCache(
				Type componentType, string propertyName, Type attrType, BindingFlags flags)
		{
			if (componentType == null)
			{
				throw new ArgumentException("componentType cannot be null");
			}

			if (attrType == null)
			{
				throw new ArgumentException("attrType cannot be null");
			}

			List<Attribute> list = new List<Attribute>();
			foreach (PropertyInfo concreteProperty in GetPropertiesInOrderOfInheritance(componentType, propertyName, flags))
			{
				List<Attribute> next = new List<Attribute>();
				foreach (Attribute attr in concreteProperty.GetCustomAttributes(attrType, false))
				{
					Attribute found = FindAttributeMatchInList(list, attr);
					if (found == null)
					{
						next.Add(attr);
						yield return attr;
					}
				}
				list.AddRange(next);
			}
		}

		static PropertyInfo[] GetPropertiesInOrderOfInheritance(Type componentType, string propertyName, BindingFlags flags)
		{
			List<PropertyInfo> result = new List<PropertyInfo>();
			Type current = componentType;
			while (current != null)
			{
				PropertyInfo property = current.GetProperty(propertyName, flags | BindingFlags.DeclaredOnly);
				if (property != null)
				{
					result.Add(property);
				}
				current = current.BaseType;
			}
			result.Sort(delegate(PropertyInfo x, PropertyInfo y)
			{
				int compareResult = 0;
				if (x != y)
				{
					compareResult = x.DeclaringType.IsAssignableFrom(y.DeclaringType) ? 1 : -1;
				}
				return compareResult;
			});
			return result.ToArray();
		}

		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<AttributesOnPropertyOnlyCacheKey, Attribute[]> attributesOnPropertyOnlyCache = new LRUCache<AttributesOnPropertyOnlyCacheKey, Attribute[]>();
		class AttributesOnPropertyOnlyCacheKey
		{
			public AttributesOnPropertyOnlyCacheKey(Type componentType, string propertyName, Type attrType, BindingFlags flags)
			{
				Argument.NotNull(componentType, nameof(componentType));
				Argument.NotNull(propertyName, nameof(propertyName));
				Argument.NotNull(attrType, nameof(attrType));
				this.ComponentType = componentType;
				this.PropertyName = propertyName;
				this.AttrType = attrType;
				this.BindingFlags = flags;
			}

			public readonly Type ComponentType;
			public readonly string PropertyName;
			public readonly Type AttrType;
			public readonly BindingFlags BindingFlags;

			public override bool Equals(object obj)
			{
				AttributesOnPropertyOnlyCacheKey rhs = (AttributesOnPropertyOnlyCacheKey)obj;
				bool result = true;

				result &= rhs != null;
				if (rhs != null)
				{
					result &= ComponentType == rhs.ComponentType;
					result &= PropertyName == rhs.PropertyName;
					result &= AttrType == rhs.AttrType;
					result &= BindingFlags == rhs.BindingFlags;
				}
				return result;
			}

			public override int GetHashCode()
			{
				return ComponentType.GetHashCode() ^ PropertyName.GetHashCode() ^ AttrType.GetHashCode();
			}
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Does an attribute in a list match a given attribute (by the Match method)?
		/// </summary>
		static Attribute FindAttributeMatchInList(IEnumerable<Attribute> list, Attribute attr)
		{
			Argument.NotNull(list, nameof(list)); // Suggested By ReviewBot 
			foreach (Attribute next in list)
			{
				if (next != null && next.Match(attr))
				{
					return next;
				}
			}
			return null;
		}

#endregion
	}
}
