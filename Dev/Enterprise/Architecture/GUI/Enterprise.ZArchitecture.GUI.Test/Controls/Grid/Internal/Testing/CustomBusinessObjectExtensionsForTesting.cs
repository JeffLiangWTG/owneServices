using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	static class CustomBusinessObjectExtensionsForTesting
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
	}
}
