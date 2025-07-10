using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Integration;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap
{
	/// <summary>
	/// An IDataReflectorFilter for document macro data.
	/// Ignores properties and methods with DocumentFieldExcludeFromMapAttribute.
	/// Ignores properties with MacroIgnoreAttribute.
	/// Ignores properties and macro translate with DocumentMacroIgnoreAttribute.
	/// </summary>
	public class DocDataReflectorFilter : IDataReflectorFilter
	{
		public bool IsAllowed(PropertyInfo property)
			=> BusinessObjectReflector.IsAllowableType(property.PropertyType) && !ShouldExclude(property);

		public bool IsAllowed(MethodInfo method)
			=> BusinessObjectReflector.IsAllowableType(method.ReturnType) && !ShouldExclude(method);

		public bool CanHaveChildMembers(Type propertyType)
			=> BusinessObjectReflector.IsCollection(propertyType) || BusinessObjectReflector.IsRelatedObject(propertyType);

		public virtual bool IsCollection(Type type)
			=> BusinessObjectReflector.IsCollection(type);

		public bool IsRelatedObject(Type type)
			=> BusinessObjectReflector.IsRelatedObject(type);

		public string NamespacePrefix => string.Empty;

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

			Type declaringType = property.DeclaringType;
			foreach (var baseType in BaseTypes)
			{
				if (declaringType.IsAssignableFrom(baseType))
				{
					return true;
				}
			}
			if (Attribute.IsDefined(property, typeof(DocumentFieldExcludeFromMapAttribute), false)
				|| Attribute.IsDefined(property, typeof(MacroIgnoreAttribute), false)
				|| BusinessObjectReflector.ShouldBeIgnoredWhenEvaluating(property))
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

		readonly Type[] BaseTypes =
		{
			Type.GetType("Enterprise.DocumentWrappers.GenericWrappers.Base.GenericWrapper,DocumentWrappers"),
			Type.GetType("Enterprise.DocumentWrappers.DocBaseWrapper,DocumentWrappers"),
			typeof(BusinessObject),
			typeof(BusinessObjectCollection),
			typeof(ActiveBusinessObjectCollection)
		};

		bool ShouldExclude(MethodInfo method)
		{
			if (method.ReturnType == typeof(bool))
			{
				return true;
			}
			if (method.IsSpecialName)
			{
				return true;
			}
			if (!Attribute.IsDefined(method, typeof(DocumentFieldAttribute), false))
			{
				return true;
			}

			return false;
		}
	}
}
