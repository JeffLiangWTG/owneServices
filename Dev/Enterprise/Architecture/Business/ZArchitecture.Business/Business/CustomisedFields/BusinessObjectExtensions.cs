using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public static class BusinessObjectExtensions
	{
		#region Operating on Possibly Custom Properties

		public static bool HasPossiblyCustomProperty(this BusinessObject bizObj, string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return false;
			}
			if (bizObj.ZPropertyInfoHash.ContainsKey(identifier))
			{
				return true;
			}
			var customFields = CustomBusinessObjectExtensions.GetCustomBusinessObject(bizObj, true);
			DynamicBusinessObjectProperty customProperty = customFields != null ? ((IDynamicBusinessObject)customFields).GetProperty(identifier) : null;
			return customProperty != null;
		}

		public static object GetPossiblyCustomProperty(this BusinessObject bizObj, string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return null;
			}
			if (bizObj.ZPropertyInfoHash.ContainsKey(identifier))
			{
				return bizObj[identifier];
			}
			var customFields = CustomBusinessObjectExtensions.GetCustomBusinessObject(bizObj, true);
			DynamicBusinessObjectProperty customProperty = customFields != null ? ((IDynamicBusinessObject)customFields).GetProperty(identifier) : null;
			if (customProperty != null)
			{
				return customFields[identifier];
			}
			return null;
		}

		public static void SetPossiblyCustomProperty(this BusinessObject bizObj, string identifier, object value)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return;
			}
			if (bizObj.ZPropertyInfoHash.ContainsKey(identifier))
			{
				bizObj[identifier] = value;
			}
			var customFields = CustomBusinessObjectExtensions.GetCustomBusinessObject(bizObj, true);
			DynamicBusinessObjectProperty customProperty = customFields != null ? ((IDynamicBusinessObject)customFields).GetProperty(identifier) : null;
			if (customProperty != null)
			{
				customFields[identifier] = value;
			}
		}

		public static int GetPossiblyCustomPropertyMaxLength(this BusinessObject bizObj, string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return -1;
			}
			if (bizObj.ZPropertyInfoHash.ContainsKey(identifier))
			{
				return bizObj.ZPropertyInfoHash[identifier].MaxLength;
			}
			var customFields = CustomBusinessObjectExtensions.GetCustomBusinessObject(bizObj, true);
			DynamicBusinessObjectProperty customProperty = customFields != null ? ((IDynamicBusinessObject)customFields).GetProperty(identifier) : null;
			if (customProperty != null)
			{
				DynamicMetaData maxLengthMetaData = customProperty.GetMetaData(MetaDataTypes.MaxLength);

				if (maxLengthMetaData != null)
				{
					return (int)maxLengthMetaData.Value;
				}
			}
			return -1;
		}

		public static bool GetPossiblyCustomPropertyReadOnly(this BusinessObject bizObj, string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return false;
			}
			if (bizObj.ZPropertyInfoHash.ContainsKey(identifier))
			{
				return bizObj.ZPropertyInfoHash[identifier].ReadOnly;
			}
			var customFields = CustomBusinessObjectExtensions.GetCustomBusinessObject(bizObj, true);
			DynamicBusinessObjectProperty customProperty = customFields != null ? ((IDynamicBusinessObject)customFields).GetProperty(identifier) : null;
			if (customProperty != null)
			{
				return customProperty.ReadOnly;
			}
			return true;
		}

		#endregion
	}

	public static class CustomBusinessObjectExtensions
	{
		/// <remarks>Modifier 'this' for BusinessObject is used to please ReflectionTest.TestStaticMethodsAreLocatedOnCorrectClass()</remarks>
		public static CustomBusinessObject GetCustomBusinessObject(this BusinessObject businessObject, bool createWhenAbsent = false)
		{
			if (businessObject == null)
			{
				return null;
			}

			if (businessObject is CustomBusinessObject customBusinessObject)
			{
				return customBusinessObject;
			}

			customBusinessObject = ((IBusiness)businessObject).Children.OfType<CustomBusinessObject>().FirstOrDefault();

			if (customBusinessObject != null)
			{
				return customBusinessObject;
			}

			if (createWhenAbsent)
			{
				if (businessObject is ICustomFieldProvider customFieldProvider)
				{
					customBusinessObject = customFieldProvider.GetCustomBusinessObject();
					businessObject.RegisterEditableChildObject(customBusinessObject);
					return customBusinessObject;
				}
			}

			return null;
		}

		public static void UnRegisterCustomBusinessObject(this BusinessObject businessObject)
		{
			if (businessObject != null)
			{
				var customBusinessObject = businessObject.GetCustomBusinessObject();
				if (customBusinessObject != null)
				{
					businessObject.UnRegisterEditableChildObject(customBusinessObject);
				}
			}
		}
	}
}
