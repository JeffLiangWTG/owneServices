using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.Integration;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	/// <summary>
	/// An IDataReflectorFilter for Universal XML data transfer objects
	/// Used for building XML macros in MapTreeForm.
	/// </summary>
	public class UniversalXmlDataReflectorFilter : IDataReflectorFilter
	{
		public bool IsAllowed(PropertyInfo property)
			=> IsAllowableType(property.PropertyType) && !ShouldExclude(property);

		/// <summary>
		/// Methods are not allowed. They are not used for data transfer in Universal XML.
		/// </summary>
		public bool IsAllowed(MethodInfo method)
			=> false;

		public bool CanHaveChildMembers(Type type)
			=> !IsField(type) && (IsCollection(type) || IsRelatedObject(type));

		public bool IsCollection(Type type)
			=> type != typeof(string) &&
			   (
				   BusinessObjectReflector.IsCollection(type)
					|| InheritsFrom(type, typeof(ICollection<>))
					 || IsIEnumerable(type)
				)
			;

		static bool IsIEnumerable(Type type)
		{
			return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || type.GetInterfaces().Any(IsIEnumerable);
		}

		public bool IsRelatedObject(Type type)
		{
			return typeof(UniversalDataBuss.Integration.IDataObject).IsAssignableFrom(type);
		}

		public string NamespacePrefix => "@UXML.";

		bool IsAllowableType(Type type)
		{
			return IsField(type)
				|| IsRelatedObject(type)
				|| IsCollection(type);
		}

		bool IsField(Type type)
			=> IsNullableZType(type) || BusinessObjectReflector.IsField(type);

		bool IsNullableZType(Type type)
		{
			var underlyingType = Nullable.GetUnderlyingType(type);
			return underlyingType != null
				&& typeof(IZType).IsAssignableFrom(underlyingType);
		}

		bool InheritsFrom(Type t1, Type t2)
		{
			if (t1 == null || t2 == null)
			{
				return false;
			}

			var baseType = t1.BaseType;
			if (baseType != null &&
				baseType.IsGenericType &&
				baseType.GetGenericTypeDefinition() == t2)
			{
				return true;
			}

			if (InheritsFrom(baseType, t2))
			{
				return true;
			}

			return
				(t2.IsAssignableFrom(t1) && t1 != t2)
				||
				t1.GetInterfaces().Any(x =>
				  x.IsGenericType &&
				  x.GetGenericTypeDefinition() == t2);
		}

		bool ShouldExclude(PropertyInfo property)
		{
			var getMethod = property.GetGetMethod();
			if (getMethod == null)
			{
				return true;
			}
			if (getMethod.GetParameters().Any())
			{
				return true;
			}

			Type propertyType = property.PropertyType;
			if (TypesToIgnore.Contains(propertyType))
			{
				return true;
			}

			return false;
		}

		readonly List<Type> TypesToIgnore = new List<Type>
		{
			typeof(bool),
			typeof(BusinessObjectCollection),
			typeof(IBusinessObjectCollection),
			typeof(ActiveBusinessObjectCollection),
			typeof(IActiveBusinessObjectCollection),
			typeof(NonPersistentBusinessObjectCollection<>),
			typeof(IKeyDataPairCollection)
		};
	}
}
