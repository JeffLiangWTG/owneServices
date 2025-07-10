using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IActiveCustomPropertyContainer : ICustomPropertyContainer
	{
		event EventHandler PropertySetChanged;
		void AddCustomProperty(ICustomProperty property);
	}

	public static class CustomPropertyContainerExtensions
	{
		public static ICustomProperty FindPropertyByIdentifier(this ICustomPropertyContainer propertyContainer, string propertyIdentifier)
		{
			return propertyContainer.CustomProperties.FirstOrDefault(property => property.Identifier.Equals(propertyIdentifier, StringComparison.Ordinal) && !property.IsDeleted);
		}

		public static bool HasPropertyWithIdentifier(this ICustomPropertyContainer propertyContainer, string propertyIdentifier)
		{
			return propertyContainer.FindPropertyByIdentifier(propertyIdentifier) != null;
		}

		public static object GetValue(this ICustomPropertyContainer propertyContainer, BusinessObject businessObject, string propertyIdentifier)
		{
			ICustomProperty property = propertyContainer.FindPropertyByIdentifier(propertyIdentifier);
			return property != null ? property.GetValue(businessObject) : null;
		}

		public static bool TrySetValue(this ICustomPropertyContainer propertyContainer, BusinessObject businessObject, string propertyIdentifier, object value)
		{
			ICustomProperty property = propertyContainer.FindPropertyByIdentifier(propertyIdentifier);

			if (property != null)
			{
				CustomBusinessObject customBusinessObject = propertyContainer as CustomBusinessObject;
				if (customBusinessObject != null)
				{
					customBusinessObject[propertyIdentifier] = value;
					return true;
				}
				else
				{
					var result = property.TrySetValue(businessObject, value);

					if (businessObject != null)
					{
						ZPropertyInfo propertyInfo = businessObject.ZPropertyInfoHash.GetPropertySafe(propertyIdentifier);
						if (propertyInfo != null)
						{
							propertyInfo.RefreshBinding();
						}
					}
					return result;
				}
			}

			return false;
		}

		public static bool IsReadonly(this ICustomPropertyContainer propertyContainer, string propertyIdentifier)
		{
			bool result = true;

			ICustomProperty property = propertyContainer.FindPropertyByIdentifier(propertyIdentifier);

			if (property != null)
			{
				bool isReadOnly = property.Info == null || property.Info.ReadOnly;

				if (!isReadOnly)
				{
					var customBusinessObject = propertyContainer as CustomBusinessObject;

					if (customBusinessObject != null)
					{
						var propertyInfo = customBusinessObject.ZPropertyInfoHash.GetPropertySafe(propertyIdentifier);

						if (propertyInfo != null)
						{
							isReadOnly = MetaData.GetReadOnly(customBusinessObject, propertyInfo.PropertyDescriptor);
						}
					}
				}

				result = isReadOnly;
			}

			return result;
		}
	}
}
