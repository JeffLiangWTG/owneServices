using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Decorate properties with this attribute to have their value stored on the specified XML column
	/// (or a single column if no XmlColumnName is specified).
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class XmlColumnPropertyAttribute : Attribute
	{
		public XmlColumnPropertyAttribute()
			: base()
		{
		}

		public XmlColumnPropertyAttribute(string xmlColumnName)
		{
			XmlColumnName = xmlColumnName;
		}

		/// <summary>
		/// Specifies the name of the XML column on the business object which this property will be serialised against.
		/// </summary>
		public string XmlColumnName { get; private set; }

		/// <summary>
		/// Specifies the value which will be used if no value is found when de-serialising this property.
		/// </summary>
		public object DefaultValue { get; set; }

		/// <summary>
		/// Specifies whether 'empty' values are serialised, such as empty strings, ZBool.False.
		/// </summary>
		public bool SerialiseDefaultValues { get; set; }
	}
}
