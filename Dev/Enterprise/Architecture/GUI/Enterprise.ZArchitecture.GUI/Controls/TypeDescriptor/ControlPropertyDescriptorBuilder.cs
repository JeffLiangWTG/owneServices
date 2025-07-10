using System.Collections.Generic;
using System.ComponentModel;

using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ControlPropertyDescriptorBuilder<TComponent>
	{
		readonly List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

		public ControlPropertyDescriptorBuilder()
		{
			var method = ZControlTypeDescriptor.GetPropertyDescriptorMetadataMethod(typeof(TComponent).BaseType);
			if (method != null)
			{
				properties.AddRange((PropertyDescriptor[])method.Invoke(null, null));
			}
		}

		public ControlPropertyDescriptorBuilder<TComponent> Property<TProperty>(string name)
		{
			return Property(name, default(TProperty));
		}

		public ControlPropertyDescriptorBuilder<TComponent> Property<TProperty>(string name, TProperty defaultValue)
		{
			return Property(name, defaultValue, true);
		}

		public ControlPropertyDescriptorBuilder<TComponent> Property<TProperty>(string name, TProperty defaultValue, bool registerEvent)
		{
			foreach (var property in properties)
			{
				if (property.Name == name)
				{
					properties.Remove(property);
					break;
				}
			}
			properties.Add(new ControlPropertyDescriptor<TComponent, TProperty>(name, defaultValue, registerEvent));
			return this;
		}

		public PropertyDescriptor[] Result
		{
			get { return properties.ToArray(); }
		}
	}
}
