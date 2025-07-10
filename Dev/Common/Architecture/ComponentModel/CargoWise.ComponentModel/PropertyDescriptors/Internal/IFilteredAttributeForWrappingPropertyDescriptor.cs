using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Implemented on a PropertyDescriptor to allow attributes to be modified while the property
	/// is being wrapped. For example, MyMember("Member") might need to be changed to
	/// MyMember("Relation.Member") when the property is wrapped.
	/// </summary>
	public interface IFilteredAttributeForWrappingPropertyDescriptor
	{
		Attribute GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty);
	}
}
