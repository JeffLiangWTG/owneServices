using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;

[assembly: DebuggerDisplay("{PropertyType.Name, nq} {Name, nq}[{GetType().Name, nq}]", Target = typeof(PropertyDescriptor))]
namespace Enterprise.ZArchitecture.ComponentModel
{
	internal class ControlPropertyDescriptorCollection : PropertyDescriptorCollection
	{
		public ControlPropertyDescriptorCollection(Type controlType, PropertyDescriptor[] defaultDescriptors)
			: base(FilteredPropertyDescriptors(defaultDescriptors))
		{
			this.controlType = controlType;
		}

		public override PropertyDescriptor Find(string name, bool ignoreCase)
		{
			var result = base.Find(name, ignoreCase);

			if (result == null)
			{
				result = CreatePropertyDescriptor(controlType, name);
				if (result != null)
				{
					Add(result);
				}
			}

			return result;
		}

		public override string ToString() => string.Join(", ", this.Cast<PropertyDescriptor>().Select(pd => pd?.Name));

		#region Implementation

		readonly Type controlType;

		static PropertyDescriptor CreatePropertyDescriptor(Type componentType, string name)
		{
			var info = componentType.GetProperty(name);
			return info == null ? null : CreatePropertyDescriptor(componentType, info);
		}

		static PropertyDescriptor[] FilteredPropertyDescriptors(PropertyDescriptor[] unfilteredPropertyDescriptors)
		{
			if (unfilteredPropertyDescriptors == null)
			{
				return null;
			}

			var filteredPropertyDescriptors = unfilteredPropertyDescriptors.Where(x => x != null).ToArray();

			if (filteredPropertyDescriptors.Length < unfilteredPropertyDescriptors.Length)
			{
				ErrorReporter.ReportOnce("ControlPropertyDescriptorCollection should not contain null values.");
			}

			return filteredPropertyDescriptors;
		}

		static PropertyDescriptor CreatePropertyDescriptor(Type componentType, PropertyInfo info)
		{
			var getMethod = info.GetGetMethod();
			var setMethod = info.GetSetMethod();
			return new ReflectPropertyDescriptor(componentType, info.Name, info.PropertyType, info, getMethod, setMethod, null);
		}

		#endregion
	}
}
