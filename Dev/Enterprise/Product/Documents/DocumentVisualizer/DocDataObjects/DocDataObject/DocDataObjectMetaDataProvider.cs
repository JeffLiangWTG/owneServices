using System;
using System.Collections;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using MetaDataType = Enterprise.DocumentVisualizer.Core.MetaDataType;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed class DocDataObjectMetaDataProvider : IMetaDataProvider
	{
		public object GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
		{
			if (dynamicData == null)
			{
				return null;
			}

			object metaData = null;

			switch (metaDataType)
			{
				case MetaDataType.Identifier:
					switch (dynamicData.Value)
					{
						case IDocDataObject docDataObject:
							metaData = docDataObject.Identifier;
							break;

						case IBusiness bizObj:
							metaData = bizObj.Identifier;
							break;
					}
					break;

				case MetaDataType.NaturalKey:
					if (dynamicData is IDynamicDataCollection collection)
					{
						metaData = collection.GetNaturalKey();
					}
					break;

				case MetaDataType.ListDataSource:
					metaData = new Func<IList>(dynamicData.GetList);
					break;

				case MetaDataType.MaxLength:
					metaData = dynamicData.GetMaxLength();
					break;

				case MetaDataType.IsReadOnly:
					if (dynamicData is DocDataObjectDynamicData docDataObjectDynamicData)
					{
						var propertyInfo = docDataObjectDynamicData.ZPropertyInfo;

						metaData = propertyInfo != null
							&& (propertyInfo.ReadOnly || !propertyInfo.HasSetter);
					}
					break;

				case MetaDataType.IsNonOverridable:
				case MetaDataType.SuppressHasChanges:
					metaData = dynamicData.GetIgnoreChanges();
					break;

				case MetaDataType.BindTo:
					var bindTo = (BindToAttribute)dynamicData.Type.GetCustomAttribute(typeof(BindToAttribute));
					if (bindTo != null)
					{
						metaData = bindTo.PropertyName;
					}
					break;

				case MetaDataType.DateTimeFormat:
					var dateTimeFormat = dynamicData.GetAttribute<DateTimeFormatAttribute>();
					if (dateTimeFormat != null)
					{
						metaData = dateTimeFormat.DateTimeFormat;
					}
					break;
			}

			return metaData;
		}
	}
}
