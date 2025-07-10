using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class CodePropertyAttribute : ZPropertyAttribute
	{
		public CodePropertyAttribute(string codePropertyName)
			: this(codePropertyName, string.Empty)
		{
		}

		public CodePropertyAttribute(string codePropertyName, string getQueryMethod)
			: base(codePropertyName)
		{
			QueryMethod = getQueryMethod;
		}

		public readonly string QueryMethod;

		public static MethodInfo QueryMethodFromType(Type type)
		{
			var methodName = type.GetCustomAttribute<CodePropertyAttribute>()?.QueryMethod;
			if (!string.IsNullOrEmpty(methodName))
			{
				var queryMethod = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				return queryMethod;
			}
			return null;
		}

		public static string CodePropertyNameFromType(Type type)
		{
			return PropertyNameFromType(type, typeof(CodePropertyAttribute));
		}

		public static ZString CodeFromBusinessObject(BusinessObject bizO)
		{
			string propertyName = CodePropertyNameFromType(bizO.GetType());
			object result = bizO[propertyName];
			if (result is ZString)
			{
				return (ZString)result;
			}
			else if (result is IMultilingualString)
			{
				return new ZString(result.ToString());
			}
			else
			{
				throw new Exception("You must set your CodePropertyAttribute to a ZString field on " + bizO.GetType().FullName);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class DescriptionPropertyAttribute : ZPropertyAttribute
	{
		public DescriptionPropertyAttribute(string descriptionPropertyName)
			: this(descriptionPropertyName, string.Empty)
		{
		}

		public DescriptionPropertyAttribute(string codePropertyName, string getQueryMethod)
		: base(codePropertyName)
		{
			QueryMethod = getQueryMethod;
		}

		public readonly string QueryMethod;

		public static MethodInfo QueryMethodFromType(Type type)
		{
			var methodName = type.GetCustomAttribute<DescriptionPropertyAttribute>()?.QueryMethod;
			if (!string.IsNullOrEmpty(methodName))
			{
				var queryMethod = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				return queryMethod;
			}
			return null;
		}

		public bool CanBeReferencedBy { get; set; }

		public static bool CanBeReferencedByDescription(Type type)
		{
			DescriptionPropertyAttribute[] attributes = (DescriptionPropertyAttribute[])type.GetCustomAttributes(typeof(DescriptionPropertyAttribute), true);
			return attributes.Length > 0 && attributes[0].CanBeReferencedBy;
		}

		public static string DescriptionPropertyNameFromType(Type type)
		{
			var property = PropertyNameFromType(type, typeof(DescriptionPropertyAttribute));
			var multilingualProperty = type.GetProperty(property + "Multilingual");
			if (multilingualProperty != null)
			{
				property = multilingualProperty.Name;
			}
			return property;
		}

		public static ZString DescriptionFromBusinessObject(BusinessObject bizO)
		{
			return PropertyFromBusinessObject(bizO, DescriptionPropertyNameFromType(bizO.GetType()));
		}

		public static string DescriptionPropertyNameFromTypeWithNoMultilingual(Type type) => PropertyNameFromType(type, typeof(DescriptionPropertyAttribute));

		static ZString PropertyFromBusinessObject(BusinessObject bizO, string propertyName)
		{
			object result = bizO[propertyName];
			if (result is ZString)
			{
				return (ZString)result;
			}
			else if (result is IMultilingualString)
			{
				return new ZString(result.ToString());
			}
			else
			{
				throw new Exception("You must set your DescriptionPropertyAttribute to a ZString field on " + bizO.GetType().FullName);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class IsActivePropertyAttribute : ZPropertyAttribute
	{
		public IsActivePropertyAttribute(string isActivePropertyName)
			: base(isActivePropertyName)
		{
		}

		public static string IsActivePropertyName(Type type)
		{
			return PropertyNameFromType(type, typeof(IsActivePropertyAttribute));
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class IsCancelledPropertyAttribute : ZPropertyAttribute
	{
		public IsCancelledPropertyAttribute(string isCancelledPropertyName)
			: base(isCancelledPropertyName)
		{
		}

		public static string IsCancelledPropertyName(Type type)
		{
			return PropertyNameFromType(type, typeof(IsCancelledPropertyAttribute));
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class PreventDeleteAttribute : Attribute
	{
		public PreventDeleteAttribute(bool preventDelete)
		{
			PreventDelete = preventDelete;
		}

		readonly bool PreventDelete;

		public static bool IsTrue(Type type)
		{
			bool result = false;
			Attribute[] attributes = (Attribute[])type.GetCustomAttributes(typeof(PreventDeleteAttribute), true);
			if (attributes.Length == 1)
			{
				result = (attributes[0] as PreventDeleteAttribute).PreventDelete;
			}
			else if (attributes.Length > 1)
			{
				ZPropertyAttribute.ReportMultipleAttributesError(type, typeof(PreventDeleteAttribute));
			}
			return result;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class ShouldDisplayInUtcTimeForEditAndCreateLogFieldsAttribute : Attribute
	{
		public static bool IsApplied(Type type)
		{
			var attributes = type.GetCustomAttributes(typeof(ShouldDisplayInUtcTimeForEditAndCreateLogFieldsAttribute), true);

			if (attributes.Length > 1)
			{
				ZPropertyAttribute.ReportMultipleAttributesError(type, typeof(ShouldDisplayInUtcTimeForEditAndCreateLogFieldsAttribute));
			}

			return attributes.Length == 1;
		}
	}

	public abstract class ZPropertyAttribute : Attribute
	{
		protected ZPropertyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public readonly string PropertyName;

		internal static string PropertyNameFromType(Type type, Type attribute)
		{
			var cache = LazyInitializer.EnsureInitialized(ref customAttributeCache);
			var key = (type, attribute);
			if (!cache.TryGetValue(key, out var result))
			{
				Attribute[] attributes = (Attribute[])type.GetCustomAttributes(attribute, true);

				if (attributes.Length == 0)
				{
					throw new NoCodePropertyException(type);
				}
				else
				{
					result = ((ZPropertyAttribute)attributes[0]).PropertyName;
					cache.Add(key, result);

					if (attributes.Length > 1)
					{
						ReportMultipleAttributesError(type, attribute);
					}
				}
			}

			return result;
		}

		[ThreadStatic]
		static Dictionary<(Type bizoType, Type attrType), string> customAttributeCache;

		internal static void ReportMultipleAttributesError(Type type, Type attribute)
		{
			ErrorReporter.ReportOnce("MultipleAttributes" + type.FullName, type.FullName + " implements multiple " + attribute.FullName + " attributes - Remove all but the base implementation");
		}

		public static PropertyInfo GetProperty(Type type, Type attributeType)
		{
			var attribute = type.GetCustomAttributes(attributeType, inherit: true).OfType<ZPropertyAttribute>().SingleOrDefault();
			return attribute != null ? type.GetProperty(attribute.PropertyName) : null;
		}
	}

	[Serializable]
	public class NoCodePropertyException : Exception
	{
		public NoCodePropertyException(Type typeOfElements)
			: base("You should implement [CodeProperty(BizObj.Schema.XX_CodePropertyName), DescriptionProperty(BizObj.Schema.XX_DescriptionPropertyName)] on " + typeOfElements.FullName + " before using it in a FindBox.")
		{
		}

#if NETFRAMEWORK
		protected NoCodePropertyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
