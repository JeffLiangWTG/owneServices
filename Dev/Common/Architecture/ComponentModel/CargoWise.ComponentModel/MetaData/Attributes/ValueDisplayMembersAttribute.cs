using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to the classes of the elements of lists when they are used on a
	/// ComboBox or ListBox. This removes the need to apply ListValueMember and ListDisplayMember
	/// on every property that uses a list to select the values.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ValueDisplayMembersAttribute : MetaDataBaseAttribute
	{
		/// <summary>
		/// Specify the value and display member properties.
		/// </summary>
		/// <param name="valueMember">The member property that provides the element's value.</param>
		/// <param name="displayMember">The member property that provides the element's display string.</param>
		public ValueDisplayMembersAttribute(string valueMember, string displayMember)
		{
			this.valueMember = valueMember;
			this.displayMember = displayMember;
		}

		/// <summary>
		/// Get the value member.
		/// </summary>
		public string ValueMember
		{
			get { return valueMember; }
		}
		readonly string valueMember;

		/// <summary>
		/// Get the display member;
		/// </summary>
		public string DisplayMember
		{
			get { return displayMember; }
		}
		readonly string displayMember;

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return false;
		}

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return
				metaDataTypeId == MetaDataTypes.ListDisplayMember ||
				metaDataTypeId == MetaDataTypes.ListValueMember;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			return "";
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			object result = null;
			if (metaDataTypeId == MetaDataTypes.ListDisplayMember)
			{
				result = DisplayMember;
			}
			else if (metaDataTypeId == MetaDataTypes.ListValueMember)
			{
				result = ValueMember;
			}
			return result;
		}

		protected override Attribute GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			return new ValueDisplayMembersAttribute(
				wrappingProperty.Outer.Name + "+" + ValueMember,
				wrappingProperty.Outer.Name + "+" + DisplayMember);
		}
	}
}
