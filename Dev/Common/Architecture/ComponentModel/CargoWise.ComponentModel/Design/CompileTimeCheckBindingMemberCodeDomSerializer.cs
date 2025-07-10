using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security;
using CargoWise.Common;
using CargoWise.Common.CodeDom;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel.Design
{
	[SecurityCritical]
	public class CompileTimeCheckBindingMemberCodeDomSerializer : KCodeDomSerializer
	{
		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Type name")]
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			CodeExpressionStatement result = null;
			if (value != null)
			{
				CodeExpression expression = null;
				bool isError, isEmpty;

				// using reflection like this may not be required now that the type != type problem is gone
				var dataSourceType = GetPropertyValueFromTypeDescriptor(value, "DataSourceType") as Type;
				var controlPropertyType = GetPropertyValueFromTypeDescriptor(value, "ControlPropertyType") as Type;
				var bindingMember = GetPropertyValueFromTypeDescriptor(value, "BindingMember") as string;
				SetPropertyValueFromTypeDescriptor(value, "IsError", out isError);
				SetPropertyValueFromTypeDescriptor(value, "IsEmpty", out isEmpty);

				if (isEmpty)
				{
					expression = null;
				}
				else if (isError)
				{
					var errorMessage = (string)GetPropertyValueFromTypeDescriptor(value, "ErrorMessage") ?? "";
					expression = new CodeTypeReferenceExpression(errorMessage.Replace(" ", "_"));
				}
				else if (dataSourceType == null)
				{
					expression = new CodeTypeReferenceExpression("You_must_set_DataSourceType_on_the_control_binder");
				}
				else
				{
					expression = GenerateCompileTimeCheckExpression(bindingMember, controlPropertyType, dataSourceType, value);
				}
				if (expression != null)
				{
					result = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(CompileTimeCheckBindingMember)), "Check", expression));
				}
			}
			return result;
		}

		void SetPropertyValueFromTypeDescriptor(object value, string propertyName, out bool result)
		{
			Argument.NotNull(value, nameof(value));
			Argument.NotNull(propertyName, nameof(propertyName));
			result = false;
			var propertyValue = GetPropertyValueFromTypeDescriptor(value, propertyName);
			if (propertyValue is bool)
			{
				result = (bool)propertyValue;
			}
		}

		object GetPropertyValueFromTypeDescriptor(object value, string propertyName)
		{
			Argument.NotNull(value, nameof(value));
			Argument.NotNull(propertyName, nameof(propertyName));
			object result = null;
			var property = TypeDescriptor.GetProperties(value)[propertyName];
			if (property != null)
			{
				result = property.GetValue(value);
			}
			return result;
		}

		#region Implementation

		CodeCastExpression GenerateCompileTimeCheckExpression(string bindingMember, Type controlPropertyType, Type dataSourceType, object compileTimeCheckBindingMember)
		{
			Argument.NotNull(dataSourceType, nameof(dataSourceType));
			Argument.NotNull(compileTimeCheckBindingMember, nameof(compileTimeCheckBindingMember));
			// convert the root type to a collection element type if it is a collection
			var rootType = GetNonCollectionDataSourceType(dataSourceType);

			var rootDataSourceCast = new CodeCastExpression(KCodeTypeReference.FromType(rootType), new CodePrimitiveExpression(null));
			CodeExpression propertyPathGetter = rootDataSourceCast;

			if (string.IsNullOrEmpty(bindingMember) || bindingMember.Trim().Length == 0)
			{
				propertyPathGetter = new CodePrimitiveExpression(null);
			}
			else if (bindingMember.Trim() != ".")
			{
				var propertyPath = bindingMember.Split(new char[] { '.' });
				var properties = GetProperties(compileTimeCheckBindingMember, rootType);
				propertyPathGetter = BuildPropertyPathCode(
					properties,
					rootDataSourceCast,
					propertyPath,
					CompileTimeCheckBindingMember.IsList(controlPropertyType),
					compileTimeCheckBindingMember);
			}

			var boundDataType = GetBoundDataType(dataSourceType, controlPropertyType, bindingMember, compileTimeCheckBindingMember);
			return new CodeCastExpression(KCodeTypeReference.FromType(boundDataType), propertyPathGetter);
		}

		static PropertyDescriptorCollection GetProperties(object compileTimeCheckBindingMember, Type type)
		{
			Argument.NotNull(compileTimeCheckBindingMember, nameof(compileTimeCheckBindingMember)); // Suggested By ReviewBot 
			return (PropertyDescriptorCollection)compileTimeCheckBindingMember.GetType().InvokeMember("GetProperties", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, compileTimeCheckBindingMember, new object[] { type }, CultureInfo.InvariantCulture);
		}

		static Type GetBoundDataType(Type dataSourceType, Type controlPropertyType, string bindingMember, object compileTimeCheckBindingMember)
		{
			Argument.NotNull(compileTimeCheckBindingMember, nameof(compileTimeCheckBindingMember)); // Suggested By ReviewBot 
			var result = controlPropertyType ?? typeof(object);
			var properties = (PropertyDescriptor[])compileTimeCheckBindingMember.GetType().InvokeMember("NavigateProperties", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, compileTimeCheckBindingMember, new object[] { dataSourceType, bindingMember }, CultureInfo.InvariantCulture);
			var rightMostProperty = (properties == null || properties.Length == 0) ? null : properties[properties.Length - 1];
			if (rightMostProperty != null && rightMostProperty.Converter != null && rightMostProperty.Converter.CanConvertFrom(controlPropertyType))
			{
				result = rightMostProperty.PropertyType ?? result;
			}
			return result;
		}

		static Type GetNonCollectionDataSourceType(Type dataSourceType)
		{
			Argument.NotNull(dataSourceType, nameof(dataSourceType));
			var result = dataSourceType;
			if (CompileTimeCheckBindingMember.IsList(result))
			{
				var elementType = ListUtil.GetListElementType(result);
				if (elementType != null)
				{
					result = elementType;
				}
			}
			return result;
		}

		CodeExpression BuildPropertyPathCode(PropertyDescriptorCollection properties, CodeExpression expr, string[] propertyPath, bool controlPropTypeIsCollection, object compileTimeCheckBindingMember)
		{
			Argument.NotNull(propertyPath, nameof(propertyPath)); // Suggested By ReviewBot 
			Argument.NotNull(compileTimeCheckBindingMember, nameof(compileTimeCheckBindingMember));
			if (propertyPath[0] != null)
			{
				var propertyGet = NewCodePropertyReferenceExpression(expr, propertyPath[0]);
				CodeExpression result = propertyGet;

				var property = properties == null ? null : properties[propertyPath[0]];
				var childProperties = (property == null || property.PropertyType == null) ? null : GetProperties(compileTimeCheckBindingMember, property.PropertyType);
				bool isRightmostPropertyAndWeExpectCollection = (propertyPath.Length == 1 && controlPropTypeIsCollection);
				if (property != null && !isRightmostPropertyAndWeExpectCollection)
				{
					if (property.PropertyType != null && CompileTimeCheckBindingMember.IsList(property.PropertyType))
					{
						var elementType = ListUtil.GetListElementType(property.PropertyType) ?? property.PropertyType;
						var listCast = new CodeCastExpression(typeof(IList), propertyGet);
						var listProperty = NewCodePropertyReferenceExpression(listCast, "SyncRoot");
						result = new CodeCastExpression(KCodeTypeReference.FromType(elementType), listProperty);
						childProperties = PropertyDescriptorCollectionWithWrappingProperties.FromType(elementType);
					}
				}

				var newPropertyPath = new string[propertyPath.Length - 1];
				if (newPropertyPath.Length > 0)
				{
					for (int i = 1; i < propertyPath.Length; i++)
					{
						newPropertyPath[i - 1] = propertyPath[i];
					}
					result = BuildPropertyPathCode(childProperties, result, newPropertyPath, controlPropTypeIsCollection, compileTimeCheckBindingMember);
				}
				return result;
			}
			return null;
		}

		CodePropertyReferenceExpression NewCodePropertyReferenceExpression(CodeExpression expr, string propertyName)
		{
			Argument.NotNull(propertyName, nameof(propertyName)); // Suggested By ReviewBot 
			int plusIndex = propertyName.LastIndexOf('+');
			if (plusIndex != -1)
			{
				string left = propertyName.Substring(0, plusIndex);
				string right = propertyName.Substring(plusIndex + 1);
				return NewCodePropertyReferenceExpression(NewCodePropertyReferenceExpression(expr, left), right);
			}
			else
			{
				return new CodePropertyReferenceExpression(expr, propertyName.Replace("+", "."));
			}
		}

		#endregion
	}
}
