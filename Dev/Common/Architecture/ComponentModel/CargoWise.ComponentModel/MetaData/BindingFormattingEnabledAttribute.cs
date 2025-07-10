using System;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Apply this attribute to a property on a control that is bound to a data source
	/// to specify that formatting is required on the data. For example, a ZString needs
	/// to be formatted to a String and vice-versa.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class BindingOptionsAttribute : Attribute
	{
		public static BindingOptionsAttribute Default
		{
			get
			{
				return new BindingOptionsAttribute();
			}
		}

		public bool FormattingEnabled
		{
			get { return formattingEnabled; }
			set { formattingEnabled = value; }
		}
		bool formattingEnabled;

		public bool UseTypeConverters
		{
			get { return useTypeConverters; }
			set { useTypeConverters = value; }
		}
		bool useTypeConverters;

		public bool UpdateDataSourceOnPropertyChange
		{
			get { return updateDataSourceOnPropertyChange; }
			set { updateDataSourceOnPropertyChange = value; }
		}
		bool updateDataSourceOnPropertyChange;
	}
}
