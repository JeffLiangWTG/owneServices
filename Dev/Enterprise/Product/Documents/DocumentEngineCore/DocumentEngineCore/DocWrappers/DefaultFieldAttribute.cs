using System;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultFieldAttribute : Attribute
	{
		public DefaultFieldAttribute(string fieldName)
		{
			FieldName = fieldName;
		}
		readonly string FieldName;

		public static string GetDefaultFieldName(Type wrapperType)
		{
			DefaultFieldAttribute[] defaultFields = (DefaultFieldAttribute[])wrapperType.GetCustomAttributes(typeof(DefaultFieldAttribute), true);
			if (defaultFields.Length == 0)
			{
				return null;
			}
			else if (defaultFields.Length == 1)
			{
				return defaultFields[0].FieldName;
			}
			else
			{
				throw new InvalidOperationException(string.Format("You can only have one DefaultFieldAttribute applied against a DocumentWrapper. ({0})", wrapperType.FullName));
			}
		}

		public static string GetDefaultValue(DocumentWrapper wrapper)
		{
			Type wrapperType = wrapper.GetType();
			string fieldName = GetDefaultFieldName(wrapperType);
			if (fieldName == null)
			{
				return string.Format(DefaultFieldNotImplementedMessage, wrapper.HumanReadableName);
			}
			else
			{
				var propertyInfo = wrapperType.GetProperty(fieldName) ?? throw new MissingFieldException(string.Format("DefaultFieldAttribute is used incorrectly - Property [public <ToStringableObjectType> {0}] does not exist on {1}", fieldName, wrapperType.FullName));
				return propertyInfo.GetValue(wrapper, null).ToString();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string DefaultFieldNotImplementedMessage = "(No Default Field Value Available on {0})";
	}
}
