using System;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.DataMapping
{
	public static class ImportPropertyInfoExtensions
	{
		public static IFindBoxListProvider GetFindBoxListProvider(this IImportPropertyInfo property, BusinessObject bizObj)
		{
			if (property != null)
			{
				if (bizObj is ImportWizardPreviewLine)
				{
					DynamicBusinessObjectProperty dynamicProperty = ((IDynamicBusinessObject)bizObj).GetProperty(property.MappingName);
					if (dynamicProperty != null)
					{
						DynamicMetaData metaData = dynamicProperty.GetMetaData(MetaDataTypes.ListDataSource);
						if (metaData != null)
						{
							return metaData.Value as IFindBoxListProvider;
						}
					}
				}
				else
				{
					return property.GetBindToList(bizObj) as IFindBoxListProvider;
				}
			}

			return null;
		}

		public static bool IsType(this IImportPropertyInfo property, BusinessObject bizObj, Type parentType)
		{
			Type type;
			if (property != null)
			{
				if (bizObj != null && !(bizObj is ImportWizardPreviewLine) && property.IsMultiControl)
				{
					type = property.GetExpectedTypeForMultiControl(bizObj);
				}
				else
				{
					type = property.PropertyType;
				}
			}
			else
			{
				type = typeof(ZString);
			}

			return ImportWizard.IsType(type, parentType);
		}
	}
}
