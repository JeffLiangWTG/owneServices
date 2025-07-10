using System;
using System.Reflection;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	interface IXmlColumnAccessStrategy
	{
		string Name { get; }
		object DefaultValue { get; }
		Type ColumnType { get; }
		XmlColumnPropertyAttribute ColumnAttribute { get; }

		object GetValue(BusinessObject businessObject);
		void SetValue(BusinessObject businessObject, object value);
	}

	static class IXmlColumnAccessStrategyExtensions
	{
		internal static IZType GetDefaultValueAsZType(this IXmlColumnAccessStrategy strategy)
		{
			return strategy.DefaultValue == null ? null : ZDataType.ObjectToZType(strategy.DefaultValue);
		}
	}

	class XmlPropertyStrategy : IXmlColumnAccessStrategy
	{
		internal XmlPropertyStrategy(PropertyInfo info, XmlColumnPropertyAttribute attribute)
		{
			this.info = info;
			this.ColumnAttribute = attribute;
		}

		readonly PropertyInfo info;

		#region IXmlColumnAccessStrategy Members

		public object GetValue(BusinessObject businessObject)
		{
			return info.GetValue(businessObject, null);
		}

		public void SetValue(BusinessObject businessObject, object value)
		{
			var iztypeValue = value as IZType;
			if (iztypeValue != null)
			{
				businessObject.XmlColumnDictionary.SetValue(iztypeValue, Name);
			}
			else if (info.SetMethod != null)
			{
				info.SetValue(businessObject, value, null);
			}
		}

		public Type ColumnType
		{
			get { return info.PropertyType; }
		}

		public string Name
		{
			get { return info.Name; }
		}

		public object DefaultValue
		{
			get { return ColumnAttribute.DefaultValue; }
		}

		public XmlColumnPropertyAttribute ColumnAttribute { get; }

		#endregion
	}
}
