using System;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	public abstract class CalculatedFromAttributeBase : Attribute, IFilteredAttributeForWrappingPropertyDescriptor
	{
		[CLSCompliant(false)]
		protected CalculatedFromAttributeBase(params string[] propertyNames)
		{
			Argument.NotNull(propertyNames, nameof(propertyNames));
			this.propertyNames = propertyNames;
		}

		protected CalculatedFromAttributeBase(string propertyName)
			: this(new string[] { propertyName })
		{
		}

		/// <summary>
		/// Get the names of the properties the property this attribute is applied to is calculated from.
		/// </summary>
		public string[] PropertyNames
		{
			get
			{
				return (string[])propertyNames.Clone();
			}
		}
		readonly string[] propertyNames;

		#region IFilteredAttributeForWrappingPropertyDescriptor Members

		Attribute IFilteredAttributeForWrappingPropertyDescriptor.GetAttributeOnOuterProperty(WrappingPropertyDescriptor wrappingProperty)
		{
			var result = new string[PropertyNames.Length];
			for (var i = 0; i < PropertyNames.Length; i++)
			{
				result[i] = wrappingProperty.Outer.Name + "." + PropertyNames[i];
			}
			return new CalculatedFromAttribute(result);
		}

		#endregion
	}
}
