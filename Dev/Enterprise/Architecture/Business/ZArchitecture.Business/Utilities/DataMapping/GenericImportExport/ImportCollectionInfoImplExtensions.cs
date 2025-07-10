using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public static class ImportCollectionInfoImplExtensionsExtensions
	{
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static void AddFlattenedProperty<T>(this ImportCollectionInfoImpl importPropertyCollection, string propertyName, Type parentType, string prefixToRemove = "", string captionPrefix = "")
		{
			var importProperty = new ImportPropertyInfoImpl<T>(propertyName);
			importProperty.HeaderText = string.IsNullOrEmpty(captionPrefix) ? string.Empty : captionPrefix + " - ";

			var propertyNameWithoutPrefix = string.IsNullOrEmpty(prefixToRemove) ? propertyName : propertyName.Replace(prefixToRemove, string.Empty);
			var resData = DataBoundResourceStrings.GetDataForProperty(parentType, propertyNameWithoutPrefix);
			importProperty.HeaderText += resData != null ? resData.Caption : propertyNameWithoutPrefix;

			importPropertyCollection.Add(importProperty);
		}
	}
}
