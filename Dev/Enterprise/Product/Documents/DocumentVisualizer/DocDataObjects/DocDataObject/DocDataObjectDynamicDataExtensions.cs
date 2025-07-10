using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	static class DocDataObjectDynamicDataExtensions
	{
		public static object GetNaturalKey(this IDynamicDataCollection dynamicDataCollection)
		{
			var naturalKeyAttribute = dynamicDataCollection.GetAttribute<NaturalKeyAttribute>();
			return naturalKeyAttribute?.PropertyName;
		}

		public static int? GetMaxLength(this IDynamicData dynamicData)
		{
			return dynamicData.GetAttribute<MaxLengthAttribute>()?.MaxLength
				?? dynamicData.GetAttribute<CargoWise.ComponentModel.MaxLengthAttribute>()?.MaxLength;
		}

		public static bool GetIgnoreChanges(this IDynamicData dynamicData)
		{
			return dynamicData.GetAttribute<IgnoreChangesAttribute>() != null;
		}

		[SuppressWeaklyTypedCollectionMessage]
		public static IList GetList(this IDynamicData dynamicData)
		{
			if (dynamicData?.Parent?.Value is BusinessObject bizObj)
			{
				var listAttribute = dynamicData.GetAttribute<ListAttribute>();

				if (!string.IsNullOrWhiteSpace(listAttribute?.ListDataSourceMember))
				{
					const char zArchitectureLegacyPropertySeparator = '+';
					const char propertySeparator = '.';

					var macro = listAttribute.ListDataSourceMember.Replace(zArchitectureLegacyPropertySeparator, propertySeparator);

					return bizObj.GetFromMacro(macro) as IList;
				}
			}

			return null;
		}

		static object GetFromMacro(this object dataSource, string macro)
		{
			var expr = macro.With<StandardLibrary>().CreateExpression();

			return expr.Evaluate(dataSource);
		}

		public static bool GetDisableModifiable(this IDynamicData dynamicData, DisableModifiableMemberAttribute disableModifiableMemberAttribute)
		{
			if (dynamicData?.Parent?.Value is BusinessObject bizObj && disableModifiableMemberAttribute != null)
			{
				var propertyInfo = bizObj.GetType().GetProperty(disableModifiableMemberAttribute.Member);
				if (propertyInfo != null)
				{
					return (bool)propertyInfo.GetValue(bizObj);
				}
			}
			return false;
		}
	}
}
