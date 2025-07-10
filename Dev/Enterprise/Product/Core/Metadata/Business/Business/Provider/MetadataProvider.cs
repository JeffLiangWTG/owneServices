using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.Metadata.Business.Extension;
using Enterprise.Metadata.Integration;

namespace Enterprise.Metadata.Business
{
	public class MetadataProvider : IMetadataProvider
	{
		public IMetadata GetMetadataFromBO(object businessObject)
		{
			if (businessObject == null)
			{
				return null;
			}

			var metadata = GetMetadataFromType(businessObject.GetType());

			if (metadata != null)
			{
				SetMetadataPropertyFromBusinessObject(metadata, businessObject);
				CreateLinkedMetadata(metadata, businessObject);
			}

			return metadata;
		}

		#region Help GetMetadataFromBO

		void SetMetadataPropertyFromBusinessObject(IMetadata metadata, object businessObject)
		{
			var properties = GetMetadataInterfaceProperties(metadata, interfaceSuffix_ShareProperty);

			foreach (var property in properties)
			{
				var businessObjectPropertyValue = GetPropertyValueFromBusinessObject(property, businessObject);
				SetPropertyValueIntoMetadata(property, businessObjectPropertyValue, metadata);
			}
		}

		void CreateLinkedMetadata(IMetadata metadata, object businessObject)
		{
			var properties = GetMetadataInterfaceProperties(metadata, interfaceSuffix_LinkedMetadata);

			foreach (var property in properties)
			{
				var businessObjectPropertyValue = GetPropertyValueFromBusinessObject(property, businessObject);
				object metadataPropertyValue;

				if (businessObjectPropertyValue is IEnumerable)
				{
					var linkedMetadatas = new List<IMetadata>();

					foreach (var elementValue in (IEnumerable)businessObjectPropertyValue)
					{
						var linkedMetadata = new MetadataProvider().GetMetadataFromBO(elementValue);
						linkedMetadatas.Add(linkedMetadata);
					}

					metadataPropertyValue = linkedMetadatas.ToArray();
				}
				else
				{
					metadataPropertyValue = new MetadataProvider().GetMetadataFromBO(businessObjectPropertyValue);
				}

				SetPropertyValueIntoMetadata(property, metadataPropertyValue, metadata);
			}
		}

		IEnumerable<PropertyInfo> GetMetadataInterfaceProperties(IMetadata metadata, string interfaceSuffix)
		{
			var result = new List<PropertyInfo>();
			var foundInterfaces = metadata.GetType().GetInterfacesInHierarchy(interfacePrefix, interfaceSuffix, typeof(EnterpriseBusinessObject).BaseType);
			foreach (var foundInterface in foundInterfaces)
			{
				result.AddRange(foundInterface.GetProperties());
			}

			return result.ToArray();
		}

		object GetPropertyValueFromBusinessObject(PropertyInfo property, object businessObject)
		{
			var businessObjectType = businessObject.GetType();
			var propertyName = property.Name;
			var businessObjectProperty = businessObjectType.GetProperty(propertyName) ?? throw new InvalidOperationException(string.Format("No public property '{0}' is defined in business object type '{1}'.", propertyName, businessObjectType.Name));

			var businessObjectPropertyValue = businessObjectProperty.GetValue(businessObject, null);
			return businessObjectPropertyValue;
		}

		void SetPropertyValueIntoMetadata(PropertyInfo property, object value, IMetadata metadata)
		{
			var metadataType = metadata.GetType();
			var metadataProperty = metadataType.GetProperty(property.Name);
			metadataProperty.SetValue(metadata, value, null);
		}

		const string interfacePrefix = "I";
		const string interfaceSuffix_LinkedMetadata = "LinkedMetadata";
		const string interfaceSuffix_ShareProperty = "ShareProperty";

		#endregion

		public IMetadata GetMetadataFromType(Type type)
		{
			var attribute = (MetadataContextAttribute)type.GetFirstAttributeInHierarchy<MetadataContextAttribute>(typeof(ZArchitecture.EnterpriseBusinessObject).BaseType);

			if (attribute != null)
			{
				return GetMetadata(attribute.Context, attribute.FullQualifiedMetadataName);
			}

			return null;
		}

		#region Help GetMetadataFromType

		IMetadata GetMetadata(MetadataContext context, string fullQualifiedMetadataName = "")
		{
			Type type;

			if (MetadataFactory.Metadatas.TryGetValue(context, out type) || TryGetMetadataUsingFullQualifiedName(fullQualifiedMetadataName, out type))
			{
				return (IMetadata)Activator.CreateInstance(type);
			}
			else
			{
				throw new InvalidOperationException(string.Format("No metadata is defined for context '{0}'.", context.ToString()));
			}
		}

		bool TryGetMetadataUsingFullQualifiedName(string fullQualifiedMetadataName, out Type type)
		{
			type = null;

			if (!string.IsNullOrEmpty(fullQualifiedMetadataName))
			{
				type = Type.GetType(fullQualifiedMetadataName, false);
			}

			return type != null;
		}

		#endregion
	}
}
