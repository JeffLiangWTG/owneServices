using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using MetaDataType = Enterprise.DocumentVisualizer.Core.MetaDataType;

namespace Enterprise.DocumentVisualizer.Business
{
	static class DynamicMetaDataExtensions
	{
		public static DynamicMetaData[] GetMetaData(this IDynamicData dynamicData)
		{
			if (dynamicData == null)
			{
				return System.Array.Empty<DynamicMetaData>();
			}

			var metaData = new List<DynamicMetaData>();

			var list = dynamicData.GetMetaData<IList>(MetaDataType.ListDataSource);

			if (list != null)
			{
				metaData.Add(DynamicMetaData.ListDataSource(list));
			}

			var maxLength = dynamicData.GetMetaData<int?>(MetaDataType.MaxLength);

			if (maxLength.HasValue)
			{
				metaData.Add(DynamicMetaData.MaxLength(maxLength.Value));
			}

			var decimalPlaces = dynamicData.GetMetaData<int?>(MetaDataType.DecimalPlaces);

			if (decimalPlaces.HasValue)
			{
				metaData.Add(DynamicMetaData.DecimalPlaces(decimalPlaces.Value));
			}

			var dateTimeFormat = dynamicData.GetMetaData<KDateTimeFormat?>(MetaDataType.DateTimeFormat);

			if (dateTimeFormat.HasValue)
			{
				metaData.Add(DynamicMetaData.DateTimeFormat(dateTimeFormat.Value));
			}

			var readOnly = dynamicData.GetMetaData<bool>(MetaDataType.IsReadOnly);

			if (readOnly)
			{
				metaData.Add(DynamicMetaData.ReadOnly(true));
			}

			var propertyInfo = dynamicData.GetMetaData<PropertyInfo>(MetaDataType.DeclaringProperty);

			if (propertyInfo?.GetCustomAttribute(typeof(CustomFindBoxPopupAttribute)) is CustomFindBoxPopupAttribute customFindBoxAttribute)
			{
				metaData.Add(new CustomFindBoxMetaData(customFindBoxAttribute.GetInstance, customFindBoxAttribute.ShowDescription));
			}

			if (propertyInfo?.GetCustomAttribute(typeof(DisableModifiableMemberAttribute)) is DisableModifiableMemberAttribute disableModifiableMemberAttribute && dynamicData.GetDisableModifiable(disableModifiableMemberAttribute))
			{
				metaData.Add(DynamicMetaData.DisableModifiable(true));
			}

			return metaData.ToArray();
		}
	}
}
