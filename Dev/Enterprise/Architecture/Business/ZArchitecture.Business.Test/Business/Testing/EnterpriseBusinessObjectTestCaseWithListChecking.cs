using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class EnterpriseBusinessObjectTestCaseWithListChecking<T> : EnterpriseBusinessObjectTestCase where T : EnterpriseBusinessObject
	{
		public void TestCheckListAttributesAndLookups()
		{
			var errorLog = new List<string>();

			var header = Factory.New<T>();
			Type type = header.GetType();

			PropertyInfo lookupsProperty = null;
			foreach (var propertyInfo in typeof(T).GetProperties())
			{
				if (propertyInfo.Name == "Lookups" && (lookupsProperty == null || lookupsProperty.DeclaringType.IsAssignableFrom(propertyInfo.DeclaringType)))
				{
					lookupsProperty = propertyInfo;
				}
			}

			AssertNotNull("Must have a 'Lookups' property.", lookupsProperty);

			object lookupsValue = lookupsProperty.GetValue(header, null);
			AssertNotNull("'Lookups' property must return a value.", lookupsValue);

			Type lookupsType = lookupsValue.GetType();

			foreach (var propertyInfo in type.GetProperties())
			{
				if (typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType))
				{
					var listAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(ListAttribute)) as ListAttribute;

					if (listAttribute != null)
					{
						string listName = listAttribute.ListDataSourceMember;
						if (listName.StartsWith("Lookups."))
						{
							CheckList(errorLog, propertyInfo, lookupsType, listName.Substring(8), listName);
						}
						else if (!listName.Contains("."))
						{
							CheckList(errorLog, propertyInfo, type, listName, listName);
						}
						else
						{
							errorLog.Add("Property [" + propertyInfo.Name + "] references List [" + listName + "] which should be moved to the class returned from the Lookups property.");
						}
					}
				}
			}

			AssertMultilineASCIIEquals("Any property with a ListAttribute applied must point towards a real list.", "", string.Join("\r\n", errorLog.ToArray()));
		}

		static void CheckList(List<string> result, PropertyInfo propertyInfo, Type typeContainingList, string listPropertyName, string listDataSourceMember)
		{
			var listProperty = typeContainingList.GetProperty(listPropertyName);
			if (listProperty == null)
			{
				result.Add("Property [" + propertyInfo.Name + "] references List [" + listDataSourceMember + "] which does not exist on class [" + typeContainingList.FullName + "].");
			}
		}
	}
}
