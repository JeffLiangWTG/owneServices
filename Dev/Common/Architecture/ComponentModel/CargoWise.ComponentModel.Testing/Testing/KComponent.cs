#if DEBUG
using System;
using System.ComponentModel;

namespace CargoWise.ComponentModel.Testing
{
	/// <summary>
	/// For testing only.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[ToolboxItem(false)]
	public class KComponent : Component, ICustomTypeDescriptor, ITypedList
	{
		readonly KPropertyDescriptorCollection properties;

		public KComponent()
		{ this.properties = NewPropertyDescriptorCollection(GetType()); }

		/// <summary>
		/// Get the factory that produces the component's PropertyDescriptorS.
		/// </summary>
		protected virtual KPropertyDescriptorCollection NewPropertyDescriptorCollection(Type type)
		{ return PropertyDescriptorCollectionWithWrappingProperties.FromType(type); }

		/// <summary>
		/// Get the PropertyDescriptorS on this object.
		/// </summary>
		protected KPropertyDescriptorCollection GetProperties()
		{ return properties; }

		#region ICustomTypeDescriptor

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{ return new AttributeCollection(null); }

		string ICustomTypeDescriptor.GetClassName()
		{ return null; }

		string ICustomTypeDescriptor.GetComponentName()
		{ return null; }

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{ return null; }

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{ return null; }

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{ return null; }

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{ return null; }

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{ return new EventDescriptorCollection(null); }

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{ return new EventDescriptorCollection(null); }

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{ return ((ICustomTypeDescriptor)this).GetProperties(null); }

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{ return (attributes == null) ? this.GetProperties() : null; }

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{ return this; }

		#endregion

		#region ITypedList

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			return TypedListHelper.GetItemProperties(GetType(), listAccessors, NewPropertyDescriptorCollection);
		}

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{ return GetType().FullName; }

		#endregion
	}
}
#endif
