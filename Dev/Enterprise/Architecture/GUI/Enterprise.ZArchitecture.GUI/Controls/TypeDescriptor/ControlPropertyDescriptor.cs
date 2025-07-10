using System;
using System.Reflection;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.ComponentModel
{
	public class ControlPropertyDescriptor<TComponent, TProperty> : ReflectPropertyDescriptor
	{
		public ControlPropertyDescriptor(string name)
			: this(name, default(TProperty))
		{
		}

		public ControlPropertyDescriptor(string name, TProperty defaultValue)
			: this(name, defaultValue, true)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public ControlPropertyDescriptor(string name, TProperty defaultValue, bool registerEvent)
			: this(name, defaultValue, registerEvent, name + "Changed")
		{
		}

		public ControlPropertyDescriptor(string name, TProperty defaultValue, bool registerEvent, string eventName)
			: this(GetPropertyInfo(name), defaultValue, registerEvent, eventName)
		{
		}

		protected ControlPropertyDescriptor(PropertyInfo property, TProperty defaultValue, bool registerEvent, string eventName)
			: base(typeof(TComponent), property.Name, typeof(TProperty), property, property.GetGetMethod(), property.GetSetMethod(), null)
		{
			this.defaultValue = defaultValue;

			propertyInfo = property;
			if (registerEvent)
			{
				eventInfo = typeof(TComponent).GetEvent(eventName);
				if (eventInfo == null)
				{
					throw new ArgumentException(
						string.Format("Event with the name {0} is not found in declaration of {1}", eventName, typeof(TComponent)));
				}
			}
		}

		public override bool IsReadOnly
		{
			get { return false; }
		}

		public override bool SupportsChangeEvents
		{
			get { return true; }
		}

		public override object GetValue(object component)
		{
			return propertyInfo.GetValue(component, null);
		}

		public override void SetValue(object component, object value)
		{
			propertyInfo.SetValue(component, value ?? defaultValue, null);
		}

		public override void AddValueChanged(object component, EventHandler handler)
		{
			if (eventInfo != null)
			{
				eventInfo.AddEventHandler(component, handler);
			}
		}

		public override void RemoveValueChanged(object component, EventHandler handler)
		{
			if (eventInfo != null)
			{
				eventInfo.RemoveEventHandler(component, handler);
			}
		}

		#region Implementation

		readonly PropertyInfo propertyInfo;
		readonly EventInfo eventInfo;
		readonly TProperty defaultValue;

		static PropertyInfo GetPropertyInfo(string name)
			=> typeof(TComponent).GetProperty(name)
				?? throw new ArgumentException(
					string.Format("Property with the name {0} is not found in declaration of {1}", name, typeof(TComponent)));

		#endregion
	}
}
