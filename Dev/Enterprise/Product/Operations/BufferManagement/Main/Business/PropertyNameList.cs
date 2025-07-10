using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class PropertyNameList : CodeDescriptionPairList
	{
		public PropertyNameList(Type propertySourceType)
		{
			if (propertySourceType != null)
			{
				var properties = propertySourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
				var groupItems = properties
					.Where(property => !Attribute.IsDefined(property, typeof(CustomisedControlExcludeAttribute)) && IsAllowedPropertyType(property.PropertyType))
					.Select(property => new { property.Name, DataBoundResourceStrings.GetDataForProperty(property)?.Caption })
					.Where(property => !string.IsNullOrEmpty(property.Caption))
					.OrderBy(property => property.Caption)
					.GroupBy(property => property.Caption, StringComparer.OrdinalIgnoreCase);

				foreach (var group in groupItems)
				{
					if (group.Count() == 1)
					{
						var item = group.Single();
						AddPair(item.Name, item.Caption);
					}
					else
					{
						var items = group.Select(item => new
						{
							item.Name,
							Caption = item.Caption + FormattableString.Invariant($" ({item.Name})") // Include property Name to avoid duplicate descriptions
						}).OrderBy(item => item.Caption);

						foreach (var item in items)
						{
							AddPair(item.Name, item.Caption);
						}
					}
				}
			}
		}

		static bool IsAllowedPropertyType(Type propertyType)
		{
			return typeof(IZType).IsAssignableFrom(propertyType) && propertyType != typeof(ZGuid);
		}
	}
}
