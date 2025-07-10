using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Common;
using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy
{
	public class PropertyCopyTemplateNode : CopyTemplateNode
	{
		public PropertyCopyTemplateNode()
		{
		}

		public PropertyCopyTemplateNode(PropertyInfo propertyInfo)
			: this(propertyInfo, null)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo)); // Suggested By ReviewBot 
		}

		public PropertyCopyTemplateNode(PropertyInfo propertyInfo, ICopyTreeConfiguration copyTreeConfiguration)
			: base(propertyInfo.Name)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo)); // Suggested By ReviewBot 
			Type propertyType = propertyInfo.PropertyType;
			while (propertyType.IsGenericType && propertyType.IsValueType && propertyType.GetGenericArguments().Length == 1)
			{
				propertyType = propertyType.GetGenericArguments()[0];
			}
			if (copyTreeConfiguration != null)
			{
				propertyType = copyTreeConfiguration.GetPropertyTypeSubstitute(propertyType) ?? propertyType;
			}

			PropertyType = propertyType.Name;

			var defaultValueAttribute = (DefaultValueAttribute)propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault();
			if (defaultValueAttribute != null)
			{
				DefaultValue = defaultValueAttribute.Value;
			}

			var maxLengthAttribute = (MaxLengthAttribute)propertyInfo.GetCustomAttributes(typeof(MaxLengthAttribute), false).FirstOrDefault();
			if (maxLengthAttribute != null && maxLengthAttribute.MaxLength == 1 && propertyInfo.PropertyType == typeof(string))
			{
				PropertyType = nameof(Boolean);
			}
		}

		[XmlIgnore]
		public string CustomCopyTemplateNode { get; set; }

		[XmlIgnore]
		public string PropertyType { get; set; }

		[XmlIgnore]
		public object DefaultValue { get; set; }

		[XmlAttribute(AttributeName = "Do")]
		public CopyMethod CopyMethod { get; set; }

		public object Value { get; set; }

		internal override void ResetAndUpdateId()
		{
			base.ResetAndUpdateId();
			CopyMethod = default(CopyMethod);
			Value = null;
		}

		public override bool HasData()
		{
			return CopyMethod != CopyMethod.None;
		}

		internal override void CopyTransientData(CopyTemplateNode sourceNode)
		{
			base.CopyTransientData(sourceNode);

			var sourcePropertyNode = sourceNode as PropertyCopyTemplateNode;
			if (sourcePropertyNode != null)
			{
				DefaultValue = sourcePropertyNode.DefaultValue;
				PropertyType = sourcePropertyNode.PropertyType;
				CustomCopyTemplateNode = sourcePropertyNode.CustomCopyTemplateNode;
			}
		}
	}
}
