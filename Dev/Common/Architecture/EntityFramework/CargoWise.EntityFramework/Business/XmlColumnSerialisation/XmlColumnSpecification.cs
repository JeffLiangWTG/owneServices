using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	[Immutable]
	public class XmlColumnSpecification
	{
		public XmlColumnSpecification(SchemaColumn xmlColumn, string rootElementName = null)
			: this(rootElementName)
		{
			Argument.NotNull(xmlColumn, "xmlColumn");

			this.xmlColumn = xmlColumn;
		}

		public XmlColumnSpecification(PropertyInfo xmlColumnProperty, string rootElementName = null)
			: this(rootElementName)
		{
			Argument.NotNull(xmlColumnProperty, "xmlColumnProperty");

			if (xmlColumnProperty.PropertyType != typeof(ZString))
			{
				throw new ArgumentException("xmlColumnProperty must be of ZString PropertyType");
			}

			this.xmlColumnProperty = xmlColumnProperty;
		}

		XmlColumnSpecification(string rootElementName)
		{
			this.rootElementName = rootElementName;
		}

		public string XmlColumnName
		{
			get { return XmlColumn != null ? XmlColumn.Name : XmlColumnProperty.Name; }
		}

		public SchemaColumn XmlColumn => xmlColumn;
		readonly SchemaColumn xmlColumn;
		public PropertyInfo XmlColumnProperty => xmlColumnProperty;
		readonly PropertyInfo xmlColumnProperty;
		public string RootElementName => rootElementName;
		readonly string rootElementName;

		public override bool Equals(object obj)
		{
			var item = obj as XmlColumnSpecification;
			if (item != null)
			{
				return XmlColumnName.Equals(item.XmlColumnName) &&
					((XmlColumn != null && XmlColumn.Equals(item.XmlColumn)) || (XmlColumnProperty != null && XmlColumnProperty.Equals(item.XmlColumnProperty)));
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return XmlColumnName.GetHashCode();
		}
	}
}
