using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Design;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	internal static class FinalTypeRetriever
	{
		public static Type Retrieve(Type dataSourceType, string pathToProperty)
		{
			DivideStringOnFirstDot(pathToProperty, out var currentDataMember, out var restOfPath);

			if (TypeUtilities.IsSubclassOfBusinessObjectCollection(dataSourceType))
			{
				try
				{
					var elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(dataSourceType);
					return Retrieve(elementType, pathToProperty);
				}
				catch
				{
					throw new Exception(BusinessObjectCollection.IndexerErrorExceptionMessage);
				}
			}
			else if (string.IsNullOrEmpty(restOfPath))
			{
				var property = ZCustomTypeDescriptor.GetProperties(dataSourceType)[currentDataMember];

				if (property != null)
				{
					if (TypeUtilities.IsSubclassOfBusinessObjectCollection(property.PropertyType))
					{
						try
						{
							return BusinessObjectCollection.GetElementTypeFromCollectionType(property.PropertyType);
						}
						catch (Exception e)
						{
							throw new Exception(BusinessObjectCollection.IndexerErrorExceptionMessage, e);
						}
					}
				}
				else if (string.IsNullOrEmpty(currentDataMember))
				{
					return dataSourceType;
				}
				else
				{
					return null;
				}
			}
			else if (dataSourceType.IsInterface || TypeUtilities.IsSubclassOfBusinessObject(dataSourceType))
			{
				var property = PropertyDescriptorCollectionWithWrappingProperties.FromType(dataSourceType)[currentDataMember];
				if (property != null)
				{
					return Retrieve(property.PropertyType, restOfPath);
				}
			}

			return null;
		}

		static void DivideStringOnFirstDot(string input, out string head, out string tail)
		{
			var split = input.Split(new[] { '.' }, 2);
			head = split[0];
			tail = split.Length > 1 ? split[1] : string.Empty;
		}
	}
}
