using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// This class is used at design time to store the inherited value of an IExtenderProvider extended property.
	/// </summary>
	public class InheritedExtendedPropertyValueMemory
	{
		public void NotifySiteChanged(IComponent extenderProvider)
		{
			if (extenderProvider.Site != null && extenderProvider.Site.DesignMode)
			{
				IDesignerHost designerHost = (IDesignerHost)extenderProvider.Site.GetService(typeof(IDesignerHost));
				if (designerHost != null && designerHost.Loading)
				{
					foreach (IComponent component in extenderProvider.Site.Container.Components)
					{
						foreach (ProvidePropertyAttribute attr in extenderProvider.GetType().GetCustomAttributes(typeof(ProvidePropertyAttribute), true))
						{
							ReadInheritedValue(extenderProvider, component, attr);
						}
					}
				}
			}
		}

		public object GetInheritedValue(object component, string property)
		{
			return GetInheritedValue(component, property, null);
		}

		public object GetInheritedValue(object component, string property, object defaultValue)
		{
			object result;
			if (!InheritedValues.TryGetValue(new PropertyComponentPair(component, property), out result))
			{
				result = defaultValue;
			}
			return result;
		}

		#region Implementation

		void ReadInheritedValue(IComponent extenderProvider, IComponent component, ProvidePropertyAttribute attr)
		{
			Type receiverType = Type.GetType(attr.ReceiverTypeName);
			if (receiverType.IsInstanceOfType(component) &&
				((IExtenderProvider)extenderProvider).CanExtend(component))
			{
				string getterMethodName = (NoResString)"Get" + attr.PropertyName;
				MethodInfo method = extenderProvider.GetType().GetMethod(getterMethodName, new Type[] { receiverType })
					?? throw new InvalidOperationException("Could not find extender provider getter '" + getterMethodName + "'");
				object inheritedValueSetInBaseClassInitializeComponent = method.Invoke(extenderProvider, new object[] { component });
				SetInheritedValue(component, attr.PropertyName, inheritedValueSetInBaseClassInitializeComponent);
			}
		}

		void SetInheritedValue(object component, string property, object originalValue)
		{
			InheritedValues[new PropertyComponentPair(component, property)] = originalValue;
		}

		Dictionary<PropertyComponentPair, object> InheritedValues
		{
			get
			{
				if (inheritedValues == null)
				{
					inheritedValues = new Dictionary<PropertyComponentPair, object>();
				}
				return inheritedValues;
			}
		}
		Dictionary<PropertyComponentPair, object> inheritedValues;

		class PropertyComponentPair
		{
			public PropertyComponentPair(object component, string property)
			{
				this.component = component;
				this.property = property;
			}

			public override bool Equals(object obj)
			{
				PropertyComponentPair rhs = obj as PropertyComponentPair;
				return rhs != null && property == rhs.property && component == rhs.component;
			}

			public override int GetHashCode()
			{
				return property.GetHashCode();
			}

			readonly object component;
			readonly string property;
		}

		#endregion
	}
}
