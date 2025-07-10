using System;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ChildEditableAttribute : Attribute
	{
		public ChildEditableAttribute()
			: this(true)
		{
		}

		public ChildEditableAttribute(bool value)
		{
			this.Value = value;
		}

		public static bool GetValue(PropertyDescriptor property)
		{
			ChildEditableAttribute attr = null;
			foreach (ChildEditableAttribute next in property.GetAttributesAllowMultiple(typeof(ChildEditableAttribute)))
			{
				attr = next;
				break;
			}
			return attr != null && attr.Value;
		}

		public bool Value
		{
			get { return fValue; }
			set { fValue = value; }
		}
		public bool fValue;
	}
}
