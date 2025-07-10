using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Security;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A designer action list that honors the DesignerActionAttribute attribute on designable components.
	/// </summary>
	[SecurityCritical]
	public class KDesignerActionList : DesignerActionList, ICustomTypeDescriptor
	{
		public KDesignerActionList(IComponent component)
			: base(component)
		{
			Argument.NotNull(component, nameof(component));
		}

		public new IComponent Component
		{
			get
			{
				return base.Component;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsSettingDesignerActionListProperty
		{
			get { return isSettingDesignerActionListProperty; }
			set { isSettingDesignerActionListProperty = value; }
		}
		[ThreadStatic]
		static bool isSettingDesignerActionListProperty;

		#region GetSortedActionItems

		public override DesignerActionItemCollection GetSortedActionItems()
		{
			var result = new DesignerActionItemCollection();
			foreach (var property in PropertiesWithDesignerActionAttribute)
			{
				result.Add(new DesignerActionPropertyItem(property.Name, property.Name));
			}
			foreach (var itemSource in DesignerActionItemSources)
			{
				foreach (var item in itemSource.GetSortedActionItems(this))
				{
					result.Add(item);
				}
			}
			return result;
		}

		#endregion

		#region PropertiesWithDesignerActionAttribute

		PropertyDescriptor[] PropertiesWithDesignerActionAttribute
		{
			get
			{
				if (propertiesWithDesignerActionAttribute == null)
				{
					propertiesWithDesignerActionAttribute = GetPropertiesFromDesignerActionEntries(DesignerActionEntries);
				}
				return propertiesWithDesignerActionAttribute;
			}
		}
		PropertyDescriptor[] propertiesWithDesignerActionAttribute;

		static PropertyDescriptor[] GetPropertiesFromDesignerActionEntries(IList<DesignerActionEntry> entries)
		{
			Argument.NotNull(entries, nameof(entries)); // Suggested By ReviewBot 
			var result = new PropertyDescriptor[entries.Count];
			for (var i = 0; i < entries.Count; i++)
			{
				if (entries[i] != null)
				{
					result[i] = new ProxyPropertyDescriptor(entries[i].Property);
				}
			}
			return result;
		}

		#endregion

		#region DesignerActionEntries / Verbs

		IDesignerActionItemSource[] DesignerActionItemSources
		{
			get
			{
				if (designerActionItemSources == null)
				{
					designerActionItemSources = new List<IDesignerActionItemSource>();
					var itemSource = Component as IDesignerActionItemSource;
					if (itemSource != null)
					{
						designerActionItemSources.Add(itemSource);
					}
					foreach (var extenderProviderItemSource in ExtenderProviderActionItemSources)
					{
						designerActionItemSources.Add(extenderProviderItemSource);
					}
				}
				return designerActionItemSources.ToArray();
			}
		}
		List<IDesignerActionItemSource> designerActionItemSources;

		IDesignerActionItemSource[] ExtenderProviderActionItemSources
		{
			get
			{
				PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemsIfRequired();
				return extenderProviderActionItemSources.ToArray();
			}
		}
		readonly List<IDesignerActionItemSource> extenderProviderActionItemSources = new List<IDesignerActionItemSource>();

		DesignerActionEntry[] DesignerActionEntries
		{
			get
			{
				PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemsIfRequired();
				return designerActionEntries.ToArray();
			}
		}
		readonly List<DesignerActionEntry> designerActionEntries = new List<DesignerActionEntry>();

		void PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemsIfRequired()
		{
			if (!populatePropertyDesignerActionEntriesAndExtenderProviderActionItemSourcesCalled)
			{
				PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources();
				populatePropertyDesignerActionEntriesAndExtenderProviderActionItemSourcesCalled = true;
			}
		}
		bool populatePropertyDesignerActionEntriesAndExtenderProviderActionItemSourcesCalled;

		void PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources()
		{
			PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources(TypeDescriptor.GetProperties(Component));
			foreach (var interfaceType in Component.GetType().GetInterfaces())
			{
				PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources(interfaceType);
			}
			var eventService = (IEventBindingService)GetService(typeof(IEventBindingService));
			if (eventService != null)
			{
				var events = TypeDescriptor.GetEvents(Component);
				var properties = eventService.GetEventProperties(events);
				if (properties != null)
				{
					PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources(properties);
				}
			}
			designerActionEntries.Sort();
		}

		void PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources(Type componentType)
		{
			PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources(TypeDescriptor.GetProperties(componentType));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		void PopulatePropertyDesignerActionEntriesAndExtenderProviderActionItemSources(PropertyDescriptorCollection properties)
		{
			Argument.NotNull(properties, nameof(properties)); // Suggested By ReviewBot 
			foreach (PropertyDescriptor property in properties)
			{
				if (property != null)
				{
					var attr = (SmartTagVisibleAttribute)property.Attributes[typeof(SmartTagVisibleAttribute)];
					if (attr != null || (property.Name == "Name" && IsNamePropertyElligible))
					{
						var propertyOnComponent = TypeDescriptor.GetProperties(Component)[property.Name] ?? property;
						var item = new DesignerActionEntry(attr, propertyOnComponent);
						if (!designerActionEntries.Contains(item))
						{
							designerActionEntries.Add(item);
						}
					}
					PopulateExtenderProviderActionItemSourceIfRequired(property);
				}
			}
		}

		void PopulateExtenderProviderActionItemSourceIfRequired(PropertyDescriptor property)
		{
			var provider = property.GetExtenderProvider();
			var itemSource = provider as IDesignerActionItemSource;
			if (itemSource != null && !extenderProviderActionItemSources.Contains(itemSource))
			{
				extenderProviderActionItemSources.Add(itemSource);
			}
		}

		#endregion

		#region Contract Invariants

		#endregion

		#region DesignerActionEntry

		class DesignerActionEntry : IComparable<DesignerActionEntry>
		{
			public DesignerActionEntry(SmartTagVisibleAttribute attribute, PropertyDescriptor property)
			{
				Argument.NotNull(property, nameof(property));
				Attribute = attribute;
				Property = property;
			}

			public SmartTagVisibleAttribute Attribute { get; private set; }
			public PropertyDescriptor Property { get; private set; }

			public override int GetHashCode()
			{
				return Property.Name.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var rhs = obj as DesignerActionEntry;
				return rhs != null && Property.Name == rhs.Property.Name;
			}

			public int OrderIndex
			{
				get { return Attribute == null ? 0 : Attribute.OrderIndex; }
			}

			#region IComparable<DesignerActionEntry> Members

			int IComparable<DesignerActionEntry>.CompareTo(DesignerActionEntry other)
			{
				var result = -1;
				if (other != null && (result = OrderIndex - other.OrderIndex) == 0)
				{
					result = string.Compare(Property.Name, other.Property.Name, StringComparison.Ordinal);
				}
				return result;
			}

			#endregion
		}

		#endregion

		#region ICustomTypeDescriptor.GetProperties

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return (this as ICustomTypeDescriptor).GetProperties(Array.Empty<Attribute>());
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			var result = new ArrayList();
			var properties = TypeDescriptor.GetProperties(this, attributes, true);
			foreach (PropertyDescriptor prop in properties)
			{
				if (prop != null)
				{
					result.Add(new ProxyPropertyDescriptor(prop));
				}
			}
			result.AddRange(PropertiesWithDesignerActionAttribute);
			return new PropertyDescriptorCollection((PropertyDescriptor[])result.ToArray(typeof(PropertyDescriptor)));
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			object result = this;
			if (pd != null && pd.ComponentType != null && !pd.ComponentType.IsAssignableFrom(typeof(KDesignerActionList)))
			{
				result = TypeDescriptor.GetAssociation(pd.ComponentType, Component);
			}
			return result;
		}

		#endregion

		#region ICustomTypeDescriptor delegate to TypeDescriptor

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			if (editorBaseType == null)
			{
				throw new ArgumentNullException(nameof(editorBaseType));
			}
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		#endregion

		#region ProxyPropertyDescriptor

		class ProxyPropertyDescriptor : KPropertyDescriptor
		{
			public ProxyPropertyDescriptor(PropertyDescriptor inner)
				: base(null, inner)
			{
				Argument.NotNull(inner, nameof(inner));
			}

			// HACK: the .net designer has the code 'PropertyDescriptor.ComponentModel.GetProperty("name").GetSetMethod() == null'
			// which raises a NullReferenceException, which doesn't even produce a call stack in the debugger!
			public override Type ComponentType
			{
				get { return new MyComponentType(base.ComponentType, Inner.Name); }
			}

			protected override object GetValueCore(object component)
			{
				var list = (KDesignerActionList)component;
				if (list != null)
				{
					return base.GetValueCore(list.Component);
				}

				return null;
			}

			protected override void SetValueCore(object component, object value)
			{
				var list = (KDesignerActionList)component;
				if (list != null)
				{
					KDesignerActionList.IsSettingDesignerActionListProperty = true;
					try
					{
						base.SetValueCore(list.Component, value);
					}
					finally
					{
						KDesignerActionList.IsSettingDesignerActionListProperty = false;
					}
				}
			}

			protected override void HandleException(object component, Exception e)
			{
			}

			class MyComponentType : TypeDelegator
			{
				public MyComponentType(Type type, string fakePropertyName)
					: base(type)
				{
					this.fakePropertyName = fakePropertyName;
				}

				protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttribute, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
				{
					if (name == fakePropertyName)
					{
						return typeof(MyComponentType).GetProperty("FakeProperty");
					}
					return base.GetPropertyImpl(name, bindingAttribute, binder, returnType, types, modifiers);
				}

				public object FakeProperty
				{
					get { return null; }
					set { }
				}

				readonly string fakePropertyName;
			}
		}

		#endregion

		#region Implementation

		bool IsNamePropertyElligible
		{
			get
			{
				if (!isNamePropertyElligibleRetrieved)
				{
					var nameProperty = TypeDescriptor.GetProperties(Component)["Name"];
					isNamePropertyElligible =
						nameProperty != null &&
						!nameProperty.IsReadOnly &&
						DesignerHost != null &&
						Component != DesignerHost.RootComponent;
					isNamePropertyElligibleRetrieved = true;
				}
				return isNamePropertyElligible;
			}
		}
		bool isNamePropertyElligible;
		bool isNamePropertyElligibleRetrieved;

		IDesignerHost DesignerHost
		{
			get
			{
				if (designerHost == null)
				{
					designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
				}
				return designerHost;
			}
		}
		IDesignerHost designerHost;

		#endregion
	}
}
