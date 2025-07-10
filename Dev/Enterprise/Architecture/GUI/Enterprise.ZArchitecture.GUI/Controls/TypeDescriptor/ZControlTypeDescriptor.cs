using System;
using System.ComponentModel;
using System.Reflection;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZControlTypeDescriptor : ICustomTypeDescriptor
	{
		const string MetadataReflectionMethodName = "GetPropertyDescriptors";

		public ZControlTypeDescriptor(Type controlType)
		{
			this.ControlType = controlType;
		}

		#region ICustomTypeDescriptor Proxying Members

		public PropertyDescriptorCollection GetProperties()
		{
			if (DesignModeFinder.IsDesigning)
			{
				return DotNetTypeDescriptor.GetProperties();
			}

			if (properties == null)
			{
				properties = new ControlPropertyDescriptorCollection(ControlType, GetPropertyDescriptors(ControlType));
			}

			return properties;
		}

		public static PropertyDescriptor[] GetPropertyDescriptors(Type controlType)
		{
			var method = GetPropertyDescriptorMetadataMethod(controlType) ?? throw new InvalidOperationException(MetadataReflectionMethodName + "() method should be implemented for " + controlType.FullName);

			return (PropertyDescriptor[])method.Invoke(null, null);
		}

		internal static MethodInfo GetPropertyDescriptorMetadataMethod(Type controlType)
		{
			return controlType.GetMethod(MetadataReflectionMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		}

		public TypeConverter GetConverter()
		{
			return DotNetTypeDescriptor.GetConverter();
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			if (DesignModeFinder.IsDesigning)
			{
				return DotNetTypeDescriptor.GetEvents(attributes);
			}

			if (events == null)
			{
				events = new ControlEventDescriptorCollection(ControlType);
			}

			return events;
		}

		public EventDescriptorCollection GetEvents()
		{
			return GetEvents(Array.Empty<Attribute>());
		}

		public AttributeCollection GetAttributes()
		{
			return DotNetTypeDescriptor.GetAttributes();
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection result;

			if (attributes == null)
			{
				result = GetProperties();
			}
			else
			{
				result = DotNetTypeDescriptor.GetProperties(attributes);
			}

			return result;
		}

		public object GetEditor(Type editorBaseType)
		{
			return DotNetTypeDescriptor.GetEditor(editorBaseType);
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			return DotNetTypeDescriptor.GetDefaultProperty();
		}

		public EventDescriptor GetDefaultEvent()
		{
			return DotNetTypeDescriptor.GetDefaultEvent();
		}

		public string GetClassName()
		{
			return ControlType.FullName;
		}

		#endregion

		#region ICustomTypeDescriptor Unsupported Members

		string ICustomTypeDescriptor.GetComponentName()
		{
			throw new NotSupportedException();
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			throw new NotSupportedException();
		}

		#endregion

		ICustomTypeDescriptor DotNetTypeDescriptor
		{
			get
			{
				if (dotNetTypeDescriptor == null)
				{
					// Passing in typeof(Component) so that we don't get stack overflow.
					// DotNetTypeDescriptor.GetProvider(ControlType) will return ZControlDescriptionProvider which will return this class on GetTypeDescriptor()
					var dotNetTypeDescriptionProvider = TypeDescriptor.GetProvider(typeof(Component));
					dotNetTypeDescriptor = dotNetTypeDescriptionProvider.GetTypeDescriptor(ControlType);
				}
				return dotNetTypeDescriptor;
			}
		}

		public readonly Type ControlType;

		PropertyDescriptorCollection properties;
		EventDescriptorCollection events;

		ICustomTypeDescriptor dotNetTypeDescriptor;
	}
}
