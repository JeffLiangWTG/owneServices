using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Windows.UI;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A UITypeEditor for design time use on a property that specifies a bind to member.
	/// </summary>
	public class BindingMemberEditor : UITypeEditor
	{
		#region UITypeEditor Proxied Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Required at design time only")]
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			string bindingMember = FormatBindingMember(value as string);
			MyTypeDescriptorContext modifiedContext = new MyTypeDescriptorContext(this, context);
			KBindingSource controlBinder = modifiedContext.BindingSource;
			if (controlBinder != null)
			{
				controlBinder.WarnUserIfDataSourceTypeNotValid();
				if (modifiedContext.DataSourceType is TypeNameHolder)
				{
					MessageBox.Show("Could not find DataSourceType '" + modifiedContext.DataSourceType.FullName + "' defined by the " + nameof(KBindingSource) + ".");
				}
			}
			string result = FieldEditor.EditValue(modifiedContext, new MyServiceProvider(provider), bindingMember) as string;
			return ParseBindingMember(result);
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{ return FieldEditor.GetEditStyle(new MyTypeDescriptorContext(this, context)); }

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{ return FieldEditor.GetPaintValueSupported(new MyTypeDescriptorContext(this, context)); }

		public override bool IsDropDownResizable
		{ get { return FieldEditor.IsDropDownResizable; } }

		public override void PaintValue(PaintValueEventArgs e)
		{ FieldEditor.PaintValue(e); }

		#endregion

		#region FormatBindingMember / ParseBindingMember

		static string FormatBindingMember(string bindingMember)
		{
			string result = bindingMember;
			if (bindingMember != null)
			{
				if (bindingMember.Trim() == ".")
				{
					result = RootNodeCaption;
				}
				else if (bindingMember.Trim().Length > 0)
				{
					result = RootNodeCaption + "." + bindingMember.Trim();
				}
			}
			return result;
		}

		static string ParseBindingMember(string bindingMember)
		{
			string result = bindingMember;
			if (bindingMember != null)
			{
				if (bindingMember == RootNodeCaption)
				{
					result = ".";
				}
				else if (bindingMember.StartsWith(RootNodeCaption + ".", StringComparison.Ordinal))
				{
					result = bindingMember.Substring(RootNodeCaption.Length + 1);
				}
			}
			return result;
		}

		#endregion

		#region FieldEditor

		UITypeEditor FieldEditor
		{
			get
			{
				if (fieldEditor == null)
				{
					PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(ListControl))["DisplayMember"];
					fieldEditor = (UITypeEditor)property.GetEditor(typeof(UITypeEditor));
				}
				return fieldEditor;
			}
		}
		UITypeEditor fieldEditor;

		#endregion

		#region MyTypeDescriptorContext class

		sealed class MyTypeDescriptorContext : ITypeDescriptorContext
		{
			public MyTypeDescriptorContext(BindingMemberEditor editor, ITypeDescriptorContext inner)
			{
				Editor = editor;
				Inner = inner;
			}

			public BindingMemberEditor Editor { get; private set; }
			public ITypeDescriptorContext Inner { get; private set; }

			public Type DataSourceType
			{ get { return Attribute?.GetDataSourceType(Inner.Instance, Inner.PropertyDescriptor); } }

			public Type BindingMemberTypeFilter
			{ get { return Attribute?.GetBindingMemberTypeFilter(Inner.Instance, Inner.PropertyDescriptor); } }

			public bool AllowListProperties
			{ get { return Attribute == null || Attribute.AllowListProperties; } }

			public KBindingSource BindingSource
			{
				get
				{
					object result = Inner.PropertyDescriptor.GetExtenderProvider();
					return result as KBindingSource;
				}
			}

			BindingMemberEditorAttribute Attribute
			{
				get
				{
					return (BindingMemberEditorAttribute)Inner.PropertyDescriptor.Attributes[typeof(BindingMemberEditorAttribute)]
						?? throw new InvalidOperationException("If you apply editor " + typeof(BindingMemberEditor).FullName + " to a property, you must also apply " + typeof(BindingMemberEditorAttribute).FullName + " to that property.");
				}
			}

			#region ITypeDescriptorContext

			public IContainer Container
			{ get { return Inner.Container; } }

			public object Instance
			{ get { return new InstanceWithFakeDataSource(this); } }

			public void OnComponentChanged()
			{ Inner.OnComponentChanged(); }

			public bool OnComponentChanging()
			{ return Inner.OnComponentChanging(); }

			public PropertyDescriptor PropertyDescriptor
			{ get { return Inner.PropertyDescriptor; } }

			public object GetService(Type serviceType)
			{ return Inner.GetService(serviceType); }

			#endregion
		}

		#endregion

		#region MyServiceProvider class

		class MyServiceProvider : IServiceProvider
		{
			public MyServiceProvider(IServiceProvider inner)
			{ this.inner = inner; }

			public object GetService(Type serviceType)
			{
				object result = inner.GetService(serviceType);
				if (serviceType == typeof(IWindowsFormsEditorService))
				{
					result = new MyWindowsFormsEditorService((IWindowsFormsEditorService)result);
				}
				return result;
			}

			readonly IServiceProvider inner;
		}

		#endregion

		#region MyWindowsFormsEditorService class

		class MyWindowsFormsEditorService : IWindowsFormsEditorService
		{
			public MyWindowsFormsEditorService(IWindowsFormsEditorService inner)
			{ this.inner = inner; }

			public void CloseDropDown()
			{ inner.CloseDropDown(); }

			public void DropDownControl(Control control)
			{
				TreeView treeView = null;
				foreach (Control childCtrl in control.Controls)
				{
					treeView = childCtrl as TreeView;
					if (treeView != null)
					{
						// treeView.Nodes[1].Expand(); // doesn't populate the child nodes.. so best not to do this.
						treeView.DoubleClick += new EventHandler(TreeView_DoubleClick);
						treeView.KeyUp += new KeyEventHandler(TreeView_KeyUp);
						break;
					}
				}
				inner.DropDownControl(control);
			}

			void TreeView_KeyUp(object sender, KeyEventArgs e)
			{ (sender as Control).BeginInvoke(new KeyEventHandler(TreeView_KeyUpDeferred), sender, e); }

			void TreeView_KeyUpDeferred(object sender, KeyEventArgs e)
			{
				if (e.KeyCode == Keys.Return)
				{
					TryCommitSelection((TreeView)sender);
				}
			}

			void TreeView_DoubleClick(object sender, EventArgs e)
			{ TryCommitSelection((TreeView)sender); }

			void TryCommitSelection(TreeView tv)
			{
				if (tv.SelectedNode != null)
				{
					object designBinding = tv.SelectedNode.GetType().InvokeMember("OnSelect", BindingFlags.InvokeMethod, null, tv.SelectedNode, null, CultureInfo.InvariantCulture);
					tv.Parent.GetType().InvokeMember("selectedItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, tv.Parent, new object[] { designBinding }, CultureInfo.InvariantCulture);
					CloseDropDown();
				}
			}

			public DialogResult ShowDialog(Form dialog)
			{ return inner.ShowDialog(dialog); }

			readonly IWindowsFormsEditorService inner;
		}

		#endregion

		#region InstanceWithFakeDataSource class

		class InstanceWithFakeDataSource
		{
			public InstanceWithFakeDataSource(MyTypeDescriptorContext context)
			{ this.context = context; }

			public object DataSource
			{
				get
				{
					object result = null;
					if (context.DataSourceType != null)
					{
						Type elementType = GetTypeOrListElementType(context.DataSourceType);
						result = new FakeList(context, elementType);
					}
					return result;
				}
			}

			readonly MyTypeDescriptorContext context;
		}

		#endregion

		#region FakeList class

		class FakeList : IBindingList, ITypedList
		{
			public FakeList(MyTypeDescriptorContext context, Type elementType)
			{
				this.context = context;
				this.elementType = elementType;
			}

			#region ITypedList

			PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
			{ return GetTypedListItemProperties(context, elementType, listAccessors); }

			string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
			{ return ""; }

			#endregion

			#region IBindingList Members

			void IBindingList.AddIndex(PropertyDescriptor property)
			{ }

			object IBindingList.AddNew()
			{ return null; }

			bool IBindingList.AllowEdit
			{ get { return false; } }

			bool IBindingList.AllowNew
			{ get { return false; } }

			bool IBindingList.AllowRemove
			{ get { return false; } }

			void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
			{ throw new NotSupportedException(); }

			int IBindingList.Find(PropertyDescriptor property, object key)
			{ throw new NotSupportedException(); }

			bool IBindingList.IsSorted
			{ get { return false; } }

			event ListChangedEventHandler IBindingList.ListChanged
			{
				add { }
				remove { }
			}

			void IBindingList.RemoveIndex(PropertyDescriptor property)
			{ }

			void IBindingList.RemoveSort()
			{ }

			ListSortDirection IBindingList.SortDirection
			{ get { return ListSortDirection.Ascending; } }

			PropertyDescriptor IBindingList.SortProperty
			{ get { return null; } }

			bool IBindingList.SupportsChangeNotification
			{ get { return false; } }

			bool IBindingList.SupportsSearching
			{ get { return false; } }

			bool IBindingList.SupportsSorting
			{ get { return false; } }

			#endregion

			#region IList Members

			int IList.Add(object value)
			{ throw new NotSupportedException(); }

			void IList.Clear()
			{ throw new NotSupportedException(); }

			bool IList.Contains(object value)
			{ return false; }

			int IList.IndexOf(object value)
			{ return -1; }

			void IList.Insert(int index, object value)
			{ throw new NotSupportedException(); }

			bool IList.IsFixedSize
			{ get { return true; } }

			bool IList.IsReadOnly
			{ get { return true; } }

			void IList.Remove(object value)
			{ throw new NotSupportedException(); }

			void IList.RemoveAt(int index)
			{ throw new NotSupportedException(); }

			object IList.this[int index]
			{
				get { return new FakeElement(context, elementType); }
				set { throw new NotSupportedException(); }
			}

			#endregion

			#region ICollection Members

			void ICollection.CopyTo(Array array, int index)
			{ throw new NotSupportedException(); }

			int ICollection.Count
			{ get { return 1; } }

			bool ICollection.IsSynchronized
			{ get { return false; } }

			object ICollection.SyncRoot
			{ get { return this; } }

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{ yield return new FakeElement(context, elementType); }

			#endregion

			readonly MyTypeDescriptorContext context;
			readonly Type elementType;
		}

		#endregion

		#region FakeElement class

		class FakeElement : ICustomTypeDescriptor, ITypedList
		{
			public FakeElement(MyTypeDescriptorContext context, Type elementType)
			{
				this.context = context;
				this.elementType = elementType;
			}

			PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
			{ return (this as ICustomTypeDescriptor).GetProperties(null); }

			PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
			{ return GetTypedListItemProperties(context, elementType, null); }

			#region ICustomTypeDescriptor Null-Action Members

			AttributeCollection ICustomTypeDescriptor.GetAttributes()
			{ return new AttributeCollection(); }

			string ICustomTypeDescriptor.GetClassName()
			{ return ""; }

			string ICustomTypeDescriptor.GetComponentName()
			{ return ""; }

			TypeConverter ICustomTypeDescriptor.GetConverter()
			{ return null; }

			EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
			{ return null; }

			PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
			{ return null; }

			object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
			{ return null; }

			EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
			{ return (this as ICustomTypeDescriptor).GetEvents(null); }

			EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
			{ return new EventDescriptorCollection(Array.Empty<EventDescriptor>()); }

			object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
			{ return this; }

			#endregion

			#region ITypedList Members

			PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
			{ return GetTypedListItemProperties(context, elementType, listAccessors); }

			string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
			{ return ""; }

			#endregion

			readonly MyTypeDescriptorContext context;
			readonly Type elementType;
		}

		#endregion

		#region FakePropertyDescriptor class

		class FakePropertyDescriptor : KPropertyDescriptor
		{
			public FakePropertyDescriptor(string name)
				: base(null, name, Array.Empty<Attribute>())
			{
			}

			public FakePropertyDescriptor(MyTypeDescriptorContext context, PropertyDescriptor realProperty)
				: base(null, realProperty.Name, Array.Empty<Attribute>())
			{
				this.context = context;
				RealProperty = realProperty;
			}

			public readonly PropertyDescriptor RealProperty;

			public override Type PropertyType
			{ get { return typeof(IList); } }

			protected override object GetValueCore(object component)
			{
				object result = null;
				if (RealProperty != null)
				{
					Type newElementType = GetTypeOrListElementType(RealProperty.PropertyType);
					result = new FakeList(context, newElementType);
				}
				return result;
			}

			public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
			{
				PropertyDescriptorCollection result = base.GetChildProperties(instance, filter);
				return result;
			}

			readonly MyTypeDescriptorContext context;
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		const string RootNodeCaption = "(root)";

		static Type GetTypeOrListElementType(Type type)
		{
			Type result = null;
			if (typeof(IList).IsAssignableFrom(type))
			{
				result = ListUtil.GetListElementType(type);
			}
			if (result == null)
			{
				result = type;
			}
			return result;
		}

		static PropertyDescriptorCollection GetTypedListItemProperties(MyTypeDescriptorContext context, Type topLevelDataSourceType, PropertyDescriptor[] listAccessors)
		{ return GetPropertiesInTreeViewHierarchy(context, topLevelDataSourceType, listAccessors); }

		static PropertyDescriptorCollection GetPropertiesInTreeViewHierarchy(MyTypeDescriptorContext context, Type topLevelDataSourceType, PropertyDescriptor[] currentPath)
		{
			PropertyDescriptorCollection result = null;
			if (currentPath == null || currentPath.Length == 0)
			{
				result = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());
				result.Add(new FakePropertyDescriptor(RootNodeCaption));
			}
			else if (currentPath.Length == 1)
			{
				result = new PropertyDescriptorCollection(null);
				foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(topLevelDataSourceType))
				{
					result.Add(property);
				}
				result = result.Sort();
			}
			else
			{
				PropertyDescriptor next = ((FakePropertyDescriptor)currentPath[currentPath.Length - 1]).RealProperty;
				if (IsPrimitiveOrValueType(next.PropertyType))
				{
					result = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());
				}
				else if (typeof(IList).IsAssignableFrom(next.PropertyType))
				{
					Type elementType = ListUtil.GetListElementType(next.PropertyType);
					if (elementType != null)
					{
						result = TypeDescriptor.GetProperties(elementType).Sort();
					}
				}
				if (result == null)
				{
					result = TypeDescriptor.GetProperties(next.PropertyType).Sort();
				}
			}
			result = GetBrowsableOnlyFakeProperties(context, result);
			return result;
		}

		static PropertyDescriptorCollection GetBrowsableOnlyFakeProperties(MyTypeDescriptorContext context, PropertyDescriptorCollection properties)
		{
			Type bindingMemberTypeFilter = context.BindingMemberTypeFilter;
			PropertyDescriptorCollection result = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());
			foreach (PropertyDescriptor property in properties)
			{
				if (property.Name == RootNodeCaption || IsPropertyBrowsable(bindingMemberTypeFilter, context.AllowListProperties, property))
				{
					result.Add(new FakePropertyDescriptor(context, property));
				}
			}
			return result;
		}

		static bool IsPropertyBrowsable(Type typeFilter, bool allowListProperties, PropertyDescriptor property)
		{
			TypeConverter typeFilterTypeConverter = typeFilter == null ? null : TypeDescriptor.GetConverter(typeFilter);
			TypeConverter propertyTypeConverter = property.Converter;
			bool result = false;
			if (property.IsBrowsable)
			{
				result = result || (typeFilter == null);
				result = result || (typeFilter == property.PropertyType);
				result = result || typeFilter.IsAssignableFrom(property.PropertyType);
				result = result || (propertyTypeConverter.GetType().Assembly != typeof(TypeConverter).Assembly && propertyTypeConverter != null && propertyTypeConverter.CanConvertTo(typeFilter));
				result = result || (typeFilterTypeConverter.GetType().Assembly != typeof(TypeConverter).Assembly && typeFilterTypeConverter != null && typeFilterTypeConverter.CanConvertFrom(property.PropertyType));
				result = result || (HasBrowsableAndNonPrimitiveChildProperty(property.PropertyType) && (allowListProperties || !typeof(IList).IsAssignableFrom(property.PropertyType)));
			}
			return result;
		}

		static bool HasBrowsableAndNonPrimitiveChildProperty(Type type)
		{
			bool? result = hasBrowsableAndNonPrimitiveChildProperties[type];
			if (result == null)
			{
				result = false;
				foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(type))
				{
					if (property.IsBrowsable && !IsPrimitiveOrValueType(type))
					{
						result = true;
						break;
					}
				}
				hasBrowsableAndNonPrimitiveChildProperties.Add(type, result);
			}
			return (bool)result;
		}
		[Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, bool?> hasBrowsableAndNonPrimitiveChildProperties = new LRUCache<Type, bool?>();

		static bool IsPrimitiveOrValueType(Type type)
		{ return type.IsValueType || type.IsPrimitive || type == typeof(string); }

		#endregion
	}
}
