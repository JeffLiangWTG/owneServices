using System;
using System.IO;
using System.Reflection;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	abstract class ElementWithChildren : ComplexElement
	{
		protected ElementWithChildren(PropertyInfo propertyInfo, Type typeForFields, ElementProcessor processor)
			: base(propertyInfo, processor)
		{
			Initialise(typeForFields, processor);
		}

		protected ElementWithChildren(string name, Type typeForFields, ElementProcessor processor)
			: base(name, processor)
		{
			Initialise(typeForFields, processor);
		}

		void Initialise(Type typeForFields, ElementProcessor processor)
		{
			this.TypeForFields = Argument.NotNull(typeForFields, "Type typeForFields");
		}

		internal Type TypeForFields { get; private set; }

		protected virtual void ReflectOutChildren(ElementList<Element> childConverters)
		{
			foreach (var propertyInfo in TypeForFields.GetProperties())
			{
				var element = GetChildElementHandler(propertyInfo);

				if (element != null)
				{
					childConverters.Add(element);
				}
			}
		}

		protected virtual Element GetChildElementHandler(PropertyInfo propertyInfo)
		{
			if (propertyInfo.DeclaringType != TypeForFields && !Attribute.IsDefined(propertyInfo, typeof(DataPropertyAttribute)))
			{
				return null;
			}
			try
			{
				if (RestrictedPropertyAttribute.IsRestricted(propertyInfo))
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				if((ex is InvalidOperationException && ex.Message.StartsWith("Protected Data Services are not configured.")) ||
					(ex is TargetInvocationException && ex.InnerException is InvalidOperationException && ex.InnerException.Message.StartsWith("Protected Data Services are not configured.")))
				{
					// This can happen if the UniversalXsdGenerator is used in a tool ouside of CW
					// We ignore this and assume this property is not restricted.
				}
				else
				{
					throw;
				}
			}

			var propertyType = propertyInfo.PropertyType;

			if (propertyType.IsGenericType)
			{
				var genericType = propertyType.GetGenericTypeDefinition();

				if (genericType == typeof(Nullable<>))
				{
					return GetNewFieldElement(propertyInfo);
				}
				else if (genericType.IsATypeUsedForCollections())
				{
					var typeOfContent = propertyType.GetGenericArguments()[0];

					if (typeof(IDataObject).IsAssignableFrom(typeOfContent))
					{
						return GetNewListElement(propertyInfo, typeOfContent);
					}
				}
			}
			else if (typeof(Stream).IsAssignableFrom(propertyType))
			{
				return GetNewFieldElement(propertyInfo);
			}
			else
			{
				if (typeof(IDataObject).IsAssignableFrom(propertyType))
				{
					Type namespaceDependentPropertyType;
					if (NamespaceDependentAttribute.TryGetType(propertyInfo, Namespace, out namespaceDependentPropertyType))
					{
						if (namespaceDependentPropertyType == null)
						{
							return null;
						}

						propertyType = namespaceDependentPropertyType;

#if DEBUG
						if (namespaceDependentPropertyType.GetCustomAttribute<NamespaceSpecificAttribute>(false) == null || !namespaceDependentPropertyType.IsValidInThisNamespace(Namespace))
						{
							throw new XmlProcessingException(namespaceDependentPropertyType, "NamespaceSpecificAttribute needs to be applied to this type to say it is valid in the namespace:- " + Namespace);
						}
#endif
					}

					if (propertyType.IsAbstract || propertyType.IsInterface)
					{
						throw new XmlProcessingException(propertyInfo, "Invalid Namespace. NamespaceDependentAttribute needs to be applied to this property for the namespace:- " + Namespace);
					}

					return GetNewObjectElement(propertyInfo, propertyType);
				}
			}

			return null;
		}

		protected abstract Element GetNewFieldElement(PropertyInfo propertyInfo);

		protected abstract Element GetNewListElement(PropertyInfo propertyInfo, Type typeOfContainedObjects);
	}
}
